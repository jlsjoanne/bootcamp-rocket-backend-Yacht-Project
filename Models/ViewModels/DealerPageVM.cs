using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TayanaYachts.Models.ViewModels
{
    public class DealerPageVM
    {
        public IEnumerable<Country> Countries { get; set; }
        public Country SelectedCountry { get; set; }
        public IEnumerable<Dealer> Dealers { get; set; }

    }
}