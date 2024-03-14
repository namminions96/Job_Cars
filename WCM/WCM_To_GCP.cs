using Dapper;
using Job_By_SAP.Models;
using Job_By_SAP.WCM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using static Job_By_SAP.Models.SalesGCP_Retry;

namespace Job_By_SAP
{

    public class WCM_To_GCP
    {
        private readonly ILogger _logger;
        public WCM_To_GCP(ILogger logger)
        {
            _logger = logger;
        }
        IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
        public List<WcmGCPModels> OrderWcmToGCPAsync_Fix(string configWcm, List<string> reciept, List<TransTempGCP_WCM> results)
        {
            var timeout = 600;
            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                try
                {
                    DbsetWcm.Open();
                    if (reciept.Count > 0)
                    {
                        var TransDiscountGCP = DbsetWcm.Query<TransDiscountGCP>(WCM_Data.SUMD11_DISCOUNT_BLUE_New(), new { ReceiptNo = reciept }, commandTimeout: timeout).ToList();
                        var TransPaymentEntryGCP = DbsetWcm.Query<TransPaymentEntryGCP>(WCM_Data.SUMD11_PAYMENT_New(), new { ReceiptNo = reciept }, commandTimeout: timeout).ToList();
                        //var TransDiscountCouponEntryGCP = DbsetWcm.Query<TransDiscountCouponEntryGCP>(WCM_Data.SUMD11_Coupon_New(), new { ReceiptNo = reciept }, commandTimeout: timeout).ToList();
                        var concurrentBag = new ConcurrentBag<WcmGCPModels>();
                        try
                        {
                            Parallel.ForEach(results, order =>
                            {
                                List<TransLineGCP> Transline = new List<TransLineGCP>();
                                TransLineGCP transLine = new TransLineGCP();
                                transLine.Barcode = order.Barcode;
                                transLine.TranNo = int.Parse(order.TranNo);
                                transLine.Article = order.Article;
                                transLine.Uom = order.Uom;
                                transLine.Name = order.Name;
                                transLine.POSQuantity = order.POSQuantity;
                                transLine.Price = order.Price;
                                transLine.Amount = order.Amount;
                                transLine.Brand = order.Brand;
                                transLine.DiscountEntry = TransDiscountGCP.Where(p => p.ReceiptNo == order.ReceiptNo && p.ItemNo == order.Article).ToList();
                                Transline.Add(transLine);
                                WcmGCPModels orderExp = new WcmGCPModels();

                                orderExp.CalendarDay = order.CalendarDay;
                                orderExp.StoreCode = order.StoreCode;
                                orderExp.PosNo = order.PosNo;
                                orderExp.ReceiptNo = order.ReceiptNo;
                                orderExp.TranTime = order.TranTime;
                                orderExp.MemberCardNo = order.MemberCardNo;
                                orderExp.VinidCsn = order.VinidCsn;
                                orderExp.Header_ref_01 = order.Header_ref_01;
                                orderExp.Header_ref_02 = order.Header_ref_02;
                                orderExp.Header_ref_03 = order.Header_ref_03;
                                orderExp.Header_ref_04 = order.Header_ref_04;
                                orderExp.Header_ref_05 = order.Header_ref_05;
                               //-------------------------------------------//
                                List<OrderInfo> orderInfos = new List<OrderInfo>();
                                OrderInfo orderInfo = new OrderInfo();
                                orderInfo.key = "DrWinSource";
                                if (order.Source != "" && order.Source != null)
                                {
                                    orderInfo.value = order.Source;
                                }else
                                {
                                    orderInfo.value = "Retail";
                                }    
                             
                                orderInfos.Add(orderInfo);
                                //------------------------------------------//
                                orderExp.OrderInfo = orderInfos;
                                orderExp.IsRetry = order.IsRetry;
                                orderExp.TransLine = Transline;
                                orderExp.TransPaymentEntry = TransPaymentEntryGCP.Where(p => p.ReceiptNo == order.ReceiptNo).ToList();
                                orderExp.TransDiscountCouponEntry = new List<TransDiscountCouponEntryGCP>();
                                concurrentBag.Add(orderExp);
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                        finally
                        {
                            DbsetWcm.Close();
                        }
                        return concurrentBag.ToList();
                    }
                    else
                    {
                        _logger.Information("Không có Data");
                        return new List<WcmGCPModels>();
                    }
                }
                catch (Exception e)
                {
                    _logger.Information("Không có Data" + e.Message);
                    return new List<WcmGCPModels>();
                }
            }
        }
        public void OrderWcmToGCPAsync_Json(string configWcm, List<string> reciept)
        {
            var timeout = 10000;
            string delimitedString = string.Join(",", reciept);

            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                try
                {
                    DbsetWcm.Open();

                    if (reciept.Count > 0)
                    {
                        var parameters = new
                        {
                            OrderList = delimitedString
                        };
                        DbsetWcm.Query(WCM_Data.Insert_Data_RetryProc(), parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                        List<SP_Data_WCM_Insert> SP_Data_WCMs = new List<SP_Data_WCM_Insert>();
                        foreach (var item in reciept)
                        {
                            SP_Data_WCM_Insert sP_Data_WCM = new SP_Data_WCM_Insert();
                            var tempObject = new TempObject
                            {
                                TransDiscountEntry = DbsetWcm.Query<object>(WCM_Data.Transdiscount_Retry(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransPaymentEntry = DbsetWcm.Query<object>(WCM_Data.TransPayment_Retry(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransDiscountCouponEntry = DbsetWcm.Query<object>(WCM_Data.TransCp_Retry(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransInputData = DbsetWcm.Query<object>(WCM_Data.TransInput_Retry(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransHeader = DbsetWcm.Query<object>(WCM_Data.Transheader_Retry(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransLine = DbsetWcm.Query<object>(WCM_Data.TransLine_Retry(), new { ReceiptNo = item }, commandTimeout: timeout).ToList()
                            };
                            if (tempObject.TransHeader != null && tempObject.TransHeader.Count > 0)
                            {

                                var newDataObject = new
                                {
                                    Type = "SALE",
                                    Data = new
                                    {
                                        tempObject.TransLine,
                                        tempObject.TransHeader,
                                        tempObject.TransInputData,
                                        tempObject.TransDiscountCouponEntry,
                                        tempObject.TransDiscountEntry,
                                        tempObject.TransPaymentEntry,

                                    }
                                };
                                SP_Data_WCM_Insert SP_Data_WCMss = new SP_Data_WCM_Insert();
                                SP_Data_WCMss.ID = Guid.NewGuid();
                                SP_Data_WCMss.OrderNo = item;
                                SP_Data_WCMss.DataJson = JsonConvert.SerializeObject(newDataObject);
                                SP_Data_WCMss.ChgDate = DateTime.Now;
                                SP_Data_WCMss.OrderDate = DateTime.Now;
                                SP_Data_WCMss.CrtDate = DateTime.Now;
                                SP_Data_WCMss.StoreNo = item.Substring(0, 4);
                                SP_Data_WCMss.PosNo = item.Substring(0, 6);
                                SP_Data_WCMss.Type = "Tran";
                                SP_Data_WCMss.BatchFile = $"Retry_{item}";
                                SP_Data_WCMss.FileName = $"File_{item}";
                                SP_Data_WCMss.IsRead = false;
                                SP_Data_WCMss.MemberCardNo = "";
                                SP_Data_WCMss.VATAmount = 0;
                                SP_Data_WCMss.LineAmountIncVAT = 0;
                                SP_Data_WCMss.DiscountAmount = 0;
                                SP_Data_WCMs.Add(SP_Data_WCMss);
                            }
                        };
                        ReadDataRawJson readDataRawJson = new ReadDataRawJson(_logger);
                        if (SP_Data_WCMs.Count > 0)
                        {
                            readDataRawJson.InsertStatusWCM(SP_Data_WCMs, configWcm);
                        }
                        else
                        {
                            _logger.Information("Không có Data Insert");
                        }

                        //readDataRawJson.UpdateStatusWCM_Retry_data(SP_Data_WCMs, configWcm);

                    }
                    else
                    {
                        _logger.Information("Không có Data");

                    }
                }
                catch (Exception e)
                {
                    _logger.Information("Không có Data" + e.Message);

                }
                DbsetWcm.Close();
            }
        }
        public List<WcmGCPModels> OrderWcmToGCPAsync_Json(string configWcm, List<SP_Data_WCM> reciept, string Namefuntion)
        {

            ReadDataRawJson readDataRawJson = new ReadDataRawJson(_logger);
            var timeout = 600;
            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                try
                {
                    DbsetWcm.Open();
                    List<SP_Data_WCM> SP_Data_WCMs = new List<SP_Data_WCM>();
                    List<Temp_Zalo_Survey> temp_Zalo_Survey = new List<Temp_Zalo_Survey>();
                    var concurrentBag = new ConcurrentBag<WcmGCPModels>();
                    foreach (var result in reciept)
                    {
                        try
                        {
                            JObject jsonObject = JObject.Parse(result.DataJson);
                            //TransInputDataGCP//
                            JArray TransInputData = (JArray)jsonObject["Data"]["TransInputData"];
                            List<TransInputDataGCP> TransInputDataResult = readDataRawJson.TransInputDataGCP(TransInputData);

                            //TransDiscountCouponEntry//
                            JArray TransDiscountCouponEntry = (JArray)jsonObject["Data"]["TransDiscountCouponEntry"];
                            List<TransDiscountCouponEntryGCP> CouponEntryResult = readDataRawJson.TransDiscountCouponEntryGCP(TransDiscountCouponEntry);

                            //payment//
                            JArray TransPaymentEntry = (JArray)jsonObject["Data"]["TransPaymentEntry"];
                            List<TransPaymentEntryGCP> PaymentEntryResult = readDataRawJson.TransPaymentEntryGCP(TransPaymentEntry);
                            //TransDiscountEntry//
                            JArray TransDiscountEntry = (JArray)jsonObject["Data"]["TransDiscountEntry"];
                            List<TransDiscountGCP> DiscountEntryResult = readDataRawJson.TransDiscountGCP(TransDiscountEntry);
                            //TransLine///
                            JArray TransLine = (JArray)jsonObject["Data"]["TransLine"];
                            List<TransLineGCP> TransLineResult = new List<TransLineGCP>();
                            DateTime ScanTime = new DateTime();
                            decimal VATAmount = 0;
                            decimal LineAmountIncVAT = 0;
                            decimal DiscountAmount = 0;
                            bool IsWPH = false;
                            foreach (JObject Item in TransLine)
                            {
                                if ((int)Item["LineType"] == 0)
                                {
                                    VATAmount += (decimal)Item["VATAmount"];
                                    LineAmountIncVAT += (decimal)Item["LineAmountIncVAT"];
                                    DiscountAmount += (decimal)Item["DiscountAmount"];

                                    TransLineGCP TransLines = new TransLineGCP();
                                    int Linetype = (int)Item["LineType"];
                                    TransLines.TranNo = (int)Item["LineNo"];
                                    TransLines.Barcode = (string)Item["Barcode"];
                                    TransLines.Article = (string)Item["ItemNo"];
                                    TransLines.Uom = (string)Item["UnitOfMeasure"];
                                    TransLines.Name = (string)Item["Description"];
                                    TransLines.POSQuantity = (decimal)Item["Quantity"];
                                    if (Item.TryGetValue("UnitPrice", out var unitPriceValue) && unitPriceValue.Type != JTokenType.Null)
                                    {
                                        TransLines.Price = (decimal)unitPriceValue;
                                    }
                                    if (Item.TryGetValue("AmountBeforeDiscount", out var lineAmountValue) && lineAmountValue.Type != JTokenType.Null)
                                    {
                                        TransLines.Amount = (decimal)lineAmountValue;
                                    }
                                    if (Item.TryGetValue("VATAmount", out var VatAmountValue) && VatAmountValue.Type != JTokenType.Null)
                                    {
                                        TransLines.VATAmount = (decimal)VatAmountValue;
                                    }
                                    TransLines.Brand = (string)Item["DivisionCode"];
                                    TransLines.DiscountEntry = DiscountEntryResult.Where(p => p.ItemNo == TransLines.Article && p.TranNo == TransLines.TranNo).ToList();

                                    if (Item.TryGetValue("ScanTime", out var scanTimeValue) && scanTimeValue.Type != JTokenType.Null)
                                    {
                                        if (DateTime.TryParse(scanTimeValue.ToString(), out DateTime scanTime))
                                        {
                                            ScanTime = scanTime;
                                        }
                                    }
                                    TransLineResult.Add(TransLines);
                                }
                                if ((string)Item["DivisionCode"] == "WPH")
                                {
                                    IsWPH = true;
                                }
                            }
                            //TransHeader//
                            var SOURCEBILL = TransInputDataResult.FirstOrDefault(P => P.DataType == "SOURCEBILL");
                            var CUSTYPE = TransInputDataResult.FirstOrDefault(P => P.DataType == "CUSTYPE");

                            List<WcmGCPModels> TransHeaderss = new List<WcmGCPModels>();
                            JArray TransHeader = (JArray)jsonObject["Data"]["TransHeader"];
                            foreach (JObject headerItem in TransHeader)
                            {
                                WcmGCPModels TransHeaders = new WcmGCPModels();
                                TransHeaders.CalendarDay = (string)headerItem["OrderDate"];
                                TransHeaders.StoreCode = (string)headerItem["StoreNo"];
                                string posNo = (string)headerItem["POSTerminalNo"];
                                TransHeaders.PosNo = posNo.Substring(posNo.Length - 2, 2);
                                TransHeaders.ReceiptNo = (string)headerItem["OrderNo"];
                                TransHeaders.TranTime = ScanTime.ToString("HHmmssfff");
                                TransHeaders.MemberCardNo = (string)headerItem["MemberCardNo"];
                                TransHeaders.VinidCsn = (string)headerItem["VinidCsn"];
                                TransHeaders.Header_ref_01 = (string)headerItem["ReturnedOrderNo"];// mã đơn trả hàng
                                TransHeaders.Header_ref_02 = SOURCEBILL?.DataValue;               /// Souce Bill
                                TransHeaders.Header_ref_03 = (string)headerItem["RefKey1"];      //Đơn Hàng đối tác
                                TransHeaders.Header_ref_04 = CUSTYPE?.DataValue;                /// mã KH
                                string zoneNo = (string)headerItem["ZoneNo"];
                                if (zoneNo == "TCBTOPUP" || zoneNo == "TCBWITHDRAW")            //Nocap//
                                {
                                    TransHeaders.Header_ref_05 = "NO_CAP";
                                }
                                else
                                {
                                    TransHeaders.Header_ref_05 = zoneNo;
                                }
                                if (Namefuntion == "GCP_WCM_Retry")
                                {
                                    TransHeaders.IsRetry = true;
                                }
                                else
                                {
                                    TransHeaders.IsRetry = false;
                                }
                                //List<OrderInfo> orderInfos = new List<OrderInfo>();
                                //OrderInfo orderInfo = new OrderInfo();
                                //orderInfo.key = "NoteDHCX";
                                //orderInfo.value = (string)headerItem["RefKey1"];
                                //orderInfos.Add(orderInfo);
                                TransHeaders.OrderInfo = new List<OrderInfo>();
                                TransHeaders.TransLine = TransLineResult.ToList();
                                TransHeaders.TransPaymentEntry = PaymentEntryResult.ToList();
                                TransHeaders.TransDiscountCouponEntry = CouponEntryResult.Where
                                (coupon => TransLineResult.Any(transLine => transLine.TranNo == coupon.ParentLineId)).ToList();
                                concurrentBag.Add(TransHeaders);
                                SP_Data_WCM SP_Data_WCMss = new SP_Data_WCM();
                                SP_Data_WCMss.ID = result.ID;
                                SP_Data_WCMss.OrderNo = TransHeaders.ReceiptNo;
                                DateTime parsedDate;
                                if (DateTime.TryParseExact(TransHeaders.CalendarDay, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                                {
                                    SP_Data_WCMss.OrderDate = parsedDate;
                                }
                                SP_Data_WCMss.ChgDate = DateTime.Now;
                                SP_Data_WCMss.IsRead = true;
                                SP_Data_WCMss.DataJson = result.DataJson;
                                SP_Data_WCMss.MemberCardNo = TransHeaders.MemberCardNo;
                                SP_Data_WCMss.VATAmount = VATAmount;
                                SP_Data_WCMss.LineAmountIncVAT = LineAmountIncVAT;
                                SP_Data_WCMss.DiscountAmount = DiscountAmount;
                                SP_Data_WCMs.Add(SP_Data_WCMss);

                                Temp_Zalo_Survey temp_Zalo_Survey1 = new Temp_Zalo_Survey();
                                if (IsWPH == true && (string)headerItem["HouseNo"] =="CAP")
                                {
                                    temp_Zalo_Survey1.RECEIPT_NO = TransHeaders.ReceiptNo;
                                    temp_Zalo_Survey1.PhoneNo = TransHeaders.MemberCardNo;
                                    temp_Zalo_Survey1.OrderDate = TransHeaders.CalendarDay;
                                    temp_Zalo_Survey1.UpdateFlg = false;
                                    temp_Zalo_Survey1.CrtDate = DateTime.Now;
                                    temp_Zalo_Survey.Add(temp_Zalo_Survey1);
                                }
                            }
                        }
                        catch (JsonReaderException e)
                        {
                            SP_Data_WCM SP_Data_WCMss = new SP_Data_WCM();
                            SP_Data_WCMss.ID = result.ID;
                            SP_Data_WCMss.ChgDate = DateTime.Now;
                            SP_Data_WCMss.IsRead = true;
                            SP_Data_WCMss.DataJson = result.DataJson;
                            SP_Data_WCMss.OrderNo = "Error_JsonFomat";
                            SP_Data_WCMss.MemberCardNo = "";
                            SP_Data_WCMss.VATAmount = 0;
                            SP_Data_WCMss.LineAmountIncVAT = 0;
                            SP_Data_WCMss.DiscountAmount = 0;
                            SP_Data_WCMs.Add(SP_Data_WCMss);
                            _logger.Information($"Lỗi:  {e.Message}");
                        }
                    }

                    if (Namefuntion == "GCP_WCM_Retry")
                    {
                        //_logger.Information(Namefuntion);
                        readDataRawJson.UpdateStatusWCM_Retry_Json(SP_Data_WCMs, configWcm);
                        readDataRawJson.Insert_WPH_Zalo_Sv(temp_Zalo_Survey, configWcm);
                    }
                    else
                    {
                        //_logger.Information(Namefuntion);
                        readDataRawJson.UpdateStatusWCM(SP_Data_WCMs, configWcm);
                        readDataRawJson.Insert_WPH_Zalo_Sv(temp_Zalo_Survey, configWcm);
                        //ServiceMongo serviceMongo = new ServiceMongo();
                        //var dataService = new MongoService<SP_Data_WCM>(serviceMongo.SeviceData(), "Sale_GCP", "Transactions");
                        //dataService.InsertData(SP_Data_WCMs);
                    }

                    return concurrentBag.ToList();
                }
                catch (Exception e)
                {
                    _logger.Information("Không có Data" + e.Message);
                    return new List<WcmGCPModels>();
                }
            }
        }
        public List<TransVoidGCP> OrderWcmToGCPVoidAsync_Json(string configWcm, List<SP_Data_WCM> reciept)
        {
            ReadDataRawJson readDataRawJson = new ReadDataRawJson(_logger);
            var timeout = 600;
            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                ReadTranVoid_GCP readTranVoid_GCP = new ReadTranVoid_GCP();
                try
                {
                    DbsetWcm.Open();
                    List<SP_Data_WCM> SP_Data_WCMs = new List<SP_Data_WCM>();
                    var concurrentBag = new ConcurrentBag<TransVoidGCP>();
                    foreach (var result in reciept)
                    {
                        JObject jsonObject = JObject.Parse(result.DataJson);
                        //TransVoidLine//
                        JArray TransVoidLine = (JArray)jsonObject["Data"]["TransVoidLine"];
                        //List<TransVoidLine> TransVoidLineResult = readTranVoid_GCP.TransVoidLine(TransVoidLine);
                        List<TransVoidLine> TransInputDatasss = new List<TransVoidLine>();
                        foreach (JObject TransInputDatass in TransVoidLine)
                        {
                            SP_Data_WCM SP_Data_WCMss = new SP_Data_WCM();
                            if ((int)TransInputDatass["LineType"] == 0)
                            {
                                TransVoidLine TransInputDatas = new TransVoidLine();
                                TransInputDatas.ScanTime = (DateTime)TransInputDatass["ScanTime"];
                                TransInputDatas.OrderNo = (string)TransInputDatass["DocumentNo"];
                                TransInputDatas.LineType = (int)TransInputDatass["LineType"];
                                TransInputDatas.LocationCode = (string)TransInputDatass["LocationCode"];
                                TransInputDatas.ItemNo = (string)TransInputDatass["ItemNo"];
                                TransInputDatas.Description = (string)TransInputDatass["Description"];
                                TransInputDatas.UnitOfMeasure = (string)TransInputDatass["UnitOfMeasure"];
                                TransInputDatas.Quantity = (decimal)TransInputDatass["Quantity"];
                                TransInputDatas.UnitPrice = (decimal)TransInputDatass["UnitPrice"];
                                TransInputDatas.DiscountAmount = (decimal)TransInputDatass["DiscountAmount"];
                                TransInputDatas.LineAmountIncVAT = (decimal)TransInputDatass["LineAmountIncVAT"];
                                TransInputDatas.StaffID = (string)TransInputDatass["StaffID"];
                                TransInputDatas.VATCode = (string)TransInputDatass["VATCode"];
                                TransInputDatas.DeliveringMethod = (int)TransInputDatass["DeliveringMethod"];
                                TransInputDatas.Barcode = (string)TransInputDatass["Barcode"];
                                TransInputDatas.DivisionCode = (string)TransInputDatass["DivisionCode"];
                                TransInputDatas.SerialNo = (string)TransInputDatass["SerialNo"];
                                TransInputDatas.OrigOrderNo = (string)TransInputDatass["OrigOrderNo"];
                                TransInputDatas.LotNo = (string)TransInputDatass["LotNo"];
                                TransInputDatas.ArticleType = (string)TransInputDatass["ArticleType"];
                                TransInputDatas.LastUpdated = DateTime.Now;
                                TransInputDatasss.Add(TransInputDatas);
                                //---------------------------------------------------------------------------//
                                SP_Data_WCMss.ID = result.ID;
                                SP_Data_WCMss.OrderNo = TransInputDatas.OrderNo;
                                string OrderDate = TransInputDatas.ScanTime.ToString();
                                SP_Data_WCMss.ChgDate = DateTime.Now;
                                SP_Data_WCMss.IsRead = true;
                                SP_Data_WCMss.DataJson = result.DataJson;
                                SP_Data_WCMs.Add(SP_Data_WCMss);
                            }
                            else
                            {
                                SP_Data_WCMss.ID = result.ID;
                                SP_Data_WCMss.OrderNo = (string)TransInputDatass["DocumentNo"];
                                string OrderDate = (string)TransInputDatass["ExpireDate"];
                                SP_Data_WCMss.ChgDate = DateTime.Now;
                                SP_Data_WCMss.IsRead = true;
                                SP_Data_WCMss.DataJson = result.DataJson;
                                SP_Data_WCMs.Add(SP_Data_WCMss);
                            }
                        }
                        //TransVoidHeader//
                        JArray TransVoidHeader = (JArray)jsonObject["Data"]["TransVoidHeader"];
                        List<TransVoidHeader> TransVoidHeaderResult = readTranVoid_GCP.TransVoidHeader(TransVoidHeader);
                        TransVoidGCP transVoidGCP = new TransVoidGCP();
                        transVoidGCP.TransVoidLine = TransInputDatasss;
                        transVoidGCP.TransVoidHeader = TransVoidHeaderResult;
                        concurrentBag.Add(transVoidGCP);
                    }
                    readDataRawJson.UpdateStatusVoidWCM(SP_Data_WCMs, configWcm);
                    return concurrentBag.ToList();
                }
                catch (Exception e)
                {
                    _logger.Information("Không có Data" + e.Message);
                    return new List<TransVoidGCP>();
                }
            }
        }
        public void SaveFile(string json, string FilePath)
        {
            string filePathSave = FilePath;
            File.WriteAllText(filePathSave, json);
        }
    }

}

