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
    public class HomeController : Controller
    {

        private readonly TayanaContext db = new TayanaContext();
        public ActionResult Index()
        {
            var heroSlides = db.YachtHeroImages
                .Include(h => h.Yacht)
                .Where(h => h.Yacht.IsPublished)
                .OrderByDescending(h => h.Yacht.IsNew)
                .ThenBy(h => h.Yacht.SortOrder)
                .ThenByDescending(h => h.Yacht.Id)
                .Take(6)
                .ToList()
                .Select(ToHeroSlideVM)
                .ToList();

            var latestNews = db.News
                .Include(n => n.ThumbnailImage)
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate)
                .ThenByDescending(n => n.Id)
                .Take(3)
                .ToList()
                .Select(ToNewsItemVM)
                .ToList();

            var vm = new HomePageVM
            {
                HeroSlides = heroSlides,
                LatestNews = latestNews
            };

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        private static void SplitYachtName(string yachtName, out string title, out string modelNumber)
        {
            title = "";
            modelNumber = "";

            if (String.IsNullOrWhiteSpace(yachtName))
            {
                return;
            }

            var parts = yachtName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length == 1)
            {
                title = parts[0].ToUpperInvariant();
                return;
            }
            title = String.Join(" ", parts.Take(parts.Length - 1)).ToUpperInvariant();
            modelNumber = parts[parts.Length - 1];
        }

        private static HomeHeroSlideVM ToHeroSlideVM(YachtHeroImage heroImage)
        {
            var yacht = heroImage.Yacht;

            string heroTitle;
            string heroModelNumber;
            SplitYachtName(yacht.Name, out heroTitle, out heroModelNumber);

            return new HomeHeroSlideVM
            {
                YachtId = yacht.Id,
                YachtName = yacht.Name,
                HeroTitle = heroTitle,
                HeroNumber = heroModelNumber,
                HeroImagePath = heroImage.FilePath,
                HeroImageAlt = heroImage.OriginalFileName,
                IsNew = yacht.IsNew
            };
        }

        private static HomeNewsItemVM ToNewsItemVM(News news)
        {
            return new HomeNewsItemVM
            {
                NewsId = news.Id,
                Title = news.Title,
                Summary = news.Summary,
                PublishDate = news.PublishDate,
                ThumbnailPath = news.ThumbnailImage != null ? news.ThumbnailImage.FilePath : null,
                ThumbnailAlt = news.ThumbnailImage != null ? news.ThumbnailImage.OriginalFileName : news.Title,
                IsPinned = news.IsPinned
            };
        }
    }
}