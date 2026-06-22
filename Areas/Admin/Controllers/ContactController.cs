using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TayanaYachts.DAL;
using TayanaYachts.Models;

namespace TayanaYachts.Areas.Admin.Controllers
{
    public class ContactController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/Contact
        public ActionResult Index()
        {
            var contacts = db.Contacts.Include(c => c.Country).Include(c => c.Yacht);
            return View(contacts.ToList());
        }

        // GET: Admin/Contact/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts
                .Include(c => c.Country)
                .Include(c => c.Yacht)
                .SingleOrDefault(c => c.Id == id.Value);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsComplete(Guid id)
        {
            var contact = db.Contacts.Find(id);

            if(contact == null)
            {
                return HttpNotFound();
            }

            contact.IsCompleted = true;
            db.SaveChanges();

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
    }
}
