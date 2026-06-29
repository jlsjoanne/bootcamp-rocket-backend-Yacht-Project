using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TayanaYachts.Models;

namespace TayanaYachts.Methods
{
    public static class UploadHelper
    {
        // allowed image and file type setting
        // Image uploads are restricted to common browser-displayable formats.
        private static readonly HashSet<string> AllowedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

        // MIME types are checked together with extensions to reject mismatched uploads.
        private static readonly HashSet<string> AllowedImageContentType = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        // General file uploads allow office documents, spreadsheets, slides, images, PDFs, CSV, and text files.
        private static readonly HashSet<string> AllowedFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // pdf
            ".pdf",

            // word
            ".doc",
            ".docx",

            // excel or spreadsheet
            ".xls",
            ".xlsx",
            ".csv",

            // powerpoint or slides
            ".ppt",
            ".pptx",

            // image
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp",

            // text file
            ".txt"
        };

        // Keep this list aligned with AllowedFileExtensions so extension and MIME validation agree.
        private static readonly HashSet<string> AllowedFileContentType = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // pdf
            "application/pdf",

            // word
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            // excel or spreadsheet
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/csv",
            "application/csv",

            // powerpoint or slides
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",

            // image
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",

            // text
            "text/plain"
        };

        private const int MaxFileSizeBytes = 15 * 1024 * 1024;


        // fileCategory => 0: File Upload, 1: Image Upload
        public static bool IsFileValid(HttpPostedFileBase file, int uploadType)
        {
            // uploadType 0 validates general attachments; uploadType 1 validates image-only uploads.
            var AllowedExtensions = uploadType == 0 ? AllowedFileExtensions : AllowedImageExtensions;
            var AllowedContentType = uploadType == 0 ? AllowedFileContentType : AllowedImageContentType;

            if(file == null || file.ContentLength == 0)
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName);

            // Require a valid size, extension, and browser-reported MIME type before saving.
            return file.ContentLength <= MaxFileSizeBytes
                && AllowedExtensions.Contains(extension)
                && AllowedContentType.Contains(file.ContentType);
        }

        public static T SaveUploadedFile<T>(HttpPostedFileBase file,string relativeFolder, 
            HttpServerUtilityBase server, UrlHelper url)
            where T : UploadedFile, new()
        {
            // Store uploads by generated GUID file names to avoid collisions and hide original file names.
            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName);
            Guid fileId = Guid.NewGuid();
            var storedFileName = fileId.ToString("N") + extension;

            var absoluteFolder = server.MapPath(relativeFolder);

            if(!Directory.Exists(absoluteFolder))
            {
                // Create the target folder on demand for first-time deployments.
                Directory.CreateDirectory(absoluteFolder);
            }

            file.SaveAs(Path.Combine(absoluteFolder, storedFileName));

            // Return EF metadata that points to the saved physical file.
            return new T
            {
                Id = fileId,
                OriginalFileName = originalFileName,
                FileType = extension,
                ContentType = file.ContentType,
                FilePath = url.Content(relativeFolder + "/" + storedFileName)
            };

        }

        public static void DeleteUploadedFile(UploadedFile uploadedFile, HttpServerUtilityBase server)
        {
            // Missing upload metadata should not fail a cleanup path.
            if(uploadedFile == null || string.IsNullOrWhiteSpace(uploadedFile.FilePath))
            {
                return;
            }

            var absolutePath = server.MapPath(uploadedFile.FilePath);

            if (File.Exists(absolutePath))
            {
                // Physical cleanup is used after database commits or failed upload transactions.
                File.Delete(absolutePath);
            }
        }
    }
}
