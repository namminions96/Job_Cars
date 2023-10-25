using Read_xml.Data;
using Read_xml.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Job_By_SAP
{
    public class ReadFileHR
    {
        private readonly ILogger _logger;
        public ReadFileHR(ILogger logger)
        {
            _logger = logger;
        }
        public void ProcessXmlFileDbdashboard(string xmlFile, string processedFolderPathter)
        {
            try
            {
                using (var dbContext = new Dbhrcontext())
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    using (FileStream fileStream = new FileStream(xmlFile, FileMode.Open))
                    {
                        xmlDoc.Load(fileStream);
                    }
                    string[] pathParts = xmlFile.Split('\\');
                    string fileName = pathParts[pathParts.Length - 1];
                    XmlNodeList bookNodes = xmlDoc.GetElementsByTagName("Items");
                    HR_Dashboard models_Xml = new HR_Dashboard();
                    foreach (XmlNode bookNode in bookNodes)
                    {
                        models_Xml.Id = new Guid();
                        models_Xml.YEAR = bookNode.SelectSingleNode("YEAR").InnerText;
                        models_Xml.MONTH = bookNode.SelectSingleNode("MONTH").InnerText;
                        models_Xml.KEY_DATE = bookNode.SelectSingleNode("KEY_DATE").InnerText;
                        models_Xml.PERNR = bookNode.SelectSingleNode("PERNR").InnerText;
                        models_Xml.FULLNAME = bookNode.SelectSingleNode("FULLNAME").InnerText;
                        models_Xml.SEX = bookNode.SelectSingleNode("SEX").InnerText;
                        models_Xml.DOB = bookNode.SelectSingleNode("DOB").InnerText;
                        models_Xml.AGE = bookNode.SelectSingleNode("AGE").InnerText;
                        models_Xml.BU = bookNode.SelectSingleNode("BU").InnerText;
                        models_Xml.ENTITY = bookNode.SelectSingleNode("ENTITY").InnerText;
                        models_Xml.DEPARTMENT = bookNode.SelectSingleNode("DEPARTMENT").InnerText;
                        models_Xml.POSITION = bookNode.SelectSingleNode("POSITION").InnerText;
                        models_Xml.RANK = bookNode.SelectSingleNode("RANK").InnerText;
                        models_Xml.RANK_GROUP = bookNode.SelectSingleNode("RANK_GROUP").InnerText;
                        models_Xml.FUNCTION = bookNode.SelectSingleNode("FUNCTION").InnerText;
                        models_Xml.FUNCTION_GROUP = bookNode.SelectSingleNode("FUNCTION_GROUP").InnerText;
                        models_Xml.MAKE = bookNode.SelectSingleNode("MAKE").InnerText;
                        models_Xml.ONBOA_DATE = bookNode.SelectSingleNode("ONBOA_DATE").InnerText;
                        models_Xml.WORK_PLACE = bookNode.SelectSingleNode("WORK_PLACE").InnerText;
                        models_Xml.CONTRACT = bookNode.SelectSingleNode("CONTRACT").InnerText;
                        models_Xml.SENIORITY = bookNode.SelectSingleNode("SENIORITY").InnerText;
                        models_Xml.DIRECT = bookNode.SelectSingleNode("DIRECT").InnerText;
                        models_Xml.FILENAME = fileName;
                        dbContext.HR_Dashboards.Add(models_Xml);
                        dbContext.SaveChanges();
                    }
                    if (Directory.Exists(processedFolderPathter))
                    {
                        string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(xmlFile));
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Move(xmlFile, destinationPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(processedFolderPathter);
                        string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(xmlFile));
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Move(xmlFile, destinationPath);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Lỗi ProcessXmlFileDbdashboard");

            }
        }
        public void ProcessXmlFileHR_Terninate(string xmlFile, string processedFolderPathter)
        {
            try
            {
                using (var dbContext = new Dbhrcontext())
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    using (FileStream fileStream = new FileStream(xmlFile, FileMode.Open))
                    {
                        xmlDoc.Load(fileStream);
                    }
                    string[] pathParts = xmlFile.Split('\\');
                    string fileName = pathParts[pathParts.Length - 1];
                    XmlNodeList bookNodes = xmlDoc.GetElementsByTagName("Items");
                    HR_Terninate models_Xml = new HR_Terninate();
                    foreach (XmlNode bookNode in bookNodes)
                    {
                        models_Xml.Id = new Guid();
                        models_Xml.YEAR = bookNode.SelectSingleNode("YEAR").InnerText;
                        models_Xml.START_DATE = bookNode.SelectSingleNode("START_DATE").InnerText;
                        models_Xml.END_DATE = bookNode.SelectSingleNode("END_DATE").InnerText;
                        models_Xml.BU = bookNode.SelectSingleNode("BU").InnerText;
                        models_Xml.PERNR = bookNode.SelectSingleNode("PERNR").InnerText;
                        models_Xml.FULLNAME = bookNode.SelectSingleNode("FULLNAME").InnerText;
                        models_Xml.SEX = bookNode.SelectSingleNode("SEX").InnerText;
                        models_Xml.DOB = bookNode.SelectSingleNode("DOB").InnerText;
                        models_Xml.AGE = bookNode.SelectSingleNode("AGE").InnerText;
                        models_Xml.ENTITY = bookNode.SelectSingleNode("ENTITY").InnerText;
                        models_Xml.DEPARTMENT = bookNode.SelectSingleNode("DEPARTMENT").InnerText;
                        models_Xml.TITLE = bookNode.SelectSingleNode("TITLE").InnerText;
                        models_Xml.RANK = bookNode.SelectSingleNode("RANK").InnerText;
                        models_Xml.FUNCTION = bookNode.SelectSingleNode("FUNCTION").InnerText;
                        models_Xml.STAFF = bookNode.SelectSingleNode("STAFF").InnerText;
                        models_Xml.TERMI_DATE = bookNode.SelectSingleNode("TERMI_DATE").InnerText;
                        models_Xml.ONBOA_DATE = bookNode.SelectSingleNode("ONBOA_DATE").InnerText;
                        models_Xml.CONTRACT = bookNode.SelectSingleNode("CONTRACT").InnerText;
                        models_Xml.W_MONTH = bookNode.SelectSingleNode("W_MONTH").InnerText;
                        models_Xml.W_YEAR = bookNode.SelectSingleNode("W_YEAR").InnerText;
                        models_Xml.NATIO = bookNode.SelectSingleNode("NATIO").InnerText;
                        models_Xml.TERMI_REASON = bookNode.SelectSingleNode("TERMI_REASON").InnerText;
                        models_Xml.DIRECT = bookNode.SelectSingleNode("DIRECT").InnerText;
                        models_Xml.TURNOVER = bookNode.SelectSingleNode("TURNOVER").InnerText;
                        models_Xml.VOLUNTARY = bookNode.SelectSingleNode("VOLUNTARY").InnerText;
                        models_Xml.FILENAME = fileName;
                        dbContext.HR_Terninates.Add(models_Xml);
                        dbContext.SaveChanges();
                    }
                }
                if (Directory.Exists(processedFolderPathter))
                {
                    string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(xmlFile));
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }
                    File.Move(xmlFile, destinationPath);
                }
                else
                {
                    Directory.CreateDirectory(processedFolderPathter);
                    string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(xmlFile));
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }
                    File.Move(xmlFile, destinationPath);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Lỗi ProcessXmlFileHR_Terninate");
            }
        }
    }
}


