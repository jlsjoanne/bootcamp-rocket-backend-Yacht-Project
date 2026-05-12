using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.IO;

namespace TayanaYachts.Models
{
    public class UploadedFile
    {
        public Guid Id { get; set; }
        
        [NotMapped]
        public HttpPostedFileBase File { get;set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public string ContentType { get; set; }

        public UploadedFile()
        {
            FileName = Path.GetFileNameWithoutExtension(File.FileName);
            FileType = Path.GetExtension(File.FileName);
            ContentType = File.ContentType;
        }
    }
}