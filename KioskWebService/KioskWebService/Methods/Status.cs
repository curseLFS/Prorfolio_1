using KioskWebService.Controllers;
using KioskWebService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KioskWebService.Methods
{
    public class Status
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<DataListController> _logger;

        public Status(ILogger<DataListController> logger, IConfiguration configuration)
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

        public string transactionStatus(string status, ILogger _logger)
        {
            List<Transactions> transactions = new List<Transactions>();
            TransactionCodeClass codeClass = new TransactionCodeClass();
     
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from Kiosk.Transactions where status='" + status + "'", con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            var trans = new Transactions();

                            trans.priorityID = Convert.ToInt32(reader["priorityID"]);
                            trans.nickName = reader["nickName"].ToString();
                            trans.prioritystatus = reader["prioritystatus"].ToString();
                            trans.priorityvalue = Convert.ToInt32(reader["priorityvalue"]);
                            trans.types = reader["types"].ToString();
                            trans.arrTypes = trans.types.Trim('[', ']')
                                                .Split(",")
                                                .Select(x => x.Trim('"'))
                                                .ToArray();
                            trans.code = reader["code"].ToString();
                            trans.status = reader["status"].ToString();
                            trans.statusvalue = Convert.ToInt32(reader["statusvalue"]);
                            trans.transdate = Convert.ToDateTime(reader["transdate"].ToString());
                            trans.counterNo = Convert.ToInt32(reader["counterNo"]);
                            trans.machineName = reader["machineName"].ToString();
                            trans.branchCode = reader["branchCode"].ToString();
                            trans.branchName = reader["branchName"].ToString();
                            trans.zoneCode = Convert.ToInt32(reader["zoneCode"]);
                            trans.roleID = reader["roleID"].ToString();
                            trans.operatorID = reader["machineName"].ToString();
                            trans.operatorName = reader["operatorID"].ToString();


                            transactions.Add(trans);
                        }
                        reader.Close();
                        con.Close();

                        codeClass.statusCode = 1;
                        codeClass.statusMessage = "success";
                        codeClass.transList = transactions;

                        var jsonResp = JsonConvert.SerializeObject(codeClass, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                        _logger.LogInformation(" Kiosk Web Service " + status + " list - " + jsonResp);
                        return jsonResp;
                    }
                    else
                    {
                        reader.Close();
                        con.Close();

                        codeClass.statusCode = 0;
                        codeClass.statusMessage = "No data found.";

                        var jsonResp = JsonConvert.SerializeObject(codeClass, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                        _logger.LogInformation(" Kiosk Web Service " + status + " list - " + jsonResp);
                        return jsonResp;
                    }
                }
            }
            catch (Exception ex)
            {
                codeClass.statusCode = 0;
                codeClass.statusMessage = ex.Message.ToString();
                codeClass.transList = transactions;

                var jsonResp = JsonConvert.SerializeObject(codeClass);

                _logger.LogError(" Kiosk Web Service " + status + " list - " + jsonResp);
                return jsonResp;
            }
        }
    }
}
