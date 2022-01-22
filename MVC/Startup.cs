using DFPay.MVC.Data.Services;
using DFPay.Application.Services;
using DFPay.Infrastructure.Data.Context;
using DFPay.IoC;
using DFPay.MVC.Hubs;
using DFPay.MVC.Lang;
using DFPay.MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using DFPay.MVC.Data.Context;
using System;
using System.Collections.Generic;
using System.Globalization;
using DFPay.MVC.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace DFPay.MVC
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
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("DefaultConnection")));

            //services.AddDbContext<InvoiceListDbContext>(options =>
            //{
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("InvoiceConnection"));
            //});
            #endregion

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Tokens.PasswordResetTokenProvider = "CustomPasswordResetTokenProvider";
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<CustomPasswordResetTokenProvider
                <IdentityUser>>("CustomPasswordResetTokenProvider");

            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Authorization", policyCorrectUser =>
                {
                    policyCorrectUser.Requirements.Add(new AuthorizationRequirement());
                });
            });

            //Changes token life span of all token types
            services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromDays(1));
            //Changes token life span of password reset confirmation token
            services.Configure<CustomPasswordResetTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromMinutes(30));

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;
            });

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
            services.AddSignalR();

            services.AddMvc(o =>
            {
                ////Add Authentication to all Controllers by default.
                //var policy = new AuthorizationPolicyBuilder()
                //    .RequireAuthenticatedUser()
                //    .Build();
                //o.Filters.Add(new AuthorizeFilter(policy));

            })
                // =========== Start Multilanguage #2 =============
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(DFPay.MVC.Lang.Lang));
                });
                // =========== End Multilanguage #2 =============

            //============================== Start IP SafeList =================================
            services.AddScoped<ClientIpCheckControllerOrAction>(container =>
            {
                var loggerFactory = container.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ClientIpCheckControllerOrAction>();

                return new ClientIpCheckControllerOrAction(
                    Configuration["RapydSafeListIPs"], logger);
            });
            //============================== End IP SafeList =================================
                            
            DependencyContainer.RegisterServices(services);

            services.AddTransient<MailService, MailKitMailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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

            app.UseAuthentication();
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
                endpoints.MapHub<SignalRHub>("/signalr-hub");
            });
        }
        private static void RegisterServices(IServiceCollection services)
        {
            DependencyContainer.RegisterServices(services);
        }
    }
}
