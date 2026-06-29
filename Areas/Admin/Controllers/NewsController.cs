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
using System.Data.Entity.Validation;
using System.Text;
using System.Diagnostics;

namespace TayanaYachts.Areas.Admin.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/News
        public ActionResult Index(string searchString, string currentFilter, int? page)
        {

            // Start from page 1 when a new search term is submitted.
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                // Keep the previous search term while moving between paged results.
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var news = db.News.AsQueryable();


            if (!String.IsNullOrWhiteSpace(searchString))
            {
                // Search the main text fields shown in the admin news list.
                news = news.Where(n =>
                    n.Title.Contains(searchString) ||
                    (n.Summary != null && n.Summary.Contains(searchString)) ||
                    (n.Content != null && n.Content.Contains(searchString)));
            }

            // Pinned news appears first, then newer publish dates.
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
                // Details displays uploaded images and files, including thumbnail labeling.
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
            // Normalize posted upload arrays so validation and saving can handle empty file inputs.
            var imageUploads = (newsVM.ImageUploads ?? new HttpPostedFileBase[0]);
            // filter out null or empty file for fileUploads array
            var fileUploads = (newsVM.FileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            // For non empty image file in upload array, validate image file
            foreach (var image in imageUploads.Where(i => i != null && i.ContentLength > 0))
            {
                // Validate each uploaded image before anything is saved to disk.
                if (!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("ImageUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach (var file in fileUploads)
            {
                // Validate general attachments using IsFileValid to make sure only approved file type can be saved.
                if (!UploadHelper.IsFileValid(file, 0))
                {
                    ModelState.AddModelError("FileUploads", "One or more uploaded files are not allowed.");
                }
            }

            // A new news item must select one uploaded image as its thumbnail.
            if (!newsVM.ThumbnailImageUploadIndex.HasValue)
            {
                ModelState.AddModelError("ThumbnailImageUploadIndex", "Please select one uploaded image as the thumbnail.");
            }
            else
            {
                var index = newsVM.ThumbnailImageUploadIndex.Value;

                // Guard against a thumbnail index that does not point to an actual uploaded image.
                if (index < 0 || index >= imageUploads.Length || imageUploads[index] == null || imageUploads[index].ContentLength == 0)
                {
                    ModelState.AddModelError("ThumbnailImageUploadIndex", "The selected thumbnail must have an uploaded image.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(newsVM);
            }

            // this variable is for recording which file is saved in transaction
            var savedUploads = new List<UploadedFile>();
            string step = "starting";

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    step = "mapping NewsVM to News";
                    // Convert the form-only view model into the persisted News entity.
                    var news = ToNews(newsVM);
                    NewsImage thumbnailImage = null;

                    step = "saving upload images";
                    for (int i = 0; i < imageUploads.Length; i++)
                    {
                        var image = imageUploads[i];

                        if (image == null || image.ContentLength == 0)
                        {
                            continue;
                        }

                        // Save the physical image file and attach its metadata to the News entity.
                        var savedImage = UploadHelper.SaveUploadedFile<NewsImage>(image, "~/Images", Server, Url);
                        savedUploads.Add(savedImage);
                        news.Images.Add(savedImage);

                        // Remember which saved image should become the thumbnail after the first database save.
                        if (i == newsVM.ThumbnailImageUploadIndex.Value)
                        {
                            thumbnailImage = savedImage;
                        }
                    }

                    step = "saving upload files";
                    foreach (var file in fileUploads)
                    {
                        // Save attachment files under ~/Files and attach their metadata to the News entity.
                        var savedFile = UploadHelper.SaveUploadedFile<NewsFile>(file, "~/Files", Server, Url);
                        savedUploads.Add(savedFile);
                        news.Files.Add(savedFile);
                    }

                    step = "adding News entity";
                    db.News.Add(news);

                    step = "saving news, images, and files to db before thumbnail";
                    db.SaveChanges();

                    if (thumbnailImage != null)
                    {
                        step = "saving thumbnail image reference";
                        // Save the News and its uploaded NewsImage rows first so EF can persist the
                        // NewsImage-NewsId relationship to the new News row.
                        // After the image exists as a child record, save again to store
                        // News.ThumbnailImageId back to that image. This avoids EF circular FK ordering
                        // errors and prevents the thumbnail selection from being left unsaved.
                        news.ThumbnailImageId = thumbnailImage.Id;
                        db.SaveChanges();
                    }

                    step = "commiting transaction";
                    transaction.Commit();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();

                    // Remove files already written to disk because the database transaction failed.
                    foreach (var upload in savedUploads)
                    {
                        UploadHelper.DeleteUploadedFile(upload, Server);
                    }

                    var details = new StringBuilder();
                    details.AppendLine("Create news fails at step:" + step);

                    foreach (var entityErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityErrors.ValidationErrors)
                        {
                            details.AppendLine(validationError.PropertyName + ": " + validationError.ErrorMessage);
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }

                    Trace.TraceError(details.ToString());

                    ModelState.AddModelError("", "Unable to save news. Please try again.");
                    return View(newsVM);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    // Remove files already written to disk because the database transaction failed.
                    foreach (var upload in savedUploads)
                    {
                        UploadHelper.DeleteUploadedFile(upload, Server);
                    }

                    Trace.TraceError("Create news failed at step: " + step + "\r\n" + ex);

                    ModelState.AddModelError("", ex.GetBaseException().Message);
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
            // Normalize posted upload arrays so optional new uploads can be validated safely.
            var imageUploads = (newsVM.ImageUploads ?? new HttpPostedFileBase[0]);
            // filter out null or empty file for fileUploads array
            var fileUploads = (newsVM.FileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            foreach (var image in imageUploads.Where(i => i != null && i.ContentLength > 0))
            {
                // Validate new images before saving replacements or additions.
                if (!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("ImageUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach (var file in fileUploads)
            {
                // Validate new attachment files before saving them to disk.
                if (!UploadHelper.IsFileValid(file, 0))
                {
                    ModelState.AddModelError("FileUploads", "One or more uploaded files are not allowed.");
                }
            }

            // Get News from DB

            var news = db.News
                // Load existing uploads so edit validation can compare deletes and thumbnail choices.
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
                    // Existing thumbnails are posted as existing:{imageId}.
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
                        // A deleted existing image cannot also be kept as the thumbnail.
                        if (deleteImageIds.Contains(parsedId))
                        {
                            ModelState.AddModelError("ThumbnailSelection", "The selected thumbnail image cannot be deleted.");
                        }
                    }
                }
                else if (thumbnailValue.StartsWith("upload:"))
                {
                    // New upload thumbnails are posted as upload:{imageUploadIndex}.
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
                // Rebuild existing upload lists while preserving the posted news field values.
                var reloadNewsVM = ToNewsVM(news);
                reloadNewsVM.Title = newsVM.Title;
                reloadNewsVM.Summary = newsVM.Summary;
                reloadNewsVM.Content = newsVM.Content;
                reloadNewsVM.PublishDate = newsVM.PublishDate;
                reloadNewsVM.IsPinned = newsVM.IsPinned;
                reloadNewsVM.IsPublished = newsVM.IsPublished;
                reloadNewsVM.ThumbnailSelection = newsVM.ThumbnailSelection;
                return View(reloadNewsVM);
            }

            // Update News data

            // store added image and files
            var newSavedUploads = new List<UploadedFile>();
            // store waiting to delete images and files
            var deletedExistingUploads = new List<UploadedFile>();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Apply scalar News fields from the edit form before changing uploads.
                    UpdateNewsFromVM(news, newsVM);
                    NewsImage newThumbnailImage = null;

                    foreach (var image in news.Images.Where(i => deleteImageIds.Contains(i.Id)).ToList())
                    {
                        // Remove selected image records inside the transaction; delete physical files after commit.
                        deletedExistingUploads.Add(image);
                        db.NewsImages.Remove(image);
                    }

                    foreach (var file in news.Files.Where(i => deleteFileIds.Contains(i.Id)).ToList())
                    {
                        // Remove selected file records inside the transaction; delete physical files after commit.
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

                        // Save each newly uploaded image and attach it to this News entity.
                        var savedImage = UploadHelper.SaveUploadedFile<NewsImage>(image, "~/Images", Server, Url);
                        newSavedUploads.Add(savedImage);
                        news.Images.Add(savedImage);

                        // Check Thumbnail
                        // Remember the new uploaded image selected as thumbnail until after the first save.
                        if (selectedUploadThumbnailIndex.HasValue && i == selectedUploadThumbnailIndex.Value)
                        {
                            newThumbnailImage = savedImage;
                        }
                    }

                    foreach (var file in fileUploads)
                    {
                        // Save each newly uploaded attachment and attach it to this News entity.
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

                    if (newThumbnailImage != null)
                    {
                        // Save the newly uploaded NewsImage first so EF can persist its NewsId
                        // relationship to this News row.
                        // After the image exists in the database, save again to store
                        // News.ThumbnailImageId back to that image. Without this second save, the new
                        // thumbnail selection would not be written.
                        news.ThumbnailImageId = newThumbnailImage.Id;
                        db.SaveChanges();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Trace.TraceError("Edit news failed: " + ex);

                    // Remove new files written during this failed edit attempt.
                    foreach (var upload in newSavedUploads)
                    {
                        UploadHelper.DeleteUploadedFile(upload, Server);
                    }

                    ModelState.AddModelError("", ex.GetBaseException().Message);

                    // Reload persisted uploads and restore the posted scalar values for redisplay.
                    var reloadNews = db.News
                        .Include(n => n.Images)
                        .Include(n => n.Files)
                        .SingleOrDefault(n => n.Id == newsVM.Id);

                    if (reloadNews == null)
                    {
                        return HttpNotFound();
                    }

                    var reloadNewsVM = ToNewsVM(reloadNews);
                    reloadNewsVM.Title = newsVM.Title;
                    reloadNewsVM.Summary = newsVM.Summary;
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
                // Delete old physical files only after the database changes commit successfully.
                foreach (var upload in deletedExistingUploads)
                {
                    UploadHelper.DeleteUploadedFile(upload, Server);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Post-commit upload cleanup failed:" + ex);
            }


            return RedirectToAction("Details", new { id = news.Id });
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
            var news = db.News
                // Load uploads so their physical files can be cleaned up after the database delete commits.
                .Include(n => n.Images)
                .Include(n => n.Files)
                .SingleOrDefault(n => n.Id == id);

            if (news == null)
            {
                return HttpNotFound();
            }

            var imagesToDelete = news.Images.ToList();
            var filesToDelete = news.Files.ToList();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Clear both sides of the thumbnail relationship before deleting this News row.
                    // News.ThumbnailImageId points to one of the NewsImage child records, while each
                    // NewsImage also points back to this News through NewsImage.NewsId.
                    // Saving this null thumbnail reference first breaks that circular FK dependency,
                    // so EF can delete the News row and its related uploads without ordering conflicts.
                    news.ThumbnailImageId = null;
                    news.ThumbnailImage = null;
                    db.SaveChanges();

                    db.News.Remove(news);

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Trace.TraceError("Delete news failed: " + ex);

                    ModelState.AddModelError("", ex.GetBaseException().Message);
                    return View(news);
                }
            }

            try
            {
                // Delete physical upload files only after the database delete commits.
                foreach (var image in imagesToDelete)
                {
                    UploadHelper.DeleteUploadedFile(image, Server);
                }
                foreach (var file in filesToDelete)
                {
                    UploadHelper.DeleteUploadedFile(file, Server);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Post-commit upload cleanup failed: " + ex);
            }

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

        // Map create-form fields to a new News entity; uploads are attached separately.
        private static News ToNews(NewsVM newsVM)
        { 
            return new News
            {
                Title = newsVM.Title,
                Summary = newsVM.Summary,
                Content = newsVM.Content,
                PublishDate = newsVM.PublishDate,
                IsPinned = newsVM.IsPinned,
                IsPublished = newsVM.IsPublished
            };
        }

        // Update only scalar News fields; upload changes are handled by the edit action.
        private static void UpdateNewsFromVM(News news, NewsVM newsVM)
        {
            news.Title = newsVM.Title;
            news.Summary = newsVM.Summary;
            news.Content = newsVM.Content;
            news.PublishDate = newsVM.PublishDate;
            news.IsPinned = newsVM.IsPinned;
            news.IsPublished = newsVM.IsPublished;
        }

        // Build the edit view model with existing uploads converted for display and selection.
        private static NewsVM ToNewsVM(News news)
        {
            return new NewsVM
            {
                Id = news.Id,
                Title = news.Title,
                Summary = news.Summary,
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
