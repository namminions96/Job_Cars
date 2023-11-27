using Dapper;
using Job_By_SAP.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Read_xml.Data;
using Renci.SshNet.Messages;
using Serilog;
using System.Data;

namespace Job_By_SAP
{
    public class Insert_HR_ALL
    {
        private readonly ILogger _logger;
        public Insert_HR_ALL(ILogger logger)
        {
            _logger = logger;
        }
        public void Insert_HR_All()
        {
            SendEmailExample sendEmailExample = new SendEmailExample(_logger);
            try
            {
                using (var db = new Dbhrcontext())
                {
                    var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                    string Procedures = configuration["ProceduresHR"];
                    _logger.Information("Run: Exec " + Procedures);
                    //var result = db.Messages.FromSqlRaw("Exec SP_INSERT_SALE_PRICE_ONLINE").ToList();
                    var timeoutSeconds = 10000;
                    db.Database.SetCommandTimeout(timeoutSeconds);
                    var result = db.Errors.FromSqlRaw(Procedures).ToList();
                    Error message = new Models.Error();
                    foreach (var error in result)
                    {
                        message.messages = error.messages;
                    }

                    _logger.Information("Run: Exec SP_INSERT_HR_ALL Data: " + message.messages);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi Exec :Exec Procedures ");
                sendEmailExample.SendMailError("SP_INSERT_HR_ALL: " + ex.Message);
            }
        }

        public void Insert_HR_All_PRD()
        {
            SendEmailExample sendEmailExample = new SendEmailExample(_logger);
            try
            {
                using (var db = new Dbhrcontext())
                {
                    var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                    string connectionString = configuration["connectionString"];
                    _logger.Information("Run: Exec SP_INSERT_HR_ALL_PRD");
                    //var result = db.Messages.FromSqlRaw("Exec SP_INSERT_SALE_PRICE_ONLINE").ToList();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        var timeout = 10000;
                        // Thực hiện truy vấn sử dụng Dapper
                        var results = connection.Query("SP_INSERT_HR_ALL_PRD", commandType: CommandType.StoredProcedure, commandTimeout: timeout);
                        _logger.Information("Run: Exec SP_INSERT_HR_ALL Data: OK " );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi Exec :Exec Procedures ");
                sendEmailExample.SendMailError("SP_INSERT_HR_ALL_PRD: " + ex.Message);
            }
        }

    }
}
