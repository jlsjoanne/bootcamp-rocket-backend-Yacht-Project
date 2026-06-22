using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class News
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

        public bool IsPinned { get; set; } = false;

        public bool IsPublished { get; set; } = false;

        public virtual ICollection<NewsImage> Images { get; set; }
        public virtual ICollection<NewsFile> Files { get; set; }

        public Guid? ThumbnailImageId { get; set; }

        [ForeignKey("ThumbnailImageId")]
        public virtual NewsImage ThumbnailImage { get; set; }

        public News()
        {
            Images = new HashSet<NewsImage>();
            Files = new HashSet<NewsFile>();
        }
    }
}