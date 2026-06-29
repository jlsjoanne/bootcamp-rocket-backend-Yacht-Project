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
            // Show only countries that have at least one dealer under one of their areas,
            // so the left navigation does not link to empty dealer pages.
            var countries = db.Countries
                .Where(c => c.Areas.Any(a => a.Dealers.Any()))
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToList();

            if (!countries.Any())
            {
                // Return an empty page model when no dealer data exists instead of
                // running the default-country lookup below.
                var noDealer = new DealerPageVM
                {
                    Countries = countries,
                    SelectedCountry = null,
                    Dealers = new List<Dealer>()
                };
                return View(noDealer);
            }

            // Use the requested country when it is valid for the dealer list; otherwise
            // fall back to the first country in the ordered navigation list.
            var selectedCountry = countryId.HasValue ? countries.SingleOrDefault(c => c.Id == countryId.Value) : null;

            if(selectedCountry == null)
            {
                selectedCountry = countries.First();
            }

            // Eager-load image and area/country data because the public view renders each
            // dealer image, area name, and selected country details after the query completes.
            var dealers = db.Dealers
                .Include(d => d.Image)
                .Include(d => d.Area.Country)
                .Where(d => d.Area.CountryId == selectedCountry.Id)
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.Name)
                .ToList();

            // DealerPageVM carries all page sections: country navigation, selected country
            // heading/breadcrumb data, and the dealer list for that country.
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
