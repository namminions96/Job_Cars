using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.Models
{
    public class ConfigConnections
    {
        public  int ID { get ; set; }
        public string Name { get; set; }
        public string ConnectString { get; set; }
        public string Type { get; set; }
        public bool Status { get; set; }
    }
}
