using DFPay.Application.Interfaces;
using DFPay.Application.Services;
using DFPay.Domain.Interfaces;
using DFPay.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFPay.IoC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            //DFPay.Application
            services.AddScoped<IInvoiceService, InvoiceService>();

            //DFPay.Domain.Interfaces | DFPay.Infrastructure.Data.Repositories
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        }
    }
}
