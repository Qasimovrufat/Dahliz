using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Models;

using BBService.Filters;
using System.Globalization;
using ClosedXML.Excel;
using System.IO;
using BBService.MyClasses;
using Microsoft.AspNet.SignalR;

namespace BBService.MyClasses
{
    public class UniversalMethods : Controller
    {
        //private HttpContextBase Context { get; set; }

        //public UniversalMethods(HttpContextBase context)
        //{
        //    this.Context = context;
        //}

        BBServiceEntities db = new BBServiceEntities();

        //Background jobs
        //Check for maintenance
        public void MaintenanceFirstCheckUp()
        {
            int? MaintenanceCheckupStep = db.Settings.FirstOrDefault(s => s.Name == "MaintenanceStepValue").Value;

            List<InitialControlSchedule> InitialControlSchedule = db.InitialControlSchedule.Where(i => i.EnterKilometer != null)
                                                                                           .GroupBy(x => x.VehicleId, (key, g) => g.OrderByDescending(e => e.EnterTime).FirstOrDefault()).ToList();
            List<MaintenanceType> MaintenanceType = db.MaintenanceType.ToList();

            foreach (var item in InitialControlSchedule)
            {
                foreach (var item2 in MaintenanceType)
                {
                    MaintenanceHistory MaintenanceHistoryFiltered = db.MaintenanceHistory.FirstOrDefault(m => m.InitialControlSchedule.VehicleId == item.VehicleId && m.MaintenanceId == item2.Id);

                    if (MaintenanceHistoryFiltered == null && item.EnterKilometer >= (item2.MaintenanceValue - MaintenanceCheckupStep))
                    {
                        MaintenanceHistory newMaintenanceHistory = new MaintenanceHistory();
                        newMaintenanceHistory.InitContId = item.Id;
                        newMaintenanceHistory.MaintenanceId = item2.Id;
                        newMaintenanceHistory.MaintenanceStatus = null;
                        newMaintenanceHistory.AddedDate = DateTime.Now;
                        db.MaintenanceHistory.Add(newMaintenanceHistory);
                        db.SaveChanges();
                    }
                }
            }
        }

        //Add jobcards
        public void MaintenanceSecondCheckUp()
        {
            int? MaintenanceCheckupStep2 = db.Settings.FirstOrDefault(s => s.Name == "MaintenanceStep2Value").Value;

            List<MaintenanceType> MaintenanceType = db.MaintenanceType.ToList();
            List<MaintenanceHistory> MaintenanceHistory = db.MaintenanceHistory.Where(m => m.MaintenanceStatus == null).ToList();

            foreach (var item in MaintenanceHistory)
            {
                JobCards jobCards = db.JobCards.FirstOrDefault(j => j.IsOpen == true && j.CheckUpCard.InitialControlSchedule.VehicleId == item.InitialControlSchedule.VehicleId);

                if (jobCards == null)
                {
                    foreach (var item2 in MaintenanceType)
                    {
                        if (item.MaintenanceId == item2.Id && db.InitialControlSchedule.Where(i => i.IsOpen == false && i.VehicleId == item.InitialControlSchedule.VehicleId).OrderByDescending(k => k.EnterTime).FirstOrDefault().EnterKilometer > (item2.MaintenanceValue + MaintenanceCheckupStep2))
                        {
                            //Create Checkup card
                            CheckUpCard checkUpCard = new CheckUpCard();
                            checkUpCard.InitContId = item.InitContId;
                            checkUpCard.MaintenanceTypeId = item2.Id;
                            checkUpCard.Description = "Created by System. Texniki qulluq - " + item2.Name + "/" + item2.MaintenanceValue + "  / Faktiki kilometr " + db.InitialControlSchedule.Where(i => i.VehicleId == item.InitialControlSchedule.VehicleId).OrderByDescending(i => i.EnterTime).FirstOrDefault().EnterKilometer;
                            checkUpCard.IsOpen = true;
                            checkUpCard.AddedDate = DateTime.Now;


                            //Create Jobcard
                            int? ConstantNo = db.Settings.FirstOrDefault(s => s.Name == "JobCardNoStart").Value;
                            int? MaxNo = db.JobCards.Max(j => j.JobCardNo);
                            JobCards newJobCards = new JobCards();
                            if (MaxNo != null)
                            {
                                newJobCards.JobCardNo = MaxNo + 1;
                            }
                            else
                            {
                                newJobCards.JobCardNo = ConstantNo;
                            }
                            newJobCards.CheckupCardId = checkUpCard.Id;
                            newJobCards.IsOpen = true;
                            newJobCards.CreatedBySystem = true;
                            newJobCards.IsMaintenance = true;
                            newJobCards.InUnderRepair = true;
                            newJobCards.AddedDate = DateTime.Now;
                            newJobCards.CompanyId = db.Companies.FirstOrDefault(c => c.IsClient == true).Id;

                            ////Updating Maintenance history
                            //MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.Find(item.Id);
                            //maintenanceHistory.JobCardId = newJobCards.Id;
                            //db.Entry(maintenanceHistory).State = EntityState.Modified;
                            
                            //Adding to db and saving
                            db.CheckUpCard.Add(checkUpCard);
                            db.JobCards.Add(newJobCards);

                            //Check for maintenance history and create requisitioin
                            CheckForMaintenanceHistoryAndCreateRequisition(checkUpCard.InitialControlSchedule.VehicleId, checkUpCard, newJobCards);

                            db.SaveChanges();
                        }
                    }
                }                
            }
        }
        //End of background jobs

