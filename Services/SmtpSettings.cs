using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TayanaYachts.Services
{
    public class SmtpSettings
    {
        // SMTP server connection settings.
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }

        // Optional SMTP authentication settings.
        public string Username { get; set; }
        public string Password { get; set; }

        // Sender identity used in outgoing contact emails.
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }

        // Internal recipient for contact form notifications.
        public string ContactHostEmail { get; set; }
    }
}
