using BBService.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Models;

namespace BBService.Controllers
{
    [LogOut]
    public class ReportsController : Controller
    {
        // GET: Reports
        BBServiceEntities db = new BBServiceEntities();

        [OperationFilter]
        public ActionResult RepIndex()
        {
            ViewBag.Reports = true;

            //Notification reload
            Users usr = (Users)Session["User"];
            int notificCount = 0;
            int count = 0;
            bool notificStatus = false;
            List<Notifications> Notifications = db.Notifications.Where(n => n.UserId == usr.Id).OrderByDescending(n => n.AddingDate).ToList();
            List<Notifications> Last15Notifications = new List<Notifications>();

            foreach (var item in Notifications)
            {
                if (item.Status == true)
                {
                    notificCount++;
                }
                if (count < 15)
                {
                    Last15Notifications.Add(item);
                }
                count++;
            }
            if (count > 0)
            {
                notificStatus = true;
            }

            Session["Last15Notifications"] = Last15Notifications;
            Session["notificStatus"] = notificStatus;
            Session["notificCount"] = notificCount;
            //End

            return View();
        }















        //API Test
        public ActionResult APITest()
        {
            ViewBag.Reports = true;            
            return View();
        }
    }
}