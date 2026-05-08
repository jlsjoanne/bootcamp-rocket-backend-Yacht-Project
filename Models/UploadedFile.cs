using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TayanaYachts.Models
{
    public class UploadedFile
    {
        public Guid Id { get; set; }
        
        [Required]
        public HttpPostedFileBase File { get;set; }

        public string FileName { get; set; }
    }
}