using Azure;
using BluePosVoucher;
using BluePosVoucher.Data;
using BluePosVoucher.Models;
using Dapper;
using Job_By_SAP;
using Job_By_SAP.Data;
using Job_By_SAP.Models;
using Job_By_SAP.PLH;
using Job_By_SAP.WCM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Read_xml;
using Read_xml.Data;
using Serilog;
using System;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        SendEmailExample sendEmailExample = new SendEmailExample(_logger);
        InbVoucherSap inbVoucherSap1 = new InbVoucherSap(_logger_VC);
        ReadFile readfilSAP = new ReadFile(_logger);
        IConfiguration configuration = new ConfigurationBuilder()
       .SetBasePath(AppContext.BaseDirectory)
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .Build();
        using (var db = new DbConfigAll())
        {
            string functionName = args[0];
            string Name = args[1];
            // string Name = "ExpEinvoice";
            // string functionName = "PRD_ExportHD";
            if (args.Length > 0)
            {
                // _logger_WCM.Information(Name);
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
                        break;
                    case "GCP":
                        _logger_PLH.Information("------------------------------------------------------");
                        _logger_PLH.Information("Run GCP");
                        var configPLH = db.ConfigConnections.ToList().Where(p => p.Type == "PLH_INBOUND" && p.Status == true);//config DB
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
                        break;
                    case "GCP_WCM":
                        _logger_WCM.Information("--------------------------Run GCP_WCM----------------------------");
                        var configWCM = db.ConfigConnections.ToList().Where(p => p.Name == Name && p.Status == true);//config DB
                        if (configWCM.Count() > 0)
                        {
                            WCM_To_GCP WCM_To_GCPs = new WCM_To_GCP(_logger_WCM);
                            foreach (var cfig in configWCM)
                            {
                                _logger_WCM.Information($"Connect DB: {cfig.Name}");
                                //using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                //{
                                //    sqlConnection.Open();
                                //    var timeout = 600;
                                //    _logger_WCM.Information($"Exec:SP_GET_SELLOUT_PBLUE_SET");
                                //    var ExcPblueSet = sqlConnection.Query(SP_GET_SELLOUT, commandType: CommandType.StoredProcedure, commandTimeout: timeout);
                                //    _logger_WCM.Information($"Done : SP_GET_SELLOUT_PBLUE_SET");
                                //    sqlConnection.Close();
                                //}
                                var listOrder = WCM_To_GCPs.OrderWcmToGCPAsync(cfig.ConnectString);//listOrder
                                if (listOrder.Count > 0)
                                {
                                    //string json = JsonConvert.SerializeObject(listOrder);
                                    //string filePathError = "data.text";
                                    //File.WriteAllText(filePathError, json);
                                    _logger_WCM.Information($"Send Data To API: {listOrder.Count} Row");
                                    string apiUrl = configuration["API_GCP_WCM"];
                                    using (HttpClient httpClient = new HttpClient())
                                    {
                                        try
                                        {
                                            List<List<WcmGCPModels>> orderBatches = listOrder
                                          .Select((order, index) => new { order, index })
                                          .GroupBy(x => x.index / 1000)
                                          .Select(group => group.Select(x => x.order).ToList())
                                          .ToList();
                                            foreach (var batch in orderBatches)
                                            {
                                                string json = JsonConvert.SerializeObject(batch);
                                                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                request.Content = content;
                                                HttpResponseMessage response = await httpClient.SendAsync(request);
                                                // HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                                                if (response.IsSuccessStatusCode)
                                                {
                                                    _logger_WCM.Information($"Response API: {response.StatusCode}, {batch.Count} Row Thành Công!!");
                                                }
                                                else
                                                {
                                                    DateTime currentDateTime = DateTime.Now;
                                                    string dateTimeString = currentDateTime.ToString("yyyyMMddHHmmss");
                                                    _logger_WCM.Information($"Response API: {response.StatusCode}");
                                                    string filePathError = $"data{dateTimeString}.text";
                                                    File.WriteAllText(filePathError, json);
                                                    _logger_WCM.Information($"Send API Data Fail");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger_WCM.Error("Lỗi:GCP_WCM " + ex.InnerException);
                                            sendEmailExample.SendMailError("Loi: " + cfig.Name + ":" + ex.Message);
                                        }
                                    }
                                }
                                else
                                {
                                    _logger_WCM.Information($"Không có Send data GCP");
                                }
                            }
                        }
                        else
                        {
                            _logger_WCM.Information("Staus đang Off or chưa khai báo Connections type = API_GCP_WCM");
                        }
                        break;
                    case "JOB_GCP_WCM":
                        _logger_Job.Information("--------RUN_JOB_GCP_WCM----------");
                        var configWCM_JOB = db.ConfigConnections.ToList().Where(p => p.Name == Name && p.Status == true);
                        if (configWCM_JOB.Count() > 0)
                        {
                            foreach (var cfig in configWCM_JOB)
                            {
                                try
                                {
                                    _logger_Job.Information($"Connect DB: {cfig.Name}");
                                    using (SqlConnection sqlConnection = new SqlConnection(cfig.ConnectString))
                                    {
                                        sqlConnection.Open();
                                        var timeout = 2200;
                                        _logger_Job.Information($"Exec:SP_GET_SELLOUT_PBLUE_SET");
                                        var ExcPblueSet = sqlConnection.Query(WCM_Data.SP_GET_SELLOUT_PBLUE_SET(), commandType: CommandType.StoredProcedure, commandTimeout: timeout);
                                        sqlConnection.Close();
                                        _logger_Job.Information($"Run Job Done");
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger_Job.Error($"xLôi", e.ToString);
                                    sendEmailExample.SendMailError($"Lỗi DB {cfig.Name}, SP_GET_SELLOUT_PBLUE_SET (10.235.25.91)");
                                }
                            }
                        }
                        else
                        {
                            _logger_Job.Information("Staus đang Off or chưa khai báo Connections type = JOB_GCP_WCM");
                            sendEmailExample.SendMailError("Staus đang Off or chưa khai báo Connections type = JOB_GCP_WCM");
                        }
                        break;
                    case "GCP_ARCHIVE":
                        _logger.Information("Run GCP_ARCHIVE");
                        var configPLH_Ar = db.ConfigConnections.ToList().Where(p => p.Type == "PLH_GCP_Retry" && p.Status == true);//config DB
                        if (configPLH_Ar.Count() > 0)
                        {
                            PLH_To_GCP_Retry pLH_To_GCP_Retry = new PLH_To_GCP_Retry(_logger);
                            foreach (var cfig in configPLH_Ar)
                            {
                                _logger.Information($"Connect DB : {cfig.Name}");
                                var listOrder = pLH_To_GCP_Retry.OrderExpToGCPAsyncArchive(cfig.ConnectString);//listOrder
                                if (listOrder.Count > 0)
                                {
                                    _logger.Information($"Send Data To API: {listOrder.Count} Row");
                                    string apiUrl = configuration["API_GCP"];
                                    using (HttpClient httpClient = new HttpClient())
                                    {
                                        try
                                        {
                                            List<List<OrderExpToGCP>> orderBatches = listOrder
                                            .Select((order, index) => new { order, index })
                                            .GroupBy(x => x.index / 1900)
                                            .Select(group => group.Select(x => x.order).ToList())
                                            .ToList();
                                            foreach (var batch in orderBatches)
                                            {
                                                string json = JsonConvert.SerializeObject(batch);
                                                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                                request.Content = content;
                                                HttpResponseMessage response = await httpClient.SendAsync(request);
                                                // HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                                                _logger.Information($"Response API: {response.StatusCode}");
                                                PLH_To_GCP PLH_To_GCPs = new PLH_To_GCP(_logger);
                                                if (response.IsSuccessStatusCode)
                                                {
                                                    PLH_To_GCPs.InsertTempGCP(batch, "True", cfig.ConnectString);
                                                }
                                                else
                                                {
                                                    PLH_To_GCPs.InsertTempGCP(batch, "False", cfig.ConnectString);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error("Lỗi: " + ex.Message);
                                        }
                                    }
                                }
                                else
                                {
                                    _logger.Information($"Không có data GCP");
                                }
                            }
                        }
                        else
                        {
                            _logger.Information("Staus đang Off or chưa khai báo Connections type = GCP_ARCHIVE");
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
                        var configSet1Wcm = db.ConfigConnections.ToList().Where(p => p.Type == "WCM_GCP_Retry" && p.Status == true);//config DB
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
                                Directory.CreateDirectory(configPLH_RetrySftp.LocalFoderPath);
                                _logger.Information("Không có file or thư mục");
                            }
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
                    case "GCP_WCM_NEW":
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
                                        var timeout = 3000;
                                        var results = sqlConnection.Query<TransTempGCP_WCM>(WCM_Data.SP_GET_SELLOUT_PBLUE_SET(), commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();

                                        if (results.Count > 0)
                                        {
                                            var ReceiptNos = results.GroupBy(p => p.ReceiptNo).Select(group => group.Key).ToList();

                                            var batchSize = 1900;
                                            for (int i = 0; i < ReceiptNos.Count; i += batchSize)
                                            {
                                                List<string> batch = ReceiptNos.Skip(i).Take(batchSize).ToList();
                                                var filteredResults = results.Where(r => batch.Contains(r.ReceiptNo)).ToList();

                                                var listOrder = WCM_To_GCPs.OrderWcmToGCPAsync_Fix(cfig.ConnectString, batch, filteredResults);//listOrder

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
                        _logger_Einvoice.Information("-----------------------ExpEinvoice-------------------------------");
                        ReadFile ExportXML = new ReadFile(_logger_Einvoice);
                        try
                        {
                            var connections = db.ConfigConnections.SingleOrDefault(p => p.Type == Name && p.Status == true);
                            var configXml = db.Configs.SingleOrDefault(p => p.Type == Name && p.Status == true);
                            if (connections != null && connections.ConnectString != null)
                            {
                                var currentDatepathxml = DateTime.Now;
                                string currentDatepath = currentDatepathxml.ToString("yyyyMMddHHmmss");
                                //--------------------------------------------------//
                                string[] splitValues = configXml.TimeRun.Split(';');
                                if (splitValues.Length >= 3)
                                {
                                    string lastdate = splitValues[0];
                                    int intValue;
                                    int.TryParse(lastdate, out intValue);
                                    string firstValuePOS = splitValues[1];
                                    string secondValueSAP = splitValues[2];
                                    //---------------------------------------------//
                                    var currentDatexml = DateTime.Today;
                                    var previousDate = currentDatexml.AddDays(-intValue);
                                    string StartDateString = previousDate.ToString("yyyy-MM-dd");
                                    string EndDateString = previousDate.ToString("yyyy-MM-dd");
                                    if (firstValuePOS.Length > 0)
                                    {
                                        var resultxmlPOS = ExportXML.ConvertSQLtoXML(connections.ConnectString, firstValuePOS, StartDateString, EndDateString, "0104918404", "", "");
                                        string outputFilePathPos = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_STORE_{currentDatepath}.xml";
                                        if (resultxmlPOS != null)
                                        {
                                            using (StreamWriter writer = new StreamWriter(outputFilePathPos))
                                            {
                                                writer.Write(resultxmlPOS.ToString());
                                                //   _logger_Einvoice.Information($"Tạo File TAX_RECONCILE_NOSTORE_{currentDatepath}.xml Done");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logger_Einvoice.Information($"Chưa Khai Báo Time Run định Dạng  2;(1);2  :: 1 là Tạo file POS ");
                                    }
                                    if (secondValueSAP.Length > 0)
                                    {
                                        var resultxmlSAP = ExportXML.ConvertSQLtoXML(connections.ConnectString, secondValueSAP, StartDateString, EndDateString, "0104918404", "", "");
                                        string outputFilePathSAP = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_NOSTORE_{currentDatepath}.xml";
                                        if (resultxmlSAP != null)
                                        {
                                            using (StreamWriter writer = new StreamWriter(outputFilePathSAP))
                                            {
                                                writer.Write(resultxmlSAP.ToString());
                                                //  _logger_Einvoice.Information($"Tạo File TAX_RECONCILE_NOSTORE_{currentDatepath}.xml Done");
                                            }
                                        }

                                    }
                                    else
                                    {
                                        _logger_Einvoice.Information($"Chưa Khai Báo Time Run định Dạng  2;1;(2)   2 : Tạo file SAP ");
                                    }
                                }
                                else
                                {
                                    _logger_Einvoice.Information($"Chưa Khai Báo Time Run định Dạng  2;1;2  :: (2) là Getdate- so ngay, 1 : Tạo file Pos, 2 : Tạo File SAP ");
                                }
                                string[] filteredStringsxml = Directory.GetFiles(configXml.LocalFoderPath, "*.XML");
                                if (filteredStringsxml.Length > 0)
                                {
                                    if (Directory.Exists(configXml.MoveFolderPath))
                                    {
                                        foreach (string f in filteredStringsxml)
                                        {
                                            string fName = f.Substring(configXml.LocalFoderPath.Length);
                                            File.Copy(Path.Combine(configXml.LocalFoderPath, fName), Path.Combine(configXml.MoveFolderPath, fName), true);
                                        }
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(configXml.MoveFolderPath);
                                        foreach (string f in filteredStringsxml)
                                        {
                                            string fName = f.Substring(configXml.LocalFoderPath.Length);
                                            File.Copy(Path.Combine(configXml.LocalFoderPath, fName), Path.Combine(configXml.MoveFolderPath, fName), true);
                                        }
                                    }
                                }
                                else
                                {
                                    _logger_VINID.Information("Không có file Copy");
                                }
                                if (filteredStringsxml.Length > 0)
                                {
                                    if (configXml != null && configXml.pathRemoteDirectory != null && configXml.MoveFolderPath != null
                                        && configXml.IpSftp != null && configXml.username != null && configXml.password != null && configXml.LocalFoderPath != null)
                                    {
                                        SftpHelper sftpHelperupfile = new SftpHelper(configXml.IpSftp, 22, configXml.username, configXml.password, _logger_Einvoice);
                                        sftpHelperupfile.UploadSftpLinux2(configXml.LocalFoderPath, configXml.pathRemoteDirectory, configXml.MoveFolderPath, "*.XML");
                                    }
                                    else
                                    {
                                        _logger_Einvoice.Information("Chưa khai báo Upload file config ExpEinvoice");
                                    }
                                }
                                else
                                {
                                    _logger_Einvoice.Information("Không Có data để UpLoad.");
                                }
                            }
                            else
                            {
                                _logger_Einvoice.Information("Chưa khai báo đủ connection or config");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger_Einvoice.Information(ex.Message);
                        }

                        break;
                    default:
                        _logger.Information("Invalid function name.");
                        sendEmailExample.SendMailError("Invalid function name.");
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

