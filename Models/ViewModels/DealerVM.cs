using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TayanaYachts.Models.ViewModels
{
    public class DealerVM
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Dealer Name")]
        public string Name { get; set; }

        [AllowHtml]
        public string Content { get; set; }

        [Required]
        [Display(Name="Dealer Image")]
        public HttpPostedFileBase ImageFile { get; set; }

        [Display(Name="Image")]
        public string FilePath { get; set; }

        [Required]
        [Display(Name="Country")]
        public int? CountryId { get; set; }

        [Required]
        [Display(Name="Area")]
        public int? AreaId { get; set; }

        

        public IEnumerable<SelectListItem> Countries { get; set; }
        public IEnumerable<SelectListItem> Areas { get; set; }
    }
}