using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayanaYachts.Models;

namespace TayanaYachts.Services
{
    public interface IEmailService
    {
        Task SendContactFormEmailAsync(Contact contact);
    }
}
