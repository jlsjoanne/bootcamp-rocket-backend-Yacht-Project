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
            // Build homepage carousel slides from the separate YachtHeroImage table.
            // Only hero images whose yacht is published should appear on the public homepage.
            var heroSlides = db.YachtHeroImages
                .Include(h => h.Yacht)
                .Where(h => h.Yacht.IsPublished)
                // Match the public yacht navigation order: newest models first, then
                // admin display order, then newest database records.
                .OrderByDescending(h => h.Yacht.IsNew)
                .ThenBy(h => h.Yacht.SortOrder)
                .ThenByDescending(h => h.Yacht.Id)
                // Keep the carousel short so the homepage remains focused and fast to render.
                .Take(6)
                // Convert after materializing so formatting helpers run in memory instead
                // of inside the Entity Framework SQL projection.
                .Select(ToHeroSlideVM)
                .ToList();

            // Homepage news mirrors the public News list rules: published, already reached
            // its publish date, pinned first, then newest by date/id.
            var latestNews = db.News
                .Include(n => n.ThumbnailImage)
                .Where(n => n.IsPublished && n.PublishDate <= DateTime.Today)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate)
                .ThenByDescending(n => n.Id)
                // The homepage design renders only the first three news cards.
                .Take(3)
                .Select(ToNewsItemVM)
                .ToList();

            // HomePageVM keeps the view simple by separating carousel data from news-card data.
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
            // The homepage hero design displays the first word as the large title and
            // the remaining words/model number in a smaller span.
            title = "";
            modelNumber = "";

            if (String.IsNullOrWhiteSpace(yachtName))
            {
                return;
            }

            var parts = yachtName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            title = parts[0].ToUpperInvariant();
            if(parts.Length > 1)
            {
                modelNumber = String.Join(" ", parts.Skip(1));
            }
        }

        private static HomeHeroSlideVM ToHeroSlideVM(YachtHeroImage heroImage)
        {
            // Flatten the YachtHeroImage + Yacht relationship into exactly the fields the
            // homepage carousel needs: link target, display title, image, and "new" badge.
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
            // Flatten News and its optional thumbnail into the lightweight card model used
            // by the homepage news block. The view supplies a fallback image when needed.
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
