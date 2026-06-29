using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TayanaYachts.DAL;
using TayanaYachts.Models;
using TayanaYachts.Models.ViewModels;
using System.Data.Entity;

namespace TayanaYachts.Controllers
{
    public class YachtController : Controller
    {

        private readonly TayanaContext db = new TayanaContext();
        // GET: Yacht
        public ActionResult Index(int? id, string tab = "overview")
        {
            // Build the left-side yacht navigation from public yachts only, with latest
            // models first and admin display order used inside each group.
            var yachts = db.Yachts.Where(y => y.IsPublished)
                .OrderByDescending(y => y.IsNew)
                .ThenBy(y => y.SortOrder)
                .ThenByDescending(y => y.Id)
                .ToList();

            if (!yachts.Any())
            {
                // Return a page model with no current yacht so the view can show the
                // "No yacht available" state instead of failing on null data.
                var noYacht = new YachtPageVM
                {
                    Yachts = yachts,
                    CurrentYacht = null,
                    ActiveTab = ParseYachtTab(tab)
                };
                return View(noYacht);
            }

            // If no yacht id is supplied, use the first yacht from the same ordered
            // navigation list so the default page and sidebar stay consistent.
            var currentYachtId = id.HasValue ? id.Value : yachts.First().Id;

            // Load related data used by the public yacht page: interiors drive the
            // banner carousel, deck images drive the layout tab, and downloads render
            // on the overview tab.
            var currentYacht = db.Yachts
                .Include(y => y.DeckImgs)
                .Include(y => y.Interiors)
                .Include(y => y.Downloads)
                .SingleOrDefault(y => y.Id == currentYachtId && y.IsPublished);

            if(currentYacht == null)
            {
                // Return 404 when the id is missing from the database or points to an
                // unpublished yacht that should not be visible publicly.
                return HttpNotFound();
            }

            // Normalize the tab route value to the enum the view switches on.
            var activeTab = ParseYachtTab(tab);

            // YachtPageVM carries both the navigation list and the selected yacht/tab
            // state needed by the single public Yacht view.
            var yachtPageVM = new YachtPageVM
            {
                Yachts = yachts,
                CurrentYacht = currentYacht,
                ActiveTab = activeTab
            };
            
            return View(yachtPageVM);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        // parse yacht tab
        private static YachtTab ParseYachtTab(string tab)
        {
            // Accept friendly route strings and fall back to Overview for unknown,
            // empty, or missing tab values.
            switch((tab ?? "").ToLowerInvariant())
            {
                case "layout":
                    return YachtTab.Layout;
                case "specification":
                    return YachtTab.Specification;
                default:
                    return YachtTab.Overview;

            }
        }
    }
}
