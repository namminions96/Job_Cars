﻿using Azure;
using BluePosVoucher;
using BluePosVoucher.Data;
using Dapper;
using Job_By_SAP.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Job_By_SAP.PLH
{

    public class PLH_To_GCP_Retry
    {
        private readonly ILogger _logger;
        public PLH_To_GCP_Retry(ILogger logger)
        {
            _logger = logger;
        }
        IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
        public List<OrderExpToGCP> OrderExpToGCPAsyncArchive(string configPLH)
        {
            try
            {
                string Procedure = configuration["ProcedureArchive"];
                using (SqlConnection DBINBOUND = new SqlConnection(configPLH))
                {
                    DBINBOUND.Open();
                    var timeout = 600;
                    _logger.Information(Procedure);
                    var results = DBINBOUND.Query<OrderExpToGCP>(Procedure, commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                    DBINBOUND.Close();
                    _logger.Information($"Total Data results: {results.Count}");
                    if (results.Count > 0)
                    {
                        List<List<OrderExpToGCP>> orderBatches = results
                      .Select((order, index) => new { order, index })
                      .GroupBy(x => x.index / 1900)
                      .Select(group => group.Select(x => x.order).ToList())
                      .ToList();
                        List<OrderExpToGCP> listOrder = new List<OrderExpToGCP>();
                        foreach (var orderlist in orderBatches)
                        {
                            string connectionStringPLH = configuration["PLH_To_GCP_Archive"];
                            using (SqlConnection connection = new SqlConnection(connectionStringPLH))
                            {
                                List<string> results_order = orderlist.Select(p => p.OrderNo).ToList();
                                connection.Open();
                                var resultTransLine = connection.Query<TransLine_PLH_BLUEPOS>(PLH_Data.TransLineQuery(), new { documentNo = results_order }, commandTimeout: timeout).ToList();
                                var resultTransDiscountCoupon = connection.Query<TransDiscountCouponEntry_PLH_BLUEPOS>(PLH_Data.TransDiscountCouponEntryQuery(), new { orderNo = results_order }, commandTimeout: timeout).ToList();
                                var resultTransPayment = connection.Query<TransPaymentEntry_PLH_BLUEPOS>(PLH_Data.TransPaymentEntryQuery(), new { orderNo = results_order }, commandTimeout: timeout).ToList();
                                var resultTransDiscount = connection.Query<TransDiscountEntry_PLH_BLUEPOS>(PLH_Data.TransDiscountEntryQuery(), new { orderNo = results_order }, commandTimeout: timeout).ToList();
                                var resultTransPoin = connection.Query<TransPointEntry_PLH_BLUEPOS>(PLH_Data.TransPoinEntryQuery(), new { orderNo = results_order }).ToList();
                                connection.Close();
                                foreach (var order in orderlist)
                                {
                                    OrderExpToGCP orderExp = new OrderExpToGCP();
                                    orderExp.OrderNo = order.OrderNo;
                                    orderExp.OrderDate = order.OrderDate;
                                    orderExp.StoreNo = order.StoreNo;
                                    orderExp.PosNo = order.PosNo;
                                    orderExp.CustName = order.CustName;
                                    orderExp.Note = order.Note;
                                    orderExp.TransactionType = order.TransactionType;
                                    orderExp.SalesType = order.SalesType;
                                    orderExp.Note = order.Note;
                                    orderExp.OrderTime = order.OrderTime;
                                    orderExp.ReturnedOrderNo = order.ReturnedOrderNo;
                                    orderExp.Items = resultTransLine.Where(p => p.OrderNo == order.OrderNo).ToList();
                                    orderExp.CouponEntry = resultTransDiscountCoupon.Where(p => p.OrderNo == order.OrderNo).ToList();
                                    orderExp.Payments = resultTransPayment.Where(p => p.OrderNo == order.OrderNo).ToList();
                                    orderExp.DiscountEntry = resultTransDiscount.Where(p => p.OrderNo == order.OrderNo).ToList();
                                    orderExp.TransPointEntry = resultTransPoin.Where(p => p.OrderNo == order.OrderNo).ToList();
                                    listOrder.Add(orderExp);
                                }
                            }
                        }
                        return listOrder;
                    }
                    else
                    {
                        _logger.Information("Không có Data");
                        return new List<OrderExpToGCP>();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Lỗi :", e.Message);
                return new List<OrderExpToGCP>();
            }
        }
        public void InsertTempGCP(List<OrderExpToGCP> LstempSales, string status, string cfig)
        {
            try
            {
                string json = JsonConvert.SerializeObject(LstempSales);
                using (SqlConnection DBINBOUND = new SqlConnection(cfig))
                {
                    string currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    if (status == "True")
                    {

                        DBINBOUND.Open();
                        List<TempSalesGCP> tempSalesGCP = new List<TempSalesGCP>();
                        foreach (var order in LstempSales)
                        {
                            var tempSalesGCP_1 = new TempSalesGCP();
                            tempSalesGCP_1.SalesType = "PLH";
                            tempSalesGCP_1.OrderNo = order.OrderNo;
                            tempSalesGCP_1.OrderDate = order.OrderDate;
                            tempSalesGCP_1.CrtDate = DateTime.Now;
                            tempSalesGCP_1.Batch = currentDateTime;
                            tempSalesGCP.Add(tempSalesGCP_1);
                        }
                        int rowsAffected = DBINBOUND.Execute(PLH_Data.InsertTemp_SalesGCP(), tempSalesGCP);
                        //string filePathError = "data.text";
                        //File.WriteAllText(filePathError, json);
                        _logger.Information($"Insert {rowsAffected} row Thành công ");
                        _logger.Information("Dữ liệu đã được gửi thành công đến API.");

                    }
                    else
                    {
                        DBINBOUND.Open();
                        List<TempSalesGCP> tempSalesGCP = new List<TempSalesGCP>();
                        foreach (var order in LstempSales)
                        {
                            var tempSalesGCP_1 = new TempSalesGCP();
                            tempSalesGCP_1.SalesType = "PLH";
                            tempSalesGCP_1.OrderNo = order.OrderNo;
                            tempSalesGCP_1.OrderDate = order.OrderDate;
                            tempSalesGCP_1.CrtDate = DateTime.Now;
                            tempSalesGCP_1.Batch = currentDateTime;
                            tempSalesGCP.Add(tempSalesGCP_1);
                        }
                        int rowsAffected = DBINBOUND.Execute(PLH_Data.InsertTemp_SalesGCP(), tempSalesGCP);
                        _logger.Information($"Insert {rowsAffected} row Thành công ");
                        string filePathError = $"error_{currentDateTime}.text";
                        File.WriteAllText(filePathError, json);
                        _logger.Information("Gửi không thành công or chưa được phản hồi.");
                    }
                    DBINBOUND.Close();
                }
            }
            catch (Exception e)
            {
                _logger.Error("Lỗi :", e.Message);
            }
        }
    }
}
