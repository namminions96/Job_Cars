using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.Models
{
    public class SalesGCP_Retry
    {
        public class Temp_SalesGCP_Retry
        {
            public string RECEIPT_NO { get; set; }
            public string UpdateFlg  { get; set; }
            public string CrtDate { get; set; }

        }
        public class TempObject
        {
            public List<object> TransDiscountCouponEntry { get; set; }
            public List<object> TransDiscountEntry { get; set; }
            public List<object> TransHeader { get; set; }
            public List<object> TransInputData { get; set; }
            public List<object> TransLine { get; set; }
            public List<object> TransPaymentEntry { get; set; }
        }
    }
}
