using KioskWebService.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;

namespace KioskWebService.Methods
{
    public class IsNickNameExist
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<KioskController> _logger;

        public IsNickNameExist(ILogger<KioskController> logger, IConfiguration configuration)
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
        public bool isExist(string nickname)
        {
            bool isExist = false;

            var nick = nickname.ToUpper();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "select transdate, NickName from Kiosk.Transactions where nickName = '" + nick + "' and date(transdate)= curdate()";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("nickName", nick);

                        MySqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            reader.Read();
                            var nickName = reader["nickName"].ToString().ToUpper();

                            if (nickName == nick)
                            {
                                isExist = true;
                            }

                        }
                        reader.Close();
                    }

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }

            return isExist;
        }
    }
}
