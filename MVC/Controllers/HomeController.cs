using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFPay.Application.Interfaces;
using DFPay.Application.Services;
using DFPay.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DFPay.MVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace DFPay.MVC.Controllers
{
    [Authorize("Authorization")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MailService _mailService;
        private IInvoiceService _invoiceService;

        public HomeController(ILogger<HomeController> logger, 
            MailService mailService,
            IInvoiceService invoiceService)
        {
            _logger = logger;
            _mailService = mailService;
            _invoiceService = invoiceService;
        }

        public IActionResult Index()
        {
            var InvoiceListing = _invoiceService.GetInvoices();
            ViewBag.TotalInvoiceCounter = InvoiceListing.InvoiceList.Count();
            ViewBag.Percentage = ViewBag.TotalInvoiceCounter > 0 ? InvoiceListing.InvoiceList.Where(i => i.Status == (int)InvoiceStatus.Success).Count() * 100 / ViewBag.TotalInvoiceCounter : 0;
            ViewBag.PendingInvoiceCounter = InvoiceListing.InvoiceList.Where(i => i.Status == (int)InvoiceStatus.Pending).Count();
            ViewBag.ExpiredInvoiceCounter = InvoiceListing.InvoiceList.Where(i => i.Status == (int)InvoiceStatus.Expired).Count();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
