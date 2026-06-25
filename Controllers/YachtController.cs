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
            var yachts = db.Yachts.Where(y => y.IsPublished)
                .OrderByDescending(y => y.IsNew)
                .ThenBy(y => y.SortOrder)
                .ThenByDescending(y => y.Id)
                .ToList();

            if (!yachts.Any())
            {
                var noYacht = new YachtPageVM
                {
                    Yachts = yachts,
                    CurrentYacht = null,
                    ActiveTab = ParseYachtTab(tab)
                };
                return View(noYacht);
            }

            var currentYachtId = id.HasValue ? id.Value : yachts.First().Id;

            var currentYacht = db.Yachts
                .Include(y => y.DeckImgs)
                .Include(y => y.Interiors)
                .Include(y => y.Downloads)
                .SingleOrDefault(y => y.Id == currentYachtId && y.IsPublished);

            if(currentYacht == null)
            {
                return HttpNotFound();
            }

            var activeTab = ParseYachtTab(tab);

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