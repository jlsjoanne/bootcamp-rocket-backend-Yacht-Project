using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class Contact
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(25)]
        public string Phone { get; set; }

        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }

        [ForeignKey("Yacht")]
        [Display(Name="Brochure of Interest")]
        public int YachtId { get; set; }
        public virtual Yacht Yacht { get; set; }

        [StringLength(200)]
        [Display(Name="Comments")]
        public string Comment { get; set; }
    }
}