using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Read_xml.Models
{
    public class HR_Dashboard
    {
        public Guid Id { get; set; }
        public string? YEAR { get; set; }
        public string? MONTH { get; set; }
        public string? KEY_DATE { get; set; }
        public string? PERNR { get; set; }
        public string? FULLNAME { get; set; }
        public string? SEX { get; set; }
        public string? DOB { get; set; }
        public string? AGE { get; set; }
        public string? BU { get; set; }
        public string? ENTITY { get; set; }
        public string? DEPARTMENT { get; set; }
        public string? POSITION { get; set; }
        public string? RANK { get; set; }
        public string? RANK_GROUP { get; set; }
        public string? FUNCTION { get; set; }
        public string? FUNCTION_GROUP { get; set; }
        public string? MAKE { get; set; }
        public string? ONBOA_DATE { get; set; }
        public string? WORK_PLACE { get; set; }
        public string? CONTRACT { get; set; }
        public string? SENIORITY { get; set; }
        public string? DIRECT { get; set; }
        public string? FILENAME { get; set; }
    }

}
