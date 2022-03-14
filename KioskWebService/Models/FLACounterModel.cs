using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskWebService.Models
{
    public class FLACounterModel
    {
        public int id { get; set; }
        public string branchCode { get; set; }
        public int zoneCode { get; set; }
        public string operatorID { get; set; }
        public int counterNum { get; set; }
        public DateTime syscreated { get; set; }
        public int respCode { get; set; }
        public string respMessage { get; set; }
    }
}
