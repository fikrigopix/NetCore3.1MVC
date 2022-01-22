using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFPay.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 400:
                    ViewBag.ErrorMessage = "Bad Request";
                    ViewBag.ErrorCode = "400";
                    break;
                case 404:
                    ViewBag.ErrorMessage = "Sorry, your resource is not found";
                    ViewBag.ErrorCode = "404";
                    break;
                case 405:
                    ViewBag.ErrorMessage = "Ahh, method not allowed.";
                    ViewBag.ErrorCode = "405";
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Opps, something went wrong.";
                    ViewBag.ErrorCode = "500";
                    break;
            }

            return View("Error");
        }
    }
}
