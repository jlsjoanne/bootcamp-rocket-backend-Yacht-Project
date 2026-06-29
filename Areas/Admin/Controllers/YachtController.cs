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
using TayanaYachts.Methods;
using TayanaYachts.Models.ViewModels;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Web.WebPages.Html;
using PagedList;

namespace TayanaYachts.Areas.Admin.Controllers
{
    [Authorize]
    public class YachtController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/Yacht
        public ActionResult Index(string searchString, string currentFilter, int? page)
        {
            // Non-scaffolded list behavior: keep the search term while paging and reset
            // to page 1 when the user submits a new search from the Index view.
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var yachts = db.Yachts.AsQueryable();

            if (!String.IsNullOrWhiteSpace(searchString))
            {
                // Search both the display name and HTML overview content used on yacht pages.
                yachts = yachts.Where(y => 
                    y.Name.Contains(searchString) ||
                    (y.Overview != null && y.Overview.Contains(searchString)));
            }

            // Put latest models first, then use the admin-defined display order for ties.
            yachts = yachts.OrderByDescending(y => y.IsNew)
                .ThenBy(y => y.SortOrder)
                .ThenByDescending(y => y.Id);

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View(yachts.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Yacht/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Yacht yacht = db.Yachts
                .Include(y => y.DeckImgs)
                .Include(y => y.Interiors)
                .Include(y => y.Downloads)
                .SingleOrDefault(y => y.Id == id);
            if (yacht == null)
            {
                return HttpNotFound();
            }

            var heroImage = db.YachtHeroImages.SingleOrDefault(h => h.YachtId == yacht.Id);

            // Hero image is stored separately from the Yacht upload collections, so pass
            // a lightweight upload VM through ViewBag for the details page preview.
            ViewBag.HeroImage = heroImage?.ToExistingUploadFileVM();

            return View(yacht);
        }

        // GET: Admin/Yacht/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Yacht/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(YachtVM yachtVM)
        {
            // The create view posts repeatable file inputs with the same names; filter out
            // empty slots from rows the admin added but did not choose a file for.
            var deckImageUploads = (yachtVM.DeckImgsUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var interiorUploads = (yachtVM.InteriorUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var downloadFileUploads = (yachtVM.DownloadFileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            var heroImageUpload = yachtVM.HeroImageUpload;
            var hasHeroImageUpload = heroImageUpload != null && heroImageUpload.ContentLength > 0;

            foreach(var deckimage in deckImageUploads)
            {
                if (!UploadHelper.IsFileValid(deckimage, 1))
                {
                    ModelState.AddModelError("DeckImgsUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach(var interior in interiorUploads)
            {
                if (!UploadHelper.IsFileValid(interior, 1))
                {
                    ModelState.AddModelError("InteriorUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach(var download in downloadFileUploads)
            {
                if(!UploadHelper.IsFileValid(download, 0))
                {
                    ModelState.AddModelError("DownloadFileUploads", "One or more uploaded files are not allowed.");
                }
            }

            if(hasHeroImageUpload && !UploadHelper.IsFileValid(heroImageUpload, 1))
            {
                ModelState.AddModelError("HeroImageUpload", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
            }

            if(!ModelState.IsValid)
            {
                return View(yachtVM);
            }

            var savedUploads = new List<UploadedFile>();
            var deletedEditorImages = new List<YachtEditorImage>();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var yacht = ToYacht(yachtVM);

                    // Save each upload to disk first, then attach the saved metadata object
                    // to the correct Yacht collection so EF writes the FK relationship.
                    foreach(var deckImage in deckImageUploads)
                    {
                        var savedUpload = UploadHelper.SaveUploadedFile<YachtImage>(deckImage, "~/Images", Server, Url);
                        savedUploads.Add(savedUpload);
                        yacht.DeckImgs.Add(savedUpload);
                    }

                    foreach(var interior in interiorUploads)
                    {
                        var savedUpload = UploadHelper.SaveUploadedFile<YachtInterior>(interior, "~/Images", Server, Url);
                        savedUploads.Add(savedUpload);
                        yacht.Interiors.Add(savedUpload);
                    }

                    foreach(var download in downloadFileUploads)
                    {
                        var savedUpload = UploadHelper.SaveUploadedFile<YachtDownload>(download, "~/Files", Server, Url);
                        savedUploads.Add(savedUpload);
                        yacht.Downloads.Add(savedUpload);
                    }

                    db.Yachts.Add(yacht);
                    db.SaveChanges();

                    // Summernote uploads are created before the Yacht form is submitted.
                    // After the Yacht has an Id, bind images still referenced by editor HTML
                    // and mark unreferenced editor uploads for post-commit file cleanup.
                    var editorContent =
                        (yachtVM.Overview ?? "") +
                        (yachtVM.Dimensions ?? "") +
                        (yachtVM.Specification ?? "");

                    deletedEditorImages = HandleEditorImages(yacht.Id, editorContent);
                    db.SaveChanges();

                    if (hasHeroImageUpload)
                    {
                        // Homepage hero image uses its own table, not Yacht.DeckImgs, because
                        // it is displayed by homepage carousel logic rather than yacht detail sections.
                        var savedHeroImage = UploadHelper.SaveUploadedFile<YachtHeroImage>(heroImageUpload, "~/Images", Server, Url);
                        savedUploads.Add(savedHeroImage);
                        
                        savedHeroImage.YachtId = yacht.Id;
                        db.YachtHeroImages.Add(savedHeroImage);

                        db.SaveChanges();
                    }

                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();

                    // Database rollback cannot undo files already written to disk.
                    foreach(var savedUpload in savedUploads)
                    {
                        UploadHelper.DeleteUploadedFile(savedUpload, Server);
                    }

                    Trace.TraceError("Create yacht failed: " + ex);
                    ModelState.AddModelError("", "Unable to save yacht. Please try again.");

                    return View(yachtVM);
                }
            }

            try
            {
                // Delete orphaned editor files only after the database transaction commits,
                // so a failed save does not remove files still referenced by persisted content.
                foreach (var editorImage in deletedEditorImages)
                {
                    UploadHelper.DeleteUploadedFile(editorImage, Server);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Post-commit delete cleanup failed:" + ex);
            }

            return RedirectToAction("Index");
        }

        // GET: Admin/Yacht/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Yacht yacht = db.Yachts
                .Include(y => y.DeckImgs)
                .Include(y => y.Interiors)
                .Include(y => y.Downloads)
                .SingleOrDefault(y => y.Id == id);
            if (yacht == null)
            {
                return HttpNotFound();
            }

            var existingHeroImage = db.YachtHeroImages.SingleOrDefault(h => h.YachtId == yacht.Id);

            // Convert EF upload collections into view models the edit page can render as
            // existing files with delete checkboxes.
            var yachtVM = ToYachtVM(yacht,existingHeroImage);

            return View(yachtVM);
        }

        // POST: Admin/Yacht/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(YachtVM yachtVM)
        {

            // The edit view posts new upload rows separately from existing upload delete
            // checkbox ids, so normalize the new upload arrays before validation.
            var deckImageUploads = (yachtVM.DeckImgsUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var interiorUploads = (yachtVM.InteriorUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var downfileUploads = (yachtVM.DownloadFileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            var heroImageUpload = yachtVM.HeroImageUpload;
            var hasHeroImageUpload = heroImageUpload != null && heroImageUpload.ContentLength > 0;

            foreach(var image in deckImageUploads)
            {
                if(!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("DeckImgsUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach(var image in interiorUploads)
            {
                if (!UploadHelper.IsFileValid(image, 1))
                {
                    ModelState.AddModelError("InteriorUploads", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
                }
            }

            foreach(var file in downfileUploads)
            {
                if (!UploadHelper.IsFileValid(file, 0))
                {
                    ModelState.AddModelError("DownloadFileUploads", "One or more uploaded files are not allowed.");
                }
            }

            if(hasHeroImageUpload && !UploadHelper.IsFileValid(heroImageUpload, 1))
            {
                ModelState.AddModelError("HeroImageUpload", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
            }

            var yacht = db.Yachts
                .Include(y => y.DeckImgs)
                .Include(y => y.Interiors)
                .Include(y => y.Downloads)
                .SingleOrDefault(y => y.Id == yachtVM.Id);

            if(yacht == null)
            {
                return HttpNotFound();
            }

            var existingHeroImage = db.YachtHeroImages.SingleOrDefault(h => h.YachtId == yacht.Id);

            var deleteDeckImageIds = yachtVM.DeleteDeckImgIds ?? new Guid[0];
            var deleteInteriorIds = yachtVM.DeleteInteriorIds ?? new Guid[0];
            var deleteFileIds = yachtVM.DeleteFileIds ?? new Guid[0];

            if(db.Yachts.Any(y => y.Name == yachtVM.Name && y.Id != yachtVM.Id))
            {
                ModelState.AddModelError("Name", "A yacht with this name already exists.");
            }

            if(!ModelState.IsValid)
            {
                // Rebuild existing upload lists before returning the view; otherwise the
                // validation response would lose file previews and delete checkboxes.
                var reloadYachtVM = ReloadYachtVM(yachtVM, yacht, existingHeroImage);
                return View(reloadYachtVM);
            }

            var newSavedUploads = new List<UploadedFile>();
            var deletedExistingUploads = new List<UploadedFile>();
            var deletedEditorImages = new List<YachtEditorImage>();

            using(var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Update Yacht data
                    UpdateYachtFromYachtVM(yacht, yachtVM);

                    // Update Hero Image. A new upload replaces the existing hero image, and
                    // the delete checkbox removes it without requiring a replacement.
                    if((yachtVM.DeleteHeroImage || hasHeroImageUpload) && existingHeroImage != null)
                    {
                        deletedExistingUploads.Add(existingHeroImage);
                        db.YachtHeroImages.Remove(existingHeroImage);
                        db.SaveChanges();
                    }

                    if (hasHeroImageUpload)
                    {
                        var newHeroImage = UploadHelper.SaveUploadedFile<YachtHeroImage>(heroImageUpload, "~/Images", Server, Url);
                        newSavedUploads.Add(newHeroImage);
                        newHeroImage.YachtId = yacht.Id;
                        db.YachtHeroImages.Add(newHeroImage);
                    }

                    // Remove checked existing uploads from the database transaction, but keep
                    // their physical files until the transaction commits successfully.
                    foreach(var deckId in deleteDeckImageIds)
                    {
                        var deckImages = yacht.DeckImgs.SingleOrDefault(d => d.Id == deckId);
                        if(deckImages != null)
                        {
                            deletedExistingUploads.Add(deckImages);
                            db.YachtImages.Remove(deckImages);
                        }
                    }

                    foreach(var interiorId in deleteInteriorIds)
                    {
                        var interiorImage = yacht.Interiors.SingleOrDefault(i => i.Id == interiorId);
                        if(interiorImage != null)
                        {
                            deletedExistingUploads.Add(interiorImage);
                            db.YachtInteriors.Remove(interiorImage);
                        }
                    }

                    foreach(var fileId in deleteFileIds)
                    {
                        var yachtDownload = yacht.Downloads.SingleOrDefault(d => d.Id == fileId);
                        if(yachtDownload != null)
                        {
                            deletedExistingUploads.Add(yachtDownload);
                            db.YachtDownloads.Remove(yachtDownload);
                        }   
                    }

                    // Add new upload files to disk and attach their metadata to the Yacht
                    // collections so EF persists the child upload rows.
                    foreach(var newDeckImage in deckImageUploads)
                    {
                        var newSaveUpload = UploadHelper.SaveUploadedFile<YachtImage>(newDeckImage, "~/Images", Server, Url);
                        newSavedUploads.Add(newSaveUpload);
                        yacht.DeckImgs.Add(newSaveUpload);
                    }

                    foreach(var newInterior in interiorUploads)
                    {
                        var newSaveUpload = UploadHelper.SaveUploadedFile<YachtInterior>(newInterior, "~/Images", Server, Url);
                        newSavedUploads.Add(newSaveUpload);
                        yacht.Interiors.Add(newSaveUpload);
                    }

                    foreach(var newFile in downfileUploads)
                    {
                        var newSaveUpload = UploadHelper.SaveUploadedFile<YachtDownload>(newFile, "~/Files", Server, Url);
                        newSavedUploads.Add(newSaveUpload);
                        yacht.Downloads.Add(newSaveUpload);
                    }

                    var editorContent =
                        (yachtVM.Overview ?? "") +
                        (yachtVM.Dimensions ?? "") +
                        (yachtVM.Specification ?? "");

                    // Sync Summernote image ownership to the current HTML content before
                    // committing, and postpone physical deletion until after commit.
                    deletedEditorImages = HandleEditorImages(yacht.Id, editorContent);

                    db.SaveChanges();
                    transaction.Commit();

                }
                catch(Exception ex)
                {
                    transaction.Rollback();

                    foreach(var newSavedUpload in newSavedUploads)
                    {
                        UploadHelper.DeleteUploadedFile(newSavedUpload, Server);
                    }

                    Trace.TraceError("Edit yacht failed: " + ex);
                    ModelState.AddModelError("", "Unable to save yacht. Please try again.");

                    var freshYacht = db.Yachts
                        .AsNoTracking()
                        .Include(y => y.DeckImgs)
                        .Include(y => y.Interiors)
                        .Include(y => y.Downloads)
                        .SingleOrDefault(y => y.Id == yachtVM.Id);

                    if(freshYacht == null)
                    {
                        return HttpNotFound();
                    }

                    var freshHeroImage = db.YachtHeroImages.AsNoTracking().SingleOrDefault(h => h.YachtId == freshYacht.Id);

                    var reloadYachtVM = ReloadYachtVM(yachtVM, freshYacht);
                    return View(reloadYachtVM);
                }
            }

            // Physical files are deleted after commit so the filesystem follows the
            // database result instead of getting ahead of a rolled-back transaction.
            try
            {
                foreach(var deletedExistingUpload in deletedExistingUploads)
                {
                    UploadHelper.DeleteUploadedFile(deletedExistingUpload, Server);
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Post-commit delete existing file cleanup failed:" + ex);
            }

            // physical in editor image deletion until after commit.
            try
            {
                foreach(var deletedEditorImage in deletedEditorImages)
                {
                    UploadHelper.DeleteUploadedFile(deletedEditorImage, Server);
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Post-commit delete editor image cleanup failed:" + ex);
            }

            return RedirectToAction("Details", new { Id = yacht.Id });
        }

        // GET: Admin/Yacht/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Yacht yacht = db.Yachts.SingleOrDefault(y => y.Id == id);
            if (yacht == null)
            {
                return HttpNotFound();
            }
            return View(yacht);
        }

        // POST: Admin/Yacht/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var yacht = db.Yachts
                .Include(y => y.DeckImgs)
                .Include(y => y.Interiors)
                .Include(y => y.Downloads)
                .Include(y => y.EditorImgs)
                .SingleOrDefault(y => y.Id == id);

            if(yacht == null)
            {
                return HttpNotFound();
            }

            var heroImage = db.YachtHeroImages.SingleOrDefault(h => h.YachtId == yacht.Id);

            var deleteUploads = new List<UploadedFile>();

            using(var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Capture all related upload records before removing the Yacht so their
                    // physical files can be deleted after the database commit.
                    foreach (var deckimage in yacht.DeckImgs)
                    {
                        deleteUploads.Add(deckimage);
                    }

                    foreach(var interior in yacht.Interiors)
                    {
                        deleteUploads.Add(interior);
                    }

                    foreach(var download in yacht.Downloads)
                    {
                        deleteUploads.Add(download);
                    }

                    foreach(var editorImage in yacht.EditorImgs)
                    {
                        deleteUploads.Add(editorImage);
                    }

                    if(heroImage != null)
                    {
                        // Hero image is not part of the Yacht upload collections, so remove
                        // it explicitly and include its file in post-commit cleanup.
                        deleteUploads.Add(heroImage);
                        db.YachtHeroImages.Remove(heroImage);
                    }

                    db.Yachts.Remove(yacht);
                    db.SaveChanges();

                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();

                    Trace.TraceError("Delete yacht failed: " + ex);
                    ModelState.AddModelError("", "Unable to delete yacht. Please try again.");

                    return View(yacht);
                }
            }

            try
            {
                foreach(var deleteUpload in deleteUploads)
                {
                    UploadHelper.DeleteUploadedFile(deleteUpload, Server);
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Post-commit delete yacht files cleanup failed:" + ex);
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

        // Yacht Model <=> View Model transform

        private Yacht ToYacht(YachtVM yachtVM)
        {
            return new Yacht
            {
                Name = yachtVM.Name,
                IsNew = yachtVM.IsNew,
                IsPublished = yachtVM.IsPublished,
                PostDate = DateTime.Now,
                Overview = yachtVM.Overview,
                Dimensions = yachtVM.Dimensions,
                Specification = yachtVM.Specification,
                SortOrder = yachtVM.SortOrder.HasValue ? yachtVM.SortOrder.Value : 0
            };
        }

        private void UpdateYachtFromYachtVM(Yacht yacht, YachtVM yachtVM)
        {
            yacht.Name = yachtVM.Name;
            yacht.IsNew = yachtVM.IsNew;
            yacht.IsPublished = yachtVM.IsPublished;
            yacht.PostDate = DateTime.Now;
            yacht.Overview = yachtVM.Overview;
            yacht.Dimensions = yachtVM.Dimensions;
            yacht.Specification = yachtVM.Specification;

            if (yachtVM.SortOrder.HasValue)
            {
                yacht.SortOrder = yachtVM.SortOrder.Value;
            }
        }

        private YachtVM ToYachtVM(Yacht yacht, YachtHeroImage heroImage = null)
        {
            return new YachtVM
            {
                Id = yacht.Id,
                Name = yacht.Name,
                IsNew = yacht.IsNew,
                IsPublished = yacht.IsPublished,
                SortOrder = yacht.SortOrder,
                Overview = yacht.Overview,
                Dimensions = yacht.Dimensions,
                Specification = yacht.Specification,
                ExistingHeroImage = heroImage?.ToExistingUploadFileVM(),
                ExistingDeckImgs = yacht.DeckImgs.Select(y => y.ToExistingUploadFileVM()).ToList(),
                ExistingInteriors = yacht.Interiors.Select(y => y.ToExistingUploadFileVM()).ToList(),
                ExistingDownloadFile = yacht.Downloads.Select(y => y.ToExistingUploadFileVM()).ToList()
            };
        }

        private YachtVM ReloadYachtVM(YachtVM yachtVM, Yacht yacht, YachtHeroImage heroImage = null)
        {
            var reloadYachtVM = ToYachtVM(yacht, heroImage);
            reloadYachtVM.Name = yachtVM.Name;
            reloadYachtVM.IsNew = yachtVM.IsNew;
            reloadYachtVM.IsPublished = yachtVM.IsPublished;
            reloadYachtVM.Overview = yachtVM.Overview;
            reloadYachtVM.Dimensions = yachtVM.Dimensions;
            reloadYachtVM.Specification = yachtVM.Specification;
            reloadYachtVM.SortOrder = yachtVM.SortOrder;

            return reloadYachtVM;
        }

        // Editor Image Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadEditorImage(HttpPostedFileBase file)
        {
            // Summernote uploads images before the Yacht form is saved. Store them with a
            // null YachtId first; HandleEditorImages attaches referenced ones on submit.
            if(!UploadHelper.IsFileValid(file, 1))
            {
                return JsonUploadError("Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed.");
            }

            var uploadedImage = UploadHelper.SaveUploadedFile<YachtEditorImage>(file, "~/Images", Server, Url);
            var imageUrl = uploadedImage.FilePath;
            db.YachtEditorImages.Add(uploadedImage);
            db.SaveChanges();

            return Json(new { url = imageUrl });
        }

        private ActionResult JsonUploadError(string message)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new { error = message });
        }

        // Handle Editor Image Upload after click submit
        private List<YachtEditorImage> HandleEditorImages(int yachtId, string editorContent)
        {
            // Summernote uploads editor images before the Yacht form is submitted, so new
            // YachtEditorImage rows may still have a null YachtId at this point.
            // Compare the submitted HTML against the persisted editor-image records to:
            // 1. remove images that used to belong to this yacht but are no longer referenced;
            // 2. attach newly uploaded, still-unowned images that are referenced by this yacht.
            HashSet<string> referencedUrls = ExtractEditorImageUrls(editorContent);
            List<YachtEditorImage> currentEditorImages = db.YachtEditorImages.Where(i => i.YachtId == yachtId).ToList();
            var deletedImages = new List<YachtEditorImage>();

            // Remove database rows for images deleted from the editor content, but return
            // those records so their physical files can be deleted only after commit.
            foreach(var image in currentEditorImages.Where(i => !referencedUrls.Contains(i.FilePath)).ToList())
            {
                deletedImages.Add(image);
                db.YachtEditorImages.Remove(image);
            }

            // Claim each image URL still present in the editor. The null YachtId check
            // picks up images uploaded during this editing session before the form save.
            foreach(string imageUrl in referencedUrls)
            {
                YachtEditorImage editorImage = db.YachtEditorImages
                    .FirstOrDefault(i => i.FilePath == imageUrl && (i.YachtId == null || i.YachtId == yachtId));

                if(editorImage != null)
                {
                    editorImage.YachtId = yachtId;
                }
            }

            return deletedImages;
        }

        private HashSet<String> ExtractEditorImageUrls(string editorContent)
        {
            // Use a set because the same image may appear more than once in the editor HTML,
            // but each uploaded file record should only be reconciled once.
            HashSet<string> imageUrls = new HashSet<String>();

            if (String.IsNullOrWhiteSpace(editorContent))
            {
                return imageUrls;
            }

            // Scan the saved rich-text HTML for image tags instead of trusting form fields;
            // the editor content is the source of truth for which uploaded images remain in use.
            foreach (Match match in Regex.Matches(editorContent, "<img[^>]+src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.IgnoreCase))
            {
                // Normalize editor img src values so query strings, fragments, and URL
                // encoding do not prevent matching them to uploaded file records.
                string source = HttpUtility.UrlDecode(match.Groups["src"].Value);
                if (String.IsNullOrWhiteSpace(source))
                {
                    continue;
                }

                int queryIndex = source.IndexOfAny(new[] { '?', '#' });
                if(queryIndex >= 0)
                {
                    // Browser/editor cache-busting query strings and anchors are not stored
                    // in UploadedFile.FilePath, so strip them before comparing paths.
                    source = source.Substring(0, queryIndex);
                }

                string fileName = Path.GetFileName(source);
                if( !String.IsNullOrWhiteSpace(fileName) && source.IndexOf("/Images/",StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Rebuild the app-relative Images path used by UploadHelper so the value
                    // matches YachtEditorImage.FilePath regardless of absolute/relative src form.
                    imageUrls.Add(Url.Content("~/Images/" + fileName));
                }
            }

            return imageUrls;
        }

        
    }
}
