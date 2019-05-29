using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Filters;
using BBService.Models;
using BBService.MyClasses;

namespace BBService.Controllers
{
    [LogOut]
    public class NotificationsController : Controller
    {
        // GET: Notifications
        BBServiceEntities db = new BBServiceEntities();

        public ActionResult NotifIndex(SearchModelDate searchModel)
        {
            ViewBag.Home = true;
            if (Session["UserId"] != null)
            {
                int UserId = (int)Session["UserId"];

                ViewHome model = new ViewHome();
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrEmpty(searchModel.startDate))
                {
                    startDate = DateTime.ParseExact(searchModel.startDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(searchModel.endDate))
                {
                    endDate = DateTime.ParseExact(searchModel.endDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                }

                model.Notifications = db.Notifications.Where(av => (av.UserId == UserId) &&
                                                                  (startDate != null ? av.AddingDate >= startDate : true) &&
                                                                  (endDate != null ? av.AddingDate <= endDate : true)
                                                                  ).OrderByDescending(av => av.AddingDate).ToList();
                model.SearchDate = searchModel;
                return View(model);
            }
            else
            {
                return RedirectToAction("login", "home");
            }

        }

        public ActionResult NotifDetails(SearchModelDate searchModel)
        {
            ViewBag.Home = true;
            if (searchModel.employeeId != null)
            {
                if (ModelState.IsValid)
                {
                    Notifications notif = db.Notifications.Find(searchModel.notificationId);
                    notif.Status = false;
                    db.Entry(notif).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("DrvInpIndex", "Inputs", new { searchModel.employeeId });
            }
            else
            {
                return RedirectToAction("NotifIndex");
            }
        }


        //public JsonResult GetNotificationContacts()
        //{
        //    var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
        //    NotificationComponents NC = new NotificationComponents();
        //    var list = NC.GetContacts(notificationRegisterTime);
        //    //update session here for get only new added contacts (notification)
        //    Session["LastUpdate"] = DateTime.Now;
        //    return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}

        public JsonResult GetNotifications()
        {
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
            NotificationComponents NC = new NotificationComponents();
            var list = NC.GetContacts(notificationRegisterTime);
            //update session here for get only new added contacts (notification)
            Session["LastUpdated"] = DateTime.Now;
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNotificationsOfUser(int Id)
        {
            AJAXResponseModel result = new AJAXResponseModel();
            result.status = db.Notifications.Where(n => n.UserId == Id && n.Status == true).Count().ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //public JsonResult GetNotifications()
        //{
        //    Users usr = (Users)Session["User"];

        //    var list = db.Notifications.Where(n => n.UserId == usr.Id).OrderByDescending(n => n.AddingDate).Take(15).Select(n=> new {
        //        n.NotificationTitleId,
        //        n.DataId,
        //        n.UserId,
        //        n.AddingDate
        //    }).ToList();
        //    //Session["LastUpdated"] = DateTime.Now;
        //    return Json(list, JsonRequestBehavior.AllowGet);
        //}
    }
}