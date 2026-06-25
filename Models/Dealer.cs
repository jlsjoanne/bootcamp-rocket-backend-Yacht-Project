using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class Dealer
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name="Dealer Name")]
        public string Name { get; set; }

        [AllowHtml]
        public string Content { get; set; }

        [Required]
        public virtual DealerImage Image { get; set; }

        [Required]
        [Display(Name = "Area")]
        public int AreaId { get; set; }

        [ForeignKey("AreaId")]
        public virtual Area Area { get; set; }

        [Display(Name ="Display Order")]
        [Range(0,9999)]
        public int SortOrder { get; set; }
        
    }
}