using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TayanaYachts.Models
{
    public class DealerImage : InEditorImage
    {
        public int? DealerId { get; set; }

        [ForeignKey("DealerId")]
        public virtual Dealer Dealer { get; set; }
    }
}