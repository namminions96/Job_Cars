using Confluent.Kafka;
using Job_By_SAP.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Job_By_SAP.WCM
{
    public class ReadDataRawJson
    {
        private readonly ILogger _logger;
        public ReadDataRawJson(ILogger logger)
        {
            _logger = logger;
        }
        IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
        public List<TransInputDataGCP> TransInputDataGCP(JArray TransInputData)
        {
            List<TransInputDataGCP> TransInputDatasss = new List<TransInputDataGCP>();
            if (TransInputData != null)
            {
                foreach (JObject TransInputDatass in TransInputData)
                {
                    TransInputDataGCP TransInputDatas = new TransInputDataGCP();
                    TransInputDatas.TransNo = (string)TransInputDatass["TransNo"];
                    TransInputDatas.LineNumber = (string)TransInputDatass["LineNumber"];
                    TransInputDatas.TableName = (string)TransInputDatass["TableName"];
                    TransInputDatas.DataType = (string)TransInputDatass["DataType"];
                    TransInputDatas.DataValue = (string)TransInputDatass["DataValue"];
                    TransInputDatasss.Add(TransInputDatas);
                }
                return TransInputDatasss;
            }
            else
            {
                return new List<TransInputDataGCP>();
            }


        }
        public List<TransDiscountCouponEntryGCP> TransDiscountCouponEntryGCP(JArray Data)
        {
            List<TransDiscountCouponEntryGCP> CouponEntryss = new List<TransDiscountCouponEntryGCP>();
            //TransDiscountCouponEntry//
            if (Data != null)
            {

                foreach (JObject CouponEntry in Data)
                {
                    TransDiscountCouponEntryGCP CouponEntrys = new TransDiscountCouponEntryGCP();
                    CouponEntrys.OrderNo = (string)CouponEntry["OrderNo"];
                    CouponEntrys.ParentLineId = (int)CouponEntry["OrderLineNo"];
                    CouponEntrys.LineId = (int)CouponEntry["LineNo"];
                    CouponEntrys.OfferNo = (string)CouponEntry["ItemNo"];
                    CouponEntrys.OfferType = (string)CouponEntry["OfferType"];
                    CouponEntrys.Barcode = (string)CouponEntry["Barcode"];
                    CouponEntrys.DiscountAmount = (decimal)CouponEntry["DiscountAmount"];//DiscountAmount
                    CouponEntryss.Add(CouponEntrys);
                }
                return CouponEntryss;
            }
            else
            {
                return new List<TransDiscountCouponEntryGCP>();
            }

        }
        public List<TransPaymentEntryGCP> TransPaymentEntryGCP(JArray Data)
        {
            List<TransPaymentEntryGCP> PaymentEntryss = new List<TransPaymentEntryGCP>();
            if (Data != null)
            {
                foreach (JObject PaymentEntry in Data)
                {
                    TransPaymentEntryGCP PaymentEntrys = new TransPaymentEntryGCP();
                    PaymentEntrys.ReceiptNo = (string)PaymentEntry["OrderNo"];
                    PaymentEntrys.LineNo = (int)PaymentEntry["LineNo"];
                    PaymentEntrys.TenderType = (string)PaymentEntry["TenderType"];
                    PaymentEntrys.CurrencyCode = (string)PaymentEntry["OfferNo"];
                    PaymentEntrys.ExchangeRate = (decimal)PaymentEntry["ExchangeRate"];
                    PaymentEntrys.AmountTendered = (decimal)PaymentEntry["AmountTendered"];
                    PaymentEntrys.AmountInCurrency = (decimal)PaymentEntry["AmountInCurrency"];
                    PaymentEntrys.ApprovalCode = (string)PaymentEntry["ApprovalCode"];
                    PaymentEntrys.ReferenceNo = (string)PaymentEntry["ReferenceNo"];
                    PaymentEntrys.BankPOSCode = (string)PaymentEntry["BankPOSCode"];
                    PaymentEntrys.BankCardType = (string)PaymentEntry["BankCardType"];
                    if (PaymentEntry.TryGetValue("IsOnline", out var isOnlineValue) && isOnlineValue.Type != JTokenType.Null)
                    {
                        PaymentEntrys.IsOnline = (bool)isOnlineValue;
                    }
                    PaymentEntryss.Add(PaymentEntrys);
                }
                return PaymentEntryss;
            }
            else
            {
                return new List<TransPaymentEntryGCP>();
            }
        }
        public List<TransDiscountGCP> TransDiscountGCP(JArray Data)
        {
            List<TransDiscountGCP> DiscountEntryss = new List<TransDiscountGCP>();
            List<additionalStringsDiscount> additionalStringsDiscount = new List<additionalStringsDiscount>();
            List<string> strings = new List<string>();
            if (Data != null)
            {
                foreach (JObject DiscountEntry in Data)
                {
                    TransDiscountGCP DiscountEntrys = new TransDiscountGCP();
                    DiscountEntrys.ReceiptNo = (string)DiscountEntry["OrderNo"];
                    DiscountEntrys.LineNo = (int)DiscountEntry["LineNo"];
                    DiscountEntrys.TranNo = (int)DiscountEntry["OrderLineNo"];
                    DiscountEntrys.ItemNo = (string)DiscountEntry["ItemNo"];
                    DiscountEntrys.UOM = (string)DiscountEntry["UOM"];
                    DiscountEntrys.OfferType = (string)DiscountEntry["OfferType"];
                    DiscountEntrys.OfferNo = (string)DiscountEntry["OfferNo"];
                    DiscountEntrys.Quantity = (int)DiscountEntry["Quantity"];
                    if (DiscountEntry.TryGetValue("DiscountAmount", out var discountAmountValue) && discountAmountValue.Type != JTokenType.Null)
                    {
                        DiscountEntrys.DiscountAmount = (decimal)discountAmountValue;
                    }
                    DiscountEntryss.Add(DiscountEntrys);
                }
                return (DiscountEntryss);
            }
            else
            {
                return (new List<TransDiscountGCP>());
            }
        }
        public List<TransLineGCP> TransLineGCP(JArray Data, List<TransDiscountGCP> transDiscountGCPs)
        {
            List<TransLineGCP> TransLiness = new List<TransLineGCP>();
            DateTime ScanTime = new DateTime();
            foreach (JObject Item in Data)
            {
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
                if (Item.TryGetValue("LineAmountIncVAT", out var lineAmountValue) && lineAmountValue.Type != JTokenType.Null)
                {
                    TransLines.Amount = (decimal)lineAmountValue;
                }

                TransLines.Brand = (string)Item["DivisionCode"];
                TransLines.DiscountEntry = transDiscountGCPs.Where(p => p.ItemNo == TransLines.Article && p.TranNo == TransLines.TranNo).ToList();

                if (Item.TryGetValue("ScanTime", out var scanTimeValue) && scanTimeValue.Type != JTokenType.Null)
                {
                    if (DateTime.TryParse(scanTimeValue.ToString(), out DateTime scanTime))
                    {
                        ScanTime = scanTime;
                    }
                }
                if (Linetype == 0)
                {
                    TransLiness.Add(TransLines);
                }

            }
            return TransLiness;
        }
        public void UpdateStatusWCM(List<SP_Data_WCM> SP_Data_WCM, string configWcm)
        {
            try
            {
                var timeout = 600;
                foreach (SP_Data_WCM data_WCMs in SP_Data_WCM)
                {
                    using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                    {
                        DbsetWcm.Open();
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.UpdateWCM();
                            command.Parameters.AddWithValue("@OrderNo", data_WCMs.OrderNo);
                            command.Parameters.AddWithValue("@Id", data_WCMs.ID);
                            command.Parameters.AddWithValue("@IsRead", data_WCMs.IsRead);
                            command.Parameters.AddWithValue("@ChgDate", data_WCMs.ChgDate);
                            command.Parameters.AddWithValue("@MemberCardNo", data_WCMs.MemberCardNo);
                            command.Parameters.AddWithValue("@DiscountAmount", data_WCMs.DiscountAmount);
                            command.Parameters.AddWithValue("@VATAmount", data_WCMs.VATAmount);
                            command.Parameters.AddWithValue("@LineAmountIncVAT", data_WCMs.LineAmountIncVAT);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateStatusWCM_Retry_Json(List<SP_Data_WCM> SP_Data_WCM, string configWcm)
        {
            try
            {
                var timeout = 600;
                foreach (SP_Data_WCM data_WCMs in SP_Data_WCM)
                {
                    using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                    {
                        DbsetWcm.Open();
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.UpdateWCM_Retry_Json();
                            command.Parameters.AddWithValue("@OrderNo", data_WCMs.OrderNo);
                            command.Parameters.AddWithValue("@Id", data_WCMs.ID);
                            command.Parameters.AddWithValue("@IsRead", data_WCMs.IsRead);
                            command.Parameters.AddWithValue("@ChgDate", data_WCMs.ChgDate);
                            command.Parameters.AddWithValue("@MemberCardNo", data_WCMs.MemberCardNo);
                            command.Parameters.AddWithValue("@DiscountAmount", data_WCMs.DiscountAmount);
                            command.Parameters.AddWithValue("@VATAmount", data_WCMs.VATAmount);
                            command.Parameters.AddWithValue("@LineAmountIncVAT", data_WCMs.LineAmountIncVAT);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                        DbsetWcm.Close();
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateStatusVoidWCM(List<SP_Data_WCM> SP_Data_WCM, string configWcm)
        {
            try
            {
                var timeout = 600;
                foreach (SP_Data_WCM data_WCMs in SP_Data_WCM)
                {
                    using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                    {
                        DbsetWcm.Open();
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.UpdateWCMVoid();
                            command.Parameters.AddWithValue("@OrderNo", data_WCMs.OrderNo);
                            command.Parameters.AddWithValue("@Id", data_WCMs.ID);
                            command.Parameters.AddWithValue("@IsRead", 1);
                            command.Parameters.AddWithValue("@ChgDate", data_WCMs.ChgDate);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void InsertStatusWCM(List<SP_Data_WCM_Insert> SP_Data_WCM, string configWcm)
        {
            try
            {
                using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                {
                    DbsetWcm.Open();
                    var timeout = 600;
                    foreach (SP_Data_WCM_Insert data_WCMs in SP_Data_WCM)
                    {


                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.InsertWCM_BK();
                            command.Parameters.AddWithValue("@OrderNo", data_WCMs.OrderNo);
                            command.Parameters.AddWithValue("@Id", data_WCMs.ID);
                            command.Parameters.AddWithValue("@IsRead", data_WCMs.IsRead);
                            command.Parameters.AddWithValue("@ChgDate", data_WCMs.ChgDate);
                            command.Parameters.AddWithValue("@MemberCardNo", data_WCMs.MemberCardNo);
                            command.Parameters.AddWithValue("@DiscountAmount", data_WCMs.DiscountAmount);
                            command.Parameters.AddWithValue("@VATAmount", data_WCMs.VATAmount);
                            command.Parameters.AddWithValue("@LineAmountIncVAT", data_WCMs.LineAmountIncVAT);
                            //------------------------------------
                            command.Parameters.AddWithValue("@StoreNo", data_WCMs.StoreNo);
                            command.Parameters.AddWithValue("@PosNo", data_WCMs.PosNo);
                            command.Parameters.AddWithValue("@Type", data_WCMs.Type);
                            command.Parameters.AddWithValue("@BatchFile", data_WCMs.BatchFile);
                            command.Parameters.AddWithValue("@FileName", data_WCMs.FileName);
                            command.Parameters.AddWithValue("@CrtDate", data_WCMs.CrtDate);
                            command.Parameters.AddWithValue("@OrderDate", data_WCMs.OrderDate);
                            command.Parameters.AddWithValue("@DataJson", data_WCMs.DataJson);
                            command.Parameters.AddWithValue("@Srv", "");
                            command.Parameters.AddWithValue("@TransactionType", "");
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                    DbsetWcm.Close();
                }

            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateStatusWCM_Retry_data(List<SP_Data_WCM_Insert> SP_Data_WCM, string configWcm)
        {
            try
            {
                using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                {
                    DbsetWcm.Open();
                    foreach (SP_Data_WCM_Insert data_WCMs in SP_Data_WCM)
                    {
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.UpdateWCM_Retry();
                            command.Parameters.AddWithValue("@OrderNo", data_WCMs.OrderNo);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                    DbsetWcm.Close();
                }

            }
            catch (Exception ex)
            {
                _logger.Information(ex.Message);

            }
        }
        public void UpdateStatusWCM_Retry(List<SP_Data_WCM> SP_Data_WCM, string configWcm)
        {
            try
            {
                using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                {
                    DbsetWcm.Open();
                    foreach (SP_Data_WCM data_WCMs in SP_Data_WCM)
                    {
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.UpdateWCM_Retry();
                            command.Parameters.AddWithValue("@OrderNo", data_WCMs.OrderNo);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                    DbsetWcm.Close();
                }

            }
            catch (Exception ex)
            {
                _logger.Information(ex.Message);

            }
        }
        public void Insert_WPH_Zalo_Sv(List<Temp_Zalo_Survey> SP_Data_WCM, string configWcm)
        {
            try
            {
                var timeout = 600;
                foreach (Temp_Zalo_Survey data_WCMs in SP_Data_WCM)
                {
                    using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                    {
                        DbsetWcm.Open();
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.Insert_Zalo_WPH();
                            command.Parameters.AddWithValue("@RECEIPT_NO", data_WCMs.RECEIPT_NO);
                            command.Parameters.AddWithValue("@PhoneNo", data_WCMs.PhoneNo);
                            command.Parameters.AddWithValue("@OrderDate", data_WCMs.OrderDate);
                            command.Parameters.AddWithValue("@UpdateFlg", data_WCMs.UpdateFlg);
                            command.Parameters.AddWithValue("@CrtDate", data_WCMs.CrtDate);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void Update_WPH_Zalo_Sv(string RECEIPT_NO, string configWcm)
        {
            try
            {
                var timeout = 600;
                using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
                {
                    DbsetWcm.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = DbsetWcm;
                        command.CommandText = WCM_Data.Update_Zalo_WPH();
                        command.Parameters.AddWithValue("@RECEIPT_NO", RECEIPT_NO);
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void Insert_RP_Detail(List<ReportSaleDetail> SP_Data_WCM, string configWcm)
        {
            string configReport = configuration["DB_112_Report"];
            try
            {
                var timeout = 600;
                foreach (ReportSaleDetail data_WCMs in SP_Data_WCM)
                {
                    using (SqlConnection DbsetWcm = new SqlConnection(configReport))
                    {
                        DbsetWcm.Open();
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = DbsetWcm;
                            command.CommandText = WCM_Data.Insert_RP_Detail();
                            command.Parameters.AddWithValue("@PromotionID", data_WCMs.PromotionID != null ? data_WCMs.PromotionID : DBNull.Value);
                            command.Parameters.AddWithValue("@CouponCode", data_WCMs.CouponCode != null ? data_WCMs.CouponCode : DBNull.Value);
                            command.Parameters.AddWithValue("@LineNo", data_WCMs.LineNo);
                            command.Parameters.AddWithValue("@OrderNo", data_WCMs.OrderNo);
                            command.Parameters.AddWithValue("@OrderTime", data_WCMs.OrderTime);
                            command.Parameters.AddWithValue("@OrderDate", data_WCMs.OrderDate);
                            command.Parameters.AddWithValue("@StoreNo", data_WCMs.StoreNo);
                            command.Parameters.AddWithValue("@POSTerminalNo", data_WCMs.POSTerminalNo);
                            command.Parameters.AddWithValue("@CashierID", data_WCMs.CashierID);
                            command.Parameters.AddWithValue("@ReturnedOrderNo", data_WCMs.ReturnedOrderNo);
                            command.Parameters.AddWithValue("@SalesIsReturn", data_WCMs.SalesIsReturn);
                            command.Parameters.AddWithValue("@Barcode", data_WCMs.Barcode);
                            command.Parameters.AddWithValue("@ItemNo", data_WCMs.ItemNo);
                            command.Parameters.AddWithValue("@Description", data_WCMs.Description);
                            command.Parameters.AddWithValue("@UnitOfMeasure", data_WCMs.UnitOfMeasure);
                            command.Parameters.AddWithValue("@Quantity", data_WCMs.Quantity);
                            command.Parameters.AddWithValue("@UnitPrice", data_WCMs.UnitPrice);
                            command.Parameters.AddWithValue("@DiscountAmount", data_WCMs.DiscountAmount);
                            command.Parameters.AddWithValue("@VATCode", data_WCMs.VATCode);
                            command.Parameters.AddWithValue("@LineAmountIncVAT", data_WCMs.LineAmountIncVAT);
                            command.Parameters.AddWithValue("@VATAmount", data_WCMs.VATAmount);
                            command.Parameters.AddWithValue("@HouseNo", data_WCMs.HouseNo);
                            command.Parameters.AddWithValue("@CityNo", data_WCMs.CityNo);
                            command.Parameters.AddWithValue("@MemberCardNo", data_WCMs.MemberCardNo);
                            command.Parameters.AddWithValue("@MemberPointsEarn", data_WCMs.MemberPointsEarn);
                            command.Parameters.AddWithValue("@MemberPointsRedeem", data_WCMs.MemberPointsRedeem);
                            command.Parameters.AddWithValue("@BlockedMemberPoint", data_WCMs.BlockedMemberPoint);
                            command.Parameters.AddWithValue("@AmountCalPoint", data_WCMs.AmountCalPoint);
                            command.Parameters.AddWithValue("@RefKey1", data_WCMs.RefKey1 != null ? data_WCMs.RefKey1 : "");
                            command.Parameters.AddWithValue("@VoucherDiscountNo", data_WCMs.VoucherDiscountNo != null ? data_WCMs.VoucherDiscountNo : "");
                            command.Parameters.AddWithValue("@DeliveringMethod", data_WCMs.DeliveringMethod != null ? data_WCMs.DeliveringMethod :"");
                            command.Parameters.AddWithValue("@DivisionCode", data_WCMs.DivisionCode != null ? data_WCMs.DivisionCode : DBNull.Value);
                            command.Parameters.AddWithValue("@UserID", data_WCMs.UserID != null ? data_WCMs.UserID : DBNull.Value);
                            command.Parameters.AddWithValue("@SerialNo", data_WCMs.SerialNo != null ? data_WCMs.SerialNo : DBNull.Value);
                            command.Parameters.AddWithValue("@DeliveryComment", data_WCMs.DeliveryComment != null ? data_WCMs.DeliveryComment :"");
                            command.Parameters.AddWithValue("@TanencyNo", data_WCMs.TanencyNo != null ? data_WCMs.TanencyNo : "");
                            command.Parameters.AddWithValue("@BusinessAreaNo", data_WCMs.BusinessAreaNo != null ? data_WCMs.BusinessAreaNo : DBNull.Value);
                            command.Parameters.AddWithValue("@StyleProfile", data_WCMs.StyleProfile != null ? data_WCMs.StyleProfile : DBNull.Value);
                            command.Parameters.AddWithValue("@CustomerName", data_WCMs.CustomerName != null ? data_WCMs.CustomerName :"");
                            command.Parameters.AddWithValue("@AmountDiscountAtPOS", data_WCMs.AmountDiscountAtPOS != null ? data_WCMs.AmountDiscountAtPOS : DBNull.Value);
                            command.Parameters.AddWithValue("@VATPercent", data_WCMs.VATPercent != null ? data_WCMs.VATPercent : DBNull.Value);
                            command.Parameters.AddWithValue("@IsTenancy", data_WCMs.IsTenancy != null ? data_WCMs.IsTenancy : "");
                            command.Parameters.AddWithValue("@SalesType", data_WCMs.SalesType != null ? data_WCMs.SalesType : DBNull.Value);
                            command.Parameters.AddWithValue("@SOURCEBILL", data_WCMs.SOURCEBILL != null ? data_WCMs.SOURCEBILL : "");
                            command.Parameters.AddWithValue("@HANDLINGSTAFF", data_WCMs.HANDLINGSTAFF != null ? data_WCMs.HANDLINGSTAFF : "");
                            command.Parameters.AddWithValue("@ReturnVoucherNo", data_WCMs.ReturnVoucherNo != null ? data_WCMs.ReturnVoucherNo : DBNull.Value);
                            command.Parameters.AddWithValue("@ReturnVoucherExpire", data_WCMs.ReturnVoucherExpire != null ? data_WCMs.ReturnVoucherExpire : DBNull.Value);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void Insert_RP_Kafka(List<ReportSaleDetail> SP_Data_WCM, string configWcm)
        {
            try
            {
                string configKafka = configuration["ConfigKafka"];
                var config = new ProducerConfig { BootstrapServers = configKafka };

                using (var producer = new ProducerBuilder<Null, string>(config).Build())
                {
                    foreach (ReportSaleDetail data_WCMs in SP_Data_WCM)
                    {
                        try
                        {
                            string json = JsonConvert.SerializeObject(data_WCMs);
                            var deliveryReport =  producer.ProduceAsync("ReportSaleDetail", new Message<Null, string> { Value = json });
                        }
                        catch (ProduceException<Null, string> e)
                        {
                            _logger.Information($"Delivery failed: {e.Error.Reason}");
                        }
                    }

                    producer.Flush(TimeSpan.FromSeconds(10));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }
    }

}

          