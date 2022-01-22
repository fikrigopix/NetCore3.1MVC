using DFPay.Application.Interfaces;
using DFPay.Application.ViewModels;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DFPay.Payment.Controllers
{
    public class InvoiceController : Controller
    {
        private static readonly ILog _log4net = LogManager.GetLogger(typeof(InvoiceController));
        private readonly ILogger<InvoiceController> _logger;
        private IInvoiceService _invoiceService;
        private IConfiguration _configuration;
        private readonly IStringLocalizer<DFPay.Payment.Lang.Lang> _loc;

        public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService, IConfiguration configuration, IStringLocalizer<DFPay.Payment.Lang.Lang> loc)
        {
            _logger = logger;
            _invoiceService = invoiceService;
            _configuration = configuration;
            _loc = loc;
        }

        [HttpGet]
        public IActionResult Index(string p)
        {
            InvoiceViewModel invoice = _invoiceService.GetInvoiceByHash(p);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpGet]
        public IActionResult PaymentClosed()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckoutRapyd(string hash)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            bool bIsSuccess = false;
            string Status = "Success";
            string errMsg = string.Empty;

            var invoice = _invoiceService.GetInvoiceByHash(hash);

            if (invoice == null)
            {
                return Json(new
                {
                    Status = bIsSuccess ? "Success" : "Failed",
                    Message = _loc["Invoice has expired."].Value
                });
            }

            string RapydAccountId = "";
            string RapydSecretKey = "";
            string RapydRedirectUrl = "";
            string RapydReturnUrl = "";

            string hostName1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("Hostname");
            string RapydAccountId1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("RapydAccountId"); //_configuration["RapydAccountId"];
            string RapydSecretKey1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("RapydSecretKey"); //_configuration["RapydSecretKey"];
            string RapydRedirectUrl1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("RapydRedirectUrl");
            string RapydReturnUrl1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("RapydReturnUrl");

            var hostName2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("Hostname");
            string RapydAccountId2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("RapydAccountId"); //_configuration["RapydAccountId"];
            string RapydSecretKey2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("RapydSecretKey"); //_configuration["RapydSecretKey"];
            string RapydRedirectUrl2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("RapydRedirectUrl");
            string RapydReturnUrl2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("RapydReturnUrl");

            var tenantName = HttpContext.Request.Host.Value;
            if (tenantName == hostName1)
            {
                RapydAccountId = RapydAccountId1;
                RapydSecretKey = RapydSecretKey1;
                RapydRedirectUrl = RapydRedirectUrl1;
                RapydReturnUrl = RapydReturnUrl1;
            }
            else if (tenantName == hostName2)
            {
                RapydAccountId = RapydAccountId2;
                RapydSecretKey = RapydSecretKey2;
                RapydRedirectUrl = RapydRedirectUrl2;
                RapydReturnUrl = RapydReturnUrl2;
            }

            #region Variables for rapyd checkout            
            string sign_string = "";
            string responseData = string.Empty;
            Random random = new Random();
            int randomNumber = random.Next(10000000, 99999999);
            string idempotency = hash; // Unique for each 'Create Payment' request.
            string http_method = "post"; // Lower case.
            string path = "/v1/checkout"; // Portion after the base URL.
            int salt = randomNumber; // Randomly generated for each request.                        
            Int32 unixTimestamp = (Int32)(DateTime.Now.AddHours(-int.Parse(_configuration["RapydOffsetInHour"].ToString())).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string access_key = RapydAccountId;
            string secret_key = RapydSecretKey;
            string merchant_reference_id = hash;
            var complete_payment_url = RapydRedirectUrl;
            var error_payment_url = RapydReturnUrl;
            #endregion

            #region Calling Rapyd
            try
            {
                string body_string = "{\"amount\":" + Convert.ToDouble(invoice.Amount) + ",\"complete_payment_url\":\"" + complete_payment_url
                    + "\",\"country\":\"" + _configuration["RapydCountryCode"] + "\",\"currency\":\"" + _configuration["RapydCurrencyCode"] + "\",\"customer\":null,\"error_payment_url\":\"" + error_payment_url
                    + "\",\"merchant_reference_id\":\"" + merchant_reference_id +
                    "\",\"metadata\":{\"merchant_defined\":true},\"payment_method_type\":null,\"payment_method_type_categories\":[\"bank_redirect\",\"bank_transfer\",\"card\",\"cash\"]}";

                using (var content = new StringContent(
                        body_string,
                        Encoding.UTF8, "application/json"))
                {
                    sign_string = http_method + path + salt.ToString() + unixTimestamp + access_key + secret_key + body_string;
                    var sign_string_hmac256 = hashHMAC256(secret_key, sign_string);
                    byte[] sign_string_hmac256_encodedByte = System.Text.ASCIIEncoding.ASCII.GetBytes(sign_string_hmac256);
                    string sign_string_base64 = Convert.ToBase64String(sign_string_hmac256_encodedByte);
                    var baseAddress = new Uri(_configuration["RapydUrl"]);

                    using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("access_key", access_key);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("salt", salt.ToString());
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("timestamp", Convert.ToString(unixTimestamp));
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("signature", sign_string_base64);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("idempotency", idempotency);

                        using (var response = await httpClient.PostAsync(path, content))
                        {
                            responseData = await response.Content.ReadAsStringAsync();
                        }

                        var jsonResponseData = (JObject)JsonConvert.DeserializeObject(responseData);

                        if (jsonResponseData["status"]["status"].Value<string>() == "SUCCESS")
                        {
                            bIsSuccess = true;
                        }
                        else
                        {
                            bIsSuccess = false;
                            errMsg = _loc["Failed to deposit, please contact support for assistance."].Value;
                            _log4net.Info("Error Rapyd - " + DateTime.Now + " " + jsonResponseData["status"]["error_code"].Value<string>() + " | " + jsonResponseData["status"]["message"].Value<string>());
                        }
                    }
                }
                var Result = Json(responseData);

                return Json(new
                {
                    Status = bIsSuccess ? "Success" : "Failed",
                    Result,
                    Message = errMsg
                });
            }
            catch (Exception ex)
            {
                Status = "Failed";
                errMsg = ex.Message;
                _log4net.Error("Error Rapyd - " + errMsg);
            }

            return Json(new
            {
                Status = bIsSuccess ? "Success" : "Failed",
                Message = errMsg
            });
            #endregion
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
    }
}
