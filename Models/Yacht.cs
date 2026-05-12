using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    
    public class Yacht
    {
        public int Id { get; set; }

        [Required]
        [Index("IX_Name",IsUnique = true)]
        [StringLength(50)]
        public string Name { get; set; }

        public bool IsNew { get; set; } = false;

        public bool IsPublished { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime PostDate { get; set; }

        public string Overview { get; set; }

        // 1圖1文編輯功能??
        public virtual UploadedFile OverviewImage { get; set; }

        [AllowHtml]
        public string Dimensions { get; set; }

        [Required]
        [AllowHtml]
        [Display(Name="DETAIL SPECIFICATION")]
        public string Specification { get; set; }

        [Display(Name= "Layout & deck plan")]
        public virtual ICollection<UploadedFile> Images { get; set; }

        public virtual ICollection<UploadedFile> Interiors { get; set; }

        public virtual ICollection<UploadedFile> Downloads { get; set; }
    }
}