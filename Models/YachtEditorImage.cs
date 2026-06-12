using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayanaYachts.Models
{
    public class YachtEditorImage : InEditorImage
    {
        public int? YachtId { get; set; }

        [ForeignKey("YachtId")]
        public virtual Yacht Yacht { get; set; }
    }
}