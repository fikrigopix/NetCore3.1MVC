using DFPay.Application.Interfaces;
using DFPay.Application.ViewModels;
using DFPay.Domain.Interfaces;
using DFPay.Domain.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFPay.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly int ExpiryMinutes = 30;

        public IInvoiceRepository _invoiceRepository;
        private IConfiguration _configuration;

        public InvoiceService(IInvoiceRepository invoiceRepository, IConfiguration configuration)
        {
            _invoiceRepository = invoiceRepository;
            _configuration = configuration;
        }

        public InvoiceListViewModel GetInvoices()
        {
            return new InvoiceListViewModel()
            {
                InvoiceList = _invoiceRepository.GetInvoices()
            };
        }

        public InvoiceViewModel GetInvoiceById(int id)
        {
            var data = _invoiceRepository.GetInvoiceById(id);

            if (data == null)
                return null;

            InvoiceViewModel Invoice = new InvoiceViewModel()
            {
                InvoiceNo = data.InvoiceNo,
                ClientName =data.ClientName,
                ClientEmail = data.ClientEmail,
                Amount = data.Amount,
                InvoiceDate = data.InvoiceDate,
                Hash = data.Hash,
                Status = data.Status,
                Unread = data.Unread
            };

            List<InvoiceItemsViewModel> invoiceItemsViewModelList = new List<InvoiceItemsViewModel>();

            foreach (var item in data.InvoiceItems)
            {
                InvoiceItemsViewModel invoiceItemsViewModel = new InvoiceItemsViewModel()
                {
                    Description = item.Description,
                    Quantity = item.Qty,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                };

                invoiceItemsViewModelList.Add(invoiceItemsViewModel);
            }

            Invoice.InvoiceItems = invoiceItemsViewModelList;

            return Invoice;
        }

        public InvoiceViewModel GetInvoiceByHash(string hash)
        {
            var data = _invoiceRepository.GetInvoiceByHash(hash);

            if (data == null)
                return null;

            InvoiceViewModel Invoice = new InvoiceViewModel()
            {
                InvoiceNo = data.InvoiceNo,
                ClientName = data.ClientName,
                ClientEmail = data.ClientEmail,
                Amount = data.Amount,
                InvoiceDate = data.InvoiceDate,
                Hash = data.Hash,
                Status = data.Status
            };

            List<InvoiceItemsViewModel> invoiceItemsViewModelList = new List<InvoiceItemsViewModel>();

            foreach (var item in data.InvoiceItems)
            {
                InvoiceItemsViewModel invoiceItemsViewModel = new InvoiceItemsViewModel()
                {
                    Description = item.Description,
                    Quantity = item.Qty,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                };

                invoiceItemsViewModelList.Add(invoiceItemsViewModel);
            }

            Invoice.InvoiceItems = invoiceItemsViewModelList;

            if (new SecurityService().HashMatch(hash, data.InvoiceNo, data.Amount, _configuration["DFPayHashKey"]))
                return Invoice;
            else
                return null;
        }

        public bool UpdateInvoiceStatusById(int id, int status)
        {

            var data = _invoiceRepository.GetInvoiceById(id);

            if (data == null)
                return false;

            if (status == (int)InvoiceStatus.Pending)
            {
                data.ValidDate = DateTime.Now.AddMinutes(ExpiryMinutes);
                data.Hash = new SecurityService().Hash(data.InvoiceNo, data.Amount, _configuration["DFPayHashKey"]);   // Generate transaction Hash
            }
            else if (status == (int)InvoiceStatus.Success)
            {
                data.Unread = true;
            }
            data.Status = (byte)status;
            //Invoice Invoice = new Invoice()
            //{
            //    Id = data.Id,
            //    InvoiceNo = data.InvoiceNo,
            //    ClientName = data.ClientName,
            //    ClientEmail = data.ClientEmail,
            //    Amount = data.Amount,
            //    Description = data.Description,
            //    InvoiceDate = data.InvoiceDate,
            //    Hash = data.Hash,
            //    Status = (byte)status,
            //    ValidDate = data.ValidDate
            //};

            return _invoiceRepository.UpdateInvoice(data);
        }

        public string AddNewInvoice(InvoiceViewModel model)
        {

            Invoice inv = new Invoice()
            {
                InvoiceNo = model.InvoiceNo,
                InvoiceDate = model.InvoiceDate,
                Amount = model.Amount,
                ClientName = model.ClientName,
                ClientEmail = model.ClientEmail,
                Hash = new SecurityService().Hash(model.InvoiceNo, model.Amount, _configuration["DFPayHashKey"]),   // Generate transaction Hash
                Status = (int)InvoiceStatus.Pending,
                ValidDate = DateTime.Now.AddMinutes(ExpiryMinutes)
            };

            int invoiceId = _invoiceRepository.AddInvoice(inv);
            // Return Success Notification
            /*if (_invoiceRepository.AddNewInvoice(inv))*/  // Insert new invoice into DB
            if (invoiceId > 0)
            {
                List<InvoiceItem> invoiceItems = new List<InvoiceItem>();

                foreach (var item in model.InvoiceItems)
                {
                    InvoiceItem invoiceItem = new InvoiceItem()
                    {
                        InvoiceId = invoiceId,
                        Description = item.Description,
                        Qty = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    };

                    invoiceItems.Add(invoiceItem);
                }

                _invoiceRepository.AddInvoiceItems(invoiceItems);

                return inv.Hash;                        // Return Invoice Hash if success
            }
            else
            {
                return "";
            }
        }

        public int GetUnreadInvoice()
        {
            return _invoiceRepository.GetUnreadInvoice();
        }

        public bool UpdateInvoiceReadedById(int id)
        {
            var invoice = _invoiceRepository.GetInvoiceById(id);

            if (invoice == null) 
            {
                return false;
            }
            if (invoice.Unread.HasValue)
            {
                if (invoice.Unread.Value)
                {
                    invoice.Unread = false;
                    _invoiceRepository.Update(invoice);
                    return true;
                }
            }
            return false;
        }

        public bool UpdateInvoicePaymentById(int id, int status)
        {
            var invoice = _invoiceRepository.GetInvoiceById(id);

            if (invoice == null)
            {
                return false;
            }

            if (invoice.Status == (int)InvoiceStatus.Pending)
            {
                if (status == (int)InvoiceStatus.Success)
                {
                    invoice.PaymentDate = DateTime.Now;
                    invoice.Unread = true;
                    invoice.Status = (byte)status;

                    return _invoiceRepository.UpdateInvoice(invoice);
                }
            }

            return false;
        }

        public Invoice GetPendingInvoiceByHashAllFields(string hash)
        {
            return _invoiceRepository.GetInvoiceByHash(hash);
        }
    }
}