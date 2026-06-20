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
            Yacht yacht = db.Yachts.Find(id);
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
            //if (ModelState.IsValid)
            //{
            //    db.Entry(yacht).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            return View(yachtVM);
        }

        // GET: Admin/Yacht/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Admin/Yacht/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Yacht yacht = db.Yachts.Find(id);
            db.Yachts.Remove(yacht);
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
