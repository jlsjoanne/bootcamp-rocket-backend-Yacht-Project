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
using TayanaYachts.Models.ViewModels;

namespace TayanaYachts.Areas.Admin.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET Admin/News
        public ActionResult Index()
        {
            var news = db.News
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate);
            return View(news.ToList());
        }
    }
}