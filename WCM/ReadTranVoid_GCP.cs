using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.WCM
{
    public class ReadTranVoid_GCP
    {
        public List<TransVoidLine> TransVoidLine(JArray TransInputData)
        {
            List<TransVoidLine> TransInputDatasss = new List<TransVoidLine>();
            foreach (JObject TransInputDatass in TransInputData)
            {
                if ((int)TransInputDatass["LineType"] == 0)
                {
                TransVoidLine TransInputDatas = new TransVoidLine();
                TransInputDatas.ScanTime = (DateTime)TransInputDatass["ScanTime"];
                TransInputDatas.OrderNo = (string)TransInputDatass["DocumentNo"];
                TransInputDatas.LineType = (int)TransInputDatass["LineType"];
                TransInputDatas.LocationCode = (string)TransInputDatass["LocationCode"];
                TransInputDatas.ItemNo = (string)TransInputDatass["ItemNo"];
                TransInputDatas.Description = (string)TransInputDatass["Description"];
                TransInputDatas.UnitOfMeasure = (string)TransInputDatass["UnitOfMeasure"];
                TransInputDatas.Quantity = (decimal)TransInputDatass["Quantity"];
                TransInputDatas.UnitPrice = (decimal)TransInputDatass["UnitPrice"];
                TransInputDatas.DiscountAmount = (decimal)TransInputDatass["DiscountAmount"];
                TransInputDatas.LineAmountIncVAT = (decimal)TransInputDatass["LineAmountIncVAT"];
                TransInputDatas.StaffID = (string)TransInputDatass["StaffID"];
                TransInputDatas.VATCode = (string)TransInputDatass["VATCode"];
                TransInputDatas.DeliveringMethod = (int)TransInputDatass["DeliveringMethod"];
                TransInputDatas.Barcode = (string)TransInputDatass["Barcode"];
                TransInputDatas.DivisionCode = (string)TransInputDatass["DivisionCode"];
                TransInputDatas.SerialNo = (string)TransInputDatass["SerialNo"];
                TransInputDatas.OrigOrderNo = (string)TransInputDatass["OrigOrderNo"];
                TransInputDatas.LotNo = (string)TransInputDatass["LotNo"];
                TransInputDatas.ArticleType = (string)TransInputDatass["ArticleType"];
                TransInputDatas.LastUpdated = TransInputDatas.ScanTime;
                TransInputDatasss.Add(TransInputDatas);
                }
            }
            return TransInputDatasss;
        }
        public List<TransVoidHeader> TransVoidHeader(JArray TransInputData)
        {
            List<TransVoidHeader> TransInputDatasss = new List<TransVoidHeader>();
            foreach (JObject TransInputDatass in TransInputData)
            {
                TransVoidHeader TransInputDatas = new TransVoidHeader();
                TransInputDatas.OrderNo = (string)TransInputDatass["OrderNo"];
                TransInputDatas.OrderDate = (DateTime)TransInputDatass["OrderDate"];
                TransInputDatas.CustomerNo = (string)TransInputDatass["CustomerNo"];
                TransInputDatas.CustomerName = (string)TransInputDatass["CustomerName"];
                TransInputDatas.ZoneNo = (string)TransInputDatass["ZoneNo"];
                TransInputDatas.ShipToAddress = (string)TransInputDatass["ShipToAddress"];
                TransInputDatas.StoreNo = (string)TransInputDatass["StoreNo"];
                TransInputDatas.POSTerminalNo = (string)TransInputDatass["POSTerminalNo"];
                TransInputDatas.ShiftNo = (string)TransInputDatass["ShiftNo"];
                TransInputDatas.CashierID = (string)TransInputDatass["CashierID"];
                TransInputDatas.AmountInclVAT = (decimal)TransInputDatass["AmountInclVAT"];
                TransInputDatas.UserID = (string)TransInputDatass["UserID"];
                TransInputDatas.DeliveringMethod = (int)TransInputDatass["DeliveringMethod"];
                TransInputDatas.PrepaymentAmount = (decimal)TransInputDatass["PrepaymentAmount"];
                TransInputDatas.DeliveringMethod = (int)TransInputDatass["DeliveringMethod"];
                TransInputDatas.TanencyNo = (string)TransInputDatass["TanencyNo"];
                TransInputDatas.SalesIsReturn = (int)TransInputDatass["SalesIsReturn"];
                TransInputDatas.MemberCardNo = (string)TransInputDatass["MemberCardNo"];
                TransInputDatas.ReturnedOrderNo = (string)TransInputDatass["ReturnedOrderNo"];
                TransInputDatas.TransactionType = (int)TransInputDatass["TransactionType"];
                TransInputDatas.PrintedNumber = (int)TransInputDatass["PrintedNumber"];
                TransInputDatas.LastUpdated = (DateTime)TransInputDatass["EndingTime"];
                TransInputDatas.UserVoid = (string)TransInputDatass["UserVoid"];
                TransInputDatas.DocumentType = (string)TransInputDatass["DocumentType"];
                TransInputDatasss.Add(TransInputDatas);
            }
            return TransInputDatasss;
        }
    }
}
