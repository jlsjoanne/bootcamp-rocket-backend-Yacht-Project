using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TayanaYachts.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string SenderName { get; set; }
        public string SenderAddress { get; set; }

        public string ContactHostEmail { get; set; }
    }
}