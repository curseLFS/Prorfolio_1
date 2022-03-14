using System.Collections.Generic;

namespace KioskWebService.Models
{
    public class TransactionCodeClass
    {
        public List<Transactions> transList { get; set; }
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
    }
}
