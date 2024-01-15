using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP
{
    public class WCM_Data
    {

        public static string Procedure_SaleOut()
        {
            return @"SP_GET_GCP_SELLOUT_ALL";
        }

        public static string Procedure_SaleOut_fix()
        {
            return @"SP_GET_SELLOUT";
        }
        public static string SP_GET_SELLOUT_PBLUE_SET()
        {
            return @"SP_GET_SELLOUT_PBLUE_SET";
        }

        public static string SP_Sale_GCP()
        {
            return @"SP_Sale_GCP";
        }
        public static string SP_Sale_GCP_Retry()
        {
            return @"SP_Sale_GCP_Retry";
        }
        public static string SP_Sale_Void_GCP()
        {
            return @"SP_Sale_Void_GCP";
        }
        public static string UpdateWCM()
        {
            return @" UPDATE[dbo].[DataRawJson]
                        SET
                      [OrderNo] = @OrderNo
                      ,[IsRead] = @IsRead
                      ,[ChgDate] = @ChgDate
                      ,[MemberCardNo] = @MemberCardNo
                      ,[DiscountAmount] = @DiscountAmount
                      ,[VATAmount] = @VATAmount
                      ,[LineAmountIncVAT] = @LineAmountIncVAT
                       WHERE[Id] = @Id";
        }
        public static string UpdateWCM_Retry()
        {
            return @" UPDATE[dbo].[Temp_SalesGCP_Retry]
                        SET
                       [UpdateFlg] = 'Y'
                       WHERE [RECEIPT_NO] = @OrderNo";
        }

        public static string SUMD11_DISCOUNT_BLUE()
        {
            return @"SELECT * FROM [SUMD11_DISCOUNT_BLUE] NOLOCK WHERE UpdateFlg ='N'";
        }

        public static string SUMD11_DISCOUNT_BLUE_New()
        {
            return @"  SELECT* FROM[SUMD11_DISCOUNT_BLUE] NOLOCK WHERE UpdateFlg ='N' and ReceiptNo IN @ReceiptNo";
        }

        public static string SUMD11_PAYMENT()
        {
            return @"SELECT [ReceiptNo],[LineNo],[ExchangeRate],[TenderType],[AmountTendered],[CurrencyCode]
            ,[AmountInCurrency],[ReferenceNo],[ApprovalCode], [BankPOSCode],[BankCardType],[IsOnline] FROM [SUMD11_PAYMENT] NOLOCK";
        }
        public static string SUMD11_PAYMENT_New()
        {
            return @"SELECT [ReceiptNo],[LineNo],[ExchangeRate],[TenderType],[AmountTendered],[CurrencyCode]
            ,[AmountInCurrency],[ReferenceNo],[ApprovalCode], [BankPOSCode],[BankCardType],[IsOnline] FROM [SUMD11_PAYMENT] NOLOCK where ReceiptNo IN @ReceiptNo";
        }
        public static string SUMD11_Coupon_New()
        {
            return @"SELECT OrderNo ,[OrderLineNo] ParentLineId, [LineNo] LineId, ItemNo [OfferNo],OfferType, [Barcode],[DiscountAmount] FROM SUMD11_COUPON NOLOCK where OrderNo IN @ReceiptNo";
        }


    }
}
