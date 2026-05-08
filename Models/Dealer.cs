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

        public UploadedFile Image { get;set; }

        public virtual Area Area { get; set; }
        
    }
}