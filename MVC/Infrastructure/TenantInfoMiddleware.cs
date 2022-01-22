using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DFPay.MVC.Infrastructure
{
    public class TenantInfoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public TenantInfoMiddleware(RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tenantInfo = context.RequestServices.GetRequiredService<TenantInfo>();
            var tenantName = context.Request.Host.Value;

            var hostName1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("Hostname");
            var hostName2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("Hostname");

            if (tenantName == hostName1)
            {
                tenantInfo.Name = "DefaultConnection,InvoiceConnection";
            }
            else if (tenantName == hostName2)
            {
                tenantInfo.Name = "DefaultConnection2,InvoiceConnection2";
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
