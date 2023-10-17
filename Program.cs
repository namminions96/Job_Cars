using BluePosVoucher;
using BluePosVoucher.Data;
using BluePosVoucher.Models;
using Dapper;
using Job_By_SAP.Data;
using Job_By_SAP.Models;
using Job_By_SAP.PLH;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Read_xml;
using Serilog;
using System;
using System.Data;
using System.Diagnostics.Metrics;
using System.Text;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

internal class Program
{
    private static IConfiguration _configuration;
    private static ILogger _logger;
    private static async Task Main(string[] args)
    {
        _logger = SerilogLogger.GetLogger();
        InbVoucherSap inbVoucherSap1 = new InbVoucherSap(_logger);
        IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();
        using (var db = new DbConfigAll())
        {
            string functionName = args[0];
            //string functionName = "GCP";
            if (args.Length > 0)
            {
                switch (functionName)
                {
                    case "VoucherSAP":
                        _logger.Information("------------------------------------------------------");
                        _logger.Information("Run VoucherSAP");
                        try
                        {
                            //var connections = db.ConfigConnections.ToList().Where(p => p.Type == "VC" && p.Status == true);
                            var connectionsVC = db.ConfigConnections.ToList().Where(p => p.Type == "VC" && p.Status == true);
                            // --------------------------------Voucher SAP---------------------------------------------------------- 
                            foreach (var connectionString in connectionsVC)
                            {
                                _logger.Information("Connect DB : " + connectionString.Name);
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
                                            _logger.Information($"{connectionString.Name} : Không có Data ");
                                        }
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
                                                        INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP()
                                                        {
                                                            Voucher_Type = inb_Voucher.Voucher_Type,
                                                            SerialNo = inb_Voucher.SerialNo,
                                                            Voucher_Value = inb_Voucher.Voucher_Value,
                                                            Voucher_Currency = inb_Voucher.Voucher_Currency,
                                                            Validity_From_Date = inb_Voucher.Validity_From_Date,
                                                            Expiry_Date = inb_Voucher.Expiry_Date,
                                                            Processing_Type = inb_Voucher.Processing_Type,
                                                            Status = inb_Voucher.Status,
                                                            Site = inb_Voucher.Site,
                                                            Article_No = inb_Voucher.Article_No,
                                                            Bonus_Buy = inb_Voucher.Bonus_Buy,
                                                            POSNo = inb_Voucher.POSNo,
                                                            ReceiptNo = inb_Voucher.ReceiptNo,
                                                            TranDate = inb_Voucher.TranDate,
                                                            TranTime = inb_Voucher.TranTime,
                                                            FileName = inb_Voucher.SerialNo
                                                        };
                                                        string insertSql = @"INSERT INTO INB_VoucherToSAP 
                                        (Voucher_Type, SerialNo, Voucher_Value, Voucher_Currency, Validity_From_Date, Expiry_Date, 
                                        Processing_Type, Status, Site, Article_No, Bonus_Buy, POSNo, ReceiptNo, TranDate, TranTime, FileName)
                                        VALUES
                                        (@Voucher_Type, @SerialNo, @Voucher_Value, @Voucher_Currency, @Validity_From_Date, @Expiry_Date,
                                        @Processing_Type, @Status, @Site, @Article_No, @Bonus_Buy, @POSNo, @ReceiptNo, @TranDate, @TranTime, @FileName)";
                                                        int rowsAffected = connection.Execute(insertSql, inbVoucherSap);
                                                        if (rowsAffected > 0)
                                                        {
                                                            _logger.Information("Data inserted successfully!" + rowsAffected + " Row");
                                                        }
                                                        string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                        int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inbVoucherSap.SerialNo });

                                                        if (rowsAffectedupdate > 0)
                                                        {
                                                            _logger.Information("Data Updated successfully!" + rowsAffectedupdate + " Row");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger.Information("Result API : " + calResult);
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.Information("Không có Data ");
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
                                                        INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP()
                                                        {
                                                            Voucher_Type = inb_Voucher.Voucher_Type,
                                                            SerialNo = inb_Voucher.SerialNo,
                                                            Voucher_Value = inb_Voucher.Voucher_Value,
                                                            Voucher_Currency = inb_Voucher.Voucher_Currency,
                                                            Validity_From_Date = inb_Voucher.Validity_From_Date,
                                                            Expiry_Date = inb_Voucher.Expiry_Date,
                                                            Processing_Type = inb_Voucher.Processing_Type,
                                                            Status = inb_Voucher.Status,
                                                            Site = inb_Voucher.Site,
                                                            Article_No = inb_Voucher.Article_No,
                                                            Bonus_Buy = inb_Voucher.Bonus_Buy,
                                                            POSNo = inb_Voucher.POSNo,
                                                            ReceiptNo = inb_Voucher.ReceiptNo,
                                                            TranDate = inb_Voucher.TranDate,
                                                            TranTime = inb_Voucher.TranTime,
                                                            FileName = inb_Voucher.SerialNo
                                                        };
                                                        string insertSql = @"INSERT INTO INB_VoucherToSAP 
                                        (Voucher_Type, SerialNo, Voucher_Value, Voucher_Currency, Validity_From_Date, Expiry_Date, 
                                        Processing_Type, Status, Site, Article_No, Bonus_Buy, POSNo, ReceiptNo, TranDate, TranTime, FileName)
                                        VALUES
                                        (@Voucher_Type, @SerialNo, @Voucher_Value, @Voucher_Currency, @Validity_From_Date, @Expiry_Date,
                                        @Processing_Type, @Status, @Site, @Article_No, @Bonus_Buy, @POSNo, @ReceiptNo, @TranDate, @TranTime, @FileName)";
                                                        int rowsAffected = connection.Execute(insertSql, inbVoucherSap);
                                                        if (rowsAffected > 0)
                                                        {
                                                            _logger.Information("Data inserted successfully!" + rowsAffected + "Row");
                                                        }
                                                        string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                                        int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inbVoucherSap.SerialNo });

                                                        if (rowsAffectedupdate > 0)
                                                        {
                                                            _logger.Information("Data Updated successfully!" + rowsAffectedupdate + " Row");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger.Information("Result API : " + calResult);
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.Information("Không có Data ");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.Error("Loi: Connect DB ", e.Message);

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error("Loi: Connect DB", e.Message);
                        }
                        break;
                    case "PRD_CARStockBalance":
                        _logger.Information("------------------------------------------------------");
                        _logger.Information("Run PRD_CARStockBalance");
                        ReadFile readfilSAP = new ReadFile(_logger);
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
                                SftpHelper sftpHelper = new SftpHelper(IpSftp, Host, username, password, _logger);
                                _logger.Information("Bắt đầu tải CARStockBalance : Call sftpHelper.DownloadAuthen ");
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
                            _logger.Information("Copy file Process");
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
                                    _logger.Information($"Copy file : {filteredStrings.Length}");
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
                                _logger.Information("Không có file Copy");
                            }
                            _logger.Information("Upload File Stock VinID");
                            SftpHelper sftpHelperup = new SftpHelper(IpSftpvinid, Hostvinid, usernamevinid, passwordvinid, _logger);
                            sftpHelperup.UploadSftpLinux2(pathLocalfile, pathuploadfile, processedFoldervinid, "*.CSV");

                            _logger.Information("Bắt đầu đọc CARStockBalance: " + pathLocalCar);
                            if (Directory.Exists(pathLocalCar))
                            {
                                string[] xmlFilester = filteredStrings.Where(str => str.Contains("PRD_VINID")).ToArray();
                                var count = 0;
                                int countsl = xmlFilester.Length;
                                _logger.Information("Tổng File :" + countsl);
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
                                        _logger.Information("(Job CARStockBalance)- Số File : " + count.ToString() + " Insert thành công");
                                    }
                                    else
                                    {
                                        foreach (string xmlFile1 in filteredStrings)
                                        {
                                            readfilSAP.ProcessCSV_CARStockBalance(xmlFile1, processedFoldercar);
                                            count++;
                                        }
                                        _logger.Information("(Job CARStockBalance)- Số File : " + count.ToString() + " Insert thành công");
                                    }
                                    string[] getfile = Directory.GetFiles(pathLocalCar, "*.CSV");
                                    int fileLoi = getfile.Length;
                                    if (fileLoi > 0)
                                    {
                                        _logger.Information("(Job CARStockBalance) Có : " + fileLoi + " file Lỗi Vui Lòng Kiểm tra ở Folder " + pathLocalCar);
                                    }
                                    DataSqlProcedure dataSql = new DataSqlProcedure(_logger);
                                    if (fileLoi == 0)
                                    {
                                        dataSql.Insert_TK_CarStockBalance();
                                    }
                                    else
                                    {
                                        _logger.Information("Còn " + fileLoi + " Chưa đọc hết chưa run được procedure");
                                    }
                                }
                                else
                                {
                                    _logger.Information("Không Có Data");
                                }
                            }
                            else
                            {
                                _logger.Information("File Not Found");
                                Directory.CreateDirectory(pathLocalCar);
                            }
                        }
                        else
                        {
                            _logger.Information("Chưa khai báo Host");
                        }
                        break;
                    case "GCP":
                        _logger.Information("------------------------------------------------------");
                        _logger.Information("Run GCP");
                        var configPLH = db.ConfigConnections.ToList().Where(p => p.Type == "PLH_INBOUND" && p.Status == true);//config DB
                        if (configPLH.Count() > 0)
                        {
                            PLH_To_GCP PLH_To_GCPs = new PLH_To_GCP(_logger);
                            foreach (var cfig in configPLH)
                            {
                                _logger.Information($"Connect DB: {cfig.Name}");
                                var listOrder = PLH_To_GCPs.OrderExpToGCPAsync(cfig.ConnectString);//listOrder
                                if (listOrder.Count > 0)
                                {
                                    _logger.Information($"Send Data To API: {listOrder.Count} Row");
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
                                            _logger.Information($"Response API: {response.StatusCode}");
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
                            _logger.Information("Staus đang Off or chưa khai báo Connections type = PLH_INBOUND");
                        }
                        break;

                    case "GCP_ARCHIVE":
                        _logger.Information("Run GCP_ARCHIVE");
                        var configPLH_Ar = db.ConfigConnections.ToList().Where(p => p.Type == "PLH_INBOUNDArchive" && p.Status == true);//config DB
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
                                            string json = JsonConvert.SerializeObject(listOrder);
                                            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                                            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                                            request.Content = content;
                                            HttpResponseMessage response = await httpClient.SendAsync(request);
                                            // HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                                            _logger.Information($"Response API: {response.StatusCode}");
                                            PLH_To_GCP PLH_To_GCPs = new PLH_To_GCP(_logger);
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

                    default:
                        _logger.Information("Invalid function name.");
                        break;
                }
            }
            else
            {
                _logger.Information("Please provide a function name as an argument.");
            }
        }
    }
}

