using Microsoft.Extensions.Localization;
using System.Reflection;

namespace DFPay.MVC.Lang
{
    public class LangService
    {
        private readonly IStringLocalizer _localizer;

        public LangService(IStringLocalizerFactory factory)
        {
            var type = typeof(Lang);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create("Lang", assemblyName.Name);
        }

        public LocalizedString GetLocalizedHtmlString(string key)
        {
            return _localizer[key];
        }
    }
}
