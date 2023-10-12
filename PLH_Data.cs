using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP
{
    public class PLH_Data
    {
        public static string TransLineQuery()
        {
            return @"SELECT DocumentNo [OrderNo], [LineNo] LineId, [LineType] ParentLineId, ItemNo, [Description] ItemName, [UnitOfMeasure] Uom, [UnitPrice] OldPrice,[UnitPrice] UnitPrice, [Quantity] Qty, DiscountAmount,[LineAmountIncVAT] LineAmount, 
		                [VATCode] VatGroup, [VATPercent] VatPercent, Note, '' CupType, '' Size, ISNULL(IsTopping, 0) IsTopping, IsCombo, 0 ComboId, ArticleType, Barcode, BlockedMemberPoint IsLoyalty
                        FROM CentralSales.dbo.TransLine (NOLOCK) WHERE DocumentNo IN @documentNo AND LineType IN (0, 1) ";
        }
        public static string TransLineQueryArchive()
        {
            return @"SELECT DocumentNo [OrderNo], [LineNo] LineId, [LineType] ParentLineId, ItemNo, [Description] ItemName, [UnitOfMeasure] Uom, [UnitPrice] OldPrice,[UnitPrice] UnitPrice, [Quantity] Qty, DiscountAmount,[LineAmountIncVAT] LineAmount, 
		                [VATCode] VatGroup, [VATPercent] VatPercent, Note, '' CupType, '' Size, ISNULL(IsTopping, 0) IsTopping, IsCombo, 0 ComboId, ArticleType, Barcode, BlockedMemberPoint IsLoyalty
                        FROM CentralSalesArchive.dbo.TransLine (NOLOCK) WHERE DocumentNo IN @documentNo AND LineType IN (0, 1) ";
        }
        public static string TransPaymentEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId, [TenderType] PaymentMethod, CurrencyCode, ExchangeRate, AmountTendered, AmountInCurrency, TransactionNo
                        FROM CentralSales.dbo.TransPaymentEntry (NOLOCK) WHERE OrderNo IN @orderNo";
        }
        public static string TransPaymentEntryQueryArchive()
        {
            return @"SELECT [OrderNo], [LineNo] LineId, [TenderType] PaymentMethod, CurrencyCode, ExchangeRate, AmountTendered, AmountInCurrency, TransactionNo
                        FROM CentralSalesArchive.dbo.TransPaymentEntry (NOLOCK) WHERE OrderNo IN @orderNo";
        }
        public static string TransDiscountEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo] PromotionNo,[OfferType] PromotionType,[Quantity] Qty, DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo
                     UNION
                     SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo] PromotionNo,[OfferType] PromotionType,[Quantity] Qty, DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.TransDiscountCouponEntry NOLOCK 
                     WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')";
        }
        public static string TransDiscountCouponEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,ItemNo OfferNo,Barcode
                     FROM CentralSales.dbo.[TransDiscountCouponEntry] NOLOCK WHERE OrderNo IN @orderNo";
        }
    }
}
