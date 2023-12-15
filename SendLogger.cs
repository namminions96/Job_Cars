using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP
{
    internal class SendLogger
    {
        public async Task SendKibanaAsync(string HttpContext, string PosNo, string WebApi, string DeveloperMessage, long ResponseTime)
        {
            try
            {
                string apiUrl = "https://apibluepos.winmart.vn/api/common/logging";

                // Dữ liệu để gửi đi dưới dạng JSON
                string jsonData = $@"{{
            ""HttpContext"": ""{HttpContext}"",
            ""PosNo"": ""{PosNo}"",
            ""WebApi"": ""{WebApi}"",
            ""DeveloperMessage"": ""{DeveloperMessage}"",
            ""ResponseTime"": {ResponseTime}
        }}";

                using (HttpClient client = new HttpClient())
                {
                    // Thêm thông tin xác thực cơ bản vào tiêu đề Authorization
                    string username = "BLUEOPS";
                    string password = "BluePos@123";
                    string authInfo = $"{username}:{password}";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);

                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Kiểm tra phản hồi từ API
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Phản hồi từ API: " + responseBody);
                    }
                    else
                    {
                        Console.WriteLine("Lỗi: " + response.StatusCode);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
