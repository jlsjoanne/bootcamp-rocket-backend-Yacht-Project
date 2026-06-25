using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Configuration;
using System.Threading.Tasks;
using TayanaYachts.Models;

namespace TayanaYachts.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService()
        {
            _settings = LoadSettingFromWebConfig();
        }

        public async Task SendContactFormEmailAsync(Contact contact)
        {
            // Email 1: confirmation email to user
            string confirmationText = $@"
Hi {contact.Name},
Thank you for contacting us.

We have received your message:

Country: {contact.Country.Name}
Brochure of Interest: {contact.Yacht.Name}
Comment: {contact.Comment}

We will get back as soon as possible.";

            await SendEmailAsync(toEmail: contact.Email,
                subject: "Your message was received - Tayana Yachts",
                plainTextBody: confirmationText);

            // Email 2: notification email to host/admin

            string notificationText = $@"New Contact from Submission:
Name: {contact.Name}
Email: {contact.Email}
Phone: {contact.Phone}
Country: {contact.Country.Name}
Brochure of Interest: {contact.Yacht.Name}
Comment: {contact.Comment}";

            await SendEmailAsync(toEmail: _settings.ContactHostEmail,
                subject: $"New contact form submission: {contact.Id}",
                 plainTextBody:notificationText, replyToEmail: contact.Email);
                
        }

        private async Task SendEmailAsync(string toEmail, string subject, string plainTextBody, string replyToEmail = null)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderAddress));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            if( !String.IsNullOrWhiteSpace(replyToEmail))
            {
                email.ReplyTo.Add(MailboxAddress.Parse(replyToEmail));
            }

            email.Body = new TextPart("plain")
            {
                Text = plainTextBody
            };

            using(var smtp = new SmtpClient())
            {
                var secureSocketOption = _settings.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                await smtp.ConnectAsync(_settings.Host, _settings.Port, secureSocketOption);

                if( !String.IsNullOrWhiteSpace(_settings.Username))
                {
                    await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
                }

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
        }

        private static SmtpSettings LoadSettingFromWebConfig()
        {
            return new SmtpSettings
            {
                Host = ConfigurationManager.AppSettings["SmtpHost"],
                Port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]),
                UseSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpUseSsl"]),

                Username = ConfigurationManager.AppSettings["SmtpUsername"],
                Password = ConfigurationManager.AppSettings["SmtpPassword"],

                SenderName = ConfigurationManager.AppSettings["EmailSenderName"],
                SenderAddress = ConfigurationManager.AppSettings["EmailSenderAddress"],

                ContactHostEmail = ConfigurationManager.AppSettings["ContactHostEmail"]
            };
        }
    }
}