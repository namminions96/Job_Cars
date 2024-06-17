using Dapper;
using Job_By_SAP.Models;
using Job_By_SAP.PLH;
using Job_By_SAP.WCM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Read_xml;
using Serilog;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
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
                        //var TransLines = DbsetWcm.Query<TransPaymentEntryGCP>(WCM_Data.SUMD11_PAYMENT_New(), new { ReceiptNo = reciept }, commandTimeout: timeout).ToList();
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
                                transLine.VATAmount = order.VATAmount;
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
                                }
                                else
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
            try
            { ReadFile readFile = new ReadFile(_logger);
                ReadDataRawJson readDataRawJson = new ReadDataRawJson(_logger);
                var timeout = 600;
                var MD_Connect = configuration["DbStaging_WCM"];
                var ConfigSend = configuration["ConfigSend"];
                using (SqlConnection DbsetWcm_MD = new SqlConnection(MD_Connect))
                {
                    try
                    {
                        DbsetWcm_MD.Open();
                        var query = "select No,BusinessAreaNo,[StyleProfile] from CentralMD..Store";
                        var Store = DbsetWcm_MD.Query<Store>(query).ToList();
                        List<ReportSaleDetail> reportSaleDetailsSave = new List<ReportSaleDetail>();
                        List<ReportSaleDetail> reportSaleDetails = new List<ReportSaleDetail>();
                        List<SP_Data_WCM> SP_Data_WCMs = new List<SP_Data_WCM>();
                        //List<Temp_Zalo_Survey> temp_Zalo_Survey = new List<Temp_Zalo_Survey>();
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
                                var SOURCEBILL = TransInputDataResult.FirstOrDefault(P => P.DataType == "SOURCEBILL");
                                var HANDLINGSTAFF = TransInputDataResult.FirstOrDefault(P => P.DataType == "HANDLINGSTAFF");
                                var CUSTYPE = TransInputDataResult.FirstOrDefault(P => P.DataType == "CUSTYPE");
                                foreach (JObject Item in TransLine)
                                {
                                    ReportSaleDetail reportSaleDetail = new ReportSaleDetail();
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
                                        TransLines.SerialNo = (string)Item["SerialNo"];
                                        TransLines.Brand = (string)Item["DivisionCode"];
                                        TransLines.DiscountEntry = DiscountEntryResult.Where(p => p.ItemNo == TransLines.Article && p.TranNo == TransLines.TranNo).ToList();

                                        if (Item.TryGetValue("ScanTime", out var scanTimeValue) && scanTimeValue.Type != JTokenType.Null)
                                        {
                                            if (DateTime.TryParse(scanTimeValue.ToString(), out DateTime scanTime))
                                            {
                                                ScanTime = scanTime;
                                            }
                                        }

                                        var promotionID = DiscountEntryResult.Where(p => p.TranNo == TransLines.TranNo)
                                                                                       .Select(p => p.OfferNo)
                                                                                       .ToList();
                                        if (promotionID != null && promotionID.Any())
                                        {
                                            reportSaleDetail.PromotionID = string.Join(",", promotionID);
                                        }
                                        else
                                        {
                                            reportSaleDetail.PromotionID = "";
                                        }
                                        reportSaleDetail.OrderNo = (string)Item["DocumentNo"];
                                        reportSaleDetail.LineNo = (int)Item["LineNo"];
                                        reportSaleDetail.Barcode = (string)Item["Barcode"];
                                        reportSaleDetail.ItemNo = (string)Item["ItemNo"];
                                        reportSaleDetail.Description = (string)Item["Description"];
                                        reportSaleDetail.UnitOfMeasure = (string)Item["UnitOfMeasure"];
                                        reportSaleDetail.Quantity = (decimal)Item["Quantity"];
                                        if (Item.TryGetValue("UnitPrice", out var unitPriceValues) && unitPriceValues.Type != JTokenType.Null)
                                        {
                                            reportSaleDetail.UnitPrice = (float)unitPriceValues;
                                        }
                                        reportSaleDetail.DiscountAmount = (decimal)Item["DiscountAmount"];
                                        reportSaleDetail.VATCode = (string)Item["VATCode"];
                                        reportSaleDetail.LineAmountIncVAT = (decimal)Item["LineAmountIncVAT"];
                                        reportSaleDetail.VATAmount = TransLines.VATAmount;
                                        reportSaleDetail.DivisionCode = TransLines.Brand;
                                        reportSaleDetail.RefKey1 = (string)Item["RefKey1"];
                                        reportSaleDetail.BlockedMemberPoint = (byte)Item["BlockedMemberPoint"];
                                        reportSaleDetail.MemberPointsEarn = (float)Item["MemberPointsEarn"];
                                        reportSaleDetail.MemberPointsRedeem = (float)Item["MemberPointsRedeem"];
                                        if (!string.IsNullOrEmpty(SOURCEBILL?.DataValue))
                                        {
                                            reportSaleDetail.SOURCEBILL = SOURCEBILL?.DataValue;
                                        }
                                        else
                                        {
                                            reportSaleDetail.SOURCEBILL = "Winmart-Local";
                                        }

                                        reportSaleDetail.HANDLINGSTAFF = HANDLINGSTAFF?.DataValue;
                                        reportSaleDetail.AmountCalPoint = (float)Item["AmountCalPoint"];
                                        reportSaleDetail.DeliveringMethod = (string)Item["DeliveringMethod"];
                                        reportSaleDetail.SerialNo = (string)Item["SerialNo"];
                                        reportSaleDetail.VATPercent = Item["VATPercent"] != null ? (byte)Item["VATPercent"] : (byte)0;
                                        if (CouponEntryResult.Count == 0 || CouponEntryResult == null)
                                        {
                                            reportSaleDetail.CouponCode = "";
                                        }
                                        else
                                        {
                                            var couponEntryResults = CouponEntryResult.Where(x => x.ParentLineId == reportSaleDetail.LineNo)
                                                                                       .Select(p => p.Barcode)
                                                                                       .ToList();
                                            if (couponEntryResults != null && couponEntryResults.Any())
                                            {
                                                reportSaleDetail.CouponCode = string.Join(",", couponEntryResults);
                                            }
                                            else
                                            {
                                                reportSaleDetail.CouponCode = "";
                                            }
                                        }
                                        reportSaleDetails.Add(reportSaleDetail);
                                        TransLineResult.Add(TransLines);
                                    }
                                    if ((string)Item["DivisionCode"] == "WPH")
                                    {
                                        IsWPH = true;
                                    }
                                }
                                //TransHeader//


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

                                    List<ReportSaleDetail> reportSaleDetail = reportSaleDetails.Where(item => item.OrderNo == TransHeaders.ReceiptNo).ToList(); // Adjust the condition as needed
                                    foreach (var item in reportSaleDetail)
                                    {
                                        ReportSaleDetail reportSaleDetailss = new ReportSaleDetail();
                                        reportSaleDetailss.ReturnedOrderNo = (string)headerItem["ReturnedOrderNo"];
                                        reportSaleDetailss.StoreNo = TransHeaders.StoreCode;
                                        reportSaleDetailss.POSTerminalNo = posNo;
                                        reportSaleDetailss.CashierID = (string)headerItem["CashierID"];
                                        if (reportSaleDetailss.ReturnedOrderNo.IsNullOrEmpty())
                                        {
                                            reportSaleDetailss.SalesIsReturn = false;

                                        }
                                        else
                                        {
                                            reportSaleDetailss.SalesIsReturn = true;
                                        }
                                        reportSaleDetailss.HouseNo = (string)headerItem["HouseNo"];
                                        reportSaleDetailss.CityNo = (string)headerItem["CityNo"];
                                        reportSaleDetailss.MemberCardNo = TransHeaders.MemberCardNo;
                                        reportSaleDetailss.UserID = (string)headerItem["UserID"];
                                        reportSaleDetailss.OrderDate = (DateTime)headerItem["OrderDate"];
                                        reportSaleDetailss.OrderTime = (DateTime)headerItem["OrderTime"];
                                        reportSaleDetailss.DeliveryComment = (string)headerItem["DeliveryComment"];
                                        reportSaleDetailss.StyleProfile = (string)headerItem["StyleProfile"];
                                        reportSaleDetailss.CustomerName = (string)headerItem["CustomerName"];
                                        reportSaleDetailss.AmountDiscountAtPOS = (string)headerItem["AmountDiscountAtPOS"];
                                        reportSaleDetailss.IsTenancy = headerItem["IsTenancy"] != null ? (byte)headerItem["IsTenancy"] : (byte)0;
                                        reportSaleDetailss.ReturnVoucherNo = (string)headerItem["ReturnVoucherNo"];
                                        reportSaleDetailss.ReturnVoucherExpire = (string)headerItem["ReturnVoucherExpire"];
                                        reportSaleDetailss.TanencyNo = (string)headerItem["TanencyNo"];
                                        reportSaleDetailss.CouponCode = item.CouponCode;
                                        reportSaleDetailss.PromotionID = item.PromotionID;
                                        reportSaleDetailss.OrderNo = item.OrderNo;
                                        reportSaleDetailss.SerialNo = item.SerialNo;
                                        reportSaleDetailss.LineNo = item.LineNo;
                                        reportSaleDetailss.Barcode = item.Barcode;
                                        reportSaleDetailss.ItemNo = item.ItemNo;
                                        reportSaleDetailss.Description = item.Description;
                                        reportSaleDetailss.UnitOfMeasure = item.UnitOfMeasure;
                                        reportSaleDetailss.Quantity = item.Quantity;
                                        reportSaleDetailss.UnitPrice = item.UnitPrice;
                                        reportSaleDetailss.VATPercent = item.VATPercent;
                                        reportSaleDetailss.DiscountAmount = item.DiscountAmount;
                                        reportSaleDetailss.VATCode = item.VATCode;
                                        reportSaleDetailss.LineAmountIncVAT = item.LineAmountIncVAT;
                                        reportSaleDetailss.VATAmount = item.VATAmount;
                                        reportSaleDetailss.DivisionCode = item.DivisionCode;
                                        reportSaleDetailss.RefKey1 = item.RefKey1;
                                        reportSaleDetailss.BlockedMemberPoint = item.BlockedMemberPoint;
                                        reportSaleDetailss.MemberPointsEarn = item.MemberPointsEarn;
                                        reportSaleDetailss.MemberPointsRedeem = item.MemberPointsRedeem;
                                        reportSaleDetailss.SOURCEBILL = item.SOURCEBILL;
                                        reportSaleDetailss.HANDLINGSTAFF = item.HANDLINGSTAFF;
                                        reportSaleDetailss.AmountCalPoint = item.AmountCalPoint;
                                        reportSaleDetailss.DeliveringMethod = item.DeliveringMethod;
                                        var BusinessAreaNo = Store.SingleOrDefault(x => x.No == reportSaleDetailss.StoreNo);
                                        if (BusinessAreaNo != null)
                                        {
                                            reportSaleDetailss.BusinessAreaNo = BusinessAreaNo.BusinessAreaNo;
                                            reportSaleDetailss.StyleProfile = BusinessAreaNo.StyleProfile;
                                        }
                                        else
                                        {
                                            reportSaleDetailss.BusinessAreaNo = "";
                                            reportSaleDetailss.StyleProfile = "";
                                        }

                                        if (reportSaleDetailss.DeliveryComment.StartsWith("HomeDelivery"))
                                        {
                                            reportSaleDetailss.SalesType = "2";//giao tai nha
                                        }
                                        else if (reportSaleDetailss.TanencyNo == "SNG")
                                        {
                                            reportSaleDetailss.SalesType = "SNG";//SNG
                                        }
                                        else if (reportSaleDetailss.TanencyNo == "WCM")
                                        {
                                            reportSaleDetailss.SalesType = "WCM";//WCM
                                        }
                                        else if (reportSaleDetailss.VoucherDiscountNo == "KIOS")
                                        {
                                            reportSaleDetailss.SalesType = "KIOS";//KIOS
                                        }
                                        else if (reportSaleDetailss.TanencyNo == "" && reportSaleDetailss.DeliveringMethod != "0")
                                        {
                                            reportSaleDetailss.SalesType = reportSaleDetailss.DeliveringMethod;//giao tai nha
                                        }
                                        else if (reportSaleDetailss.TanencyNo == "" && reportSaleDetailss.DeliveringMethod == "0")
                                        {
                                            reportSaleDetailss.SalesType = "";
                                        }
                                        else
                                        {
                                            reportSaleDetailss.SalesType = reportSaleDetailss.TanencyNo;
                                        }
                                        reportSaleDetailsSave.Add(reportSaleDetailss);
                                    }


                                    //Temp_Zalo_Survey temp_Zalo_Survey1 = new Temp_Zalo_Survey();
                                    //if (IsWPH == true && (string)headerItem["HouseNo"] =="CAP")
                                    //{
                                    //    temp_Zalo_Survey1.RECEIPT_NO = TransHeaders.ReceiptNo;
                                    //    temp_Zalo_Survey1.PhoneNo = TransHeaders.MemberCardNo;
                                    //    temp_Zalo_Survey1.OrderDate = TransHeaders.CalendarDay;
                                    //    temp_Zalo_Survey1.UpdateFlg = false;
                                    //    temp_Zalo_Survey1.CrtDate = DateTime.Now;
                                    //    temp_Zalo_Survey.Add(temp_Zalo_Survey1);
                                    //}
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

                            if (ConfigSend == "0")
                            {
                                readDataRawJson.Insert_RP_Detail(reportSaleDetailsSave, configWcm);
                            }
                            else if (ConfigSend == "1")
                            {
                                readDataRawJson.Insert_RP_Kafka(reportSaleDetailsSave, configWcm);
                            }
                            else
                            {
                                _logger.Information("Chưa khai báo ConfigSend Lưu data");
                            }
                            readDataRawJson.UpdateStatusWCM_Retry_Json(SP_Data_WCMs, configWcm);
                            //readDataRawJson.Insert_WPH_Zalo_Sv(temp_Zalo_Survey, configWcm);
                        }
                        else
                        {
                            //_logger.Information(Namefuntion);
                            if (ConfigSend == "0")
                            {
                                readDataRawJson.Insert_RP_Detail(reportSaleDetailsSave, configWcm);
                            }
                            else if(ConfigSend == "1")
                            {
                                readDataRawJson.Insert_RP_Kafka(reportSaleDetailsSave, configWcm);
                            }
                            else
                            {
                                _logger.Information("Chưa khai báo ConfigSend Lưu data");
                            }
                            readDataRawJson.UpdateStatusWCM(SP_Data_WCMs, configWcm);
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
            catch (Exception e)
            {
                _logger.Information("Lỗi" + e.Message);
                return new List<WcmGCPModels>();
            }
        }//WCM GCP
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
        }//WCM GCP Void
        public void SaveFile(string json, string FilePath)
        {
            string filePathSave = FilePath;
            File.WriteAllText(filePathSave, json);
        }
        public List<OrderExpToGCP> OrderPLHToGCPAsync_Json(string configWcm, List<SP_Data_WCM> reciept, string Namefuntion)
        {
            string config = configuration["PLH_MD"];

            using (SqlConnection DbsetPLH = new SqlConnection(config))
            {
                List<CpnVchBOMHeader> couponDescription = DbsetPLH.Query<CpnVchBOMHeader>(WCM_Data.CpnVchBOMHeader_PLH()).ToList();
                DataJson_PLH DataJson_PLHs = new DataJson_PLH(_logger);
                var timeout = 600;
                using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                {
                    try
                    {
                        DbsetWcm.Open();
                        List<SP_Data_WCM> SP_Data_WCMs = new List<SP_Data_WCM>();
                        var concurrentBag = new ConcurrentBag<OrderExpToGCP>();
                        foreach (var result in reciept)
                        {
                            try
                            {
                                JObject jsonObject = JObject.Parse(result.DataJson);
                                //TransInputDataGCP//
                                JArray TransPointEntry_PLH_BLUEPOSs = (JArray)jsonObject["Data"]["TransPointLine"];
                                List<TransPointEntry_PLH_BLUEPOS> TranspointDataResult = DataJson_PLHs.TransPoinGCP(TransPointEntry_PLH_BLUEPOSs);
                                //TransDiscountCouponEntry//
                                JArray TransDiscountCouponEntry = (JArray)jsonObject["Data"]["TransDiscountCouponEntry"];
                                List<TransDiscountCouponEntry_PLH_BLUEPOS> CouponEntryResult = DataJson_PLHs.TransDiscountCouponEntryGCP(TransDiscountCouponEntry, couponDescription);
                                //payment//
                                JArray TransPaymentEntry = (JArray)jsonObject["Data"]["TransPaymentEntry"];
                                List<TransPaymentEntry_PLH_BLUEPOS> PaymentEntryResult = DataJson_PLHs.TransPaymentEntryGCP(TransPaymentEntry);
                                //TransDiscountEntry//
                                JArray TransDiscountEntry = (JArray)jsonObject["Data"]["TransDiscountEntry"];
                                List<TransDiscountEntry_PLH_BLUEPOS> DiscountEntryResult = DataJson_PLHs.TransDiscountGCP(TransDiscountEntry);
                                //TransLine///
                                JArray TransLine = (JArray)jsonObject["Data"]["TransLine"];
                                List<TransLine_PLH_BLUEPOS> TransLineResult = new List<TransLine_PLH_BLUEPOS>();
                                DateTime ScanTime = new DateTime();
                                decimal VATAmount = 0;
                                decimal LineAmountIncVAT = 0;
                                decimal DiscountAmount = 0;
                                foreach (JObject Item in TransLine)
                                {
                                    if ((int)Item["LineType"] == 0)
                                    {
                                        VATAmount += (decimal)Item["VATAmount"];
                                        LineAmountIncVAT += (decimal)Item["LineAmountIncVAT"];
                                        DiscountAmount += (decimal)Item["DiscountAmount"];

                                        TransLine_PLH_BLUEPOS TransLines = new TransLine_PLH_BLUEPOS();
                                        TransLines.OrderNo = (string)Item["DocumentNo"];
                                        TransLines.LineId = (int)Item["LineNo"];
                                        TransLines.ParentLineId = (int)Item["LineType"];
                                        TransLines.ItemNo = (string)Item["ItemNo"];
                                        TransLines.ItemName = (string)Item["Description"];
                                        TransLines.Uom = (string)Item["UnitOfMeasure"];
                                        if (Item.TryGetValue("UnitPrice", out var unitPriceValue) && unitPriceValue.Type != JTokenType.Null)
                                        {
                                            TransLines.OldPrice = (decimal)unitPriceValue;
                                        }
                                        if (Item.TryGetValue("UnitPrice", out var unitPriceValuefix) && unitPriceValuefix.Type != JTokenType.Null)
                                        {
                                            TransLines.UnitPrice = (decimal)unitPriceValuefix;
                                        }
                                        TransLines.Qty = (decimal)Item["Quantity"];

                                        if (Item.TryGetValue("DiscountAmount", out var lineAmountValue) && lineAmountValue.Type != JTokenType.Null)
                                        {
                                            TransLines.DiscountAmount = (decimal)lineAmountValue;
                                        }
                                        if (Item.TryGetValue("LineAmountIncVAT", out var VatAmountValue) && VatAmountValue.Type != JTokenType.Null)
                                        {
                                            TransLines.LineAmount = (decimal)VatAmountValue;
                                        }
                                        TransLines.VatGroup = (string)Item["VATCode"];
                                        TransLines.VatPercent = (int)Item["VATPercent"];
                                        TransLines.Note = (string)Item["Note"];
                                        TransLines.CupType = "";
                                        TransLines.Size = "";

                                        if (Item.TryGetValue("IsTopping", out var IsToppingss) && IsToppingss.Type != JTokenType.Null)
                                        {
                                            TransLines.IsTopping = (bool)IsToppingss;
                                        }
                                        if (Item.TryGetValue("IsCombo", out var IsComboss) && IsComboss.Type != JTokenType.Null)
                                        {
                                            TransLines.IsCombo = (bool)IsComboss;
                                        }
                                        TransLines.ComboId = 0;
                                        TransLines.ArticleType = (string)Item["ArticleType"];
                                        TransLines.Barcode = (string)Item["Barcode"];
                                        TransLines.IsLoyalty = (bool)Item["BlockedMemberPoint"];
                                        if (Item.TryGetValue("ScanTime", out var scanTimeValue) && scanTimeValue.Type != JTokenType.Null)
                                        {
                                            if (DateTime.TryParse(scanTimeValue.ToString(), out DateTime scanTime))
                                            {
                                                ScanTime = scanTime;
                                            }
                                        }
                                        TransLineResult.Add(TransLines);
                                    }
                                }
                                //TransHeader//

                                List<OrderExpToGCP> TransHeaderss = new List<OrderExpToGCP>();
                                JArray TransHeader = (JArray)jsonObject["Data"]["TransHeader"];
                                foreach (JObject headerItem in TransHeader)
                                {
                                    OrderExpToGCP TransHeaders = new OrderExpToGCP();
                                    TransHeaders.OrderNo = (string)headerItem["OrderNo"];
                                    TransHeaders.OrderDate = (DateTime)headerItem["OrderDate"];
                                    TransHeaders.StoreNo = (string)headerItem["StoreNo"];
                                    TransHeaders.PosNo = (string)headerItem["POSTerminalNo"];
                                    string label = (string)headerItem["Label"];
                                    TransHeaders.CustName = label != null ? Regex.Replace(label, "[^a-zA-Z0-9]", "") : "";
                                    TransHeaders.Note = (string)headerItem["Note"];
                                    TransHeaders.TransactionType = (int)headerItem["TransactionType"];
                                    TransHeaders.SalesType = (string)headerItem["SalesType"];
                                    TransHeaders.OrderTime = (DateTime)headerItem["OrderTime"];
                                    if (Namefuntion == "PLH_GCP_Retry_New")
                                    {
                                        TransHeaders.IsRetry = true;
                                    }
                                    else
                                    {
                                        TransHeaders.IsRetry = false;
                                    }
                                    TransHeaders.Items = TransLineResult.Where(p => p.OrderNo == TransHeaders.OrderNo).ToList();
                                    TransHeaders.Payments = PaymentEntryResult.Where(p => p.OrderNo == TransHeaders.OrderNo).ToList();
                                    TransHeaders.DiscountEntry = DiscountEntryResult.Where(p => p.OrderNo == TransHeaders.OrderNo).ToList();
                                    TransHeaders.CouponEntry = CouponEntryResult.Where(p => p.OrderNo == TransHeaders.OrderNo).ToList();
                                    TransHeaders.TransPointEntry = TranspointDataResult.Where(p => p.OrderNo == TransHeaders.OrderNo).ToList();
                                    concurrentBag.Add(TransHeaders);

                                    SP_Data_WCM SP_Data_WCMss = new SP_Data_WCM();
                                    SP_Data_WCMss.ID = result.ID;
                                    SP_Data_WCMss.OrderNo = TransHeaders.OrderNo;
                                    SP_Data_WCMss.OrderDate = TransHeaders.OrderDate;
                                    SP_Data_WCMss.ChgDate = DateTime.Now;
                                    SP_Data_WCMss.IsRead = true;
                                    SP_Data_WCMss.DataJson = result.DataJson;
                                    SP_Data_WCMss.MemberCardNo = (string)headerItem["MemberCardNo"];
                                    SP_Data_WCMss.VATAmount = VATAmount;
                                    SP_Data_WCMss.LineAmountIncVAT = LineAmountIncVAT;
                                    SP_Data_WCMss.DiscountAmount = DiscountAmount;
                                    SP_Data_WCMs.Add(SP_Data_WCMss);
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

                        if (Namefuntion == "PLH_GCP_Retry_New")
                        {
                            //_logger.Information(Namefuntion);
                            DataJson_PLHs.UpdateStatusWCM_Retry_Json(SP_Data_WCMs, configWcm);
                        }
                        else
                        {
                            //_logger.Information(Namefuntion);
                            DataJson_PLHs.UpdateStatusWCM(SP_Data_WCMs, configWcm);

                        }

                        return concurrentBag.ToList();
                    }
                    catch (Exception e)
                    {
                        _logger.Information("Không có Data" + e.Message);
                        return new List<OrderExpToGCP>();
                    }
                }
            }
        }//Phúc Long GCP
        public void OrderPLHToGCPAsync_Json(string configWcm, List<string> reciept)//Phúc Long Retry
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
                        DbsetWcm.Query(WCM_Data.Insert_Data_RetryProc_PLH(), parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                        List<SP_Data_WCM_Insert> SP_Data_WCMs = new List<SP_Data_WCM_Insert>();
                        foreach (var item in reciept)
                        {
                            SP_Data_WCM_Insert sP_Data_WCM = new SP_Data_WCM_Insert();
                            var tempObject = new TempObject_PLH
                            {
                                TransDiscountEntry = DbsetWcm.Query<object>(WCM_Data.Transdiscount_Retry_PL(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransPaymentEntry = DbsetWcm.Query<object>(WCM_Data.TransPayment_Retry_PL(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransDiscountCouponEntry = DbsetWcm.Query<object>(WCM_Data.TransCp_Retry_PL(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransPointLine = DbsetWcm.Query<object>(WCM_Data.TransPoin_Retry_PL(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransHeader = DbsetWcm.Query<object>(WCM_Data.Transheader_Retry_PL(), new { ReceiptNo = item }, commandTimeout: timeout).ToList(),
                                TransLine = DbsetWcm.Query<object>(WCM_Data.TransLine_Retry_PL(), new { ReceiptNo = item }, commandTimeout: timeout).ToList()
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
                                        tempObject.TransPointLine,
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

    }

}

