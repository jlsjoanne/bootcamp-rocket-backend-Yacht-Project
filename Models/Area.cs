using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TayanaYachts.Models
{
    public class Area
    {
        public int Id { get;set; }

        [Required]
        [StringLength(200)]
        [Display(Name="Area Name")]
        public string Name { get; set; }

        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<Dealer> Dealers { get;set; }
    }
}