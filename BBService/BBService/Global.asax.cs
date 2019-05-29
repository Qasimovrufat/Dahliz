using BBService.Models;
using BBService.MyClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BBService
{
    public class MvcApplication : System.Web.HttpApplication
    {
        string con = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;

            //Start sql dependency - ALTER DATABASE BBService SET ENABLE_BROKER
            //SqlDependency.Start(con);
        }
        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");           //Remove Server Header   
            Response.Headers.Remove("X-AspNet-Version"); //Remove X-AspNet-Version Header
        }

        //protected void Session_Start(object sender, EventArgs e)
        //{
        //    NotificationComponents NC = new NotificationComponents();            
        //    var currentTime = DateTime.Now;
        //    HttpContext.Current.Session["LastUpdated"] = currentTime;
        //    NC.RegisterNotification(currentTime);
        //}


        protected void Application_End()
        {
            //here we will stop Sql Dependency
            //SqlDependency.Stop(con);
        }
    }
}
