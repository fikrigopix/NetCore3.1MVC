using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DFPay.MVC.Controllers
{
    public class ResourceController : Controller
    {
        public JsonResult GetResources()
        {
            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();            
            var culture = rqf.RequestCulture.Culture;
            string resourcePath = Environment.CurrentDirectory + "\\Lang\\";
            string fileName = culture.Name == "en-US" ? "Lang.en-US.resx" : "Lang.zh-CN.resx";
            var xml = System.IO.File.ReadAllText(@"" + resourcePath + fileName + "");
            var obj = new
            {
                Datas = XElement.Parse(xml)
                    .Elements("data")
                    .Select(el => new
                    {
                        id = el.Attribute("name").Value,
                        text = el.Element("value").Value.Trim()
                    })
                    .ToList()
            };

            var listOfTexts = obj.Datas;
            var toDictionary = listOfTexts.Select(s => new { s.id, s.text }).ToDictionary(x => x.id, x => x.text);
            
            return Json(new Dictionary<string, string>(toDictionary));
        }
    }
}
