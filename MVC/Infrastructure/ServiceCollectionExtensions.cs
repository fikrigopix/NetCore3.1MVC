using DFPay.Infrastructure.Data.Context;
using DFPay.MVC.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DFPay.MVC.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseConnectionPerTenant(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ApplicationDbContext>((serviceProvider) =>
            {
                var tenant = serviceProvider.GetRequiredService<TenantInfo>(); // Get from parent service provider (ASP.NET MVC Pipeline)
                var name = tenant.Name.Split(',')[0];
                var connectionString = configuration.GetConnectionString(name);
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                var context = new ApplicationDbContext(options);
                return context;
            });

            services.AddScoped<InvoiceListDbContext>((serviceProvider) =>
            {
                var tenant = serviceProvider.GetRequiredService<TenantInfo>(); // Get from parent service provider (ASP.NET MVC Pipeline)
                var name = tenant.Name.Split(',')[1];
                var connectionString = configuration.GetConnectionString(name);
                var options = new DbContextOptionsBuilder<InvoiceListDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                var context = new InvoiceListDbContext(options);
                return context;
            });

            return services;
        }
    }
}
