using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;


namespace TayanaYachts.Models
{
    public class DealerImage
    {
        [Key]
        [ForeignKey("Dealer")]
        public int DealerId { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; }

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; }

        [Required]
        [StringLength(20)]
        public string FileType { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        public virtual Dealer Dealer { get; set; }

    }
}