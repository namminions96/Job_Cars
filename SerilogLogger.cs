
using Serilog;

namespace BluePosVoucher
{
    public class SerilogLogger
    {

        public static ILogger GetLogger()
        {
            return new LoggerConfiguration() 
                .MinimumLevel.Information()
                .WriteTo.File("LogDetailsFile/LOG.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }

        public static ILogger GetLogger_WCM()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogDetailsFile_WCM/LOG.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }
        public static ILogger GetLogger_VinID()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogDetailsFile_VINID/LOG.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }
        public static ILogger GetLogger_PLH()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogDetailsFile_PLH/LOG.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }
        public static ILogger GetLogger_HR()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogDetailsFile_HR/LOG.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }
        public static ILogger GetLogger_Job()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("LogDetailsFile_Job/LOG.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }
    }
}
