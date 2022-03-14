using KioskWebService.Methods;
using KioskWebService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Net;

namespace KioskWebService.Controllers
{
    [Authorize]
    [Route("users/")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger, IConfiguration configuration)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                _logger = logger;
                _configuration = configuration;

                connectionString = _configuration.GetConnectionString("DomesticConnection");
                _logger.LogInformation(" Kiosk Web Service Get Connection String - " + connectionString);

            }
            catch (Exception ex)
            {
                _logger.LogInformation(" Kiosk Web Service Get Connection String - " + ex.ToString());
                throw new Exception(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("userDetails")]
        public IActionResult UserDetails(UserDetails userDetails) 
        {
            Users users = new Users();

            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from kpusers.sysuseraccounts where userlogin='" + 
                        userDetails._userLogin + "' AND userpassword = '" + userDetails._userPassword + "'", con);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        userDetails._uID = Convert.ToInt32(reader["UID"]);
                        userDetails._resourceID = Convert.ToDecimal(reader["resourceID"]);
                        userDetails._userLogin = reader["UserLogin"].ToString();
                        userDetails._userPassword = reader["UserPassword"].ToString();
                        userDetails._branchCode = reader["BranchCode"].ToString();
                        userDetails._roleID = reader["RoleID"].ToString();
                        userDetails._divCode = reader["DivCode"].ToString();
                        userDetails._isreliever = Convert.ToInt32(reader["IsReliever"]);
                        userDetails._tempBranchCode = reader["TempBranchCode"].ToString();
                        userDetails._tempZoneCode = Convert.ToInt32(reader["TempZoneCode"]);
                        userDetails._zoneCode = Convert.ToInt32(reader["ZoneCode"]);
                        userDetails._startDate = Convert.ToDateTime(reader["StartDate"]);
                        userDetails._endDate = Convert.ToDateTime(reader["EndDate"]);
                        userDetails._isActive = Convert.ToInt32(reader["IsActive"]);
                        userDetails._syscreated = Convert.ToDateTime(reader["syscreated"]);
                        userDetails._sysmodified = Convert.ToDateTime(reader["sysmodified"]);
                        userDetails._syscreator = reader["syscreator"].ToString();
                        userDetails._sysmodifier = reader["sysmodifier"].ToString();
                        userDetails._isresign = Convert.ToInt32(reader["IsResign"]);
                        userDetails._cellNo = reader["CellNo"].ToString();
                        userDetails._emailAddress = reader["EmailAddress"].ToString();
                        userDetails._passwordStrength = reader["PasswordStrength"].ToString();

                        users.respcode = 1;
                        users.respmessage = "Success";

                        reader.Close();
                        con.Close();

                 
                        _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                        return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage, data = userDetails });
                    }
                    else
                    {
                        reader.Close();
                        con.Close();

                        users.respcode = 0;
                        users.respmessage = "Something went wrong";

                        _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                        return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage });
                    }
                }
            }
            catch (Exception ex)
            {
                users.respcode = 0;
                users.respmessage = ex.Message.ToString();

                _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage });
            }
        }

        [HttpPost]
        [Route("userLogin")]
        public IActionResult UserLogin(UserDetails user) 
        {
            Users users = new Users();

            if (string.IsNullOrEmpty(user._userLogin) || string.IsNullOrEmpty(user._userPassword)) 
            {
                users.respcode = 0;
                users.respmessage = "Please enter valid username/password";
                _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage });
            }

            UsersLogin login = new UsersLogin();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string commandText = "SELECT s.ResourceID,b.fullname AS Operator,IF(s.isreliever=1, s.TempBranchCode, s.BranchCode) " + 
                        "AS Branch_Code,c.branchname AS Branch_Name, s.roleid AS ROLE,IF(s.isreliever=1, s.TempZoneCode, s.zonecode) AS " + 
                        "Zone_Code FROM kpusers.sysuseraccounts s LEFT JOIN kpusers.branchusers b ON s.ResourceID = b.ResourceID AND " +
                        "IF(s.isreliever=1, s.TempZoneCode, s.ZoneCode) = b.ZoneCode LEFT JOIN kpusers.branches c ON " +
                        "b.branchcode = c.branchcode AND b.ZoneCode = c.zonecode WHERE s.UserLogin = '" + user._userLogin + "' AND " +
                        "IF(s.isreliever= 1, s.TempBranchCode, s.BranchCode) = b.BranchCode AND s.UserPassword = '" + user._userPassword + "'";

                    MySqlCommand cmd = new MySqlCommand(commandText, con);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        login._resourceID = Convert.ToDecimal(reader["ResourceID"]);
                        login._operator = reader["Operator"].ToString();
                        login._branchCode = reader["Branch_Code"].ToString();
                        login._branchName = reader["Branch_Name"].ToString();
                        login._role = reader["ROLE"].ToString();
                        login._zoneCode = Convert.ToInt32(reader["Zone_Code"]);
                                              
                        reader.Close();
                        con.Close();

                        if (string.IsNullOrEmpty(Convert.ToString(login._resourceID)) || string.IsNullOrWhiteSpace(login._operator) ||
                            string.IsNullOrEmpty(login._branchCode) || string.IsNullOrEmpty(login._branchName) ||
                            string.IsNullOrEmpty(login._role) || string.IsNullOrEmpty(Convert.ToString(login._zoneCode)))
                        {
                            users.respcode = 0;
                            users.respmessage = "One or more of the user details is empty or null.";
                            _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                            return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage });
                        }
                        else 
                        {
                            if (login._role == "KP-BM" || login._role == "KP-ABM" || login._role == "KP-TELLER")
                            {
                                users.respcode = 1;
                                users.respmessage = "Success";

                                _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                                return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage, data = login });
                                
                            }
                            else 
                            {
                                users.respcode = 0;
                                users.respmessage = "Unauthorized user.";
                                _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                                return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage });
                            }
                        }
                    }
                    else
                    {
                        reader.Close();
                        con.Close();

                        users.respcode = 0;
                        users.respmessage = "Something went wrong";
                        _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                        return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage });
                    }
                }
            }
            catch (Exception ex)
            {
                users.respcode = 0;
                users.respmessage = ex.Message.ToString();

                _logger.LogInformation(" Kiosk Web Service Login - " + users.respcode + " | " + users.respmessage);
                return Ok(new { statusCode = users.respcode, statusMessage = users.respmessage });
            }
        }

        [HttpPost]
        [Route("GetCounterNumber")]
        public IActionResult GetCounterNumber(FLACounterModel fLA)
        {
            var flaCounter = new FLACounter(_logger, _configuration);

            var counterSave = flaCounter.counterSave(fLA.operatorID, fLA.branchCode, fLA.zoneCode, fLA.counterNum);
            var des = JsonConvert.DeserializeObject<FLACounterModel>(counterSave);

            if (des.respCode == 1)
            {
                _logger.LogInformation(" Kiosk Web Service get counter number - " + des.respCode + " | " + des.respMessage);
                return Ok(new { statusCode = des.respCode, statusMessage = des.respMessage, data = des });
            }
            else
            {
                _logger.LogInformation(" Kiosk Web Service get counter number - " + des.respCode + " | " + des.respMessage);
                return Ok(new { statusCode = des.respCode, statusMessage = des.respMessage });
            }
        }

        [HttpPost]
        [Route("userLogout")]
        public IActionResult UserLogout(FLACounterModel user) 
        {

            var flaCounter = new FLACounter(_logger, _configuration);

            var counterUpdate = flaCounter.counterUpdate(user.operatorID, user.branchCode, user.zoneCode);

            return Ok(new { statusCode = 1, statusMessage = "succes", data = counterUpdate });
        }
    }
}
