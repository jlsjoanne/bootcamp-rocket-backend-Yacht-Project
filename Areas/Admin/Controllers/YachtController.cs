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
    public class YachtController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/Yacht
        public ActionResult Index()
        {
            // 還要加搜尋、分頁
            var yachts = db.Yachts.OrderByDescending(y => y.IsNew)
                .ThenByDescending(y => y.PostDate);

            return View(yachts.ToList());
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
            var deckImageUploads = (yachtVM.DeckImgsUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var interiorUploads = (yachtVM.InteriorUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var downloadFileUploads = (yachtVM.DownloadFileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

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

                    var editorContent =
                        (yachtVM.Overview ?? "") +
                        (yachtVM.Dimensions ?? "") +
                        (yachtVM.Specification ?? "");

                    deletedEditorImages = HandleEditorImages(yacht.Id, editorContent);
                    db.SaveChanges();

                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();

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

            var yachtVM = ToYachtVM(yacht);

            return View(yachtVM);
        }

        // POST: Admin/Yacht/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(YachtVM yachtVM)
        {

            // set deck, interior, download file lists
            var deckImageUploads = (yachtVM.DeckImgsUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var interiorUploads = (yachtVM.InteriorUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();
            var downfileUploads = (yachtVM.DownloadFileUploads ?? new HttpPostedFileBase[0])
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

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

            var yacht = db.Yachts
                .Include(y => y.DeckImgs)
                .Include(y => y.Interiors)
                .Include(y => y.Downloads)
                .SingleOrDefault(y => y.Id == yachtVM.Id);

            if(yacht == null)
            {
                return HttpNotFound();
            }

            var deleteDeckImageIds = yachtVM.DeleteDeckImgIds ?? new Guid[0];
            var deleteInteriorIds = yachtVM.DeleteInteriorIds ?? new Guid[0];
            var deleteFileIds = yachtVM.DeleteFileIds ?? new Guid[0];

            if(db.Yachts.Any(y => y.Name == yachtVM.Name && y.Id != yachtVM.Id))
            {
                ModelState.AddModelError("Name", "A yacht with this name already exists.");
            }

            if(!ModelState.IsValid)
            {
                var reloadYachtVM = ReloadYachtVM(yachtVM, yacht);
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


                    // Delete deck, interior, download file from db and add to deletedExistingUploads list
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

                    // Add new upload files to db and relative folder
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

                    var reloadYachtVM = ReloadYachtVM(yachtVM, freshYacht);
                    return View(reloadYachtVM);
                }
            }

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

            var deleteUploads = new List<UploadedFile>();

            using(var transaction = db.Database.BeginTransaction())
            {
                try
                {
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

        private static Yacht ToYacht(YachtVM yachtVM)
        {
            return new Yacht
            {
                Name = yachtVM.Name,
                IsNew = yachtVM.IsNew,
                IsPublished = yachtVM.IsPublished,
                PostDate = DateTime.Now,
                Overview = yachtVM.Overview,
                Dimensions = yachtVM.Dimensions,
                Specification = yachtVM.Specification
            };
        }

        private static void UpdateYachtFromYachtVM(Yacht yacht, YachtVM yachtVM)
        {
            yacht.Name = yachtVM.Name;
            yacht.IsNew = yachtVM.IsNew;
            yacht.IsPublished = yachtVM.IsPublished;
            yacht.PostDate = DateTime.Now;
            yacht.Overview = yachtVM.Overview;
            yacht.Dimensions = yachtVM.Dimensions;
            yacht.Specification = yachtVM.Specification;
        }

        private static YachtVM ToYachtVM(Yacht yacht)
        {
            return new YachtVM
            {
                Id = yacht.Id,
                Name = yacht.Name,
                IsNew = yacht.IsNew,
                IsPublished = yacht.IsPublished,
                Overview = yacht.Overview,
                Dimensions = yacht.Dimensions,
                Specification = yacht.Specification,
                ExistingDeckImgs = yacht.DeckImgs.Select(y => y.ToExistingUploadFileVM()).ToList(),
                ExistingInteriors = yacht.Interiors.Select(y => y.ToExistingUploadFileVM()).ToList(),
                ExistingDownloadFile = yacht.Downloads.Select(y => y.ToExistingUploadFileVM()).ToList()
            };
        }

        private YachtVM ReloadYachtVM(YachtVM yachtVM, Yacht yacht)
        {
            var reloadYachtVM = ToYachtVM(yacht);
            reloadYachtVM.Name = yachtVM.Name;
            reloadYachtVM.IsNew = yachtVM.IsNew;
            reloadYachtVM.IsPublished = yachtVM.IsPublished;
            reloadYachtVM.Overview = yachtVM.Overview;
            reloadYachtVM.Dimensions = yachtVM.Dimensions;
            reloadYachtVM.Specification = yachtVM.Specification;

            return reloadYachtVM;
        }

        // Editor Image Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadEditorImage(HttpPostedFileBase file)
        {
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
            HashSet<string> referencedUrls = ExtractEditorImageUrls(editorContent);
            List<YachtEditorImage> currentEditorImages = db.YachtEditorImages.Where(i => i.YachtId == yachtId).ToList();
            var deletedImages = new List<YachtEditorImage>();

            foreach(var image in currentEditorImages.Where(i => !referencedUrls.Contains(i.FilePath)).ToList())
            {
                deletedImages.Add(image);
                db.YachtEditorImages.Remove(image);
            }

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
            HashSet<string> imageUrls = new HashSet<String>();

            if (String.IsNullOrWhiteSpace(editorContent))
            {
                return imageUrls;
            }

            foreach (Match match in Regex.Matches(editorContent, "<img[^>]+src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.IgnoreCase))
            {
                string source = HttpUtility.UrlDecode(match.Groups["src"].Value);
                if (String.IsNullOrWhiteSpace(source))
                {
                    continue;
                }

                int queryIndex = source.IndexOfAny(new[] { '?', '#' });
                if(queryIndex >= 0)
                {
                    source = source.Substring(0, queryIndex);
                }

                string fileName = Path.GetFileName(source);
                if( !String.IsNullOrWhiteSpace(fileName) && source.IndexOf("/Images/",StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    imageUrls.Add(Url.Content("~/Images/" + fileName));
                }
            }

            return imageUrls;
        }
    }
}
