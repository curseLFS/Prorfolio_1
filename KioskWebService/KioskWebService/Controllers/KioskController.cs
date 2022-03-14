using KioskWebService.Methods;
using KioskWebService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace KioskWebService.Controllers
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class KioskController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<KioskController> _logger;
         
        public KioskController(ILogger<KioskController> logger, IConfiguration configuration)
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
                       
        [HttpPost]
        [Route("Kiosk")]
        public IActionResult PostKiosk(Transactions transactions) 
        {
            // nickname count
            int countName = transactions.nickName.Length;
            transactions.machineName = Environment.MachineName.ToString();

            _logger.LogInformation(" Kiosk Web Service nickName count - " + countName);

            // Check if nickName exist
            var isexist = new IsNickNameExist(_logger, _configuration);
            if (isexist.isExist(transactions.nickName) == true)
            {
                _logger.LogWarning(" Kiosk Web Service - 0 | NickName Already Exist");
                return Ok(new { statusCode = 0, statusMessage = "NickName Already Exist" });
            }

            // ************ TYPES METHODS **********************************
            var types = new Types(_logger, _configuration);
            var isInquiry = types.isContainInquiry(transactions.transTypes);
            var isService = types.isContainService(transactions.transTypes);

            // Check nickName count
            if (countName <= 8) 
            {
                // Check if INQUIRY
                if (isInquiry == true) 
                {
                    _logger.LogInformation(" Kiosk Web Service transaction code - [inquiry]");
                    transactions.code = "inquiry";
                }

                // Check if SERVICE
                if (isService == true)
                {
                    _logger.LogInformation(" Kiosk Web Service transaction code - [service]");
                    transactions.code = "service";
                }
            
                // if count is zero
                if (transactions.transTypes.Count == 0) 
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Please select transaction.");
                    return Ok(new { statusCode = 0, statusMessage = "Please select transaction." });
                }
                // if the priority status got other values
                if (transactions.prioritystatus == "" || transactions.prioritystatus == "\"\"" || transactions.prioritystatus == "[]")
                {
                    _logger.LogInformation(" Kiosk Web Service transaction prioritystatus - [Normal]");
                    transactions.prioritystatus = "Normal";
                    transactions.priorityvalue = 1;
                }
                else 
                {
                    transactions.priorityvalue = 0;
                }

                // get the in process status
                if (transactions.status == null)
                {
                    _logger.LogInformation(" Kiosk Web Service transaction status - [In Process]");
                    transactions.status = "In Process";
                    transactions.statusvalue = 1;
                }

                var des = transactions.transTypes;
                string serialized = JsonConvert.SerializeObject(des);
                transactions.types = serialized;

                _logger.LogInformation(" Kiosk Web Service transaction type/s - " + transactions.types);

                var serverdate = DateTime.Now;
                transactions.transdate = serverdate;
                _logger.LogInformation(" Kiosk Web Service transaction transdate - " + transactions.transdate);

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

                            cmd.CommandText = "INSERT INTO Kiosk.Transactions" +
                                              "(nickName,prioritystatus,priorityvalue,types,code,status,statusvalue,transdate,"+
                                              "machineName,counterNo,branchCode,branchName,zoneCode,roleID,operatorID,operatorName)" +
                                              "VALUES" +
                                              "(@nickName,@prioritystatus,@priorityvalue,@types,@code,@status,@statusvalue,@transdate,"+
                                              "@machineName,@counterNo,@branchCode,@branchName,@zoneCode,@roleID,@operatorID,@operatorName)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("nickName", transactions.nickName);
                            cmd.Parameters.AddWithValue("prioritystatus", transactions.prioritystatus);
                            cmd.Parameters.AddWithValue("priorityvalue", transactions.priorityvalue);
                            cmd.Parameters.AddWithValue("types", transactions.types);
                            cmd.Parameters.AddWithValue("code", transactions.code);
                            cmd.Parameters.AddWithValue("status", transactions.status);
                            cmd.Parameters.AddWithValue("statusvalue", transactions.statusvalue);
                            cmd.Parameters.AddWithValue("transdate", transactions.transdate);
                            cmd.Parameters.AddWithValue("machineName", transactions.machineName);
                            cmd.Parameters.AddWithValue("counterNo", transactions.counterNo);
                            cmd.Parameters.AddWithValue("branchCode", transactions.branchCode);
                            cmd.Parameters.AddWithValue("branchName", transactions.branchName);
                            cmd.Parameters.AddWithValue("zoneCode", transactions.zoneCode);
                            cmd.Parameters.AddWithValue("roleID", transactions.roleID);
                            cmd.Parameters.AddWithValue("operatorID", transactions.operatorID);
                            cmd.Parameters.AddWithValue("operatorName", transactions.operatorName);
                            var xxx = cmd.ExecuteNonQuery();
                            
                            if (xxx > 0) 
                            {
                                trans.Commit();
                                con.Close();

                                _logger.LogInformation(" Kiosk Web Service transaction - " + transactions);
                                return Ok(new { statusCode = 1, statusMessage = "Transaction Successful", data = transactions });
                            }
                            else 
                            {
                                trans.Rollback();
                                con.Close();

                                _logger.LogCritical(" Kiosk Web Service transaction - 0 | Something went wrong");
                                return Ok(new { statusCode = 0, statusMessage = "Something went wrong"});
                            }
                        }                       
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(" Kiosk Web Service" + ex.Message.ToString());
                    return Ok( new { statusCode = 0, statusMessage = new Exception(ex.Message.ToString()) });
                }                
            }
            else
            {
                _logger.LogWarning(" Kiosk Web Service - 0 | NickName is too long.");
                return Ok(new { statusCode = 0, statusMessage = "NickName is too long." });
            }          
        }

        [HttpPost]
        [Route("Kiosk/Complete/{id}")]
        public IActionResult CompleteKiosk(int id, Transactions transactions)
        {
            var getstatus = new GetStatus(_logger, _configuration);
            var status = getstatus.getStatus(id);
            try
            {              
                if (status == "Now Serving")
                {
                    var mName = Environment.MachineName.ToString();

                    var transMethod = new TransactionMethods(_logger, _configuration);

                    var complete = transMethod.Queries(id, mName, transactions.counterNo, "Complete", 2, "CompleteDate", 
                        transactions.branchCode, transactions.branchName, transactions.zoneCode, transactions.roleID, 
                        transactions.operatorID, transactions.operatorName);

                    return Ok(complete);
                }
                else if (status == "In Process") 
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction must be serve.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction must be serve." });
                }
                else if (status == "Cancel") 
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction already cancel.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction already cancel." });
                }
                else
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction already completed.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction already completed." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" Kiosk Web Service - " + ex.Message.ToString());
                throw new Exception(ex.Message.ToString());
            }    
        }
   
        [HttpPost]
        [Route("Kiosk/Cancel/{id}")]
        public IActionResult CancelKiosk(int id, Transactions transactions)
        {
            var getstatus = new GetStatus(_logger, _configuration);
            var status = getstatus.getStatus(id);
            try
            {               
                if (status == "In Process" || status == "Now Serving")
                {
                    var mName = Environment.MachineName.ToString();

                    var transMethod = new TransactionMethods(_logger, _configuration);

                    var cancel = transMethod.Queries(id, mName, transactions.counterNo, "Cancel", 3, "CancelDate", 
                        transactions.branchCode, transactions.branchName, transactions.zoneCode, transactions.roleID, 
                        transactions.operatorID, transactions.operatorName);

                    return Ok(cancel);
                }
                else if (status == "Complete")
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction already complete.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction already complete." });
                }
                else
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction already cancelled.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction already cancelled." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" Kiosk Web Service" + ex.Message.ToString());
                throw new Exception(ex.Message.ToString());
            }                    
        }

        [HttpPost]
        [Route("Kiosk/Serve/{id}")]
        public IActionResult ForServing(int id, Transactions transactions) 
        {

            var getstatus = new GetStatus(_logger, _configuration);
            var status = getstatus.getStatus(id);
            try
            {
                var mName = Environment.MachineName.ToString();

                if (status == "In Process") 
                {
                    var transMethod = new TransactionMethods(_logger, _configuration);

                    var serve = transMethod.Queries(id, mName, transactions.counterNo, "Now Serving", 0, "ServeDate", 
                        transactions.branchCode, transactions.branchName, transactions.zoneCode, transactions.roleID, 
                        transactions.operatorID, transactions.operatorName);

                    return Ok(serve);
                }
                else if (status == "Complete") 
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction already complete. Nothing to serve.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction already complete. Nothing to serve." });
                }
                else if (status == "Cancel") 
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction already cancel.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction already cancel." });
                }
                else
                {
                    _logger.LogWarning(" Kiosk Web Service - 0 | Transaction already serve.");
                    return Ok(new { statusCode = 0, statusMessage = "Transaction already serve." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" Kiosk Web Service - " + ex.Message.ToString());
                throw new Exception(ex.Message.ToString());
            }                   
        }              
    }
}
