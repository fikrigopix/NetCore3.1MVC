using DFPay.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFPay.Infrastructure.Data.Context
{
    public class InvoiceListDbContext : DbContext
    {
        public InvoiceListDbContext(DbContextOptions options) : base(options) { }
        public InvoiceListDbContext() { }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoicesItems { get; set; }
    }
}
