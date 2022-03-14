using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KioskWebService
{
    public class Transactions
    {
        public int priorityID { get; set; }
        public string nickName { get; set; }
        public string prioritystatus { get; set; }
        public int priorityvalue { get; set; }
        public List<string> transTypes { get; set; }
        public string types { get; set; }
        public string[] arrTypes { get; set; }
        public string code { get; set; }
        public string status { get; set; }
        public int statusvalue { get; set; }
        public DateTime transdate { get; set; }
        public int counterNo { get; set; }
        public string machineName { get; set; }
        public string branchCode { get; set; }
        public string branchName { get; set; }
        public int zoneCode { get; set; }
        public string roleID { get; set; }
        public string operatorID { get; set; }
        public string operatorName { get; set; }
    }
}
