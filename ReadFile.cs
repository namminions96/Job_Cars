using BluePosVoucher.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Job_By_SAP.Data;
using Job_By_SAP.Models;
using Job_By_SAP.PLH;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;
using System.Globalization;
using System.Text;

namespace Read_xml
{
    public class ReadFile
    {
        private readonly ILogger _logger;
        public ReadFile(ILogger logger)
        {
            _logger = logger;
        }

        public void ProcessCSV_CARStockBalance(string csvFile, string processedFolderPathter)
        {
            try
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(csvFile);
                using (var dbContext = new DbStaging_Inventory())
                {
                    using (var reader = new StreamReader(csvFile))
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = "|",
                        HasHeaderRecord = true,
                    }))

                    {
                        string[] lines = File.ReadAllLines(csvFile);
                        CARStockBalance cARStock = new CARStockBalance();
                        int count = 0;
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            string[] data = line.Split('|');
                            cARStock.Id = new Guid();
                            cARStock.Status = 0;
                            cARStock.TimeStamp = data[0];
                            cARStock.Site = data[1];
                            cARStock.ArticleNumber = data[2];
                            cARStock.MCH5 = data[3];
                            cARStock.BaseUoM = data[4];
                            cARStock.UnreUseQty = data[5];
                            cARStock.UnreConsQty = data[6];
                            cARStock.TransitQty = data[7];
                            cARStock.UnprSaleQty = RemoveCommas(data[8]);
                            cARStock.FileName = fileName;
                            cARStock.Created = DateTime.Now;

                            //if (string.IsNullOrEmpty(cARStock.UnreUseQty))
                            //{
                            //    cARStock.UnreUseQty = "0";
                            //}

                            //if (string.IsNullOrEmpty(cARStock.UnreConsQty))
                            //{
                            //    cARStock.UnreConsQty = "0";
                            //}

                            //if (string.IsNullOrEmpty(cARStock.TransitQty))
                            //{
                            //    cARStock.TransitQty = "0";
                            //}

                            //if (string.IsNullOrEmpty(cARStock.UnprSaleQty))
                            //{
                            //    cARStock.UnprSaleQty = "0";
                            //}

                            dbContext.CARStockBalances.Add(cARStock);
                            dbContext.SaveChanges();
                            count++;
                        }
                    }
                }
                if (Directory.Exists(processedFolderPathter))
                {
                    string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }
                    File.Move(csvFile, destinationPath);
                }
                else
                {
                    Directory.CreateDirectory(processedFolderPathter);
                    string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }
                    File.Move(csvFile, destinationPath);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Lỗi ProcessCSV_CARStockBalance");
            }
        }
        public void ProcessCSV_GCP_Sale_Retry(string csvFile, string processedFolderPathter, string configdb)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(configdb);
                string databaseName = builder.InitialCatalog;
                List<Receipt_Retry> receiptData = new List<Receipt_Retry>();
                using (SqlConnection DBINBOUND = new SqlConnection(configdb))
                {
                    DBINBOUND.Open();
                    using (var reader = new StreamReader(csvFile))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        var records = csv.GetRecords<dynamic>().ToList();
                        foreach (var record in records)
                        {
                            var receipt = new Receipt_Retry
                            {
                                OrderNo = record.RECEIPT_NO,
                                UpdateFlg = "N",
                                CrtDate = DateTime.Now,
                            };
                            receiptData.Add(receipt);
                        }
                    }
                    if (receiptData.Count > 0)
                    {
                        string DeleteCommand = $"Delete Temp_SalesGCP_Retry";
                        int rowsupdate = DBINBOUND.Execute(DeleteCommand);
                        int rowsAffected = DBINBOUND.Execute(PLH_Data.InsertTemp_SalesGCP_Retry(), receiptData);
                    }
                    if (Directory.Exists(processedFolderPathter))
                    {
                        string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Move(csvFile, destinationPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(processedFolderPathter);
                        string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Move(csvFile, destinationPath);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Lỗi ProcessCSV_GCP_Sale_Retry");
            }
        }


        public void ProcessCSV_GCP_Sale_Retry_WCM(string csvFile, string processedFolderPathter, string configdb)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(configdb);
                string databaseName = builder.InitialCatalog;
                List<Receipt_Retry_WCM> receiptData = new List<Receipt_Retry_WCM>();
                using (SqlConnection DBINBOUND = new SqlConnection(configdb))
                {
                    DBINBOUND.Open();
                    using (var reader = new StreamReader(csvFile))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        var records = csv.GetRecords<dynamic>().ToList();
                        foreach (var record in records)
                        {
                            var receipt = new Receipt_Retry_WCM
                            {
                                RECEIPT_NO = record.RECEIPT_NO,
                                UpdateFlg = "N",
                                CrtDate = DateTime.Now,
                            };
                            receiptData.Add(receipt);
                        }
                    }
                    if (receiptData.Count > 0)
                    {
                        string DeleteCommand = $"Delete Temp_SalesGCP_Retry";
                        int rowsupdate = DBINBOUND.Execute(DeleteCommand);
                        int rowsAffected = DBINBOUND.Execute(PLH_Data.InsertTemp_SalesGCP_Retry_WCM(), receiptData);
                    }
                    if (Directory.Exists(processedFolderPathter))
                    {
                        string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Move(csvFile, destinationPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(processedFolderPathter);
                        string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Move(csvFile, destinationPath);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Lỗi ProcessCSV_GCP_Sale_Retry");
            }
        }

        public string RemoveCommas(string input)
        {
            while (input.EndsWith(",,") || input.EndsWith(","))
            {
                input = input.Substring(0, input.Length - 1);
            }

            return input;
        }
        public string ConvertSQLtoXML(string connectionString, string SearchBy, string StartDate,
                                      string EndDate, string Taxcode, string Branch, string Serial)
        {
            StringBuilder xml = new StringBuilder();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int timeout = 1000;
                connection.Open();
                using (SqlCommand command = new SqlCommand("vcm_get_list_einvoice_report_exp_Nam", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 1000;
                    command.Parameters.AddWithValue("@FromDate", StartDate);
                    command.Parameters.AddWithValue("@ToDate", EndDate);
                    command.Parameters.AddWithValue("@SearchBy", SearchBy);
                    command.Parameters.AddWithValue("@Branch", Branch); // Giá trị null hoặc trống
                    command.Parameters.AddWithValue("@Taxcode", Taxcode);//"0104918404"
                    command.Parameters.AddWithValue("@Serial", Serial); // Giá trị null hoặc trống                     
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // Kiểm tra nếu có bản ghi để đọc
                        {
                            xml.AppendLine("<n0:MAPPING xmlns:n0=\"urn:Vincommerce:BLUEPOS_FPT:To:BW:Tax_Reconcicle\">");

                            while (reader.Read())
                            {
                                xml.AppendLine("<CONTENT>");

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string fieldName = reader.GetName(i);
                                    string fieldValue = reader[i].ToString();
                                    if (fieldName.EndsWith("REV_AMT_WO_TAX"))
                                    {
                                        fieldValue = fieldValue.Replace(",", ".");
                                    }
                                    if (fieldName.EndsWith("TAX_AMOUNT"))
                                    {
                                        fieldValue = fieldValue.Replace(",", ".");
                                    }
                                    xml.AppendLine($"<{fieldName}>{fieldValue}</{fieldName}>");
                                }
                                xml.AppendLine("</CONTENT>");
                            }
                            xml.AppendLine("</n0:MAPPING>");
                        }
                        else
                        {
                            _logger.Information($"Không Có Data Type  : {SearchBy}");
                            return null;
                        }
                    }
                }
            }

            return xml.ToString();
        }

        public string ConvertSQLtoXMLRetry(string connectionString, string SearchBy)
        {
            StringBuilder xml = new StringBuilder();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("vcm_get_list_einvoice_report_exp_Retry", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchBy", SearchBy);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // Kiểm tra nếu có bản ghi để đọc
                        {
                            xml.AppendLine("<n0:MAPPING xmlns:n0=\"urn:Vincommerce:BLUEPOS_FPT:To:BW:Tax_Reconcicle\">");

                            while (reader.Read())
                            {
                                xml.AppendLine("<CONTENT>");

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string fieldName = reader.GetName(i);
                                    string fieldValue = reader[i].ToString();
                                    if (fieldName.EndsWith("REV_AMT_WO_TAX"))
                                    {
                                        fieldValue = fieldValue.Replace(",", ".");
                                    }
                                    if (fieldName.EndsWith("TAX_AMOUNT"))
                                    {
                                        fieldValue = fieldValue.Replace(",", ".");
                                    }
                                    xml.AppendLine($"<{fieldName}>{fieldValue}</{fieldName}>");
                                }
                                xml.AppendLine("</CONTENT>");
                            }
                            xml.AppendLine("</n0:MAPPING>");
                        }
                        else
                        {
                            _logger.Information($"Không Có Data Type : {SearchBy}");
                            return null;
                        }
                    }
                }
            }

            return xml.ToString();
        }

        public void ConvertSQLtoXML_CSV_PLH(string connectionString,string query)
        {
            StringBuilder xml = new StringBuilder();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int timeout = 1000;
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 1000;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // Kiểm tra nếu có bản ghi để đọc
                        {
                            var currentDatepathxml = DateTime.Now;
                            string currentDatepath = currentDatepathxml.ToString("yyyyMMddHHmmss");
                            string outputFilePathPos = @$"TransPoint_Reconcile\TransPoint_Reconcile_{currentDatepath}.csv";
                            using (var writer = new StreamWriter(outputFilePathPos, false, Encoding.UTF8))
                            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    csv.WriteField(reader.GetName(i));
                                }
                                csv.NextRecord();

                                // Ghi dữ liệu từ SqlDataReader vào file CSV
                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        csv.WriteField(reader[i]);
                                    }
                                    csv.NextRecord();
                                }
                                _logger.Information("Done");
                            }
                        }
                        else
                        {
                            _logger.Information("Không Có Data");
                        }
                    }
                }
            }

        }

    }
}

