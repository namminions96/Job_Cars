
using Job_By_SAP.Models;
using Job_By_SAP.WCM;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.PLH
{
    public class DataJson_PLH
    {
        private readonly ILogger _logger;
        public DataJson_PLH(ILogger logger)
        {
            _logger = logger;
        }
        public List<TransDiscountCouponEntry_PLH_BLUEPOS> TransDiscountCouponEntryGCP(JArray Data)
        {
            List<TransDiscountCouponEntry_PLH_BLUEPOS> CouponEntryss = new List<TransDiscountCouponEntry_PLH_BLUEPOS>();
            //TransDiscountCouponEntry//
            if (Data != null)
            {

                foreach (JObject CouponEntry in Data)
                {
                    TransDiscountCouponEntry_PLH_BLUEPOS CouponEntrys = new TransDiscountCouponEntry_PLH_BLUEPOS();
                    CouponEntrys.OrderNo = (string)CouponEntry["OrderNo"];
                    CouponEntrys.ParentLineId = (int)CouponEntry["OrderLineNo"];
                    CouponEntrys.LineId = (int)CouponEntry["LineNo"];
                    CouponEntrys.OfferNo = (string)CouponEntry["ItemNo"];
                    CouponEntrys.OfferType = (string)CouponEntry["OfferType"];
                    CouponEntrys.Barcode = (string)CouponEntry["Barcode "];
                    CouponEntryss.Add(CouponEntrys);
                }
                return CouponEntryss;
            }
            else
            {
                return new List<TransDiscountCouponEntry_PLH_BLUEPOS>();
            }
        }
        public List<TransPaymentEntry_PLH_BLUEPOS> TransPaymentEntryGCP(JArray Data)
        {
            List<TransPaymentEntry_PLH_BLUEPOS> PaymentEntryss = new List<TransPaymentEntry_PLH_BLUEPOS>();
            if (Data != null)
            {
                foreach (JObject PaymentEntry in Data)
                {
                    TransPaymentEntry_PLH_BLUEPOS PaymentEntrys = new TransPaymentEntry_PLH_BLUEPOS();
                    PaymentEntrys.OrderNo = (string)PaymentEntry["OrderNo"];
                    PaymentEntrys.LineId = (int)PaymentEntry["LineNo"];
                    PaymentEntrys.PaymentMethod = (string)PaymentEntry["TenderType"];
                    PaymentEntrys.CurrencyCode = (string)PaymentEntry["CurrencyCode"];
                    PaymentEntrys.ExchangeRate = (decimal)PaymentEntry["ExchangeRate"];
                    PaymentEntrys.AmountTendered = (decimal)PaymentEntry["AmountTendered"];
                    PaymentEntrys.AmountInCurrency = (decimal)PaymentEntry["AmountInCurrency"];
                    PaymentEntrys.TransactionNo = (string)PaymentEntry["TransactionNo"];
                    PaymentEntryss.Add(PaymentEntrys);
                }
                return PaymentEntryss;
            }
            else
            {
                return new List<TransPaymentEntry_PLH_BLUEPOS>();
            }
        }
        public List<TransDiscountEntry_PLH_BLUEPOS> TransDiscountGCP(JArray Data)
        {
            List<TransDiscountEntry_PLH_BLUEPOS> DiscountEntryss = new List<TransDiscountEntry_PLH_BLUEPOS>();
            if (Data != null)
            {
                foreach (JObject DiscountEntry in Data)
                {
                    TransDiscountEntry_PLH_BLUEPOS DiscountEntrys = new TransDiscountEntry_PLH_BLUEPOS();
                    DiscountEntrys.OrderNo = (string)DiscountEntry["OrderNo"];
                    DiscountEntrys.LineId = (int)DiscountEntry["LineNo"];
                    DiscountEntrys.ParentLineId = (int)DiscountEntry["OrderLineNo"];
                    DiscountEntrys.Note = (string)DiscountEntry["LineGroup"];
                    DiscountEntrys.OfferType = (string)DiscountEntry["OfferType"];
                    DiscountEntrys.OfferNo = (string)DiscountEntry["OfferNo"];
                    DiscountEntrys.Quantity = (int)DiscountEntry["Quantity"];
                    DiscountEntrys.ParentLineNo = (int)DiscountEntry["ParentLineNo"];
                    if (DiscountEntry.TryGetValue("DiscountAmount", out var discountAmountValue) && discountAmountValue.Type != JTokenType.Null)
                    {
                        DiscountEntrys.DiscountAmount = (decimal)discountAmountValue;
                    }
                    DiscountEntryss.Add(DiscountEntrys);
                }
                return DiscountEntryss;
            }
            else
            {
                return new List<TransDiscountEntry_PLH_BLUEPOS>();
            }
        }
        public List<TransPointEntry_PLH_BLUEPOS> TransPoinGCP(JArray Data)
        {
            try
            {
                List<TransPointEntry_PLH_BLUEPOS> Transpoin = new List<TransPointEntry_PLH_BLUEPOS>();
                foreach (JObject Item in Data)
                {
                    TransPointEntry_PLH_BLUEPOS Transpoins = new TransPointEntry_PLH_BLUEPOS();
                    Transpoins.OrderNo = (string)Item["OrderNo"];
                    if (Item.TryGetValue("EarnPoints", out var EarnPointss) && EarnPointss.Type != JTokenType.Null)
                    {
                        Transpoins.EarnPoints = (double)EarnPointss;
                    }
                    if (Item.TryGetValue("RedeemPoints", out var RedeemPointss) && RedeemPointss.Type != JTokenType.Null)
                    {
                        Transpoins.RedeemPoints = (double)RedeemPointss;
                    }
                    Transpoins.MemberNumber = (string)Item["MemberNumber"];
                    Transpoins.CardLevel = (string)Item["CardLevel"];
                    Transpoins.MemberCSN = (string)Item["MemberCSN"];
                    Transpoin.Add(Transpoins);
                }
                return Transpoin;

            }
            catch (Exception e)
            {
                return new List<TransPointEntry_PLH_BLUEPOS>();
            }
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
    }
}
