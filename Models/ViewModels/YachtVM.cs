using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TayanaYachts.Models.ViewModels
{
    public class YachtVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "Is this the latest Model?")]
        public bool IsNew { get; set; }

        [Display(Name = "Publish to Website")]
        public bool IsPublished { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 9999)]
        public int? SortOrder { get; set; }

        [AllowHtml]
        public string Overview { get; set; }

        [AllowHtml]
        public string Dimensions { get; set; }

        [AllowHtml]
        [Display(Name = "DETAIL SPECIFICATION")]
        public string Specification { get; set; }

        [Display(Name = "Upload Layout & deck plan")]
        public HttpPostedFileBase[] DeckImgsUploads { get; set; }

        [Display(Name = "Upload Interiors")]
        public HttpPostedFileBase[] InteriorUploads { get; set; }

        [Display(Name = "Upload Files")]
        public HttpPostedFileBase[] DownloadFileUploads { get; set; }

        [Display(Name = "Upload Homepage Hero Image")]
        public HttpPostedFileBase HeroImageUpload { get; set; }

        [Display(Name = "Existing Layout & deck plan")]
        public IList<ExistingUploadFileVM> ExistingDeckImgs { get; set; }

        [Display(Name = "Existing Interiors")]
        public IList<ExistingUploadFileVM> ExistingInteriors { get; set; }

        [Display(Name = "Existing Files")]
        public IList<ExistingUploadFileVM> ExistingDownloadFile { get; set; }

        [Display(Name = "Existing Homepage Hero Image")]
        public ExistingUploadFileVM ExistingHeroImage { get; set; }


        public Guid[] DeleteDeckImgIds { get; set; }
        public Guid[] DeleteInteriorIds { get; set; }
        public Guid[] DeleteFileIds { get; set; }

        public bool DeleteHeroImage { get; set; }
    }
}