using DFPay.Infrastructure.Data.Context;
using DFPay.IoC;
using DFPay.Payment.Infrastructure;
using DFPay.Payment.Lang;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;

namespace DFPay.Payment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Multitenant //Disable this for Migration and Update-Database(code first)
            services.AddScoped<TenantInfo>(); // Adds a scoped tenant object, controlled via middleware (TenantInfoMiddleware)
            services.UseConnectionPerTenant(Configuration);
            #endregion

            #region Context //Enable this for Migration and Update-Database (code first)
            //services.AddDbContext<InvoiceListDbContext>(options =>
            //{
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("InvoiceConnection"));
            //});
            #endregion

            // =========== Start Multilanguage #1 =============
            services.AddSingleton<LangService>();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "";
            });

            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                        {
                            new CultureInfo("en-US"),
                            new CultureInfo("zh-CN")
                        };

                    options.DefaultRequestCulture = new RequestCulture(culture: "zh-CN", uiCulture: "zh-CN");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                    options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new QueryStringRequestCultureProvider(),
                        new CookieRequestCultureProvider()
                    };
                });

            // =========== End Multilanguage #1 =============
                        
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();

            // =========== Start Multilanguage #2 =============
            services.AddMvc(o=> { })
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(DFPay.Payment.Lang.Lang));
                });
            // =========== End Multilanguage #2 =============

            DependencyContainer.RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            #region Multitenant //Disable this for Migration and Update-Database(code first)
            app.UseMiddleware<TenantInfoMiddleware>();
            #endregion

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseAuthorization();

            //====================== Start Localization Datetime Format =========================
            app.Use(async (context, next) =>
            {
                var culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
                culture.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
                culture.NumberFormat.CurrencySymbol = "$";
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                await next();
            });
            //====================== End Localization Datetime Format =========================

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
        private static void RegisterServices(IServiceCollection services)
        {
            DependencyContainer.RegisterServices(services);
        }
    }
}
