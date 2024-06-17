﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.WCM
{
    public class WcmGCPModels_Json
    {
        public List<TransHeader> TransHeader { get; set; }
        public List<TransLineGCP> TransLine { get; set; }
        public List<TransPaymentEntryGCP> TransPaymentEntry { get; set; }
        public List<TransDiscountGCP> TransDiscountEntry { get; set; }
        public List<TransDiscountCouponEntryGCP> TransDiscountCouponEntry { get; set; }
    }
    public class TransHeader
    {
        public string CalendarDay { get; set; }
        public string StoreCode { get; set; }
        public string PosNo { get; set; }
        public string ReceiptNo { get; set; }
        public string TranTime { get; set; }
        public string MemberCardNo { get; set; }
        public string VinidCsn { get; set; }
        public string Header_ref_01 { get; set; }
        public string Header_ref_02 { get; set; }
        public string Header_ref_03 { get; set; }
        public string Header_ref_04 { get; set; }
        public string Header_ref_05 { get; set; }
    }

    public class WcmGCPModels
    {
        public string CalendarDay { get; set; }
        public string StoreCode { get; set; }
        public string PosNo { get; set; }
        public string ReceiptNo { get; set; }
        public string TranTime { get; set; }
        public string MemberCardNo { get; set; }
        public string VinidCsn { get; set; }
        public string Header_ref_01 { get; set; }
        public string Header_ref_02 { get; set; }
        public string Header_ref_03 { get; set; }
        public string Header_ref_04 { get; set; }
        public string Header_ref_05 { get; set; }
        public List<OrderInfo> OrderInfo { get; set; }
        public Boolean IsRetry { get; set; }
        public List<TransLineGCP> TransLine { get; set; }
        public List<TransPaymentEntryGCP> TransPaymentEntry { get; set; }
        public List<TransDiscountCouponEntryGCP> TransDiscountCouponEntry { get; set; }
    }

    public class OrderInfo
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class TransTempGCP_WCM
    {
        public string CalendarDay { get; set; }
        public string StoreCode { get; set; }
        public string PosNo { get; set; }
        public string ReceiptNo { get; set; }
        public string TranNo { get; set; }
        public string TranTime { get; set; }
        public string Barcode { get; set; }
        public string Article { get; set; }
        public string Uom { get; set; }
        public string Name { get; set; }
        public decimal POSQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal VATAmount { get; set; }
        public string Brand { get; set; }
        public string MemberCardNo { get; set; }
        public string VinidCsn { get; set; }
        public string Header_ref_01 { get; set; }
        public string Header_ref_02 { get; set; }
        public string Header_ref_03 { get; set; }
        public string Header_ref_04 { get; set; }
        public string Header_ref_05 { get; set; }
        public string Source { get; set; }
        public Boolean IsRetry { get; set; }

    }
    public class TransLineGCP
    {
        public int TranNo { get; set; }
        public string Barcode { get; set; }
        public string Article { get; set; }
        public string Uom { get; set; }
        public string Name { get; set; }
        public decimal POSQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal VATAmount { get; set; }
        public string Brand { get; set; }
        public string SerialNo { get; set; }
        public List<TransDiscountGCP> DiscountEntry { get; set; }
    }
    public class TransPaymentEntryGCP
    {
        public string ReceiptNo { get; set; }
        public int LineNo { get; set; }
        public string TenderType { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal AmountTendered { get; set; }
        public decimal AmountInCurrency { get; set; }
        public string ApprovalCode { get; set; }
        public string ReferenceNo { get; set; }
        public string BankPOSCode { get; set; }
        public string BankCardType { get; set; }
        public bool IsOnline { get; set; }
    }
    public class TransDiscountGCP
    {
        public string ReceiptNo { get; set; }
        public int LineNo { get; set; }
        public int TranNo { get; set; }
        public string ItemNo { get; set; }
        public string UOM { get; set; }
        public string OfferType { get; set; }
        public string OfferNo { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountAmount { get; set; }
    }
    public class TransDiscountCouponEntryGCP
    {
        public string OrderNo { get; set; }
        public int ParentLineId { get; set; }
        public int LineId { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public string Barcode { get; set; }
        public decimal DiscountAmount { get; set; }
    }
    public class TransInputDataGCP
    {
        public string TransNo { get; set; }
        public string LineNumber { get; set; }
        public string TableName { get; set; }
        public string DataType { get; set; }
        public string DataValue { get; set; }
    }

    public class CpnVchBOMHeader
    {
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
    }
    public class SP_Data_WCM
    {
        //[BsonId]
        //public ObjectId Id { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string DataJson { get; set; }
        public bool IsRead { get; set; }
        public DateTime ChgDate  { get; set; }
        public Guid ID { get; set; }
        public string MemberCardNo { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal LineAmountIncVAT { get; set; }
    }
    public class SP_Data_WCM_Insert
    {
        //[BsonId]
        //public ObjectId Id { get; set; }
        public string StoreNo { get; set; }
        public string PosNo { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string Type { get; set; }
        public string BatchFile { get; set; }
        public string FileName { get; set; }
        public string DataJson { get; set; }
        public bool IsRead { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgDate { get; set; }
        public Guid ID { get; set; }
        public string MemberCardNo { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal LineAmountIncVAT { get; set; }
    }

}
