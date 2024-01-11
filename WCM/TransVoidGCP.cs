using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.WCM
{
    public class TransVoidGCP
    {
        public List<TransVoidHeader> TransVoidHeader { get; set; }
        public List<TransVoidLine> TransVoidLine { get; set; }
    }
    public class TransVoidHeader
    {
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string ZoneNo { get; set; }
        public string ShipToAddress { get; set; }
        public string StoreNo { get; set; }
        public string POSTerminalNo { get; set; }
        public string ShiftNo { get; set; }
        public string CashierID { get; set; }
        public decimal AmountInclVAT { get; set; }
        public string UserID { get; set; }
        public decimal PrepaymentAmount { get; set; }
        public int DeliveringMethod { get; set; }
        public string TanencyNo { get; set; }
        public int SalesIsReturn { get; set; }
        public string MemberCardNo { get; set; }
        public string ReturnedOrderNo { get; set; }
        public int TransactionType { get; set; }
        public int PrintedNumber { get; set; }
        public DateTime LastUpdated { get; set; }
        public string UserVoid { get; set; }
        public string DocumentType { get; set; }

    }
    public class TransVoidLine
    {
        public DateTime ScanTime { get; set; }
        public string OrderNo { get; set; }
        public int LineType { get; set; }
        public string LocationCode { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public string StaffID { get; set; }
        public string VATCode { get; set; }
        public int DeliveringMethod { get; set; }
        public string Barcode { get; set; }
        public string DivisionCode { get; set; }
        public string SerialNo { get; set; }
        public string OrigOrderNo { get; set; }
        public string LotNo { get; set; }
        public string ArticleType { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
