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
using TayanaYachts.Methods;

namespace TayanaYachts.Areas.Admin.Controllers
{
    public class NewsController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/News
        public ActionResult Index()
        {
            // 未完成: 加搜尋框
            var news = db.News
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate);
            return View(news.ToList());
        }

        // GET: Admin/News/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }

        // GET: Admin/News/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/News/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewsVM newsVM)
        {
            var imageUploads = (newsVM.ImageUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var fileUploads = (newsVM.FileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            foreach(var image in imageUploads)
            {
                if(!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("ImageUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.")
                }
            }

            foreach(var file in fileUploads)
            {
                if (!UploadHelper.IsFileValid(file, 0))
                {
                    ModelState.AddModelError("FileUploads", "One or more uploaded files are not allowed.");
                }
            }

            if(!ModelState.IsValid)
            {
                return View(newsVM);
            }

            using(var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var news = new News
                    {
                        Title = newsVM.Title,
                        Content = newsVM.Content,
                        PublishDate = newsVM.PublishDate,
                        IsPinned = newsVM.IsPinned,
                        IsPublished = newsVM.IsPublished
                    };

                    foreach(var image in imageUploads)
                    {
                        news.Images.Add(UploadHelper.SaveUploadedFile<NewsImage>(image, "~/Images", Server, Url));
                    }

                    foreach(var file in fileUploads)
                    {
                        news.Files.Add(UploadHelper.SaveUploadedFile<NewsFile>(file, "~/Files", Server, Url));
                    }

                    db.News.Add(news);
                    db.SaveChanges();

                    transaction.Commit();
                    return RedirectToAction("Index");
                }
                catch
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Unable to save news. Please try again.");
                    return View(newsVM);
                }
            }
        }

        // GET: Admin/News/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }

        // POST: Admin/News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News news)
        {
            if (ModelState.IsValid)
            {
                db.Entry(news).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(news);
        }

        // GET: Admin/News/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }

        // POST: Admin/News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            News news = db.News.Find(id);
            foreach(var newsImage in news.Images)
            {
                UploadHelper.DeleteUploadedFile(newsImage, Server);
            }
            foreach(var newsFile in news.Files)
            {
                UploadHelper.DeleteUploadedFile(newsFile, Server);
            }
            db.News.Remove(news);
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


    }
}
