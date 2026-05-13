using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class YachtDownload : UploadedFile
    {
        [Required]
        [ForeignKey("Yacht")]
        public int YachtId { get; set; }

        public virtual Yacht Yacht { get; set; }
    }
}