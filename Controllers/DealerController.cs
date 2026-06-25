using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TayanaYachts.DAL;
using TayanaYachts.Models;
using TayanaYachts.Models.ViewModels;

namespace TayanaYachts.Controllers
{
    public class DealerController : Controller
    {
        private readonly TayanaContext db = new TayanaContext();

        // GET: Dealer
        public ActionResult Index(int? countryId)
        {
            var countries = db.Countries
                .Where(c => c.Areas.Any(a => a.Dealers.Any()))
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToList();

            if (!countries.Any())
            {
                var noDealer = new DealerPageVM
                {
                    Countries = countries,
                    SelectedCountry = null,
                    Dealers = new List<Dealer>()
                };
                return View(noDealer);
            }

            var selectedCountry = countryId.HasValue ? countries.SingleOrDefault(c => c.Id == countryId.Value) : null;

            if(selectedCountry == null)
            {
                selectedCountry = countries.First();
            }

            var dealers = db.Dealers
                .Include(d => d.Image)
                .Include(d => d.Area.Country)
                .Where(d => d.Area.CountryId == selectedCountry.Id)
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.Name)
                .ToList();

            return View(new DealerPageVM
            {
                Countries = countries,
                SelectedCountry = selectedCountry,
                Dealers = dealers
            });
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