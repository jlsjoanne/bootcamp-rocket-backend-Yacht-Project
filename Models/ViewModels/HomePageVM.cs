using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace TayanaYachts.Models.ViewModels
{
    public class HomePageVM
    {
        public IList<HomeHeroSlideVM> HeroSlides { get; set; }
        public IList<HomeNewsItemVM> LatestNews { get; set; }

        public HomePageVM()
        {
            HeroSlides = new List<HomeHeroSlideVM>();
            LatestNews = new List<HomeNewsItemVM>();
        }
    }

    public class HomeHeroSlideVM
    {
        public int YachtId { get; set; }
        public string HeroTitle { get; set; }
        public string HeroNumber { get; set; }
        public string YachtName { get; set; }
        public string HeroImagePath { get; set; }
        public string HeroImageAlt { get; set; }
        public bool IsNew { get; set; }
    }

    public class HomeNewsItemVM
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime PublishDate { get; set; }
        public string ThumbnailPath { get; set; }
        public string ThumbnailAlt { get; set; }
        public bool IsPinned { get; set; }
    }
}