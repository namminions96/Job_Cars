﻿using Azure;
using BluePosVoucher;
using BluePosVoucher.Data;
using Dapper;
using Job_By_SAP.Models;
using Job_By_SAP.PLH;
using Job_By_SAP.WCM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Read_xml.Data;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Job_By_SAP
{

    public class WCM_To_GCP
    {
        private readonly ILogger _logger;
        public WCM_To_GCP(ILogger logger)
        {
            _logger = logger;
        }
        IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
        public List<WcmGCPModels> OrderWcmToGCPAsync_Fix(string configWcm, List<string> reciept, List<TransTempGCP_WCM> results)
        {
            var timeout = 600;
            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                try
                {
                    DbsetWcm.Open();
                    if (reciept.Count > 0)
                    {
                        var TransDiscountGCP = DbsetWcm.Query<TransDiscountGCP>(WCM_Data.SUMD11_DISCOUNT_BLUE_New(), new { ReceiptNo = reciept }, commandTimeout: timeout).ToList();
                        var TransPaymentEntryGCP = DbsetWcm.Query<TransPaymentEntryGCP>(WCM_Data.SUMD11_PAYMENT_New(), new { ReceiptNo = reciept }, commandTimeout: timeout).ToList();
                        var TransDiscountCouponEntryGCP = DbsetWcm.Query<TransDiscountCouponEntryGCP>(WCM_Data.SUMD11_Coupon_New(), new { ReceiptNo = reciept }, commandTimeout: timeout).ToList();
                        var concurrentBag = new ConcurrentBag<WcmGCPModels>();
                        try
                        {
                            Parallel.ForEach(results, order =>
                            {
                                List<TransLineGCP> Transline = new List<TransLineGCP>();
                                TransLineGCP transLine = new TransLineGCP();
                                transLine.Barcode = order.Barcode;
                                transLine.TranNo = int.Parse(order.TranNo);
                                transLine.Article = order.Article;
                                transLine.Uom = order.Uom;
                                transLine.Name = order.Name;
                                transLine.POSQuantity = order.POSQuantity;
                                transLine.Price = order.Price;
                                transLine.Amount = order.Amount;
                                transLine.Brand = order.Brand;
                                transLine.DiscountEntry = TransDiscountGCP.Where(p => p.ReceiptNo == order.ReceiptNo && p.ItemNo == order.Article).ToList();
                                Transline.Add(transLine);
                                WcmGCPModels orderExp = new WcmGCPModels();
                                orderExp.CalendarDay = order.CalendarDay;
                                orderExp.StoreCode = order.StoreCode;
                                orderExp.PosNo = order.PosNo;
                                orderExp.ReceiptNo = order.ReceiptNo;
                                orderExp.TranTime = order.TranTime;
                                orderExp.MemberCardNo = order.MemberCardNo;
                                orderExp.VinidCsn = order.VinidCsn;
                                orderExp.Header_ref_01 = order.Header_ref_01;
                                orderExp.Header_ref_02 = order.Header_ref_02;
                                orderExp.Header_ref_03 = order.Header_ref_03;
                                orderExp.Header_ref_04 = order.Header_ref_04;
                                orderExp.Header_ref_05 = order.Header_ref_05;
                                orderExp.IsRetry = order.IsRetry;
                                orderExp.TransLine = Transline;
                                orderExp.TransPaymentEntry = TransPaymentEntryGCP.Where(p => p.ReceiptNo == order.ReceiptNo).ToList();
                                orderExp.TransDiscountCouponEntry = TransDiscountCouponEntryGCP.Where(p => p.OrderNo == order.ReceiptNo && p.ParentLineId.ToString() == order.TranNo).ToList();
                                concurrentBag.Add(orderExp);
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                        finally
                        {
                            DbsetWcm.Close();
                        }
                        return concurrentBag.ToList();
                    }
                    else
                    {
                        _logger.Information("Không có Data");
                        return new List<WcmGCPModels>();
                    }
                }
                catch (Exception e)
                {
                    _logger.Information("Không có Data" + e.Message);
                    return new List<WcmGCPModels>();
                }
            }
        }

        public List<WcmGCPModels> OrderWcmToGCPAsync_Json(string configWcm, List<SP_Data_WCM> reciept)
        {
            ReadDataRawJson readDataRawJson = new ReadDataRawJson();
            var timeout = 600;
            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                try
                {
                    DbsetWcm.Open();
                    List<SP_Data_WCM> SP_Data_WCMs = new List<SP_Data_WCM>();
                    var concurrentBag = new ConcurrentBag<WcmGCPModels>();
                    foreach (var result in reciept)
                    {
                        JObject jsonObject = JObject.Parse(result.DataJson);
                        //TransInputDataGCP//
                        JArray TransInputData = (JArray)jsonObject["Data"]["TransInputData"];
                        List<TransInputDataGCP> TransInputDataResult = readDataRawJson.TransInputDataGCP(TransInputData);
                        //TransDiscountCouponEntry//
                        JArray TransDiscountCouponEntry = (JArray)jsonObject["Data"]["TransDiscountCouponEntry"];
                        List<TransDiscountCouponEntryGCP> CouponEntryResult = readDataRawJson.TransDiscountCouponEntryGCP(TransDiscountCouponEntry);
                        //payment//
                        JArray TransPaymentEntry = (JArray)jsonObject["Data"]["TransPaymentEntry"];
                        List<TransPaymentEntryGCP> PaymentEntryResult = readDataRawJson.TransPaymentEntryGCP(TransPaymentEntry);
                        //TransDiscountEntry//
                        JArray TransDiscountEntry = (JArray)jsonObject["Data"]["TransDiscountEntry"];
                        List<TransDiscountGCP> DiscountEntryResult = readDataRawJson.TransDiscountGCP(TransDiscountEntry);
                        //TransLine///
                        JArray TransLine = (JArray)jsonObject["Data"]["TransLine"];
                        List<TransLineGCP> TransLineResult = new List<TransLineGCP>();
                        DateTime ScanTime = new DateTime();
                        foreach (JObject Item in TransLine)
                        {
                            TransLineGCP TransLines = new TransLineGCP();
                            int Linetype = (int)Item["LineType"];
                            TransLines.TranNo = (int)Item["LineNo"];
                            TransLines.Barcode = (string)Item["Barcode"];
                            TransLines.Article = (string)Item["ItemNo"];
                            TransLines.Uom = (string)Item["UnitOfMeasure"];
                            TransLines.Name = (string)Item["Description"];
                            TransLines.POSQuantity = (decimal)Item["Quantity"];
                            if (Item.TryGetValue("UnitPrice", out var unitPriceValue) && unitPriceValue.Type != JTokenType.Null)
                            {
                                TransLines.Price = (decimal)unitPriceValue;
                            }
                            if (Item.TryGetValue("LineAmountIncVAT", out var lineAmountValue) && lineAmountValue.Type != JTokenType.Null)
                            {
                                TransLines.Amount = (decimal)lineAmountValue;
                            }
                            TransLines.Brand = (string)Item["DivisionCode"];
                            TransLines.DiscountEntry = DiscountEntryResult.Where(p => p.ItemNo == TransLines.Article && p.TranNo == TransLines.TranNo).ToList();

                            if (Item.TryGetValue("ScanTime", out var scanTimeValue) && scanTimeValue.Type != JTokenType.Null)
                            {
                                if (DateTime.TryParse(scanTimeValue.ToString(), out DateTime scanTime))
                                {
                                    ScanTime = scanTime;
                                }
                            }
                            if (Linetype == 0)
                            {
                                TransLineResult.Add(TransLines);
                            }
                        }
                        //TransHeader//
                        var SOURCEBILL = TransInputDataResult.FirstOrDefault(P => P.DataType == "SOURCEBILL");
                        var CUSTYPE = TransInputDataResult.FirstOrDefault(P => P.DataType == "CUSTYPE");
                        List<WcmGCPModels> TransHeaderss = new List<WcmGCPModels>();
                        JArray TransHeader = (JArray)jsonObject["Data"]["TransHeader"];
                        foreach (JObject headerItem in TransHeader)
                        {
                            WcmGCPModels TransHeaders = new WcmGCPModels();
                            TransHeaders.CalendarDay = (string)headerItem["OrderDate"];
                            TransHeaders.StoreCode = (string)headerItem["StoreNo"];
                            TransHeaders.PosNo = (string)headerItem["POSTerminalNo"];
                            TransHeaders.ReceiptNo = (string)headerItem["OrderNo"];
                            TransHeaders.TranTime = ScanTime.ToString("HHmmssfff");
                            TransHeaders.MemberCardNo = (string)headerItem["MemberCardNo"];
                            TransHeaders.VinidCsn = (string)headerItem["VinidCsn"];
                            TransHeaders.Header_ref_01 = (string)headerItem["OrigOrderNo"];
                            TransHeaders.Header_ref_02 = SOURCEBILL?.DataType;
                            TransHeaders.Header_ref_03 = (string)headerItem["RefKey1"];
                            TransHeaders.Header_ref_04 = CUSTYPE?.DataType;
                            string zoneNo = (string)headerItem["ZoneNo"];
                            if (zoneNo == "TCBTOPUP" || zoneNo == "TCBWITHDRAW")
                            {
                                TransHeaders.Header_ref_05 = "NO_CAP";
                            }
                            else
                            {
                                TransHeaders.Header_ref_05 = zoneNo;
                            }
                            TransHeaders.IsRetry = false;
                            TransHeaders.TransLine = TransLineResult.ToList();
                            TransHeaders.TransPaymentEntry = PaymentEntryResult.ToList();
                            TransHeaders.TransDiscountCouponEntry = CouponEntryResult.Where
                            (coupon => TransLineResult.Any(transLine => transLine.TranNo == coupon.ParentLineId)).ToList();
                            concurrentBag.Add(TransHeaders);
                            SP_Data_WCM SP_Data_WCMss = new SP_Data_WCM();
                            SP_Data_WCMss.ID = result.ID;
                            SP_Data_WCMss.OrderNo = TransHeaders.ReceiptNo;
                            DateTime parsedDate;
                            if (DateTime.TryParseExact(TransHeaders.CalendarDay, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                            {
                                SP_Data_WCMss.OrderDate = parsedDate;
                            }
                            SP_Data_WCMss.ChgDate = DateTime.Now;
                            SP_Data_WCMss.IsRead = true;
                            SP_Data_WCMss.DataJson = TransHeaders.ReceiptNo;
                            SP_Data_WCMs.Add(SP_Data_WCMss);
                        }
                    }
                    readDataRawJson.UpdateStatusWCM(SP_Data_WCMs, configWcm);
                    return concurrentBag.ToList();
                }
                catch (Exception e)
                {
                    _logger.Information("Không có Data" + e.Message);
                    return new List<WcmGCPModels>();
                }
            }
        }
    }

}

