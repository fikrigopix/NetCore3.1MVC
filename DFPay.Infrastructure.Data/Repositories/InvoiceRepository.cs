using DFPay.Application.ViewModels;
using DFPay.Domain.Interfaces;
using DFPay.Domain.Models;
using DFPay.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFPay.Infrastructure.Data.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly InvoiceListDbContext _context;

        public InvoiceRepository(InvoiceListDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Invoice> GetInvoices()
        {
            foreach(var i in _context.Invoices)
            {
                if(i.Status == (int)InvoiceStatus.Pending)              // Check if Invoice status is pending, update to Expired if past Valid datetime
                {
                    if (i.ValidDate < DateTime.Now)
                    {
                        i.Status = (int)InvoiceStatus.Expired;
                    }
                }
            }
            _context.SaveChanges();

            _context.Invoices.Include(inv => inv.InvoiceItems).ToList();
            return _context.Invoices;                                   // Return updated Invoice List
        }

        public Invoice GetInvoiceById(int id)
        {
            Invoice invoice = new Invoice();

            invoice = _context.Invoices.Find(id);

            if(invoice == null)
            {
                return null;
            }
            invoice.InvoiceItems = _context.InvoicesItems.Where(w => w.InvoiceId == id).ToList();
            return invoice;
        }

        public Invoice GetInvoiceByHash(string hash)
        {
            Invoice invoice = new Invoice();
            invoice = _context.Invoices.FirstOrDefault(u => u.Hash == hash);

            if (invoice == null)
            {
                return null;
            }

            if (invoice.Status == (int)InvoiceStatus.Pending)              // Check if Invoice status is pending, update to Expired if past Valid datetime
            {
                if (invoice.ValidDate < DateTime.Now)
                {
                    invoice.Status = (int)InvoiceStatus.Expired;
                    _context.SaveChanges();
                    return null;
                }
            }
            else
            {
                return null;
            }

            invoice.InvoiceItems = _context.InvoicesItems.Where(w => w.InvoiceId == invoice.Id).ToList();
            return invoice;
        }

        public bool UpdateInvoice(Invoice invoice)
        {
            Invoice inv = GetInvoiceById(invoice.Id);
            if (invoice == null)
            {
                return false;
            }

            try
            {
                _context.Entry(inv).CurrentValues.SetValues(invoice);

                _context.SaveChanges();
            }
            catch(Exception)
            {
                return false;
            }
            
            return true;
        }

        public bool AddNewInvoice(Invoice invoice)
        {
            try
            {
                _context.Add(invoice);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int AddInvoice(Invoice invoice)
        {
            try
            {
                _context.Add(invoice);
                _context.SaveChanges();
                return invoice.Id;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int GetUnreadInvoice()
        {
            return _context.Invoices.Where(i => i.Unread == true).Count();
        }

        public void Update(Invoice invoice)
        {
            _context.Attach(invoice);
            _context.Entry(invoice).State = EntityState.Modified;
            _context.SaveChanges();
        }

        //=================== Start Invoice Item Repository ====================
        public IEnumerable<InvoiceItem> GetInvoiceItems()
        {
            return _context.InvoicesItems;
        }

        public InvoiceItem GetInvoiceItemById(int id)
        {
            InvoiceItem invoiceItem = new InvoiceItem();

            invoiceItem = _context.InvoicesItems.Find(id);

            if (invoiceItem == null)
            {
                return null;
            }
            return invoiceItem;
        }

        public IEnumerable<InvoiceItem> GetInvoiceItemsByInvoiceId(int id)
        {
            List<InvoiceItem> invoiceItems = new List<InvoiceItem>();

            invoiceItems = _context.InvoicesItems.Where(w => w.InvoiceId == id).ToList();

            if (invoiceItems == null)
            {
                return null;
            }
            return invoiceItems;
        }

        public bool UpdateInvoiceItem(InvoiceItem invoiceItem)
        {
            InvoiceItem inv = GetInvoiceItemById(invoiceItem.Id);

            if (invoiceItem == null)
            {
                return false;
            }

            try
            {
                _context.Entry(inv).CurrentValues.SetValues(invoiceItem);

                _context.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool AddInvoiceItem(InvoiceItem invoiceItem)
        {
            try
            {
                _context.Add(invoiceItem);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddInvoiceItems(List<InvoiceItem> invoiceItems)
        {
            try
            {
                _context.AddRange(invoiceItems);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //=================== End Invoice Item Repository ====================
    }
}
