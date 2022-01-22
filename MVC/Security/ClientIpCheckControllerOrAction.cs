using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;

namespace DFPay.MVC.Security
{
    public class ClientIpCheckControllerOrAction : ActionFilterAttribute
    {
        private readonly ILogger _logger;
        private readonly string _safelist;

        public ClientIpCheckControllerOrAction(string safelist, ILogger logger)
        {
            _safelist = safelist;
            _logger = logger;
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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = GetRemoteIPAddress(context.HttpContext);
            _logger.LogDebug("Remote IpAddress: {RemoteIp}", remoteIp);
            var ip = _safelist.Split(';');
            var badIp = true;

            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            foreach (var address in ip)
            {
                var testIp = IPAddress.Parse(address);

                if (testIp.Equals(remoteIp))
                {
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {
                _logger.LogWarning("Forbidden Request from IP: {RemoteIp}", remoteIp);
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}