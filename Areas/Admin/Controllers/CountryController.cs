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
    public class CountryController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/Country
        public ActionResult Index()
        {
            var Countries = db.Countries
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name);
            return View(Countries.ToList());
        }

        // GET: Admin/Country/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var country = db.Countries.Include(c => c.Areas).SingleOrDefault(c => c.Id == id);
            if (country == null)
            {
                return HttpNotFound();
            }
            return View(country);
        }

        // GET: Admin/Country/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Country/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Country country)
        {
            if (ModelState.IsValid)
            {
                // The Create view uses 0 to let the controller append the country after the current ordered list.
                if(country.SortOrder <= 0)
                {
                    country.SortOrder = GetNextSortOrder();
                }

                db.Countries.Add(country);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(country);
        }

        // GET: Admin/Country/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Country country = db.Countries.Find(id);
            if (country == null)
            {
                return HttpNotFound();
            }
            return View(country);
        }

        // POST: Admin/Country/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Country country)
        {
            if (ModelState.IsValid)
            {
                var countryToUpdate = db.Countries.Find(country.Id);

                if(countryToUpdate == null)
                {
                    return HttpNotFound();
                }

                countryToUpdate.Name = country.Name;

                if(country.SortOrder > 0)
                {
                    countryToUpdate.SortOrder = country.SortOrder;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(country);
        }

        // GET: Admin/Country/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Country country = db.Countries.Find(id);
            if (country == null)
            {
                return HttpNotFound();
            }
            return View(country);
        }

        // POST: Admin/Country/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Country country = db.Countries.Find(id);
            if(country == null)
            {
                return HttpNotFound();
            }

            db.Countries.Remove(country);
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

        // private methods
        // GetNextSortOrder if sort order not input
        private int GetNextSortOrder()
        {
            var maxSortOrder = db.Countries
                .Select(c => (int?)c.SortOrder)
                .Max();

            return (maxSortOrder ?? 0) + 10;
        }
    }
}
