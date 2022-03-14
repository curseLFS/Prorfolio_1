using KioskWebService.Controllers;
using KioskWebService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace KioskWebService.Methods
{
    public class FLACounter
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "";

        private readonly ILogger<LoginController> _logger;

        public FLACounter(ILogger<LoginController> logger, IConfiguration configuration) 
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

        public string counterSave(string operatorID, string branchCode, int zoneCode, int counterNum) 
        {
            FLACounterModel fLA = new FLACounterModel();
            string jsonResp = "";

            var checkAvailable = checkCounterAvailability(operatorID, branchCode, zoneCode, counterNum);

            if (checkAvailable.Contains("No data found.")) 
            {
                try
                {
                    var counterDetail = counterDetails(operatorID, branchCode, zoneCode, counterNum);
                    var des = JsonConvert.DeserializeObject<dynamic>(counterDetail);

                    var data = des.data.Value;


                    if (data == "No data found.")
                    {

                        using (var con = new MySqlConnection(connectionString))
                        {
                            con.Open();
                            using (MySqlCommand cmd = con.CreateCommand())
                            {
                                MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                                cmd.Transaction = trans;
                                cmd.CommandText = "set autocommit=0";

                                cmd.CommandText = "INSERT INTO Kiosk.FLA_Counter (branchCode,zoneCode,operatorID,counterNum,syscreated)" +
                                                  "VALUES (@branchCode,@zoneCode,@operatorID,@counterNum,@syscreated)";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("branchCode", branchCode);
                                cmd.Parameters.AddWithValue("zoneCode", zoneCode);
                                cmd.Parameters.AddWithValue("operatorID", operatorID);
                                cmd.Parameters.AddWithValue("counterNum", counterNum);
                                cmd.Parameters.AddWithValue("syscreated", DateTime.Now);

                                var xxx = cmd.ExecuteNonQuery();

                                if (xxx > 0)
                                {
                                    trans.Commit();
                                    con.Close();

                                    fLA.branchCode = branchCode;
                                    fLA.zoneCode = zoneCode;
                                    fLA.operatorID = operatorID;
                                    fLA.counterNum = counterNum;
                                    fLA.respCode = 1;
                                    fLA.respMessage = "Success";

                                    jsonResp = JsonConvert.SerializeObject(fLA);

                                    _logger.LogInformation(" Kiosk Web Service FLA Counter - " + jsonResp);
                                }
                                else
                                {
                                    trans.Rollback();
                                    con.Close();

                                    fLA.respCode = 0;
                                    fLA.respMessage = "Something went wrong.";

                                    jsonResp = JsonConvert.SerializeObject(fLA);

                                    _logger.LogCritical(" Kiosk Web Service  FLA Counter - 0 | Something went wrong");
                                }
                            }
                        }
                    }
                    else
                    {
                        var des1 = JsonConvert.DeserializeObject<FLACounterModel>(data);

                        if (des1.counterNum != 0)
                        {
                            fLA.respCode = 0;
                            fLA.respMessage = "User already use in another counter.";

                            jsonResp = JsonConvert.SerializeObject(fLA);
                        }
                    }
                }
                catch (Exception ex)
                {
                    fLA.respCode = 0;
                    fLA.respMessage = ex.Message.ToString();

                    jsonResp = JsonConvert.SerializeObject(fLA);

                    _logger.LogCritical(" Kiosk Web Service" + ex.Message.ToString());
                    return jsonResp;
                }
            }
            else 
            {
                fLA.respCode = 0;
                fLA.respMessage = "Counter number already use.";

                jsonResp = JsonConvert.SerializeObject(fLA);
            }

            return jsonResp;
        }

        public string counterDetails(string operatorID, string branchCode, int zoneCode, int counterNum) 
        {
            FLACounterModel fLACounter = new FLACounterModel();
           
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from Kiosk.FLA_Counter where operatorID='" + 
                        operatorID + "' AND zonecode = '"+ zoneCode + "' AND branchcode = '"+ branchCode + "'", con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        fLACounter.branchCode = reader["branchCode"].ToString();
                        fLACounter.zoneCode = Convert.ToInt32(reader["zoneCode"]);
                        fLACounter.operatorID = reader["operatorID"].ToString();
                        fLACounter.counterNum = Convert.ToInt32(reader["counterNum"]);
                        fLACounter.syscreated = Convert.ToDateTime(reader["syscreated"]);
                       
                        reader.Close();
                        con.Close();

                        fLACounter.respCode = 1;
                        fLACounter.respMessage = "success";

                        var resp = new
                        {
                            data = JsonConvert.SerializeObject(fLACounter),
                            statusCode = fLACounter.respCode,
                            statusMessage = fLACounter.respMessage
                        };

                        var jsonResp = JsonConvert.SerializeObject(resp);

                        _logger.LogInformation(" Kiosk Web Service FLA Counter list - " + jsonResp);
                        return jsonResp;
                    }
                    else
                    {
                        reader.Close();
                        con.Close();

                        fLACounter.respCode = 0;
                        fLACounter.respMessage = "No data found.";

                        var resp = new
                        {
                            data = fLACounter.respMessage,
                            statusCode = fLACounter.respCode,
                            statusMessage = fLACounter.respMessage
                        };

                        var jsonResp = JsonConvert.SerializeObject(resp);

                        _logger.LogError(" Kiosk Web Service FLA Counter list - " + jsonResp);
                        return jsonResp;
                    }
                }
            }
            catch (Exception ex)
            {
                fLACounter.respCode = 0;
                fLACounter.respMessage = ex.Message.ToString();

                var resp = new
                {
                    data = fLACounter.respMessage,
                    statusCode = fLACounter.respCode,
                    statusMessage = fLACounter.respMessage
                };

                var jsonResp = JsonConvert.SerializeObject(resp);

                _logger.LogCritical(" Kiosk Web Service  FLA Counter list - " + jsonResp);
                return jsonResp;
            }
        }

        public string counterUpdate(string operatorID, string branchCode, int zoneCode) 
        {
            string respMessage = "";
            string jsonResp = "";
            int respCode = 0;
            object resp ;

            using (var con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                        cmd.Transaction = trans;
                        cmd.CommandText = "set autocommit=0";

                        cmd.CommandText = "update Kiosk.FLA_Counter set counterNum = '0', syscreated = now() where operatorID = '" + 
                            operatorID + "' and branchCode = '" + branchCode + "' and zoneCode = '" + zoneCode + "'";
                        cmd.Parameters.Clear();

                        var xxx = cmd.ExecuteNonQuery();

                        if (xxx >= 1)
                        {
                            respCode = 1;
                            respMessage = "Counter number update to zero.";
                            trans.Commit();
                            con.Close();
                        }
                        else
                        {
                            respCode = 0;
                            respMessage = "Something went wrong";
                            trans.Rollback();
                            con.Close();
                        }
                    }

                    resp = new
                    {
                        respCode = respCode,
                        respMessage = respMessage
                    };

                    jsonResp = JsonConvert.SerializeObject(resp);

                    return jsonResp;
                }
                catch (Exception ex)
                {                   
                    con.Close();
                    resp = new
                    {
                        respCode = respCode,
                        respMessage = ex.Message.ToString()
                    };

                    jsonResp = JsonConvert.SerializeObject(resp);

                    return jsonResp;
                }                
            }
        }

        public string checkCounterAvailability(string operatorID, string branchCode, int zoneCode, int counterNum) 
        {
            FLACounterModel fLA = new FLACounterModel();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from Kiosk.FLA_Counter WHERE zonecode = '"+ zoneCode + 
                        "' AND branchcode = '"+ branchCode + "' AND counternum = '"+ counterNum + "'", con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        fLA.branchCode = reader["branchCode"].ToString();
                        fLA.zoneCode = Convert.ToInt32(reader["zoneCode"]);
                        fLA.operatorID = reader["operatorID"].ToString();
                        fLA.counterNum = Convert.ToInt32(reader["counterNum"]);
                        fLA.syscreated = Convert.ToDateTime(reader["syscreated"]);

                        reader.Close();
                        con.Close();

                        fLA.respCode = 1;
                        fLA.respMessage = "success";

                        var resp = new
                        {
                            data = JsonConvert.SerializeObject(fLA),
                            statusCode = fLA.respCode,
                            statusMessage = fLA.respMessage
                        };

                        var jsonResp = JsonConvert.SerializeObject(resp);

                        _logger.LogInformation(" Kiosk Web Service FLA Counter list - " + jsonResp);
                        return jsonResp;
                    }
                    else
                    {
                        reader.Close();
                        con.Close();

                        fLA.respCode = 0;
                        fLA.respMessage = "No data found.";

                        var resp = new
                        {
                            data = fLA.respMessage,
                            statusCode = fLA.respCode,
                            statusMessage = fLA.respMessage
                        };

                        var jsonResp = JsonConvert.SerializeObject(resp);

                        _logger.LogError(" Kiosk Web Service FLA Counter list - " + jsonResp);
                        return jsonResp;
                    }
                }
            }
            catch (Exception ex)
            {
                fLA.respCode = 0;
                fLA.respMessage = ex.Message.ToString();

                var resp = new
                {
                    data = fLA.respMessage,
                    statusCode = fLA.respCode,
                    statusMessage = fLA.respMessage
                };

                var jsonResp = JsonConvert.SerializeObject(resp);

                _logger.LogCritical(" Kiosk Web Service  FLA Counter list - " + jsonResp);
                return jsonResp;
            }
        }
    }
}