//----------------------------------------------------------------------------------------------------------------
//public void ProcessXmlPRD_SellingPrice(string xmlFile, string processedFolderPathter)
//{
//    try
//    {
//        using (var dbContext = new Dbhrcontext())
//        {
//            XmlDocument xmlDoc = new XmlDocument();
//            using (FileStream fileStream = new FileStream(xmlFile, FileMode.Open))
//            {
//                xmlDoc.Load(fileStream);
//            }
//            XmlNodeList bookNodes = xmlDoc.GetElementsByTagName("SellingPrice");
//            PRD_SellingPrice models_Xml = new PRD_SellingPrice();
//            foreach (XmlNode bookNode in bookNodes)
//            {
//                models_Xml.ID = new Guid();
//                models_Xml.WarehouseCode = bookNode.SelectSingleNode("WarehouseCode").InnerText;
//                models_Xml.MerchantCode = bookNode.SelectSingleNode("MerchantCode").InnerText;
//                models_Xml.MerchantSku = bookNode.SelectSingleNode("MerchantSku").InnerText;
//                models_Xml.SellPrice = bookNode.SelectSingleNode("SellPrice").InnerText;
//                models_Xml.PriceFrom = bookNode.SelectSingleNode("PriceFrom").InnerText;
//                models_Xml.PriceTo = bookNode.SelectSingleNode("PriceTo").InnerText;
//                models_Xml.Delete_Indicator = bookNode.SelectSingleNode("Delete_Indicator").InnerText;
//                models_Xml.OldMerchantSku = bookNode.SelectSingleNode("OldMerchantSku").InnerText;
//                models_Xml.UoM = bookNode.SelectSingleNode("UoM").InnerText;
//                models_Xml.Barcode = bookNode.SelectSingleNode("Barcode").InnerText;
//                models_Xml.ConditionType = bookNode.SelectSingleNode("ConditionType").InnerText;
//                models_Xml.Promo_MD_Ind = bookNode.SelectSingleNode("Promo_MD_Ind").InnerText;
//                models_Xml.PurchasingGroup = bookNode.SelectSingleNode("PurchasingGroup").InnerText;
//                models_Xml.SupplyRegion = bookNode.SelectSingleNode("SupplyRegion").InnerText;
//                models_Xml.CondTable = bookNode.SelectSingleNode("CondTable").InnerText;
//                models_Xml.Vendor = bookNode.SelectSingleNode("Vendor").InnerText;
//                models_Xml.VendorSubrange = bookNode.SelectSingleNode("VendorSubrange").InnerText;
//                models_Xml.SiteRegion = bookNode.SelectSingleNode("SiteRegion").InnerText;
//                models_Xml.InfoType = bookNode.SelectSingleNode("InfoType").InnerText;
//                models_Xml.PurchasingOrg = bookNode.SelectSingleNode("PurchasingOrg").InnerText;
//                models_Xml.PurchaseUnit = bookNode.SelectSingleNode("PurchaseUnit").InnerText;
//                models_Xml.Customer = bookNode.SelectSingleNode("Customer").InnerText;
//                models_Xml.CustomerGroup = bookNode.SelectSingleNode("CustomerGroup").InnerText;
//                models_Xml.Promotion = bookNode.SelectSingleNode("Promotion").InnerText;
//                dbContext.PRD_SellingPrices.Add(models_Xml);
//                dbContext.SaveChanges();
//            }
//        }
//        if (Directory.Exists(processedFolderPathter))
//        {
//            string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(xmlFile));
//            if (File.Exists(destinationPath))
//            {
//                File.Delete(destinationPath);
//            }
//            File.Move(xmlFile, destinationPath);
//        }
//        else
//        {
//            Directory.CreateDirectory(processedFolderPathter);
//            string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(xmlFile));
//            if (File.Exists(destinationPath))
//            {
//                File.Delete(destinationPath);
//            }
//            File.Move(xmlFile, destinationPath);
//        }
//    }
//    catch (Exception e)
//    {
//        _logger.Error(e, "Lỗi ProcessXmlPRD_SellingPrice");
//    }
//}


//public static int[] ConvertStringToIntArray(string inputString)
//{
//    string[] stringArray = inputString.Split(',');
//    int[] integerArray = new int[stringArray.Length];

//    for (int i = 0; i < stringArray.Length; i++)
//    {
//        if (int.TryParse(stringArray[i], out int intValue))
//        {
//            integerArray[i] = intValue;
//        }
//        else
//        {
//            throw new ArgumentException("Invalid input format. Failed to convert to an integer.");
//        }
//    }
//    return integerArray;
//}
//public static int ExtractHourFromDate(string dateString)
//{
//    string hourString = dateString.Substring(11, 2);
//    int hour = int.Parse(hourString);

//    return hour;
//}
//public bool checkDataHR_Dashboards(string PERNR)
//{
//    var db = new Dbhrcontext();
//    var check = db.HR_Dashboards.Where(p => p.PERNR == PERNR);
//    if (check == null)
//    {
//        return true;
//    }
//    return false;
//}
//public bool checkDataHR_Terninats(string PERNR)
//{
//    var db = new Dbhrcontext();
//    var check = db.HR_Terninates.Where(p => p.PERNR == PERNR);
//    if (check == null)
//    {
//        return true;
//    }
//    return false;
//}


