using KioskWebService.Controllers;
using KioskWebService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace KioskWebService.Methods
{
    public class TransactionMethods
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<KioskController> _logger;

        public TransactionMethods(ILogger<KioskController> logger, IConfiguration configuration)
        {
            try
            {
                _logger = logger;
                _configuration = configuration;

                connectionString = _configuration.GetConnectionString("DefaultConnection");
                _logger.LogInformation(" Kiosk Web Service Connection String - " + connectionString);

            }
            catch (Exception ex)
            {
                _logger.LogInformation(" Kiosk Web Service Connection String - " + ex.ToString());
                throw new Exception(ex.Message.ToString());
            }
        }

        public TransactionCodeClass Queries(int id, string machineName, int counterNo, string status, int statusvalue, string dateName, 
            string branchCode, string branchName, int zoneCode, string roleID, string operatorID, string operatorName) 
        {
            TransactionCodeClass codeClass = new TransactionCodeClass();

            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                        cmd.Transaction = trans;
                        cmd.CommandText = "set autocommit=0";

                        cmd.CommandText = "update Kiosk.Transactions SET status = '" + status + "', " + dateName + " = NOW(), machineName = '"
                                            + machineName + "', counterNo = '" + counterNo + "', statusvalue = '" + statusvalue + "', branchCode = '" 
                                            + branchCode + "', branchName = '" + branchName + "', zoneCode = '" + zoneCode + "', roleID = '" 
                                            + roleID + "', operatorID = '" + operatorID + "', operatorName = '" + operatorName + "' " +
                                            "where `priorityID` = '" + id + "'";

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("machineName", machineName);
                        cmd.Parameters.AddWithValue("counterNo", counterNo);
                        cmd.Parameters.AddWithValue("statusvalue", statusvalue);
                        cmd.Parameters.AddWithValue("priorityID", id);
                        cmd.Parameters.AddWithValue("branchCode", branchCode);
                        cmd.Parameters.AddWithValue("branchName", branchName);
                        cmd.Parameters.AddWithValue("zoneCode", zoneCode);
                        cmd.Parameters.AddWithValue("roleID", roleID);
                        cmd.Parameters.AddWithValue("operatorID", operatorID);
                        cmd.Parameters.AddWithValue("operatorName", operatorName);
                        var xxx = cmd.ExecuteNonQuery();

                        if (xxx != 1)
                        {
                            trans.Rollback();
                            con.Close();

                            codeClass.statusCode = 0;
                            codeClass.statusMessage = "Something went wrong.";

                            _logger.LogWarning(" Kiosk Web Service [" + status + "] - 0 | Something went wrong.");
                            
                            return codeClass;
                        }
                        else
                        {
                            trans.Commit();
                            con.Close();

                            codeClass.statusCode = 1;
                            codeClass.statusMessage = status;

                            _logger.LogInformation(" Kiosk Web Service [" + status + "]  - 1 | " + status + ".");
                            return codeClass;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" Kiosk Web Service [" + status + "]  - " + ex.Message.ToString());
                throw new Exception(ex.Message.ToString()); ;
            }
        }
    }
}