        //Check for maintenance history and create requisitioin
        public void CheckForMaintenanceHistoryAndCreateRequisition(int? VehicleId, CheckUpCard CheckUp, JobCards jobCard)
        {
            //Check for maintenance
            MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.FirstOrDefault(m => m.MaintenanceStatus == null && m.JobCardId == null && m.InitialControlSchedule.VehicleId == VehicleId);
            if (maintenanceHistory != null)
            {
                CheckUp.MaintenanceTypeId = maintenanceHistory.MaintenanceId;
                jobCard.IsMaintenance = true;

                //Updating Maintenance history
                MaintenanceHistory newMaintenanceHistory = db.MaintenanceHistory.Find(maintenanceHistory.Id);
                newMaintenanceHistory.JobCardId = jobCard.Id;
                if (ModelState.IsValid)
                {
                    db.Entry(newMaintenanceHistory).State = EntityState.Modified;
                }

                //Create Requisition
                int? brandId = db.Vehicles.Find(VehicleId).BrandId;
                List<WarehouseToMaintenance> warehouseToMaintenances = db.WarehouseToMaintenance.Where(w => w.MaintenanceTypeId == maintenanceHistory.MaintenanceId && w.TempWarehouse.VehicleId == brandId).ToList();

                foreach (var item in warehouseToMaintenances)
                {
                    //If it is includes to the range(if true) must not erquire SP else require
                    if (!CheckForSparePartIfMustAdd(VehicleId, item.NotRequireSPLimit, item.WarehouseId))
                    {
                        Requisitions requisition = new Requisitions();
                        requisition.IsOpen = true;
                        requisition.JobCardId = jobCard.Id;
                        requisition.TempWarehouseId = item.WarehouseId;
                        requisition.RequiredQuantity = item.Quantity;
                        requisition.AddedDate = DateTime.Now;

                        db.Requisitions.Add(requisition);
                        db.SaveChanges();
                    }                    
                }
            }
        }

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
        }












        //public void NotificationUpdate()
        //{
        //    //Notification reload
        //    //BBService.Controllers.HomeController.Session["User"];
        //    //Users usr = Controllers.HomeController.Equals().;

        //    //Users usr = (Users)this.Context.Session["User"];

        //    int notificCount = 0;
        //    List<Notifications> Notifications = db.Notifications.Where(n => n.UserId == 1).OrderByDescending(n => n.AddingDate).Take(15).ToList();
        //    notificCount = db.Notifications.Where(n => n.UserId == 1 && n.Status == true).Count();

        //    Session["Last15Notifications"] = Notifications;
        //    Session["notificCount"] = notificCount;
        //    //End
        //}






        //Test method
        public void testMethod()
        {
            //TempWarehouse temp = new TempWarehouse();
            //temp.Name = "Test";
            //temp.Quantity = 1;
            //temp.VehicleId = 5;
            //db.TempWarehouse.Add(temp);
            //db.SaveChanges();
        }
    }
}