using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TayanaYachts.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(1000)]
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
    }
}