using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Models;
using BBService.MyClasses;
using Microsoft.AspNet.SignalR;

namespace BBService.Controllers
{
    public class BackgroundJobsMeController : Controller
    {
        BBServiceEntities db = new BBServiceEntities();
        // GET: BackgroundJobsMe
        //You can test bg job using any table of db

        public ActionResult Index()
        {
            List<InitialControlSchedule> InitialControlSchedule = db.InitialControlSchedule.GroupBy(x => x.VehicleId, (key, g) => g.OrderByDescending(e => e.EnterTime).FirstOrDefault()).ToList();
            string result = "";
            foreach (var item in InitialControlSchedule)
            {
                result += item.Vehicles.Number + "_" + item.EnterKilometer + "***";
            }
            return Content(result);
        }


        public ActionResult MaintenanceFirstCheckUp()
        {
            UniversalMethods universalMethods = new UniversalMethods();
            universalMethods.MaintenanceFirstCheckUp();

            //Test
            //UniversalMethods universalMethods = new UniversalMethods();
            //universalMethods.testMethod();

            return Content("Success");
        }

        public ActionResult MaintenanceSecondCheckUp()
        {
            UniversalMethods universalMethods = new UniversalMethods();
            universalMethods.MaintenanceSecondCheckUp();

            return Content("Success");
        }








        //Testing area

        public bool CheckForSparePartIfMustAdd(int? VehicleId, decimal? kmLimit, int? spId)
        {
            //Return true if spare part includes to the range
            //Return false if spare part do not includes to the range
            bool result = false;
            decimal? presentKM = db.InitialControlSchedule.Where(i => i.VehicleId == VehicleId).OrderByDescending(o => o.EnterTime).FirstOrDefault().EnterKilometer;
            decimal? kmDif = presentKM - kmLimit;
            List<JobCards> jobCards = db.JobCards.Where(j => j.CheckUpCard.InitialControlSchedule.VehicleId == VehicleId && j.CheckUpCard.InitialControlSchedule.EnterKilometer >= kmDif).ToList();
            foreach (var item in jobCards)
            {
                foreach (var item2 in item.Requisitions)
                {
                    if (item2.TempWarehouseId == spId)
                    {
                        result = true;
                        goto Return;
                    }
                }
            }

            Return:
            return result;
            //return presentKM;
        }

        public ActionResult testMethod()
        {            
            //test.GetUserId();
            return Content(CheckForSparePartIfMustAdd(5,25000,1).ToString());
        }
    }
}