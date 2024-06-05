using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.Models
{
    public class TransHeader_Temp
    {
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string SaleType { get; set; }
        public string TransactionType { get; set; }
        public string MemberCardNo { get; set; }
        public string SalesStoreNo { get; set; }
        public string SalesPosNo { get; set; }
        public string RefKey { get; set; }
    }
    public class TransHeader_PLH_WCM
    {
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string SaleType { get; set; }
        public string TransactionType { get; set; }
        public string MemberCardNo { get; set; }
        public string SalesStoreNo { get; set; }
        public string SalesPosNo { get; set; }
        public string RefNo { get; set; }
        public List<TransLine_PLH_WCM> TransLine { get; set; }
        public List<TransPaymentEntry_PLH_WCM> TransPaymentEntry { get; set; }
    }
    public class TransLine_PLH_WCM
    {
        public int LineNo { get; set; }
        public int ParentLineNo { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double DiscountAmount { get; set; }
        public double VATPercent { get; set; }
        public double LineAmount { get; set; }
        public double MemberPointsEarn { get; set; }
        public double MemberPointsRedeem { get; set; }
        public string CupType { get; set; }
        public string Size { get; set; }
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public DateTime ScanTime { get; set; }
    }
    public class TransLine_PLH_WCM_TEMP
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public int ParentLineNo { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double DiscountAmount { get; set; }
        public double VATPercent { get; set; }
        public double LineAmountIncVAT { get; set; }
        public double MemberPointsEarn { get; set; }
        public double MemberPointsRedeem { get; set; }
        public string CupType { get; set; }
        public string Size { get; set; }
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public DateTime ScanTime { get; set; }
    }
    public class TransPaymentEntry_PLH_WCM
    {
        public int LineNo { get; set; }
        public string TenderType { get; set; }
        public string CurrencyCode { get; set; }
        public int ExchangeRate { get; set; }
        public double PaymentAmount { get; set; }
        public string ReferenceNo { get; set; }
    }
    public class TransPaymentEntry_PLH_WCM_TEMP
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public string TenderType { get; set; }
        public string CurrencyCode { get; set; }
        public int ExchangeRate { get; set; }
        public double PaymentAmount { get; set; }
        public string ReferenceNo { get; set; }
    }
}
