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
        public static string SUMMARY_SALES_OUT()
        {
            return @"SP_SUMMARY_SALES_OUT";
        }
        public static string SP_Zalo_Survey_WPH()
        {
            return @"SP_Zalo_Survey_WPH";
        }
        public static string SP_Sale_GCP()
        {
            return @"SP_Sale_GCP";
        }
        public static string SP_Sale_GCP_PLH()
        {
            return @"SP_Sale_GCP_PLH";
        }
        public static string SP_Sale_GCP_Retry()
        {
            return @"SP_Sale_GCP_Retry";
        }
        public static string SP_Sale_GCP_Retry_PLH()
        {
            return @"SP_Sale_GCP_Retry_PLH";
        }
        public static string SP_Sale_Void_GCP()
        {
            return @"SP_Sale_Void_GCP";
        }
        public static string Insert_Data_Retry()
        {
            return @"SP_GET_RETRY_ORDER";
        }
        public static string Update_Data_Temp()
        {
            return @"update [StagingDB].[dbo].Temp_SalesGCP_Retry  set UpdateFlg='Y'
                        where RECEIPT_NO  = @RECEIPT_NO ";
        }
        public static string Insert_Data_RetryProc()
        {
            return @"GET_ALL_SALEOUT";
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
        public static string Insert_Zalo_WPH()
        {
            return @" INSERT INTO [dbo].[Temp_Zalo_Survey]
           ([RECEIPT_NO]
           ,[PhoneNo]
           ,[OrderDate]
           ,[UpdateFlg]
           ,[CrtDate])
            VALUES(@RECEIPT_NO,@PhoneNo,@OrderDate,@UpdateFlg,@CrtDate)";
        }

        public static string Insert_RP_Detail()
        {
            return @" INSERT INTO [dbo].[ReportSaleDetail]
           ([PromotionID],[CouponCode],[LineNo],[OrderNo],[OrderTime],[OrderDate],[StoreNo],[POSTerminalNo],[CashierID],[ReturnedOrderNo],[SalesIsReturn],[Barcode],[ItemNo],[Description]
           ,[UnitOfMeasure],[Quantity],[UnitPrice],[DiscountAmount],[VATCode],[LineAmountIncVAT],[VATAmount],[HouseNo],[CityNo],[MemberCardNo]
           ,[MemberPointsEarn],[MemberPointsRedeem] ,[BlockedMemberPoint],[AmountCalPoint],[RefKey1],[VoucherDiscountNo],[DeliveringMethod],[DivisionCode]
           ,[UserID],[SerialNo],[DeliveryComment] ,[TanencyNo],[BusinessAreaNo],[StyleProfile],[CustomerName],[AmountDiscountAtPOS],[VATPercent],[IsTenancy]
           ,[SalesType],[SOURCEBILL],[HANDLINGSTAFF],[ReturnVoucherNo],[ReturnVoucherExpire])
     VALUES (@PromotionID,@CouponCode,@LineNo,@OrderNo,@OrderTime,@OrderDate,@StoreNo,@POSTerminalNo,@CashierID,@ReturnedOrderNo,@SalesIsReturn,@Barcode,
				@ItemNo,@Description,@UnitOfMeasure,@Quantity,@UnitPrice,@DiscountAmount,@VATCode,@LineAmountIncVAT,@VATAmount,@HouseNo,@CityNo,@MemberCardNo,
				@MemberPointsEarn,@MemberPointsRedeem,@BlockedMemberPoint,@AmountCalPoint,@RefKey1,@VoucherDiscountNo,@DeliveringMethod,@DivisionCode,@UserID,@SerialNo,
				@DeliveryComment,@TanencyNo,@BusinessAreaNo,@StyleProfile,@CustomerName,@AmountDiscountAtPOS,@VATPercent,@IsTenancy,@SalesType,@SOURCEBILL,@HANDLINGSTAFF,@ReturnVoucherNo,@ReturnVoucherExpire)";
        }
        public static string Update_Zalo_WPH()
        {
            return @"Update Temp_Zalo_Survey set [UpdateFlg] = 1 where [RECEIPT_NO] = @RECEIPT_NO";
        }
        public static string UpdateWCM_Retry_Json()
        {
            return @" UPDATE[dbo].[DataRawJson_Retry]
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
        public static string InsertWCM()
        {
            return @" INSERT INTO [dbo].[DataRawJson]
           ([StoreNo],[PosNo],[OrderNo],[OrderDate],[Type],[BatchFile] ,[FileName] ,[DataJson]
           ,[IsRead] ,[Srv] ,[CrtDate] ,[ChgDate] ,[Id] ,[MemberCardNo]
           ,[TransactionType] ,[DiscountAmount],[VATAmount],[LineAmountIncVAT])
            VALUES (@StoreNo,@PosNo,@OrderNo,@OrderDate,@Type,@BatchFile,@FileName
           ,@DataJson,@IsRead,@Srv,@CrtDate,@CrtDate,@Id,@MemberCardNo,@TransactionType,@DiscountAmount,@VATAmount,@LineAmountIncVAT)";
        }
        public static string InsertWCM_BK()
        {
            return @" INSERT INTO [dbo].[DataRawJson_Retry]
           ([StoreNo],[PosNo],[OrderNo],[OrderDate],[Type],[BatchFile] ,[FileName] ,[DataJson]
           ,[IsRead] ,[Srv] ,[CrtDate] ,[ChgDate] ,[Id] ,[MemberCardNo]
           ,[TransactionType] ,[DiscountAmount],[VATAmount],[LineAmountIncVAT])
            VALUES (@StoreNo,@PosNo,@OrderNo,@OrderDate,@Type,@BatchFile,@FileName
           ,@DataJson,@IsRead,@Srv,@CrtDate,@CrtDate,@Id,@MemberCardNo,@TransactionType,@DiscountAmount,@VATAmount,@LineAmountIncVAT)";
        }
        public static string UpdateWCMVoid()
        {
            return @" UPDATE[dbo].[DataRawJson]
                        SET
                      [OrderNo] = @OrderNo
                      ,[IsRead] = @IsRead
                      ,[ChgDate] = @ChgDate
                       WHERE[Id] = @Id";
        }
        public static string UpdateWCM_Retry()
        {
            return @" UPDATE RETRY..Temp_SalesGCP_Retry
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
        //----------------Retry-WCM-------------------------------------
        public static string Transheader_Retry()
        {
            return @"SELECT OrderNo, OrderDate, CustomerNo, CustomerName, Address, PhoneNo, TablePhoneNo, HouseNo, CityNo, DistrictNo, WardNo, StreetNo, ShipToCityNo, ShipToDistrictNo, ShipToWardNo, ShipToStreetNo, DeliveryDate, DeliveryTimeFrom, DeliveryTimeTo, ZoneNo, ShipToAddress, DeliveryComment, BillToName, BillToAddress, VATRegistrationNo, StoreNo, POSTerminalNo, ShiftNo, CashierID, DiscountAmount, AmountExclVAT, VATAmount, AmountInclVAT, GeneralComment, UserID, PrepaymentAmount, DeliveringMethod, PaymentAtPOSAmount, InChangeAmount, OrderStatus, ShipToName, IsTenancy, InInstalments, ShipToHouseNo, ShipToPhoneNo, AmountDiscountAtPOS, IssuedVATInvoice, VoucherDiscountNo, VoucherDiscountValue, PointConversionRate, TanencyNo, StepProcess, SalesIsReturn, MemberCardNo, MemberPoint, ReturnedReceiptNo, ReturnedOrderNo, VATNumber, VATTemplateCode, VATSerial, TransactionType, EventNo, PrintedNumber, OrderTime, IsFullReturn, SendingStatus, BuyerName, BillNumber, StartingTime, EndingTime, RefKey1, RefKey2, Counter, IsOfflineVinID, IsAwardVinID, MemberPointsEarn, MemberPointsRedeem, IsExtraPoint, VinidCsn, CompanyCodeEmp, DocumentType, UserVoid, MemberPointsRedeemReturned, ReturnVoucherNo, ReturnVoucherExpire, CreatedDate, ID
            FROM TransHeader NOLOCK where OrderNo = @ReceiptNo";
        }
        public static string TransLine_Retry()
        {
            return @"SELECT [LineNo], DocumentNo, LineType, LocationCode, ItemNo, Description, UnitOfMeasure, Quantity, UnitPrice, OfferUnitPrice, AmountBeforeDiscount, DiscountPercent, DiscountAmount, VATPercent, VATAmount, LineAmountIncVAT, OfferNo, DiscountType, TriggerQuantity, StaffID, PrepaymentAmount, VATCode, DeliveringMethod, Barcode, OrderDiscountPercent, OrderDiscountAmount, DivisionCode, CategoryCode, ProductGroupCode, BlockedMemberPoint, SerialNo, OrigTransStore, OrigTransPos, OrigTransNo, OrigTransLineNo, OrigOrderNo, OrigLineNumber, BlockedPromotion, Infocodes, MemberPointsEarn, ReturnedQuantity, DeliveryQuantity, DeliveryStatus, MemberPointsRedeem, VariantNo, LotNo, ExpireDate, AmountCalPoint, WaitingListNo, ArticleType, ScanTime, DiscountCode, PromotionGroup, Counter, LineNoEffect, DiscountQuantity, DiscountUnit, SnGLineID, QtyDiscount, QtyDiscountReturned, GuiID, ID, CreatedDate 
                       FROM TransLine NOLOCK
                        where DocumentNo = @ReceiptNo";
        }
        public static string TransPayment_Retry()
        {
            return @"SELECT OrderNo, [LineNo], StoreNo, POSTerminalNo, TransactionNo, ReceiptNo, StatementCode, CardNo, ExchangeRate, TenderType, TenderTypeName, AmountTendered, CurrencyCode, AmountInCurrency, CardOrAccount, PaymentDate, PaymentTime, ShiftNo, ShiftDate, StaffID, CardPaymentType, CardValue, ReferenceNo, PayForOrderNo, Counter, ApprovalCode, BankPOSCode, BankCardType, IsOnline, ID, CreatedDate FROM TransPaymentEntry NOLOCK 
                       where OrderNo = @ReceiptNo";
        }
        public static string TransCp_Retry()
        {
            return @"SELECT OrderNo, OrderLineNo, [LineNo], OfferType, OfferNo, Quantity, DiscountType, DiscountAmount, Barcode, ParentLineNo, ItemNo, LineGroup, Counter, Company, IsOffline, ID, CreatedDate 
                    FROM TransDiscountCouponEntry NOLOCK where OrderNo = @ReceiptNo";
        }
        public static string Transdiscount_Retry()
        {
            return @"SELECT OrderNo, OrderLineNo, [LineNo], OfferType, OfferNo, Quantity, DiscountType, UnitPrice, DiscountAmount, Barcode, ParentLineNo, ItemNo, UOM, LineGroup, IsTotalBill, Counter, LineNoEffect, IsVinID, DiscountValue, GuiID, CusType, Ref1, Ref2, Ref3, Ref4, Ref5, ID, CreatedDate FROM TransDiscountEntry NOLOCK 
            where OrderNo = @ReceiptNo";
        }
        public static string TransInput_Retry()
        {
            return @"SELECT TransNo, LineNumber, TableName, DataType, DataValue, Counter, ID, CreatedDate FROM TransInputData NOLOCK 
            where [TransNo] = @ReceiptNo";
        }
        //----------------Retry-PLH-------------------------------------
        public static string Transheader_Retry_PL()
        {
            return @"SELECT [timestamp],[OrderNo],[OrderDate],[CustomerNo],[CustomerName],[Address],[PhoneNo],[TablePhoneNo],[HouseNo],[CityNo],[DistrictNo],[WardNo],[StreetNo],[ShipToCityNo],[ShipToDistrictNo],[ShipToWardNo],[ShipToStreetNo],[DeliveryDate],[DeliveryTimeFrom],[DeliveryTimeTo],[ZoneNo],[ShipToAddress],[DeliveryComment],[BillToName],[BillToAddress],[VATRegistrationNo],[StoreNo],[POSTerminalNo],[ShiftNo],[CashierID],[DiscountAmount],[AmountExclVAT],[VATAmount],[AmountInclVAT],[GeneralComment],[UserID],[PrepaymentAmount],[DeliveringMethod],[PaymentAtPOSAmount],[InChangeAmount],[OrderStatus],[ShipToName],[IsTenancy],[InInstalments],[ShipToHouseNo],[ShipToPhoneNo],[AmountDiscountAtPOS]
                    ,[IssuedVATInvoice],[VoucherDiscountNo],[VoucherDiscountValue],[PointConversionRate],[TanencyNo],[StepProcess],[SalesIsReturn],[MemberCardNo],[MemberPoint],[ReturnedReceiptNo],[ReturnedOrderNo],[VATNumber],[VATTemplateCode],[VATSerial],[TransactionType],[EventNo],[PrintedNumber],[OrderTime],[IsFullReturn],[SendingStatus],[BuyerName],[BillNumber],[StartingTime],[EndingTime],[RefKey1],[RefKey2],[Counter],[IsOfflineVinID],[IsAwardVinID],[MemberPointsEarn],[MemberPointsRedeem],[IsExtraPoint],[VinidCsn],[CompanyCodeEmp],[DocumentType]
                    ,[UserVoid],[MemberPointsRedeemReturned],[ReturnVoucherNo],[ReturnVoucherExpire],[RefKey3],[RefKey4],[RefKey5],[RefKey6],[RefKey7],[RefKey8],[RefKey9],[RefKey10]
                    ,[SalesType],[Label],[IsPrintedLabel],[MemberClub],[Note],[CreatedDate],[CardLevel]
                     FROM [TransHeader] NOLOCK where OrderNo = @ReceiptNo";
        }
        public static string TransLine_Retry_PL()
        {
            return @"SELECT  [timestamp],[LineNo],[DocumentNo],[LineType],[LocationCode],[ItemNo],[Description],[UnitOfMeasure],[Quantity],[UnitPrice],[OfferUnitPrice],[AmountBeforeDiscount],[DiscountPercent],[DiscountAmount],[VATPercent],[VATAmount],[LineAmountIncVAT],[OfferNo],[DiscountType],[TriggerQuantity],[StaffID],[PrepaymentAmount],[VATCode],[DeliveringMethod],[Barcode],[OrderDiscountPercent],[OrderDiscountAmount],[DivisionCode],[CategoryCode],[ProductGroupCode],[BlockedMemberPoint],[SerialNo],[OrigTransStore],[OrigTransPos],[OrigTransNo],[OrigTransLineNo],[OrigOrderNo],[OrigLineNumber],[BlockedPromotion],[Infocodes],[MemberPointsEarn],[ReturnedQuantity],[DeliveryQuantity],[DeliveryStatus],[MemberPointsRedeem],[VariantNo],[LotNo],[ExpireDate],[AmountCalPoint],[WaitingListNo],[ArticleType],[ScanTime],[DiscountCode]
                    ,[PromotionGroup],[Counter],[LineNoEffect],[DiscountQuantity],[DiscountUnit],[SnGLineID],[QtyDiscount],[QtyDiscountReturned],[Note],[IsTopping],[Group],[ParentCode],[GuiID],[IsCombo],[ComboName],[IsGift],[GiftGroup],[ComboQuantity],[IsGiftRedeem],[GiftRedeemPoint],[IsVAT],[IsFee],[IsChangeCardFee],[ChangeCardId],[ChangeCardCodeValue],[ChangeCardType],[ChangeCardOTP],[IsBirthdayGift],[IsAllowReturn]
                      FROM [TransLine]
                      where DocumentNo = @ReceiptNo";
        }
        public static string TransPayment_Retry_PL()
        {
            return @"SELECT [timestamp],[OrderNo],[LineNo],[StoreNo],[POSTerminalNo],[TransactionNo],[ReceiptNo],[StatementCode],[CardNo],[ExchangeRate],[TenderType],[TenderTypeName],[AmountTendered],[CurrencyCode],[AmountInCurrency],[CardOrAccount],[PaymentDate],[PaymentTime],[ShiftNo],[ShiftDate],[StaffID],[CardPaymentType],[CardValue],[ReferenceNo],[PayForOrderNo],[Counter],[ApprovalCode],[BankPOSCode],[BankCardType],[IsOnline],[IsCommission],[CommissionPercent],[Partner],[GiftRedeemPoint]
                    ,[OrderLineNo],[CustomerPaymentCode],[PaymentType],[TransactionId]
                      FROM [TransPaymentEntry]
                        where OrderNo = @ReceiptNo";
        }
        public static string TransCp_Retry_PL()
        {
            return @"SELECT [timestamp],[OrderNo],[OrderLineNo],[LineNo],[OfferType],[OfferNo],[Quantity],[DiscountType],[DiscountAmount],[Barcode],[ParentLineNo],[ItemNo],[LineGroup],[Counter],[IsCheckAPI],[IsTotalBill]
                      FROM [TransDiscountCouponEntry] NOLOCK 
                    where OrderNo = @ReceiptNo";
        }
        public static string Transdiscount_Retry_PL()
        {
            return @"SELECT [timestamp],[OrderNo],[OrderLineNo],[LineNo],[OfferType],[OfferNo],[Quantity],[DiscountType],[UnitPrice],[DiscountAmount],[Barcode],[ParentLineNo],[ItemNo],[UOM],[LineGroup],[IsTotalBill],[Counter],[LineNoEffect],[IsVinID],[DiscountValue],[GuiID],[IsConfirmed],[IsCondGet],[StepValue]
                          FROM [TransDiscountEntry] NOLOCK 
                          where OrderNo = @ReceiptNo";
        }
        public static string TransPoin_Retry_PL()
        {
            return @"SELECT [OrderNo],[LineNo],[OrderLineNo],[ClubCode],[MerchantId],[EarnPoints],[RedeemPoints],[MemberNumber],[RedeemPointsReturned],[ReceiptNumber],[OrigReceiptNumber],[ReferenceNumber],[OrigReferenceNumber],[MemberName],[CardLevel],[MemberCSN],[MemberPoint],[TotalPoint]
                         FROM [TransPointLine] NOLOCK 
                         where [OrderNo] = @ReceiptNo";
        }
        public static string Insert_Data_RetryProc_PLH()
        {
            return @"GET_ALL_SALEOUT_PLH";
        }
        public static string Insert_Data_Retry_PLH()
        {
            return @"SP_GET_RETRY_ORDER";
        }
        public static string TransHeader_PLH()
        {
            return @"Get_Hearder_OCC_GCP_PLH";
        }
        public static string PLH_WCM_Retry()
        {
            return @"Get_OCC_GCP_PLH_Retry";
        }
        public static string TransLine_PLH()
        {
            return @" SELECT  OrderNo,[LineNo],ParentLineNo,ItemNo,ItemName,Uom,Quantity,UnitPrice,DiscountAmount,VATPercent
	                    ,LineAmountIncVAT,MemberPointsEarn,MemberPointsRedeem,CupType,Size,IsTopping,IsCombo,ScanTime
                         FROM [INBOUND].[dbo].OCC_TransLine where OrderNo IN @OrderNo and ParentLineNo='0'";
        }
        public static string TransPayment_PLH()
        {
            return @" SELECT OrderNo,[LineNo],TenderType,CurrencyCode,ExchangeRate,[AmountTendered] PaymentAmount,ReferenceNo   
                 FROM [INBOUND].[dbo].OCC_TransPaymentEntry where OrderNo IN @OrderNo";
        }
        public static string CpnVchBOMHeader_PLH()
        {
            return @" SELECT [ItemNo],[ItemName] FROM [CentralMD].[dbo].[CpnVchBOMHeader]";
        }
        public static string TimeRunEinvoice ()
        {
            return @"SELECT [FileName]
                              ,[Type]
                              ,[TimeRun]
                              ,[Status]
                    FROM [TimeRunEinvoice] where Type in ('SAP','POS','SAP_CANCEL','POS_CANCEL')";
        }
    }
}
