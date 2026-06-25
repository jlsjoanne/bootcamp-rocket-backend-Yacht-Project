using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TayanaYachts.Models.ViewModels
{
    public class YachtPageVM
    {
        public IEnumerable<Yacht> Yachts { get; set; }

        public Yacht CurrentYacht { get; set; }
        public YachtTab ActiveTab { get; set; }
    }

    public enum YachtTab
    {
        Overview,
        Layout,
        Specification
    }
}