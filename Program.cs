using BluePosVoucher;
using BluePosVoucher.Data;
using BluePosVoucher.Models;
using Dapper;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
using Job_By_SAP;
using Job_By_SAP.Models;
using Job_By_SAP.MongoDB;
using Job_By_SAP.PLH;
using Job_By_SAP.SAP;
using Job_By_SAP.WCM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Read_xml;
using Read_xml.Data;
using Serilog;
using StackExchange.Redis;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using static Job_By_SAP.Models.SalesGCP_Retry;
internal class Program
{
    private static IConfiguration _configuration;
    private static ILogger _logger;
    private static ILogger _logger_WCM;
    private static ILogger _logger_VINID;
    private static ILogger _logger_PLH;
    private static ILogger _logger_HR;
    private static ILogger _logger_Job;
    private static ILogger _logger_VC;
    private static ILogger _logger_Einvoice;
    private static ILogger _logger_DeleteFile;
    private static ILogger _logger_WCM_Void;
    private static ILogger _logger__WPH_Survey;
    private static ILogger _logger_PLH_WF;
    private static async Task Main(string[] args)
    {
        _logger = SerilogLogger.GetLogger();
        _logger_WCM = SerilogLogger.GetLogger_WCM();
        _logger_VINID = SerilogLogger.GetLogger_VinID();
        _logger_PLH = SerilogLogger.GetLogger_PLH();
        _logger_HR = SerilogLogger.GetLogger_HR();
        _logger_Job = SerilogLogger.GetLogger_Job();
        _logger_VC = SerilogLogger.GetLogger_VC();
        _logger_Einvoice = SerilogLogger.GetLogger_Einvoice();
        _logger_DeleteFile = SerilogLogger.GetLogger_DeleteFile();
        _logger_WCM_Void = SerilogLogger.GetLogger_WCM_Void();
        _logger__WPH_Survey = SerilogLogger.GetLogger_WPH_Survey();
        _logger_PLH_WF = SerilogLogger.GetLogger_PLH_WF();
        SendEmailExample sendEmailExample = new SendEmailExample(_logger);
        InbVoucherSap inbVoucherSap1 = new InbVoucherSap(_logger_VC);
        ReadFile readfilSAP = new ReadFile(_logger);
        SendLogger sendLogger = new SendLogger();
        Stopwatch stopwatch = new Stopwatch();
        IConfiguration configuration = new ConfigurationBuilder()
       .SetBasePath(AppContext.BaseDirectory)
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .Build();

        //string apiUrllog = "http://api-bluepos.masan.local/api/common/logging";
        using (var db = new DbConfigAll())
        {
            var configKibana = db.Configs.SingleOrDefault(p => p.Type == "Kibana" && p.Status == true);//config DB
            if (configKibana == null)
            {
                _logger.Information("Config Send Kibana Chưa Khai báo or đang off (91).");
                //sendEmailExample.SendMailError("Config Send Kibana Chưa Khai báo or đang off (91).");
            }
            string apiUrllog = configKibana?.IpSftp ?? "Kibana";
            string usernameapi = configKibana?.username ?? "Kibana";
            string passwordapi = configKibana?.password ?? "Kibana";
            string authInfo = $"{usernameapi}:{passwordapi}";
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            //---------------------------------------------------------------------------------
            string functionName = args[0];
            string Name = args[1];
            //string Name = "WCM_GCP_NEW";
            //////// string Name = "PLH_GCP_Retry_New";
            //string Name = "PLH_INBOUND";
            //string functionName = "PLH_WCM_Sale";
            //string functionName = "GCP_WCM_Json";
            ////string functionName = "GCP_WCM_Retry_Old";
            //string Name = "WCM_GCP_NEW";
            //string functionName = "GCP_WCM_NEW_WINPHAR";
            if (args.Length > 0)
            {
                switch (functionName)
                {
                    case "VoucherSAP":
                        _logger_VC.Information("--------------------------VoucherSAP----------------------------");
                        try
                        {
                            //var connections = db.ConfigConnections.ToList().Where(p => p.Type == "VC" && p.Status == true);
                            var connectionsVC = db.ConfigConnections.ToList().Where(p => p.Type == "VC" && p.Status == true);
                            // --------------------------------Voucher SAP---------------------------------------------------------- 
                            foreach (var connectionString in connectionsVC)
                            {
                                _logger_VC.Information("Connect DB : " + connectionString.Name);
                                try
                                {
                                    string connect = connectionString.ConnectString;
                                    using (SqlConnection connection = new SqlConnection(connect))
                                    {
                                        connection.Open();
                                        var timeout = 300;
                                        var parameters = new
                                        {
                                            Status = "",
                                            Date = "",
                                            Retry = 0
                                        };
                                        var results = connection.Query("INB_Voucher ", parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        if (results.Count == 0)
                                        {
                                            _logger_VC.Information($"{connectionString.Name} : Không có Data ");
                                        }
                                        else
                                        {
                                            Inb_Voucher inb_Voucher = new Inb_Voucher();
                                            foreach (var result in results)
                                            {
                                                inb_Voucher.Voucher_Type = result.Voucher_Type;
                                                inb_Voucher.SerialNo = result.SerialNo;
                                                inb_Voucher.Voucher_Value = result.Voucher_Value;
                                                inb_Voucher.Voucher_Currency = result.Voucher_Currency;
                                                inb_Voucher.Validity_From_Date = result.Validity_From_Date;
                                                inb_Voucher.Expiry_Date = result.Expiry_Date;
                                                inb_Voucher.Processing_Type = result.Processing_Type;
                                                inb_Voucher.Status = result.Status;
                                                inb_Voucher.Site = result.Site;
                                                inb_Voucher.Article_No = result.Article_No;
                                                inb_Voucher.Bonus_Buy = result.Bonus_Buy;
                                                inb_Voucher.POSNo = result.POSNo;
                                                inb_Voucher.ReceiptNo = result.ReceiptNo;
                                                inb_Voucher.TranDate = result.TranDate;
                                                inb_Voucher.TranTime = result.TranTime;
                                                string POSTerminal = inb_Voucher.ReceiptNo.Substring(0, 6);
                                                if (inb_Voucher.Status == "EXP")
                                                {
                                                    var calResult = await inbVoucherSap1.CallApiSAPUpdate("VCM", inb_Voucher.SerialNo, inb_Voucher.Article_No, "ZVCN", inb_Voucher.Status, inb_Voucher.Site, POSTerminal);
                                                    if (calResult != null)
                                                    {
                                                        if (calResult == "200")
                                                        {
                                                            DBSetContext dBSetContext = new DBSetContext(connect);
                                                            INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP();
                                                            {
                                                                INB_VoucherToSAP iNB_VoucherToSAP = new INB_VoucherToSAP();
                                                                iNB_VoucherToSAP.Voucher_Type = inb_Voucher.Voucher_Type;
                                                                iNB_VoucherToSAP.SerialNo = inb_Voucher.SerialNo;
                                                                iNB_VoucherToSAP.Voucher_Value = inb_Voucher.Voucher_Value;
                                                                iNB_VoucherToSAP.Voucher_Currency = inb_Voucher.Voucher_Currency;
                                                                iNB_VoucherToSAP.Validity_From_Date = inb_Voucher.Validity_From_Date;
                                                                iNB_VoucherToSAP.Expiry_Date = inb_Voucher.Expiry_Date;
                                                                iNB_VoucherToSAP.Processing_Type = inb_Voucher.Processing_Type;
                                                                iNB_VoucherToSAP.Status = inb_Voucher.Status;
                                                                iNB_VoucherToSAP.Site = inb_Voucher.Site;
                                                                iNB_VoucherToSAP.Article_No = inb_Voucher.Article_No;
                                                                iNB_VoucherToSAP.Bonus_Buy = inb_Voucher.Bonus_Buy;
                                                                iNB_VoucherToSAP.POSNo = inb_Voucher.POSNo;
                                                                iNB_VoucherToSAP.ReceiptNo = inb_Voucher.ReceiptNo;
                                                                iNB_VoucherToSAP.TranDate = inb_Voucher.TranDate;
                                                                iNB_VoucherToSAP.TranTime = inb_Voucher.TranTime;
                                                                iNB_VoucherToSAP.FileName = inb_Voucher.SerialNo;
                                                                dBSetContext.INB_VoucherToSAP.Add(iNB_VoucherToSAP);
                                                                dBSetContext.SaveChanges();
                                                                _logger_VC.Information($"Insert INB_VoucherToSAP:    {inb_Voucher.SerialNo}");
                                                            };
                                                            string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                            int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                            _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");




                                                            //                    string insertSql = @"INSERT INTO INB_VoucherToSAP 
                                                            //(Voucher_Type, SerialNo, Voucher_Value, Voucher_Currency, Validity_From_Date, Expiry_Date, 
                                                            //Processing_Type, Status, Site, Article_No, Bonus_Buy, POSNo, ReceiptNo, TranDate, TranTime, FileName)
                                                            //VALUES
                                                            //(@Voucher_Type, @SerialNo, @Voucher_Value, @Voucher_Currency, @Validity_From_Date, @Expiry_Date,
                                                            //@Processing_Type, @Status, @Site, @Article_No, @Bonus_Buy, @POSNo, @ReceiptNo, @TranDate, @TranTime, @FileName)";

                                                            //int rowsAffected = connection.Execute(insertSql, inbVoucherSap);
                                                            //_logger_VC.Information($"Insert Status {inb_Voucher.SerialNo},{rowsAffected} Rows  ");
                                                        }
                                                        else
                                                        {
                                                            string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                            int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                            _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger_VC.Information("Không có Data ");
                                                    }
                                                }
                                                if (inb_Voucher.Status == "SOLD")
                                                {
                                                    var calResult = await inbVoucherSap1.CallApiSAPCreate(inb_Voucher.SerialNo, inb_Voucher.Voucher_Value,
                                                                     inb_Voucher.Validity_From_Date, inb_Voucher.Expiry_Date, inb_Voucher.Site,
                                                                     inb_Voucher.Bonus_Buy, inb_Voucher.Article_No, POSTerminal);
                                                    if (calResult != null)
                                                    {
                                                        if (calResult == "200")
                                                        {
                                                            DBSetContext dBSetContext = new DBSetContext(connect);
                                                            INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP();
                                                            {
                                                                INB_VoucherToSAP iNB_VoucherToSAP = new INB_VoucherToSAP();
                                                                iNB_VoucherToSAP.Voucher_Type = inb_Voucher.Voucher_Type;
                                                                iNB_VoucherToSAP.SerialNo = inb_Voucher.SerialNo;
                                                                iNB_VoucherToSAP.Voucher_Value = inb_Voucher.Voucher_Value;
                                                                iNB_VoucherToSAP.Voucher_Currency = inb_Voucher.Voucher_Currency;
                                                                iNB_VoucherToSAP.Validity_From_Date = inb_Voucher.Validity_From_Date;
                                                                iNB_VoucherToSAP.Expiry_Date = inb_Voucher.Expiry_Date;
                                                                iNB_VoucherToSAP.Processing_Type = inb_Voucher.Processing_Type;
                                                                iNB_VoucherToSAP.Status = inb_Voucher.Status;
                                                                iNB_VoucherToSAP.Site = inb_Voucher.Site;
                                                                iNB_VoucherToSAP.Article_No = inb_Voucher.Article_No;
                                                                iNB_VoucherToSAP.Bonus_Buy = inb_Voucher.Bonus_Buy;
                                                                iNB_VoucherToSAP.POSNo = inb_Voucher.POSNo;
                                                                iNB_VoucherToSAP.ReceiptNo = inb_Voucher.ReceiptNo;
                                                                iNB_VoucherToSAP.TranDate = inb_Voucher.TranDate;
                                                                iNB_VoucherToSAP.TranTime = inb_Voucher.TranTime;
                                                                iNB_VoucherToSAP.FileName = inb_Voucher.SerialNo;
                                                                dBSetContext.INB_VoucherToSAP.Add(iNB_VoucherToSAP);
                                                                dBSetContext.SaveChanges();
                                                                _logger_VC.Information($"Insert INB_VoucherToSAP:   {inb_Voucher.SerialNo}");
                                                            };
                                                            string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                            int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                            _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");
                                                            //                    string insertSql = @"INSERT INTO INB_VoucherToSAP 
                                                            //(Voucher_Type, SerialNo, Voucher_Value, Voucher_Currency, Validity_From_Date, Expiry_Date, 
                                                            //Processing_Type, Status, Site, Article_No, Bonus_Buy, POSNo, ReceiptNo, TranDate, TranTime, FileName)
                                                            //VALUES
                                                            //(@Voucher_Type, @SerialNo, @Voucher_Value, @Voucher_Currency, @Validity_From_Date, @Expiry_Date,
                                                            //@Processing_Type, @Status, @Site, @Article_No, @Bonus_Buy, @POSNo, @ReceiptNo, @TranDate, @TranTime, @FileName)";
                                                            //                    int rowsAffected = connection.Execute(insertSql, inbVoucherSap);
                                                            //                    _logger_VC.Information($"Insert Status {inb_Voucher.SerialNo},{rowsAffected} Rows  ");
                                                        }
                                                        else
                                                        {
                                                            string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                            int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                            _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger_VC.Information("Không có Data ");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger_VC.Error("Loi: Connect DB ", e.Message);
                                    sendEmailExample.SendMailError("Loi: Connect DB " + e.Message);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger_VC.Error("Loi: Connect DB", e.Message);
                            sendEmailExample.SendMailError("Loi: Connect DB " + e.Message);
                        }
                        break;
                    case "VoucherSAPRetry":
                        _logger_VC.Information("--------------------------VoucherSAPRetry----------------------------");
                        try
                        {
                            var connectionsVC = db.ConfigConnections.ToList().Where(p => p.Type == "VC" && p.Status == true);
                            if (Name == "All")
                            {
                                connectionsVC = connectionsVC.Where(p => p.Type == "VC" && p.Status == true).ToList();
                            }
                            else
                            {
                                connectionsVC = connectionsVC.Where(p => p.Type == "VC" && p.Status == true && p.Name == Name).ToList();
                            }


                            // --------------------------------Voucher SAP---------------------------------------------------------- 
                            foreach (var connectionString in connectionsVC)
                            {
                                _logger_VC.Information("Connect DB : " + connectionString.Name);
                                try
                                {
                                    var connectionsVC_Retry = db.ConfigConnections.ToList().Where(p => p.Type == "VC_Retry" && p.Status == true && p.Name == $"{connectionString.Name}_Retry");

                                    string connect = connectionString.ConnectString;
                                    using (SqlConnection connection = new SqlConnection(connect))
                                    {
                                        connection.Open();
                                        var timeout = 300;
                                        var parameters = new
                                        {
                                            Status = "",
                                            Date = "",
                                            Retry = 1
                                        };
                                        var results = connection.Query("INB_Voucher ", parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        if (results.Count == 0)
                                        {
                                            _logger_VC.Information($"{connectionString.Name} : Không có Data ");
                                        }
                                        else
                                        {
                                            Inb_Voucher inb_Voucher = new Inb_Voucher();
                                            foreach (var result in results)
                                            {
                                                inb_Voucher.Voucher_Type = result.Voucher_Type;
                                                inb_Voucher.SerialNo = result.SerialNo;
                                                inb_Voucher.Voucher_Value = result.Voucher_Value;
                                                inb_Voucher.Voucher_Currency = result.Voucher_Currency;
                                                inb_Voucher.Validity_From_Date = result.Validity_From_Date;
                                                inb_Voucher.Expiry_Date = result.Expiry_Date;
                                                inb_Voucher.Processing_Type = result.Processing_Type;
                                                inb_Voucher.Status = result.Status;
                                                inb_Voucher.Site = result.Site;
                                                inb_Voucher.Article_No = result.Article_No;
                                                inb_Voucher.Bonus_Buy = result.Bonus_Buy;
                                                inb_Voucher.POSNo = result.POSNo;
                                                inb_Voucher.ReceiptNo = result.ReceiptNo;
                                                inb_Voucher.TranDate = result.TranDate;
                                                inb_Voucher.TranTime = result.TranTime;
                                                string POSTerminal = inb_Voucher.ReceiptNo.Substring(0, 6);
                                                if (inb_Voucher.Status == "EXP")
                                                {
                                                    var calResult = await inbVoucherSap1.CallApiSAPUpdate("VCM", inb_Voucher.SerialNo, inb_Voucher.Article_No, "ZVCN", inb_Voucher.Status, inb_Voucher.Site, POSTerminal);
                                                    if (calResult != null)
                                                    {
                                                        if (calResult == "200")
                                                        {
                                                            DBSetContext dBSetContext = new DBSetContext(connect);
                                                            INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP();
                                                            {
                                                                INB_VoucherToSAP iNB_VoucherToSAP = new INB_VoucherToSAP();
                                                                iNB_VoucherToSAP.Voucher_Type = inb_Voucher.Voucher_Type;
                                                                iNB_VoucherToSAP.SerialNo = inb_Voucher.SerialNo;
                                                                iNB_VoucherToSAP.Voucher_Value = inb_Voucher.Voucher_Value;
                                                                iNB_VoucherToSAP.Voucher_Currency = inb_Voucher.Voucher_Currency;
                                                                iNB_VoucherToSAP.Validity_From_Date = inb_Voucher.Validity_From_Date;
                                                                iNB_VoucherToSAP.Expiry_Date = inb_Voucher.Expiry_Date;
                                                                iNB_VoucherToSAP.Processing_Type = inb_Voucher.Processing_Type;
                                                                iNB_VoucherToSAP.Status = inb_Voucher.Status;
                                                                iNB_VoucherToSAP.Site = inb_Voucher.Site;
                                                                iNB_VoucherToSAP.Article_No = inb_Voucher.Article_No;
                                                                iNB_VoucherToSAP.Bonus_Buy = inb_Voucher.Bonus_Buy;
                                                                iNB_VoucherToSAP.POSNo = inb_Voucher.POSNo;
                                                                iNB_VoucherToSAP.ReceiptNo = inb_Voucher.ReceiptNo;
                                                                iNB_VoucherToSAP.TranDate = inb_Voucher.TranDate;
                                                                iNB_VoucherToSAP.TranTime = inb_Voucher.TranTime;
                                                                iNB_VoucherToSAP.FileName = inb_Voucher.SerialNo;
                                                                dBSetContext.INB_VoucherToSAP.Add(iNB_VoucherToSAP);
                                                                dBSetContext.SaveChanges();
                                                                _logger_VC.Information($"Insert INB_VoucherToSAP:    {inb_Voucher.SerialNo}");
                                                            };

                                                            foreach (var connections_retry in connectionsVC_Retry)
                                                            {
                                                                _logger_VC.Information("Connect DB : " + connections_retry.Name);
                                                                using (SqlConnection connection_Retry = new SqlConnection(connections_retry.ConnectString))
                                                                {
                                                                    string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                                    int rowsAffectedupdate = connection_Retry.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                                    _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");

                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            foreach (var connections_retry in connectionsVC_Retry)
                                                            {
                                                                _logger_VC.Information("Connect DB : " + connections_retry.Name);
                                                                using (SqlConnection connection_Retry = new SqlConnection(connections_retry.ConnectString))
                                                                {
                                                                    string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                                    int rowsAffectedupdate = connection_Retry.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                                    _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");

                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger_VC.Information("Không có Data ");
                                                    }
                                                }
                                                if (inb_Voucher.Status == "SOLD")
                                                {
                                                    var calResult = await inbVoucherSap1.CallApiSAPCreate(inb_Voucher.SerialNo, inb_Voucher.Voucher_Value,
                                                                     inb_Voucher.Validity_From_Date, inb_Voucher.Expiry_Date, inb_Voucher.Site,
                                                                     inb_Voucher.Bonus_Buy, inb_Voucher.Article_No, POSTerminal);
                                                    if (calResult != null)
                                                    {
                                                        if (calResult == "200")
                                                        {
                                                            DBSetContext dBSetContext = new DBSetContext(connect);
                                                            INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP();
                                                            {
                                                                INB_VoucherToSAP iNB_VoucherToSAP = new INB_VoucherToSAP();
                                                                iNB_VoucherToSAP.Voucher_Type = inb_Voucher.Voucher_Type;
                                                                iNB_VoucherToSAP.SerialNo = inb_Voucher.SerialNo;
                                                                iNB_VoucherToSAP.Voucher_Value = inb_Voucher.Voucher_Value;
                                                                iNB_VoucherToSAP.Voucher_Currency = inb_Voucher.Voucher_Currency;
                                                                iNB_VoucherToSAP.Validity_From_Date = inb_Voucher.Validity_From_Date;
                                                                iNB_VoucherToSAP.Expiry_Date = inb_Voucher.Expiry_Date;
                                                                iNB_VoucherToSAP.Processing_Type = inb_Voucher.Processing_Type;
                                                                iNB_VoucherToSAP.Status = inb_Voucher.Status;
                                                                iNB_VoucherToSAP.Site = inb_Voucher.Site;
                                                                iNB_VoucherToSAP.Article_No = inb_Voucher.Article_No;
                                                                iNB_VoucherToSAP.Bonus_Buy = inb_Voucher.Bonus_Buy;
                                                                iNB_VoucherToSAP.POSNo = inb_Voucher.POSNo;
                                                                iNB_VoucherToSAP.ReceiptNo = inb_Voucher.ReceiptNo;
                                                                iNB_VoucherToSAP.TranDate = inb_Voucher.TranDate;
                                                                iNB_VoucherToSAP.TranTime = inb_Voucher.TranTime;
                                                                iNB_VoucherToSAP.FileName = inb_Voucher.SerialNo;
                                                                dBSetContext.INB_VoucherToSAP.Add(iNB_VoucherToSAP);
                                                                dBSetContext.SaveChanges();
                                                                _logger_VC.Information($"Insert INB_VoucherToSAP:   {inb_Voucher.SerialNo}");
                                                            };
                                                            foreach (var connections_retry in connectionsVC_Retry)
                                                            {
                                                                _logger_VC.Information("Connect DB : " + connections_retry.Name);
                                                                using (SqlConnection connection_Retry = new SqlConnection(connections_retry.ConnectString))
                                                                {
                                                                    string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                                    int rowsAffectedupdate = connection_Retry.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                                    _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");

                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            foreach (var connections_retry in connectionsVC_Retry)
                                                            {
                                                                _logger_VC.Information("Connect DB : " + connections_retry.Name);
                                                                using (SqlConnection connection_Retry = new SqlConnection(connections_retry.ConnectString))
                                                                {
                                                                    string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                                    int rowsAffectedupdate = connection_Retry.Execute(updatetSql, new { SerialNo = inb_Voucher.SerialNo });
                                                                    _logger_VC.Information($"Update Status:  {inb_Voucher.SerialNo},- {rowsAffectedupdate} Rows ");

                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger_VC.Information("Không có Data ");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger_VC.Error("Loi: Connect DB ", e.Message);
                                    sendEmailExample.SendMailError("Loi: Connect DB " + e.Message);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger_VC.Error("Loi: Connect DB", e.Message);
                            sendEmailExample.SendMailError("Loi: Connect DB " + e.Message);
                        }
                        break;
                    case "PRD_CARStockBalance":
                        stopwatch.Start();
                        _logger_VINID.Information("-----------------------PRD_CARStockBalance-------------------------------");
                        var configcar = db.Configs.SingleOrDefault(p => p.Type == "PRD_CARStockBalance" && p.Status == true);
                        if (configcar != null)
                        {
                            string IpSftp = configcar.IpSftp;
                            int Host = 22;
                            string username = configcar.username;
                            string password = configcar.password;
                            string pathRemoteCar = configcar.pathRemoteDirectory;
                            string pathLocalCar = configcar.pathLocalDirectory;
                            string processedFoldercar = configcar.MoveFolderPath;
                            if (configcar.IsDownload == true)
                            {
                                SftpHelper sftpHelper = new SftpHelper(IpSftp, Host, username, password, _logger_VINID);
                                _logger_VINID.Information("Bắt đầu tải CARStockBalance : Call sftpHelper.DownloadAuthen ");
                                sftpHelper.DownloadAuthen(pathRemoteCar, pathLocalCar);
                            }
                            var configupfile = db.Configs.SingleOrDefault(p => p.Type == "PRD_CARStockBalanceVinID" && p.Status == true);
                            string IpSftpvinid = configupfile.IpSftp;
                            int Hostvinid = 2022;//2022
                            string usernamevinid = configupfile.username;
                            string passwordvinid = configupfile.password;
                            string pathuploadfile = configupfile.pathRemoteDirectory;
                            string pathlocaldirectory = configupfile.pathLocalDirectory;
                            string pathLocalfile = configupfile.LocalFoderPath;
                            string processedFoldervinid = configupfile.MoveFolderPath;
                            string[] filteredStrings = Directory.GetFiles(pathlocaldirectory, "*.CSV");
                            int countslvinid = filteredStrings.Length;
                            _logger_VINID.Information("Copy file Process");
                            if (filteredStrings.Length > 0)
                            {
                                if (Directory.Exists(pathLocalfile))
                                {
                                    foreach (string f in filteredStrings)
                                    {
                                        // Remove path from the file name.
                                        string fName = f.Substring(pathlocaldirectory.Length);
                                        File.Copy(Path.Combine(pathlocaldirectory, fName), Path.Combine(pathLocalfile, fName), true);
                                    }
                                    _logger_VINID.Information($"Copy file : {filteredStrings.Length}");
                                }
                                else
                                {
                                    Directory.CreateDirectory(pathLocalfile);
                                    foreach (string f in filteredStrings)
                                    {
                                        // Remove path from the file name.
                                        string fName = f.Substring(pathlocaldirectory.Length);
                                        File.Copy(Path.Combine(pathlocaldirectory, fName), Path.Combine(pathLocalfile, fName), true);
                                    }
                                }
                            }
                            else
                            {
                                _logger_VINID.Information("Không có file Copy");
                            }
                            _logger_VINID.Information("Upload File Stock VinID");
                            SftpHelper sftpHelperup = new SftpHelper(IpSftpvinid, Hostvinid, usernamevinid, passwordvinid, _logger_VINID);
                            sftpHelperup.UploadSftpLinux2(pathLocalfile, pathuploadfile, processedFoldervinid, "*.CSV");

                            _logger_VINID.Information("Bắt đầu đọc CARStockBalance: " + pathLocalCar);
                            if (Directory.Exists(pathLocalCar))
                            {
                                string[] xmlFilester = filteredStrings.Where(str => str.Contains("PRD_VINID")).ToArray();
                                var count = 0;
                                int countsl = xmlFilester.Length;
                                _logger_VINID.Information("Tổng File :" + countsl);
                                var taskster = new Task[countsl];
                                if (countsl > 0)
                                {
                                    if (countsl > 10)
                                    {
                                        for (int i = 0; i < countsl; i++)
                                        {
                                            string xmlFile = xmlFilester[i];
                                            taskster[i] = Task.Run(() => readfilSAP.ProcessCSV_CARStockBalance(xmlFile, processedFoldercar));
                                            count++;

                                        }
                                        await Task.WhenAll(taskster);
                                        _logger_VINID.Information("(Job CARStockBalance)- Số File : " + count.ToString() + " Insert thành công");
                                    }
                                    else
                                    {
                                        foreach (string xmlFile1 in filteredStrings)
                                        {
                                            readfilSAP.ProcessCSV_CARStockBalance(xmlFile1, processedFoldercar);
                                            count++;
                                        }
                                        _logger_VINID.Information("(Job CARStockBalance)- Số File : " + count.ToString() + " Insert thành công");
                                    }
                                    string[] getfile = Directory.GetFiles(pathLocalCar, "*.CSV");
                                    int fileLoi = getfile.Length;
                                    if (fileLoi > 0)
                                    {
                                        _logger_VINID.Information("(Job CARStockBalance) Có : " + fileLoi + " file Lỗi Vui Lòng Kiểm tra ở Folder " + pathLocalCar);
                                    }
                                    DataSqlProcedure dataSql = new DataSqlProcedure(_logger_VINID);
                                    if (fileLoi == 0)
                                    {
                                        dataSql.Insert_TK_CarStockBalance();
                                    }
                                    else
                                    {
                                        _logger_VINID.Information("Còn " + fileLoi + " Chưa đọc hết chưa run được procedure");
                                    }
                                }
                                else
                                {
                                    _logger_VINID.Information("Không Có Data");
                                }
                            }
                            else
                            {
                                _logger_VINID.Information("File Not Found");
                                Directory.CreateDirectory(pathLocalCar);
                            }
                        }
                        else
                        {
                            _logger_VINID.Information("Chưa khai báo Host");
                        }
                        stopwatch.Stop();
                        string jsonDataVinid = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""VIND_CARStock"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                            StringContent content = new StringContent(jsonDataVinid, Encoding.UTF8, "application/json");
                            HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                        }
                        break;
                    case "GCP_PLH":
                        stopwatch.Start();
                        _logger_PLH.Information("------------------------------------------------------");
                        _logger_PLH.Information("Run GCP");
                        var configPLH = db.ConfigConnections.ToList().Where(p => p.Type == Name && p.Status == true);//config DB
                        if (configPLH.Count() > 0)
                        {
                            PLH_To_GCP PLH_To_GCPs = new PLH_To_GCP(_logger_PLH);
                            foreach (var cfig in configPLH)
                            {
                                _logger_PLH.Information($"Connect DB: {cfig.Name}");
                                var listOrder = PLH_To_GCPs.OrderExpToGCPAsync(cfig.ConnectString);//listOrder
                                if (listOrder.Count > 0)
                                {
                                    _logger_PLH.Information($"Send Data To API: {listOrder.Count} Row");
                                    string apiUrl = configuration["API_GCP"];
                                    using (HttpClient httpClient = new HttpClient())
                                    {
                                        try
                                        {
                                            string json = JsonConvert.SerializeObject(listOrder);
                                            //  string filePathError = $"Data.text";
                                            // File.WriteAllText(filePathError, json);
                                            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                            request.Content = content;
                                            HttpResponseMessage response = await httpClient.SendAsync(request);
                                            // HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                                            _logger_PLH.Information($"Response API: {response.StatusCode}");
                                            if (response.IsSuccessStatusCode)
                                            {
                                                PLH_To_GCPs.InsertTempGCP(listOrder, "True", cfig.ConnectString);
                                            }
                                            else
                                            {
                                                PLH_To_GCPs.InsertTempGCP(listOrder, "False", cfig.ConnectString);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger_PLH.Error("Lỗi: " + ex.Message);
                                        }
                                    }
                                }
                                else
                                {
                                    _logger_PLH.Information($"Không có data GCP");
                                }
                            }
                        }
                        else
                        {
                            _logger_PLH.Information("Staus đang Off or chưa khai báo Connections type = PLH_INBOUND");
                        }
                        stopwatch.Stop();
                        string jsonDataPLH = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""GCP_PLH"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                            StringContent content = new StringContent(jsonDataPLH, Encoding.UTF8, "application/json");
                            HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                        }
                        break;
                    case "GCP_ARCHIVE_fix":
                        _logger.Information("Run GCP_ARCHIVE");
                        var configPLH_Arfix = db.ConfigConnections.ToList().Where(p => p.Type == "PLH_INBOUNDArchive" && p.Status == true);//config DB
                        if (configPLH_Arfix.Count() > 0)
                        {
                            string isRunning_cfg = configuration["isRunning"];
                            bool isRunning = (isRunning_cfg != null && (isRunning_cfg.Equals("true", StringComparison.OrdinalIgnoreCase) || isRunning_cfg.Equals("1")));
                            PLH_To_GCP_Retry pLH_To_GCP_Retry = new PLH_To_GCP_Retry(_logger);
                            int count = 0;
                            foreach (var cfig in configPLH_Arfix)
                            {
                                _logger.Information($"Connect DB : {cfig.Name}");
                                while (isRunning)
                                {
                                    var listOrder = pLH_To_GCP_Retry.OrderExpToGCPAsyncArchive(cfig.ConnectString); // listOrder

                                    if (listOrder.Count > 0)
                                    {
                                        _logger.Information($"Gửi dữ liệu đến API: {listOrder.Count} Row");

                                        string apiUrl = configuration["API_GCP"];
                                        using (HttpClient httpClient = new HttpClient())
                                        {
                                            try
                                            {
                                                string json = JsonConvert.SerializeObject(listOrder);
                                                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                request.Content = content;
                                                HttpResponseMessage response = await httpClient.SendAsync(request);

                                                _logger.Information($"Phản hồi từ API: {response.StatusCode}");

                                                PLH_To_GCP PLH_To_GCPs = new PLH_To_GCP(_logger);
                                                if (response.IsSuccessStatusCode)
                                                {
                                                    PLH_To_GCPs.InsertTempGCP(listOrder, "True", cfig.ConnectString);
                                                }
                                                else
                                                {
                                                    PLH_To_GCPs.InsertTempGCP(listOrder, "False", cfig.ConnectString);
                                                }
                                                await Task.Delay(TimeSpan.FromSeconds(10));

                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.Error("Lỗi: " + ex.InnerException);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logger.Information("Không có dữ liệu GCP");
                                        count++;
                                        if (count == 5)
                                        {
                                            isRunning = false;
                                        }
                                    }
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                }
                            }
                        }
                        else
                        {
                            _logger.Information("Staus đang Off or chưa khai báo Connections type = GCP_ARCHIVE");
                        }

                        break;
                    case "GCP_Sale_Retry_WCM":
                        _logger.Information("Run GCP_Sale_Retry");
                        var configPLH_RetrySftp = db.Configs.SingleOrDefault(p => p.Type == Name && p.Status == true);
                        if (configPLH_RetrySftp != null)
                        {
                            if (configPLH_RetrySftp.IsDownload == true)
                            {
                                SftpHelper sftpHelper = new SftpHelper(configPLH_RetrySftp.IpSftp, 22, configPLH_RetrySftp.username, configPLH_RetrySftp.password, _logger);
                                sftpHelper.DownloadNoAuthen(configPLH_RetrySftp.pathRemoteDirectory, configPLH_RetrySftp.LocalFoderPath, true);
                            }
                        }
                        _logger.Information("RUN WCM_GCP_Retry");
                        try
                        {
                            var configSet1Wcm = db.ConfigConnections.ToList().Where(p => p.Type == "GCP_WCM_Retry" && p.Status == true);//config DB
                            if (configSet1Wcm != null)
                            {
                                foreach (var connection in configSet1Wcm)
                                {
                                    if (Directory.Exists(configPLH_RetrySftp.LocalFoderPath))
                                    {
                                        string[] filteredWcm = Directory.GetFiles(configPLH_RetrySftp.LocalFoderPath, "*.CSV");
                                        string[] xmlFilewcm = filteredWcm.Where(str => str.Contains("SALEOUT")).ToArray();
                                        foreach (string xmlFileWcm in xmlFilewcm)
                                        {
                                            readfilSAP.ProcessCSV_GCP_Sale_Retry_WCM(xmlFileWcm, configPLH_RetrySftp.MoveFolderPath, connection.ConnectString);
                                        }
                                        _logger.Information($"Run {xmlFilewcm.Length} Thành Công");
                                    }
                                    else
                                    {
                                        _logger.Information("config LocalFoderPath");
                                    }
                                }
                            }
                            else
                            {
                                _logger.Information("config null");

                            }

                        }
                        catch (Exception e)
                        {
                            _logger.Error(e.Message);
                        }
                        break;
                    case "GCP_Sale_Retry_PL":
                        _logger.Information("Run GCP_Sale_Retry_PL");
                        var configPLH_RetrySftpPL = db.Configs.SingleOrDefault(p => p.Type == Name && p.Status == true);
                        if (configPLH_RetrySftpPL != null)
                        {
                            if (configPLH_RetrySftpPL.IsDownload == true)
                            {
                                SftpHelper sftpHelper = new SftpHelper(configPLH_RetrySftpPL.IpSftp, 22, configPLH_RetrySftpPL.username, configPLH_RetrySftpPL.password, _logger);
                                sftpHelper.DownloadNoAuthen(configPLH_RetrySftpPL.pathRemoteDirectory, configPLH_RetrySftpPL.LocalFoderPath, true);
                            }
                        }
                        _logger.Information("RUN PLH_GCP_Retry");
                        var configPLH_inbound_PL = db.ConfigConnections.ToList().Where(p => p.Type == "PLH_GCP_Retry" && p.Status == true);//config DB
                        foreach (var connection in configPLH_inbound_PL)
                        {
                            if (Directory.Exists(configPLH_RetrySftpPL.LocalFoderPath))
                            {
                                string[] filteredStrings = Directory.GetFiles(configPLH_RetrySftpPL.LocalFoderPath, "*.CSV");
                                string[] xmlFilester = filteredStrings.Where(str => str.Contains("PLH")).ToArray();
                                foreach (string xmlFilesterString in xmlFilester)
                                {
                                    readfilSAP.ProcessCSV_GCP_Sale_Retry(xmlFilesterString, configPLH_RetrySftpPL.MoveFolderPath, connection.ConnectString);
                                }
                                _logger.Information($"Run {xmlFilester.Length} Thành Công");
                            }
                            else
                            {
                                Directory.CreateDirectory(configPLH_RetrySftpPL.LocalFoderPath);
                                _logger.Information("Không có file or thư mục");
                            }
                        }
                        break;
                    case "HR_All":
                        stopwatch.Start();
                        _logger_HR.Information("Run HR_All");
                        Insert_HR_ALL insertDBPRD = new Insert_HR_ALL(_logger_HR);
                        using (var dbhr = new Dbhrcontext())
                        {

                            var Host = db.Configs.FirstOrDefault(p => p.Type == "HOST");
                            if (Host != null)
                            {
                                if (Host.IpSftp == null && Host.IpSftp == "")
                                {
                                    _logger_HR.Information("Not found IpSftp ");
                                }
                                if (Host.username == null && Host.username == "")
                                {
                                    _logger_HR.Information("Not found IpSftp ");
                                }
                                if (Host.password == null && Host.password == "")
                                {
                                    _logger_HR.Information("Not found IpSftp ");
                                }
                                SftpHelper sftpHelper = new SftpHelper(Host.IpSftp, 22, Host.username, Host.password, _logger_HR);

                                ReadFileHR readfilexml = new ReadFileHR(_logger_HR);

                                try
                                {
                                    //------------config  HR_Terninate--------
                                    var configHR_Terninate = db.Configs.FirstOrDefault(p => p.Type == "HR_Terninate" && p.Status == true);

                                    if (configHR_Terninate != null)
                                    {
                                        string pathRemoteDirectoryter = configHR_Terninate.pathRemoteDirectory;
                                        string pathLocalDirectoryter = configHR_Terninate.pathLocalDirectory;
                                        string processedFolderPathter = configHR_Terninate.MoveFolderPath;
                                        if (pathRemoteDirectoryter == null)
                                        {
                                            _logger_HR.Information("Path not found pathRemoteDirectory ");

                                        }
                                        if (pathLocalDirectoryter == null)
                                        {
                                            _logger_HR.Information("Path not found pathLocalDirectory ");

                                        }
                                        if (processedFolderPathter == null)
                                        {
                                            _logger_HR.Information("Path not found pathMoveDirectory ");

                                        }
                                        if (configHR_Terninate.LastTimeRun == null)
                                        {
                                            configHR_Terninate.LastTimeRun = default(DateTime);
                                        }
                                        _logger_HR.Information("Bắt đầu tải HR_Terninate : Call sftpHelper.DownloadNoAuthen");
                                        sftpHelper.DownloadAuthen(pathRemoteDirectoryter, pathLocalDirectoryter);
                                        //------------Dowload  Getfile HR_Terninate------------
                                        _logger_HR.Information("Bắt đầu đọc file HR_Terninate" + pathLocalDirectoryter);
                                        if (Directory.Exists(pathLocalDirectoryter))
                                        {
                                            string[] filteredStrings = Directory.GetFiles(pathLocalDirectoryter, "*.xml");
                                            string[] xmlFilester = filteredStrings.Where(str => str.Contains("PRD_HR_Dashboard_Terminate")).ToArray();
                                            var counter = 0;
                                            var taskster = new Task[xmlFilester.Length];
                                            if (xmlFilester.Length > 100)
                                            {
                                                for (int i = 0; i < xmlFilester.Length; i++)
                                                {
                                                    string xmlFile = xmlFilester[i];
                                                    taskster[i] = Task.Run(() => readfilexml.ProcessXmlFileHR_Terninate(xmlFile, processedFolderPathter));
                                                    counter++;
                                                }
                                                await Task.WhenAll(taskster);
                                                _logger_HR.Information("Process : " + counter.ToString() + " file HR_Terninate");
                                            }
                                            else
                                            {
                                                foreach (string xmlFile1 in xmlFilester)
                                                {
                                                    readfilexml.ProcessXmlFileHR_Terninate(xmlFile1, processedFolderPathter);
                                                    counter++;
                                                }
                                                _logger_HR.Information("Process: " + counter.ToString() + " file HR_Terninate");
                                            }
                                        }
                                        else
                                        {
                                            _logger_HR.Information("File Not Found");
                                            Directory.CreateDirectory(pathLocalDirectoryter);

                                        }
                                    }
                                    else
                                    {
                                        _logger_HR.Information("Chưa khai báo config cho type:'HR_Terninate''");
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger_HR.Error(e, "HR_Terninate");
                                }

                                try
                                {
                                    // ------------Dowload  HR_Dashboard--------
                                    var configHR_Dashboard = db.Configs.FirstOrDefault(p => p.Type == "HR_Dashboard" && p.Status == true);

                                    if (configHR_Dashboard != null)
                                    {
                                        string pathRemoteDirectory = configHR_Dashboard.pathRemoteDirectory;
                                        string pathLocalDirectory = configHR_Dashboard.pathLocalDirectory;
                                        string processedFolderPath = configHR_Dashboard.MoveFolderPath;

                                        if (pathRemoteDirectory == null)
                                        {
                                            _logger_HR.Information("Path not found pathRemoteDirectory ");

                                        }
                                        if (pathLocalDirectory == null)
                                        {
                                            _logger_HR.Information("Path not found pathLocalDirectory ");

                                        }
                                        if (processedFolderPath == null)
                                        {
                                            _logger_HR.Information("Path not found pathMoveDirectory ");

                                        }
                                        if (configHR_Dashboard.LastTimeRun == null)
                                        {
                                            configHR_Dashboard.LastTimeRun = default(DateTime);
                                        }
                                        _logger_HR.Information("Bắt đầu tải HR_Dashboard: Call sftpHelper.DownloadNoAuthen ");
                                        sftpHelper.DownloadAuthen(pathRemoteDirectory, pathLocalDirectory);

                                        _logger_HR.Information("Bắt đầu đọc HR_Dashboard" + pathLocalDirectory);
                                        if (Directory.Exists(pathLocalDirectory))
                                        {
                                            string[] filteredStrings = Directory.GetFiles(pathLocalDirectory, "*.xml");
                                            string[] xmlFiles = filteredStrings.Where(str => str.Contains("PRD_HR_Dashboard")).ToArray();
                                            var count = 0;
                                            var tasks = new Task[xmlFiles.Length];
                                            if (xmlFiles.Length > 100)
                                            {
                                                for (int i = 0; i < xmlFiles.Length; i++)
                                                {
                                                    string xmlFile = xmlFiles[i];
                                                    tasks[i] = Task.Run(() => readfilexml.ProcessXmlFileDbdashboard(xmlFile, processedFolderPath));
                                                    count++;
                                                }
                                                await Task.WhenAll(tasks);
                                                _logger_HR.Information("Process : " + count.ToString() + " file HR_Dashboard");
                                            }
                                            else
                                            {
                                                foreach (string xmlFile1 in xmlFiles)
                                                {
                                                    readfilexml.ProcessXmlFileDbdashboard(xmlFile1, processedFolderPath);
                                                    count++;
                                                }
                                                _logger_HR.Information("Process : " + count.ToString() + " file HR_Dashboard");
                                            }
                                        }
                                        else
                                        {
                                            _logger_HR.Information("File Not Found");
                                            Directory.CreateDirectory(pathLocalDirectory);
                                        }
                                    }
                                    else
                                    {
                                        _logger_HR.Information("Chưa khai báo config cho type:'HR_Dashboard''");
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger_HR.Error(e, "HR_Dashboard");
                                }
                            }
                            else
                            {
                                _logger_HR.Information("Chưa khai báo Type: 'HOST'");
                            }
                        }
                        insertDBPRD.Insert_HR_All();
                        insertDBPRD.Insert_HR_All_PRD();
                        stopwatch.Stop();
                        string jsonDataHRSAP = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""HR_SAP"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                            StringContent content = new StringContent(jsonDataHRSAP, Encoding.UTF8, "application/json");
                            HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                        }
                        break;
                    case "JobDeleteFile":
                        _logger_DeleteFile.Information($"JobDeleteFile {Name}");
                        DeleteFileArchive deleteFileArchive = new DeleteFileArchive(_logger_DeleteFile);
                        var configJobDeleteFile = db.Configs.SingleOrDefault(p => p.Type == Name && p.Status == true);
                        if (configJobDeleteFile.MoveFolderPath != null)
                        {
                            string folderPath = configJobDeleteFile.MoveFolderPath;
                            deleteFileArchive.DeleteFileAr(folderPath);
                        }
                        else
                        {
                            _logger_DeleteFile.Information("Chưa khai báo thư mục cần xóa !");
                        }
                        break;
                    case "GCP_WCM_NEW_WINPHAR":
                        stopwatch.Start();
                        var configWCM_new = db.ConfigConnections.ToList().Where(p => p.Name == Name && p.Status == true);//config DB
                        if (configWCM_new.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_WCM);
                            foreach (var cfig in configWCM_new)
                            {
                                // _logger_WCM.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        sqlConnection.Query(WCM_Data.SP_GET_SELLOUT_PBLUE_SET(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        var results = sqlConnection.Query<TransTempGCP_WCM>(WCM_Data.Procedure_SaleOut(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();

                                        if (results.Count > 0)
                                        {
                                            var ReceiptNos = results.GroupBy(p => p.ReceiptNo).Select(group => group.Key).ToList();

                                            var batchSize = 1900;
                                            for (int i = 0; i < ReceiptNos.Count; i += batchSize)
                                            {
                                                List<string> batch = ReceiptNos.Skip(i).Take(batchSize).ToList();
                                                var filteredResults = results.Where(r => batch.Contains(r.ReceiptNo)).ToList();

                                                var listOrder = WCM_To_GCPs.OrderWcmToGCPAsync_Fix(cfig.ConnectString, batch, filteredResults);//listOrder
                                                _logger_WCM.Information($"{listOrder.Count}");
                                                List<List<WcmGCPModels>> orderBatches = listOrder
                                                .Select((order, index) => new { order, index })
                                                .GroupBy(x => x.index / 1000)
                                                .Select(group => group.Select(x => x.order).ToList())
                                                .ToList();
                                                if (listOrder.Count > 0)
                                                {
                                                    //  _logger_WCM.Information($"Send Data To API: {listOrder.Count} Row DB: {cfig.Name}");
                                                    string apiUrl = configuration["API_GCP_WCM"];
                                                    using (HttpClient httpClient = new HttpClient())
                                                    {
                                                        try
                                                        {
                                                            foreach (var batch_Send in orderBatches)
                                                            {
                                                                string json = JsonConvert.SerializeObject(batch_Send);
                                                                DateTime currentDateTime = DateTime.Now;
                                                                string dateTimeString = currentDateTime.ToString("yyyyMMddHHmmss");
                                                                string filePathError = $"LogSend\\Data{cfig.Name}{dateTimeString}.text";
                                                                File.WriteAllText(filePathError, json);
                                                                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                                request.Content = content;
                                                                HttpResponseMessage response = await httpClient.SendAsync(request);
                                                                if (response.IsSuccessStatusCode)
                                                                {
                                                                }
                                                                else
                                                                {
                                                                    //DateTime currentDateTime = DateTime.Now;
                                                                    //string dateTimeString = currentDateTime.ToString("yyyyMMddHHmmss");
                                                                    _logger_WCM.Information($"Response API: {response.StatusCode}");
                                                                    //string filePathError = $"data{dateTimeString}.text";
                                                                    File.WriteAllText(filePathError, json);
                                                                    _logger_WCM.Information($"Send API Data Fail");
                                                                    sendEmailExample.SendMailError("Send API Data Fail");
                                                                }
                                                            }
                                                            _logger_WCM.Information($"Send Data To API: {listOrder.Count} Row Done DB: {cfig.Name}");
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger_WCM.Error("Lỗi:GCP_WCM " + ex.Message);
                                                            sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                                        }
                                                        sqlConnection.Close();
                                                    }
                                                }
                                                else
                                                {
                                                    _logger_WCM.Information($"Không có Send data GCP");
                                                }
                                            }
                                            stopwatch.Stop();
                                            if (apiUrllog != "Kibana")
                                            {
                                                string jsonDatawcm = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""GCP_WCM"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                                                using (HttpClient client = new HttpClient())
                                                {
                                                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                                                    StringContent content = new StringContent(jsonDatawcm, Encoding.UTF8, "application/json");
                                                    HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                                                    //if (response.IsSuccessStatusCode)
                                                    //{
                                                    //    string responseBody = await response.Content.ReadAsStringAsync();
                                                    //    _logger_WCM.Information("Phản hồi từ API: " + responseBody);
                                                    //}
                                                    //else
                                                    //{
                                                    //    _logger_WCM.Information("Lỗi: " + response.StatusCode);
                                                    //}
                                                }
                                            }

                                        }
                                        else
                                        {
                                            _logger_WCM.Information($"Không có Send data GCP");

                                        }


                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_WCM.Error("Lỗi:GCP_WCM " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                }
                                _logger_WCM.Information($"------------END {cfig.Name}----------------");
                            }
                        }
                        else
                        {
                            _logger_WCM.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }
                        break;
                    case "PRD_ExportHD":
                        _logger_Einvoice.Information("-----------------------Start ExpEinvoice-----------------------------");
                        ExpInvoiceSAP expInvoiceSAP = new ExpInvoiceSAP(_logger_Einvoice);
                        expInvoiceSAP.ExpInvoiceSAPXML_Fix(Name, "1");
                        _logger_Einvoice.Information("-----------------------End ExpEinvoice-------------------------------");
                        break;
                    case "PRD_ExportHD_Cancel":
                        _logger_Einvoice.Information("-----------------------Start ExpEinvoice_Cancel-----------------------------");
                        ExpInvoiceSAP expInvoiceSAPS = new ExpInvoiceSAP(_logger_Einvoice);
                        expInvoiceSAPS.ExpInvoiceSAPXML_Fix(Name, "2");
                        _logger_Einvoice.Information("-----------------------End ExpEinvoice_Cancel-------------------------------");
                        break;
                    case "GCP_WCM_Json":
                        stopwatch.Start();
                        var configWCM_ss = db.ConfigConnections.ToList().Where(p => p.Type == Name && p.Status == true);//config DB
                        if (configWCM_ss.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_WCM);
                            foreach (var cfig in configWCM_ss)
                            {
                                _logger_WCM.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        var results = new List<SP_Data_WCM>();
                                        if (Name == "GCP_WCM_Retry")
                                        {
                                            results = sqlConnection.Query<SP_Data_WCM>(WCM_Data.SP_Sale_GCP_Retry(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        }
                                        else
                                        {
                                            results = sqlConnection.Query<SP_Data_WCM>(WCM_Data.SP_Sale_GCP(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        }
                                        if (results.Count > 0)
                                        {
                                            var ReceiptNos = results.GroupBy(p => p.DataJson).Select(group => group.Key).ToList();
                                            var batchSize = 1000;
                                            for (int i = 0; i < ReceiptNos.Count; i += batchSize)
                                            {
                                                DateTime currentDateTime = DateTime.Now;
                                                string dateTimeString = currentDateTime.ToString("yyyyMMddHHmmss");
                                                List<string> batch = ReceiptNos.Skip(i).Take(batchSize).ToList();
                                                var filteredResults = results.Where(r => batch.Contains(r.DataJson)).ToList();
                                                var result = WCM_To_GCPs.OrderWcmToGCPAsync_Json(cfig.ConnectString, filteredResults, Name);
                                                if (result.Count > 0)
                                                {
                                                    string json = JsonConvert.SerializeObject(result);
                                                    //string filePathSave = $"LogSend\\DataWCM_{cfig.Name}_{dateTimeString}.text";
                                                    //File.WriteAllText(filePathSave, json);
                                                    string apiUrl = configuration["API_GCP_WCM"];
                                                    using (HttpClient httpClient = new HttpClient())
                                                    {
                                                        try
                                                        {
                                                            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                            request.Content = content;
                                                            HttpResponseMessage response = await httpClient.SendAsync(request);
                                                            if (response.IsSuccessStatusCode)
                                                            {
                                                                _logger_WCM.Information($"Send Data To API: {result.Count} Row Done DB: {cfig.Name}");
                                                            }
                                                            else
                                                            {
                                                                _logger_WCM.Information($"Response API: {response.StatusCode}");
                                                                //string filePathError = $"data{dateTimeString}.text";
                                                                //File.WriteAllText(filePathError, json);
                                                                _logger_WCM.Information($"Send API Data Fail_{response.RequestMessage}");
                                                                sendEmailExample.SendMailError($"Send API Data Fail_{response.RequestMessage}");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger_WCM.Error("Lỗi:GCP_WCM " + ex.Message);
                                                            sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    _logger_WCM.Information("Không có data Send GCP");
                                                }
                                                sqlConnection.Close();
                                            }
                                        }
                                        else
                                        {
                                            _logger_WCM.Information("Không có data Send GCP");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_WCM.Error("Lỗi:GCP_WCM " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                }
                                _logger_WCM.Information($"------------END {cfig.Name}----------------");
                            }
                            stopwatch.Stop();
                            if (apiUrllog != "Kibana")
                            {
                                string jsonDatawcm = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""GCP_WCM"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                                using (HttpClient client = new HttpClient())
                                {
                                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                                    StringContent content = new StringContent(jsonDatawcm, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                                }
                            }

                        }
                        else
                        {
                            _logger_WCM.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }

                        break;
                    case "GCP_WCM_Void":
                        stopwatch.Start();
                        var configWCM_void = db.ConfigConnections.ToList().Where(p => p.Type == Name && p.Status == true);//config DB
                        if (configWCM_void.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_WCM);
                            foreach (var cfig in configWCM_void)
                            {
                                _logger_WCM_Void.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        List<SP_Data_WCM> results = sqlConnection.Query<SP_Data_WCM>(WCM_Data.SP_Sale_Void_GCP(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();

                                        if (results.Count > 0)
                                        {
                                            var ReceiptNos = results.GroupBy(p => p.DataJson).Select(group => group.Key).ToList();
                                            var batchSize = 1000;
                                            for (int i = 0; i < ReceiptNos.Count; i += batchSize)
                                            {
                                                DateTime currentDateTime = DateTime.Now;
                                                string dateTimeString = currentDateTime.ToString("yyyyMMddHHmmss");
                                                List<string> batch = ReceiptNos.Skip(i).Take(batchSize).ToList();
                                                var filteredResults = results.Where(r => batch.Contains(r.DataJson)).ToList();
                                                var result = WCM_To_GCPs.OrderWcmToGCPVoidAsync_Json(cfig.ConnectString, filteredResults);
                                                if (result.Count > 0)
                                                {
                                                    string json = JsonConvert.SerializeObject(result);
                                                    string apiUrl = configuration["API_GCP_Void_WCM"];
                                                    using (HttpClient httpClient = new HttpClient())
                                                    {
                                                        try
                                                        {
                                                            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                            request.Content = content;
                                                            HttpResponseMessage response = await httpClient.SendAsync(request);
                                                            if (response.IsSuccessStatusCode)
                                                            {
                                                                //string filePathSave = $"LogSend_Void\\DataVoid_{Name}_{dateTimeString}.text";
                                                                //WCM_To_GCPs.SaveFile(json, filePathSave);
                                                                _logger_WCM_Void.Information($"send data to api: {result.Count} row done db: {cfig.Name}");
                                                            }
                                                            else
                                                            {
                                                                _logger_WCM_Void.Information($"Response API: {response.StatusCode}");
                                                                string filePathError = $"LogSend_Void\\dataIssue.text";
                                                                WCM_To_GCPs.SaveFile(json, filePathError);
                                                                _logger_WCM_Void.Information($"Send API Data Fail_{response.RequestMessage}");
                                                                sendEmailExample.SendMailError($"Send API Data Fail_{response.RequestMessage}");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger_WCM_Void.Error("Lỗi:GCP_WCM " + ex.Message);
                                                            sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.Information($"Không có Data Send API_GCP");
                                                }
                                                sqlConnection.Close();
                                            }
                                        }
                                        else
                                        {
                                            _logger.Information($"Không có Data Send API_GCP");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_WCM_Void.Error("Lỗi:GCP_WCM " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                }
                                _logger_WCM_Void.Information($"------------END {cfig.Name}----------------");
                            }
                            stopwatch.Stop();
                            if (apiUrllog != "Kibana")
                            {
                                string jsonDatawcm = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""GCP_WCM"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                                using (HttpClient client = new HttpClient())
                                {
                                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                                    StringContent content = new StringContent(jsonDatawcm, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                                }
                            }
                        }
                        else
                        {
                            _logger_WCM_Void.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }

                        break;
                    case "Mongo":
                        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
                        IDatabase redisDatabase = redis.GetDatabase();
                        // string cachedProfile = redisDatabase.StringGet($"user:{userId}");
                        ServiceMongo serviceMongo = new ServiceMongo();
                        var clientmg = serviceMongo.SeviceData();
                        var database = clientmg.GetDatabase("Sale_GCP");
                        var collection = database.GetCollection<SP_Data_WCM>("Transactions");
                        var filter = Builders<SP_Data_WCM>.Filter.Eq("IsRead", true);
                        var projection = Builders<SP_Data_WCM>.Projection
                                                .Include(x => x.ID)
                                                .Include(x => x.OrderNo)
                                                .Include(x => x.IsRead)
                                                .Include(x => x.MemberCardNo)
                                                .Include(x => x.DiscountAmount)
                                                .Include(x => x.VATAmount)
                                                .Include(x => x.LineAmountIncVAT)
                                                .Include(x => x.DataJson);
                        var resultmg = collection.Find(filter).Project<SP_Data_WCM>(projection).ToList();
                        break;
                    case "GCP_WCM_Retry_Old":
                        _logger_WCM.Information($"Start Insert GCP_Json_Retry");
                        var configWCM_old = db.ConfigConnections.ToList().Where(p => p.Name == Name && p.Status == true);//config DB
                        if (configWCM_old.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_WCM);
                            foreach (var cfig in configWCM_old)
                            {
                                // _logger_WCM.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        var resultsRetry = sqlConnection.Query<Temp_SalesGCP_Retry>(WCM_Data.Insert_Data_Retry(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        if (resultsRetry.Count > 0)
                                        {
                                            var ReceiptRetry = resultsRetry.GroupBy(p => p.RECEIPT_NO).Select(group => group.Key).ToList();
                                            var batchSize = 1900;
                                            for (int i = 0; i < ReceiptRetry.Count; i += batchSize)
                                            {
                                                List<string> batch = ReceiptRetry.Skip(i).Take(batchSize).ToList();
                                                var filteredResults = resultsRetry.Where(r => batch.Contains(r.RECEIPT_NO)).ToList();
                                                WCM_To_GCPs.OrderWcmToGCPAsync_Json(cfig.ConnectString, batch);
                                            }
                                        }
                                        else
                                        {
                                            _logger_WCM.Information($"Không có Send data GCP");

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_WCM.Error("Lỗi:GCP_WCM " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                    sqlConnection.Close();
                                }

                                _logger_WCM.Information($"End Insert GCP_Json_Retry");
                            }
                        }
                        else
                        {
                            _logger_WCM.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }
                        break;
                    case "JobDInv_Statement":
                        _logger_DeleteFile.Information($"JobDeleteFile {Name}");
                        var configWCM_INV = db.ConfigConnections.ToList().Where(p => p.Name == "ExpEinvoice" && p.Status == true);//config DB
                        if (configWCM_INV.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_WCM);
                            foreach (var cfig in configWCM_INV)
                            {
                                // _logger_WCM.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        sqlConnection.Query("EXEC InvoiceStatement_Insert_Cancel 0", commandTimeout: timeout);
                                        sqlConnection.Query("EXEC InvoiceStatement_Insert_Nam 0", commandTimeout: timeout);
                                        _logger_DeleteFile.Information("Done");
                                    }
                                    catch (Exception e)
                                    {
                                        _logger_DeleteFile.Error(e.Message);
                                    }
                                }
                            }

                        }
                        break;
                    case "SUMMARY_SALES_OUT":
                        stopwatch.Start();
                        var configWCM_out = db.ConfigConnections.ToList().Where(p => p.Type == Name && p.Status == true);//config DB
                        if (configWCM_out.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_WCM);
                            foreach (var cfig in configWCM_out)
                            {
                                _logger_WCM.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;

                                        var results = sqlConnection.Query<SUMMARY_SALES_OUT>(WCM_Data.SUMMARY_SALES_OUT(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();

                                        if (results.Count > 0)
                                        {
                                            DateTime currentDateTime = DateTime.Now;
                                            string dateTimeString = currentDateTime.ToString("yyyyMMddHHmmss");
                                            string json = JsonConvert.SerializeObject(results);
                                            string apiUrl = configuration["API_checksum"];
                                            string filePathSave = $"LogSend\\SUMSale_{cfig.Name}{dateTimeString}.text";
                                            File.WriteAllText(filePathSave, json);
                                            using (HttpClient httpClient = new HttpClient())
                                            {
                                                try
                                                {
                                                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                    request.Content = content;
                                                    HttpResponseMessage response = await httpClient.SendAsync(request);
                                                    if (response.IsSuccessStatusCode)
                                                    {
                                                        _logger_WCM.Information($"Send Data To API: {results.Count} Row Done DB: {cfig.Name}");
                                                    }
                                                    else
                                                    {
                                                        _logger_WCM.Information($"Send API Data Fail_{response.RequestMessage}");
                                                        sendEmailExample.SendMailError($"Send API Data Fail_{response.RequestMessage}");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger_WCM.Error("Lỗi:GCP_WCM " + ex.Message);
                                                    sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger_WCM.Information("Không có data Send GCP");
                                        }
                                        sqlConnection.Close();

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_WCM.Error("Lỗi:GCP_WCM " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                }
                                _logger_WCM.Information($"------------END {cfig.Name}----------------");
                            }
                            stopwatch.Stop();
                            if (apiUrllog != "Kibana")
                            {
                                string jsonDatawcm = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""GCP_WCM"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                                using (HttpClient client = new HttpClient())
                                {
                                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                                    StringContent content = new StringContent(jsonDatawcm, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                                }
                            }

                        }
                        else
                        {
                            _logger_WCM.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }

                        break;
                    case "WPH_Zalo_Survey":
                        ReadDataRawJson readDataRawJson = new ReadDataRawJson(_logger__WPH_Survey);
                        var configWph = db.ConfigConnections.ToList().Where(p => p.Type == Name && p.Status == true);//config DB
                        if (configWph.Count() > 0)
                        {
                            foreach (var cfig in configWph)
                            {
                                _logger__WPH_Survey.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        string apiUrl = configuration["API_survey_Wph"];
                                        var results = sqlConnection.Query<Temp_Zalo_Survey>(WCM_Data.SP_Zalo_Survey_WPH(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        if (results.Count > 0)
                                        {
                                            foreach (var result in results)
                                            {
                                                string json = $@"{{
                                                      ""RecievedNumber"": ""{result.PhoneNo}"",
                                                      ""OrderCode"": ""{result.RECEIPT_NO}"",
                                                      ""OrderDate"": ""{result.OrderDate}""
                                                         }}";
                                                using (HttpClient httpClient = new HttpClient())
                                                {
                                                    try
                                                    {
                                                        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                        request.Content = content;
                                                        HttpResponseMessage response = await httpClient.SendAsync(request);
                                                        if (response.IsSuccessStatusCode)
                                                        {
                                                            string responseContent = await response.Content.ReadAsStringAsync();
                                                            readDataRawJson.Update_WPH_Zalo_Sv(result.RECEIPT_NO, cfig.ConnectString);
                                                            _logger__WPH_Survey.Information($"Send Survey WPH: RecievedNumber: {result.PhoneNo}----- OrderCode: {result.RECEIPT_NO}---{responseContent}");
                                                        }
                                                        else
                                                        {
                                                            _logger__WPH_Survey.Information($"Send API Data Fail_{response.RequestMessage}");
                                                            sendEmailExample.SendMailError($"Send API Data Fail_{response.RequestMessage}");
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        _logger__WPH_Survey.Error("Lỗi Survey WPH " + ex.Message);
                                                        sendEmailExample.SendMailError("Lỗi Survey WPH: " + cfig.Name + ":" + ex.Message);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger__WPH_Survey.Information("Không có data Survey WPH");
                                        }
                                        sqlConnection.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger__WPH_Survey.Error("Lỗi Survey WPH " + ex.Message);
                                        sendEmailExample.SendMailError("Lỗi Survey WPH: " + cfig.Name + ":" + ex.Message);
                                    }
                                }
                                _logger__WPH_Survey.Information($"------------END {cfig.Name}----------------");
                            }

                        }
                        else
                        {
                            _logger__WPH_Survey.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }

                        break;
                    case "GG_Cloud_Data":
                        string credentialPath = "namnd-415201-bb60219dc9d7.json";
                        GoogleCredential credential = GoogleCredential.FromFile(credentialPath);
                        BigQueryClient clientss = BigQueryClient.Create("namnd-415201", credential);
                        string query = $@"insert into `namnd-415201.Trans.Order` " +
                                         "select '0123', 'Nam5456'";
                        BigQueryResults QerryData = clientss.ExecuteQuery(query, parameters: null);
                        foreach (BigQueryRow row in QerryData)
                        {
                            // Access individual columns by name.
                            string columnValue = row["Name"].ToString();
                            Console.WriteLine(columnValue);
                        }
                        break;
                    case "PRD_ExportTranspoin":
                        var configPLH_ = db.ConfigConnections.ToList().Where(p => p.Type == "VC_Retry" && p.Status == true);
                        string querrys = @"select A.DocumentNo,A.[LineNo],A.ItemNo,A.Barcode,A.SerialNo from CentralSales.[dbo].TransLine A with (Nolock) 
                                        Join CentralSales.[dbo].TransHeader B With(Nolock)  on A.DocumentNo = B.OrderNo
                                        where len(A.SerialNo)>0 
                                        and  B.OrderDate between '20240501'and '20240617'";
                        foreach (var cfig in configPLH_)
                        {
                            _logger.Information(cfig.Name);
                            readfilSAP.ConvertSQLtoXML_CSV_PLH(cfig.ConnectString, querrys, cfig.Name);
                        }
                        break;
                    case "GCP_PLH_Json":
                        stopwatch.Start();
                        var configPLH_Cf = db.ConfigConnections.ToList().Where(p => p.Type == Name && p.Status == true);//config DB
                        if (configPLH_Cf.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_PLH);
                            foreach (var cfig in configPLH_Cf)
                            {
                                _logger_PLH.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        var results = new List<SP_Data_WCM>();
                                        if (Name == "PLH_GCP_Retry_New")
                                        {
                                            results = sqlConnection.Query<SP_Data_WCM>(WCM_Data.SP_Sale_GCP_Retry_PLH(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        }
                                        else
                                        {
                                            results = sqlConnection.Query<SP_Data_WCM>(WCM_Data.SP_Sale_GCP_PLH(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();

                                        }
                                        if (results.Count > 0)
                                        {
                                            var ReceiptNos = results.GroupBy(p => p.DataJson).Select(group => group.Key).ToList();
                                            var batchSize = 1000;
                                            for (int i = 0; i < ReceiptNos.Count; i += batchSize)
                                            {
                                                DateTime currentDateTime = DateTime.Now;
                                                string dateTimeString = currentDateTime.ToString("yyyyMMddHHmmss");
                                                List<string> batch = ReceiptNos.Skip(i).Take(batchSize).ToList();
                                                var filteredResults = results.Where(r => batch.Contains(r.DataJson)).ToList();
                                                var result = WCM_To_GCPs.OrderPLHToGCPAsync_Json(cfig.ConnectString, filteredResults, Name);
                                                if (result.Count > 0)
                                                {
                                                    string json = JsonConvert.SerializeObject(result);
                                                    //string filePathSave = $"LogSend\\DataWCM_{cfig.Name}_{dateTimeString}.text";
                                                    //File.WriteAllText(filePathSave, json);
                                                    string apiUrl = configuration["API_GCP_PLH"];
                                                    using (HttpClient httpClient = new HttpClient())
                                                    {
                                                        try
                                                        {
                                                            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                            request.Content = content;
                                                            HttpResponseMessage response = await httpClient.SendAsync(request);
                                                            if (response.IsSuccessStatusCode)
                                                            {
                                                                _logger_PLH.Information($"Send Data To API: {result.Count} Row Done DB: {cfig.Name}");
                                                            }
                                                            else
                                                            {
                                                                _logger_PLH.Information($"Response API: {response.StatusCode}");
                                                                //string filePathError = $"data{dateTimeString}.text";
                                                                //File.WriteAllText(filePathError, json);
                                                                _logger_WCM.Information($"Send API Data Fail_{response.RequestMessage}");
                                                                sendEmailExample.SendMailError($"Send API Data Fail_{response.RequestMessage}");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger_PLH.Error("Lỗi:GCP_WCM " + ex.Message);
                                                            sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    _logger_PLH.Information("Không có data Send GCP");
                                                }
                                                sqlConnection.Close();
                                            }
                                        }
                                        else
                                        {
                                            _logger_PLH.Information("Không có data Send GCP");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_PLH.Error("Lỗi:GCP_WCM " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                }
                                _logger_PLH.Information($"------------END {cfig.Name}----------------");
                            }
                            stopwatch.Stop();
                            if (apiUrllog != "Kibana")
                            {
                                string jsonDatawcm = $@"{{
                                                      ""HttpContext"": ""{functionName}"",
                                                      ""PosNo"": ""{Name}"",
                                                      ""WebApi"": ""GCP_PLH"",
                                                      ""DeveloperMessage"": ""Done"",
                                                      ""ResponseTime"": {stopwatch.ElapsedMilliseconds}
                                                         }}";
                                using (HttpClient client = new HttpClient())
                                {
                                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                                    StringContent content = new StringContent(jsonDatawcm, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = await client.PostAsync(apiUrllog, content);
                                }
                            }

                        }
                        else
                        {
                            _logger_PLH.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }

                        break;
                    case "PLH_Insert_Retry":
                        _logger_PLH.Information($"Start Insert GCP_Json_Retry");
                        var configPLH_Retry = db.ConfigConnections.ToList().Where(p => p.Name == Name && p.Status == true);//config DB
                        if (configPLH_Retry.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_PLH);
                            foreach (var cfig in configPLH_Retry)
                            {
                                // _logger_WCM.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        var resultsRetry = sqlConnection.Query<Temp_SalesGCP_Retry>(WCM_Data.Insert_Data_Retry_PLH(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                                        if (resultsRetry.Count > 0)
                                        {
                                            var ReceiptRetry = resultsRetry.GroupBy(p => p.RECEIPT_NO).Select(group => group.Key).ToList();
                                            var batchSize = 1900;
                                            for (int i = 0; i < ReceiptRetry.Count; i += batchSize)
                                            {
                                                List<string> batch = ReceiptRetry.Skip(i).Take(batchSize).ToList();
                                                var filteredResults = resultsRetry.Where(r => batch.Contains(r.RECEIPT_NO)).ToList();
                                                WCM_To_GCPs.OrderPLHToGCPAsync_Json(cfig.ConnectString, batch);
                                            }
                                        }
                                        else
                                        {
                                            _logger_PLH.Information($"Không có Send data GCP");

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_PLH.Error("Lỗi:GCP_WCM " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                    sqlConnection.Close();
                                }

                                _logger_PLH.Information($"End Insert GCP_Json_Retry");
                            }
                        }
                        else
                        {
                            _logger_WCM.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }
                        break;
                    case "PLH_WCM_Sale":
                        _logger_PLH_WF.Information($"Start Send Data PLH GCP");
                        PLH_To_GCP PLH_WCM_GCP = new PLH_To_GCP(_logger_PLH_WF);
                        //DateTime currentDateTimes = DateTime.Now;
                        //string dateTimeStrings = currentDateTimes.ToString("yyyyMMdd");
                        //------------------------------------------------------------------------------------/
                        var configPLH_Sale = db.ConfigConnections.ToList().Where(p => p.Name == Name && p.Status == true);//config DB

                        if (configPLH_Sale.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_PLH_WF);
                            foreach (var cfig in configPLH_Sale)
                            {
                                // _logger_WCM.Information($"------------START {cfig.Name}----------------");
                                using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                {
                                    try
                                    {
                                        sqlConnection.Open();
                                        var timeout = 10000;
                                        var resultsRetry = sqlConnection.Query<TransHeader_Temp>(WCM_Data.TransHeader_PLH(), commandTimeout: timeout,commandType:CommandType.StoredProcedure).ToList();
                                        if (resultsRetry.Count > 0)
                                        {
                                            var ListOrder = PLH_WCM_GCP.OrderToGCPAsync_PLHWCM(cfig.ConnectString, resultsRetry);
                                            _logger_PLH_WF.Information($"Send Data To API: {ListOrder.Count} Row");
                                            string apiUrl = configuration["API_PLH_GCP_WLF"];
                                            using (HttpClient httpClient = new HttpClient())
                                            {
                                                try
                                                {
                                                    string json = JsonConvert.SerializeObject(ListOrder);
                                                    //string filePathSave = $"LogSend\\DataWCM_{cfig.Name}_{dateTimeStrings}.text";
                                                    //File.WriteAllText(filePathSave, json);
                                                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                    request.Content = content;
                                                    HttpResponseMessage response = await httpClient.SendAsync(request);
                                                    _logger_PLH_WF.Information($"Response API: {response.StatusCode}");
                                                    if (response.IsSuccessStatusCode)
                                                    {;
                                                        PLH_WCM_GCP.UpdateStausPLH_WCM(resultsRetry, "True", cfig.ConnectString);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger_PLH_WF.Error("Lỗi: " + ex.Message);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger_PLH_WF.Information($"Không có data Send GCP");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_PLH_WF.Error("Lỗi:PLH GCP " + ex.Message);
                                        sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                    }
                                    sqlConnection.Close();
                                }
                                _logger_PLH_WF.Information($"End Send Data PLH GCP");
                            }
                        }
                        else
                        {
                            _logger_PLH_WF.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }
                        break;
                    default:
                        _logger.Information("Invalid function name.");
                        sendEmailExample.SendMailError($@"Invalid function name{functionName}_+___+_{Name}.");
                        break;
                }
            }
            else
            {
                _logger.Information("Please provide a function name as an argument.");
                sendEmailExample.SendMailError("Please provide a function name as an argument.");
            }
        }
    }
}

