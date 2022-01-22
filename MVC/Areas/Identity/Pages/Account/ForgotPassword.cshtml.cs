using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using DFPay.Application.Services;
using MimeKit;
using MimeKit.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;

namespace DFPay.MVC.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly MailService _mailService;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, MailService mailService)
        {
            _userManager = userManager;
            _mailService = mailService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Email")]
            [Required(ErrorMessage = "The {0} field is required.")]
            [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid Email Address.")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    TempData["WarningMessage"] = "Email does not exist or is not confirmed";
                    return Page();
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var redirectedUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var SendTo = Input.Email;
                var subject = "Desfran Payment System: Reset Password";

                #region Mail Content
                StringBuilder sb = new StringBuilder();
                sb.Append(@"<p>You've requested to reset password.</p>");
                sb.Append("<div style='border-radius: 2px;background-color:#007bff;display: inline-block;border-radius: 25px;'> <a href='" + redirectedUrl + "' target='_blank' style=' padding: 8px 12px; border-radius: 2px; font-family: Helvetica, Arial, sans-serif; font-size: 14px; color: #ffffff; text-decoration: none; font-weight: bold; display: inline-block;' > Reset Password </a></div>");
                sb.Append("<p>If you can't confirm by clicking the button, please copy paste the link below into your browser's address:");
                sb.Append("<br>" + redirectedUrl + "</p>");
                sb.Append("<br><p>For your security, this link will expire in 30 minutes time.</p>");
                sb.Append("<p>If you don't recognize this activity, please contact us immediately at");
                sb.Append("<br>support@desfranpaymentservice.com</p>");
                sb.Append("<br>");
                sb.Append("<p>Desfran Payment Team");
                sb.Append("<br>This is an automatic message, please do not reply.</p>");
                #endregion

                try
                {
                    string err = await _mailService.SendEmailAsync(SendTo, subject, new MimeMessage { Body = new TextPart(TextFormat.Html) { Text = sb.ToString()}});
                    if (err != "")
                    {
                        TempData["ErrorMessage"] = "Email not send. | " + err;
                        return Page();
                    }                    
                }
                catch (System.Exception)
                {
                    TempData["ErrorMessage"] = "Email not send.";
                    return Page();
                }

                TempData["InfoMessage"] = "Please check your email to reset your password.";
                return RedirectToPage("./Login");
            }

            return Page();
        }

        //[HttpPost]
        public IActionResult OnPostSetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
