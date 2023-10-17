
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
    }
}
