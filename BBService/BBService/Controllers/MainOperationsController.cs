using BBService.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Models;
using System.Data.Entity;
using System.Globalization;
using ClosedXML.Excel;
using System.IO;
using BBService.MyClasses;
using Rotativa;

namespace BBService.Controllers
{
    [LogOut]
    public class MainOperationsController : Controller
    {
        // GET: MainOperations
        BBServiceEntities db = new BBServiceEntities();

        [OperationFilter]
        public ActionResult OprIndex()
        {
            ViewBag.Ope = true;

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

        //Create
        [OperationFilter]
        public ActionResult InitContCreate()
        {
            ViewBag.Ope = true;
            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList(),
                Routes = db.Routes.ToList(),
                Employees = db.Employees.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult InitContCreate(InitControl initCon)
        {
            ViewBag.Ope = true;
            InitialControlSchedule InitCon = new InitialControlSchedule();
            CheckUpCard CheckUp = new CheckUpCard();
            ViewHome model = new ViewHome()
            {
                Vehicles = db.Vehicles.ToList(),
                Routes = db.Routes.ToList(),
                Employees = db.Employees.ToList()
            };
                       
            //Check if vehicle is in a route
            if (db.InitialControlSchedule.FirstOrDefault(i => i.IsOpen == true && i.VehicleId == initCon.VehicleId) != null)
            {
                Session["repeatedVehicle"] = true;
                TempData["InitCon"] = initCon;
                return View(model);
            }

            if (initCon.CheckUp == true)
            {
                //Create Checkup cars

                //Choose vehicle
                if (initCon.VehicleId == 0)
                {
                    Session["emptyFild"] = true;
                    TempData["InitCon"] = initCon;
                    return View(model);
                }


                //Vehicle is in maintenance or under repair
                JobCards newJobCards = db.JobCards.FirstOrDefault(j => j.IsOpen == true && j.CheckUpCard.InitialControlSchedule.VehicleId == initCon.VehicleId);
                if (newJobCards != null)
                {
                    Session["VehicleIsUnderRepair"] = true;
                    TempData["InitCon"] = initCon;
                    return View(model);
                }


                CheckUp.IsOpen = true;

                int userId = (int)Session["UserId"];
                CheckUp.InitContId =db.InitialControlSchedule.Where(i=>i.VehicleId==initCon.VehicleId).OrderByDescending(i=>i.EnterUpdateDate).FirstOrDefault()!=null? db.InitialControlSchedule.Where(i => i.VehicleId == initCon.VehicleId).OrderByDescending(i => i.EnterUpdateDate).FirstOrDefault().Id :Convert.ToInt32(null);
                CheckUp.Description = initCon.Description;
                CheckUp.AddedDate = DateTime.Now;
                CheckUp.AddedUserId = userId;
                if (db.Users.Find(userId).EmployeeId != null)
                {
                    CheckUp.AddedMechanicId = db.Users.Find(userId).EmployeeId;
                }
                else
                {
                    CheckUp.AddedMechanicId = null;
                }


                //Create Job Card
                JobCards jobCard = new JobCards();
                int? ConstantNo = db.Settings.FirstOrDefault(s => s.Name == "JobCardNoStart").Value; ;
                int? MaxNo = db.JobCards.Max(j => j.JobCardNo);
                if (MaxNo != null)
                {
                    jobCard.JobCardNo = MaxNo + 1;
                }
                else
                {
                    jobCard.JobCardNo = ConstantNo;
                }

                jobCard.IsOpen = true;
                jobCard.InUnderRepair = true;
                jobCard.CheckupCardId = CheckUp.Id;
                jobCard.CompanyId = db.Companies.FirstOrDefault(c => c.IsClient == true).Id;

                //Adding
                db.CheckUpCard.Add(CheckUp);
                db.JobCards.Add(jobCard);

                //Check for maintenance history
                UniversalMethods universalMethods = new UniversalMethods();
                universalMethods.MaintenanceFirstCheckUp();
                //universalMethods.CheckForMaintenanceHistoryAndCreateRequisition(initCon.VehicleId, CheckUp, jobCard);


                //Check for maintenance
                MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.FirstOrDefault(m => m.MaintenanceStatus == null && m.JobCardId == null && m.InitialControlSchedule.VehicleId == initCon.VehicleId);
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
                    int? brandId = db.Vehicles.Find(initCon.VehicleId).BrandId;
                    List<WarehouseToMaintenance> warehouseToMaintenances = db.WarehouseToMaintenance.Where(w => w.MaintenanceTypeId == maintenanceHistory.MaintenanceId && w.TempWarehouse.VehicleId == brandId).ToList();

                    foreach (var item in warehouseToMaintenances)
                    {
                        if (!universalMethods.CheckForSparePartIfMustAdd(initCon.VehicleId, item.NotRequireSPLimit, item.WarehouseId))
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
            else
            {
                //Create Init Cont Card

                //Choose vehicle
                if (initCon.VehicleId == 0 || initCon.RouteId == 0 || initCon.FirstDriverId == 0 || string.IsNullOrEmpty(initCon.LeavingKilometer) || string.IsNullOrEmpty(initCon.LeavingFuel))
                {
                    Session["emptyFild"] = true;
                    TempData["InitCon"] = initCon;
                    return View(model);
                }


                //Repeated vehicle or is in maintenance
                JobCards jobCards = db.JobCards.FirstOrDefault(j => j.IsOpen == true && j.InWaitRoute == null && j.CheckUpCard.InitialControlSchedule.VehicleId == initCon.VehicleId);
                if (jobCards != null)
                {
                    Session["VehicleIsUnderRepair"] = true;
                    TempData["InitCon"] = initCon;
                    return View(model);
                }

                //Mechanics
                int UserId = (int)Session["UserId"];
                int? MechId = null;
                int? SeniorMechId = null;
                if (db.Users.FirstOrDefault(u => u.Id == UserId) != null)
                {
                    MechId = db.Users.FirstOrDefault(u => u.Id == UserId).EmployeeId;
                    InitCon.LeavingRecMechId = MechId;
                }

                if (db.MechanicBinding.FirstOrDefault(b => b.RecMechId == MechId) != null)
                {
                    SeniorMechId = db.MechanicBinding.FirstOrDefault(b => b.RecMechId == MechId).SeniorRecMechId;
                    InitCon.LeavingSeniorRecMechId = SeniorMechId;
                }
                if (MechId == null || SeniorMechId == null)
                {
                    Session["DataDeficiency"] = true;
                    TempData["InitCon"] = initCon;
                    return View(model);
                }

                InitCon.IsOpen = true;
                InitCon.LeavingTime = DateTime.Now;
                if (initCon.VehicleId != 0)
                {
                    InitCon.VehicleId = initCon.VehicleId;
                }
                else
                {
                    InitCon.VehicleId = null;
                }

                //Leaving km
                decimal? kmDot = null;
                decimal kmTemp;
                string kmTempDot;

                if (initCon.LeavingKilometer != null && initCon.LeavingKilometer.Contains("."))
                {
                    kmTempDot = initCon.LeavingKilometer.Replace('.', ',');
                }
                else if (initCon.LeavingKilometer != null)
                {
                    kmTempDot = initCon.LeavingKilometer;
                }
                else
                {
                    kmTempDot = null;
                }

                if (kmTempDot != null && decimal.TryParse(kmTempDot, out kmTemp))
                {
                    kmDot = decimal.Parse(kmTempDot);
                }
                InitCon.LeavingKilometer = kmDot;
                //End

                //Check km
                if (initCon.VehicleId != 0 && initCon.VehicleId != null)
                {
                    ViewBag.InitCont = db.InitialControlSchedule.Where(i => i.VehicleId == initCon.VehicleId).OrderByDescending(i => i.EnterKilometer).ToList();

                    foreach (var item in ViewBag.InitCont)
                    {
                        if (item.EnterKilometer > kmDot)
                        {
                            Session["overKm"] = true;
                            TempData["InitCon"] = initCon;
                            return View(model);
                        }
                    }
                }


                //Leaving fuel
                decimal? fuelDot = null;
                decimal fuelTemp;
                string fuelTempDot;

                if (initCon.LeavingFuel != null && initCon.LeavingFuel.Contains("."))
                {
                    fuelTempDot = initCon.LeavingFuel.Replace('.', ',');
                }
                else if (initCon.LeavingFuel != null)
                {
                    fuelTempDot = initCon.LeavingFuel;
                }
                else
                {
                    fuelTempDot = null;
                }

                if (fuelTempDot != null && decimal.TryParse(fuelTempDot, out fuelTemp))
                {
                    fuelDot = decimal.Parse(fuelTempDot);
                }
                InitCon.LeavingFuel = fuelDot;
                //End

                //Route
                if (initCon.RouteId != 0)
                {
                    InitCon.RouteId = initCon.RouteId;
                }
                else
                {
                    InitCon.RouteId = null;
                }

                //Route Length
                if (initCon.RouteId != 0 && db.Routes.FirstOrDefault(r => r.Id == initCon.RouteId).ForwardLength != null && db.Routes.FirstOrDefault(r => r.Id == initCon.RouteId).BackwardLength != null)
                {
                    InitCon.RouteLength = db.Routes.FirstOrDefault(r => r.Id == initCon.RouteId).ForwardLength + db.Routes.FirstOrDefault(r => r.Id == initCon.RouteId).BackwardLength;
                }
                else
                {
                    InitCon.RouteLength = null;
                }

                //First driver
                if (initCon.FirstDriverId != 0)
                {
                    InitCon.FirstDriverId = initCon.FirstDriverId;
                }
                else
                {
                    InitCon.FirstDriverId = null;
                }

                //Leaving note
                InitCon.LeavingNote = initCon.LeavingNote;

                db.InitialControlSchedule.Add(InitCon);
            }

            db.SaveChanges();
            Session["InitContCreated"] = true;

            return RedirectToAction("InitContCreate");
        }

        //Open        
        [OperationFilter]
        public ActionResult OpenInitContIndex(SearchModelInitCont searchModel)
        {
            ViewBag.Ope = true;
            ViewHome model = new ViewHome()
            {
                Vehicles = db.Vehicles.ToList(),
                Routes = db.Routes.ToList(),
                Employees = db.Employees.ToList(),
            };

            //Variables
            //Dates
            DateTime? leaveDateStart = null;
            DateTime? leaveDateEnd = null;

            //Hours
            TimeSpan? leaveHourStart = null;
            TimeSpan? leaveHourEnd = null;



            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.LeaveDateStart))
            {
                leaveDateStart = DateTime.ParseExact(searchModel.LeaveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveDateEnd))
            {
                leaveDateEnd = DateTime.ParseExact(searchModel.LeaveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Leaving Hour
            if (!string.IsNullOrEmpty(searchModel.LeaveHourStart))
            {
                leaveHourStart = TimeSpan.Parse(searchModel.LeaveHourStart);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveHourEnd))
            {
                leaveHourEnd = TimeSpan.Parse(searchModel.LeaveHourEnd);
            }


            //Leaving km
            //min
            decimal? leaveKmMin = null;
            decimal leaveKmMinTemp;
            string leaveKmMinTempDot;

            if (searchModel.LeaveKmMin != null && searchModel.LeaveKmMin.Contains("."))
            {
                leaveKmMinTempDot = searchModel.LeaveKmMin.Replace('.', ',');
            }
            else
            {
                leaveKmMinTempDot = searchModel.LeaveKmMin;
            }
            if (Decimal.TryParse(leaveKmMinTempDot, out leaveKmMinTemp))
            {
                leaveKmMin = leaveKmMinTemp;
            }

            //max
            decimal? leaveKmMax = null;
            decimal leaveKmMaxTemp;
            string leaveKmMaxTempDot;

            if (searchModel.LeaveKmMax != null && searchModel.LeaveKmMax.Contains("."))
            {
                leaveKmMaxTempDot = searchModel.LeaveKmMax.Replace('.', ',');
            }
            else
            {
                leaveKmMaxTempDot = searchModel.LeaveKmMax;
            }
            if (Decimal.TryParse(leaveKmMaxTempDot, out leaveKmMaxTemp))
            {
                leaveKmMax = leaveKmMaxTemp;
            }
            //End


            //Leaving fuel
            //min
            decimal? leaveFuelMin = null;
            decimal leaveFuelMinTemp;
            string leaveFuelMinTempDot;

            if (searchModel.LeaveFuelMin != null && searchModel.LeaveFuelMin.Contains("."))
            {
                leaveFuelMinTempDot = searchModel.LeaveFuelMin.Replace('.', ',');
            }
            else
            {
                leaveFuelMinTempDot = searchModel.LeaveFuelMin;
            }
            if (Decimal.TryParse(leaveFuelMinTempDot, out leaveFuelMinTemp))
            {
                leaveFuelMin = leaveFuelMinTemp;
            }

            //max
            decimal? leaveFuelMax = null;
            decimal leaveFuelMaxTemp;
            string leaveFuelMaxTempDot;

            if (searchModel.LeaveFuelMax != null && searchModel.LeaveFuelMax.Contains("."))
            {
                leaveFuelMaxTempDot = searchModel.LeaveFuelMax.Replace('.', ',');
            }
            else
            {
                leaveFuelMaxTempDot = searchModel.LeaveFuelMax;
            }
            if (Decimal.TryParse(leaveFuelMaxTempDot, out leaveFuelMaxTemp))
            {
                leaveFuelMax = leaveFuelMaxTemp;
            }
            //End

            model.InitialControlSchedule = db.InitialControlSchedule.Where(i => (i.IsOpen == true) &&
                                                                                (searchModel.VehicleId != null ? i.VehicleId == searchModel.VehicleId : true) &&
                                                                                (searchModel.RouteId != null ? i.RouteId == searchModel.RouteId : true) &&

                                                                                (searchModel.FirstDriverId != null ? i.FirstDriverId == searchModel.FirstDriverId : true) &&

                                                                                (searchModel.LeavingRecMechId != null ? i.LeavingRecMechId == searchModel.LeavingRecMechId : true) &&
                                                                                (searchModel.LeavingSeniorRecMechId != null ? i.LeavingSeniorRecMechId == searchModel.LeavingSeniorRecMechId : true) &&

                                                                                (searchModel.LeaveDateStart != null ? DbFunctions.TruncateTime(i.LeavingTime) >= leaveDateStart : true) &&
                                                                                (searchModel.LeaveDateEnd != null ? DbFunctions.TruncateTime(i.LeavingTime) <= leaveDateEnd : true) &&

                                                                                (searchModel.LeaveHourStart != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) >= leaveHourStart : true) &&
                                                                                (searchModel.LeaveHourEnd != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) <= leaveHourEnd : true) &&

                                                                                (searchModel.LeaveKmMin != null ? i.LeavingKilometer >= leaveKmMin : true) &&
                                                                                (searchModel.LeaveKmMax != null ? i.LeavingKilometer <= leaveKmMax : true) &&

                                                                                (searchModel.LeaveFuelMin != null ? i.LeavingFuel >= leaveFuelMin : true) &&
                                                                                (searchModel.LeaveFuelMax != null ? i.LeavingFuel <= leaveFuelMax : true)

                                                                                ).OrderByDescending(i => i.LeavingTime).ToList();

            TempData["SortedInitContOpen"] = model;

            model.SearchModelInitCont = searchModel;

            return View(model);
        }

        [OperationFilter]
        public ActionResult InitContUpdate(int id)
        {
            ViewBag.Ope = true;
            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList(),
                Routes = db.Routes.ToList(),
                Employees = db.Employees.ToList()
            };

            ViewBag.InitCont = db.InitialControlSchedule.Find(id);
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult InitContUpdate(InitControl initCon)
        {
            ViewBag.Ope = true;
            InitialControlSchedule InitCon = db.InitialControlSchedule.Find(initCon.Id);

            int UserId = (int)Session["UserId"];
            //Updated user
            InitCon.LeavingUpdateEmpId = UserId;

            //Updated date
            InitCon.LeavingUpdateDate = DateTime.Now;

            //Vehicle
            if (initCon.VehicleId == 0)
            {
                InitCon.VehicleId = null;
            }
            else
            {
                InitCon.VehicleId = initCon.VehicleId;
            }

            //Route
            if (initCon.RouteId == 0)
            {
                InitCon.RouteId = null;
            }
            else
            {
                InitCon.RouteId = initCon.RouteId;
            }

            //First driver
            if (initCon.FirstDriverId == 0)
            {
                InitCon.FirstDriverId = null;
            }
            else
            {
                InitCon.FirstDriverId = initCon.FirstDriverId;
            }

            //Leaving note
            InitCon.LeavingNote = initCon.LeavingNote;

            //Leaving km
            decimal? kmDot = null;
            decimal kmTemp;
            string kmTempDot;

            if (initCon.LeavingKilometer != null && initCon.LeavingKilometer.Contains("."))
            {
                kmTempDot = initCon.LeavingKilometer.Replace('.', ',');
            }
            else if (initCon.LeavingKilometer != null)
            {
                kmTempDot = initCon.LeavingKilometer;
            }
            else
            {
                kmTempDot = null;
            }

            if (kmTempDot != null && decimal.TryParse(kmTempDot, out kmTemp))
            {
                kmDot = decimal.Parse(kmTempDot);
            }
            InitCon.LeavingKilometer = kmDot;
            //End

            //Leaving fuel
            decimal? fuelDot = null;
            decimal fuelTemp;
            string fuelTempDot;

            if (initCon.LeavingFuel != null && initCon.LeavingFuel.Contains("."))
            {
                fuelTempDot = initCon.LeavingFuel.Replace('.', ',');
            }
            else if (initCon.LeavingFuel != null)
            {
                fuelTempDot = initCon.LeavingFuel;
            }
            else
            {
                fuelTempDot = null;
            }

            if (fuelTempDot != null && decimal.TryParse(fuelTempDot, out fuelTemp))
            {
                fuelDot = decimal.Parse(fuelTempDot);
            }
            InitCon.LeavingFuel = fuelDot;
            //End


            db.Entry(InitCon).State = EntityState.Modified;

            //Disable updating
            db.Entry(InitCon).Property(x => x.IsOpen).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingSeniorRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingTime).IsModified = false;

            db.Entry(InitCon).Property(x => x.EnterRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterSeniorRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterTime).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterKilometer).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterFuel).IsModified = false;
            db.Entry(InitCon).Property(x => x.SecondDriverId).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterNote).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterUpdateEmpId).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterUpdateDate).IsModified = false;

            db.SaveChanges();
            return RedirectToAction("OpenInitContIndex");
        }

        [OperationFilter]
        public ActionResult InitContOpenDelete(int id)
        {
            InitialControlSchedule InitCon = db.InitialControlSchedule.Find(id);

            if (InitCon == null)
            {
                return RedirectToAction("OpenInitContIndex");
            }

            db.InitialControlSchedule.Remove(InitCon);
            db.SaveChanges();
            return RedirectToAction("OpenInitContIndex");
        }

        public void ExportOpenInitCont()
        {
            ViewHome model = new ViewHome();
            model = (ViewHome)TempData["SortedInitContOpen"];

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Open Initial Control list");

            ws.Range("B2:L2").Merge();
            ws.Cell("B2").Value = "Açıq yoxlama cədvəlləri siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "NV Nömrəsi";
            ws.Cell("D3").Value = "Xətt";
            ws.Cell("E3").Value = "Sürücü-1";
            ws.Cell("F3").Value = "Tarix";
            ws.Cell("G3").Value = "Saat";
            ws.Cell("H3").Value = "Mexanik";
            ws.Cell("I3").Value = "Baş Mexanik";
            ws.Cell("J3").Value = "Çıxış km";
            ws.Cell("K3").Value = "Çıxış yanacaq";
            ws.Cell("L3").Value = "Qeyd";

            ws.Cell("B2").Style.Font.SetBold();
            ws.Cell("B2").Style.Font.FontSize = 14;
            ws.Cell("B2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("B2").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("B2").Style.Alignment.WrapText = true;


            ws.Cell("B3").Style.Font.SetBold();
            ws.Cell("B3").Style.Font.FontColor = XLColor.White;
            ws.Cell("B3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("B3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("B3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("B3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Alignment.WrapText = true;

            ws.Cell("C3").Style.Font.SetBold();
            ws.Cell("C3").Style.Font.FontColor = XLColor.White;
            ws.Cell("C3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("C3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("C3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("C3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Alignment.WrapText = true;

            ws.Cell("D3").Style.Font.SetBold();
            ws.Cell("D3").Style.Font.FontColor = XLColor.White;
            ws.Cell("D3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("D3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("D3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("D3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Alignment.WrapText = true;

            ws.Cell("E3").Style.Font.SetBold();
            ws.Cell("E3").Style.Font.FontColor = XLColor.White;
            ws.Cell("E3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("E3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("E3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("E3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Alignment.WrapText = true;

            ws.Cell("F3").Style.Font.SetBold();
            ws.Cell("F3").Style.Font.FontColor = XLColor.White;
            ws.Cell("F3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("F3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("F3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("F3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Alignment.WrapText = true;

            ws.Cell("G3").Style.Font.SetBold();
            ws.Cell("G3").Style.Font.FontColor = XLColor.White;
            ws.Cell("G3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("G3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("G3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("G3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Alignment.WrapText = true;

            ws.Cell("H3").Style.Font.SetBold();
            ws.Cell("H3").Style.Font.FontColor = XLColor.White;
            ws.Cell("H3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("H3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("H3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("H3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Alignment.WrapText = true;

            ws.Cell("I3").Style.Font.SetBold();
            ws.Cell("I3").Style.Font.FontColor = XLColor.White;
            ws.Cell("I3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("I3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("I3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("I3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Alignment.WrapText = true;

            ws.Cell("J3").Style.Font.SetBold();
            ws.Cell("J3").Style.Font.FontColor = XLColor.White;
            ws.Cell("J3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("J3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("J3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("J3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Alignment.WrapText = true;

            ws.Cell("K3").Style.Font.SetBold();
            ws.Cell("K3").Style.Font.FontColor = XLColor.White;
            ws.Cell("K3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("K3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("K3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("K3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Alignment.WrapText = true;


            ws.Cell("L3").Style.Font.SetBold();
            ws.Cell("L3").Style.Font.FontColor = XLColor.White;
            ws.Cell("L3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("L3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("L3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("L3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Alignment.WrapText = true;


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 13;
            ws.Column("D").Width = 7;
            ws.Column("E").Width = 30;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 8;
            ws.Column("H").Width = 30;
            ws.Column("I").Width = 30;
            ws.Column("J").Width = 10;
            ws.Column("K").Width = 10;
            ws.Column("L").Width = 40;


            int i = 4;
            foreach (var item in model.InitialControlSchedule)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.VehicleId == null ? "" : item.Vehicles.Number);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;

                ws.Cell("D" + i).Value = (item.RouteId == null ? "" : item.Routes.Number);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                ws.Cell("E" + i).Value = (item.FirstDriverId == null ? "" : item.Employees4.Surname + " " + item.Employees4.Name + " " + item.Employees4.FutherName);
                ws.Cell("E" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                ws.Cell("F" + i).Value = (item.LeavingTime == null ? "" : item.LeavingTime.Value.ToString("dd.MM.yyyy"));
                ws.Cell("F" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("F" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("F" + i).Style.Alignment.WrapText = true;

                ws.Cell("G" + i).Value = (item.LeavingTime == null ? "" : item.LeavingTime.Value.ToString("HH:MM"));
                ws.Cell("G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("G" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("G" + i).Style.Alignment.WrapText = true;

                ws.Cell("H" + i).Value = (item.LeavingRecMechId == null ? "" : item.Employees.Surname + " " + item.Employees.Name + " " + item.Employees.FutherName);
                ws.Cell("H" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("H" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("H" + i).Style.Alignment.WrapText = true;

                ws.Cell("I" + i).Value = (item.LeavingSeniorRecMechId == null ? "" : item.Employees1.Surname + " " + item.Employees1.Name + " " + item.Employees1.FutherName);
                ws.Cell("I" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("I" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("I" + i).Style.Alignment.WrapText = true;

                ws.Cell("J" + i).Value = (item.LeavingKilometer == null ? "" : item.LeavingKilometer.Value.ToString("#.##"));
                ws.Cell("J" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("J" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("J" + i).Style.Alignment.WrapText = true;

                ws.Cell("K" + i).Value = (item.LeavingFuel == null ? "" : item.LeavingFuel.Value.ToString("#.##"));
                ws.Cell("K" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("K" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("K" + i).Style.Alignment.WrapText = true;

                ws.Cell("L" + i).Value = (item.LeavingNote == null ? "" : item.LeavingNote);
                ws.Cell("L" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("L" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("L" + i).Style.Alignment.WrapText = true;
                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"OpenInitContList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Close
        [OperationFilter]
        public ActionResult ClosedInitContIndex(SearchModelInitCont searchModel, string searchModelString, int? page = 1)
        {
            //Employees-Leaving Mechaic
            //Employees1-Leaving Senior Mechaic
            //Employees2-Enter Mechaic
            //Employees3-Enter Senior Mechaic
            //Employees4-First Driver
            //Employees5-Second Driver

            //Codes for pagination
            string[] searchModelList=null;
            string test = "";
            bool searchModelStatus = false;
            foreach (var item in searchModel.GetType().GetProperties())
            {
                if (item.GetValue(searchModel, null) !=null)
                {
                    searchModelStatus = true;
                    break;
                }
            }

            if (searchModelString!=null && searchModelStatus == false)
            {
               searchModelList = searchModelString.Split('/');

                if (!string.IsNullOrEmpty(searchModelList[0]))
                {
                    searchModel.EnterDateEnd = searchModelList[0];
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModel.EnterDateStart = searchModelList[1];
                }
                if (!string.IsNullOrEmpty(searchModelList[2]))
                {
                    searchModel.EnterFuelMax = searchModelList[2];
                }
                if (!string.IsNullOrEmpty(searchModelList[3]))
                {
                    searchModel.EnterFuelMin = searchModelList[3];
                }
                if (!string.IsNullOrEmpty(searchModelList[4]))
                {
                    searchModel.EnterHourEnd = searchModelList[4];
                }
                if (!string.IsNullOrEmpty(searchModelList[5]))
                {
                    searchModel.EnterHourStart = searchModelList[5];
                }
                if (!string.IsNullOrEmpty(searchModelList[6]))
                {
                    searchModel.EnterKmMax = searchModelList[6];
                }
                if (!string.IsNullOrEmpty(searchModelList[7]))
                {
                    searchModel.EnterKmMin = searchModelList[7];
                }
                if (!string.IsNullOrEmpty(searchModelList[8]))
                {
                    searchModel.EnterRecMechId =Convert.ToInt32(searchModelList[8]);
                }
                if (!string.IsNullOrEmpty(searchModelList[9]))
                {
                    searchModel.EnterSeniorRecMechId = Convert.ToInt32(searchModelList[9]);
                }
                if (!string.IsNullOrEmpty(searchModelList[10]))
                {
                    searchModel.FinalFuelMax = searchModelList[10];
                }
                if (!string.IsNullOrEmpty(searchModelList[11]))
                {
                    searchModel.FinalFuelMin = searchModelList[11];
                }
                if (!string.IsNullOrEmpty(searchModelList[12]))
                {
                    searchModel.FinalHourEnd = searchModelList[12];
                }
                if (!string.IsNullOrEmpty(searchModelList[13]))
                {
                    searchModel.FinalHourStart = searchModelList[13];
                }
                if (!string.IsNullOrEmpty(searchModelList[14]))
                {
                    searchModel.FinalKmMax = searchModelList[14];
                }
                if (!string.IsNullOrEmpty(searchModelList[15]))
                {
                    searchModel.FinalKmMin = searchModelList[15];
                }
                if (!string.IsNullOrEmpty(searchModelList[16]))
                {
                    searchModel.FirstDriverId = Convert.ToInt32(searchModelList[16]);
                }
                if (!string.IsNullOrEmpty(searchModelList[17]))
                {
                    searchModel.LeaveDateEnd = searchModelList[17];
                }
                if (!string.IsNullOrEmpty(searchModelList[18]))
                {
                    searchModel.LeaveDateStart = searchModelList[18];
                }
                if (!string.IsNullOrEmpty(searchModelList[19]))
                {
                    searchModel.LeaveFuelMax = searchModelList[19];
                }
                if (!string.IsNullOrEmpty(searchModelList[20]))
                {
                    searchModel.LeaveFuelMin = searchModelList[20];
                }
                if (!string.IsNullOrEmpty(searchModelList[21]))
                {
                    searchModel.LeaveHourEnd = searchModelList[21];
                }
                if (!string.IsNullOrEmpty(searchModelList[22]))
                {
                    searchModel.LeaveHourStart = searchModelList[22];
                }
                if (!string.IsNullOrEmpty(searchModelList[23]))
                {
                    searchModel.LeaveKmMax = searchModelList[23];
                }
                if (!string.IsNullOrEmpty(searchModelList[24]))
                {
                    searchModel.LeaveKmMin = searchModelList[24];
                }
                if (!string.IsNullOrEmpty(searchModelList[25]))
                {
                    searchModel.LeavingRecMechId = Convert.ToInt32(searchModelList[25]);
                }
                if (!string.IsNullOrEmpty(searchModelList[26]))
                {
                    searchModel.LeavingSeniorRecMechId = Convert.ToInt32(searchModelList[26]);
                }
                if (!string.IsNullOrEmpty(searchModelList[27]))
                {
                    searchModel.RouteId = Convert.ToInt32(searchModelList[27]);
                }
                if (!string.IsNullOrEmpty(searchModelList[28]))
                {
                    searchModel.SecondDriverId = Convert.ToInt32(searchModelList[28]);
                }
                if (!string.IsNullOrEmpty(searchModelList[29]))
                {
                    searchModel.SOCARFuelMax = searchModelList[29];
                }
                if (!string.IsNullOrEmpty(searchModelList[30]))
                {
                    searchModel.SOCARFuelMin = searchModelList[30];
                }
                if (!string.IsNullOrEmpty(searchModelList[31]))
                {
                    searchModel.VehicleId = Convert.ToInt32(searchModelList[31]);
                }
            }            
            //End
            
            ViewBag.Ope = true;
            ViewHome model = new ViewHome()
            {
                Vehicles = db.Vehicles.ToList(),
                Employees = db.Employees.ToList(),
                Routes = db.Routes.ToList()
            };
            

            //Variables
            //Dates
            DateTime? leaveDateStart = null;
            DateTime? leaveDateEnd = null;
            DateTime? enterDateStart = null;
            DateTime? enterDateEnd = null;

            //Hours
            TimeSpan? leaveHourStart = null;
            TimeSpan? leaveHourEnd = null;
            TimeSpan? enterHourStart = null;
            TimeSpan? enterHourEnd = null;



            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.LeaveDateStart))
            {
                leaveDateStart = DateTime.ParseExact(searchModel.LeaveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveDateEnd))
            {
                leaveDateEnd = DateTime.ParseExact(searchModel.LeaveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.EnterDateStart))
            {
                enterDateStart = DateTime.ParseExact(searchModel.EnterDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.EnterDateEnd))
            {
                enterDateEnd = DateTime.ParseExact(searchModel.EnterDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Leaving Hour
            if (!string.IsNullOrEmpty(searchModel.LeaveHourStart))
            {
                leaveHourStart = TimeSpan.Parse(searchModel.LeaveHourStart);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveHourEnd))
            {
                leaveHourEnd = TimeSpan.Parse(searchModel.LeaveHourEnd);
            }

            //Enter Hour
            if (!string.IsNullOrEmpty(searchModel.EnterHourStart))
            {
                enterHourStart = TimeSpan.Parse(searchModel.EnterHourStart);
            }
            if (!string.IsNullOrEmpty(searchModel.EnterHourEnd))
            {
                enterHourEnd = TimeSpan.Parse(searchModel.EnterHourEnd);
            }



            //Leaving km
            //min
            decimal? leaveKmMin = null;
            decimal leaveKmMinTemp;
            string leaveKmMinTempDot;

            if (searchModel.LeaveKmMin != null && searchModel.LeaveKmMin.Contains("."))
            {
                leaveKmMinTempDot = searchModel.LeaveKmMin.Replace('.', ',');
            }
            else
            {
                leaveKmMinTempDot = searchModel.LeaveKmMin;
            }
            if (Decimal.TryParse(leaveKmMinTempDot, out leaveKmMinTemp))
            {
                leaveKmMin = leaveKmMinTemp;
            }

            //max
            decimal? leaveKmMax = null;
            decimal leaveKmMaxTemp;
            string leaveKmMaxTempDot;

            if (searchModel.LeaveKmMax != null && searchModel.LeaveKmMax.Contains("."))
            {
                leaveKmMaxTempDot = searchModel.LeaveKmMax.Replace('.', ',');
            }
            else
            {
                leaveKmMaxTempDot = searchModel.LeaveKmMax;
            }
            if (Decimal.TryParse(leaveKmMaxTempDot, out leaveKmMaxTemp))
            {
                leaveKmMax = leaveKmMaxTemp;
            }
            //End


            //Enter km
            //min
            decimal? enterKmMin = null;
            decimal enterKmMinTemp;
            string enterKmMinTempDot;

            if (searchModel.EnterKmMin != null && searchModel.EnterKmMin.Contains("."))
            {
                enterKmMinTempDot = searchModel.EnterKmMin.Replace('.', ',');
            }
            else
            {
                enterKmMinTempDot = searchModel.EnterKmMin;
            }
            if (Decimal.TryParse(enterKmMinTempDot, out enterKmMinTemp))
            {
                enterKmMin = enterKmMinTemp;
            }

            //max
            decimal? enterKmMax = null;
            decimal enterKmMaxTemp;
            string enterKmMaxTempDot;

            if (searchModel.EnterKmMax != null && searchModel.EnterKmMax.Contains("."))
            {
                enterKmMaxTempDot = searchModel.EnterKmMax.Replace('.', ',');
            }
            else
            {
                enterKmMaxTempDot = searchModel.EnterKmMax;
            }
            if (Decimal.TryParse(enterKmMaxTempDot, out enterKmMaxTemp))
            {
                enterKmMax = enterKmMaxTemp;
            }
            //End

            //Leaving fuel
            //min
            decimal? leaveFuelMin = null;
            decimal leaveFuelMinTemp;
            string leaveFuelMinTempDot;

            if (searchModel.LeaveFuelMin != null && searchModel.LeaveFuelMin.Contains("."))
            {
                leaveFuelMinTempDot = searchModel.LeaveFuelMin.Replace('.', ',');
            }
            else
            {
                leaveFuelMinTempDot = searchModel.LeaveFuelMin;
            }
            if (Decimal.TryParse(leaveFuelMinTempDot, out leaveFuelMinTemp))
            {
                leaveFuelMin = leaveFuelMinTemp;
            }

            //max
            decimal? leaveFuelMax = null;
            decimal leaveFuelMaxTemp;
            string leaveFuelMaxTempDot;

            if (searchModel.LeaveFuelMax != null && searchModel.LeaveFuelMax.Contains("."))
            {
                leaveFuelMaxTempDot = searchModel.LeaveFuelMax.Replace('.', ',');
            }
            else
            {
                leaveFuelMaxTempDot = searchModel.LeaveFuelMax;
            }
            if (Decimal.TryParse(leaveFuelMaxTempDot, out leaveFuelMaxTemp))
            {
                leaveFuelMax = leaveFuelMaxTemp;
            }
            //End

            //Enter fuel
            //min
            decimal? enterFuelMin = null;
            decimal enterFuelMinTemp;
            string enterFuelMinTempDot;

            if (searchModel.EnterFuelMin != null && searchModel.EnterFuelMin.Contains("."))
            {
                enterFuelMinTempDot = searchModel.EnterFuelMin.Replace('.', ',');
            }
            else
            {
                enterFuelMinTempDot = searchModel.EnterFuelMin;
            }
            if (Decimal.TryParse(enterFuelMinTempDot, out enterFuelMinTemp))
            {
                enterFuelMin = enterFuelMinTemp;
            }

            //max
            decimal? enterFuelMax = null;
            decimal enterFuelMaxTemp;
            string enterFuelMaxTempDot;

            if (searchModel.EnterFuelMax != null && searchModel.EnterFuelMax.Contains("."))
            {
                enterFuelMaxTempDot = searchModel.EnterFuelMax.Replace('.', ',');
            }
            else
            {
                enterFuelMaxTempDot = searchModel.EnterFuelMax;
            }
            if (Decimal.TryParse(enterFuelMaxTempDot, out enterFuelMaxTemp))
            {
                enterFuelMax = enterFuelMaxTemp;
            }
            //End


            //Final km
            //min
            decimal? finalKmMin = null;
            decimal finalKmMinTemp;
            string finalKmMinTempDot;

            if (searchModel.FinalKmMin != null && searchModel.FinalKmMin.Contains("."))
            {
                finalKmMinTempDot = searchModel.FinalKmMin.Replace('.', ',');
            }
            else
            {
                finalKmMinTempDot = searchModel.FinalKmMin;
            }
            if (Decimal.TryParse(finalKmMinTempDot, out finalKmMinTemp))
            {
                finalKmMin = finalKmMinTemp;
            }

            //max
            decimal? finalKmMax = null;
            decimal finalKmMaxTemp;
            string finalKmMaxTempDot;

            if (searchModel.FinalKmMax != null && searchModel.FinalKmMax.Contains("."))
            {
                finalKmMaxTempDot = searchModel.FinalKmMax.Replace('.', ',');
            }
            else
            {
                finalKmMaxTempDot = searchModel.FinalKmMax;
            }
            if (Decimal.TryParse(finalKmMaxTempDot, out finalKmMaxTemp))
            {
                finalKmMax = finalKmMaxTemp;
            }
            //End

            //Final fuel
            //min
            decimal? finalFuelMin = null;
            decimal finalFuelMinTemp;
            string finalFuelMinTempDot;

            if (searchModel.FinalFuelMin != null && searchModel.FinalFuelMin.Contains("."))
            {
                finalFuelMinTempDot = searchModel.FinalFuelMin.Replace('.', ',');
            }
            else
            {
                finalFuelMinTempDot = searchModel.FinalFuelMin;
            }
            if (Decimal.TryParse(finalFuelMinTempDot, out finalFuelMinTemp))
            {
                finalFuelMin = finalFuelMinTemp;
            }

            //max
            decimal? finalFuelMax = null;
            decimal finalFuelMaxTemp;
            string finalFuelMaxTempDot;

            if (searchModel.FinalFuelMax != null && searchModel.FinalFuelMax.Contains("."))
            {
                finalFuelMaxTempDot = searchModel.FinalFuelMax.Replace('.', ',');
            }
            else
            {
                finalFuelMaxTempDot = searchModel.FinalFuelMax;
            }
            if (Decimal.TryParse(finalFuelMaxTempDot, out finalFuelMaxTemp))
            {
                finalFuelMax = finalFuelMaxTemp;
            }
            //End


            //Final hour
            //min
            double? finalHourMin = null;
            double finalHourMinTemp;
            string finalHourMinTempDot;

            if (searchModel.FinalHourStart != null && searchModel.FinalHourStart.Contains("."))
            {
                finalHourMinTempDot = searchModel.FinalHourStart.Replace('.', ',');
            }
            else
            {
                finalHourMinTempDot = searchModel.FinalHourStart;
            }
            if (Double.TryParse(finalHourMinTempDot, out finalHourMinTemp))
            {
                finalHourMin = finalHourMinTemp;
            }

            //max
            double? finalHourMax = null;
            double finalHourMaxTemp;
            string finalHourMaxTempDot;

            if (searchModel.FinalHourEnd != null && searchModel.FinalHourEnd.Contains("."))
            {
                finalHourMaxTempDot = searchModel.FinalHourEnd.Replace('.', ',');
            }
            else
            {
                finalHourMaxTempDot = searchModel.FinalHourEnd;
            }
            if (Double.TryParse(finalHourMaxTempDot, out finalHourMaxTemp))
            {
                finalHourMax = finalHourMaxTemp;
            }
            //End

            List<InitialControlSchedule> init = db.InitialControlSchedule.Where(i => (i.IsOpen == false) &&
                                                                                (searchModel.VehicleId != null ? i.VehicleId == searchModel.VehicleId : true) &&
                                                                                (searchModel.RouteId != null ? i.RouteId == searchModel.RouteId : true) &&

                                                                                (searchModel.FirstDriverId != null ? i.FirstDriverId == searchModel.FirstDriverId : true) &&
                                                                                (searchModel.SecondDriverId != null ? i.SecondDriverId == searchModel.SecondDriverId : true) &&

                                                                                (searchModel.LeavingRecMechId != null ? i.LeavingRecMechId == searchModel.LeavingRecMechId : true) &&
                                                                                (searchModel.LeavingSeniorRecMechId != null ? i.LeavingSeniorRecMechId == searchModel.LeavingSeniorRecMechId : true) &&
                                                                                (searchModel.EnterRecMechId != null ? i.EnterRecMechId == searchModel.EnterRecMechId : true) &&
                                                                                (searchModel.EnterSeniorRecMechId != null ? i.EnterSeniorRecMechId == searchModel.EnterSeniorRecMechId : true) &&

                                                                                (searchModel.LeaveDateStart != null ? DbFunctions.TruncateTime(i.LeavingTime) >= leaveDateStart : true) &&
                                                                                (searchModel.LeaveDateEnd != null ? DbFunctions.TruncateTime(i.LeavingTime) <= leaveDateEnd : true) &&
                                                                                (searchModel.EnterDateStart != null ? DbFunctions.TruncateTime(i.EnterTime) >= enterDateStart : true) &&
                                                                                (searchModel.EnterDateEnd != null ? DbFunctions.TruncateTime(i.EnterTime) <= enterDateEnd : true) &&

                                                                                (searchModel.LeaveHourStart != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) >= leaveHourStart : true) &&
                                                                                (searchModel.LeaveHourEnd != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) <= leaveHourEnd : true) &&
                                                                                (searchModel.EnterHourStart != null ? DbFunctions.CreateTime(i.EnterTime.Value.Hour, i.EnterTime.Value.Minute, 0) >= enterHourStart : true) &&
                                                                                (searchModel.EnterHourEnd != null ? DbFunctions.CreateTime(i.EnterTime.Value.Hour, i.EnterTime.Value.Minute, 0) <= enterHourEnd : true) &&

                                                                                (searchModel.LeaveKmMin != null ? i.LeavingKilometer >= leaveKmMin : true) &&
                                                                                (searchModel.LeaveKmMax != null ? i.LeavingKilometer <= leaveKmMax : true) &&
                                                                                (searchModel.EnterKmMin != null ? i.EnterKilometer >= enterKmMin : true) &&
                                                                                (searchModel.EnterKmMax != null ? i.EnterKilometer <= enterKmMax : true) &&

                                                                                (searchModel.LeaveFuelMin != null ? i.LeavingFuel >= leaveFuelMin : true) &&
                                                                                (searchModel.LeaveFuelMax != null ? i.LeavingFuel <= leaveFuelMax : true) &&
                                                                                (searchModel.EnterFuelMin != null ? i.EnterFuel >= enterFuelMin : true) &&
                                                                                (searchModel.EnterFuelMax != null ? i.EnterFuel <= enterFuelMax : true) &&

                                                                                (searchModel.FinalKmMin != null ? (i.EnterKilometer - i.LeavingKilometer) >= finalKmMin : true) &&
                                                                                (searchModel.FinalKmMax != null ? (i.EnterKilometer - i.LeavingKilometer) <= finalKmMax : true) &&

                                                                                (searchModel.FinalFuelMin != null ? (i.LeavingFuel - i.EnterFuel) >= finalFuelMin : true) &&
                                                                                (searchModel.FinalFuelMax != null ? (i.LeavingFuel - i.EnterFuel) <= finalFuelMax : true) &&

                                                                                (searchModel.FinalHourStart != null ? DbFunctions.DiffHours(i.LeavingTime, i.EnterTime) >= finalHourMin : true) &&
                                                                                (searchModel.FinalHourEnd != null ? DbFunctions.DiffHours(i.LeavingTime, i.EnterTime) <= finalHourMax : true)
                                                                                ).OrderByDescending(i => i.EnterTime).ToList();

            //Pagination modul
            if (page == null)
            {
                page = 1;
            }

            int limit = 10;
            if (limit > init.Count)
            {
                limit = init.Count;
            }

            ViewBag.total = (int)Math.Ceiling(((init.Count()>0? init.Count():1) / (decimal)(limit>0? limit :1)));
            ViewBag.page = page;

            model.InitialControlSchedule = init.Skip(((int)page - 1) * limit).Take(limit);
            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul

            TempData["SortedInitContClose"] = model;
            model.SearchModelInitCont = searchModel;

            return View(model);
        }

        [OperationFilter]
        public ActionResult InitContClose(int id)
        {
            ViewBag.Ope = true;
            InitialControlSchedule initCon = new InitialControlSchedule();
            if (id != null && db.InitialControlSchedule.FirstOrDefault(i => i.Id == id) != null)
            {
                initCon = db.InitialControlSchedule.FirstOrDefault(i => i.Id == id);
            }

            ViewHome model = new ViewHome()
            {
                Employees = db.Employees.ToList()
            };

            ViewBag.InitCon = initCon;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult InitContClose(InitControl initCon)
        {
            //return Content(initCon.CheckUp.ToString());
            ViewBag.Ope = true;
            InitialControlSchedule InitCon = db.InitialControlSchedule.Find(initCon.Id);
            TempData["InitContCls"] = initCon;
            ViewBag.InitCon = InitCon;

            ViewHome model = new ViewHome()
            {
                Employees = db.Employees.ToList()
            };


            if (ModelState.IsValid)
            {
                //Check for inputs
                if (initCon.SecondDriverId == 0 || string.IsNullOrEmpty(initCon.FirstDrvFuel) || string.IsNullOrEmpty(initCon.FirstDrvKm) || string.IsNullOrEmpty(initCon.EnterFuel) || string.IsNullOrEmpty(initCon.EnterKilometer))
                {
                    Session["emptyFildClose"] = true;
                    return View(model);
                }



                //Mechanic and Senior mechanic
                int UserId = (int)Session["UserId"];
                int? MechId2 = null;
                int? SeniorMechId2 = null;
                if (db.Users.FirstOrDefault(u => u.Id == UserId) != null)
                {
                    MechId2 = db.Users.FirstOrDefault(u => u.Id == UserId).EmployeeId;
                    InitCon.EnterRecMechId = MechId2;
                }

                if (db.MechanicBinding.FirstOrDefault(b => b.RecMechId == MechId2) != null)
                {
                    SeniorMechId2 = db.MechanicBinding.FirstOrDefault(b => b.RecMechId == MechId2).SeniorRecMechId;
                    InitCon.EnterSeniorRecMechId = SeniorMechId2;
                }
                if (MechId2 == null || SeniorMechId2 == null)
                {
                    Session["DataDeficiencyClose"] = true;
                    return RedirectToAction("InitContClose");
                }

                //Enter km
                decimal? kmDot = null;
                decimal kmTemp;
                string kmTempDot;

                if (initCon.EnterKilometer != null && initCon.EnterKilometer.Contains("."))
                {
                    kmTempDot = initCon.EnterKilometer.Replace('.', ',');
                }
                else if (initCon.EnterKilometer != null)
                {
                    kmTempDot = initCon.EnterKilometer;
                }
                else
                {
                    kmTempDot = null;
                }

                if (kmTempDot != null && decimal.TryParse(kmTempDot, out kmTemp))
                {
                    kmDot = decimal.Parse(kmTempDot);
                }
                InitCon.EnterKilometer = kmDot;
                //End

                //Check for wrong km
                decimal? routeMaxKm = db.RouteDailyKM.FirstOrDefault(k => k.RouteId == InitCon.RouteId).KmLimit;
                TempData["LimitKm"] = routeMaxKm;
                if ((kmDot - InitCon.LeavingKilometer) > routeMaxKm)
                {
                    Session["overLimitKmClose"] = true;
                    return RedirectToAction("InitContClose");
                }

                if (InitCon.LeavingKilometer > kmDot)
                {
                    Session["overEnterKmClose"] = true;
                    return RedirectToAction("InitContClose");
                }

                //First driver km
                decimal? FirstDrvkmDot = null;
                decimal FirstDrvkmTemp;
                string FirstDrvkmTempDot;

                if (initCon.FirstDrvKm != null && initCon.FirstDrvKm.Contains("."))
                {
                    FirstDrvkmTempDot = initCon.FirstDrvKm.Replace('.', ',');
                }
                else if (initCon.FirstDrvKm != null)
                {
                    FirstDrvkmTempDot = initCon.FirstDrvKm;
                }
                else
                {
                    FirstDrvkmTempDot = null;
                }

                if (FirstDrvkmTempDot != null && decimal.TryParse(FirstDrvkmTempDot, out FirstDrvkmTemp))
                {
                    FirstDrvkmDot = decimal.Parse(FirstDrvkmTempDot);
                }
                InitCon.FirstDrvKm = FirstDrvkmDot;
                //End

                //Check for wrong km
                decimal? FstDrvrouteMaxKm = db.RouteDailyKM.FirstOrDefault(k => k.RouteId == InitCon.RouteId).KmLimit;
                TempData["FstDrvoverLimitKmClose"] = FstDrvrouteMaxKm;
                if ((FirstDrvkmDot - InitCon.LeavingKilometer) > FstDrvrouteMaxKm)
                {
                    Session["FstDrvoverLimitKmClose"] = true;
                    return RedirectToAction("InitContClose");
                }

                if (InitCon.LeavingKilometer > FirstDrvkmDot)
                {
                    Session["FstDrvoverEnterKmClose"] = true;
                    return RedirectToAction("InitContClose");
                }


                //Enter fuel
                decimal? fuelDot = null;
                decimal fuelTemp;
                string fuelTempDot;

                if (initCon.EnterFuel != null && initCon.EnterFuel.Contains("."))
                {
                    fuelTempDot = initCon.EnterFuel.Replace('.', ',');
                }
                else if (initCon.EnterFuel != null)
                {
                    fuelTempDot = initCon.EnterFuel;
                }
                else
                {
                    fuelTempDot = null;
                }

                if (fuelTempDot != null && decimal.TryParse(fuelTempDot, out fuelTemp))
                {
                    fuelDot = decimal.Parse(fuelTempDot);
                }
                InitCon.EnterFuel = fuelDot;
                //End

                //Check for wrong fuel
                if (InitCon.LeavingFuel < fuelDot)
                {
                    Session["overLeaveFuel"] = true;
                    TempData["overLeaveFuel"] = fuelDot;
                    return RedirectToAction("InitContClose");
                }


                //First driver fuel
                decimal? FstDrvFuelDot = null;
                decimal FstDrvFuelTemp;
                string FstDrvFuelTempDot;

                if (initCon.FirstDrvFuel != null && initCon.FirstDrvFuel.Contains("."))
                {
                    FstDrvFuelTempDot = initCon.FirstDrvFuel.Replace('.', ',');
                }
                else if (initCon.FirstDrvFuel != null)
                {
                    FstDrvFuelTempDot = initCon.FirstDrvFuel;
                }
                else
                {
                    FstDrvFuelTempDot = null;
                }

                if (FstDrvFuelTempDot != null && decimal.TryParse(FstDrvFuelTempDot, out FstDrvFuelTemp))
                {
                    FstDrvFuelDot = decimal.Parse(FstDrvFuelTempDot);
                }
                InitCon.FirstDrvFuel = FstDrvFuelDot;
                //End

                //Check for wrong fuel
                if (InitCon.LeavingFuel < FstDrvFuelDot)
                {
                    Session["FstDrvOverLeaveFuel"] = true;
                    TempData["FstDrvOverLeaveFuel"] = FstDrvFuelDot;
                    return RedirectToAction("InitContClose");
                }

                //Time
                InitCon.EnterTime = DateTime.Now;

                //Second driver
                if (initCon.SecondDriverId == 0)
                {
                    InitCon.SecondDriverId = null;
                }
                else
                {
                    InitCon.SecondDriverId = initCon.SecondDriverId;
                }

                //Note
                InitCon.EnterNote = initCon.EnterNote;

                //Close init
                InitCon.IsOpen = false;

                db.Entry(InitCon).State = EntityState.Modified;

                //Disable updating
                db.Entry(InitCon).Property(x => x.VehicleId).IsModified = false;
                db.Entry(InitCon).Property(x => x.LeavingFuel).IsModified = false;
                db.Entry(InitCon).Property(x => x.LeavingKilometer).IsModified = false;
                db.Entry(InitCon).Property(x => x.LeavingRecMechId).IsModified = false;
                db.Entry(InitCon).Property(x => x.LeavingSeniorRecMechId).IsModified = false;
                db.Entry(InitCon).Property(x => x.LeavingTime).IsModified = false;
                db.Entry(InitCon).Property(x => x.RouteId).IsModified = false;
                db.Entry(InitCon).Property(x => x.FirstDriverId).IsModified = false;
                db.Entry(InitCon).Property(x => x.LeavingNote).IsModified = false;

                db.SaveChanges();

                TempData["InitContCls"] = null;
            }

            if (initCon.CheckUp == true)
            {
                return RedirectToAction("InitContCheckupCreate", "MainOperations", new { id = InitCon.Id });
            }
            return RedirectToAction("OpenInitContIndex");
        }

        [OperationFilter]
        public ActionResult ClosedInitContUpdate(int id)
        {
            ViewBag.Ope = true;
            if (db.JobCards.FirstOrDefault(j => j.CheckUpCard.InitContId == id)!=null && db.JobCards.FirstOrDefault(j=>j.CheckUpCard.InitContId==id).IsOpen==true)
            {
                Session["jobcardIsOpen"] = true;
                return RedirectToAction("ClosedInitContIndex");
            }

            ViewHome model = new ViewHome
            {
                Employees = db.Employees.ToList(),
                Routes = db.Routes.ToList(),
                Vehicles = db.Vehicles.ToList()
            };

            ViewBag.InitCont = db.InitialControlSchedule.Find(id);
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult ClosedInitContUpdate(InitControl initCont)
        {
            ViewBag.Ope = true;
            InitialControlSchedule InitCon = db.InitialControlSchedule.Find(initCont.Id);

            int UserId = (int)Session["UserId"];
            //Updated user
            InitCon.AllUpdateUserId = UserId;

            //Updated date
            InitCon.AllUpdateDate = DateTime.Now;

            //Vehicle
            if (initCont.VehicleId == 0)
            {
                InitCon.VehicleId = null;
            }
            else
            {
                InitCon.VehicleId = initCont.VehicleId;
            }

            //Route
            if (initCont.RouteId == 0)
            {
                InitCon.RouteId = null;
            }
            else
            {
                InitCon.RouteId = initCont.RouteId;
            }

            //First driver
            if (initCont.FirstDriverId == 0)
            {
                InitCon.FirstDriverId = null;
            }
            else
            {
                InitCon.FirstDriverId = initCont.FirstDriverId;
            }

            //Second driver
            if (initCont.SecondDriverId == 0)
            {
                InitCon.SecondDriverId = null;
            }
            else
            {
                InitCon.SecondDriverId = initCont.SecondDriverId;
            }

            //Leave Mech
            if (initCont.LeavingRecMechId == 0)
            {
                InitCon.LeavingRecMechId = null;
            }
            else
            {
                InitCon.LeavingRecMechId = initCont.LeavingRecMechId;
            }

            //Leave Senior Mech
            if (initCont.LeavingSeniorRecMechId == 0)
            {
                InitCon.LeavingSeniorRecMechId = null;
            }
            else
            {
                InitCon.LeavingSeniorRecMechId = initCont.LeavingSeniorRecMechId;
            }


            //Enter Mech
            if (initCont.EnterRecMechId == 0)
            {
                InitCon.EnterRecMechId = null;
            }
            else
            {
                InitCon.EnterRecMechId = initCont.EnterRecMechId;
            }

            //Enter Senior Mech
            if (initCont.EnterSeniorRecMechId == 0)
            {
                InitCon.EnterSeniorRecMechId = null;
            }
            else
            {
                InitCon.EnterSeniorRecMechId = initCont.EnterSeniorRecMechId;
            }

            //Leave note
            InitCon.LeavingNote = initCont.LeavingNote;

            //Enter note
            InitCon.EnterNote = initCont.EnterNote;


            //Leave km
            decimal? LeavekmDot = null;
            decimal LeavekmTemp;
            string LeavekmTempDot;

            if (initCont.LeavingKilometer != null && initCont.LeavingKilometer.Contains("."))
            {
                LeavekmTempDot = initCont.LeavingKilometer.Replace('.', ',');
            }
            else if (initCont.LeavingKilometer != null)
            {
                LeavekmTempDot = initCont.LeavingKilometer;
            }
            else
            {
                LeavekmTempDot = null;
            }

            if (LeavekmTempDot != null && decimal.TryParse(LeavekmTempDot, out LeavekmTemp))
            {
                LeavekmDot = decimal.Parse(LeavekmTempDot);
            }
            InitCon.LeavingKilometer = LeavekmDot;
            //End

            //Leave fuel
            decimal? LeavefuelDot = null;
            decimal LeavefuelTemp;
            string LeavefuelTempDot;

            if (initCont.LeavingFuel != null && initCont.LeavingFuel.Contains("."))
            {
                LeavefuelTempDot = initCont.LeavingFuel.Replace('.', ',');
            }
            else if (initCont.LeavingFuel != null)
            {
                LeavefuelTempDot = initCont.LeavingFuel;
            }
            else
            {
                LeavefuelTempDot = null;
            }

            if (LeavefuelTempDot != null && decimal.TryParse(LeavefuelTempDot, out LeavefuelTemp))
            {
                LeavefuelDot = decimal.Parse(LeavefuelTempDot);
            }
            InitCon.LeavingFuel = LeavefuelDot;
            //End


            //Enter km
            decimal? kmDot = null;
            decimal kmTemp;
            string kmTempDot;

            if (initCont.EnterKilometer != null && initCont.EnterKilometer.Contains("."))
            {
                kmTempDot = initCont.EnterKilometer.Replace('.', ',');
            }
            else if (initCont.EnterKilometer != null)
            {
                kmTempDot = initCont.EnterKilometer;
            }
            else
            {
                kmTempDot = null;
            }

            if (kmTempDot != null && decimal.TryParse(kmTempDot, out kmTemp))
            {
                kmDot = decimal.Parse(kmTempDot);
            }
            InitCon.EnterKilometer = kmDot;
            //End

            //Enter fuel
            decimal? fuelDot = null;
            decimal fuelTemp;
            string fuelTempDot;

            if (initCont.EnterFuel != null && initCont.EnterFuel.Contains("."))
            {
                fuelTempDot = initCont.EnterFuel.Replace('.', ',');
            }
            else if (initCont.EnterFuel != null)
            {
                fuelTempDot = initCont.EnterFuel;
            }
            else
            {
                fuelTempDot = null;
            }

            if (fuelTempDot != null && decimal.TryParse(fuelTempDot, out fuelTemp))
            {
                fuelDot = decimal.Parse(fuelTempDot);
            }
            InitCon.EnterFuel = fuelDot;
            //End



            db.Entry(InitCon).State = EntityState.Modified;

            //Disable updating
            db.Entry(InitCon).Property(x => x.IsOpen).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingTime).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingUpdateEmpId).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingUpdateDate).IsModified = false;

            db.Entry(InitCon).Property(x => x.EnterUpdateEmpId).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterUpdateDate).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterTime).IsModified = false;

            db.SaveChanges();
            return RedirectToAction("ClosedInitContIndex");
        }


        [OperationFilter]
        public ActionResult InitContCloseDelete(int id)
        {
            InitialControlSchedule InitCon = db.InitialControlSchedule.Find(id);
            FuelSOCAR fuelSocar = db.FuelSOCAR.FirstOrDefault(s => s.InitContId == id);
            CheckUpCard Checkup = db.CheckUpCard.FirstOrDefault(c => c.InitContId == id);
            MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.FirstOrDefault(m => m.InitContId == id);
            if (InitCon == null || fuelSocar != null || Checkup != null || maintenanceHistory != null)
            {
                return RedirectToAction("ClosedInitContIndex");
            }

            db.InitialControlSchedule.Remove(InitCon);
            db.SaveChanges();
            return RedirectToAction("ClosedInitContIndex");
        }

        public void ExportCloseInitCont()
        {
            ViewHome model = new ViewHome();
            model = (ViewHome)TempData["SortedInitContClose"];

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Closed Initial Control list");

            ws.Range("B2:X2").Merge();
            ws.Cell("B2").Value = "Bağlı yoxlama cədvəlləri Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "NV Nömrəsi";
            ws.Cell("D3").Value = "TQ Nəzarət";
            ws.Cell("E3").Value = "Xətt";
            ws.Cell("F3").Value = "Sürücü-1";
            ws.Cell("G3").Value = "Sürücü-2";
            ws.Cell("H3").Value = "Çıxış tarixi";
            ws.Cell("I3").Value = "Çıxış mexanik";
            ws.Cell("J3").Value = "Çıxış Baş mexanik";
            ws.Cell("K3").Value = "Çıxış km";
            ws.Cell("L3").Value = "Çıxış yanacaq";
            ws.Cell("M3").Value = "Yanacaq SOCAR";
            ws.Cell("N3").Value = "Giriş mexanik";
            ws.Cell("O3").Value = "Giriş Baş mexanik";
            ws.Cell("P3").Value = "Giriş tarixi";
            ws.Cell("Q3").Value = "Giriş km";
            ws.Cell("R3").Value = "Giriş yanacaq";
            ws.Cell("S3").Value = "Yekun KM";
            ws.Cell("T3").Value = "Yekun yanacaq";
            ws.Cell("U3").Value = "Yekun saat";
            ws.Cell("V3").Value = "Xəttin uzunluğu";
            ws.Cell("W3").Value = "Qalıq Yanacaq SOCAR";
            ws.Cell("X3").Value = "Faktiki Yanacaq SOCAR";

            ws.Cell("B2").Style.Font.SetBold();
            ws.Cell("B2").Style.Font.FontSize = 14;
            ws.Cell("B2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("B2").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("B2").Style.Alignment.WrapText = true;


            ws.Cell("B3").Style.Font.SetBold();
            ws.Cell("B3").Style.Font.FontColor = XLColor.White;
            ws.Cell("B3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("B3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("B3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("B3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Alignment.WrapText = true;

            ws.Cell("C3").Style.Font.SetBold();
            ws.Cell("C3").Style.Font.FontColor = XLColor.White;
            ws.Cell("C3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("C3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("C3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("C3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Alignment.WrapText = true;

            ws.Cell("D3").Style.Font.SetBold();
            ws.Cell("D3").Style.Font.FontColor = XLColor.White;
            ws.Cell("D3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("D3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("D3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("D3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Alignment.WrapText = true;

            ws.Cell("E3").Style.Font.SetBold();
            ws.Cell("E3").Style.Font.FontColor = XLColor.White;
            ws.Cell("E3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("E3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("E3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("E3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Alignment.WrapText = true;

            ws.Cell("F3").Style.Font.SetBold();
            ws.Cell("F3").Style.Font.FontColor = XLColor.White;
            ws.Cell("F3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("F3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("F3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("F3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Alignment.WrapText = true;

            ws.Cell("G3").Style.Font.SetBold();
            ws.Cell("G3").Style.Font.FontColor = XLColor.White;
            ws.Cell("G3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("G3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("G3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("G3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Alignment.WrapText = true;

            ws.Cell("H3").Style.Font.SetBold();
            ws.Cell("H3").Style.Font.FontColor = XLColor.White;
            ws.Cell("H3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("H3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("H3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("H3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Alignment.WrapText = true;

            ws.Cell("I3").Style.Font.SetBold();
            ws.Cell("I3").Style.Font.FontColor = XLColor.White;
            ws.Cell("I3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("I3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("I3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("I3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Alignment.WrapText = true;

            ws.Cell("J3").Style.Font.SetBold();
            ws.Cell("J3").Style.Font.FontColor = XLColor.White;
            ws.Cell("J3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("J3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("J3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("J3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Alignment.WrapText = true;

            ws.Cell("K3").Style.Font.SetBold();
            ws.Cell("K3").Style.Font.FontColor = XLColor.White;
            ws.Cell("K3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("K3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("K3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("K3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Alignment.WrapText = true;

            ws.Cell("L3").Style.Font.SetBold();
            ws.Cell("L3").Style.Font.FontColor = XLColor.White;
            ws.Cell("L3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("L3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("L3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("L3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Alignment.WrapText = true;

            ws.Cell("M3").Style.Font.SetBold();
            ws.Cell("M3").Style.Font.FontColor = XLColor.White;
            ws.Cell("M3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("M3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("M3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("M3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Alignment.WrapText = true;

            ws.Cell("N3").Style.Font.SetBold();
            ws.Cell("N3").Style.Font.FontColor = XLColor.White;
            ws.Cell("N3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("N3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("N3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("N3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("N3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("N3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("N3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("N3").Style.Alignment.WrapText = true;

            ws.Cell("O3").Style.Font.SetBold();
            ws.Cell("O3").Style.Font.FontColor = XLColor.White;
            ws.Cell("O3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("O3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("O3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("O3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("O3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("O3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("O3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("O3").Style.Alignment.WrapText = true;

            ws.Cell("P3").Style.Font.SetBold();
            ws.Cell("P3").Style.Font.FontColor = XLColor.White;
            ws.Cell("P3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("P3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("P3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("P3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("P3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("P3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("P3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("P3").Style.Alignment.WrapText = true;

            ws.Cell("Q3").Style.Font.SetBold();
            ws.Cell("Q3").Style.Font.FontColor = XLColor.White;
            ws.Cell("Q3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("Q3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("Q3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("Q3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("Q3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("Q3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("Q3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("Q3").Style.Alignment.WrapText = true;

            ws.Cell("R3").Style.Font.SetBold();
            ws.Cell("R3").Style.Font.FontColor = XLColor.White;
            ws.Cell("R3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("R3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("R3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("R3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("R3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("R3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("R3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("R3").Style.Alignment.WrapText = true;

            ws.Cell("S3").Style.Font.SetBold();
            ws.Cell("S3").Style.Font.FontColor = XLColor.White;
            ws.Cell("S3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("S3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("S3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("S3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("S3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("S3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("S3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("S3").Style.Alignment.WrapText = true;

            ws.Cell("T3").Style.Font.SetBold();
            ws.Cell("T3").Style.Font.FontColor = XLColor.White;
            ws.Cell("T3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("T3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("T3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("T3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("T3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("T3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("T3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("T3").Style.Alignment.WrapText = true;

            ws.Cell("U3").Style.Font.SetBold();
            ws.Cell("U3").Style.Font.FontColor = XLColor.White;
            ws.Cell("U3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("U3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("U3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("U3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("U3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("U3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("U3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("U3").Style.Alignment.WrapText = true;

            ws.Cell("V3").Style.Font.SetBold();
            ws.Cell("V3").Style.Font.FontColor = XLColor.White;
            ws.Cell("V3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("V3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("V3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("V3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("V3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("V3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("V3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("V3").Style.Alignment.WrapText = true;

            ws.Cell("W3").Style.Font.SetBold();
            ws.Cell("W3").Style.Font.FontColor = XLColor.White;
            ws.Cell("W3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("W3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("W3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("W3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("W3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("W3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("W3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("W3").Style.Alignment.WrapText = true;

            ws.Cell("X3").Style.Font.SetBold();
            ws.Cell("X3").Style.Font.FontColor = XLColor.White;
            ws.Cell("X3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("X3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("X3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("X3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("X3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("X3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("X3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("X3").Style.Alignment.WrapText = true;


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 11;
            ws.Column("D").Width = 9;
            ws.Column("E").Width = 6;
            ws.Column("F").Width = 25;
            ws.Column("G").Width = 25;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 25;
            ws.Column("J").Width = 25;
            ws.Column("K").Width = 10;
            ws.Column("L").Width = 9;
            ws.Column("M").Width = 9;
            ws.Column("N").Width = 25;
            ws.Column("O").Width = 25;
            ws.Column("P").Width = 15;
            ws.Column("Q").Width = 10;
            ws.Column("R").Width = 9;
            ws.Column("S").Width = 9;
            ws.Column("T").Width = 9;
            ws.Column("U").Width = 7;
            ws.Column("V").Width = 15;
            ws.Column("W").Width = 15;
            ws.Column("X").Width = 15;


            int i = 4;
            foreach (var item in model.InitialControlSchedule)
            {
                //S/s
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                //NV Nömrəsi
                ws.Cell("C" + i).Value = (item.VehicleId == null ? "" : item.Vehicles.Number);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;

                //TQ Nəzarət
                ws.Cell("D" + i).Value = "?";
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                //Xətt
                ws.Cell("E" + i).Value = (item.RouteId == null ? "" : item.Routes.Number);
                ws.Cell("E" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                //Sürücü-1 
                ws.Cell("F" + i).Value = (item.FirstDriverId == null ? "" : item.Employees4.Surname + " " + item.Employees4.Name + " " + item.Employees4.FutherName);
                ws.Cell("F" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("F" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("F" + i).Style.Alignment.WrapText = true;

                //Sürücü-2
                ws.Cell("G" + i).Value = (item.SecondDriverId == null ? "" : item.Employees5.Surname + " " + item.Employees5.Name + " " + item.Employees5.FutherName);
                ws.Cell("G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("G" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("G" + i).Style.Alignment.WrapText = true;

                //Çıxış tarixi
                ws.Cell("H" + i).Value = (item.LeavingTime == null ? "" : item.LeavingTime.Value.ToString("dd.MM.yyyy HH:mm"));
                ws.Cell("H" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("H" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("H" + i).Style.Alignment.WrapText = true;

                //Çıxış mexanik
                ws.Cell("I" + i).Value = (item.LeavingRecMechId == null ? "" : item.Employees.Surname + " " + item.Employees.Name + " " + item.Employees.FutherName);
                ws.Cell("I" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("I" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("I" + i).Style.Alignment.WrapText = true;

                //Çıxış baş mexanik
                ws.Cell("J" + i).Value = (item.LeavingSeniorRecMechId == null ? "" : item.Employees1.Surname + " " + item.Employees1.Name + " " + item.Employees1.FutherName);
                ws.Cell("J" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("J" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("J" + i).Style.Alignment.WrapText = true;

                //Çıxış KM
                ws.Cell("K" + i).Value = (item.LeavingKilometer == null ? "" : item.LeavingKilometer.Value.ToString("#.##"));
                ws.Cell("K" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("K" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("K" + i).Style.Alignment.WrapText = true;

                //Çıxış yanacaq
                ws.Cell("L" + i).Value = (item.LeavingFuel == null ? "" : item.LeavingFuel.Value.ToString("#.##"));
                ws.Cell("L" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("L" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("L" + i).Style.Alignment.WrapText = true;

                //SOCAR yanacaq
                ws.Cell("M" + i).Value = ((item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) == null || item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount1 == null) ? "" : item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount1.Value.ToString("#.##"));
                //ws.Cell("M" + i).Value = "";
                ws.Cell("M" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("M" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("M" + i).Style.Alignment.WrapText = true;

                //Giriş mexanik
                ws.Cell("N" + i).Value = (item.EnterRecMechId == null ? "" : item.Employees2.Surname + " " + item.Employees2.Name + " " + item.Employees2.FutherName);
                ws.Cell("N" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("N" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("N" + i).Style.Alignment.WrapText = true;

                //Giriş baş mexanik
                ws.Cell("O" + i).Value = (item.EnterSeniorRecMechId == null ? "" : item.Employees3.Surname + " " + item.Employees3.Name + " " + item.Employees3.FutherName);
                ws.Cell("O" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("O" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("O" + i).Style.Alignment.WrapText = true;

                //Giriş tarixi
                ws.Cell("P" + i).Value = (item.EnterTime == null ? "" : item.EnterTime.Value.ToString("dd.MM.yyyy HH:mm"));
                ws.Cell("P" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("P" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("P" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("P" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("P" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("P" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("P" + i).Style.Alignment.WrapText = true;

                //Giriş km
                ws.Cell("Q" + i).Value = (item.EnterKilometer == null ? "" : item.EnterKilometer.Value.ToString("#.##"));
                ws.Cell("Q" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("Q" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("Q" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("Q" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("Q" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("Q" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("Q" + i).Style.Alignment.WrapText = true;

                //Giriş yanacaq
                ws.Cell("R" + i).Value = (item.EnterFuel == null ? "" : item.EnterFuel.Value.ToString("#.##"));
                ws.Cell("R" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("R" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("R" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("R" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("R" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("R" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("R" + i).Style.Alignment.WrapText = true;

                //Yekun km
                ws.Cell("S" + i).Value = ((item.LeavingKilometer != null && item.EnterKilometer != null) ? (item.EnterKilometer - item.LeavingKilometer).Value.ToString("#.##") : "");
                ws.Cell("S" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("S" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("S" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("S" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("S" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("S" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("S" + i).Style.Alignment.WrapText = true;

                //Yekun yanacaq
                ws.Cell("T" + i).Value = ((item.LeavingFuel != null && item.EnterFuel != null) ? (item.LeavingFuel - item.EnterFuel).Value.ToString("#.##") : "");
                ws.Cell("T" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("T" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("T" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("T" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("T" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("T" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("T" + i).Style.Alignment.WrapText = true;

                //Yekun saat
                ws.Cell("U" + i).Value = ((item.LeavingTime != null && item.EnterTime != null) ? Math.Round((item.EnterTime - item.LeavingTime).Value.TotalHours, 1) : Convert.ToInt32(null));
                ws.Cell("U" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("U" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("U" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("U" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("U" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("U" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("U" + i).Style.Alignment.WrapText = true;

                //Xəttin uzunluğu
                ws.Cell("V" + i).Value = (item.EnterKilometer != null && item.LeavingKilometer != null) ? (item.EnterKilometer - item.LeavingKilometer).Value.ToString("#.##") : "";
                ws.Cell("V" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("V" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("V" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("V" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("V" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("V" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("V" + i).Style.Alignment.WrapText = true;

                //Qalıq yanacaq SOCAR
                ws.Cell("W" + i).Value = "";
                ws.Cell("W" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("W" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("W" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("W" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("W" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("W" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("W" + i).Style.Alignment.WrapText = true;

                //Faktiki yanacaq SOCAR
                ws.Cell("X" + i).Value = "";
                ws.Cell("X" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("X" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("X" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("X" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("X" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("X" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("X" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"ClosedInitContList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }




        //Close for Mechanics
        [OperationFilter]
        public ActionResult ClosedInitContMechIndex(SearchModelInitCont searchModel)
        {
            ViewBag.Ope = true;
            ViewHome model = new ViewHome()
            {
                Vehicles = db.Vehicles.ToList(),
                Routes = db.Routes.ToList()
            };


            //Variables
            //Dates
            DateTime? leaveDateStart = null;
            DateTime? leaveDateEnd = null;
            DateTime? enterDateStart = null;
            DateTime? enterDateEnd = null;

            //Hours
            TimeSpan? leaveHourStart = null;
            TimeSpan? leaveHourEnd = null;
            TimeSpan? enterHourStart = null;
            TimeSpan? enterHourEnd = null;



            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.LeaveDateStart))
            {
                leaveDateStart = DateTime.ParseExact(searchModel.LeaveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveDateEnd))
            {
                leaveDateEnd = DateTime.ParseExact(searchModel.LeaveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.EnterDateStart))
            {
                enterDateStart = DateTime.ParseExact(searchModel.EnterDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.EnterDateEnd))
            {
                enterDateEnd = DateTime.ParseExact(searchModel.EnterDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Leaving Hour
            if (!string.IsNullOrEmpty(searchModel.LeaveHourStart))
            {
                leaveHourStart = TimeSpan.Parse(searchModel.LeaveHourStart);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveHourEnd))
            {
                leaveHourEnd = TimeSpan.Parse(searchModel.LeaveHourEnd);
            }

            //Enter Hour
            if (!string.IsNullOrEmpty(searchModel.EnterHourStart))
            {
                enterHourStart = TimeSpan.Parse(searchModel.EnterHourStart);
            }
            if (!string.IsNullOrEmpty(searchModel.EnterHourEnd))
            {
                enterHourEnd = TimeSpan.Parse(searchModel.EnterHourEnd);
            }



            //Leaving km
            //min
            decimal? leaveKmMin = null;
            decimal leaveKmMinTemp;
            string leaveKmMinTempDot;

            if (searchModel.LeaveKmMin != null && searchModel.LeaveKmMin.Contains("."))
            {
                leaveKmMinTempDot = searchModel.LeaveKmMin.Replace('.', ',');
            }
            else
            {
                leaveKmMinTempDot = searchModel.LeaveKmMin;
            }
            if (Decimal.TryParse(leaveKmMinTempDot, out leaveKmMinTemp))
            {
                leaveKmMin = leaveKmMinTemp;
            }

            //max
            decimal? leaveKmMax = null;
            decimal leaveKmMaxTemp;
            string leaveKmMaxTempDot;

            if (searchModel.LeaveKmMax != null && searchModel.LeaveKmMax.Contains("."))
            {
                leaveKmMaxTempDot = searchModel.LeaveKmMax.Replace('.', ',');
            }
            else
            {
                leaveKmMaxTempDot = searchModel.LeaveKmMax;
            }
            if (Decimal.TryParse(leaveKmMaxTempDot, out leaveKmMaxTemp))
            {
                leaveKmMax = leaveKmMaxTemp;
            }
            //End


            //Enter km
            //min
            decimal? enterKmMin = null;
            decimal enterKmMinTemp;
            string enterKmMinTempDot;

            if (searchModel.EnterKmMin != null && searchModel.EnterKmMin.Contains("."))
            {
                enterKmMinTempDot = searchModel.EnterKmMin.Replace('.', ',');
            }
            else
            {
                enterKmMinTempDot = searchModel.EnterKmMin;
            }
            if (Decimal.TryParse(enterKmMinTempDot, out enterKmMinTemp))
            {
                enterKmMin = enterKmMinTemp;
            }

            //max
            decimal? enterKmMax = null;
            decimal enterKmMaxTemp;
            string enterKmMaxTempDot;

            if (searchModel.EnterKmMax != null && searchModel.EnterKmMax.Contains("."))
            {
                enterKmMaxTempDot = searchModel.EnterKmMax.Replace('.', ',');
            }
            else
            {
                enterKmMaxTempDot = searchModel.EnterKmMax;
            }
            if (Decimal.TryParse(enterKmMaxTempDot, out enterKmMaxTemp))
            {
                enterKmMax = enterKmMaxTemp;
            }
            //End

            //Leaving fuel
            //min
            decimal? leaveFuelMin = null;
            decimal leaveFuelMinTemp;
            string leaveFuelMinTempDot;

            if (searchModel.LeaveFuelMin != null && searchModel.LeaveFuelMin.Contains("."))
            {
                leaveFuelMinTempDot = searchModel.LeaveFuelMin.Replace('.', ',');
            }
            else
            {
                leaveFuelMinTempDot = searchModel.LeaveFuelMin;
            }
            if (Decimal.TryParse(leaveFuelMinTempDot, out leaveFuelMinTemp))
            {
                leaveFuelMin = leaveFuelMinTemp;
            }

            //max
            decimal? leaveFuelMax = null;
            decimal leaveFuelMaxTemp;
            string leaveFuelMaxTempDot;

            if (searchModel.LeaveFuelMax != null && searchModel.LeaveFuelMax.Contains("."))
            {
                leaveFuelMaxTempDot = searchModel.LeaveFuelMax.Replace('.', ',');
            }
            else
            {
                leaveFuelMaxTempDot = searchModel.LeaveFuelMax;
            }
            if (Decimal.TryParse(leaveFuelMaxTempDot, out leaveFuelMaxTemp))
            {
                leaveFuelMax = leaveFuelMaxTemp;
            }
            //End

            //Enter fuel
            //min
            decimal? enterFuelMin = null;
            decimal enterFuelMinTemp;
            string enterFuelMinTempDot;

            if (searchModel.EnterFuelMin != null && searchModel.EnterFuelMin.Contains("."))
            {
                enterFuelMinTempDot = searchModel.EnterFuelMin.Replace('.', ',');
            }
            else
            {
                enterFuelMinTempDot = searchModel.EnterFuelMin;
            }
            if (Decimal.TryParse(enterFuelMinTempDot, out enterFuelMinTemp))
            {
                enterFuelMin = enterFuelMinTemp;
            }

            //max
            decimal? enterFuelMax = null;
            decimal enterFuelMaxTemp;
            string enterFuelMaxTempDot;

            if (searchModel.EnterFuelMax != null && searchModel.EnterFuelMax.Contains("."))
            {
                enterFuelMaxTempDot = searchModel.EnterFuelMax.Replace('.', ',');
            }
            else
            {
                enterFuelMaxTempDot = searchModel.EnterFuelMax;
            }
            if (Decimal.TryParse(enterFuelMaxTempDot, out enterFuelMaxTemp))
            {
                enterFuelMax = enterFuelMaxTemp;
            }
            //End


            model.InitialControlSchedule = db.InitialControlSchedule.GroupBy(k => k.VehicleId, (kay, e) => e.OrderByDescending(f => f.LeavingTime).FirstOrDefault()).
                                                                     Where(i => (i.IsOpen == false) &&
                                                                                (i.CheckUpCard.FirstOrDefault().IsOpen == null) &&
                                                                                (searchModel.VehicleId != null ? i.VehicleId == searchModel.VehicleId : true) &&
                                                                                (searchModel.RouteId != null ? i.RouteId == searchModel.RouteId : true) &&

                                                                                (searchModel.FirstDriverId != null ? i.FirstDriverId == searchModel.FirstDriverId : true) &&
                                                                                (searchModel.SecondDriverId != null ? i.SecondDriverId == searchModel.SecondDriverId : true) &&

                                                                                (searchModel.LeaveDateStart != null ? DbFunctions.TruncateTime(i.LeavingTime) >= leaveDateStart : true) &&
                                                                                (searchModel.LeaveDateEnd != null ? DbFunctions.TruncateTime(i.LeavingTime) <= leaveDateEnd : true) &&
                                                                                (searchModel.EnterDateStart != null ? DbFunctions.TruncateTime(i.EnterTime) >= enterDateStart : true) &&
                                                                                (searchModel.EnterDateEnd != null ? DbFunctions.TruncateTime(i.EnterTime) <= enterDateEnd : true) &&

                                                                                (searchModel.LeaveHourStart != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) >= leaveHourStart : true) &&
                                                                                (searchModel.LeaveHourEnd != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) <= leaveHourEnd : true) &&
                                                                                (searchModel.EnterHourStart != null ? DbFunctions.CreateTime(i.EnterTime.Value.Hour, i.EnterTime.Value.Minute, 0) >= enterHourStart : true) &&
                                                                                (searchModel.EnterHourEnd != null ? DbFunctions.CreateTime(i.EnterTime.Value.Hour, i.EnterTime.Value.Minute, 0) <= enterHourEnd : true) &&

                                                                                (searchModel.LeaveKmMin != null ? i.LeavingKilometer >= leaveKmMin : true) &&
                                                                                (searchModel.LeaveKmMax != null ? i.LeavingKilometer <= leaveKmMax : true) &&
                                                                                (searchModel.EnterKmMin != null ? i.EnterKilometer >= enterKmMin : true) &&
                                                                                (searchModel.EnterKmMax != null ? i.EnterKilometer <= enterKmMax : true) &&

                                                                                (searchModel.LeaveFuelMin != null ? i.LeavingFuel >= leaveFuelMin : true) &&
                                                                                (searchModel.LeaveFuelMax != null ? i.LeavingFuel <= leaveFuelMax : true) &&
                                                                                (searchModel.EnterFuelMin != null ? i.EnterFuel >= enterFuelMin : true) &&
                                                                                (searchModel.EnterFuelMax != null ? i.EnterFuel <= enterFuelMax : true)
                                                                                ).OrderByDescending(i => i.EnterTime).ToList();

            TempData["SortedInitContMechClose"] = model;
            model.SearchModelInitCont = searchModel;

            return View(model);
        }

        [OperationFilter]
        public ActionResult ClosedInitContMechUpdate(int id)
        {
            ViewBag.Ope = true;
            ViewHome model = new ViewHome
            {
                Employees = db.Employees.ToList()
            };

            ViewBag.InitCont = db.InitialControlSchedule.Find(id);
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult ClosedInitContMechUpdate(InitControl initCont)
        {
            ViewBag.Ope = true;
            InitialControlSchedule InitCon = db.InitialControlSchedule.Find(initCont.Id);

            int UserId = (int)Session["UserId"];
            //Updated user
            InitCon.EnterUpdateEmpId = UserId;

            //Updated date
            InitCon.EnterUpdateDate = DateTime.Now;

            //Second driver
            if (initCont.SecondDriverId == 0)
            {
                InitCon.SecondDriverId = null;
            }
            else
            {
                InitCon.SecondDriverId = initCont.SecondDriverId;
            }

            //Enter note
            InitCon.EnterNote = initCont.EnterNote;

            //Enter km
            decimal? kmDot = null;
            decimal kmTemp;
            string kmTempDot;

            if (initCont.EnterKilometer != null && initCont.EnterKilometer.Contains("."))
            {
                kmTempDot = initCont.EnterKilometer.Replace('.', ',');
            }
            else if (initCont.EnterKilometer != null)
            {
                kmTempDot = initCont.EnterKilometer;
            }
            else
            {
                kmTempDot = null;
            }

            if (kmTempDot != null && decimal.TryParse(kmTempDot, out kmTemp))
            {
                kmDot = decimal.Parse(kmTempDot);
            }
            InitCon.EnterKilometer = kmDot;
            //End

            //Enter fuel
            decimal? fuelDot = null;
            decimal fuelTemp;
            string fuelTempDot;

            if (initCont.EnterFuel != null && initCont.EnterFuel.Contains("."))
            {
                fuelTempDot = initCont.EnterFuel.Replace('.', ',');
            }
            else if (initCont.EnterFuel != null)
            {
                fuelTempDot = initCont.EnterFuel;
            }
            else
            {
                fuelTempDot = null;
            }

            if (fuelTempDot != null && decimal.TryParse(fuelTempDot, out fuelTemp))
            {
                fuelDot = decimal.Parse(fuelTempDot);
            }
            InitCon.EnterFuel = fuelDot;
            //End

            db.Entry(InitCon).State = EntityState.Modified;

            //Disable updating
            db.Entry(InitCon).Property(x => x.IsOpen).IsModified = false;
            db.Entry(InitCon).Property(x => x.VehicleId).IsModified = false;
            db.Entry(InitCon).Property(x => x.RouteId).IsModified = false;
            db.Entry(InitCon).Property(x => x.FirstDriverId).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingSeniorRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingTime).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingKilometer).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingFuel).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingUpdateEmpId).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingUpdateDate).IsModified = false;
            db.Entry(InitCon).Property(x => x.LeavingNote).IsModified = false;

            db.Entry(InitCon).Property(x => x.EnterRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterSeniorRecMechId).IsModified = false;
            db.Entry(InitCon).Property(x => x.EnterTime).IsModified = false;

            db.SaveChanges();
            return RedirectToAction("ClosedInitContMechIndex");
        }


        //Checkup Card
        [OperationFilter]
        public ActionResult InitContCheckupIndex(SearchCheckup searchModel)
        {
            ViewBag.Ope = true;
            //return Content(searchModel.VehicleId.ToString());

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList(),
                JobCards = db.JobCards.ToList(),
                Employees = db.Employees.Where(m => m.PositionId == 3).ToList()
            };

            model.CheckUpCard = db.CheckUpCard.Where(c => (searchModel.VehicleId != null ? c.InitialControlSchedule.VehicleId == searchModel.VehicleId : true) &&
                                                          (searchModel.JobcardId != null ? c.JobCards.FirstOrDefault().Id == searchModel.JobcardId : true) &&
                                                          (searchModel.MechanicId != null ? c.Employees.Id == searchModel.MechanicId : true)
                                                          ).OrderByDescending(c => c.AddedDate).ToList();
            model.SearchCheckup = searchModel;
            return View(model);
        }

        [OperationFilter]
        public ActionResult InitContCheckupCreate(int? id, int? vehicleId)
        {
            ViewBag.Ope = true;
            int? VehicleId = vehicleId != null ? vehicleId : db.InitialControlSchedule.Find(id).VehicleId;

            if (id != null)
            {
                ViewBag.InitContId = id;
            }
            else if (vehicleId != null && db.InitialControlSchedule.Where(i => i.VehicleId == vehicleId).OrderByDescending(i => i.LeavingTime).FirstOrDefault() != null)
            {
                ViewBag.InitContId = db.InitialControlSchedule.Where(i => i.VehicleId == vehicleId).OrderByDescending(i => i.LeavingTime).FirstOrDefault().Id;
            }

            if (ViewBag.InitContId == null)
            {
                ViewBag.VehicleId = vehicleId;
            }

            //Check for if vehicle is in route
            InitialControlSchedule ınitialControlSchedule = db.InitialControlSchedule.FirstOrDefault(init => init.VehicleId == vehicleId && init.IsOpen == true);
            if (ınitialControlSchedule != null)
            {
                Session["VehicleIsInRoute"] = true;
                return View();
            }

            //Check for if there is open jobcard
            JobCards jobCards = db.JobCards.FirstOrDefault(j => j.IsOpen == true && j.CheckUpCard.InitialControlSchedule.VehicleId == VehicleId);
            if (jobCards != null)
            {
                Session["OpenJobCard"] = true;
                return View();
            }

            //Check for maintenance history
            UniversalMethods universalMethods = new UniversalMethods();
            universalMethods.MaintenanceFirstCheckUp();

            //Check for maintenance
            MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.FirstOrDefault(m => m.MaintenanceStatus == null && m.InitialControlSchedule.VehicleId == VehicleId);
            if (maintenanceHistory != null)
            {
                ViewBag.MaintenanceStataus =maintenanceHistory.MaintenanceType.Name + "/" + maintenanceHistory.MaintenanceType.MaintenanceValue;
            }


            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult InitContCheckupCreate(CheckupModel checkUp)
        {
            ViewBag.Ope = true;
            //Create Check up
            CheckUpCard CheckUp = new CheckUpCard();
            int userId = (int)Session["UserId"];
            int? VehicleId = null;

            if (checkUp.InitContId != null)
            {
                CheckUp.InitContId = checkUp.InitContId;
                VehicleId = db.InitialControlSchedule.Find(checkUp.InitContId).VehicleId;
            }

            CheckUp.Description = checkUp.Description;
            CheckUp.IsOpen = true;
            CheckUp.AddedDate = DateTime.Now;
            CheckUp.AddedUserId = userId;

            if (db.Users.Find(userId).EmployeeId != null)
            {
                CheckUp.AddedMechanicId = db.Users.Find(userId).EmployeeId;
            }
            else
            {
                CheckUp.AddedMechanicId = null;
            }

            //Create Job Card
            JobCards jobCard = new JobCards();
            int? ConstantNo = db.Settings.FirstOrDefault(s => s.Name == "JobCardNoStart").Value;
            int? MaxNo = db.JobCards.Max(j => j.JobCardNo);
            if (MaxNo != null)
            {
                jobCard.JobCardNo = MaxNo + 1;
            }
            else
            {
                jobCard.JobCardNo = ConstantNo;
            }


            jobCard.IsOpen = true;
            jobCard.InUnderRepair = true;
            jobCard.AddedDate = DateTime.Now;
            jobCard.CheckupCardId = CheckUp.Id;
            jobCard.CompanyId = db.Companies.FirstOrDefault(c => c.IsClient == true).Id;

            //Adding and saving
            db.CheckUpCard.Add(CheckUp);
            db.JobCards.Add(jobCard);

            //Check for maintenance history
            UniversalMethods universalMethods = new UniversalMethods();
            universalMethods.MaintenanceFirstCheckUp();
            //universalMethods.CheckForMaintenanceHistoryAndCreateRequisition(VehicleId, CheckUp, jobCard);


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
                        if (!universalMethods.CheckForSparePartIfMustAdd(VehicleId, item.NotRequireSPLimit, item.WarehouseId))
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

            db.SaveChanges();
            return RedirectToAction("OpenInitContIndex");
        }

        [OperationFilter]
        public ActionResult InitContCheckupUpdate(int? id)
        {
            ViewBag.Ope = true;
            ViewBag.InitCont = db.CheckUpCard.Find(id);
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult InitContCheckupUpdate(CheckupModel checkupModel)
        {
            ViewBag.Ope = true;
            CheckUpCard checkUpCard = db.CheckUpCard.Find(checkupModel.Id);
            checkUpCard.Description = checkupModel.Description;

            db.Entry(checkUpCard).State = EntityState.Modified;

            //Disable updating
            db.Entry(checkUpCard).Property(x => x.InitContId).IsModified = false;
            db.Entry(checkUpCard).Property(x => x.IsOpen).IsModified = false;
            db.Entry(checkUpCard).Property(x => x.AddedDate).IsModified = false;
            db.Entry(checkUpCard).Property(x => x.AddedUserId).IsModified = false;
            db.Entry(checkUpCard).Property(x => x.AddedMechanicId).IsModified = false;

            db.SaveChanges();

            return RedirectToAction("InitContCheckupIndex");
        }

        [OperationFilter]
        public ActionResult InitContCheckupDelete(int id)
        {
            CheckUpCard Checkup = db.CheckUpCard.Find(id);
            JobCards jobcard = db.JobCards.FirstOrDefault(c => c.CheckupCardId == id);
            if (Checkup == null)
            {
                return RedirectToAction("InitContCheckupIndex");
            }
            if (jobcard != null)
            {
                db.JobCards.Remove(jobcard);
            }

            db.CheckUpCard.Remove(Checkup);
            db.SaveChanges();
            return RedirectToAction("InitContCheckupIndex");
        }


        //Job Card
        [OperationFilter]
        public ActionResult JobcardDashboard(SearchModelJobcard searchModel, int? JobCardId)
        {
            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            //ViewHome model = new ViewHome
            //{
            //    Vehicles = db.Vehicles.ToList()
            //};

            ////Variables
            ////Dates
            //DateTime? ReceiveDateStart = null;
            //DateTime? ReceiveDateEnd = null;
            //DateTime? DeliveredDateStart = null;
            //DateTime? DeliveredDateEnd = null;


            ////Filtering

            ////Leaving Date
            //if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            //{
            //    ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}
            //if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            //{
            //    ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}

            ////Enter Date
            //if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            //{
            //    DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}
            //if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            //{
            //    DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}

            //model.JobCards = db.JobCards.Where(j => (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
            //                                        (JobCardId != null ? j.Id == JobCardId : true) &&
            //                                        (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
            //                                        (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
            //                                        (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
            //                                        (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
            //                                        (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true) &&
            //                                        (searchModel.InUnderRepair != null ? j.InUnderRepair == true : true) &&
            //                                        (searchModel.InWaitDepot != null ? j.InWaitDepot == true : true) &&
            //                                        (searchModel.InWaitRoute != null ? j.InWaitRoute == true : true)
            //                                        ).OrderByDescending(o => o.JobCardNo).ToList();

            //model.SearchModelJobcard = searchModel;

            return View();
        }

        [OperationFilter]
        public ActionResult JobcardIndex(SearchModelJobcard searchModel, string searchModelString, int? JobCardId,int page=1)
        {

            //Codes for pagination
            string[] searchModelList = null;
            string test = "";
            bool searchModelStatus = false;
            foreach (var item in searchModel.GetType().GetProperties())
            {
                if (item.GetValue(searchModel, null) != null)
                {
                    searchModelStatus = true;
                    break;
                }
            }

            if (searchModelString != null && searchModelStatus == false)
            {
                searchModelList = searchModelString.Split('/');

                if (!string.IsNullOrEmpty(searchModelList[0]))
                {
                    searchModel.JobCardNo = Convert.ToInt32(searchModelList[0]);
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModel.vehicleId = Convert.ToInt32(searchModelList[1]);
                }
                if (!string.IsNullOrEmpty(searchModelList[2]))
                {
                    searchModel.receiveDateStart = searchModelList[2];
                }
                if (!string.IsNullOrEmpty(searchModelList[3]))
                {
                    searchModel.receiveDateEnd = searchModelList[3];
                }
                if (!string.IsNullOrEmpty(searchModelList[4]))
                {
                    searchModel.deliveredDateStart = searchModelList[4];
                }
                if (!string.IsNullOrEmpty(searchModelList[5]))
                {
                    searchModel.deliveredDateEnd = searchModelList[5];
                }
                if (!string.IsNullOrEmpty(searchModelList[6]))
                {
                    searchModel.InUnderRepair = Convert.ToBoolean(searchModelList[6]);
                }
                if (!string.IsNullOrEmpty(searchModelList[7]))
                {
                    searchModel.InWaitDepot = Convert.ToBoolean(searchModelList[7]);
                }
                if (!string.IsNullOrEmpty(searchModelList[8]))
                {
                    searchModel.InWaitRoute = Convert.ToBoolean(searchModelList[8]);
                }
                if (!string.IsNullOrEmpty(searchModelList[9]))
                {
                    searchModel.IsOpen = Convert.ToBoolean(searchModelList[9]);
                }
            }
            //End



            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            {
                ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            {
                ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            {
                DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            {
                DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            List<JobCards> jobcards = db.JobCards.Where(j => (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
                                                    (JobCardId != null ? j.Id == JobCardId : true) &&
                                                    (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
                                                    (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
                                                    (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true) &&
                                                    (searchModel.InUnderRepair != null ? j.InUnderRepair == true : true) &&
                                                    (searchModel.InWaitDepot != null ? j.InWaitDepot == true : true) &&
                                                    (searchModel.InWaitRoute != null ? j.InWaitRoute == true : true) &&
                                                    (searchModel.IsOpen != null ? j.IsOpen == null : true)
                                                    ).OrderByDescending(o => o.JobCardNo).ToList();



            //Pagination modul
        
            int limit = 10;
            if (limit > jobcards.Count)
            {
                limit = jobcards.Count;
            }

            ViewBag.total = (int)Math.Ceiling(((jobcards.Count() > 0 ? jobcards.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul


            model.JobCards = jobcards.Skip(((int)page - 1) * limit).Take(limit);
            model.SearchModelJobcard = searchModel;
            return View(model);
        }

        [OperationFilter]
        public ActionResult JobcardAllAdmin(SearchModelJobcard searchModel,string searchModelString, int? JobCardId,int page=1)
        {


            //Codes for pagination
            string[] searchModelList = null;
            string test = "";
            bool searchModelStatus = false;
            foreach (var item in searchModel.GetType().GetProperties())
            {
                if (item.GetValue(searchModel, null) != null)
                {
                    searchModelStatus = true;
                    break;
                }
            }

            if (searchModelString != null && searchModelStatus == false)
            {
                searchModelList = searchModelString.Split('/');

                if (!string.IsNullOrEmpty(searchModelList[0]))
                {
                    searchModel.JobCardNo = Convert.ToInt32(searchModelList[0]);
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModel.vehicleId = Convert.ToInt32(searchModelList[1]);
                }
                if (!string.IsNullOrEmpty(searchModelList[2]))
                {
                    searchModel.receiveDateStart = searchModelList[2];
                }
                if (!string.IsNullOrEmpty(searchModelList[3]))
                {
                    searchModel.receiveDateEnd = searchModelList[3];
                }
                if (!string.IsNullOrEmpty(searchModelList[4]))
                {
                    searchModel.deliveredDateStart = searchModelList[4];
                }
                if (!string.IsNullOrEmpty(searchModelList[5]))
                {
                    searchModel.deliveredDateEnd = searchModelList[5];
                }
                if (!string.IsNullOrEmpty(searchModelList[6]))
                {
                    searchModel.InUnderRepair = Convert.ToBoolean(searchModelList[6]);
                }
                if (!string.IsNullOrEmpty(searchModelList[7]))
                {
                    searchModel.InWaitDepot = Convert.ToBoolean(searchModelList[7]);
                }
                if (!string.IsNullOrEmpty(searchModelList[8]))
                {
                    searchModel.InWaitRoute = Convert.ToBoolean(searchModelList[8]);
                }
                if (!string.IsNullOrEmpty(searchModelList[9]))
                {
                    searchModel.IsOpen = Convert.ToBoolean(searchModelList[9]);
                }
            }
            //End



            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            {
                ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            {
                ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            {
                DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            {
                DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            var jobcards = db.JobCards.Where(j => (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
                                                    (JobCardId != null ? j.Id == JobCardId : true) &&
                                                    (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
                                                    (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
                                                    (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true) &&
                                                    (searchModel.InUnderRepair != null ? j.InUnderRepair == true : true) &&
                                                    (searchModel.InWaitDepot != null ? j.InWaitDepot == true : true) &&
                                                    (searchModel.InWaitRoute != null ? j.InWaitRoute == true : true) &&
                                                    (searchModel.IsOpen != null ? j.IsOpen == null : true)
                                                    ).OrderByDescending(o => o.JobCardNo).ToList();




            int limit = 10;
            if (limit > jobcards.Count)
            {
                limit = jobcards.Count;
            }

            ViewBag.total = (int)Math.Ceiling(((jobcards.Count() > 0 ? jobcards.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul


            model.JobCards = jobcards.Skip(((int)page - 1) * limit).Take(limit);

            model.SearchModelJobcard = searchModel;
            return View(model);
        }

        [OperationFilter]
        public ActionResult JobcardOpen(SearchModelJobcard searchModel, int? JobCardId)
        {
            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            {
                ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            {
                ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            {
                DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            {
                DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            model.JobCards = db.JobCards.Where(j => (j.IsOpen==true) &&
                                                    (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
                                                    (JobCardId != null ? j.Id == JobCardId : true) &&
                                                    (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
                                                    (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
                                                    (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true)
                                                    ).OrderByDescending(o => o.JobCardNo).ToList();

            model.SearchModelJobcard = searchModel;
            return View(model);
        }

        [OperationFilter]
        public ActionResult JobcardClosed(SearchModelJobcard searchModel,string searchModelString, int? JobCardId,int page=1)
        {


            //Codes for pagination
            string[] searchModelList = null;
            string test = "";
            bool searchModelStatus = false;
            foreach (var item in searchModel.GetType().GetProperties())
            {
                if (item.GetValue(searchModel, null) != null)
                {
                    searchModelStatus = true;
                    break;
                }
            }

            if (searchModelString != null && searchModelStatus == false)
            {
                searchModelList = searchModelString.Split('/');

                if (!string.IsNullOrEmpty(searchModelList[0]))
                {
                    searchModel.JobCardNo = Convert.ToInt32(searchModelList[0]);
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModel.vehicleId = Convert.ToInt32(searchModelList[1]);
                }
                if (!string.IsNullOrEmpty(searchModelList[2]))
                {
                    searchModel.receiveDateStart = searchModelList[2];
                }
                if (!string.IsNullOrEmpty(searchModelList[3]))
                {
                    searchModel.receiveDateEnd = searchModelList[3];
                }
                if (!string.IsNullOrEmpty(searchModelList[4]))
                {
                    searchModel.deliveredDateStart = searchModelList[4];
                }
                if (!string.IsNullOrEmpty(searchModelList[5]))
                {
                    searchModel.deliveredDateEnd = searchModelList[5];
                }
              
            }
            //End


            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            {
                ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            {
                ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            {
                DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            {
                DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

           var jobcards = db.JobCards.Where(j => (j.IsOpen == null) &&
                                                    (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
                                                    (JobCardId != null ? j.Id == JobCardId : true) &&
                                                    (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
                                                    (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
                                                    (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true)
                                                    ).OrderByDescending(o => o.JobCardNo).ToList();



            int limit = 10;
            if (limit > jobcards.Count)
            {
                limit = jobcards.Count;
            }

            ViewBag.total = (int)Math.Ceiling(((jobcards.Count() > 0 ? jobcards.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul


            model.JobCards = jobcards.Skip(((int)page - 1) * limit).Take(limit);
            model.SearchModelJobcard = searchModel;
            return View(model);
        }

        [OperationFilter]
        public ActionResult JobcardInWaitRoute(SearchModelJobcard searchModel,string searchModelString, int? JobCardId,int page=1)
        {


            //Codes for pagination
            string[] searchModelList = null;
            string test = "";
            bool searchModelStatus = false;
            foreach (var item in searchModel.GetType().GetProperties())
            {
                if (item.GetValue(searchModel, null) != null)
                {
                    searchModelStatus = true;
                    break;
                }
            }

            if (searchModelString != null && searchModelStatus == false)
            {
                searchModelList = searchModelString.Split('/');

                if (!string.IsNullOrEmpty(searchModelList[0]))
                {
                    searchModel.JobCardNo = Convert.ToInt32(searchModelList[0]);
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModel.vehicleId = Convert.ToInt32(searchModelList[1]);
                }
                if (!string.IsNullOrEmpty(searchModelList[2]))
                {
                    searchModel.receiveDateStart = searchModelList[2];
                }
                if (!string.IsNullOrEmpty(searchModelList[3]))
                {
                    searchModel.receiveDateEnd = searchModelList[3];
                }
                if (!string.IsNullOrEmpty(searchModelList[4]))
                {
                    searchModel.deliveredDateStart = searchModelList[4];
                }
                if (!string.IsNullOrEmpty(searchModelList[5]))
                {
                    searchModel.deliveredDateEnd = searchModelList[5];
                }
            }
            //End



            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            {
                ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            {
                ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            {
                DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            {
                DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            var jobcards = db.JobCards.Where(j => (j.InWaitRoute == true) &&
                                                    (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
                                                    (JobCardId != null ? j.Id == JobCardId : true) &&
                                                    (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
                                                    (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
                                                    (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true)
                                                    ).OrderByDescending(o => o.JobCardNo).ToList();



            int limit = 10;
            if (limit > jobcards.Count)
            {
                limit = jobcards.Count;
            }

            ViewBag.total = (int)Math.Ceiling(((jobcards.Count() > 0 ? jobcards.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul


            model.JobCards = jobcards.Skip(((int)page - 1) * limit).Take(limit);

            model.SearchModelJobcard = searchModel;
            return View(model);
        }

        [OperationFilter]
        public ActionResult JobcardInWaitDepot(SearchModelJobcard searchModel,string searchModelString, int? JobCardId,int page=1)
        {


            //Codes for pagination
            string[] searchModelList = null;
            string test = "";
            bool searchModelStatus = false;
            foreach (var item in searchModel.GetType().GetProperties())
            {
                if (item.GetValue(searchModel, null) != null)
                {
                    searchModelStatus = true;
                    break;
                }
            }

            if (searchModelString != null && searchModelStatus == false)
            {
                searchModelList = searchModelString.Split('/');

                if (!string.IsNullOrEmpty(searchModelList[0]))
                {
                    searchModel.JobCardNo = Convert.ToInt32(searchModelList[0]);
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModel.vehicleId = Convert.ToInt32(searchModelList[1]);
                }
                if (!string.IsNullOrEmpty(searchModelList[2]))
                {
                    searchModel.receiveDateStart = searchModelList[2];
                }
                if (!string.IsNullOrEmpty(searchModelList[3]))
                {
                    searchModel.receiveDateEnd = searchModelList[3];
                }
                if (!string.IsNullOrEmpty(searchModelList[4]))
                {
                    searchModel.deliveredDateStart = searchModelList[4];
                }
                if (!string.IsNullOrEmpty(searchModelList[5]))
                {
                    searchModel.deliveredDateEnd = searchModelList[5];
                }

            }
            //End

            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            {
                ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            {
                ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            {
                DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            {
                DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

           var jobcards = db.JobCards.Where(j => (j.InWaitDepot == true) &&
                                                    (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
                                                    (JobCardId != null ? j.Id == JobCardId : true) &&
                                                    (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
                                                    (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
                                                    (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true)
                                                    ).OrderByDescending(o => o.JobCardNo).ToList();


            int limit = 10;
            if (limit > jobcards.Count)
            {
                limit = jobcards.Count;
            }

            ViewBag.total = (int)Math.Ceiling(((jobcards.Count() > 0 ? jobcards.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul


            model.JobCards = jobcards.Skip(((int)page - 1) * limit).Take(limit);
            model.SearchModelJobcard = searchModel;
            return View(model);
        }


        [OperationFilter]
        public ActionResult StartMaintenance(int jobCardId)
        {
            Users usr = (Users)Session["User"];
            if (db.JobCards.Find(jobCardId).IsOpen==null && usr.IsAdmin == null)
            {
                return RedirectToAction("OprIndex");
            }

            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.Find(jobCardId);
            ViewBag.MainHistory = db.MaintenanceHistory.FirstOrDefault(m => m.JobCardId == jobCardId);
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult StartMaintenance(JobcardUpdate jobcardUpdate)
        {
            Users usr = (Users)Session["User"];
            if (db.JobCards.Find(jobcardUpdate.Id).IsOpen == null && usr.IsAdmin == null)
            {
                return RedirectToAction("OprIndex");
            }

            ViewBag.Ope = true;
            JobCards jobCard = db.JobCards.Find(jobcardUpdate.Id);
            //return Content(jobcardUpdate.EndTime);
            if (ModelState.IsValid)
            {
                //Start time
                if (!string.IsNullOrEmpty(jobcardUpdate.StartTime))
                {
                    DateTime startTime = DateTime.ParseExact(jobcardUpdate.StartTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    jobCard.StartTime = startTime;
                }
                else
                {
                    jobCard.StartTime = null;
                }

                //Is Maintenance
                if (jobcardUpdate.IsMaintenance==true)
                {
                    jobCard.IsMaintenance = true;
                }
                else
                {
                    jobCard.IsMaintenance = null;
                }

                //Is Accident
                if (jobcardUpdate.IsAccident == true)
                {
                    jobCard.IsAccident = true;
                }
                else
                {
                    jobCard.IsAccident = null;
                }

                //Is Repair
                if (jobcardUpdate.IsRepair == true)
                {
                    jobCard.IsRepair = true;
                }
                else
                {
                    jobCard.IsRepair = null;
                }

                //Is Guarantee
                if (jobcardUpdate.IsGuarantee == true)
                {
                    jobCard.IsGuarantee = true;
                }
                else
                {
                    jobCard.IsGuarantee = null;
                }

                //Receiving service officer
                int userId = (int)Session["UserId"];
                int? receivingServisOfficer = db.Users.Find(userId).EmployeeId;
                if (receivingServisOfficer != null)
                {
                    jobCard.ReceivingServisOfficer = receivingServisOfficer;
                }

                db.Entry(jobCard).State = EntityState.Modified;

                //Disable updating
                db.Entry(jobCard).Property(x => x.JobCardNo).IsModified = false;
                db.Entry(jobCard).Property(x => x.CheckupCardId).IsModified = false;
                db.Entry(jobCard).Property(x => x.CompanyId).IsModified = false;
                db.Entry(jobCard).Property(x => x.IsOpen).IsModified = false;
                db.Entry(jobCard).Property(x => x.InUnderRepair).IsModified = false;
                db.Entry(jobCard).Property(x => x.InWaitDepot).IsModified = false;
                db.Entry(jobCard).Property(x => x.InWaitRoute).IsModified = false;
                db.Entry(jobCard).Property(x => x.EndTime).IsModified = false;
                db.Entry(jobCard).Property(x => x.TakeOverServisOfficer).IsModified = false;
                db.Entry(jobCard).Property(x => x.HandOverServisOfficer).IsModified = false;
                db.Entry(jobCard).Property(x => x.Note).IsModified = false;

                db.SaveChanges();
            }

            return RedirectToAction("JobcardIndex", new { JobCardId=jobCard.Id });
        }
        
        [OperationFilter]
        public ActionResult FinishMaintenance(int jobCardId)
        {
            Users usr = (Users)Session["User"];
            if (db.JobCards.Find(jobCardId).IsOpen == null && usr.IsAdmin == null)
            {
                return RedirectToAction("OprIndex");
            }
            ViewBag.Ope = true;
            JobCards JC = db.JobCards.Find(jobCardId);
            if (JC.StartTime == null || JC.ReceivingServisOfficer == null)
            {
                return RedirectToAction("JobcardIndex");
            }
            ViewBag.JobCard = JC;
            ViewBag.MainHistory = db.MaintenanceHistory.FirstOrDefault(m => m.JobCardId == jobCardId);

            ViewHome model = new ViewHome()
            {
                Employees = db.Employees.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult FinishMaintenance(JobcardUpdate jobcardUpdate)
        {
            Users usr = (Users)Session["User"];
            if (db.JobCards.Find(jobcardUpdate.Id).IsOpen == null && usr.IsAdmin == null)
            {
                return RedirectToAction("OprIndex");
            }
            ViewBag.Ope = true;
            JobCards jobCard = db.JobCards.Find(jobcardUpdate.Id);
            
            //return Content(jobcardUpdate.EndTime);
            if (string.IsNullOrEmpty(jobcardUpdate.EndTime) || jobcardUpdate.TakeOverServisOfficer==0 || (jobCard.IsMaintenance!=null && jobcardUpdate.MaintenanceDone!=true))
            {
                Session["emptyFildCloseJobcard"]= true;
                ViewBag.JobCard = jobCard;
                ViewBag.EndTime = jobcardUpdate.EndTime;
                ViewBag.TakeOverServisOfficer = jobcardUpdate.TakeOverServisOfficer;
                ViewHome model = new ViewHome()
                {
                    Employees = db.Employees.ToList()
                };
                return View(model);
            }

            if (ModelState.IsValid)
            {
                //End time
                if (!string.IsNullOrEmpty(jobcardUpdate.EndTime))
                {
                    DateTime endTime = DateTime.ParseExact(jobcardUpdate.EndTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    jobCard.EndTime = endTime;
                }
                else
                {
                    jobCard.EndTime = null;
                }

                //Is waiting in route
                if (jobcardUpdate.InWaitRoute == true)
                {
                    jobCard.InWaitRoute = true;
                }
                else
                {
                    jobCard.InWaitRoute = null;
                }

                //Hand over service officer
                int userId = (int)Session["UserId"];
                int? ServisOfficer = db.Users.Find(userId).EmployeeId;
                if (ServisOfficer != null)
                {
                    jobCard.HandOverServisOfficer = ServisOfficer;
                }

                //Take over service officer
                if (jobcardUpdate.TakeOverServisOfficer != null && jobcardUpdate.TakeOverServisOfficer != 0)
                {
                    jobCard.TakeOverServisOfficer = jobcardUpdate.TakeOverServisOfficer;
                }
                else
                {
                    jobCard.TakeOverServisOfficer = null;
                }

                //Does maintenance done?
                if (jobcardUpdate.MaintenanceDone == true)
                {
                    MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.FirstOrDefault(m => m.JobCardId == jobCard.Id);
                    if (maintenanceHistory != null)
                    {
                        maintenanceHistory.MaintenanceStatus = true;
                        db.Entry(maintenanceHistory).State = EntityState.Modified;
                        //Disable updating
                        db.Entry(maintenanceHistory).Property(x => x.InitContId).IsModified = false;
                        db.Entry(maintenanceHistory).Property(x => x.JobCardId).IsModified = false;
                        db.Entry(maintenanceHistory).Property(x => x.MaintenanceId).IsModified = false;
                        db.Entry(maintenanceHistory).Property(x => x.AddedDate).IsModified = false;
                    }
                }
                else
                {
                    MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.FirstOrDefault(m => m.JobCardId == jobCard.Id);
                    if (maintenanceHistory != null)
                    {
                        maintenanceHistory.MaintenanceStatus = null;
                        db.Entry(maintenanceHistory).State = EntityState.Modified;
                        //Disable updating
                        db.Entry(maintenanceHistory).Property(x => x.InitContId).IsModified = false;
                        db.Entry(maintenanceHistory).Property(x => x.JobCardId).IsModified = false;
                        db.Entry(maintenanceHistory).Property(x => x.MaintenanceId).IsModified = false;
                        db.Entry(maintenanceHistory).Property(x => x.AddedDate).IsModified = false;
                    }
                }

                //Does all requsitions met?
                List<Requisitions> requisitions = db.Requisitions.Where(r => r.JobCardId == jobcardUpdate.Id && r.RequiredQuantity != r.MeetingQuantity).ToList();
                foreach (var item in requisitions)
                {
                    NotMetRequisitions notMetRequisitions = new NotMetRequisitions();
                    notMetRequisitions.IsOpen = true;
                    notMetRequisitions.CreatedDate = DateTime.Now;
                    notMetRequisitions.SPId = item.TempWarehouseId;
                    notMetRequisitions.RemainingQuantity = item.MeetingQuantity != null ? (item.RequiredQuantity - item.MeetingQuantity) : item.RequiredQuantity;
                    notMetRequisitions.OrijinalJCId = item.JobCardId;

                    db.NotMetRequisitions.Add(notMetRequisitions);
                    db.SaveChanges();
                }


                //Status of jobcard
                if (jobcardUpdate.status == "IsOpen")
                {
                    jobCard.InUnderRepair = null;
                    jobCard.InWaitDepot = null;
                    jobCard.IsOpen = null;
                }
                else if (jobcardUpdate.status == "InUnderRepair")
                {
                    jobCard.InUnderRepair = true;
                    jobCard.InWaitDepot = null;
                    jobCard.IsOpen = true;
                }
                else if (jobcardUpdate.status == "InWaitDepot")
                {
                    jobCard.InUnderRepair = null;
                    jobCard.InWaitDepot = true;
                    jobCard.IsOpen = true;
                }


                db.Entry(jobCard).State = EntityState.Modified;

                //Disable updating
                db.Entry(jobCard).Property(x => x.JobCardNo).IsModified = false;
                db.Entry(jobCard).Property(x => x.CheckupCardId).IsModified = false;
                db.Entry(jobCard).Property(x => x.CompanyId).IsModified = false;
                db.Entry(jobCard).Property(x => x.IsAccident).IsModified = false;
                db.Entry(jobCard).Property(x => x.IsMaintenance).IsModified = false;
                db.Entry(jobCard).Property(x => x.IsRepair).IsModified = false;
                db.Entry(jobCard).Property(x => x.IsGuarantee).IsModified = false;
                db.Entry(jobCard).Property(x => x.StartTime).IsModified = false;
                db.Entry(jobCard).Property(x => x.ReceivingServisOfficer).IsModified = false;
                db.Entry(jobCard).Property(x => x.Note).IsModified = false;

                db.SaveChanges();
            }

            return RedirectToAction("JobcardIndex", new { JobCardId = jobCard.Id });
        }

        [OperationFilter]
        public ActionResult JobcardDetails(int id)
        {
            //ReceivingServisOfficer - Employee
            //TakeOverServisOfficer - Employee1
            //HandOverServisOfficer - Employee2

            //Employees-Leaving Mechaic
            //Employees1-Leaving Senior Mechaic
            //Employees2-Enter Mechaic
            //Employees3-Enter Senior Mechaic
            //Employees4-First Driver
            //Employees5-Second Driver

            ViewBag.Ope = true;

            //Color #1dd60f
            //Color- txt #0004ab

            JobCards jobCard = db.JobCards.Find(id);
            ViewHome model = new ViewHome() { };
            JobcardDetailsModel jobcardDetailsModel = new JobcardDetailsModel();

            jobcardDetailsModel.Id = id;
            jobcardDetailsModel.JobcardNo = jobCard.JobCardNo;
            jobcardDetailsModel.Company = jobCard.Companies.Name;
            jobcardDetailsModel.VehicleNumber = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId!=null) ? db.Vehicles.FirstOrDefault(v=>v.Id==jobCard.CheckUpCard.InitialControlSchedule.VehicleId).Number : "");
            jobcardDetailsModel.VehicleBrand = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId!=null) ? db.Vehicles.FirstOrDefault(v=>v.Id==jobCard.CheckUpCard.InitialControlSchedule.VehicleId).VehiclesBrand.Brand : "");
            jobcardDetailsModel.VehicleCode = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId!=null) ? db.Vehicles.FirstOrDefault(v=>v.Id==jobCard.CheckUpCard.InitialControlSchedule.VehicleId).VehicleCode : "");
            jobcardDetailsModel.ReleaseYear = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId!=null) ? db.Vehicles.FirstOrDefault(v=>v.Id==jobCard.CheckUpCard.InitialControlSchedule.VehicleId).ReleaseYear.ToString() : "");
            jobcardDetailsModel.ChassisNumber = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId!=null) ? db.Vehicles.FirstOrDefault(v=>v.Id==jobCard.CheckUpCard.InitialControlSchedule.VehicleId).ChassisNumber : "");
            jobcardDetailsModel.EngineCode = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId!=null) ? db.Vehicles.FirstOrDefault(v=>v.Id==jobCard.CheckUpCard.InitialControlSchedule.VehicleId).EngineCode : "");
            jobcardDetailsModel.MaxKm = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId!=null) ? db.InitialControlSchedule.FirstOrDefault(i=>i.Id==jobCard.CheckUpCard.InitContId).EnterKilometer.Value.ToString("#.##") : "");

            //Failture type
            List<string> failtureType = new List<string>();
            if (jobCard.IsMaintenance!=null)
            {
                failtureType.Add("Texniki qulluq");
            }
            if (jobCard.IsAccident != null)
            {
                failtureType.Add("Qəza");
            }

            if (jobCard.IsRepair != null)
            {
                failtureType.Add("Təmir");
            }

            if (jobCard.IsGuarantee != null)
            {
                failtureType.Add("Zəmanət");
            }
            foreach (var item in failtureType)
            {
                jobcardDetailsModel.FailureType += item + (failtureType.Last()!=item? ", ":"");
            }
            //End

            jobcardDetailsModel.SeniorMech = (jobCard.CheckupCardId != null && jobCard.CheckUpCard.InitContId != null) ? (jobCard.CheckUpCard.InitialControlSchedule.Employees3.Surname + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees3.Name + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees3.FutherName):"";
            jobcardDetailsModel.Mech = (jobCard.CheckupCardId!=null && jobCard.CheckUpCard.InitContId!=null)? (jobCard.CheckUpCard.InitialControlSchedule.Employees2.Surname + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees2.Name + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees2.FutherName):"";
            jobcardDetailsModel.ReceivTime = jobCard.StartTime!=null? jobCard.StartTime.Value.ToString("dd.MM.yyyy HH:mm"):"";
            jobcardDetailsModel.ReceivingController = jobCard.ReceivingServisOfficer!=null? (jobCard.Employees.Surname +" "+ jobCard.Employees.Name+" "+ jobCard.Employees.FutherName):"";
            jobcardDetailsModel.HandOverTime = jobCard.EndTime!=null? jobCard.EndTime.Value.ToString("dd.MM.yyyy HH:mm"):"";
            jobcardDetailsModel.HandOverController = jobCard.HandOverServisOfficer!=null? (jobCard.Employees2.Surname + " " + jobCard.Employees2.Name + " " + jobCard.Employees2.FutherName):"";
            jobcardDetailsModel.TakeOverController = jobCard.TakeOverServisOfficer!=null? (jobCard.Employees1.Surname + " " + jobCard.Employees1.Name + " " + jobCard.Employees1.FutherName):"";

            //Lists
            jobcardDetailsModel.Requisitions = db.Requisitions.Where(r => r.JobCardId == id).ToList();
            jobcardDetailsModel.JobcardToDoneWorks = db.JobcardToDoneWorks.Where(d => d.JobcardId == id).ToList();
            model.JobcardDetailsModel = jobcardDetailsModel;

            return View(model);
        }

        [OperationFilter]
        public ActionResult JobcardNote(int jobCardId)
        {
            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.Find(jobCardId);
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult JobcardNote(int JobCardId, string Note)
        {
            if (ModelState.IsValid)
            {
                JobCards jobCards = db.JobCards.Find(JobCardId);

                jobCards.Note = Note;

                db.Entry(jobCards).State = EntityState.Modified;
                db.Entry(jobCards).Property(x => x.JobCardNo).IsModified = false;
                db.Entry(jobCards).Property(x => x.CheckupCardId).IsModified = false;
                db.Entry(jobCards).Property(x => x.CompanyId).IsModified = false;
                db.Entry(jobCards).Property(x => x.IsOpen).IsModified = false;
                db.Entry(jobCards).Property(x => x.InUnderRepair).IsModified = false;
                db.Entry(jobCards).Property(x => x.InWaitDepot).IsModified = false;
                db.Entry(jobCards).Property(x => x.InWaitRoute).IsModified = false;
                db.Entry(jobCards).Property(x => x.IsAccident).IsModified = false;
                db.Entry(jobCards).Property(x => x.IsMaintenance).IsModified = false;
                db.Entry(jobCards).Property(x => x.IsRepair).IsModified = false;
                db.Entry(jobCards).Property(x => x.IsGuarantee).IsModified = false;
                db.Entry(jobCards).Property(x => x.EndTime).IsModified = false;
                db.Entry(jobCards).Property(x => x.StartTime).IsModified = false;
                db.Entry(jobCards).Property(x => x.ReceivingServisOfficer).IsModified = false;
                db.Entry(jobCards).Property(x => x.TakeOverServisOfficer).IsModified = false;
                db.Entry(jobCards).Property(x => x.HandOverServisOfficer).IsModified = false;

                db.SaveChanges();
                return RedirectToAction("JobcardDetails", new { id = JobCardId });
            }
            return RedirectToAction("JobcardDetails", new { id = JobCardId });
        }

        [OperationFilter]
        public ActionResult JobcardPrint(int jobCardId)
        {          
            JobCards jobCard = db.JobCards.Find(jobCardId);
            ViewHome model = new ViewHome() { };
            JobcardDetailsModel jobcardDetailsModel = new JobcardDetailsModel();

            jobcardDetailsModel.Id = jobCardId;
            jobcardDetailsModel.JobcardNo = jobCard.JobCardNo;
            jobcardDetailsModel.Company = jobCard.Companies.Name;
            jobcardDetailsModel.VehicleNumber = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId != null) ? db.Vehicles.FirstOrDefault(v => v.Id == jobCard.CheckUpCard.InitialControlSchedule.VehicleId).Number : "");
            jobcardDetailsModel.VehicleBrand = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId != null) ? db.Vehicles.FirstOrDefault(v => v.Id == jobCard.CheckUpCard.InitialControlSchedule.VehicleId).VehiclesBrand.Brand : "");
            jobcardDetailsModel.VehicleCode = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId != null) ? db.Vehicles.FirstOrDefault(v => v.Id == jobCard.CheckUpCard.InitialControlSchedule.VehicleId).VehicleCode : "");
            jobcardDetailsModel.ReleaseYear = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId != null) ? db.Vehicles.FirstOrDefault(v => v.Id == jobCard.CheckUpCard.InitialControlSchedule.VehicleId).ReleaseYear.ToString() : "");
            jobcardDetailsModel.ChassisNumber = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId != null) ? db.Vehicles.FirstOrDefault(v => v.Id == jobCard.CheckUpCard.InitialControlSchedule.VehicleId).ChassisNumber : "");
            jobcardDetailsModel.EngineCode = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId != null) ? db.Vehicles.FirstOrDefault(v => v.Id == jobCard.CheckUpCard.InitialControlSchedule.VehicleId).EngineCode : "");
            jobcardDetailsModel.MaxKm = ((jobCard.CheckUpCard.InitContId != null && jobCard.CheckUpCard.InitialControlSchedule.VehicleId != null) ? db.InitialControlSchedule.FirstOrDefault(i => i.Id == jobCard.CheckUpCard.InitContId).EnterKilometer.Value.ToString("#.##") : "");

            //Failture type
            List<string> failtureType = new List<string>();
            if (jobCard.IsMaintenance != null)
            {
                failtureType.Add("Texniki qulluq");
            }
            if (jobCard.IsAccident != null)
            {
                failtureType.Add("Qəza");
            }

            if (jobCard.IsRepair != null)
            {
                failtureType.Add("Təmir");
            }

            if (jobCard.IsGuarantee != null)
            {
                failtureType.Add("Zəmanət");
            }
            foreach (var item in failtureType)
            {
                jobcardDetailsModel.FailureType += item + (failtureType.Last() != item ? ", " : "");
            }
            //End

            jobcardDetailsModel.SeniorMech = (jobCard.CheckupCardId != null && jobCard.CheckUpCard.InitContId != null) ? (jobCard.CheckUpCard.InitialControlSchedule.Employees3.Surname + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees3.Name + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees3.FutherName) : "";
            jobcardDetailsModel.Mech = (jobCard.CheckupCardId != null && jobCard.CheckUpCard.InitContId != null) ? (jobCard.CheckUpCard.InitialControlSchedule.Employees2.Surname + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees2.Name + " " + jobCard.CheckUpCard.InitialControlSchedule.Employees2.FutherName) : "";
            jobcardDetailsModel.ReceivTime = jobCard.StartTime != null ? jobCard.StartTime.Value.ToString("dd.MM.yyyy HH:mm") : "";
            jobcardDetailsModel.ReceivingController = jobCard.ReceivingServisOfficer != null ? (jobCard.Employees.Surname + " " + jobCard.Employees.Name + " " + jobCard.Employees.FutherName) : "";
            jobcardDetailsModel.HandOverTime = jobCard.EndTime != null ? jobCard.EndTime.Value.ToString("dd.MM.yyyy HH:mm") : "";
            jobcardDetailsModel.HandOverController = jobCard.HandOverServisOfficer != null ? (jobCard.Employees2.Surname + " " + jobCard.Employees2.Name + " " + jobCard.Employees2.FutherName) : "";
            jobcardDetailsModel.TakeOverController = jobCard.TakeOverServisOfficer != null ? (jobCard.Employees1.Surname + " " + jobCard.Employees1.Name + " " + jobCard.Employees1.FutherName) : "";

            //Lists
            jobcardDetailsModel.Requisitions = db.Requisitions.Where(r => r.JobCardId == jobCardId).ToList();
            jobcardDetailsModel.JobcardToDoneWorks = db.JobcardToDoneWorks.Where(d => d.JobcardId == jobCardId).ToList();
            model.JobcardDetailsModel = jobcardDetailsModel;

            return new ViewAsPdf(model) { PageOrientation = Rotativa.Options.Orientation.Landscape, PageSize = Rotativa.Options.Size.A4 };

            //return View(model);
        }


        //Mobile Service
        [OperationFilter]
        public ActionResult mobileServiceDashboard()
        {
            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            //if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            //{
            //    ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}
            //if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            //{
            //    ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}

            ////Enter Date
            //if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            //{
            //    DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}
            //if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            //{
            //    DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //}

            //model.JobCards = db.JobCards.Where(j => (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
            //                                        (JobCardId != null ? j.Id == JobCardId : true) &&
            //                                        (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
            //                                        (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
            //                                        (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
            //                                        (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
            //                                        (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true) &&
            //                                        (searchModel.InUnderRepair != null ? j.InUnderRepair == true : true) &&
            //                                        (searchModel.InWaitDepot != null ? j.InWaitDepot == true : true) &&
            //                                        (searchModel.InWaitRoute != null ? j.InWaitRoute == true : true)
            //                                        ).OrderByDescending(o => o.JobCardNo).ToList();

            //model.SearchModelJobcard = searchModel;
            return View(model);
        }

        //Receive repaired bus
        [OperationFilter]
        public ActionResult SubmitRepairedBus(SearchModelJobcard searchModel, int? JobCardId)
        {
            ViewBag.Ope = true;
            ViewBag.JobCard = db.JobCards.ToList();
            int userId = (int)Session["UserId"];
            int? employeeIdOfUser = db.Users.Find(userId).EmployeeId;

            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList()
            };

            //Variables
            //Dates
            DateTime? ReceiveDateStart = null;
            DateTime? ReceiveDateEnd = null;
            DateTime? DeliveredDateStart = null;
            DateTime? DeliveredDateEnd = null;


            //Filtering

            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.receiveDateStart))
            {
                ReceiveDateStart = DateTime.ParseExact(searchModel.receiveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.receiveDateEnd))
            {
                ReceiveDateEnd = DateTime.ParseExact(searchModel.receiveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Enter Date
            if (!string.IsNullOrEmpty(searchModel.deliveredDateStart))
            {
                DeliveredDateStart = DateTime.ParseExact(searchModel.deliveredDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.deliveredDateEnd))
            {
                DeliveredDateEnd = DateTime.ParseExact(searchModel.deliveredDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            model.JobCards = db.JobCards.Where(j => (j.TakeOverServisOfficerSubmit == null) &&
                                                    (j.IsOpen == null) &&
                                                    (j.TakeOverServisOfficer == employeeIdOfUser) &&
                                                    (searchModel.JobCardNo != null ? j.JobCardNo == searchModel.JobCardNo : true) &&
                                                    (JobCardId != null ? j.Id == JobCardId : true) &&
                                                    (searchModel.vehicleId != null ? j.CheckUpCard.InitialControlSchedule.VehicleId == searchModel.vehicleId : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime >= ReceiveDateStart : true) &&
                                                    (searchModel.receiveDateStart != null ? j.StartTime <= ReceiveDateEnd : true) &&
                                                    (searchModel.deliveredDateStart != null ? j.EndTime >= DeliveredDateStart : true) &&
                                                    (searchModel.deliveredDateEnd != null ? j.EndTime <= DeliveredDateEnd : true)
                                                    ).OrderByDescending(o => o.JobCardNo).ToList();

            model.SearchModelJobcard = searchModel;
            return View(model);
        }

        [OperationFilter]
        public JsonResult SubmitRepairedBusSubmit(string checkedItems, string uncheckedItems)
        {
            AJAXResponseModel result = new AJAXResponseModel();
            
            //Getting pure items for checking
            string[] checkedItemsListString = checkedItems.Split('-');
            List<int> checkedItemsList = new List<int>();
            int num;
            foreach (var item in checkedItemsListString)
            {
                if (int.TryParse(item, out num))
                {
                    checkedItemsList.Add(num);
                }
            }

            //Getting pure items for unchecking
            string[] uncheckedItemsListString = uncheckedItems.Split('-');
            List<int> uncheckedItemsList = new List<int>();
            int num2;
            foreach (var item in uncheckedItemsListString)
            {
                if (int.TryParse(item, out num2))
                {
                    uncheckedItemsList.Add(num2);
                }
            }

            //Submitting checked items            
            foreach (var item in db.JobCards.Where(d => d.IsOpen == null && d.TakeOverServisOfficerSubmit == null).ToList())
            {
                foreach (var item2 in checkedItemsList)
                {
                    if (item.Id == item2)
                    {
                        item.TakeOverServisOfficerSubmit = true;

                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                        result.statusChecked = "success";
                    }
                }
            }

            //Submitting Unchecked items
            //int day = 0;
            foreach (var item in db.JobCards.Where(d => d.IsOpen == null && d.TakeOverServisOfficerSubmit == true).ToList())
            {
                foreach (var item2 in uncheckedItemsList)
                {

                    //if (item.SubmitInsurDateFirst != null)
                    //{
                    //    DateTime toDay = DateTime.Today;
                    //    DateTime? subInsDate = null;
                    //    subInsDate = DateTime.ParseExact(item.SubmitInsurDateFirst.Value.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    //    day = (toDay - (DateTime)subInsDate).Days;
                    //}

                    if (item.Id == item2 /*&& day <= 3*/)
                    {
                        item.TakeOverServisOfficerSubmit = null;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                        result.statusUnchecked = "success";
                    }
                }
            }

            //Adding date of current accidents
            //DateTime addingDate = new DateTime();
            //int firstChecked = 0;
            //int firstunchecked = 0;
            //if (checkedItemsList.Count > 0)
            //{
            //    firstChecked += checkedItemsList[0];
            //    addingDate = Convert.ToDateTime(db.AccidentDescription.FirstOrDefault(d => d.Id == firstChecked).AccidentVehicle.AddingDate);
            //}
            //else if (uncheckedItemsList.Count > 0)
            //{
            //    firstunchecked += uncheckedItemsList[0];
            //    addingDate = Convert.ToDateTime(db.AccidentDescription.FirstOrDefault(d => d.Id == firstunchecked).AccidentVehicle.AddingDate);
            //}

            ////Count of new
            //result.countOfNew = db.AccidentDescription.Where(d => d.Submit == true && d.SubmitInsurFirst == null && (d.AccidentVehicle.AddingDate.Value.Year == addingDate.Year && d.AccidentVehicle.AddingDate.Value.Month == addingDate.Month && d.AccidentVehicle.AddingDate.Value.Day == addingDate.Day)).ToList().Count;

            ////Count of Submitted
            //result.countOfSubmitted = db.AccidentDescription.Where(d => d.Submit == true && d.SubmitInsurFirst == true).ToList().Count;

            return Json(result, JsonRequestBehavior.AllowGet);

        }


        //Requisitions
        [OperationFilter]
        public ActionResult RequisitionsIndex(int jobCardId)
        {
            Users usr = (Users)Session["User"];
            if (db.JobCards.Find(jobCardId).IsOpen == null && usr.IsAdmin == null)
            {
                return RedirectToAction("OprIndex");
            }
            ViewBag.Ope = true;
            JobCards JC = db.JobCards.Find(jobCardId);
            if (JC.StartTime==null || JC.ReceivingServisOfficer==null)
            {
                return RedirectToAction("JobcardIndex");
            }

            ViewBag.JobcardId = jobCardId;
            ViewHome model = new ViewHome();
            List<RequisitionsModel> requisitionsModelList = new List<RequisitionsModel>();
            foreach (var item in db.Requisitions.Where(j => j.JobCardId == jobCardId).ToList())
            {
                RequisitionsModel requisitionsModel = new RequisitionsModel();
                requisitionsModel.Id = item.Id;
                requisitionsModel.JobCardId = item.JobCardId;
                requisitionsModel.TempWarehouseId = item.TempWarehouseId;
                requisitionsModel.RequiredQuantity = item.RequiredQuantity;
                requisitionsModel.MeetingQuantity = item.MeetingQuantity;
                requisitionsModel.AddedDate = item.AddedDate;
                requisitionsModel.IsOpen = item.IsOpen;

                requisitionsModel.sparePartName = item.TempWarehouse.Name;
                requisitionsModel.warehouse = db.TempWarehouse.FirstOrDefault(w=>w.Id==item.TempWarehouseId).Quantity;
                requisitionsModel.requiredBefore = db.Requisitions.Where(r=>r.IsOpen==true && r.TempWarehouseId==item.TempWarehouseId && r.JobCardId!=item.JobCardId && r.AddedDate<item.AddedDate).ToList().Sum(t=>t.RequiredQuantity);

                requisitionsModelList.Add(requisitionsModel);
            }

            model.RequisitionsModel = requisitionsModelList;

            return View(model);
        }

        [OperationFilter]
        public ActionResult RequisitionsAdd(int id)
        {
            ViewBag.Ope = true;
            int? brandId = null;
            
            brandId = db.JobCards.Find(id).CheckUpCard.InitialControlSchedule.Vehicles.VehiclesBrand.Id;            
            ViewBag.JobCard = db.JobCards.Find(id);
            ViewHome model = new ViewHome()
            {
                TempWarehouse = db.TempWarehouse.Where(w=>w.VehicleId== brandId).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult RequisitionsAdd(int JobCardId, List<RequisitionList> Requisitions)
        {
            ViewBag.Ope = true;
            int jobCardId = 0;
            if (int.TryParse(JobCardId.ToString(), out jobCardId))
            {
                jobCardId = int.Parse(JobCardId.ToString());
            }
            else
            {
                return RedirectToAction("JobcardIndex");
            }

            foreach (var item in Requisitions)
            {
                if (item.TempWarehouseId>0)
                {
                    Requisitions requisitions = db.Requisitions.FirstOrDefault(r => r.JobCardId == JobCardId && r.TempWarehouseId == item.TempWarehouseId);
                    if (requisitions != null)
                    {
                        Session["repeatedSP"] = true;
                        return RedirectToAction("RequisitionsAdd");
                    }

                    Requisitions requisition = new Requisitions();
                    requisition.IsOpen = true;
                    requisition.JobCardId = jobCardId;
                    requisition.TempWarehouseId = item.TempWarehouseId;

                    //Required Quantity
                    decimal? QuaDot = null;
                    decimal QuaTemp;
                    string QuaTempDot;

                    if (item.RequiredQuantity != null && item.RequiredQuantity.Contains("."))
                    {
                        QuaTempDot = item.RequiredQuantity.Replace('.', ',');
                    }
                    else if (item.RequiredQuantity != null)
                    {
                        QuaTempDot = item.RequiredQuantity;
                    }
                    else
                    {
                        QuaTempDot = null;
                    }

                    if (QuaTempDot != null && decimal.TryParse(QuaTempDot, out QuaTemp))
                    {
                        QuaDot = decimal.Parse(QuaTempDot);
                    }
                    //End

                    requisition.RequiredQuantity = QuaDot;
                    requisition.AddedDate = DateTime.Now;

                    db.Requisitions.Add(requisition);
                }                
            }
            db.SaveChanges();

            return RedirectToAction("RequisitionsIndex", new { jobCardId = jobCardId});
        }

        [OperationFilter]
        public ActionResult RequisitionsUpdate(int id)
        {
            ViewBag.Ope = true;
            ViewBag.Requisition = db.Requisitions.Find(id);
            ViewBag.JobCard = db.JobCards.Find(ViewBag.Requisition.JobCardId);
            int? brandId = db.Requisitions.Find(id).TempWarehouse.VehicleId;

            ViewHome model = new ViewHome()
            {
                TempWarehouse = db.TempWarehouse.Where(t=>t.VehicleId == brandId).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult RequisitionsUpdate(RequisitionList requisitionList)
        {
            if (requisitionList == null)
            {
                return RedirectToAction("RequisitionsUpdate");
            }

            if (ModelState.IsValid)
            {
                Requisitions requisitions = db.Requisitions.Find(requisitionList.Id);

                //Required Quantity
                decimal? QuaDot = null;
                decimal QuaTemp;
                string QuaTempDot;

                if (requisitionList.RequiredQuantity != null && requisitionList.RequiredQuantity.Contains("."))
                {
                    QuaTempDot = requisitionList.RequiredQuantity.Replace('.', ',');
                }
                else if (requisitionList.RequiredQuantity != null)
                {
                    QuaTempDot = requisitionList.RequiredQuantity;
                }
                else
                {
                    QuaTempDot = null;
                }

                if (QuaTempDot != null && decimal.TryParse(QuaTempDot, out QuaTemp))
                {
                    QuaDot = decimal.Parse(QuaTempDot);
                }
                //End
                requisitions.RequiredQuantity = QuaDot;

                db.Entry(requisitions).State = EntityState.Modified;
                db.Entry(requisitions).Property(x => x.JobCardId).IsModified = false;
                db.Entry(requisitions).Property(x => x.TempWarehouseId).IsModified = false;
                db.Entry(requisitions).Property(x => x.MeetingQuantity).IsModified = false;
                db.Entry(requisitions).Property(x => x.AddedDate).IsModified = false;
                db.Entry(requisitions).Property(x => x.IsOpen).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("RequisitionsIndex", new { jobCardId = requisitionList.JobCardId });
            }
            return RedirectToAction("RequisitionsIndex", new { jobCardId=requisitionList.JobCardId });
        }

        [OperationFilter]
        public ActionResult RequisitionsDelete(int id)
        {
            Requisitions requisitions= db.Requisitions.Find(id);
            int? jobcardId = requisitions.JobCardId;
            if (requisitions == null)
            {
                Session["InvalidDelete"] = true;
                return RedirectToAction("RequisitionsIndex", new { jobCardId = jobcardId });
            }

            db.Requisitions.Remove(requisitions);
            db.SaveChanges();
            return RedirectToAction("RequisitionsIndex", new { jobCardId = jobcardId });
        }

        //
        public JsonResult RequisitionsAddItem(int id)
        {
            List<AJAXResponseModel> result = new List<AJAXResponseModel>();
            int? brandId = null;
            brandId = db.JobCards.Find(id).CheckUpCard.InitialControlSchedule.Vehicles.VehiclesBrand.Id;
            foreach (var item in db.TempWarehouse.Where(w => w.VehicleId == brandId).ToList())
            {
                AJAXResponseModel wItem = new AJAXResponseModel();
                wItem.PartCode = item.SparePartCode;
                wItem.Name = item.Name;
                wItem.wId = item.Id;
                result.Add(wItem);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        //
        public JsonResult RequisitionsAddItemCheckWarehouse(int PartId, int? requisitionId, string Quantity)
        {
            AJAXResponseModel result = new AJAXResponseModel();
            //Required Quantity
            decimal? QuaDot = null;
            decimal QuaTemp;
            string QuaTempDot;

            if (Quantity != null && Quantity.Contains("."))
            {
                QuaTempDot = Quantity.Replace('.', ',');
            }
            else if (Quantity != null)
            {
                QuaTempDot = Quantity;
            }
            else
            {
                QuaTempDot = null;
            }

            if (QuaTempDot != null && decimal.TryParse(QuaTempDot, out QuaTemp))
            {
                QuaDot = decimal.Parse(QuaTempDot);
            }
            //End

            //Part Id
            int partId = 0;
            if (int.TryParse(PartId.ToString(), out partId))
            {
                partId = int.Parse(PartId.ToString());
            }

            //Requisition Id
            int RequisitionId = 0;
            if (int.TryParse(requisitionId.ToString(), out RequisitionId))
            {
                RequisitionId = int.Parse(requisitionId.ToString());
            }

            //Checking with requisitions
            List<Requisitions> reqQuantity = db.Requisitions.Where(r => r.TempWarehouseId == partId && r.IsOpen == true && r.Id != RequisitionId).ToList();
            decimal? inReq = 0;
            foreach (var item in reqQuantity)
            {
                if (item.MeetingQuantity == null)
                {
                    inReq += item.RequiredQuantity;
                }
                else if ((item.RequiredQuantity - item.MeetingQuantity) > 0)
                {
                    inReq += (item.RequiredQuantity - item.MeetingQuantity);
                }
            }

            result.inreq = inReq;

            //Checking with warehouse
            decimal? quantity = db.TempWarehouse.FirstOrDefault(w => w.Id == partId).Quantity;

            if (inReq!=null)
            {
                quantity -= inReq;
            }

            if (QuaDot > quantity)
            {
                result.statusCompare = false;
                result.balance = quantity;
            }
            else
            {
                result.statusCompare = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Check for not repeat spare part
        public JsonResult CheckForNotRepeatSparePart(string sparePartId, string jobCardId)
        {
            AJAXResponseModel result = new AJAXResponseModel();
            int SparePartId = 0;
            int JobCardId = 0;

            if (int.TryParse(sparePartId.ToString(), out SparePartId)==true && int.TryParse(jobCardId.ToString(), out JobCardId) ==true)
            {
                Requisitions requisitions = db.Requisitions.FirstOrDefault(r => r.JobCardId == JobCardId && r.TempWarehouseId == SparePartId);
                if (requisitions!=null)
                {
                    result.status = "included";
                }
                else
                {
                    result.status = "true";
                }
            }
            else
            {
                result.status = "invalidId";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //Not met requisitions
        [OperationFilter]
        public ActionResult NotMetRequisitionsIndex(int? jobCardId)
        {
            ViewBag.Ope = true;
            ViewHome model = new ViewHome()
            {
                NotMetRequisitions = db.NotMetRequisitions.ToList()
            };
            return View(model);
        }
        //End


        //Done Works
        [OperationFilter]
        public ActionResult DoneWorksIndex(int jobCardId)
        {
            //Service Officer - Employee
            //Master - Employee1
            Users usr = (Users)Session["User"];
            if (db.JobCards.Find(jobCardId).IsOpen == null && usr.IsAdmin == null)
            {
                return RedirectToAction("OprIndex");
            }
            JobCards JC = db.JobCards.Find(jobCardId);
            if (JC.StartTime == null || JC.ReceivingServisOfficer == null)
            {
                return RedirectToAction("JobcardIndex");
            }

            int JK = 0;
            if (int.TryParse(jobCardId.ToString(), out JK))
            {
                JK = int.Parse(jobCardId.ToString());
            }
            else
            {
                return RedirectToAction("JobcardIndex");
            }

            ViewBag.Ope = true;
            ViewBag.JobcardId = JK;
            ViewHome model = new ViewHome()
            {
                JobcardToDoneWorks=db.JobcardToDoneWorks.Where(j=>j.JobcardId == JK).ToList()
            };           

            return View(model);
        }

        [OperationFilter]
        public ActionResult DoneWorksAdd(int jobcardId)
        {
            ViewBag.Ope = true;
            int JobcardId = 0;
            if (int.TryParse(jobcardId.ToString(),out JobcardId))
            {
                JobcardId = int.Parse(jobcardId.ToString());
            }
            else
            {
                return RedirectToAction("DoneWorksIndex");
            }
                       
            int BrandId = 0;

            if (db.JobCards.FirstOrDefault(j => j.Id == JobcardId).CheckUpCard.InitialControlSchedule.VehicleId!=null)
            {
                BrandId = db.JobCards.FirstOrDefault(j => j.Id == JobcardId).CheckUpCard.InitialControlSchedule.Vehicles.VehiclesBrand.Id;
            }
            

            ViewHome model = new ViewHome()
            {
                DoneWorks = db.DoneWorks.Where(w => w.VehicleBrandId == BrandId).ToList(),
                Employees = db.Employees.ToList()
            };
            ViewBag.JobCard = db.JobCards.Find(JobcardId);

            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult DoneWorksAdd(JobcardToDoneWorksModel jobcardToDoneWorks)
        {
            ViewBag.Ope = true;
            JobcardToDoneWorks JobcardToDoneWorks = new JobcardToDoneWorks();
            int userId = (int)Session["UserId"];
            int? ServisOfficer = db.Users.Find(userId).EmployeeId;

            //Jobcard id
            if (jobcardToDoneWorks.JobcardId!=null && jobcardToDoneWorks.JobcardId != 0)
            {
                JobcardToDoneWorks.JobcardId = jobcardToDoneWorks.JobcardId;
            }
            else
            {
                JobcardToDoneWorks.JobcardId = null;
            }

            //Work id
            if (jobcardToDoneWorks.WorkId != null && jobcardToDoneWorks.WorkId != 0)
            {
                JobcardToDoneWorks.WorkId = jobcardToDoneWorks.WorkId;
            }
            else
            {
                JobcardToDoneWorks.WorkId = null;
            }

            //Servis officer
            JobcardToDoneWorks.ServisOfficer = ServisOfficer;

            //Master
            if (jobcardToDoneWorks.Master != null && jobcardToDoneWorks.Master != 0)
            {
                JobcardToDoneWorks.Master = jobcardToDoneWorks.Master;
            }
            else
            {
                JobcardToDoneWorks.Master = null;
            }

            //Start time
            if (jobcardToDoneWorks.WorkStartTime != null)
            {
                DateTime workStartTime = DateTime.ParseExact(jobcardToDoneWorks.WorkStartTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                JobcardToDoneWorks.WorkStartTime = workStartTime;
            }
            else
            {
                JobcardToDoneWorks.WorkStartTime = null;
            }

            //End time
            if (jobcardToDoneWorks.WorkEndTime != null)
            {
                DateTime workEndTime = DateTime.ParseExact(jobcardToDoneWorks.WorkEndTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                JobcardToDoneWorks.WorkEndTime = workEndTime;
            }
            else
            {
                JobcardToDoneWorks.WorkEndTime = null;
            }

            db.JobcardToDoneWorks.Add(JobcardToDoneWorks);            
            db.SaveChanges();

            return RedirectToAction("DoneWorksIndex", new { jobCardId = jobcardToDoneWorks.JobcardId });
        }

        [OperationFilter]
        public ActionResult DoneWorksUpdate(int id)
        {
            ViewBag.Ope = true;
            JobcardToDoneWorks jobcardToDoneWorks = db.JobcardToDoneWorks.Find(id);
            ViewBag.JobCard = db.JobCards.Find(jobcardToDoneWorks.JobcardId);
            ViewBag.JobcardToDoneWorks = jobcardToDoneWorks;

            int BrandId = 0;
            if (db.JobCards.FirstOrDefault(j => j.Id == jobcardToDoneWorks.JobcardId).CheckUpCard.InitialControlSchedule.VehicleId != null)
            {
                BrandId = db.JobCards.FirstOrDefault(j => j.Id == jobcardToDoneWorks.JobcardId).CheckUpCard.InitialControlSchedule.Vehicles.VehiclesBrand.Id;
            }

            ViewHome model = new ViewHome()
            {
                DoneWorks = db.DoneWorks.Where(w => w.VehicleBrandId == BrandId).ToList(),
                Employees = db.Employees.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult DoneWorksUpdate(JobcardToDoneWorksModel jobcardToDoneWorks)
        {
            ViewBag.Ope = true;
            JobcardToDoneWorks JobcardToDoneWorks = db.JobcardToDoneWorks.Find(jobcardToDoneWorks.Id);

            if (ModelState.IsValid)
            {
                //Work id
                if (jobcardToDoneWorks.WorkId != null && jobcardToDoneWorks.WorkId != 0)
                {
                    JobcardToDoneWorks.WorkId = jobcardToDoneWorks.WorkId;
                }
                else
                {
                    JobcardToDoneWorks.WorkId = null;
                }

                //Master
                if (jobcardToDoneWorks.Master != null && jobcardToDoneWorks.Master != 0)
                {
                    JobcardToDoneWorks.Master = jobcardToDoneWorks.Master;
                }
                else
                {
                    JobcardToDoneWorks.Master = null;
                }

                //Start time
                if (jobcardToDoneWorks.WorkStartTime != null)
                {
                    DateTime workStartTime = DateTime.ParseExact(jobcardToDoneWorks.WorkStartTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    JobcardToDoneWorks.WorkStartTime = workStartTime;
                }
                else
                {
                    JobcardToDoneWorks.WorkStartTime = null;
                }

                //End time
                if (jobcardToDoneWorks.WorkEndTime != null)
                {
                    DateTime workEndTime = DateTime.ParseExact(jobcardToDoneWorks.WorkEndTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    JobcardToDoneWorks.WorkEndTime = workEndTime;
                }
                else
                {
                    JobcardToDoneWorks.WorkEndTime = null;
                }
            }

            db.Entry(JobcardToDoneWorks).State= EntityState.Modified;
            db.Entry(JobcardToDoneWorks).Property(j => j.ServisOfficer).IsModified = false;
            db.Entry(JobcardToDoneWorks).Property(j => j.JobcardId).IsModified = false;

            db.SaveChanges();

            return RedirectToAction("DoneWorksIndex", new { jobCardId = JobcardToDoneWorks.JobcardId });
        }
        
        [OperationFilter]
        public ActionResult DoneWorksDelete(int id)
        {
            JobcardToDoneWorks jobcardToDoneWorks = db.JobcardToDoneWorks.Find(id);
            int? jobcardId = jobcardToDoneWorks.JobcardId;
            if (jobcardToDoneWorks == null)
            {
                //Session["InvalidDelete"] = true;
                return RedirectToAction("DoneWorksIndex", new { jobCardId = jobcardId });
            }

            db.JobcardToDoneWorks.Remove(jobcardToDoneWorks);
            db.SaveChanges();
            return RedirectToAction("DoneWorksIndex", new { jobCardId = jobcardId });
        }


        //Maintenance monitorng
        [OperationFilter]
        public ActionResult MaintenanceMonitorngIndex(SearchModelTQ searchModel)
        {
            ViewBag.Ope = true;

            //Check for maintenance history
            UniversalMethods universalMethods = new UniversalMethods();
            universalMethods.MaintenanceFirstCheckUp();
            universalMethods.MaintenanceSecondCheckUp();

            //Converting kilometers
            //Main km
            //min
            decimal? mainKmMin = null;
            decimal mainKmMinTemp;
            string mainKmMinTempDot;

            if (searchModel.MainKmMin != null && searchModel.MainKmMin.Contains("."))
            {
                mainKmMinTempDot = searchModel.MainKmMin.Replace('.', ',');
            }
            else
            {
                mainKmMinTempDot = searchModel.MainKmMin;
            }
            if (Decimal.TryParse(mainKmMinTempDot, out mainKmMinTemp))
            {
                mainKmMin = mainKmMinTemp;
            }

            //max
            decimal? mainKmMax = null;
            decimal mainKmMaxTemp;
            string mainKmMaxTempDot;

            if (searchModel.MainKmMax != null && searchModel.MainKmMax.Contains("."))
            {
                mainKmMaxTempDot = searchModel.MainKmMax.Replace('.', ',');
            }
            else
            {
                mainKmMaxTempDot = searchModel.MainKmMax;
            }
            if (Decimal.TryParse(mainKmMaxTempDot, out mainKmMaxTemp))
            {
                mainKmMax = mainKmMaxTemp;
            }
            //End

            //Remaining km
            //min
            decimal? remainingKmMin = null;
            decimal remainingKmMinTemp;
            string remainingKmMinTempDot;

            if (searchModel.RemainingKmMin != null && searchModel.RemainingKmMin.Contains("."))
            {
                remainingKmMinTempDot = searchModel.RemainingKmMin.Replace('.', ',');
            }
            else
            {
                remainingKmMinTempDot = searchModel.RemainingKmMin;
            }
            if (Decimal.TryParse(remainingKmMinTempDot, out remainingKmMinTemp))
            {
                remainingKmMin = remainingKmMinTemp;
            }

            //max
            decimal? remainingKmMax = null;
            decimal remainingKmMaxTemp;
            string remainingKmMaxTempDot;

            if (searchModel.RemainingKmMax != null && searchModel.RemainingKmMax.Contains("."))
            {
                remainingKmMaxTempDot = searchModel.RemainingKmMax.Replace('.', ',');
            }
            else
            {
                remainingKmMaxTempDot = searchModel.RemainingKmMax;
            }
            if (Decimal.TryParse(remainingKmMaxTempDot, out remainingKmMaxTemp))
            {
                remainingKmMax = remainingKmMaxTemp;
            }
            //End

            ViewHome model = new ViewHome
            {
                InitialControlSchedule = db.InitialControlSchedule.ToList(),
                MaintenanceHistory = db.MaintenanceHistory.Where(m => m.MaintenanceStatus == null).ToList(),
                MaintenanceType = db.MaintenanceType.ToList(),
                JobCards = db.JobCards.Where(j => j.IsOpen == true).ToList(),
                Vehicles = db.Vehicles.ToList(),
                SearchModelTQ = searchModel         
            };
            List<int> vehiclesYellow = new List<int>();
            List<int> vehiclesPink = new List<int>();
            List<int> vehiclesRed = new List<int>();
            int? MaintenanceCheckupStep = db.Settings.FirstOrDefault(s => s.Name == "MaintenanceStepValue").Value;
            int? MaintenanceCheckupStep2 = db.Settings.FirstOrDefault(s => s.Name == "MaintenanceStep2Value").Value;

            List<InitialControlSchedule> ınitialControlSchedules = db.InitialControlSchedule.Where(i => i.EnterKilometer != null).GroupBy(x => x.VehicleId, (key, g) => g.OrderByDescending(e => e.EnterTime).FirstOrDefault()).ToList();

            foreach (var item in ınitialControlSchedules)
            {
                //Yellow notification
                foreach (var item2 in db.MaintenanceHistory.Where(m => m.MaintenanceStatus == null).ToList())
                {
                    if (item.VehicleId==item2.InitialControlSchedule.VehicleId && item.EnterKilometer < item2.MaintenanceType.MaintenanceValue && item.EnterKilometer > (item2.MaintenanceType.MaintenanceValue - MaintenanceCheckupStep))
                    {
                        vehiclesYellow.Add((int)item.VehicleId);
                    }
                }

                //Pink notification
                foreach (var item2 in db.MaintenanceHistory.Where(m => m.MaintenanceStatus == null).ToList())
                {
                    if (item.VehicleId == item2.InitialControlSchedule.VehicleId && item.EnterKilometer > item2.MaintenanceType.MaintenanceValue && item.EnterKilometer < (item2.MaintenanceType.MaintenanceValue + MaintenanceCheckupStep2))
                    {
                        vehiclesPink.Add((int)item.VehicleId);
                    }
                }

                //Red notification
                foreach (var item2 in db.MaintenanceHistory.Where(m => m.MaintenanceStatus == null).ToList())
                {
                    if (item.VehicleId == item2.InitialControlSchedule.VehicleId && item.EnterKilometer > (item2.MaintenanceType.MaintenanceValue + MaintenanceCheckupStep2))
                    {
                        vehiclesRed.Add((int)item.VehicleId);
                    }
                }
            }

            TempData["MentionVehicleYellow"] = vehiclesYellow;
            TempData["MentionVehiclePink"] = vehiclesPink;
            TempData["MentionVehicleRed"] = vehiclesRed;

            //Sorted vehicles
            List<VehiclesSortedByMainKm> vehiclesSortedByMainKms = new List<VehiclesSortedByMainKm>();
            int? MaintenanceStepValue = db.Settings.FirstOrDefault(s => s.Name == "MaintenanceStepValue").Value;
            int? MaintenanceStep2Value = db.Settings.FirstOrDefault(s => s.Name == "MaintenanceStep2Value").Value;
            
            foreach (var item in db.Vehicles.ToList())
            {
                VehiclesSortedByMainKm newVehiclesSortedByMainKms = new VehiclesSortedByMainKm();
                newVehiclesSortedByMainKms.Id = item.Id;
                newVehiclesSortedByMainKms.BrandId = item.BrandId;
                newVehiclesSortedByMainKms.VehicleId = item.Id;
                newVehiclesSortedByMainKms.VehicleCode = item.VehicleCode;
                newVehiclesSortedByMainKms.Number = item.Number;
                newVehiclesSortedByMainKms.ReleaseYear = item.ReleaseYear;
                newVehiclesSortedByMainKms.Capacity = item.Capacity;
                newVehiclesSortedByMainKms.NumberOfSeats = item.NumberOfSeats;
                newVehiclesSortedByMainKms.RegistrationCertificationSeries = item.RegistrationCertificationSeries;
                newVehiclesSortedByMainKms.RegistrationCertificationNumber = item.RegistrationCertificationNumber;
                newVehiclesSortedByMainKms.ChassisNumber = item.ChassisNumber;
                newVehiclesSortedByMainKms.EngineCode = item.EngineCode;
                newVehiclesSortedByMainKms.KmToMaint = db.InitialControlSchedule.Where(k => k.VehicleId == item.Id).OrderByDescending(j => j.VehicleId).FirstOrDefault()!=null ? db.InitialControlSchedule.Where(k => k.VehicleId == item.Id).OrderByDescending(j => j.EnterTime).FirstOrDefault().EnterKilometer : null;

                MaintenanceHistory MaintenanceDone = db.MaintenanceHistory.OrderByDescending(m=>m.AddedDate).FirstOrDefault(m => m.InitialControlSchedule.VehicleId == item.Id);
                if (MaintenanceDone!=null && MaintenanceDone.MaintenanceStatus == null)
                {
                    newVehiclesSortedByMainKms.RemainingKmToMaint = MaintenanceDone.MaintenanceType.MaintenanceValue - newVehiclesSortedByMainKms.KmToMaint;
                }
                else if(MaintenanceDone != null && MaintenanceDone.MaintenanceStatus == true)
                {
                    newVehiclesSortedByMainKms.RemainingKmToMaint = 30000 + MaintenanceDone.MaintenanceType.MaintenanceValue - newVehiclesSortedByMainKms.KmToMaint;
                }
                else
                {
                    newVehiclesSortedByMainKms.RemainingKmToMaint = newVehiclesSortedByMainKms.KmToMaint != null? db.MaintenanceType.FirstOrDefault(m => m.MaintenanceValue > newVehiclesSortedByMainKms.KmToMaint && (m.MaintenanceValue - 30000) < newVehiclesSortedByMainKms.KmToMaint).MaintenanceValue - newVehiclesSortedByMainKms.KmToMaint: null;
                }
                vehiclesSortedByMainKms.Add(newVehiclesSortedByMainKms);
            }

            model.VehiclesSortedByMainKm = vehiclesSortedByMainKms.Where(i => (searchModel.VehicleId!=null? i.VehicleId==searchModel.VehicleId : true) &&
                                                                              (searchModel.MainKmMin != null ? i.KmToMaint >= mainKmMin : true) &&
                                                                              (searchModel.MainKmMax != null ? i.KmToMaint <= mainKmMax : true) &&

                                                                              (searchModel.RemainingKmMin != null ? i.RemainingKmToMaint >= remainingKmMin : true) &&
                                                                              (searchModel.RemainingKmMax != null ? i.RemainingKmToMaint <= remainingKmMax : true)
                                                                              ).OrderBy(v=>v.RemainingKmToMaint).ToList();

            return View(model);
        }

        [OperationFilter]
        public ActionResult MaintenanceHistory(int? vehicleId)
        {
            ViewBag.Ope = true;
            ViewBag.MaintenanceCheckupStep = db.Settings.FirstOrDefault(s => s.Name == "MaintenanceStepValue").Value;

            ViewHome model = new ViewHome();

            model.MaintenanceHistory = db.MaintenanceHistory.Where(m => (vehicleId != null ? m.InitialControlSchedule.Vehicles.Id == vehicleId : true) /*&&*/
                                                    //(JobCardId != null ? j.Id == JobCardId : true) &&
                                                    ).OrderByDescending(m=>m.AddedDate).ToList();

            return View(model);
        }




        //SOCAR Fuel Info
        [OperationFilter]
        public ActionResult InitContSOCARIndex(SearchModelInitCont searchModel)
        {
            ViewBag.Ope = true;
            ViewHome model = new ViewHome
            {
                Vehicles = db.Vehicles.ToList(),
                Routes = db.Routes.ToList(),
                Employees = db.Employees.ToList(),
            };

            //Variables
            //Dates
            DateTime? leaveDateStart = null;
            DateTime? leaveDateEnd = null;

            //Hours
            TimeSpan? leaveHourStart = null;
            TimeSpan? leaveHourEnd = null;

            //Filtering
            //Leaving Date
            if (!string.IsNullOrEmpty(searchModel.LeaveDateStart))
            {
                leaveDateStart = DateTime.ParseExact(searchModel.LeaveDateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveDateEnd))
            {
                leaveDateEnd = DateTime.ParseExact(searchModel.LeaveDateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            
            //Leaving Hour
            if (!string.IsNullOrEmpty(searchModel.LeaveHourStart))
            {
                leaveHourStart = TimeSpan.Parse(searchModel.LeaveHourStart);
            }
            if (!string.IsNullOrEmpty(searchModel.LeaveHourEnd))
            {
                leaveHourEnd = TimeSpan.Parse(searchModel.LeaveHourEnd);
            }

            //SOCAR fuel
            //min
            decimal? SOCARFuelMin = null;
            decimal SOCARFuelMinTemp;
            string SOCARFuelMinTempDot;

            if (searchModel.SOCARFuelMin != null && searchModel.SOCARFuelMin.Contains("."))
            {
                SOCARFuelMinTempDot = searchModel.SOCARFuelMin.Replace('.', ',');
            }
            else
            {
                SOCARFuelMinTempDot = searchModel.SOCARFuelMin;
            }
            if (Decimal.TryParse(SOCARFuelMinTempDot, out SOCARFuelMinTemp))
            {
                SOCARFuelMin = SOCARFuelMinTemp;
            }

            //max
            decimal? SOCARFuelMax = null;
            decimal SOCARFuelMaxTemp;
            string SOCARFuelMaxTempDot;

            if (searchModel.SOCARFuelMax != null && searchModel.SOCARFuelMax.Contains("."))
            {
                SOCARFuelMaxTempDot = searchModel.SOCARFuelMax.Replace('.', ',');
            }
            else
            {
                SOCARFuelMaxTempDot = searchModel.SOCARFuelMax;
            }
            if (Decimal.TryParse(SOCARFuelMaxTempDot, out SOCARFuelMaxTemp))
            {
                SOCARFuelMax = SOCARFuelMaxTemp;
            }
            //End

            model.InitialControlSchedule = db.InitialControlSchedule.Where(i => (i.IsOpen == false) &&
                                                                                (searchModel.VehicleId != null ? i.VehicleId == searchModel.VehicleId : true) &&
                                                                                (searchModel.RouteId != null ? i.RouteId == searchModel.RouteId : true) &&

                                                                                (searchModel.FirstDriverId != null ? i.FirstDriverId == searchModel.FirstDriverId : true) &&
                                                                                (searchModel.SecondDriverId != null ? i.SecondDriverId == searchModel.SecondDriverId : true) &&

                                                                                (searchModel.LeaveDateStart != null ? DbFunctions.TruncateTime(i.LeavingTime) >= leaveDateStart : true) &&
                                                                                (searchModel.LeaveDateEnd != null ? DbFunctions.TruncateTime(i.LeavingTime) <= leaveDateEnd : true) &&

                                                                                (searchModel.SOCARFuelMin != null ? i.FuelSOCAR.FirstOrDefault().FuelAmount1 >= SOCARFuelMin : true) &&
                                                                                (searchModel.LeaveFuelMax != null ? i.FuelSOCAR.FirstOrDefault().FuelAmount1 <= SOCARFuelMax : true) &&

                                                                                (searchModel.LeaveHourStart != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) >= leaveHourStart : true) &&
                                                                                (searchModel.LeaveHourEnd != null ? DbFunctions.CreateTime(i.LeavingTime.Value.Hour, i.LeavingTime.Value.Minute, 0) <= leaveHourEnd : true)
                                                                                
                                                                                ).OrderByDescending(i => i.LeavingTime).ToList();

            TempData["SortedInitContSOCAR"] = model;

            model.SearchModelInitCont = searchModel;
            return View(model);
        }

        [OperationFilter]
        public ActionResult InitContSOCARAdd(int id)
        {
            ViewBag.Ope = true;
            if (db.FuelSOCAR.FirstOrDefault(s => s.InitContId == id)!=null && db.FuelSOCAR.FirstOrDefault(s => s.InitContId == id).AddedUserId != null)
            {
                Session["SOCARAdded"] = true;
                return RedirectToAction("InitContSOCARIndex");
            }
            ViewBag.InitCont = db.InitialControlSchedule.Find(id);           
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult InitContSOCARAdd(SocarModel socar)
        {
            ViewBag.Ope = true;            
            FuelSOCAR FuelSocar = new FuelSOCAR();
            FuelSocar.InitContId = socar.InitContId;
            FuelSocar.AddedDate = DateTime.Now;
            FuelSocar.AddedUserId = (int)Session["UserId"];

            if (socar.DateSOCAR != null)
            {
                DateTime dateSOCAR = DateTime.ParseExact(socar.DateSOCAR, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                FuelSocar.DateSOCAR = dateSOCAR;
            }
            else
            {
                FuelSocar.DateSOCAR = null;
            }


            //SOCAR Fuel 1
            decimal? kmDotFuel1 = null;
            decimal kmTempFuel1;
            string kmTempDotFuel1;

            if (socar.FuelAmount1 != null && socar.FuelAmount1.Contains("."))
            {
                kmTempDotFuel1 = socar.FuelAmount1.Replace('.', ',');
            }
            else if (socar.FuelAmount1 != null)
            {
                kmTempDotFuel1 = socar.FuelAmount1;
            }
            else
            {
                kmTempDotFuel1 = null;
            }

            if (kmTempDotFuel1 != null && decimal.TryParse(kmTempDotFuel1, out kmTempFuel1))
            {
                kmDotFuel1 = decimal.Parse(kmTempDotFuel1);
            }
            //End

            //SOCAR Fuel 2
            decimal? kmDotFuel2 = null;
            decimal kmTempFuel2;
            string kmTempDotFuel2;

            if (socar.FuelAmount2 != null && socar.FuelAmount2.Contains("."))
            {
                kmTempDotFuel2 = socar.FuelAmount2.Replace('.', ',');
            }
            else if (socar.FuelAmount2 != null)
            {
                kmTempDotFuel2 = socar.FuelAmount2;
            }
            else
            {
                kmTempDotFuel2 = null;
            }

            if (kmTempDotFuel2 != null && decimal.TryParse(kmTempDotFuel2, out kmTempFuel2))
            {
                kmDotFuel2 = decimal.Parse(kmTempDotFuel2);
            }
            //End

            //SOCAR Fuel 3
            decimal? kmDotFuel3 = null;
            decimal kmTempFuel3;
            string kmTempDotFuel3;

            if (socar.FuelAmount3 != null && socar.FuelAmount3.Contains("."))
            {
                kmTempDotFuel3 = socar.FuelAmount3.Replace('.', ',');
            }
            else if (socar.FuelAmount3 != null)
            {
                kmTempDotFuel3 = socar.FuelAmount3;
            }
            else
            {
                kmTempDotFuel3 = null;
            }

            if (kmTempDotFuel3 != null && decimal.TryParse(kmTempDotFuel3, out kmTempFuel3))
            {
                kmDotFuel3 = decimal.Parse(kmTempDotFuel3);
            }
            //End

            FuelSocar.FuelAmount1 = kmDotFuel1;
            FuelSocar.FuelAmount2 = kmDotFuel2;
            FuelSocar.FuelAmount3 = kmDotFuel3;


            //SOCAR Fuel time 1
            TimeSpan fuelTime1;
            if (socar.FuelTime1 != null && TimeSpan.TryParse(socar.FuelTime1, out fuelTime1))
            {
                FuelSocar.FuelTime1 = TimeSpan.Parse(socar.FuelTime1);
            }
            else
            {
                FuelSocar.FuelTime1 = null;
            }
            //End    

            //SOCAR Fuel time 2
            TimeSpan fuelTime2;
            if (socar.FuelTime2 != null && TimeSpan.TryParse(socar.FuelTime2, out fuelTime2))
            {
                FuelSocar.FuelTime2 = TimeSpan.Parse(socar.FuelTime2);
            }
            else
            {
                FuelSocar.FuelTime2 = null;
            }
            //End    

            //SOCAR Fuel time 3
            TimeSpan fuelTime3;
            if (socar.FuelTime3 != null && TimeSpan.TryParse(socar.FuelTime3, out fuelTime3))
            {
                FuelSocar.FuelTime3 = TimeSpan.Parse(socar.FuelTime3);
            }
            else
            {
                FuelSocar.FuelTime3 = null;
            }
            //End    

            db.FuelSOCAR.Add(FuelSocar);
            db.SaveChanges();

            return RedirectToAction("InitContSOCARIndex");
        }

        [OperationFilter]
        public ActionResult InitContSOCARUpdate(int id)
        {
            ViewBag.Ope = true;
            if (db.FuelSOCAR.FirstOrDefault(s => s.InitContId == id) == null)
            {
                Session["SOCARUpdate"] = true;
                return RedirectToAction("InitContSOCARIndex");
            }

            ViewBag.InitCont = db.InitialControlSchedule.Find(id);
            ViewBag.InitContId = id;
            ViewHome model = new ViewHome();
            model.FuelSOCAR = db.FuelSOCAR.ToList();
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult InitContSOCARUpdate(SocarModel socar)
        {
            ViewBag.Ope = true;
            FuelSOCAR FuelSocar = db.FuelSOCAR.Find(socar.Id);

            if (socar.DateSOCAR != null)
            {
                DateTime dateSOCAR = DateTime.ParseExact(socar.DateSOCAR, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                FuelSocar.DateSOCAR = dateSOCAR;
            }
            else
            {
                FuelSocar.DateSOCAR = null;
            }
            

            //SOCAR Fuel 1
            decimal? kmDotFuel1 = null;
            decimal kmTempFuel1;
            string kmTempDotFuel1;

            if (socar.FuelAmount1 != null && socar.FuelAmount1.Contains("."))
            {
                kmTempDotFuel1 = socar.FuelAmount1.Replace('.', ',');
            }
            else if (socar.FuelAmount1 != null)
            {
                kmTempDotFuel1 = socar.FuelAmount1;
            }
            else
            {
                kmTempDotFuel1 = null;
            }

            if (kmTempDotFuel1 != null && decimal.TryParse(kmTempDotFuel1, out kmTempFuel1))
            {
                kmDotFuel1 = decimal.Parse(kmTempDotFuel1);
            }
            //End

            //SOCAR Fuel 2
            decimal? kmDotFuel2 = null;
            decimal kmTempFuel2;
            string kmTempDotFuel2;

            if (socar.FuelAmount2 != null && socar.FuelAmount2.Contains("."))
            {
                kmTempDotFuel2 = socar.FuelAmount2.Replace('.', ',');
            }
            else if (socar.FuelAmount2 != null)
            {
                kmTempDotFuel2 = socar.FuelAmount2;
            }
            else
            {
                kmTempDotFuel2 = null;
            }

            if (kmTempDotFuel2 != null && decimal.TryParse(kmTempDotFuel2, out kmTempFuel2))
            {
                kmDotFuel2 = decimal.Parse(kmTempDotFuel2);
            }
            //End

            //SOCAR Fuel 3
            decimal? kmDotFuel3 = null;
            decimal kmTempFuel3;
            string kmTempDotFuel3;

            if (socar.FuelAmount3 != null && socar.FuelAmount3.Contains("."))
            {
                kmTempDotFuel3 = socar.FuelAmount3.Replace('.', ',');
            }
            else if (socar.FuelAmount3 != null)
            {
                kmTempDotFuel3 = socar.FuelAmount3;
            }
            else
            {
                kmTempDotFuel3 = null;
            }

            if (kmTempDotFuel3 != null && decimal.TryParse(kmTempDotFuel3, out kmTempFuel3))
            {
                kmDotFuel3 = decimal.Parse(kmTempDotFuel3);
            }
            //End

            if (kmDotFuel1!=null)
            {
                FuelSocar.FuelAmount1 = kmDotFuel1;
            }
            else
            {
                FuelSocar.FuelAmount1 = null;
            }

            if (kmDotFuel2 != null)
            {
                FuelSocar.FuelAmount2 = kmDotFuel2;
            }
            else
            {
                FuelSocar.FuelAmount2 = null;
            }

            if (kmDotFuel3 != null)
            {
                FuelSocar.FuelAmount3 = kmDotFuel3;
            }
            else
            {
                FuelSocar.FuelAmount3 = null;
            }


            //SOCAR Fuel time 1
            TimeSpan fuelTime1 ;
            if (socar.FuelTime1!=null && TimeSpan.TryParse(socar.FuelTime1, out fuelTime1))
            {
                FuelSocar.FuelTime1 = TimeSpan.Parse(socar.FuelTime1);
            }
            else
            {
                FuelSocar.FuelTime1 = null;
            }
            //End    

            //SOCAR Fuel time 2
            TimeSpan fuelTime2;
            if (socar.FuelTime2 != null && TimeSpan.TryParse(socar.FuelTime2, out fuelTime2))
            {
                FuelSocar.FuelTime2 = TimeSpan.Parse(socar.FuelTime2);
            }
            else
            {
                FuelSocar.FuelTime2 = null;
            }
            //End    

            //SOCAR Fuel time 3
            TimeSpan fuelTime3;
            if (socar.FuelTime3 != null && TimeSpan.TryParse(socar.FuelTime3, out fuelTime3))
            {
                FuelSocar.FuelTime3 = TimeSpan.Parse(socar.FuelTime3);
            }
            else
            {
                FuelSocar.FuelTime3 = null;
            }
            //End    

            db.Entry(FuelSocar).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("InitContSOCARIndex");
        }

        //Rufat
        public void ExportInitContSocar()
        {
            ViewHome model = new ViewHome();
            model = (ViewHome)TempData["SortedInitContSOCAR"];

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Socar list");

            ws.Range("B2:M2").Merge();
            ws.Cell("B2").Value = "Socar yanacaq məlumatlarının müqayisəsi";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "NV Nömrəsi";
            ws.Cell("D3").Value = "Xətt";
            ws.Cell("E3").Value = "Çıxış Tarixi";
            ws.Cell("F3").Value = "Çıxış Saatı";
            ws.Cell("G3").Value = "Dolduma Tarixi";
            ws.Cell("H3").Value = "Yanacaq SOCAR-1";
            ws.Cell("I3").Value = "Dolduma saatı-1";
            ws.Cell("J3").Value = "Yanacaq SOCAR-2	";
            ws.Cell("K3").Value = "Dolduma saatı-2";
            ws.Cell("L3").Value = "Yanacaq SOCAR-3";
            ws.Cell("M3").Value = "Dolduma saatı-3";


            ws.Cell("B2").Style.Font.SetBold();
            ws.Cell("B2").Style.Font.FontSize = 14;
            ws.Cell("B2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("B2").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("B2").Style.Alignment.WrapText = true;


            ws.Cell("B3").Style.Font.SetBold();
            ws.Cell("B3").Style.Font.FontColor = XLColor.White;
            ws.Cell("B3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("B3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("B3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("B3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Alignment.WrapText = true;

            ws.Cell("C3").Style.Font.SetBold();
            ws.Cell("C3").Style.Font.FontColor = XLColor.White;
            ws.Cell("C3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("C3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("C3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("C3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Alignment.WrapText = true;

            ws.Cell("D3").Style.Font.SetBold();
            ws.Cell("D3").Style.Font.FontColor = XLColor.White;
            ws.Cell("D3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("D3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("D3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("D3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Alignment.WrapText = true;

            ws.Cell("E3").Style.Font.SetBold();
            ws.Cell("E3").Style.Font.FontColor = XLColor.White;
            ws.Cell("E3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("E3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("E3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("E3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Alignment.WrapText = true;

            ws.Cell("F3").Style.Font.SetBold();
            ws.Cell("F3").Style.Font.FontColor = XLColor.White;
            ws.Cell("F3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("F3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("F3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("F3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Alignment.WrapText = true;

            ws.Cell("G3").Style.Font.SetBold();
            ws.Cell("G3").Style.Font.FontColor = XLColor.White;
            ws.Cell("G3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("G3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("G3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("G3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Alignment.WrapText = true;

            ws.Cell("H3").Style.Font.SetBold();
            ws.Cell("H3").Style.Font.FontColor = XLColor.White;
            ws.Cell("H3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("H3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("H3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("H3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Alignment.WrapText = true;

            ws.Cell("I3").Style.Font.SetBold();
            ws.Cell("I3").Style.Font.FontColor = XLColor.White;
            ws.Cell("I3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("I3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("I3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("I3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Alignment.WrapText = true;

            ws.Cell("J3").Style.Font.SetBold();
            ws.Cell("J3").Style.Font.FontColor = XLColor.White;
            ws.Cell("J3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("J3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("J3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("J3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Alignment.WrapText = true;

            ws.Cell("K3").Style.Font.SetBold();
            ws.Cell("K3").Style.Font.FontColor = XLColor.White;
            ws.Cell("K3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("K3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("K3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("K3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("K3").Style.Alignment.WrapText = true;

            ws.Cell("L3").Style.Font.SetBold();
            ws.Cell("L3").Style.Font.FontColor = XLColor.White;
            ws.Cell("L3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("L3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("L3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("L3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("L3").Style.Alignment.WrapText = true;

            ws.Cell("M3").Style.Font.SetBold();
            ws.Cell("M3").Style.Font.FontColor = XLColor.White;
            ws.Cell("M3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("M3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("M3").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("M3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Cell("M3").Style.Alignment.WrapText = true;



            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 11;
            ws.Column("D").Width = 9;
            ws.Column("E").Width = 10;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 10;
            ws.Column("H").Width = 10;
            ws.Column("I").Width = 10;
            ws.Column("J").Width = 10;
            ws.Column("K").Width = 10;
            ws.Column("L").Width = 10;
            ws.Column("M").Width = 10;



            int i = 4;
            foreach (var item in model.InitialControlSchedule)
            {
                //S/s
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                //NV Nömrəsi
                ws.Cell("C" + i).Value = (item.VehicleId == null ? "" : item.Vehicles.Number);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;

                //Xətt
                ws.Cell("D" + i).Value = (item.RouteId == null ? "" : item.Routes.Number);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                //Çıxış tarixi
                ws.Cell("E" + i).Value = (item.LeavingTime == null ? "" : item.LeavingTime.Value.ToString("dd.MM.yyyy"));
                ws.Cell("E" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                //Çıxış saatı
                ws.Cell("F" + i).Value = (item.LeavingTime == null ? "" : item.LeavingTime.Value.ToString("HH:mm"));
                ws.Cell("F" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("F" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("F" + i).Style.Alignment.WrapText = true;

                //Dolduma Tarixi
                ws.Cell("G" + i).Value = (item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) != null && item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).DateSOCAR != null) ? item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).DateSOCAR.Value.ToString("dd.MM.yyyy") : "";
                ws.Cell("G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("G" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("G" + i).Style.Alignment.WrapText = true;

                //Yanacaq SOCAR-1
                ws.Cell("H" + i).Value = (item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) != null && item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount1 != null) ? item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount1.Value.ToString("#.##") : "";
                ws.Cell("H" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("H" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("H" + i).Style.Alignment.WrapText = true;

                //Dolduma saatı-1
                ws.Cell("I" + i).Value = ((item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) != null && item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelTime1 != null) ? item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelTime1.Value.ToString() : "");
                ws.Cell("I" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("I" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("I" + i).Style.Alignment.WrapText = true;

                //Yanacaq SOCAR-2
                ws.Cell("J" + i).Value = ((item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) != null && item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount2 != null) ? item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount2.Value.ToString("#.##") : "");
                ws.Cell("J" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("J" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("J" + i).Style.Alignment.WrapText = true;

                //Dolduma saatı-2
                ws.Cell("K" + i).Value = (item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) != null && item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelTime2 != null) ? item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelTime2.Value.ToString() : "";
                ws.Cell("K" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("K" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("K" + i).Style.Alignment.WrapText = true;

                //Yanacaq SOCAR-3
                ws.Cell("L" + i).Value = ((item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) != null && item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount3 != null) ? item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelAmount3.Value.ToString("#.##") : "");
                ws.Cell("L" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("L" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("L" + i).Style.Alignment.WrapText = true;

                //Dolduma saatı-3
                ws.Cell("M" + i).Value = ((item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id) != null && item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelTime3 != null) ? item.FuelSOCAR.FirstOrDefault(s => s.InitContId == item.Id).FuelTime3.Value.ToString() : "");
                ws.Cell("M" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("M" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("M" + i).Style.Alignment.WrapText = true;


                i++;


            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"SocarList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }
        //Rufat-end


        //Input validation checks with AJAX
        //Check for vehicle in InitCont
        public JsonResult InitCreateCheckVehicle(int id)
        {
            string result = "unset";
            foreach (var item in db.InitialControlSchedule.Where(i=>i.IsOpen==true).ToList())
            {
                if (item.VehicleId==id)
                {
                    result = "failedInitCont";
                    goto Return;
                }
            }

            foreach (var item in db.JobCards.Where(i => i.IsOpen == true && i.InWaitRoute==null).ToList())
            {                
                if(item.CheckUpCard.InitContId != null && item.CheckUpCard.InitialControlSchedule.VehicleId != null && item.CheckUpCard.InitialControlSchedule.VehicleId == id)
                {
                    result = "failedCheckup";
                    goto Return;
                }
            }

            Return:
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Check for vehicle in Checkup
        public JsonResult CheckupCreateCheckVehicle(int id)
        {
            AJAXResponseModel result = new AJAXResponseModel();

            result.status = "unset";

            //Check for if is in a route
            InitialControlSchedule ınitialControlSchedule = db.InitialControlSchedule.FirstOrDefault(i => i.IsOpen == true && i.VehicleId == id);
            if (ınitialControlSchedule!=null)
            {
                result.status = "failedInitCont";
                goto Return;
            }

            //Check for if there is open jobcard
            JobCards jobCards = db.JobCards.FirstOrDefault(j=>j.IsOpen==true && j.CheckUpCard.InitialControlSchedule.VehicleId != null && j.CheckUpCard.InitialControlSchedule.VehicleId == id);
            if (jobCards!=null)
            {
                result.status = "failedCheckup";
                goto Return;
            }

            //Check for if maintenance needed
            MaintenanceHistory maintenanceHistory = db.MaintenanceHistory.FirstOrDefault(m => m.MaintenanceStatus == null && m.JobCardId == null && m.InitialControlSchedule.VehicleId == id);
            if (maintenanceHistory != null)
            {
                result.maintenanceStatus = "MaintenanceTrue";
                result.maintenanceData = maintenanceHistory.MaintenanceType.Name + "-" + maintenanceHistory.MaintenanceType.MaintenanceValue;
            }

            Return:
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Check for vehicle leave km
        public JsonResult InitCreateCheckKM(int id, string km)
        {
            string result = "unset";
            int Km = 0;
            if (id!=0 && int.TryParse(km, out Km))
            {
               ViewBag.InitCont= db.InitialControlSchedule.Where(i => i.VehicleId == id).OrderByDescending(i => i.EnterKilometer).ToList();

                foreach (var item in ViewBag.InitCont)
                {
                    if (item.EnterKilometer > Km)
                    {
                        result = item.EnterKilometer.ToString("#.##");
                        break;
                    }
                }
            }

            if (id==0)
            {
                result = "failedId";
            }
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Check for vehicle enter km
        public JsonResult InitCreateCheckEnterKM(int id, string km)
        {
            AJAXResponseModel result = new AJAXResponseModel();
            InitialControlSchedule InitCont = db.InitialControlSchedule.Find(id);

            result.status = "unset";
            int Km = 0;
            decimal? routeMaxKm = db.RouteDailyKM.FirstOrDefault(k=>k.RouteId==InitCont.RouteId).KmLimit;
            if (id != null && int.TryParse(km, out Km))
            {
                if (InitCont.LeavingKilometer > Km)
                {
                    result.status = "wrongKM";
                    result.currentKm = InitCont.LeavingKilometer.Value.ToString("#.##");
                    goto Return;
                }
            }

            if (id != null && int.TryParse(km, out Km))
            {
                if (routeMaxKm!=null && (Km - InitCont.LeavingKilometer)> routeMaxKm)
                {
                    result.status = "overLimit";
                    result.kmLimit = routeMaxKm.Value.ToString("#.##");
                    goto Return;
                }
            }

            Return:
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Check for vehicle enter fuel
        public JsonResult InitCreateCheckEnterFuel(int id, string litr)
        {
            AJAXResponseModel result = new AJAXResponseModel();
            InitialControlSchedule InitCont = db.InitialControlSchedule.Find(id);

            result.status = "unset";
            int Litr = 0;
            int Id = 0;
            decimal? routeMaxKm = db.RouteDailyKM.FirstOrDefault(k => k.RouteId == InitCont.RouteId).KmLimit;
            if (id != null && int.TryParse(litr, out Litr) && int.TryParse(id.ToString(), out Id))
            {
                if (InitCont.LeavingFuel < Litr)
                {
                    result.status = "wrongLitr";
                    result.leaveFuel = InitCont.LeavingFuel.Value.ToString("#.##");
                    goto Return;
                }
            }

            Return:
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}