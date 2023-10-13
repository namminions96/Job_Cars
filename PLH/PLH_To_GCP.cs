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

    public class PLH_To_GCP
    {
        private readonly ILogger _logger;
        public PLH_To_GCP(ILogger logger)
        {
            _logger = logger;
        }
        IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
        public List<OrderExpToGCP> OrderExpToGCPAsync()
        {
            string INBOUND = configuration["INBOUND"];
            string Procedure = configuration["Procedure"];
            _logger.Information(INBOUND);
            using (SqlConnection DBINBOUND = new SqlConnection(INBOUND))
            {
                DBINBOUND.Open();
                _logger.Information(Procedure);
                var results = DBINBOUND.Query<OrderExpToGCP>(Procedure, commandType: CommandType.StoredProcedure).ToList();
                DBINBOUND.Close();
                if (results.Count > 0)
                {
                    string connectionStringPLH = configuration["PLH_To_GCP"];
                    _logger.Information(connectionStringPLH);
                    using (SqlConnection connection = new SqlConnection(connectionStringPLH))
                    {
                        List<string> results_order = results.Select(p => p.OrderNo).ToList();
                        connection.Open();
                        var timeout = 600;
                        var resultTransLine = connection.Query<TransLine_PLH_BLUEPOS>(PLH_Data.TransLineQuery(), new { documentNo = results_order }).ToList();
                        var resultTransDiscountCoupon = connection.Query<TransDiscountCouponEntry_PLH_BLUEPOS>(PLH_Data.TransDiscountCouponEntryQuery(), new { orderNo = results_order }).ToList();
                        var resultTransPayment = connection.Query<TransPaymentEntry_PLH_BLUEPOS>(PLH_Data.TransPaymentEntryQuery(), new { orderNo = results_order }).ToList();
                        var resultTransDiscount = connection.Query<TransDiscountEntry_PLH_BLUEPOS>(PLH_Data.TransDiscountEntryQuery(), new { orderNo = results_order }).ToList();
                        connection.Close();
                        List<OrderExpToGCP> listOrder = new List<OrderExpToGCP>();
                        foreach (var order in results)
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
                            listOrder.Add(orderExp);
                        }
                        return listOrder;
                        //_logger.Information("Send Data To API");
                        //string apiUrl = "http://10.235.19.71:50001/pos-plg/sale-out";
                        //using (HttpClient httpClient = new HttpClient())
                        //{
                        //    try
                        //    {
                        //        using (var db = new DBINBOUND())
                        //        {
                        //            string json = JsonConvert.SerializeObject(listOrder);
                        //            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                        //            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        //            //request.Content = content;
                        //            //HttpResponseMessage response = await httpClient.SendAsync(request);
                        //            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                        //            if (response.IsSuccessStatusCode)
                        //            {
                        //                TempSalesGCP tempSalesGCP = new TempSalesGCP();
                        //                foreach (var order in results)
                        //                {
                        //                    tempSalesGCP.OrderNo = order.OrderNo;
                        //                    tempSalesGCP.OrderDate = order.OrderDate;
                        //                    tempSalesGCP.CrtDate = DateTime.Now;
                        //                    tempSalesGCP.SalesType = "PLH";
                        //                    tempSalesGCP.Batch = DateTime.Now.ToString();
                        //                    db.Add(tempSalesGCP);
                        //                    db.SaveChanges();
                        //                }
                        //                _logger.Information("Dữ liệu đã được gửi thành công đến API.");
                        //            }
                        //            else
                        //            {
                        //                TempSalesGCP tempSalesGCP = new TempSalesGCP();
                        //                foreach (var order in results)
                        //                {
                        //                    tempSalesGCP.OrderNo = order.OrderNo;
                        //                    tempSalesGCP.OrderDate = order.OrderDate;
                        //                    tempSalesGCP.CrtDate = DateTime.Now;
                        //                    tempSalesGCP.SalesType = "PLH";
                        //                    tempSalesGCP.Batch = DateTime.Now.ToString();
                        //                    db.Add(tempSalesGCP);
                        //                    db.SaveChanges();
                        //                }
                        //                string filePath = "Error\\data.text";
                        //                if (Directory.Exists(filePath))
                        //                {
                        //                    File.WriteAllText(filePath, json);
                        //                }
                        //                else
                        //                {
                        //                    Directory.CreateDirectory(filePath);
                        //                    File.WriteAllText(filePath, json);
                        //                }
                        //                _logger.Information("Gửi không thành công or chưa được phản hồi.");
                        //            }
                        //        }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        _logger.Information("Lỗi: " + ex.Message);
                        //    }
                        //}
                    }

                }
                else
                {
                    _logger.Information("Không có Data");
                    return new List<OrderExpToGCP>();
                }
            }
        }
    }
}
