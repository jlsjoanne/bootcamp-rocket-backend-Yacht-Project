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
using System.Text.RegularExpressions;
using System.IO;


namespace TayanaYachts.Areas.Admin.Controllers
{
    [Authorize]
    public class DealerController : Controller
    {
        private TayanaContext db = new TayanaContext();

        // GET: Admin/Dealer
        public ActionResult Index()
        {
            var dealers = db.Dealers
                .Include(d => d.Area.Country)
                .OrderBy(d => d.Area.Country.SortOrder)
                .ThenBy(d => d.Area.Country.Name)
                .ThenBy(d => d.SortOrder)
                .ThenBy(d => d.Name);
            return View(dealers.ToList());
        }

        // GET: Admin/Dealer/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dealer dealer = db.Dealers
                .Include(d => d.Area.Country)
                .Include(d => d.Image)
                .SingleOrDefault(d => d.Id == id);
            if (dealer == null)
            {
                return HttpNotFound();
            }
            return View(dealer);
        }

        // GET: Admin/Dealer/Create
        public ActionResult Create()
        {
            var dealerVM = new DealerVM
            {
                CountryList = GetCountrySelectList(),
                AreaList = GetAreaSelectList()
            };
            return View(dealerVM);
        }

        // POST: Admin/Dealer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DealerVM dealerVM)
        {
            // check Image File
            if (dealerVM.ImageFile == null || dealerVM.ImageFile.ContentLength == 0)
            {
                ModelState.AddModelError("ImageFile", "Dealer Image is Required.");
            }

            else if (!IsImageValid(dealerVM.ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed");
            }

            if (!ModelState.IsValid)
            {
                dealerVM.CountryList = GetCountrySelectList(dealerVM.CountryId);
                dealerVM.AreaList = GetAreaSelectList(dealerVM.CountryId, dealerVM.AreaId);

                return View(dealerVM);
            }

            DealerImage dealerImage = null;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Check if input SortOrder
                    var sortOrder = dealerVM.SortOrder ?? GetNextDealerSortOrder(dealerVM.CountryId);

                    dealerImage = DealerImgUpload(dealerVM.ImageFile);

                    // DealerVM to Dealer class
                    var dealer = new Dealer
                    {
                        Name = dealerVM.Name,
                        Content = RemoveWrappingPTag(dealerVM.Content),
                        AreaId = dealerVM.AreaId.Value,
                        SortOrder = sortOrder,
                        Image = dealerImage
                    };

                    db.Dealers.Add(dealer);

                    db.SaveChanges();

                    transaction.Commit();

                    return RedirectToAction("Index");
                }
                catch
                {
                    transaction.Rollback();

                    if( dealerImage != null)
                    {
                        DeleteImageFile(dealerImage);
                    }

                    ModelState.AddModelError("", "Unable to save dealer. Please try again");

                    dealerVM.CountryList = GetCountrySelectList(dealerVM.CountryId);
                    dealerVM.AreaList = GetAreaSelectList(dealerVM.CountryId, dealerVM.AreaId);
                    return View(dealerVM);
                }
            }
        }

        // GET: Admin/Dealer/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dealer dealer = db.Dealers
                .Include(d => d.Area.Country)
                .Include(d => d.Image)
                .SingleOrDefault(d => d.Id == id);
            if (dealer == null)
            {
                return HttpNotFound();
            }
            var dealerVM = new DealerVM
            {
                Id = dealer.Id,
                Name = dealer.Name,
                Content = dealer.Content,
                FilePath = dealer.Image.FilePath,
                CountryId = dealer.Area.CountryId,
                AreaId = dealer.AreaId,
                SortOrder = dealer.SortOrder,
                CountryList = GetCountrySelectList(dealer.Area.CountryId),
                AreaList = GetAreaSelectList(dealer.Area.CountryId, dealer.AreaId)
            };
            return View(dealerVM);
        }

        // POST: Admin/Dealer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DealerVM dealerVM)
        {
            // Check if update new image
            var hasNewImage = dealerVM.ImageFile != null && dealerVM.ImageFile.ContentLength > 0;

            if(hasNewImage && !IsImageValid(dealerVM.ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Only JPG, PNG, GIF, or WEBP images under 15 MB are allowed");
            }

            if(!ModelState.IsValid)
            {
                var existingDealer = db.Dealers
                    .Include(d => d.Image)
                    .SingleOrDefault(d => d.Id == dealerVM.Id);

                if(existingDealer == null)
                {
                    return HttpNotFound();
                }

                dealerVM.FilePath = existingDealer.Image.FilePath;
                dealerVM.CountryList = GetCountrySelectList(dealerVM.CountryId);
                dealerVM.AreaList = GetAreaSelectList(dealerVM.CountryId, dealerVM.AreaId);
                return View(dealerVM);
            }

            var dealer = db.Dealers
                .Include(d => d.Image)
                .SingleOrDefault(d => d.Id == dealerVM.Id);

            if(dealer == null)
            {
                return HttpNotFound();
            }

            DealerImage newImage = null;
            DealerImage oldImage = null;

            try
            {
                dealer.Name = dealerVM.Name;
                dealer.Content = RemoveWrappingPTag(dealerVM.Content);
                dealer.AreaId = dealerVM.AreaId.Value;

                if (dealerVM.SortOrder.HasValue)
                {
                    dealer.SortOrder = dealerVM.SortOrder.Value;
                }

                if (hasNewImage)
                {
                    oldImage = new DealerImage { FilePath = dealer.Image.FilePath };
                    newImage = DealerImgUpload(dealerVM.ImageFile);

                    dealer.Image.OriginalFileName = newImage.OriginalFileName;
                    dealer.Image.StoredFileName = newImage.StoredFileName;
                    dealer.Image.FileType = newImage.FileType;
                    dealer.Image.ContentType = newImage.ContentType;
                    dealer.Image.FilePath = newImage.FilePath;
                }

                db.SaveChanges();
            }
            catch
            {
                if(newImage != null)
                {
                    DeleteImageFile(newImage);
                }

                ModelState.AddModelError("", "Unable to save dealer. Please try again.");

                dealerVM.FilePath = dealer.Image.FilePath;
                dealerVM.CountryList = GetCountrySelectList(dealerVM.CountryId);
                dealerVM.AreaList = GetAreaSelectList(dealerVM.CountryId, dealerVM.AreaId);

                return View(dealerVM);
            }

            if (oldImage != null)
            {
                DeleteImageFile(oldImage);
            }

            return RedirectToAction("Details", new { id = dealer.Id });
        }

        // GET: Admin/Dealer/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dealer dealer = db.Dealers.Find(id);
            if (dealer == null)
            {
                return HttpNotFound();
            }
            return View(dealer);
        }

        // POST: Admin/Dealer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Dealer dealer = db.Dealers.Include(d => d.Image).SingleOrDefault(d => d.Id == id);

            if (dealer == null)
            {
                return HttpNotFound();
            }

            var dealerImage = dealer.Image;
            DeleteImageFile(dealerImage);
            db.Dealers.Remove(dealer);
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

        // private methods

        // Dropdown list
        private IEnumerable<SelectListItem> GetCountrySelectList(int? selectedCountryId = null)
        {
            return db.Countries
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = selectedCountryId.HasValue && c.Id == selectedCountryId.Value
                })
                .ToList();
        }

        private IEnumerable<SelectListItem> GetAreaSelectList(int? countryId = null, int? selectedAreaId = null)
        {
            var areaList = db.Areas.AsQueryable();

            if (countryId.HasValue)
            {
                areaList = areaList.Where(a => a.CountryId == countryId);
            }
            else
            {
                // if country not selected yet => no area records
                areaList = areaList.Where(a => false);
            }

            return areaList
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name,
                    Selected = selectedAreaId.HasValue && a.Id == selectedAreaId.Value
                })
                .ToList();
        }

        // public method
        // Used in View => When user select country, get area under that country
        public JsonResult GetAreaByCountry(int countryId)
        {
            var areas = db.Areas
                .Where(a => a.CountryId == countryId)
                .OrderBy(a => a.Name)
                .Select(a => new
                {
                    id = a.Id,
                    name = a.Name
                })
                .ToList();

            return Json(areas, JsonRequestBehavior.AllowGet);
        }


        // private method
        // For Image File
        // Check if Image file is Valid
        private bool IsImageValid(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return false;
            }


            // image limitation for type and size
            var allowedContentType = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            var allowedFileType = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp"
            };

            var imgMaxSize = 15 * 1024 * 1024;

            // verify if file match image limitation
            if (!allowedContentType.Contains(file.ContentType))
            {
                return false;
            }

            var inputfileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedFileType.Contains(inputfileExtension))
            {
                return false;
            }

            if (file.ContentLength > imgMaxSize)
            {
                return false;
            }

            return true;

        }

        // Image Upload
        private DealerImage DealerImgUpload(HttpPostedFileBase file)
        {
            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = Guid.NewGuid().ToString("N") + extension;

            var relativeFolder = "~/Images";
            var absoluteFolder = Server.MapPath(relativeFolder);

            if (!Directory.Exists(absoluteFolder))
            {
                Directory.CreateDirectory(absoluteFolder);
            }

            var absoluteDirectory = Path.Combine(absoluteFolder, storedFileName);
            file.SaveAs(absoluteDirectory);

            return new DealerImage
            {
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                FileType = extension,
                ContentType = file.ContentType,
                FilePath = Url.Content(relativeFolder + "/" + storedFileName)
            };

        }

        // Delete Old Image File
        private void DeleteImageFile(DealerImage image)
        {
            if (image == null || string.IsNullOrEmpty(image.FilePath))
            {
                return;
            }

            var absolutePath = Server.MapPath(image.FilePath);

            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }

        // Remove editor wrapping p tag
        private string RemoveWrappingPTag(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            content = content.Trim();

            if (Regex.IsMatch(content, @"^<p[^>]*>\s*(<br\s*/?>)?\s*</p>$", RegexOptions.IgnoreCase))
            {
                return String.Empty;
            }

            var match = Regex.Match(content,
                @"^<p[^>]*>([\s\S]*)</p>$",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return content;
            }

            var innerContent = match.Groups[1].Value;

            if (Regex.IsMatch(innerContent, @"</?p\b", RegexOptions.IgnoreCase))
            {
                return content;
            }

            return innerContent.Trim();
        }

        // if user doesn't enter order => automatically get next order as SortOrder
        private int GetNextDealerSortOrder(int? countryId)
        {
            var maxSortOrder = db.Dealers
                .Where(d => d.Area.CountryId == countryId)
                .Select(d => (int?)d.SortOrder)
                .Max();

            return (maxSortOrder ?? 0) + 10;
        }
    }
}
