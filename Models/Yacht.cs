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
        [Index("IX_Yacht_Name",IsUnique = true)]
        [StringLength(50)]
        public string Name { get; set; }

        public bool IsNew { get; set; } = false;

        public bool IsPublished { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name="Last Modified Time")]
        public DateTime PostDate { get; set; }

        [AllowHtml]
        public string Overview { get; set; }

        [AllowHtml]
        public string Dimensions { get; set; }

        [AllowHtml]
        [Display(Name="DETAIL SPECIFICATION")]
        public string Specification { get; set; }

        [Display(Name= "Layout & deck plan")]
        public virtual ICollection<YachtImage> DeckImgs { get; set; }

        public virtual ICollection<YachtInterior> Interiors { get; set; }

        public virtual ICollection<YachtDownload> Downloads { get; set; }

        public virtual ICollection<YachtEditorImage> EditorImgs { get; set; }

        public Yacht()
        {
            DeckImgs = new HashSet<YachtImage>();
            Interiors = new HashSet<YachtInterior>();
            Downloads = new HashSet<YachtDownload>();
            EditorImgs = new HashSet<YachtEditorImage>();
        }
    }
}