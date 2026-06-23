using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TayanaYachts.DAL;
using TayanaYachts.Models;
using TayanaYachts.Models.ViewModels;
using System.Configuration;
using System.Net.Http;
using System.Web.Script.Serialization;
using TayanaYachts.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TayanaYachts.Controllers
{
    public class ContactController : Controller
    {
        private readonly TayanaContext db = new TayanaContext();

        // GET: Contact
        public ActionResult Index()
        {

            return View(BuildeContactPageVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ContactPageVM contactVM)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildeContactPageVM(contactVM.Form));
            }

            if (!IsRecaptchaValid())
            {
                ModelState.AddModelError("Recaptcha", "Please complete the verification.");
                return View(BuildeContactPageVM(contactVM.Form));
            }

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                Name = contactVM.Form.Name,
                Email = contactVM.Form.Email,
                Phone = contactVM.Form.Phone,
                CountryId = contactVM.Form.CountryId,
                YachtId = contactVM.Form.YachtId,
                Comment = contactVM.Form.Comment,
                IsCompleted = false
            };

            db.Contacts.Add(contact);
            db.SaveChanges();

            // Add send email feature

            // explicit loading:  load related data later, after you already have the entity
            db.Entry(contact).Reference(c => c.Country).Load();
            db.Entry(contact).Reference(c => c.Yacht).Load();

            try
            {
                var emailService = new EmailService();
                await emailService.SendContactFormEmailAsync(contact);
            }
            catch(Exception ex)
            {
                Trace.TraceError($"Failed to send contact form email for ContactId={contact.Id}. {ex}");
            }

            TempData["ContactSuccessMessage"] = "Submit success. Thank you for contacting us.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private ContactPageVM BuildeContactPageVM(ContactInputVM form = null)
        {
            var contactPageVM = new ContactPageVM
            {
                Form = form ?? new ContactInputVM(),
                Countries = db.Countries
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList(),
                Yachts = db.Yachts
                    .Where(y => y.IsPublished)
                    .OrderByDescending(y => y.Id)
                    .Select(y => new SelectListItem
                    {
                        Value = y.Id.ToString(),
                        Text = y.Name
                    })
                    .ToList()
            };
            return contactPageVM;
        }

        private bool IsRecaptchaValid()
        {
            var response = Request.Form["g-recaptcha-response"];

            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            var secretKey = ConfigurationManager.AppSettings["RecaptchaSecretKey"];

            using(var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", response },
                    { "remoteip", Request.UserHostAddress }
                };

                var content = new FormUrlEncodedContent(values);
                var result = client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content).Result;
                var json = result.Content.ReadAsStringAsync().Result;

                var serializer = new JavaScriptSerializer();
                var recaptchaResult = serializer.Deserialize<RecaptchaVerifyResponse>(json);

                return recaptchaResult != null && recaptchaResult.success;
            }
        }

        private class RecaptchaVerifyResponse
        {
            public bool success { get; set; }
        }
    }
}