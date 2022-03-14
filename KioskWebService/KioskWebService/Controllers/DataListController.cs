using KioskWebService.Methods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KioskWebService.Controllers
{
    [Authorize]
    [Route("list/")]
    [ApiController]
    public class DataListController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<DataListController> _logger;

        public DataListController(ILogger<DataListController> logger, IConfiguration configuration)
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
        [Route("InProcess")]
        public IActionResult InProcess()
        {
            var status = new Status(_logger, _configuration);
            var inprocess = status.transactionStatus("In Process", _logger);

            return Ok(inprocess);
        }

        [HttpGet]
        [Route("Serve")]
        public IActionResult Serve()
        {
            var status = new Status(_logger, _configuration);
            var serve = status.transactionStatus("Now Serving", _logger);

            return Ok(serve);
        }

        [HttpGet]
        [Route("Cancel")]
        public IActionResult Cancel()
        {
            var status = new Status(_logger, _configuration);
            var cancel = status.transactionStatus("Cancel", _logger);

            return Ok(cancel);
        }

        [HttpGet]
        [Route("Complete")]
        public IActionResult Complete()
        {
            var status = new Status(_logger, _configuration);
            var complete = status.transactionStatus("Complete", _logger);

            return Ok(complete);

        }

        [HttpGet]
        [Route("ServeInProcess")]
        public IActionResult ServeInProcessList()
        {
            List<Transactions> transactions = new List<Transactions>();

            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from Kiosk.Transactions where status='Now Serving' or status='In Process'", con);
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
                    }
                    else
                    {
                        _logger.LogInformation(" Kiosk Web Service serve list - 0 | No data found");
                        return Ok(new { statuscode = 0, statusmessage = "No data found." });
                    }
                }

                var jsonResp = JsonConvert.SerializeObject(transactions);

                _logger.LogInformation(" Kiosk Web Service serve list - " + jsonResp);
                return Ok(new { data = transactions });
            }
            catch (Exception ex)
            {
                _logger.LogInformation(" Kiosk Web Service serve list - " + (ex.Message.ToString()));
                return Ok(new Exception(ex.Message.ToString()));
            }
        }

        [HttpGet]
        [Route("Service")]
        public IActionResult ServiceList()
        {
            var code = new Code(_logger, _configuration);
            var service = code.transactionCode("service");

            return Ok(service);
        }

        [HttpGet]
        [Route("Inquiry")]
        public IActionResult InquiryeList()
        {
            var code = new Code(_logger, _configuration);
            var inquiry = code.transactionCode("inquiry");

            return Ok(inquiry);
        }
    }
}
