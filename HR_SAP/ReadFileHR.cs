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
                        XmlNode yearNode = bookNode.SelectSingleNode("YEAR");
                        models_Xml.YEAR = (yearNode != null) ? yearNode.InnerText : "";
                        XmlNode monthNode = bookNode.SelectSingleNode("MONTH");
                        models_Xml.MONTH = (monthNode != null) ? monthNode.InnerText : "";
                        XmlNode keyDateNode = bookNode.SelectSingleNode("KEY_DATE");
                        models_Xml.KEY_DATE = (keyDateNode != null) ? keyDateNode.InnerText : "";
                        XmlNode pernrNode = bookNode.SelectSingleNode("PERNR");
                        models_Xml.PERNR = (pernrNode != null) ? pernrNode.InnerText : "";
                        XmlNode fullNameNode = bookNode.SelectSingleNode("FULLNAME");
                        models_Xml.FULLNAME = (fullNameNode != null) ? fullNameNode.InnerText : "";
                        XmlNode sexNode = bookNode.SelectSingleNode("SEX");
                        models_Xml.SEX = (sexNode != null) ? sexNode.InnerText : "";
                        XmlNode dobNode = bookNode.SelectSingleNode("DOB");
                        models_Xml.DOB = (dobNode != null) ? dobNode.InnerText : "";
                        XmlNode ageNode = bookNode.SelectSingleNode("AGE");
                        models_Xml.AGE = (ageNode != null) ? ageNode.InnerText : "";
                        XmlNode buNode = bookNode.SelectSingleNode("BU");
                        models_Xml.BU = (buNode != null) ? buNode.InnerText : "";
                        XmlNode entityNode = bookNode.SelectSingleNode("ENTITY");
                        models_Xml.ENTITY = (entityNode != null) ? entityNode.InnerText : "";
                        XmlNode departmentNode = bookNode.SelectSingleNode("DEPARTMENT");
                        models_Xml.DEPARTMENT = (departmentNode != null) ? departmentNode.InnerText : "";
                        XmlNode positionNode = bookNode.SelectSingleNode("POSITION");
                        models_Xml.POSITION = (positionNode != null) ? positionNode.InnerText : "";
                        XmlNode rankNode = bookNode.SelectSingleNode("RANK");
                        models_Xml.RANK = (rankNode != null) ? rankNode.InnerText : "";
                        XmlNode rankGroupNode = bookNode.SelectSingleNode("RANK_GROUP");
                        models_Xml.RANK_GROUP = (rankGroupNode != null) ? rankGroupNode.InnerText : "";
                        XmlNode functionNode = bookNode.SelectSingleNode("FUNCTION");
                        models_Xml.FUNCTION = (functionNode != null) ? functionNode.InnerText : "";
                        XmlNode functionGroupNode = bookNode.SelectSingleNode("FUNCTION_GROUP");
                        models_Xml.FUNCTION_GROUP = (functionGroupNode != null) ? functionGroupNode.InnerText : "";
                        XmlNode makeNode = bookNode.SelectSingleNode("MAKE");
                        models_Xml.MAKE = (makeNode != null) ? makeNode.InnerText : "";
                        XmlNode onboaDateNode = bookNode.SelectSingleNode("ONBOA_DATE");
                        models_Xml.ONBOA_DATE = (onboaDateNode != null) ? onboaDateNode.InnerText : "";
                        XmlNode workPlaceNode = bookNode.SelectSingleNode("WORK_PLACE");
                        models_Xml.WORK_PLACE = (workPlaceNode != null) ? workPlaceNode.InnerText : "";
                        XmlNode contractNode = bookNode.SelectSingleNode("CONTRACT");
                        models_Xml.CONTRACT = (contractNode != null) ? contractNode.InnerText : "";
                        XmlNode edudegree = bookNode.SelectSingleNode("EDU_DEGREE");
                        models_Xml.EDU_DEGREE = (edudegree != null) ? edudegree.InnerText : "";
                        XmlNode agegroup = bookNode.SelectSingleNode("AGE_GROUP");
                        models_Xml.AGE_GROUP = (agegroup != null) ? agegroup.InnerText : "";
                        XmlNode seniorityNode = bookNode.SelectSingleNode("SENIORITY");
                        models_Xml.SENIORITY = (seniorityNode != null) ? seniorityNode.InnerText : "";
                        XmlNode directNode = bookNode.SelectSingleNode("DIRECT");
                        models_Xml.DIRECT = (directNode != null) ? directNode.InnerText : "";
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
                        XmlNode yearNode = bookNode.SelectSingleNode("YEAR");
                        models_Xml.YEAR = (yearNode != null) ? yearNode.InnerText : "";
                        XmlNode startDateNode = bookNode.SelectSingleNode("START_DATE");
                        models_Xml.START_DATE = (startDateNode != null) ? startDateNode.InnerText : "";
                        XmlNode endDateNode = bookNode.SelectSingleNode("END_DATE");
                        models_Xml.END_DATE = (endDateNode != null) ? endDateNode.InnerText : "";
                        XmlNode buNode = bookNode.SelectSingleNode("BU");
                        models_Xml.BU = (buNode != null) ? buNode.InnerText : "";
                        XmlNode pernrNode = bookNode.SelectSingleNode("PERNR");
                        models_Xml.PERNR = (pernrNode != null) ? pernrNode.InnerText : "";
                        XmlNode fullNameNode = bookNode.SelectSingleNode("FULLNAME");
                        models_Xml.FULLNAME = (fullNameNode != null) ? fullNameNode.InnerText : "";
                        XmlNode sexNode = bookNode.SelectSingleNode("SEX");
                        models_Xml.SEX = (sexNode != null) ? sexNode.InnerText : "";
                        XmlNode dobNode = bookNode.SelectSingleNode("DOB");
                        models_Xml.DOB = (dobNode != null) ? dobNode.InnerText : "";
                        XmlNode ageNode = bookNode.SelectSingleNode("AGE");
                        models_Xml.AGE = (ageNode != null) ? ageNode.InnerText : "";
                        XmlNode entityNode = bookNode.SelectSingleNode("ENTITY");
                        models_Xml.ENTITY = (entityNode != null) ? entityNode.InnerText : "";
                        XmlNode departmentNode = bookNode.SelectSingleNode("DEPARTMENT");
                        models_Xml.DEPARTMENT = (departmentNode != null) ? departmentNode.InnerText : "";
                        XmlNode titleNode = bookNode.SelectSingleNode("TITLE");
                        models_Xml.TITLE = (titleNode != null) ? titleNode.InnerText : "";
                        XmlNode rankNode = bookNode.SelectSingleNode("RANK");
                        models_Xml.RANK = (rankNode != null) ? rankNode.InnerText : "";
                        XmlNode functionNode = bookNode.SelectSingleNode("FUNCTION");
                        models_Xml.FUNCTION = (functionNode != null) ? functionNode.InnerText : "";
                        XmlNode staffNode = bookNode.SelectSingleNode("STAFF");
                        models_Xml.STAFF = (staffNode != null) ? staffNode.InnerText : "";
                        XmlNode termiDateNode = bookNode.SelectSingleNode("TERMI_DATE");
                        models_Xml.TERMI_DATE = (termiDateNode != null) ? termiDateNode.InnerText : "";
                        XmlNode onboaDateNode = bookNode.SelectSingleNode("ONBOA_DATE");
                        models_Xml.ONBOA_DATE = (onboaDateNode != null) ? onboaDateNode.InnerText : "";
                        XmlNode contractNode = bookNode.SelectSingleNode("CONTRACT");
                        models_Xml.CONTRACT = (contractNode != null) ? contractNode.InnerText : "";
                        XmlNode wMonthNode = bookNode.SelectSingleNode("W_MONTH");
                        models_Xml.W_MONTH = (wMonthNode != null) ? wMonthNode.InnerText : "";
                        XmlNode wYearNode = bookNode.SelectSingleNode("W_YEAR");
                        models_Xml.W_YEAR = (wYearNode != null) ? wYearNode.InnerText : "";
                        XmlNode natioNode = bookNode.SelectSingleNode("NATIO");
                        models_Xml.NATIO = (natioNode != null) ? natioNode.InnerText : "";
                        XmlNode termiReasonNode = bookNode.SelectSingleNode("TERMI_REASON");
                        models_Xml.TERMI_REASON = (termiReasonNode != null) ? termiReasonNode.InnerText : "";
                        XmlNode directNode = bookNode.SelectSingleNode("DIRECT");
                        models_Xml.DIRECT = (directNode != null) ? directNode.InnerText : "";
                        XmlNode turnoverNode = bookNode.SelectSingleNode("TURNOVER");
                        models_Xml.TURNOVER = (turnoverNode != null) ? turnoverNode.InnerText : "";
                        XmlNode voluntaryNode = bookNode.SelectSingleNode("VOLUNTARY");
                        models_Xml.VOLUNTARY = (voluntaryNode != null) ? voluntaryNode.InnerText : "";
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


