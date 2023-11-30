using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP
{
    public class DeleteFileArchive
    {
        private readonly ILogger _logger;
        public DeleteFileArchive( ILogger logger)
        {
            _logger = logger;
        }
        public void DeleteFileAr(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath);
            int count = 0;
            if (files.Length > 0)
            {
                DateTime currentDate = DateTime.Now;
                int daysToKeep = 3;
                foreach (string filePath in files)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    TimeSpan timeSinceCreation = currentDate - fileInfo.CreationTime;
                    if (timeSinceCreation.Days >= daysToKeep)
                    {
                        try
                        {
                            count++;
                            File.Delete(filePath);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Lỗi khi xóa tệp {filePath}: {e.Message}");
                        }
                    }
                }
                _logger.Information($"Đã Xóa tổng {count} File ");
            }
            else
            {
                _logger.Information("Không có file để xóa");
            }
        }
    }
}
