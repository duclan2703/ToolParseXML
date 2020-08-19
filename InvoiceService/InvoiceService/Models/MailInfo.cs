using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Models
{
    public class MailInfo
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string EnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string CCMail { get; set; }
    }
}
