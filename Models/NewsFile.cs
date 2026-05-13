using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class NewsFile : UploadedFile
    {
        [Required]
        [ForeignKey("News")]
        public int NewsId { get; set; }
        public virtual News News { get; set; }
    }
}