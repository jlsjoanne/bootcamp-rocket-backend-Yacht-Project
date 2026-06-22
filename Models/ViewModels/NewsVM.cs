using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TayanaYachts.Models.ViewModels
{
    public class NewsVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Summary { get; set; }

        [AllowHtml]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Publish Date")]
        public DateTime PublishDate { get; set; }

        [Display(Name="Pinned")]
        public bool IsPinned { get; set; }

        [Display(Name="Publish to Website")]
        public bool IsPublished { get; set; }

        [Display(Name = "Upload Images")]
        public HttpPostedFileBase[] ImageUploads { get; set; }

        [Display(Name = "Upload Files")]
        public HttpPostedFileBase[] FileUploads { get; set; }

        [Display(Name = "Existing Images")]
        public IList<ExistingUploadFileVM> ExistingImages { get; set; }

        [Display(Name = "Existing Files")]
        public IList<ExistingUploadFileVM> ExistingFiles { get; set; }

        public Guid[] DeleteImageIds { get; set; }
        public Guid[] DeleteFileIds { get; set; }

        public Guid? ThumbnailImageId { get; set; }

        public int? ThumbnailImageUploadIndex { get; set; }

        public string ThumbnailSelection { get; set; }
    }
}