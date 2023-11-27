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
using Read_xml.Data;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public List<WcmGCPModels> OrderWcmToGCPAsync(string configWcm)
        {
            var timeout = 300;
            //DateTime currentDateTime = DateTime.Now;
           // string dateTimeString = "EXP - DATA - SET"+currentDateTime.ToString("yyyyMMddHHmmss");
            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                try
                {

                    DbsetWcm.Open();
                    var results = DbsetWcm.Query<TransTempGCP_WCM>(WCM_Data.Procedure_SaleOut(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                    //var results = DbsetWcm.Query<TransTempGCP_WCM>(WCM_Data.Procedure_SaleOut(), param: dateTimeString, commandType: CommandType.StoredProcedure).ToList();
                    _logger.Information($"Results Data {results.Count} ! ");
                    var groupedResults = results.GroupBy(p => p.ReceiptNo).ToList();

                    if (results.Count > 0)
                    {
                        var TransDiscountGCP = DbsetWcm.Query<TransDiscountGCP>(WCM_Data.SUMD11_DISCOUNT_BLUE(), commandTimeout: timeout);
                        var TransPaymentEntryGCP = DbsetWcm.Query<TransPaymentEntryGCP>(WCM_Data.SUMD11_PAYMENT(), commandTimeout: timeout);
                        List<WcmGCPModels> listOrder = new List<WcmGCPModels>();
                        foreach (var order in results)
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
                            orderExp.TransLine = Transline;
                            orderExp.TransPaymentEntry = TransPaymentEntryGCP.Where(p => p.ReceiptNo == order.ReceiptNo).ToList();
                            listOrder.Add(orderExp);
                        }
                        DbsetWcm.Close();
                        return listOrder;
                    }
                    else
                    {
                        _logger.Information("Không có Data");
                        return new List<WcmGCPModels>();
                    }
                }
                catch (Exception e)
                {
                    _logger.Information("Không có Data"+ e.Message);
                    return new List<WcmGCPModels>();
                }
            }
        }
        public List<WcmGCPModels> OrderWcmToGCPAsync_Fix(string configWcm ,List<string> reciept ,List<TransTempGCP_WCM> results)
        {
            var timeout = 600;
            using (SqlConnection DbsetWcm = new SqlConnection(configWcm))
            {
                try
                {
                    DbsetWcm.Open();
                    if (reciept.Count > 0)
                    {
                        var TransDiscountGCP = DbsetWcm.Query<TransDiscountGCP>(WCM_Data.SUMD11_DISCOUNT_BLUE_New(), new { ReceiptNo = reciept}, commandTimeout: timeout).ToList();
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
                                orderExp.TransDiscountCouponEntry = TransDiscountCouponEntryGCP.Where(p => p.OrderNo == order.ReceiptNo && p.ParentLineId.ToString() ==order.TranNo).ToList();
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
    }
}
