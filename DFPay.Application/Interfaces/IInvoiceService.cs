using DFPay.Application.ViewModels;
using DFPay.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFPay.Application.Interfaces
{
    public interface IInvoiceService
    {
        InvoiceListViewModel GetInvoices();

        InvoiceViewModel GetInvoiceById(int id);

        InvoiceViewModel GetInvoiceByHash(string hash);

        bool UpdateInvoiceStatusById(int id, int status);

        string AddNewInvoice(InvoiceViewModel model);

        int GetUnreadInvoice();

        bool UpdateInvoiceReadedById(int id);

        Invoice GetPendingInvoiceByHashAllFields(string hash);

        bool UpdateInvoicePaymentById(int id, int status);
    }
}
