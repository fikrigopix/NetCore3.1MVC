using DFPay.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFPay.Domain.Interfaces
{
    public interface IInvoiceRepository
    {
        IEnumerable<Invoice> GetInvoices();
        Invoice GetInvoiceById(int id);
        Invoice GetInvoiceByHash(string hash);

        bool UpdateInvoice(Invoice invoice);
        bool AddNewInvoice(Invoice invoice);
        int AddInvoice(Invoice invoice);

        int GetUnreadInvoice();

        void Update(Invoice invoice);

        //=================== Start Invoice Item Repository ====================
        IEnumerable<InvoiceItem> GetInvoiceItems();
        InvoiceItem GetInvoiceItemById(int id);
        IEnumerable<InvoiceItem> GetInvoiceItemsByInvoiceId(int id);
        bool UpdateInvoiceItem(InvoiceItem invoiceItem);
        bool AddInvoiceItem(InvoiceItem invoiceItem);
        bool AddInvoiceItems(List<InvoiceItem> invoiceItems);
        //=================== End Invoice Item Repository ====================
    }
}
