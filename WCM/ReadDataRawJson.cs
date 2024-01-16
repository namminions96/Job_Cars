using BluePosVoucher.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.WCM
{
    public class ReadDataRawJson
    {
        private readonly ILogger _logger;
        public ReadDataRawJson(ILogger logger)
        {
            _logger = logger;
        }
        public List<TransInputDataGCP> TransInputDataGCP(JArray TransInputData)
        {
            List<TransInputDataGCP> TransInputDatasss = new List<TransInputDataGCP>();
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
        public List<TransDiscountCouponEntryGCP> TransDiscountCouponEntryGCP(JArray Data)
        {
            List<TransDiscountCouponEntryGCP> CouponEntryss = new List<TransDiscountCouponEntryGCP>();
            //TransDiscountCouponEntry//
            foreach (JObject CouponEntry in Data)
            {
                TransDiscountCouponEntryGCP CouponEntrys = new TransDiscountCouponEntryGCP();
                CouponEntrys.OrderNo = (string)CouponEntry["OrderNo"];
                CouponEntrys.ParentLineId = (int)CouponEntry["OrderLineNo"];
                CouponEntrys.LineId = (int)CouponEntry["LineNo"];
                CouponEntrys.OfferNo = (string)CouponEntry["OfferNo"];
                CouponEntrys.OfferType = (string)CouponEntry["OfferType"];
                CouponEntrys.Barcode = (string)CouponEntry["Barcode "];
                CouponEntrys.DiscountAmount = (decimal)CouponEntry["DiscountAmount"];
                CouponEntryss.Add(CouponEntrys);
            }
            return CouponEntryss;
        }
        public List<TransPaymentEntryGCP> TransPaymentEntryGCP(JArray Data)
        {
            List<TransPaymentEntryGCP> PaymentEntryss = new List<TransPaymentEntryGCP>();
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

        public List<TransDiscountGCP> TransDiscountGCP(JArray Data)
        {
            List<TransDiscountGCP> DiscountEntryss = new List<TransDiscountGCP>();
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
            return DiscountEntryss;
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


    }

}
