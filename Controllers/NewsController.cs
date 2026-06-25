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
            var newsList = db.News
                .Include(n => n.ThumbnailImage)
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate)
                .ThenByDescending(n => n.Id);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(newsList.ToPagedList(pageNumber,pageSize));
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var news = db.News
                .Include(n => n.ThumbnailImage)
                .Include(n => n.Images)
                .Include(n => n.Files)
                .SingleOrDefault(n => n.Id == id && n.IsPublished);

            if(news == null)
            {
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