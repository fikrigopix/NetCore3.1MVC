using System;
using System.Collections.Generic;
using System.Text;

namespace DFPay.Application.ViewModels
{
    public class NotifyViewModel
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public NotificationType Type { get; set; }

    }

    public enum NotificationType
    {
        error,
        success,
        warning
    }
}
