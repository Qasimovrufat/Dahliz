using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Filters;
using BBService.Models;
using BBService.MyClasses;

namespace BBService.Controllers
{
    [LogOut]
    public class SystemOperationsController : Controller
    {
        // GET: SystemOperations
        BBServiceEntities db = new BBServiceEntities();
        
        [OperationFilter]
        public ActionResult SysOprIndex()
        {
            ViewBag.Sys = true;

            //Notification
            Users usr = (Users)Session["User"];
            int notificCount = 0;

            List<Notifications> Notifications = db.Notifications.Where(n => n.UserId == usr.Id).OrderByDescending(n => n.AddingDate).Take(15).ToList();
            notificCount = db.Notifications.Where(n => n.UserId == usr.Id && n.Status == true).Count();

            Session["Last15Notifications"] = Notifications;
            Session["notificCount"] = notificCount;
            //End

            return View();
        }
    }
}