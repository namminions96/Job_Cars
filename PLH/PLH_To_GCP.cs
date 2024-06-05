using Dapper;
using Job_By_SAP.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using System.Globalization;

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
                        var resultTransPoint = connection.Query<TransPointEntry_PLH_BLUEPOS>(PLH_Data.TransPoinEntryQuery(), new { orderNo = results_order }).ToList();
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
                            orderExp.TransPointEntry = resultTransPoint.Where(p => p.OrderNo == order.OrderNo).ToList();
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
        public void InsertTempGCP(List<OrderExpToGCP> LstempSales, string status, string cfig)
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

        public void UpdateStausPLH_WCM(List<TransHeader_Temp> transHeader_Temps, string status, string cfig)
        {
            try
            {
                using (SqlConnection DBINBOUND = new SqlConnection(cfig))
                {
                    if (status == "True")
                    {
                        int rowsAffected = 0;
                        DBINBOUND.Open();
                        foreach (var temp in transHeader_Temps)
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = DBINBOUND;
                                command.CommandText = PLH_Data.InsertOCC_Temp();
                                command.Parameters.AddWithValue("@SubSet", temp.StoreNo);
                                command.Parameters.AddWithValue("@MainCode", temp.OrderNo);
                                command.Parameters.AddWithValue("@CrtDate", DateTime.Now);
                                int rowAfect = command.ExecuteNonQuery();
                                rowsAffected++;
                            }
                        }
                        DBINBOUND.Close();
                        _logger.Information($"Update {rowsAffected} row Thành công ");
                    }

                }
            }
            catch (Exception e)
            {
                _logger.Error("Fail: .", e.Message);
            }
        }
        public List<TransHeader_PLH_WCM> OrderToGCPAsync_PLHWCM(string configPLH, List<TransHeader_Temp> transHeader_Temps)
        {
            using (SqlConnection connection = new SqlConnection(configPLH))
            {
                List<string> results_order = transHeader_Temps.Select(p => p.OrderNo).ToList();
                var parameters = new { OrderNo = results_order };
                connection.Open();
                var resultTransLine = connection.Query<TransLine_PLH_WCM_TEMP>(WCM_Data.TransLine_PLH(), parameters).ToList();
                var resultTransPayment = connection.Query<TransPaymentEntry_PLH_WCM_TEMP>(WCM_Data.TransPayment_PLH(), parameters).ToList();
                connection.Close();
                List<TransHeader_PLH_WCM> listOrder = new List<TransHeader_PLH_WCM>();
                foreach (var order in transHeader_Temps)
                {
                    TransHeader_PLH_WCM orderExp = new TransHeader_PLH_WCM();
                    orderExp.OrderNo = order.OrderNo;
                    orderExp.OrderDate = order.OrderDate.ToString("yyyy-MM-dd"); 
                    orderExp.StoreNo = order.StoreNo;
                    orderExp.SaleType = order.SaleType;
                    orderExp.TransactionType = order.TransactionType;
                    orderExp.MemberCardNo = order.MemberCardNo;
                    orderExp.SalesStoreNo = order.SalesStoreNo;
                    orderExp.SalesPosNo = order.SalesPosNo;
                    orderExp.RefNo = order.RefKey;
                    var Transline = resultTransLine.Where(p => p.OrderNo == order.OrderNo).ToList();
                    List<TransLine_PLH_WCM> transLine_s = new List<TransLine_PLH_WCM>();
                    foreach (var item in Transline)
                    {
                        TransLine_PLH_WCM transLine_PLH_WCM_V = new TransLine_PLH_WCM();
                        transLine_PLH_WCM_V.LineNo = item.LineNo;
                        transLine_PLH_WCM_V.ParentLineNo = item.ParentLineNo;
                        transLine_PLH_WCM_V.ItemNo = item.ItemNo;
                        transLine_PLH_WCM_V.ItemName = item.ItemName;
                        transLine_PLH_WCM_V.Quantity = item.Quantity;
                        transLine_PLH_WCM_V.Uom = item.Uom;
                        transLine_PLH_WCM_V.UnitPrice = item.UnitPrice;
                        transLine_PLH_WCM_V.DiscountAmount = item.DiscountAmount;
                        transLine_PLH_WCM_V.VATPercent = item.VATPercent;
                        transLine_PLH_WCM_V.LineAmount = item.LineAmountIncVAT;
                        transLine_PLH_WCM_V.MemberPointsEarn = item.MemberPointsEarn;
                        transLine_PLH_WCM_V.MemberPointsRedeem = item.MemberPointsRedeem;
                        transLine_PLH_WCM_V.CupType = item.CupType;
                        transLine_PLH_WCM_V.Size = item.Size;
                        transLine_PLH_WCM_V.IsTopping = item.IsTopping;
                        transLine_PLH_WCM_V.IsCombo = item.IsCombo;
                        transLine_PLH_WCM_V.ScanTime = item.ScanTime;
                        transLine_s.Add(transLine_PLH_WCM_V);
                    }
                    orderExp.TransLine = transLine_s;
                    List<TransPaymentEntry_PLH_WCM> transpayments = new List<TransPaymentEntry_PLH_WCM>();
                    var transpayment = resultTransPayment.Where(p => p.OrderNo == order.OrderNo).ToList();
                    foreach (var item in transpayment)
                    {
                        TransPaymentEntry_PLH_WCM transPaymentEntry_PLH_WCM = new TransPaymentEntry_PLH_WCM();
                        transPaymentEntry_PLH_WCM.LineNo = item.LineNo;
                        transPaymentEntry_PLH_WCM.TenderType = item.TenderType;
                        transPaymentEntry_PLH_WCM.CurrencyCode = item.CurrencyCode;
                        transPaymentEntry_PLH_WCM.ExchangeRate = item.ExchangeRate;
                        transPaymentEntry_PLH_WCM.PaymentAmount = item.PaymentAmount;
                        transPaymentEntry_PLH_WCM.ReferenceNo = item.ReferenceNo;
                        transpayments.Add(transPaymentEntry_PLH_WCM);
                    }
                    orderExp.TransPaymentEntry = transpayments;
                    listOrder.Add(orderExp);
                }
                return listOrder;
            }
        }
    }
}

