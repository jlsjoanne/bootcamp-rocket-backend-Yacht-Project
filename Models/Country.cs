using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class Country
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name="Country Name")]
        public string Name { get; set; }

        public virtual ICollection<Area> Areas { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 9999)]
        public int SortOrder { get; set; }

        public Country()
        {
            Areas = new HashSet<Area>();
        }
    }
}