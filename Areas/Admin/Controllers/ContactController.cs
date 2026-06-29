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
    [Authorize]
    public class ContactController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/Contact
        public ActionResult Index()
        {
            // Hide soft-deleted contacts and load related data displayed by the Index view.
            var contacts = db.Contacts.Where(c => c.IsDeleted == false).Include(c => c.Country).Include(c => c.Yacht);
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
                // Details displays the selected contact's country and yacht names.
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
            // Mark the contact request as completed from the admin list or details page.
            var contact = db.Contacts.Find(id);

            if(contact == null)
            {
                return HttpNotFound();
            }

            contact.IsCompleted = true;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsIncomplete(Guid id)
        {
            // Reopen a completed contact request from the admin list or details page.
            var contact = db.Contacts.Find(id);
            
            if(contact == null)
            {
                return HttpNotFound();
            }

            contact.IsCompleted = false;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if(contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            // Soft delete keeps the submitted contact in the database while hiding it from admin lists.
            var contact = db.Contacts.Find(id);
            
            if(contact == null)
            {
                return HttpNotFound();
            }

            contact.IsDeleted = true;
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
