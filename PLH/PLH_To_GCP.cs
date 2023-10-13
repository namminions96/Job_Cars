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
        public List<OrderExpToGCP> OrderExpToGCPAsync(string configPLH)
        {
            string Procedure = configuration["Procedure"];
            using (SqlConnection DBINBOUND = new SqlConnection(configPLH))
            {
                DBINBOUND.Open();
                _logger.Information(Procedure);
                var results = DBINBOUND.Query<OrderExpToGCP>(Procedure, commandType: CommandType.StoredProcedure).ToList();
                DBINBOUND.Close();
                _logger.Information($"Total Data results: {results.Count}");
                if (results.Count > 0)
                {
                    string connectionStringPLH = configuration["PLH_To_GCP"];
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
