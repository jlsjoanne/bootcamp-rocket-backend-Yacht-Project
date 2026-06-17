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

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var news = db.News.AsQueryable();


            if (!String.IsNullOrWhiteSpace(searchString))
            {
                news = news.Where(n => n.Title.Contains(searchString) || n.Content.Contains(searchString));
            }

            news = news.OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.PublishDate);

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View(news.ToPagedList(pageNumber, pageSize));
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
            var imageUploads = (newsVM.ImageUploads ?? new HttpPostedFileBase[0]);
            var fileUploads = (newsVM.FileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            foreach (var image in imageUploads.Where(i => i != null && i.ContentLength > 0))
            {
                if (!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("ImageUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach (var file in fileUploads)
            {
                if (!UploadHelper.IsFileValid(file, 0))
                {
                    ModelState.AddModelError("FileUploads", "One or more uploaded files are not allowed.");
                }
            }

            if (!newsVM.ThumbnailImageUploadIndex.HasValue)
            {
                ModelState.AddModelError("ThumbnailImageUploadIndex", "Please select one uploaded image as the thumbnail.");
            }
            else
            {
                var index = newsVM.ThumbnailImageUploadIndex.Value;

                if (index < 0 || index >= imageUploads.Length || imageUploads[index] == null || imageUploads[index].ContentLength == 0)
                {
                    ModelState.AddModelError("ThumbnailImageUploadIndex", "The selected thumbnail must have an uploaded image.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(newsVM);
            }

            var savedUploads = new List<UploadedFile>();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var news = ToNews(newsVM);

                    for (int i = 0; i < imageUploads.Length; i++)
                    {
                        var image = imageUploads[i];

                        if (image == null || image.ContentLength == 0)
                        {
                            continue;
                        }

                        var savedImage = UploadHelper.SaveUploadedFile<NewsImage>(image, "~/Images", Server, Url);
                        savedUploads.Add(savedImage);
                        news.Images.Add(savedImage);

                        if (i == newsVM.ThumbnailImageUploadIndex.Value)
                        {
                            news.ThumbnailImageId = savedImage.Id;
                        }
                    }

                    foreach (var file in fileUploads)
                    {
                        var savedFile = UploadHelper.SaveUploadedFile<NewsFile>(file, "~/Files", Server, Url);
                        savedUploads.Add(savedFile);
                        news.Files.Add(savedFile);
                    }

                    db.News.Add(news);
                    db.SaveChanges();

                    transaction.Commit();
                    return RedirectToAction("Index");
                }
                catch
                {
                    transaction.Rollback();

                    foreach (var upload in savedUploads)
                    {
                        UploadHelper.DeleteUploadedFile(upload, Server);
                    }

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
            var imageUploads = (newsVM.ImageUploads ?? new HttpPostedFileBase[0]);
            var fileUploads = (newsVM.FileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            foreach (var image in imageUploads.Where(i => i != null && i.ContentLength > 0))
            {
                if (!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("ImageUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach (var file in fileUploads)
            {
                if (!UploadHelper.IsFileValid(file, 0))
                {
                    ModelState.AddModelError("FileUploads", "One or more uploaded files are not allowed.");
                }
            }

            // Get News from DB

            var news = db.News
                .Include(n => n.Images)
                .Include(n => n.Files)
                .SingleOrDefault(n => n.Id == newsVM.Id);

            // Delete chosen uploaded images and files

            var deleteImageIds = newsVM.DeleteImageIds ?? new Guid[0];
            var deleteFileIds = newsVM.DeleteFileIds ?? new Guid[0];

            if (news == null)
            {
                return HttpNotFound();
            }

            // ThumbnailSelect Validation
            Guid? selectedExistingThumbnailId = null;
            int? selectedUploadThumbnailIndex = null;

            if (String.IsNullOrWhiteSpace(newsVM.ThumbnailSelection))
            {
                ModelState.AddModelError("ThumbnailSelection", "Please select one image as the thumbnail.");
            }
            else
            {
                var thumbnailValue = newsVM.ThumbnailSelection;


                if (thumbnailValue.StartsWith("existing:"))
                {
                    var idText = thumbnailValue.Substring("existing:".Length);
                    Guid parsedId;

                    if (!Guid.TryParse(idText, out parsedId))
                    {
                        ModelState.AddModelError("ThumbnailSelection", "The selected thumbnail image is invalid.");
                    }
                    else
                    {
                        selectedExistingThumbnailId = parsedId;
                        if (!news.Images.Any(i => i.Id == parsedId))
                        {
                            ModelState.AddModelError("ThumbnailSelection", "The selected thumbnail image is invalid.");
                        }
                        if (deleteImageIds.Contains(parsedId))
                        {
                            ModelState.AddModelError("ThumbnailSelection", "The selected thumbnail image cannot be deleted.");
                        }
                    }
                }
                else if (thumbnailValue.StartsWith("upload:"))
                {
                    var indexText = thumbnailValue.Substring("upload:".Length);
                    int index;
                    if (!int.TryParse(indexText, out index))
                    {
                        ModelState.AddModelError("ThumbnailSelection", "The selected thumbnail is invalid.");
                    }
                    else
                    {
                        selectedUploadThumbnailIndex = index;
                        if (index < 0 ||
                            index >= imageUploads.Length ||
                            imageUploads[index] == null ||
                            imageUploads[index].ContentLength == 0)
                        {
                            ModelState.AddModelError("ThumbnailSelection", "The selected thumbnail must have an uploaded image.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("ThumbnailSelection", "The selected thumbnail is invalid.");
                }

            }

            // Check ModelState

            if (!ModelState.IsValid)
            {
                var reloadNewsVM = ToNewsVM(news);
                reloadNewsVM.Title = newsVM.Title;
                reloadNewsVM.Content = newsVM.Content;
                reloadNewsVM.PublishDate = newsVM.PublishDate;
                reloadNewsVM.IsPinned = newsVM.IsPinned;
                reloadNewsVM.IsPublished = newsVM.IsPublished;
                reloadNewsVM.ThumbnailSelection = newsVM.ThumbnailSelection;
                return View(reloadNewsVM);
            }

            // Update News data


            var newSavedUploads = new List<UploadedFile>();
            var deletedExistingUploads = new List<UploadedFile>();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    UpdateNewsFromVM(news, newsVM);

                    foreach (var image in news.Images.Where(i => deleteImageIds.Contains(i.Id)).ToList())
                    {
                        deletedExistingUploads.Add(image);
                        db.NewsImages.Remove(image);
                    }

                    foreach (var file in news.Files.Where(i => deleteFileIds.Contains(i.Id)).ToList())
                    {
                        deletedExistingUploads.Add(file);
                        db.NewsFiles.Remove(file);
                    }

                    // Upload added image and file to related folder
                    for (int i = 0; i < imageUploads.Length; i++)
                    {
                        var image = imageUploads[i];

                        if (image == null || image.ContentLength == 0)
                        {
                            continue;
                        }

                        var savedImage = UploadHelper.SaveUploadedFile<NewsImage>(image, "~/Images", Server, Url);
                        newSavedUploads.Add(savedImage);
                        news.Images.Add(savedImage);

                        // Check Thumbnail
                        if (selectedUploadThumbnailIndex.HasValue && i == selectedUploadThumbnailIndex.Value)
                        {
                            news.ThumbnailImageId = savedImage.Id;
                        }
                    }

                    foreach (var file in fileUploads)
                    {
                        var savedFile = UploadHelper.SaveUploadedFile<NewsFile>(file, "~/Files", Server, Url);
                        newSavedUploads.Add(savedFile);
                        news.Files.Add(savedFile);
                    }

                    // if Thumbnail is Existing Image => assign news.ThumbnailImageId to that image
                    if (selectedExistingThumbnailId.HasValue)
                    {
                        news.ThumbnailImageId = selectedExistingThumbnailId.Value;
                    }

                    // save to db and return to news page

                    db.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    foreach (var upload in newSavedUploads)
                    {
                        UploadHelper.DeleteUploadedFile(upload, Server);
                    }

                    ModelState.AddModelError("", "Unable to save news. Please try again.");

                    var reloadNews = db.News
                        .Include(n => n.Images)
                        .Include(n => n.Files)
                        .SingleOrDefault(n => n.Id == newsVM.Id);

                    if(reloadNews == null)
                    {
                        return HttpNotFound();
                    }

                    var reloadNewsVM = ToNewsVM(reloadNews);
                    reloadNewsVM.Title = newsVM.Title;
                    reloadNewsVM.Content = newsVM.Content;
                    reloadNewsVM.PublishDate = newsVM.PublishDate;
                    reloadNewsVM.IsPinned = newsVM.IsPinned;
                    reloadNewsVM.IsPublished = newsVM.IsPublished;
                    reloadNewsVM.ThumbnailSelection = newsVM.ThumbnailSelection;
                    return View(reloadNewsVM);
                }
            }

            try
            {
                foreach (var upload in deletedExistingUploads)
                {
                    UploadHelper.DeleteUploadedFile(upload, Server);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Post-commit upload cleanup failed:" + ex);
            }
            

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
            foreach (var newsImage in news.Images)
            {
                UploadHelper.DeleteUploadedFile(newsImage, Server);
            }
            foreach (var newsFile in news.Files)
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
                ExistingFiles = news.Files.Select(f => f.ToExistingUploadFileVM()).ToList(),
                ThumbnailImageId = news.ThumbnailImageId
            };
        }
    }
}
