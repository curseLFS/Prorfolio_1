using KioskWebService.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskWebService.Methods
{
    public class Types
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<KioskController> _logger;

        public Types(ILogger<KioskController> logger, IConfiguration configuration)
        {
            try
            {
                _logger = logger;
                _configuration = configuration;

                connectionString = _configuration.GetConnectionString("DefaultConnection");
                _logger.LogInformation(" Kiosk Web Service Get Connection String - " + connectionString);

            }
            catch (Exception ex)
            {
                _logger.LogInformation(" Kiosk Web Service Get Connection String - " + ex.ToString());
                throw new Exception(ex.Message.ToString());
            }
        }

        public bool isContainInquiry(List<string> tType)
        {
            bool iscontains = false;
            if (tType.Contains("Loan") || tType.Contains("Rates") || tType.Contains("Prenda") || tType.Contains("Other Inquiry") ||
                tType.Contains("Other Inquiries"))
            {
                iscontains = true;
            }

            return iscontains;
        }

        public bool isContainService(List<string> tType)
        {
            bool iscontains = false;
            if (tType.Contains("Sendout") || tType.Contains("Paybills") || tType.Contains("Eload") || tType.Contains("Send Money") ||
                tType.Contains("Receive Money") || tType.Contains("Add Money") || tType.Contains("Money Changer"))
            {
                iscontains = true;
            }

            return iscontains;
        }

    }
}
