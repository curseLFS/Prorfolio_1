using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Data;
using System.Linq;

namespace KioskWebService.Controllers
{
    [Authorize]
    [Route("etc/")]
    [ApiController]
    public class MethodsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<MethodsController> _logger;

        public MethodsController(ILogger<MethodsController> logger, IConfiguration configuration)
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

        [HttpGet]
        [Route("CheckDBConnection")]
        public IActionResult checkDBConnection() 
        {
            string msg = "";
            try
            {
                var con = new MySqlConnection(connectionString);
                if (con.ConnectionString != "") 
                {
                    con.Open();

                    if (con.State == ConnectionState.Open) 
                    {
                        con.Close();
                        msg = "Database Access Success.";
                    }                   
                }
                else
                {
                    msg = "No Database Connection String.";
                }

                return Ok(new { statusCode = 1, statusMessage = msg });
            }
            catch (Exception ex)
            {
                return Ok(new { statusCode = 0, statusMessage = ex.Message.ToString() });
            }           
        }

        [HttpGet]
        [Route("Translate")]
        public IActionResult translate(string to, string text)
        {

            string result = "";

            //var client = new RestClient("https://microsoft-translator-text.p.rapidapi.com/translate?api-version=3.0&to=fil&textType=plain&profanityAction=NoAction");
            //var client = new RestClient(url + "/translate?api-version=" + version + "&to=" + to + "&textType=" + textType + "&profanityAction=" + profanityAction);
            var client = new RestClient("https://microsoft-translator-text.p.rapidapi.com/translate?api-version=3.0&to=" + to + "&textType=plain&profanityAction=NoAction");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("x-rapidapi-key", "2a9c55fceemsheaf35fe72b1a58cp1b51c4jsnc6aabac7d3ce");
            request.AddHeader("x-rapidapi-host", "microsoft-translator-text.p.rapidapi.com");
            //request.AddParameter("application/json", "[\r{\r\"Text\": \"I would really like to drive your car around the block a few times.\"\r}\r]", ParameterType.RequestBody);
            request.AddParameter("application/json", "[\r{\r\"Text\": \"" + text + "\"\r}\r]", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            result = response.Content;

            var dynamic_result = JsonConvert.DeserializeObject<dynamic>(result);
            var translated = dynamic_result[0].translations[0].text;

            _logger.LogInformation(" Kiosk Web Service translation method statusCode - 1");
            _logger.LogInformation(" Kiosk Web Service ranslation method statusMessage - Success");
            //_logger.LogInformation(" Kiosk Web Service translation method from - " + text + " to - " + translated.value);

            return Ok(new { statusCode = "1", statusMessage = "Success", from = text, to = translated.Value });
        }

        [HttpGet]
        [Route("CheckDate")]
        public IActionResult CheckDate()
        {
            using (var con = new MySqlConnection(connectionString))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                    cmd.Transaction = trans;
                    cmd.CommandText = "set autocommit=0";

                    cmd.CommandText = "UPDATE Kiosk.Transactions SET STATUS = 'Expired Date' WHERE `status` = 'In Process' AND DATEDIFF(NOW(), transdate)";
                    cmd.Parameters.Clear();

                    var xxx = cmd.ExecuteNonQuery();

                    if (xxx >= 1)
                    {
                        trans.Commit();
                    }
                    else
                    {
                        trans.Rollback();
                    }
                }
            }
            _logger.LogInformation(" Kiosk Web Service - 1 | Checking date/s success.");
            return Ok(new { statusCode = 1, statusMessage = "Checking dates success." });
        }

        [HttpGet]
        [Route("IsNickNameExist")]
        public bool IsNickNameExist(string nickname)
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
                throw new Exception(ex.ToString());
            }

            return isExist;
        }

        [HttpGet]
        [Route("TypesById")]
        public IActionResult TypesById(int id)
        {

            Transactions trans = new Transactions();

            string[] arr;

            using (var con = new MySqlConnection(connectionString))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select types from Kiosk.Transactions where priorityId = '" + id + "' and status='In Process'";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("priorityID", id);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {

                        reader.Read();

                        trans.types = reader["types"].ToString();

                        arr = trans.types.Trim('[', ']')
                                        .Split(",")
                                        .Select(x => x.Trim('"'))
                                        .ToArray();

                        _logger.LogInformation(" Kiosk Web Service get types with id - " + arr);

                        reader.Close();
                        con.Close();
                        return Ok(new { data = arr });
                    }
                    else
                    {
                        con.Close();
                        return Ok(new { statuscode = 0, statusmessage = "types not available please check your idno." });
                    }
                }
            }
        }
    }
}
