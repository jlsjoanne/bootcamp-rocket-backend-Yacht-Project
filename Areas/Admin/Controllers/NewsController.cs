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
using PagedList;

namespace TayanaYachts.Areas.Admin.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/News
        public ActionResult Index(string searchString, string currentFilter, int? page)
        {

            if( searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var news = db.News.AsQueryable();
            

            if(!String.IsNullOrWhiteSpace(searchString))
            {
                news = news.Where(n => n.Title.Contains(searchString) || n.Content.Contains(searchString));
            }

            news = news.OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate);

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View(news.ToPagedList(pageNumber,pageSize));
        }

        // GET: Admin/News/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News news = db.News
                .Include(n => n.Images)
                .Include(n => n.Files)
                .SingleOrDefault(n => n.Id == id);
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
                .Where(i => i != null && i.ContentLength > 0)
                .ToList();
            var fileUploads = (newsVM.FileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            foreach(var image in imageUploads)
            {
                if(!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("ImageUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
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
                    var news = ToNews(newsVM);

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
            News news = db.News
                .Include(n => n.Images)
                .Include(n => n.Files)
                .SingleOrDefault(n => n.Id == id);
            if (news == null)
            {
                return HttpNotFound();
            }

            var newsVM = ToNewsVM(news);
            return View(newsVM);
        }

        // POST: Admin/News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NewsVM newsVM)
        {
            // Check upload new Images and Files
            var imageUploads = (newsVM.ImageUploads ?? new HttpPostedFileBase[0])
                .Where(i => i != null && i.ContentLength > 0)
                .ToList();
            var fileUploads = (newsVM.FileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            foreach(var image in imageUploads)
            {
                if(!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("ImageUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach(var file in fileUploads)
            {
                if(!UploadHelper.IsFileValid(file, 0))
                {
                    ModelState.AddModelError("FileUploads", "One or more uploaded files are not allowed.");
                }
            }

            // Get News from DB

            var news = db.News
                .Include(n => n.Images)
                .Include(n => n.Files)
                .SingleOrDefault(n => n.Id == newsVM.Id);

            if(news == null)
            {
                return HttpNotFound();
            }

            // Check ModelState

            if( !ModelState.IsValid)
            {
                var reloadNewsVM = ToNewsVM(news);
                reloadNewsVM.Title = newsVM.Title;
                reloadNewsVM.Content = newsVM.Content;
                reloadNewsVM.PublishDate = newsVM.PublishDate;
                reloadNewsVM.IsPinned = newsVM.IsPinned;
                reloadNewsVM.IsPublished = newsVM.IsPublished;
                return View(reloadNewsVM);
            }

            // Update News data

            UpdateNewsFromVM(news, newsVM);

            // Delete chosen uploaded images and files

            var deleteImageIds = newsVM.DeleteImageIds ?? new Guid[0];
            var deleteFileIds = newsVM.DeleteFileIds ?? new Guid[0];

            foreach (var image in news.Images.Where(i => deleteImageIds.Contains(i.Id)).ToList())
            {
                UploadHelper.DeleteUploadedFile(image, Server);
                db.NewsImages.Remove(image);
            }

            foreach(var file in news.Files.Where(i => deleteFileIds.Contains(i.Id)).ToList())
            {
                UploadHelper.DeleteUploadedFile(file, Server);
                db.NewsFiles.Remove(file);
            }

            // Upload added image and file to related folder

            foreach(var image in imageUploads)
            {
                UploadHelper.SaveUploadedFile<NewsImage>(image, "~/Images", Server, Url);
            }

            foreach(var file in fileUploads)
            {
                UploadHelper.SaveUploadedFile<NewsFile>(file, "~/Files", Server, Url);
            }

            // save to db and return to news page

            db.SaveChanges();

            return RedirectToAction("Details", news.Id);

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

        private static News ToNews(NewsVM newsVM)
        {
            return new News
            {
                Title = newsVM.Title,
                Content = newsVM.Content,
                PublishDate = newsVM.PublishDate,
                IsPinned = newsVM.IsPinned,
                IsPublished = newsVM.IsPublished
            };
        }

        private static void UpdateNewsFromVM(News news, NewsVM newsVM)
        {
            news.Title = newsVM.Title;
            news.Content = newsVM.Content;
            news.PublishDate = newsVM.PublishDate;
            news.IsPinned = newsVM.IsPinned;
            news.IsPublished = newsVM.IsPublished;
        }

        private static NewsVM ToNewsVM(News news)
        {
            return new NewsVM
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                PublishDate = news.PublishDate,
                IsPinned = news.IsPinned,
                IsPublished = news.IsPublished,
                ExistingImages = news.Images.Select(i => i.ToExistingUploadFileVM()).ToList(),
                ExistingFiles = news.Files.Select(f => f.ToExistingUploadFileVM()).ToList()
            };
        }
    }
}
