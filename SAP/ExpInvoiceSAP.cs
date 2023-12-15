using BluePosVoucher;
using BluePosVoucher.Data;
using Read_xml;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.SAP
{
    public class ExpInvoiceSAP
    {
        private readonly ILogger _logger_Einvoice;
        public ExpInvoiceSAP(ILogger logger)
        {
            _logger_Einvoice = logger;
        }
        public void ExpInvoiceSAPXML(string Name)
        {
            using (var db = new DbConfigAll())
            {
                SendEmailExample sendEmailExample = new SendEmailExample(_logger_Einvoice);
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
                        if (splitValues.Length >= 4)
                        {
                            string lastdate = splitValues[0];
                            int intValue;
                            int.TryParse(lastdate, out intValue);
                            string firstValuePOS = splitValues[1];
                            string secondValueSAP = splitValues[2];
                            string ValueCancel = splitValues[3];
                            string Reconcile = splitValues[4];
                            //---------------------------------------------//
                            var currentDatexml = DateTime.Today;
                            var previousDate = currentDatexml.AddDays(-intValue);
                            string StartDateString = previousDate.ToString("yyyy-MM-dd");
                            string EndDateString = previousDate.ToString("yyyy-MM-dd");

                            if (firstValuePOS == "1")
                            {
                                var resultxmlPOS = ExportXML.ConvertSQLtoXML(connections.ConnectString, "1", StartDateString, EndDateString, "0104918404", "", "");
                                string outputFilePathPos = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_POS_{currentDatepath}.xml";
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
                                _logger_Einvoice.Information($"Tạo file POS OFF ");
                               
                            }
                            if (secondValueSAP == "2")
                            {
                                var resultxmlSAP = ExportXML.ConvertSQLtoXML(connections.ConnectString, "2", StartDateString, EndDateString, "0104918404", "", "");
                                string outputFilePathSAP = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_SAP_{currentDatepath}.xml";
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
                                _logger_Einvoice.Information($"Tạo file SAP OFF");
                               
                            }
                            if (ValueCancel == "3")
                            {
                                DateTime startDate = new DateTime(previousDate.Year, previousDate.Month, 1);
                                string StartDateCancel = startDate.ToString("yyyy-MM-dd");
                                string EndDateCancel = currentDatexml.ToString("yyyy-MM-dd");
                                var resultxmlCancel = ExportXML.ConvertSQLtoXML(connections.ConnectString, "3", StartDateCancel, EndDateCancel, "0104918404", "", "");
                                string outputFilePathCancel = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_POS_CANCEL_{currentDatepath}.xml";
                                if (resultxmlCancel != null)
                                {
                                    using (StreamWriter writer = new StreamWriter(outputFilePathCancel))
                                    {
                                        writer.Write(resultxmlCancel.ToString());
                                        //  _logger_Einvoice.Information($"Tạo File TAX_RECONCILE_NOSTORE_{currentDatepath}.xml Done");
                                    }
                                }
                                var resultxmlCancelSAP = ExportXML.ConvertSQLtoXML(connections.ConnectString, "4", StartDateCancel, EndDateCancel, "0104918404", "", "");
                                string outputFilePathCancelSAP = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_SAP_CANCEL_{currentDatepath}.xml";
                                if (resultxmlCancel != null)
                                {
                                    using (StreamWriter writer = new StreamWriter(outputFilePathCancelSAP))
                                    {
                                        writer.Write(resultxmlCancelSAP.ToString());
                                        //  _logger_Einvoice.Information($"Tạo File TAX_RECONCILE_NOSTORE_{currentDatepath}.xml Done");
                                    }
                                }

                            }
                            else
                            {
                                _logger_Einvoice.Information($" Tạo file Cancel Off");
                                
                            }
                            if (Reconcile == "4")
                            {
                                string RetryStore = "1";
                                //DateTime startDate = new DateTime(previousDate.Year, previousDate.Month, 1);
                                //string StartDateRetry = startDate.ToString("yyyy-MM-dd");
                                //string EndDateRetry = currentDatexml.ToString("yyyy-MM-dd");
                                var resultxmlStore = ExportXML.ConvertSQLtoXMLRetry(connections.ConnectString, RetryStore);
                                string outputFilePathRetry = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_POS_RETRY_{currentDatepath}.xml";
                                if (resultxmlStore != null)
                                {
                                    using (StreamWriter writer = new StreamWriter(outputFilePathRetry))
                                    {
                                        writer.Write(resultxmlStore.ToString());
                                        //  _logger_Einvoice.Information($"Tạo File TAX_RECONCILE_NOSTORE_{currentDatepath}.xml Done");
                                    }
                                }
                                string RetryNostore = "2";
                                var resultxmlNostore = ExportXML.ConvertSQLtoXMLRetry(connections.ConnectString, RetryNostore);
                                string outputFilePathRetryNostore = @$"{configXml.LocalFoderPath}\TAX_RECONCILE_SAP_RETRY_{currentDatepath}.xml";
                                if (resultxmlNostore != null)
                                {
                                    using (StreamWriter writer = new StreamWriter(outputFilePathRetryNostore))
                                    {
                                        writer.Write(resultxmlNostore.ToString());
                                        //  _logger_Einvoice.Information($"Tạo File TAX_RECONCILE_NOSTORE_{currentDatepath}.xml Done");
                                    }
                                }

                            }
                            else
                            {
                                _logger_Einvoice.Information($"Tạo file Retry Off ");
                               
                            }

                        }
                        else
                        {
                            _logger_Einvoice.Information($"Chưa Khai Báo Time Run định Dạng  2;1;2;3;4  :: (2) là Getdate- so ngay, 1 : Tạo file Pos, 2 : Tạo File SAP, 3: Send file Cancel, 4 : Send file Retry ");
                            sendEmailExample.SendMailError($"Chưa Khai Báo Time Run định Dạng  2;1;2;3;4  :: (2) là Getdate- so ngay, 1 : Tạo file Pos, 2 : Tạo File SAP, 3: Send file Cancel, 4 : Send file Retry ");
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
                            _logger_Einvoice.Information("Không có file Copy");
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
                                sendEmailExample.SendMailError("Chưa khai báo Upload file config ExpEinvoice");
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
                        sendEmailExample.SendMailError("Chưa khai báo đủ connection or config");
                    }
                }
                catch (Exception ex)
                {
                    _logger_Einvoice.Information(ex.Message);
                    sendEmailExample.SendMailError(ex.Message);
                }
            }
        }
    }
}
