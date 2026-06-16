using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TayanaYachts.Models.ViewModels
{
    public class ExistingUploadFileVM
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public string FileType { get; set; }
    }
}