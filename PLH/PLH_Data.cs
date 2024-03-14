using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.PLH
{
    public class PLH_Data
    {
   
        public static string TransLineQueryArchive()
        {
            return @"SELECT DocumentNo [OrderNo], [LineNo] LineId, [LineType] ParentLineId, ItemNo, [Description] ItemName, [UnitOfMeasure] Uom, [UnitPrice] OldPrice,[UnitPrice] UnitPrice, [Quantity] Qty, DiscountAmount,[LineAmountIncVAT] LineAmount, 
		                [VATCode] VatGroup, [VATPercent] VatPercent, Note, '' CupType, '' Size, ISNULL(IsTopping, 0) IsTopping, IsCombo, 0 ComboId, ArticleType, Barcode, BlockedMemberPoint IsLoyalty
                        FROM CentralSalesArchive.dbo.TransLine (NOLOCK) WHERE DocumentNo IN @documentNo AND LineType IN (0, 1) ";
        }
        public static string TransPaymentEntryQueryArchive()
        {
            return @"SELECT [OrderNo], [LineNo] LineId, [TenderType] PaymentMethod, CurrencyCode, ExchangeRate, AmountTendered, AmountInCurrency, TransactionNo
                        FROM CentralSalesArchive.dbo.TransPaymentEntry (NOLOCK) WHERE OrderNo IN @orderNo";
        }
        public static string TransDiscountCouponEntryQueryArchive()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,ItemNo OfferNo,OfferType,Barcode
                     FROM CentralSalesArchive.dbo.[TransDiscountCouponEntry] NOLOCK  WHERE OrderNo IN @orderNo";
        }
        public static string TransDiscountEntryQueryArchive()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo
                     UNION
                     SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     FROM CentralSalesArchive.dbo.TransDiscountCouponEntry NOLOCK 
                     WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')";
        }
        public static string TransLineQuery()
        {
            return @"SELECT DocumentNo [OrderNo], [LineNo] LineId, [LineType] ParentLineId, ItemNo, [Description] ItemName, [UnitOfMeasure] Uom, [UnitPrice] OldPrice,[UnitPrice] UnitPrice, [Quantity] Qty, DiscountAmount,[LineAmountIncVAT] LineAmount, 
		                [VATCode] VatGroup, [VATPercent] VatPercent, Note, '' CupType, '' Size, ISNULL(IsTopping, 0) IsTopping, IsCombo, 0 ComboId, ArticleType, Barcode, BlockedMemberPoint IsLoyalty
                        FROM CentralSales.dbo.TransLine (NOLOCK) WHERE DocumentNo IN @documentNo AND LineType IN (0, 1) ";
        }
        public static string TransPaymentEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId, [TenderType] PaymentMethod, CurrencyCode, ExchangeRate, AmountTendered, AmountInCurrency, TransactionNo
                        FROM CentralSales.dbo.TransPaymentEntry (NOLOCK) WHERE OrderNo IN @orderNo";
        }
     
        public static string TransDiscountEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,ParentLineNo,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo";
                     //UNION
                     //SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,ParentLineNo,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     //FROM CentralSales.dbo.TransDiscountCouponEntry NOLOCK 
                     //WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')
        }
          public static string TransDiscountEntryQuery_BS()
        {
            return @"select top 10 A.[OrderNo], A.LineId LineId,A.ParentLineId ,A.[OfferNo],B.Description,A.[OfferType],COALESCE(D.[OfferName], '') [OfferType_Des]
                                ,A.[Quantity], A.DiscountAmount,A.Note ,B.[SalesType],COALESCE(C.[Description], '')  SalesType_Des,B.[StartingDate],B.[EndingDate]
                                from
                             (SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                             FROM CentralSales.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo
                             UNION
                             SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                             FROM CentralSales.dbo.TransDiscountCouponEntry NOLOCK 
                             WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')
					         )A
					         join [10.235.55.124\PLH].[CentralMD].[dbo].[OfferHeader] B ON A.OfferNo = B.No
							 left join [10.235.55.124\PLH].[CentralMD].[dbo].[SalesOrderType] C ON B.[SalesType] = C.[Code]
							 left join [10.235.55.124\PLH].[CentralMD].[dbo].[OfferType] D ON A.[OfferType] = D.[OfferType]";
        }

        public static string TransPoinEntryQuery()
        {
            return @"select OrderNo,Sum(EarnPoints) EarnPoints,sum(RedeemPoints) RedeemPoints,MemberNumber,CardLevel,MemberCSN from CentralSales.dbo.TransPointLine (NOLOCK) where OrderNo IN @OrderNo
                    group by OrderNo,CardLevel,MemberCSN,MemberNumber";
        }
        public static string TransDiscountCouponEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,ItemNo OfferNo,OfferType,Barcode
                     FROM CentralSales.dbo.[TransDiscountCouponEntry] NOLOCK  WHERE OrderNo IN @orderNo";
        }
        public static string TransPointEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,ItemNo OfferNo,OfferType,Barcode
                     FROM CentralSales.dbo.[TransDiscountCouponEntry] NOLOCK  WHERE OrderNo IN @orderNo";
        }
        public static string InsertTemp_SalesGCP()
        {
            return @"INSERT INTO Temp_SalesGCP ([SalesType], [OrderNo], [OrderDate], [CrtDate], [Batch])
                                        VALUES
                                        (@SalesType, @OrderNo, @OrderDate, @CrtDate, @Batch)";
        }
        public static string InsertTemp_SalesGCP_Retry()
        {
            return @"INSERT INTO Temp_SalesGCP_Retry ([OrderNo],[UpdateFlg], [CrtDate])
                                        VALUES
                                        (@OrderNo, @UpdateFlg, @CrtDate)";
        }

        public static string InsertTemp_SalesGCP_Retry_WCM()
        {
            return @"INSERT INTO Temp_SalesGCP_Retry ([RECEIPT_NO],[UpdateFlg], [CrtDate])
                                        VALUES
                                        (@RECEIPT_NO, @UpdateFlg, @CrtDate)";
        }

        public static string GCP_CSV_PLH_Archive()
        {
            return @"SELECT A.OrderNo,Sum(EarnPoints) EarnPoints,sum(RedeemPoints) RedeemPoints ,A.MemberNumber,A.CardLevel,MemberCSN 
                     FROM CentralSalesArchive.[dbo].[TransPointLine]A
                     join CentralSalesArchive.[dbo].TransHeader B On A.OrderNo = B.OrderNo
                     where OrderDate between '20240101' and'20240117'
                     group by A.OrderNo,A.CardLevel,A.MemberCSN,A.MemberNumber";
        }
        public static string GCP_CSV_PLH_Prd()
        {
            return @"SELECT A.OrderNo,Sum(EarnPoints) EarnPoints,sum(RedeemPoints) RedeemPoints,A.MemberNumber,A.CardLevel,MemberCSN 
                        FROM CentralSales.[dbo].[TransPointLine]A
                        join CentralSales.[dbo].TransHeader B On A.OrderNo = B.OrderNo
                        where OrderDate between '20240101' and'20240117'
                        group by A.OrderNo,A.CardLevel,A.MemberCSN,A.MemberNumber";
        }


    }
}
