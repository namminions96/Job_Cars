
using Serilog;

namespace BluePosVoucher
{
    public class SerilogLogger
    {

        public static ILogger GetLogger()
        {
            return new LoggerConfiguration() 
                .MinimumLevel.Information()
                .WriteTo.File("LogFile/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }

        public static ILogger GetLogger_WCM()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogFile_WCM/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_VinID()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogFile_VINID/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_PLH()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogFile_PLH/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_HR()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogFile_HR/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_Job()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogFile_Job/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_VC()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogFile_VC/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_Einvoice()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogFile_Einvoice/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_DeleteFile()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Log_DeleteFile/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }

        public static ILogger GetLogger_WCM_Void()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Log_Void_GCP/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_WPH_Survey()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Log_WPH_Survey/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
        public static ILogger GetLogger_PLH_WF()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Log_PLH_WF/LOG.txt", rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
                .CreateLogger();
        }
    }
}
