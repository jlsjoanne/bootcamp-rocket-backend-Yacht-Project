using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TayanaYachts.DAL;
using TayanaYachts.Models;
using System.Data.Entity;

namespace TayanaYachts.Controllers
{
    public class NewsController : Controller
    {

        private readonly TayanaContext db = new TayanaContext();

        // GET: News
        public ActionResult Index(int? page)
        {
            // Public news list shows only published items whose publish date has arrived.
            // Include the thumbnail because the list view renders it for each row.
            var newsList = db.News
                .Include(n => n.ThumbnailImage)
                .Where(n => n.IsPublished && n.PublishDate <= DateTime.Today)
                // Pinned news stays at the top, then normal news follows by publish date.
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate)
                .ThenByDescending(n => n.Id);

            // The public view expects an IPagedList so it can render total count,
            // current page, previous/next links, and individual page numbers.
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(newsList.ToPagedList(pageNumber,pageSize));
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                // A details page cannot be resolved without the route id.
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Load all related upload data used by the details view: thumbnail first,
            // remaining gallery images next, and downloadable file attachments last.
            var news = db.News
                .Include(n => n.ThumbnailImage)
                .Include(n => n.Images)
                .Include(n => n.Files)
                // Keep unpublished news inaccessible from the public details route.
                .SingleOrDefault(n => n.Id == id && n.IsPublished);

            if(news == null)
            {
                // Return 404 for missing ids and for unpublished records.
                return HttpNotFound();
            }

            return View(news);
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
