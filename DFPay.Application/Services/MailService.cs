using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace DFPay.Application.Services
{
    public interface MailService
    {
        Task<string> SendEmailAsync(string toEmail, string subject, MimeMessage mimeMessage);
    }

    public class MailKitMailService : MailService
    {
        private IConfiguration _configuration;

        public MailKitMailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SendEmailAsync(string toEmail, string subject, MimeMessage mimeMessage)
        {
            string errSending = "";

            using (var smtpClient = new SmtpClient())
            {
                try
                {
                    var host = _configuration["SMTPHost"];
                    var port = Convert.ToInt32(_configuration["SMTPPort"]);
                    var from = _configuration["AdminEmail"];
                    var password = _configuration["AdminEmailPassword"];
                    var emailSenderName = _configuration["EmailSenderName"];
                    var emailSenderNoReply = _configuration["EmailSenderNoReply"];

                    mimeMessage.Subject = subject;
                    mimeMessage.Sender = new MailboxAddress(emailSenderName, emailSenderNoReply);
                    mimeMessage.From.Add(new MailboxAddress(emailSenderName, emailSenderNoReply));
                    mimeMessage.To.Add(new MailboxAddress(toEmail));
                    mimeMessage.ReplyTo.Add(new MailboxAddress(emailSenderName, emailSenderNoReply));

                    if (host.Contains("outlook"))
                    {
                        smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        smtpClient.Connect(host, port, false);
                        smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                        await smtpClient.AuthenticateAsync(from, password);
                    }
                    else if (host.Contains("gmail"))
                    {
                        await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable);

                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");

                        // Note: only needed if the SMTP server requires authentication
                        await smtpClient.AuthenticateAsync(from, password);
                    }
                    else
                    {
                        await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable);
                        await smtpClient.AuthenticateAsync(from, password);
                    }

                    await smtpClient.SendAsync(mimeMessage);
                    await smtpClient.DisconnectAsync(true);
                }
                catch (Exception Ex)
                {
                    errSending = Ex.Message.ToString();
                }
            }

            return errSending;
        }
    }
}