using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TayanaYachts.Models;
using TayanaYachts.Models.ViewModels;

namespace TayanaYachts.Methods
{
    public static class UploadedFileExtensions
    {
        public static ExistingUploadFileVM ToExistingUploadFileVM(this UploadedFile file)
        {
            return new ExistingUploadFileVM
            {
                Id = file.Id,
                OriginalFileName = file.OriginalFileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                FileType = file.FileType
            };
        }
    }
}