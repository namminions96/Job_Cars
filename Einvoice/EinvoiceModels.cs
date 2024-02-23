using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.Einvoice
{
    public class EinvoiceModels
    {
        public  class TimeRunEinvoice
        {
            public string? FileName { get; set; }
            public string? Type { get; set; }
            public DateTime? TimeRun { get; set; }
            public bool Status { get; set; }
        }
    }
}
