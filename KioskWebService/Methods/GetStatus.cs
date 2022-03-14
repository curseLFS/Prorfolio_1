using KioskWebService.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskWebService.Methods
{
    public class GetStatus
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<KioskController> _logger;

        public GetStatus(ILogger<KioskController> logger, IConfiguration configuration)
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

        public string getStatus(int id)
        {
            string status = "";
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "select * from Kiosk.Transactions where priorityID = '" + id + "'";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("priorityID", id);

                        MySqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            reader.Read();
                            status = reader["status"].ToString();

                            reader.Close();
                            con.Close();

                            return status;
                        }
                        else
                        {
                            reader.Close();
                            con.Close();

                            return "Transaction not found";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
    }
}
