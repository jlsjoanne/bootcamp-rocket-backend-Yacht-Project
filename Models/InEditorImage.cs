using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class InEditorImage
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string OriginalFileName { get; set; }

        [StringLength(255)]
        public string StoredFileName { get; set; }

        [StringLength(20)]
        public string FileExtension { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}