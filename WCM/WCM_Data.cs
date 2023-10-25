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
        public static string SP_GET_SELLOUT_PBLUE_SET()
        {
            return @"SP_GET_SELLOUT_PBLUE_SET";
        }

        public static string SUMD11_DISCOUNT_BLUE()
        {
            return @"SELECT * FROM [SUMD11_DISCOUNT_BLUE] NOLOCK WHERE UpdateFlg ='N'";
        }
        public static string SUMD11_PAYMENT()
        {
            return @"SELECT [ReceiptNo],[LineNo],[ExchangeRate],[TenderType],[AmountTendered],[CurrencyCode]
            ,[AmountInCurrency],[ReferenceNo],[ApprovalCode], [BankPOSCode],[BankCardType],[IsOnline] FROM [SUMD11_PAYMENT] NOLOCK";
        }

    }
}
