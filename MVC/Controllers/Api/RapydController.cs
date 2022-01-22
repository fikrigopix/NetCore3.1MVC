using DFPay.Application.Interfaces;
using DFPay.Application.ViewModels;
using DFPay.Domain.Models;
using DFPay.MVC.Hubs;
using DFPay.MVC.Models.RapydModels;
using DFPay.MVC.Security;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DFPay.MVC.Controllers.Api
{
    [AllowAnonymous]
    [ServiceFilter(typeof(ClientIpCheckControllerOrAction))]
    [Route("api/[controller]")]
    [ApiController]
    public class RapydController : Controller
    {
        private static readonly ILog _log4net = LogManager.GetLogger(typeof(RapydController));
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        private IInvoiceService _invoiceService;
        private readonly IHubContext<SignalRHub> _hubContext;

        public RapydController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IInvoiceService invoiceService, IHubContext<SignalRHub> hubcontext)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _invoiceService = invoiceService;
            _hubContext = hubcontext;
        }
        
        public static IPAddress GetRemoteIPAddress(HttpContext context)
        {
            string header = (context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault());
            if (IPAddress.TryParse(header, out IPAddress ip))
            {
                return ip;
            }
            return context.Connection.RemoteIpAddress;
        }

        [HttpPost]
        public async Task<IActionResult> DepositRapydResponse([FromBody] JsonElement value)
        {
            _log4net.Info("Start Processing Rapyd...");

            bool valid = true;
            string resultMsg = "";
            string log = "";
            string timestamp = string.Empty;
            string salt = string.Empty;
            string signature = string.Empty;
            string joParamsSerialize = string.Empty;
            ObjectResult objectRequestResult = null;
            Invoice oInvoice = new Invoice();
            var ip = GetRemoteIPAddress(Request.HttpContext);
                        
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                JObject myJObject = JObject.Parse(value.GetRawText());
                JsonSerializer serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
                RapydWebhook oRapydWebhook = (RapydWebhook)serializer.Deserialize(new JTokenReader(myJObject), typeof(RapydWebhook));
                joParamsSerialize = System.Text.Json.JsonSerializer.Serialize(value);

                string RapydAccountId = "";
                string RapydSecretKey = "";

                string hostName1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("Hostname");
                string RapydAccountId1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("RapydAccountId"); //_configuration["RapydAccountId"];
                string RapydSecretKey1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("RapydSecretKey"); //_configuration["RapydSecretKey"];

                var hostName2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("Hostname");
                string RapydAccountId2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("RapydAccountId"); //_configuration["RapydAccountId"];
                string RapydSecretKey2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("RapydSecretKey"); //_configuration["RapydSecretKey"];

                string RapydResponseUrl = _configuration["RapydResponseUrl"];

                var tenantName = HttpContext.Request.Host.Value;
                if (tenantName == hostName1)
                {
                    RapydAccountId = RapydAccountId1;
                    RapydSecretKey = RapydSecretKey1;
                }
                else if (tenantName == hostName2)
                {
                    RapydAccountId = RapydAccountId2;
                    RapydSecretKey = RapydSecretKey2;
                }

                timestamp = Request.Headers["timestamp"].ToString();
                salt = Request.Headers["salt"].ToString();
                signature = Request.Headers["signature"].ToString();

                string urlPathFix = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host.Value + "/" + RapydResponseUrl;
                urlPathFix = urlPathFix.ToLower();

                string crmSignToCalculated = urlPathFix + salt + timestamp + RapydAccountId + RapydSecretKey + joParamsSerialize;
                var sign_string_hmac256 = hashHMAC256(RapydSecretKey, crmSignToCalculated);
                byte[] sign_string_hmac256_encodedByte = System.Text.ASCIIEncoding.ASCII.GetBytes(sign_string_hmac256);
                string sign_string_base64 = Convert.ToBase64String(sign_string_hmac256_encodedByte);

                _log4net.Info("Start Processing Rapyd... [ " + oRapydWebhook.type + " ] | "
                    + "ipaddress : " + ip + " | "
                    + "timestamp : " + timestamp + " | "
                    + "salt : " + salt + " | "
                    + "signature : " + signature + " | "
                    + joParamsSerialize);

                #region Validation
                if (salt == "")
                {
                    valid = false;
                    resultMsg = "Salt Not Found";
                    return BadRequest(resultMsg);
                }
                if (timestamp == "")
                {
                    valid = false;
                    resultMsg = "Timestamp Not Found";
                    return BadRequest(resultMsg);
                }
                if (signature == "")
                {
                    valid = false;
                    resultMsg = "Signature Not Found";
                    return BadRequest(resultMsg);
                }
                //Start Remark signature comparisson
                //if (sign_string_base64 != signature)
                //{
                //    valid = false;
                //    resultMsg = "Sign Error : " + sign_string_base64;
                //    return Unauthorized(resultMsg);
                //}
                //End Remark signature comparisson

                oInvoice = _invoiceService.GetPendingInvoiceByHashAllFields(oRapydWebhook.data.merchant_reference_id);

                if (oInvoice == null)
                {
                    valid = false;
                    resultMsg = "Invoice Not Found.";
                    return BadRequest(resultMsg);
                }
                #endregion

                if (valid)
                {                    
                    if (oRapydWebhook.type == "PAYMENT_COMPLETED")
                    {
                        if (oRapydWebhook.data.status == "CLO")
                        {
                            if (oInvoice.Status == (int)InvoiceStatus.Pending)
                            {
                                if (await UpdateInvoiceSuccessById(oInvoice.ClientEmail, oInvoice.Id))
                                {
                                    valid = true;
                                    resultMsg = "Success";

                                    objectRequestResult = Ok(resultMsg);
                                }
                                else
                                {
                                    valid = false;
                                    resultMsg = "Failed";

                                    objectRequestResult = UnprocessableEntity(resultMsg);
                                }
                            }
                            else
                            {
                                valid = true;
                                resultMsg = "Success";

                                objectRequestResult = Ok(resultMsg);
                            }
                        }
                        else
                        {
                            valid = true;
                            resultMsg = "Success";

                            objectRequestResult = Ok(resultMsg);
                        }

                        #region Log
                        log = "{\"psp\": \"Rapyd_Valid\"" +
                            ",\"resultmsg\": " + resultMsg +
                            ",\"ipaddress\": " + ip +
                            ",\"salt\": " + salt +
                            ",\"timestamp\": " + timestamp +
                            ",\"signature\": " + signature +
                            ",\"webhookdatas\":" + joParamsSerialize + "}";
                        _log4net.Info(log);
                        #endregion

                        return objectRequestResult;
                    }

                    valid = true;
                    resultMsg = "Success";
                    return Ok(resultMsg);
                }
                else
                {
                    #region Error                                
                    log = "{\"psp\": \"Rapyd_Invalid\"" +
                        ",\"err\": \"" + resultMsg + "\"" +
                        ",\"ipaddress\": " + ip +
                        ",\"salt\": " + salt +
                        ",\"timestamp\": " + timestamp +
                        ",\"signature\": " + signature +
                        ",\"webhookdatas\":" + joParamsSerialize + "}";
                    _log4net.Info(log);
                    #endregion

                    return objectRequestResult;
                }
            }
            catch (Exception e)
            {
                valid = false;
                resultMsg = "Error";

                #region Error                                
                log = "{\"psp\": \"Rapyd_Error\"" +
                    ",\"resultmsg\": \"" + e.Message.ToString() + "\"" +
                    ",\"ipaddress\": " + ip +
                    ",\"salt\": " + salt +
                    ",\"timestamp\": " + timestamp +
                    ",\"signature\": " + signature +
                    ",\"webhookdatas\":" + joParamsSerialize + "}";
                _log4net.Error(log);
                #endregion
                return BadRequest(resultMsg);
            }            
        }

        private string hashHMAC256(string skey, string msg)
        {
            string key = skey;
            string message = msg;
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(key);

            HMACSHA256 hmacsha256 = new HMACSHA256(keyByte);

            byte[] messageBytes = encoding.GetBytes(message);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return ByteToString(hashmessage).ToLower();
        }

        private string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }

        public async Task<bool> UpdateInvoiceSuccessById(string user, int invoiceId)
        {
            if (_invoiceService.UpdateInvoicePaymentById(invoiceId, (int)InvoiceStatus.Success))
            {
                string message = user + " has successfully paid the invoice";
                //call signalR to broadcast the message
                await _hubContext.Clients.All.SendAsync("BroadcastMessagePaymentSuccess", invoiceId, message);

                return true;
            }

            return false;
        }
    }
}
