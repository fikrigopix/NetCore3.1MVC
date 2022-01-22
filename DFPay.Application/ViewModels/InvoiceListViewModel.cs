using DFPay.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DFPay.Application.ViewModels
{
    public class InvoiceListViewModel
    {
        public IEnumerable<Invoice> InvoiceList { get; set; }

        public InvoiceStatus StatusList { get; set; }

        public List<InvoiceItem> InvoiceItems { get; set; }
    }

    public enum InvoiceStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,
        [Display(Name = "Success")]
        Success = 2,
        [Display(Name = "Expired")]
        Expired = 3,
        [Display(Name = "Deleted")]
        Deleted = 4
    }
}
