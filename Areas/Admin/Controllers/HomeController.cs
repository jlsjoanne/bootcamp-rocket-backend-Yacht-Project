using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TayanaYachts.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        // Admin Management Homepage
        public ActionResult Index()
        {
            return View();
        }
    }
}