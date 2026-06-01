using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TayanaYachts.DAL;
using TayanaYachts.Models;
using TayanaYachts.Models.ViewModels;
using TayanaYachts.Methods;
using System.Web.Security;
using Newtonsoft.Json;

namespace TayanaYachts.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {

        private TayanaContext db = new TayanaContext();
        // GET: Admin/Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LogInVM login)
        {
            if (ModelState.IsValid)
            {
                var member = db.Members.FirstOrDefault(m => m.Account == login.Account);
                if(member == null)
                {
                    ModelState.AddModelError("", "登入失敗");
                    return View(login);
                }

                // 密碼加密對照
                var passwordSalt = member.PasswordSalt;
                var hashPassword = Utility.GenerateHashWithSalt(login.Password, passwordSalt);
                if(hashPassword != member.Password)
                {
                    ModelState.AddModelError("", "登入失敗");
                    return View(login);
                }

                // 產生表單驗證票
                var userData = JsonConvert.SerializeObject(member);
                Utility.SetAuthenTicket(userData, member.Id.ToString());

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return View(login);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}