using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using BBService.Filters;
using BBService.Models;
using BBService.MyClasses;

namespace BBService.Controllers
{
    public class HomeController : Controller
    {
        BBServiceEntities db = new BBServiceEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Users usr)
        {
            string UserName = null;
            if (usr.Fullname.Contains("."))
            {
                string[] userName = usr.Fullname.Split('.');
                UserName = userName[0] + " " + userName[1];
            }

            Users loginner = db.Users.FirstOrDefault(a => a.Fullname == UserName);
            if (loginner != null)
            {
                if (loginner.Password != null && usr.Password != null)
                {
                    if (Crypto.VerifyHashedPassword(loginner.Password, usr.Password))
                    {
                        Session["LoginValid"] = true;
                        Session["UserId"] = loginner.Id;
                        Session["User"] = loginner;
                        int UserId = loginner.Id;
                        bool? UserStatus = loginner.IsAdmin;

                        List<string> permitted = new List<string>();

                        if (UserStatus == true)
                        {
                            foreach (var item in db.Actions)
                            {
                                permitted.Add(item.Name);
                            }
                        }
                        else
                        {
                            foreach (var item in db.Actions)
                            {
                                foreach (var item2 in db.Permissions)
                                {
                                    if (item2.UserId == UserId && item2.ActionId == item.Id)
                                    {
                                        permitted.Add(item.Name);
                                    }
                                }
                            }
                        }

                        Session["permitted"] = permitted;

                        return RedirectToAction("index", "home");
                    }
                }
                else
                {
                    return RedirectToAction("login", "home");
                }

            }



            Session["LoginInvalid"] = true;
            return View();
        }

        [LogOut]
        public ActionResult Index()
        {
            ViewBag.Home = true;
            //UniversalMethods uniMeth = new UniversalMethods();
            //uniMeth.NotificationUpdate();
            ViewBag.InRoute = db.InitialControlSchedule.Where(i => i.IsOpen == true).ToList().Count;
            ViewBag.InDepot = 316 - db.InitialControlSchedule.Where(i => i.IsOpen == true).ToList().Count;
            ViewBag.InRepair = db.JobCards.Where(i => i.IsOpen == true).ToList().Count;
            ViewBag.InWaitRoute = db.JobCards.Where(i => i.InWaitRoute == true).ToList().Count;
            ViewBag.InWaitDepot = db.JobCards.Where(i => i.InWaitDepot == true).ToList().Count;
            ViewBag.Recall = db.JobCards.Where(i => i.InWaitDepot == true).ToList().Count;

            //
            DateTime leaveDateStart = DateTime.ParseExact(DateTime.Today.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);            
            ViewBag.Accident = db.JobCards.Where(i => i.IsAccident == true && DbFunctions.TruncateTime(i.AddedDate) == leaveDateStart).ToList().Count;

            Users usr = (Users)Session["User"];
            int notificCount = 0;

            List<Notifications> Notifications = db.Notifications.Where(n => n.UserId == usr.Id).OrderByDescending(n => n.AddingDate).Take(15).ToList();
            notificCount = db.Notifications.Where(n => n.UserId == usr.Id && n.Status == true).Count();

            Session["Last15Notifications"] = Notifications;
            Session["notificCount"] = notificCount;
            return View();
        }

        //Loging out
        [LogOut]
        public ActionResult Logout()
        {
            Session["LoginValid"] = null;
            return RedirectToAction("login", "home");
        }

        //Creating cookie
        //public void CreateUserNameCookieCookie(string userName)
        //{
        //    HttpCookie userNameCookie = new HttpCookie("userNameCookie");
        //    userNameCookie.Value = userName;
        //    userNameCookie.Expires = DateTime.Now.AddHours(1);
        //    Response.SetCookie(userNameCookie);
        //    Response.Flush();
        //}

        //some action method
        
        //Response.Cookies.Add(CreateStudentCookie());


        //private void CreateCokies(string userNameCookie)
        //{
        //    var authTicket = new FormsAuthenticationTicket(1, userNameCookie, DateTime.Now, DateTime.Now.AddMinutes(30), true, userNameCookie);
        //    string cookieContents = FormsAuthentication.Encrypt(authTicket);
        //    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieContents)
        //    {
        //        Expires = authTicket.Expiration,
        //        Path = FormsAuthentication.FormsCookiePath
        //    };
        //    Response.Cookies.Add(cookie);
        //}
    }
}