using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Filters;
using BBService.Models;
using ClosedXML.Excel;

namespace BBService.Controllers
{
    [LogOut]
    public class MonipulationsController : Controller
    {
        // GET: Monipulations
        BBServiceEntities db = new BBServiceEntities();

        //Binding Mechanics
        [OperationFilter]
        public ActionResult BindMechIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                MechanicBinding = db.MechanicBinding.Where(b=>b.Status==true).ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult BindMechCreate()
        {
            ViewBag.Sys = true;

            ViewBag.SenMech = db.Employees.Where(e => e.PositionId == 4).ToList();
            ViewBag.Mech = db.Employees.Where(e => e.PositionId == 3).ToList();
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BindMechCreate(MechanicBinding mech)
        {
            mech.Status = true;
            mech.AddedDate = DateTime.Now;
            db.MechanicBinding.Add(mech);
            db.SaveChanges();
            return RedirectToAction("BindMechIndex");
        }

        [OperationFilter]
        public ActionResult BindMechUpdate(int id)
        {
            ViewBag.Sys = true;
            MechanicBinding bind = db.MechanicBinding.FirstOrDefault(b=>b.Id==id);

            ViewBag.SenMech = db.Employees.Where(e => e.PositionId == 4).ToList();
            ViewBag.Mech = db.Employees.Where(e => e.PositionId == 3).ToList();

            ViewBag.SenMechSingle = db.Employees.FirstOrDefault(e => e.Id == bind.SeniorRecMechId);
            ViewBag.MechSingle = db.Employees.FirstOrDefault(e => e.Id == bind.RecMechId);
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BindMechUpdate(MechanicBinding mech)
        {
            if (mech == null)
            {
                return RedirectToAction("BindMechIndex");

            }
            if (mech.SeniorRecMechId == 0)
            {
                mech.SeniorRecMechId = null;
            }
            if (mech.RecMechId == 0)
            {
                mech.RecMechId = null;
            }

            if (ModelState.IsValid)
            {
                db.Entry(mech).State = EntityState.Modified;
                db.Entry(mech).Property(x => x.AddedDate).IsModified = false;
                db.Entry(mech).Property(x => x.Status).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("BindMechIndex");
            }
            return RedirectToAction("BindMechIndex");
        }

        //[HttpPost]
        [OperationFilter]
        public ActionResult BindMechUpdateStatus(int id)
        {
            MechanicBinding mech = db.MechanicBinding.Find(id);
            if (mech==null)
            {
                return RedirectToAction("BindMechIndex");
            }
            mech.Status = false;

            if (ModelState.IsValid)
            {
                db.Entry(mech).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("BindMechIndex");
            }
            return RedirectToAction("BindMechIndex");
        }

        [OperationFilter]
        public ActionResult BindMechDelete(int id)
        {
            MechanicBinding mech = db.MechanicBinding.Find(id);
            if (mech == null)
            {
                Session["InvalidDelete"] = true;
                return RedirectToAction("BindMechIndex");
            }

            db.MechanicBinding.Remove(mech);
            db.SaveChanges();
            return RedirectToAction("BindMechIndex");
        }

        //Rufat
        public void ExportBindMechData()
        {
            ViewHome model = new ViewHome();
            model.MechanicBinding = db.MechanicBinding.Where(b => b.Status == true).ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("MechanicBinding List");

            ws.Range("B2:D2").Merge();
            ws.Cell("B2").Value = "Mexaniklərin təhkim edilməsi Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Baş Mexanik";
            ws.Cell("D3").Value = "Mexanik";



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




            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 20;


            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;
            ws.Column("D").Width = 40;




            int i = 4;
            foreach (var item in model.MechanicBinding)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.Employees.Surname == null ? "" : item.Employees.Surname) + " " + (item.Employees.Name == null ? "" : item.Employees.Name) + " " + (item.Employees.FutherName == null ? "" : item.Employees.FutherName);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;


                ws.Cell("D" + i).Value = (item.Employees.Surname == null ? "" : item.Employees1.Surname) + " " + (item.Employees.Name == null ? "" : item.Employees1.Name) + " " + (item.Employees.FutherName == null ? "" : item.Employees1.FutherName);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;


                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"MechanicBinding.xlsx\"");

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

        //Binding Maintenance

        [OperationFilter]
        public ActionResult BindMaintenanceIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                MaintenanceType = db.MaintenanceType.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult BindMaintenanceCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BindMaintenanceCreate(MaintenanceType mainType)
        {
            if (mainType == null)
            {
                return RedirectToAction("BindMaintenanceIndex");
            }
            db.MaintenanceType.Add(mainType);
            db.SaveChanges();
            return RedirectToAction("BindMaintenanceIndex");
        }

        [OperationFilter]
        public ActionResult BindMaintenanceUpdate(int id)
        {
            ViewBag.Sys = true;
            MaintenanceType mainType = db.MaintenanceType.Find(id);
            if (mainType == null)
            {
                return RedirectToAction("BindMaintenanceIndex");
            }

            ViewBag.mainType = mainType;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BindMaintenanceUpdate(MaintenanceType mainType)
        {
            if (mainType == null)
            {
                return RedirectToAction("BindMaintenanceIndex");
            }

            if (ModelState.IsValid)
            {
                db.Entry(mainType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("BindMaintenanceIndex");
            }
            return RedirectToAction("BindMaintenanceIndex");
        }

        [OperationFilter]
        public ActionResult BindMaintenanceDelete(int id)
        {
            MaintenanceType mainType = db.MaintenanceType.Find(id);
           
            if (mainType == null)
            {
                return RedirectToAction("BindMaintenanceIndex");
            }

            db.MaintenanceType.Remove(mainType);
            db.SaveChanges();
            return RedirectToAction("BindMaintenanceIndex");
        }

        //Rufat
        public void ExportBindMaintenanceData()
        {
            ViewHome model = new ViewHome();
            model.MaintenanceType = db.MaintenanceType.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("BindMaintenanc List");

            ws.Range("B2:D2").Merge();
            ws.Cell("B2").Value = "Texniki Qulluq limitləri Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Texniki qulluq adı";
            ws.Cell("D3").Value = "Texniki qulluq Limiti";


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


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 20;


            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;
            ws.Column("D").Width = 40;


            int i = 4;
            foreach (var item in model.MaintenanceType)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.Name == null ? "" : item.Name);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;


                ws.Cell("D" + i).Value = (item.MaintenanceValue == null ? null : item.MaintenanceValue);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"BindMaintenanceList.xlsx\"");

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


        //Settings
        [OperationFilter]
        public ActionResult SettingsIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Settings = db.Settings.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult SettingsCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult SettingsCreate(Settings setting)
        {
            if (setting == null)
            {
                return RedirectToAction("SettingsIndex");
            }
            db.Settings.Add(setting);
            db.SaveChanges();
            return RedirectToAction("SettingsIndex");
        }

        [OperationFilter]
        public ActionResult SettingsUpdate(int id)
        {
            ViewBag.Sys = true;
            Settings setting = db.Settings.Find(id);
            if (setting == null)
            {
                return RedirectToAction("SettingsIndex");
            }

            ViewBag.setting = setting;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult SettingsUpdate(Settings setting)
        {
            if (setting == null)
            {
                return RedirectToAction("SettingsIndex");
            }

            if (ModelState.IsValid)
            {
                db.Entry(setting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("SettingsIndex");
            }
            return RedirectToAction("SettingsIndex");
        }

        [OperationFilter]
        public ActionResult SettingsDelete(int id)
        {
            Settings setting = db.Settings.Find(id);

            if (setting == null)
            {
                return RedirectToAction("SettingsIndex");
            }

            db.Settings.Remove(setting);
            db.SaveChanges();
            return RedirectToAction("SettingsIndex");
        }

        //Rufat
        public void ExportSettingsData()
        {
            ViewHome model = new ViewHome();
            model.Settings = db.Settings.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Settings List");

            ws.Range("B2:D2").Merge();
            ws.Cell("B2").Value = "Tənzimləmələr Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Tənzimləmə adı";
            ws.Cell("D3").Value = "Qiymət";


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


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 20;


            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;
            ws.Column("D").Width = 40;


            int i = 4;
            foreach (var item in model.Settings)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.Name == null ? "" : item.Name);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;


                ws.Cell("D" + i).Value = (item.Value == null ? null : item.Value);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"SettingsList.xlsx\"");

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


        //Spare part binding

        [OperationFilter]
        public ActionResult BindSparePartIndex(SearchMainBinding searchMainBinding)
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                MaintenanceType = db.MaintenanceType.ToList(),
                TempWarehouse = db.TempWarehouse.ToList(),
                VehiclesBrand = db.VehiclesBrand.ToList()
            };
            model.WarehouseToMaintenance = db.WarehouseToMaintenance.Where(i => (searchMainBinding.maintenanceTypeId != null ? i.MaintenanceTypeId == searchMainBinding.maintenanceTypeId : true) &&
                                                                                (searchMainBinding.warehouseId != null ? i.WarehouseId == searchMainBinding.warehouseId : true) &&
                                                                                (searchMainBinding.brandId != null ? i.TempWarehouse.VehicleId == searchMainBinding.brandId : true)
                                                                                ).ToList();

            model.SearchMainBinding = searchMainBinding;
            return View(model);
        }

        [OperationFilter]
        public ActionResult BindSparePartCreate()
        {
            ViewBag.Sys = true;
            ViewBag.Maintenance = db.MaintenanceType.ToList();
            ViewBag.VehicleBrand = db.VehiclesBrand.ToList();
            ViewBag.Warehouse = db.TempWarehouse.ToList();
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BindSparePartCreate(WarehouseToMaintenanceModel warehouseToMaintenance)
        {
            if (warehouseToMaintenance == null)
            {
                return RedirectToAction("BindSparePartIndex");
            }

            //Required Quantity
            decimal? QuaDot = null;
            decimal QuaTemp;
            string QuaTempDot;

            if (warehouseToMaintenance.Quantity != null && warehouseToMaintenance.Quantity.Contains("."))
            {
                QuaTempDot = warehouseToMaintenance.Quantity.Replace('.', ',');
            }
            else if (warehouseToMaintenance.Quantity != null)
            {
                QuaTempDot = warehouseToMaintenance.Quantity;
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

            //Limit
            decimal? LimitDot = null;
            decimal LimitTemp;
            string LimitTempDot;

            if (warehouseToMaintenance.NotRequireSPLimit != null && warehouseToMaintenance.NotRequireSPLimit.Contains("."))
            {
                LimitTempDot = warehouseToMaintenance.NotRequireSPLimit.Replace('.', ',');
            }
            else if (warehouseToMaintenance.NotRequireSPLimit != null)
            {
                LimitTempDot = warehouseToMaintenance.NotRequireSPLimit;
            }
            else
            {
                LimitTempDot = null;
            }

            if (LimitTempDot != null && decimal.TryParse(LimitTempDot, out LimitTemp))
            {
                LimitDot = decimal.Parse(LimitTempDot);
            }
            //End

            WarehouseToMaintenance newWarehouseToMaintenance = new WarehouseToMaintenance();
            newWarehouseToMaintenance.MaintenanceTypeId = warehouseToMaintenance.MaintenanceTypeId;
            newWarehouseToMaintenance.WarehouseId = warehouseToMaintenance.WarehouseId;
            newWarehouseToMaintenance.Quantity = QuaDot;
            newWarehouseToMaintenance.NotRequireSPLimit = LimitDot;

            db.WarehouseToMaintenance.Add(newWarehouseToMaintenance);
            db.SaveChanges();
            return RedirectToAction("BindSparePartIndex");
        }

        [OperationFilter]
        public ActionResult BindSparePartUpdate(int? id)
        {
            ViewBag.Sys = true;
            if (id == null)
            {
                return RedirectToAction("BindSparePartIndex");
            }

            WarehouseToMaintenance warehouseToMaintenance = db.WarehouseToMaintenance.Find(id);
            int? VBId = 0;
            if (warehouseToMaintenance.WarehouseId != null && warehouseToMaintenance.TempWarehouse.VehicleId != null)
            {
                VBId = warehouseToMaintenance.TempWarehouse.VehicleId;
            }
            if (warehouseToMaintenance == null)
            {
                return RedirectToAction("BindSparePartIndex");
            }

            ViewHome model = new ViewHome()
            {
                TempWarehouse = db.TempWarehouse.Where(t => t.VehicleId == VBId).ToList()
            };

            ViewBag.Maintenance = db.MaintenanceType.ToList();
            ViewBag.VehicleBrand = db.VehiclesBrand.ToList();
            ViewBag.WhToMain = warehouseToMaintenance;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BindSparePartUpdate(WarehouseToMaintenanceModel warehouseToMaintenance)
        {
            if (warehouseToMaintenance == null)
            {
                return RedirectToAction("BindSparePartIndex");
            }
            if (ModelState.IsValid)
            {
                //Required Quantity
                decimal? QuaDot = null;
                decimal QuaTemp;
                string QuaTempDot;

                if (warehouseToMaintenance.Quantity != null && warehouseToMaintenance.Quantity.Contains("."))
                {
                    QuaTempDot = warehouseToMaintenance.Quantity.Replace('.', ',');
                }
                else if (warehouseToMaintenance.Quantity != null)
                {
                    QuaTempDot = warehouseToMaintenance.Quantity;
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

                //Limit
                decimal? LimitDot = null;
                decimal LimitTemp;
                string LimitTempDot;

                if (warehouseToMaintenance.NotRequireSPLimit != null && warehouseToMaintenance.NotRequireSPLimit.Contains("."))
                {
                    LimitTempDot = warehouseToMaintenance.NotRequireSPLimit.Replace('.', ',');
                }
                else if (warehouseToMaintenance.NotRequireSPLimit != null)
                {
                    LimitTempDot = warehouseToMaintenance.NotRequireSPLimit;
                }
                else
                {
                    LimitTempDot = null;
                }

                if (LimitTempDot != null && decimal.TryParse(LimitTempDot, out LimitTemp))
                {
                    LimitDot = decimal.Parse(LimitTempDot);
                }
                //End

                WarehouseToMaintenance newWarehouseToMaintenance = db.WarehouseToMaintenance.Find(warehouseToMaintenance.Id);
                newWarehouseToMaintenance.MaintenanceTypeId = warehouseToMaintenance.MaintenanceTypeId;
                newWarehouseToMaintenance.WarehouseId = warehouseToMaintenance.WarehouseId;
                newWarehouseToMaintenance.Quantity = QuaDot;
                newWarehouseToMaintenance.NotRequireSPLimit = LimitDot;

                db.Entry(newWarehouseToMaintenance).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("BindSparePartIndex");
            }
            return RedirectToAction("BindSparePartIndex");
        }

        [OperationFilter]
        public ActionResult BindSparePartDelete(int id)
        {
            WarehouseToMaintenance warehouseToMaintenance = db.WarehouseToMaintenance.Find(id);

            if (warehouseToMaintenance == null)
            {
                return RedirectToAction("BindSparePartIndex");
            }

            db.WarehouseToMaintenance.Remove(warehouseToMaintenance);
            db.SaveChanges();
            return RedirectToAction("BindSparePartIndex");
        }



        //[OperationFilter]
        //public ActionResult BindSparePartIndex(SearchMainBinding searchMainBinding)
        //{
        //    ViewBag.Sys = true;
        //    ViewHome model = new ViewHome
        //    {
        //        MaintenanceType=db.MaintenanceType.ToList(),
        //        TempWarehouse=db.TempWarehouse.ToList(),
        //        VehiclesBrand=db.VehiclesBrand.ToList()
        //    };
        //    model.WarehouseToMaintenance = db.WarehouseToMaintenance.Where(i => (searchMainBinding.maintenanceTypeId != null ? i.MaintenanceTypeId == searchMainBinding.maintenanceTypeId : true) &&
        //                                                                        (searchMainBinding.warehouseId != null ? i.WarehouseId == searchMainBinding.warehouseId : true) &&
        //                                                                        (searchMainBinding.brandId != null ? i.TempWarehouse.VehicleId == searchMainBinding.brandId : true) 
        //                                                                        ).ToList();

        //    model.SearchMainBinding = searchMainBinding;
        //    return View(model);
        //}

        //[OperationFilter]
        //public ActionResult BindSparePartCreate()
        //{
        //    ViewBag.Sys = true;
        //    ViewBag.Maintenance = db.MaintenanceType.ToList();
        //    ViewBag.VehicleBrand = db.VehiclesBrand.ToList();
        //    ViewBag.Warehouse = db.TempWarehouse.ToList();
        //    return View();
        //}

        //[HttpPost]
        //[OperationFilter]
        //public ActionResult BindSparePartCreate(WarehouseToMaintenanceModel warehouseToMaintenance)
        //{
        //    if (warehouseToMaintenance == null)
        //    {
        //        return RedirectToAction("BindSparePartIndex");
        //    }

        //    //Required Quantity
        //    decimal? QuaDot = null;
        //    decimal QuaTemp;
        //    string QuaTempDot;

        //    if (warehouseToMaintenance.Quantity != null && warehouseToMaintenance.Quantity.Contains("."))
        //    {
        //        QuaTempDot = warehouseToMaintenance.Quantity.Replace('.', ',');
        //    }
        //    else if (warehouseToMaintenance.Quantity != null)
        //    {
        //        QuaTempDot = warehouseToMaintenance.Quantity;
        //    }
        //    else
        //    {
        //        QuaTempDot = null;
        //    }

        //    if (QuaTempDot != null && decimal.TryParse(QuaTempDot, out QuaTemp))
        //    {
        //        QuaDot = decimal.Parse(QuaTempDot);
        //    }
        //    //End

        //    WarehouseToMaintenance newWarehouseToMaintenance = new WarehouseToMaintenance();
        //    newWarehouseToMaintenance.MaintenanceTypeId = warehouseToMaintenance.MaintenanceTypeId;
        //    newWarehouseToMaintenance.WarehouseId = warehouseToMaintenance.WarehouseId;
        //    newWarehouseToMaintenance.Quantity = QuaDot;

        //    db.WarehouseToMaintenance.Add(newWarehouseToMaintenance);
        //    db.SaveChanges();
        //    return RedirectToAction("BindSparePartIndex");
        //}

        //[OperationFilter]
        //public ActionResult BindSparePartUpdate(int? id)
        //{
        //    ViewBag.Sys = true;
        //    if (id==null)
        //    {
        //        return RedirectToAction("BindSparePartIndex");
        //    }

        //    WarehouseToMaintenance warehouseToMaintenance = db.WarehouseToMaintenance.Find(id);
        //    int? VBId = 0;
        //    if (warehouseToMaintenance.WarehouseId!=null && warehouseToMaintenance.TempWarehouse.VehicleId!=null)
        //    {
        //        VBId = warehouseToMaintenance.TempWarehouse.VehicleId;
        //    }
        //    if (warehouseToMaintenance == null)
        //    {
        //        return RedirectToAction("BindSparePartIndex");
        //    }

        //    ViewHome model = new ViewHome()
        //    {
        //        TempWarehouse = db.TempWarehouse.Where(t=>t.VehicleId == VBId).ToList()
        //    };

        //    ViewBag.Maintenance = db.MaintenanceType.ToList();
        //    ViewBag.VehicleBrand = db.VehiclesBrand.ToList();
        //    ViewBag.WhToMain = warehouseToMaintenance;
        //    return View(model);
        //}

        //[HttpPost]
        //[OperationFilter]
        //public ActionResult BindSparePartUpdate(WarehouseToMaintenanceModel warehouseToMaintenance)
        //{
        //    if (warehouseToMaintenance == null)
        //    {
        //        return RedirectToAction("BindSparePartIndex");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        //Required Quantity
        //        decimal? QuaDot = null;
        //        decimal QuaTemp;
        //        string QuaTempDot;

        //        if (warehouseToMaintenance.Quantity != null && warehouseToMaintenance.Quantity.Contains("."))
        //        {
        //            QuaTempDot = warehouseToMaintenance.Quantity.Replace('.', ',');
        //        }
        //        else if (warehouseToMaintenance.Quantity != null)
        //        {
        //            QuaTempDot = warehouseToMaintenance.Quantity;
        //        }
        //        else
        //        {
        //            QuaTempDot = null;
        //        }

        //        if (QuaTempDot != null && decimal.TryParse(QuaTempDot, out QuaTemp))
        //        {
        //            QuaDot = decimal.Parse(QuaTempDot);
        //        }
        //        //End

        //        WarehouseToMaintenance newWarehouseToMaintenance = db.WarehouseToMaintenance.Find(warehouseToMaintenance.Id);
        //        newWarehouseToMaintenance.MaintenanceTypeId = warehouseToMaintenance.MaintenanceTypeId;
        //        newWarehouseToMaintenance.WarehouseId = warehouseToMaintenance.WarehouseId;
        //        newWarehouseToMaintenance.Quantity = QuaDot;

        //        db.Entry(newWarehouseToMaintenance).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("BindSparePartIndex");
        //    }
        //    return RedirectToAction("BindSparePartIndex");
        //}

        //[OperationFilter]
        //public ActionResult BindSparePartDelete(int id)
        //{
        //    WarehouseToMaintenance warehouseToMaintenance = db.WarehouseToMaintenance.Find(id);

        //    if (warehouseToMaintenance == null)
        //    {
        //        return RedirectToAction("BindSparePartIndex");
        //    }

        //    db.WarehouseToMaintenance.Remove(warehouseToMaintenance);
        //    db.SaveChanges();
        //    return RedirectToAction("BindSparePartIndex");
        //}

        //Rufat
        public void ExportBindSparePartData()
        {
            ViewHome model = new ViewHome();
            model.WarehouseToMaintenance = db.WarehouseToMaintenance.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("WarehouseToMaintenance List");

            ws.Range("B2:H2").Merge();
            ws.Cell("B2").Value = "Ehtiyat hissəsinin texniki qulluğa təhkimi Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "TQ adı";
            ws.Cell("D3").Value = "Ehtiyyat hissəsi kodu";
            ws.Cell("E3").Value = "Ehtiyyat hissəsi adı";
            ws.Cell("F3").Value = "Ehtiyyat hissəsi miqdarı";
            ws.Cell("G3").Value = "NV brand";
            ws.Cell("H3").Value = "Limit";





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



            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;


            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 20;
            ws.Column("D").Width = 15;
            ws.Column("E").Width = 40;
            ws.Column("F").Width = 15;
            ws.Column("G").Width = 20;
            ws.Column("H").Width = 10;



            int i = 4;
            foreach (var item in model.WarehouseToMaintenance)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.MaintenanceType.Name == null ? "" : item.MaintenanceType.Name);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;


                ws.Cell("D" + i).Value = (item.TempWarehouse.SparePartCode == null ? "" : item.TempWarehouse.SparePartCode);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                ws.Cell("E" + i).Value = (item.TempWarehouse.Name == null ? "" : item.TempWarehouse.Name);
                ws.Cell("E" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                ws.Cell("F" + i).Value = (item.Quantity == null ? null : item.Quantity);
                ws.Cell("F" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("F" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("F" + i).Style.Alignment.WrapText = true;

                ws.Cell("G" + i).Value = (item.TempWarehouse.VehiclesBrand.Brand == null ? null : item.TempWarehouse.VehiclesBrand.Brand);
                ws.Cell("G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("G" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("G" + i).Style.Alignment.WrapText = true;

                ws.Cell("H" + i).Value = (item.NotRequireSPLimit == null ? null : item.NotRequireSPLimit);
                ws.Cell("H" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("H" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("H" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"WarehouseToMaintenanceList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }
        //Rufat end 


        //Get Spare Parts by vehicle brand
        public JsonResult GetSparePartsByvehicleBrand(int id)
        {
            List<AJAXResponseModel> result = new List<AJAXResponseModel>();
            int Id = 0;
            if (!int.TryParse(id.ToString(), out Id))
            {
                Session["InvalidBrandId"] = true;
            }
            else
            {
                foreach (var item in db.TempWarehouse.Where(w => w.VehicleId == Id).ToList())
                {
                    AJAXResponseModel aJAXResponseModel = new AJAXResponseModel();
                    aJAXResponseModel.sparePartCode = item.SparePartCode;
                    aJAXResponseModel.sparePartName = item.Name;
                    aJAXResponseModel.sparePartId = item.Id;
                    result.Add(aJAXResponseModel);
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}