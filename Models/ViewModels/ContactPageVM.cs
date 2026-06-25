using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TayanaYachts.Models.ViewModels
{
    public class ContactPageVM
    {
        public ContactInputVM Form { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
        public IEnumerable<SelectListItem> Yachts { get; set; }
    }

    public class ContactInputVM
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(25)]
        public string Phone { get; set; }

        [Required]
        [Display(Name="Country")]
        public int CountryId { get; set; }

        [Required]
        [Display(Name = "Brochure of Interest")]
        public int YachtId { get; set; }

        [StringLength(200)]
        [Display(Name = "Comments")]
        public string Comment { get; set; }
    }
}