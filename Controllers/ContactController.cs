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

            // Build the page model with empty form data plus the dropdown options needed by the view.
            return View(BuildeContactPageVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ContactPageVM contactVM)
        {
            if (!ModelState.IsValid)
            {
                // Rebuild dropdown lists before returning the view; posted view models only carry
                // selected ids, not the SelectListItem collections needed to render the form again.
                return View(BuildeContactPageVM(contactVM.Form));
            }

            if (!IsRecaptchaValid())
            {
                ModelState.AddModelError("Recaptcha", "Please complete the verification.");
                // Keep the user's submitted values while restoring dropdown data after reCAPTCHA fails.
                return View(BuildeContactPageVM(contactVM.Form));
            }

            // Persist the public contact request as a Contact entity separate from the page VM.
            // Admin flags start false so the back office can track completion and soft deletion later.
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                Name = contactVM.Form.Name,
                Email = contactVM.Form.Email,
                Phone = contactVM.Form.Phone,
                CountryId = contactVM.Form.CountryId,
                YachtId = contactVM.Form.YachtId,
                Comment = contactVM.Form.Comment,
                IsCompleted = false,
                IsDeleted = false
            };

            db.Contacts.Add(contact);
            db.SaveChanges();

            // Explicitly load related display data after saving because the email body uses
            // Country.Name and Yacht.Name, while the posted form only provided their ids.
            db.Entry(contact).Reference(c => c.Country).Load();
            db.Entry(contact).Reference(c => c.Yacht).Load();

            try
            {
                // Email sending is attempted after the contact is stored so a mail failure
                // does not lose the submitted inquiry.
                var emailService = new EmailService();
                await emailService.SendContactFormEmailAsync(contact);
            }
            catch(Exception ex)
            {
                // Keep the user-facing submit flow successful even if SMTP is unavailable;
                // the saved Contact record remains available for admin follow-up.
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
            // This view model combines the user's form input with lookup data required
            // by the Contact page dropdowns.
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
                    // Only public/published yachts should be selectable from the website contact form.
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
            // Google posts this token from the rendered g-recaptcha widget.
            var response = Request.Form["g-recaptcha-response"];

            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            // Keep the secret server-side in Web.config and verify the browser token
            // directly with Google's siteverify endpoint.
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

                // Deserialize only the fields this controller needs from Google's response.
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
