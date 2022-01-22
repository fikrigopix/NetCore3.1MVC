using DFPay.Application.Interfaces;
using DFPay.Application.Services;
using DFPay.Application.ViewModels;
using DFPay.MVC.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFPay.MVC.Controllers
{
    [Authorize("Authorization")]
    public class InvoiceController : NotifyController
    {
        private readonly ILogger<InvoiceController> _logger;
        private IInvoiceService _invoiceService;
        private MailService _mailService;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IStringLocalizer<DFPay.MVC.Lang.Lang> _loc;
        private readonly IConfiguration _configuration;

        public InvoiceController(ILogger<InvoiceController> logger, 
            IInvoiceService invoiceService, 
            MailService mailService,
            IHubContext<SignalRHub> hubcontext,
            IStringLocalizer<DFPay.MVC.Lang.Lang> loc,
            IConfiguration configuration)
        {
            _logger = logger;
            _invoiceService = invoiceService;
            _mailService = mailService;
            _hubContext = hubcontext;
            _loc = loc;
            _configuration = configuration;
        }

        public IActionResult Index(int? status, DateTime? startDate, DateTime? endDate, bool? unread)
        {
            InvoiceListViewModel model = new InvoiceListViewModel();
            ViewBag.status = status;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate;
            ViewBag.unread = unread;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetInvoice(int? status, DateTime? startDate, DateTime? endDate, bool? unread)
        {
            InvoiceListViewModel model = _invoiceService.GetInvoices();

            if (status.HasValue)
            {
                var filter = model.InvoiceList.Where(x => x.Status == status.Value);
                model.InvoiceList = filter;
            }
            if (startDate.HasValue)
            {
                var filter = model.InvoiceList.Where(x => x.InvoiceDate >= startDate);
                model.InvoiceList = filter;
            }
            if (endDate.HasValue)
            {
                var filter = model.InvoiceList.Where(x => x.InvoiceDate <= endDate);
                model.InvoiceList = filter;
            }
            if (unread.HasValue)
            {
                var filter = model.InvoiceList.Where(x => x.Unread == unread);
                model.InvoiceList = filter;
            }

            return Json(new
            {
                aaData = model.InvoiceList
            });
        }

        public IActionResult View(int id)
        {
            InvoiceViewModel invoice = _invoiceService.GetInvoiceById(id);

            if (invoice == null)
            {
                return NotFound();
            }

            _invoiceService.UpdateInvoiceReadedById(id);
            //call signalR to refresh the bell
            _hubContext.Clients.All.SendAsync("RefreshTheBell");

            return View(invoice);
        }

        public IActionResult Pay(string hash)
        {
            InvoiceViewModel invoice = _invoiceService.GetInvoiceByHash(hash);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        public IActionResult Create()
        {
            InvoiceViewModel model = new InvoiceViewModel();
            model.InvoiceDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(InvoiceViewModel model,
            string[] InvoiceItemsDescription,
            int[] InvoiceItemsQuantity,
            decimal[] InvoiceItemsUnitPrice,
            decimal[] InvoiceItemsTotalPrice)
        {
            if (ModelState.IsValid)
            {
                if (InvoiceItemsDescription.Length > 0 && InvoiceItemsQuantity.Length > 0 && InvoiceItemsUnitPrice.Length > 0 && InvoiceItemsTotalPrice.Length > 0)
                {
                    List<InvoiceItemsViewModel> invoiceItemsViewModel = new List<InvoiceItemsViewModel>();

                    for (int i = 0; i < InvoiceItemsDescription.Length; i++)
                    {
                        InvoiceItemsViewModel oInvoiceItemsViewModel = new InvoiceItemsViewModel()
                        {
                            Description = InvoiceItemsDescription[i].Trim(),
                            Quantity = InvoiceItemsQuantity[i],
                            UnitPrice = InvoiceItemsUnitPrice[i],
                            TotalPrice = InvoiceItemsTotalPrice[i]
                        };

                        invoiceItemsViewModel.Add(oInvoiceItemsViewModel);
                    }

                    model.InvoiceItems = invoiceItemsViewModel;
                }

                string hash = _invoiceService.AddNewInvoice(model);
                                
                if (!string.IsNullOrEmpty(hash))            // Add new invoice and return success Invoice Hash
                {
                    Notify(new List<NotifyViewModel>
                    {
                        new NotifyViewModel()            // Success Notification
                        {
                            Title = _loc["Success"],
                            Message = _loc["Invoice created & emailed to client."],
                            Type = NotificationType.success
                        }
                    });

                    await _mailService.SendEmailAsync(model.ClientEmail, "DF Pay: Here's your payment link", new MimeMessage { Body = new TextPart(TextFormat.Html) { Text = EmailContent(model.ClientName, model.Amount, hash) } });       // Email to client
                }
                else
                {
                    Notify(new List<NotifyViewModel>
                    {
                        new NotifyViewModel()
                        {
                            Title = _loc["Failed"],
                            Message = _loc["Please contact the admin for support."],
                            Type = NotificationType.error
                        }
                    });
                }
            }
            else
            {
                Notify(new List<NotifyViewModel>
                {
                    new NotifyViewModel()
                    {
                        Title = _loc["Failed"],
                        Message = _loc["Please contact the admin for support."],
                        Type = NotificationType.error
                    }
                });
            }            

            return RedirectToAction("Index");
        }

        private string EmailContent(string Name, decimal Amount, string Hash)
        {
            var redirectedUrl = "";

            var hostName1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("Hostname");
            var payment1 = _configuration.GetSection("Multitenant").GetSection("Web1").GetValue<string>("Payment");

            var hostName2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("Hostname");
            var payment2 = _configuration.GetSection("Multitenant").GetSection("Web2").GetValue<string>("Payment");

            string hostname = HttpContext.Request.Host.Value;
            if (hostname == hostName1)
            {
                redirectedUrl = "https://" + payment1 + "/Invoice?p=" + Hash;
            }
            else if (hostname == hostName2)
            {
                redirectedUrl = "https://" + payment2 + "/Invoice?p=" + Hash;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<p>Dear " + Name + "</p>");
            sb.Append("<p>You've requested to deposit <b>USD " + Amount.ToString("F2") + " </b></p>");
            sb.Append("<br><p>Before going further with the deposit, please ensure the payment amount is correct.</p>");
            sb.Append("<div style='border-radius: 2px;background-color:#007bff;display: inline-block;border-radius: 25px;'> <a href='" + redirectedUrl + "' target='_blank' style=' padding: 8px 12px; border-radius: 2px; font-family: Helvetica, Arial, sans-serif; font-size: 14px; color: #ffffff; text-decoration: none; font-weight: bold; display: inline-block;' > Confirm Deposit </a></div>");
            sb.Append("<p>If you can't confirm by clicking the button, please copy paste the link below into your browser's address:");
            sb.Append("<br>" + redirectedUrl + "</p>");
            sb.Append("<br><p>For your security, this link will expire in 30 minutes time.</p>");
            sb.Append("<p>If you don't recognize this activity, please contact us immediately at");
            sb.Append("<br>payment@desfran.com</p>");
            sb.Append("<br>");
            sb.Append("<p>Desfran Payment Team");
            sb.Append("<br>This is an automatic message, please do not reply.</p>");

            return sb.ToString();
        }

        [HttpPost]
        
        public async Task<IActionResult> Resend(int id)
        {
            if (ModelState.IsValid)
            {
                InvoiceViewModel invoice = _invoiceService.GetInvoiceById(id);                                  // Check if Invoice exists

                if(invoice == null)
                {
                    return Json(new { success = false, message = _loc["Error while resending invoice"].Value });
                }
                else
                { 
                    if (_invoiceService.UpdateInvoiceStatusById(id, (int)InvoiceStatus.Pending))
                    {
                        invoice = _invoiceService.GetInvoiceById(id);                                           // Get updated Invoice data      
                                                
                        await _mailService.SendEmailAsync(invoice.ClientEmail, "DF Pay: Here's your updated payment link", new MimeMessage { Body = new TextPart(TextFormat.Html) { Text = EmailContent(invoice.ClientName, invoice.Amount, invoice.Hash) } });       // Email to client

                        return Json(new { success = true, message = _loc["Invoice resend successful"].Value });
                    }
                }
            }

            return Json(new { success = false, message = _loc["Error while resending invoice"].Value });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (_invoiceService.UpdateInvoiceStatusById(id, (int)InvoiceStatus.Deleted))
                return Json(new { success = true, message = _loc["Delete successful"].Value });
            else
                return Json(new { success = false, message = _loc["Error while deleting"].Value });
        }
                
        [HttpGet]
        public IActionResult GetUnreadInvoice()
        {
            int unreadInvoice = _invoiceService.GetUnreadInvoice();
            return Json(new { unreadInvoice });
        }
    }
}
