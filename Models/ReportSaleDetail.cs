using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.Models
{
    public class ReportSaleDetail
    {
        public string PromotionID { get; set; }
        public string CouponCode { get; set; }
        public int LineNo { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime OrderDate { get; set; }
        public string StoreNo { get; set; }
        public string POSTerminalNo { get; set; }
        public string CashierID { get; set; }
        public string ReturnedOrderNo { get; set; }
        public bool SalesIsReturn { get; set; }
        public string Barcode { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; }
        public float UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string VATCode { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public decimal VATAmount { get; set; }
        public string HouseNo { get; set; }
        public string CityNo { get; set; }
        public string MemberCardNo { get; set; }
        public float MemberPointsEarn { get; set; }
        public float MemberPointsRedeem { get; set; }
        public byte BlockedMemberPoint { get; set; }
        public float AmountCalPoint { get; set; }
        public string RefKey1 { get; set; }
        public string VoucherDiscountNo { get; set; }
        public string DeliveringMethod { get; set; }
        public string DivisionCode { get; set; }
        public string UserID { get; set; }
        public string SerialNo { get; set; }
        public string DeliveryComment { get; set; }
        public string TanencyNo { get; set; }
        public string BusinessAreaNo { get; set; }
        public string StyleProfile { get; set; }
        public string CustomerName { get; set; }
        public string AmountDiscountAtPOS { get; set; }
        public float VATPercent { get; set; }
        public byte IsTenancy { get; set; }
        public string SalesType { get; set; }
        public string SOURCEBILL { get; set; }
        public string HANDLINGSTAFF { get; set; }
        public string ReturnVoucherNo { get; set; }
        public string ReturnVoucherExpire { get; set; }
    }
    public class additionalStringsDiscount
    {
        public string PromotionID { get; set; }
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
    }
    public class Store
    {
        public string No { get; set; }
        public string BusinessAreaNo { get; set; }
        public string StyleProfile { get; set; }
    }
}
