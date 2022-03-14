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
    public class Code
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<DataListController> _logger;

        public Code(ILogger<DataListController> logger, IConfiguration configuration)
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

        public TransactionCodeClass transactionCode(string code)
        {
            List<Transactions> transactions = new List<Transactions>();
            TransactionCodeClass codeClass = new TransactionCodeClass();

            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from Kiosk.Transactions where code='" + code + "' and status = 'In Process'", con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            var trans = new Transactions();

                            trans.priorityID = Convert.ToInt32(reader["priorityID"]);
                            trans.nickName = reader["nickName"].ToString();
                            trans.prioritystatus = reader["prioritystatus"].ToString();
                            trans.priorityvalue = Convert.ToInt32(reader["priorityvalue"].ToString());
                            trans.types = reader["types"].ToString();
                            trans.arrTypes = trans.types.Trim('[', ']')
                                                .Split(",")
                                                .Select(x => x.Trim('"'))
                                                .ToArray();
                            trans.code = reader["code"].ToString();
                            trans.status = reader["status"].ToString();
                            trans.statusvalue = Convert.ToInt32(reader["statusvalue"].ToString());
                            trans.transdate = Convert.ToDateTime(reader["transdate"].ToString());


                            transactions.Add(trans);
                        }
                        reader.Close();
                        con.Close();

                        var jsonResp = JsonConvert.SerializeObject(transactions);

                        codeClass.statusCode = 1;
                        codeClass.statusMessage = "success";
                        codeClass.transList = transactions;

                        _logger.LogInformation(" Kiosk Web Service transaction list - " + jsonResp);
                        return codeClass;
                    }
                    else
                    {
                        reader.Close();
                        con.Close();

                        codeClass.statusCode = 0;
                        codeClass.statusMessage = "No data found.";
                        codeClass.transList = transactions;

                        var jConvert = JsonConvert.SerializeObject(codeClass);

                        _logger.LogInformation(" Kiosk Web Service transaction list - " + jConvert);

                        return codeClass;
                    }
                }
            }
            catch (Exception ex)
            {
                codeClass.statusCode = 0;
                codeClass.statusMessage = ex.Message.ToString();
                codeClass.transList = transactions;

                _logger.LogError(" Kiosk Web Service transaction list - " + codeClass);
                return codeClass;
            }
        }
    }
}
