using BBService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BBService.Filters
{

    public class OperationFilter : ActionFilterAttribute
    {
        BBServiceEntities db = new BBServiceEntities();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool Access = false;

            string action = (string)filterContext.RouteData.Values["action"];

            int ActionId = 0;
            if (db.Actions.FirstOrDefault(a => a.Name == action) != null)
            {
                ActionId = db.Actions.FirstOrDefault(a => a.Name == action).Id;
            }

            int UserId = (int)HttpContext.Current.Session["UserId"];
            bool Status = false;
            if (db.Users.Find(UserId).IsAdmin == true)
            {
                Status = true;
            }

            ViewHome model = new ViewHome();
            model.Permissions = db.Permissions.ToList();

            if (Status)
            {
                Access = true;
            }
            else
            {
                foreach (var item in model.Permissions)
                {
                    if (item.UserId == UserId && item.ActionId == ActionId)
                    {
                        Access = true;
                    }
                }
            }

            if (Access == false)
            {
                filterContext.Result = new RedirectResult("~/home/index");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}