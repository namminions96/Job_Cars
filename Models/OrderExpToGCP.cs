using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.Models
{
    public class OrderExpToGCP
    {
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string StoreNo { get; set; }
        public string PosNo { get; set; }
        public string CustName { get; set; }
        public string Note { get; set; }
        public int TransactionType { get; set; }
        public string SalesType { get; set; }
        public DateTime OrderTime { get; set; }
        public string ReturnedOrderNo { get; set; }
        public List<TransLine_PLH_BLUEPOS> Items { get; set; }
        public List<TransPaymentEntry_PLH_BLUEPOS> Payments { get; set; }
        public List<TransPointLine_PLH_BLUEPOS> Loyalty { get; set; }
        public List<TransDiscountEntry_PLH_BLUEPOS> DiscountEntry { get; set; }
        public List<TransDiscountCouponEntry_PLH_BLUEPOS> CouponEntry { get; set; }
    }

    public class TransLine_PLH_BLUEPOS
    {
        public string OrderNo { get; set; }
        public int LineId { get; set; } //[LineNo]
        public int ParentLineId { get; set; } //[OrderLineNo]
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public decimal OldPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmount { get; set; }
        public string VatGroup { get; set; }
        public int VatPercent { get; set; }
        public string Note { get; set; } = String.Empty;
        public string CupType { get; set; } = String.Empty;
        public string Size { get; set; } = String.Empty;
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public int ComboId { get; set; }
        public string ArticleType { get; set; } = String.Empty;
        public string Barcode { get; set; } = String.Empty;
        public bool IsLoyalty { get; set; }
    }
    public class TransPaymentEntry_PLH_BLUEPOS
    {
        public string OrderNo { get; set; }
        [Required]
        public int LineId { get; set; }
        [Required]
        //[StringRange(AllowableValues = new[] { "", "C000", "ZCRE", "ZTPA", "TQRP", "ZNAP" }, ErrorMessage = "Hình thức thanh toán không đúng")]
        public string PaymentMethod { get; set; }
        [Required]
        public string CurrencyCode { get; set; }
        [Required]
        public decimal ExchangeRate { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 3)")]
        public decimal AmountTendered { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal AmountInCurrency { get; set; }
        public string TransactionNo { get; set; }
        public string ApprovalCode { get; set; }
        public string TraceCode { get; set; }
        public string ReferenceId { get; set; }
    }
    public class TransPointLine_PLH_BLUEPOS
    {
        public string OrderNo { get; set; }
        public int ParentLineId { get; set; }
        public int LineId { get; set; }
        public string MemberCardNumber { get; set; }
        public string ClubCode { get; set; }
        public decimal LoyaltyPointsEarn { get; set; }
        public decimal LoyaltyPointsRedeem { get; set; }
    }
    public class TransDiscountEntry_PLH_BLUEPOS
    {
        public string OrderNo { get; set; }
        public int ParentLineId { get; set; }
        public int LineId { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; }
    }
    public class TransDiscountCouponEntry_PLH_BLUEPOS
    {
        public string OrderNo { get; set; }
        public int ParentLineId { get; set; }
        public int LineId { get; set; }
        public string OfferNo { get; set; }
        public string Barcode { get; set; }
        public decimal LoyaltyPointsEarn { get; set; }
    }
    public class TempSalesGCP
    {
        public string SalesType { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime CrtDate { get; set; }
        public string Batch { get; set; }
    }
}
