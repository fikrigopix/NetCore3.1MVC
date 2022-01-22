using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFPay.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DFPay.MVC.Controllers
{
    public class NotifyController : Controller
    {
        public void Notify(List<NotifyViewModel> notifies)
        {
            var listNotif = new List<Notif>();
            foreach (var item in notifies)
            {
                var msg = new Notif
                {
                    Message = item.Title,
                    Title = item.Message,
                    Icon = item.Type.ToString(),
                    Type = item.Type.ToString(),
                    Provider = "toastr"
                };

                listNotif.Add(msg);
            }

            TempData["message"] = JsonConvert.SerializeObject(listNotif);
        }

        private class Notif
        {
            public string Message { get; set; }
            public string Title { get; set; }
            public string Icon { get; set; }
            public string Type { get; set; }
            public string Provider { get; set; }
        }
    }
}
