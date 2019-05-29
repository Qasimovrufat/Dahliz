using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Filters;
using BBService.Models;
using ClosedXML.Excel;
using LinqToExcel;

namespace BBService.Controllers
{
    [LogOut]
    public class InputsController : Controller
    {
        // GET: Inputs
        BBServiceEntities db = new BBServiceEntities();

        //Company Input

        [OperationFilter]
        public ActionResult CompInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Companies = db.Companies.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult CompInpCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult CompInpCreate(Companies comp)
        {
            db.Companies.Add(comp);
            db.SaveChanges();
            return RedirectToAction("CompInpIndex");
        }

        [OperationFilter]
        public ActionResult CompInpUpdate(int id)
        {
            ViewBag.Sys = true;
            Companies comp = db.Companies.Find(id);
            if (comp == null)
            {
                return RedirectToAction("CompInpIndex");
            }

            ViewBag.Comp = comp;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult CompInpUpdate(Companies comp)
        {
            if (comp == null)
            {
                return RedirectToAction("CompInpIndex");
            }

            if (ModelState.IsValid)
            {
                db.Entry(comp).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("CompInpIndex");
            }
            return RedirectToAction("CompInpIndex");
        }

        [OperationFilter]
        public ActionResult CompInpDelete(int id)
        {
            Companies comp = db.Companies.Find(id);
           
            if (comp == null)
            {
                return RedirectToAction("CompInpIndex");
            }

            db.Companies.Remove(comp);
            db.SaveChanges();
            return RedirectToAction("CompInpIndex");
        }

        public void ExportCompaniesData()
        {
            ViewHome model = new ViewHome();
            model.Companies = db.Companies.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Companies List");

            ws.Range("B2:C2").Merge();
            ws.Cell("B2").Value = "Şirkətlərin Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Şirkət";

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


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;

            int i = 4;
            foreach (var item in model.Companies)
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

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"CompaniesList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Route Input

        [OperationFilter]
        public ActionResult RutInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Routes = db.Routes.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult RutInpCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult RutInpCreate(RouteModel rut)
        {
            if (rut == null)
            {
                return RedirectToAction("RutInpIndex");
            }
            Routes Rut = new Routes();
            DateTime activeDate = DateTime.ParseExact(rut.ActivationDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Rut.ActivationDate = activeDate;
            Rut.Name = rut.Name;
            Rut.Forward = rut.Forward;
            Rut.Backward = rut.Backward;
            Rut.Number = rut.Number;

            //Forward km
            decimal? ForwardkmDot = null;
            decimal ForwardkmTemp;
            string ForwardkmTempDot;

            if (rut.ForwardLength != null && rut.ForwardLength.Contains("."))
            {
                ForwardkmTempDot = rut.ForwardLength.Replace('.', ',');
            }
            else if (rut.ForwardLength != null)
            {
                ForwardkmTempDot = rut.ForwardLength;
            }
            else
            {
                ForwardkmTempDot = null;
            }

            if (ForwardkmTempDot != null && decimal.TryParse(ForwardkmTempDot, out ForwardkmTemp))
            {
                ForwardkmDot = decimal.Parse(ForwardkmTempDot);
            }

            Rut.ForwardLength = ForwardkmDot;
            //End

            //Backward km
            decimal? BackwardkmDot = null;
            decimal BackwardkmTemp;
            string BackwardkmTempDot;

            if (rut.BackwardLength != null && rut.BackwardLength.Contains("."))
            {
                BackwardkmTempDot = rut.BackwardLength.Replace('.', ',');
            }
            else if (rut.BackwardLength != null)
            {
                BackwardkmTempDot = rut.BackwardLength;
            }
            else
            {
                BackwardkmTempDot = null;
            }

            if (BackwardkmTempDot != null && decimal.TryParse(BackwardkmTempDot, out BackwardkmTemp))
            {
                BackwardkmDot = decimal.Parse(BackwardkmTempDot);
            }

            Rut.BackwardLength = BackwardkmDot;
            //End

            db.Routes.Add(Rut);
            db.SaveChanges();
            return RedirectToAction("RutInpIndex");
        }

        [OperationFilter]
        public ActionResult RutInpUpdate(int id)
        {
            ViewBag.Sys = true;
            ViewBag.RutId = id;
            Routes rut = db.Routes.Find(id);
            if (rut == null)
            {
                return RedirectToAction("RutInpIndex");
            }

            ViewBag.Rut = rut;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult RutInpUpdate(RouteModel rut)
        {
            if (rut == null)
            {
                return RedirectToAction("RutInpIndex");
            }

            if (ModelState.IsValid)
            {
                Routes Rut = db.Routes.Find(rut.Id);
                if (rut.ActivationDate!=null)
                {
                    DateTime activeDate = DateTime.ParseExact(rut.ActivationDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    Rut.ActivationDate = activeDate;
                }
                else
                {
                    Rut.ActivationDate = null;
                }
                if (rut.DeactivationDate != null)
                {
                    DateTime deactiveDate = DateTime.ParseExact(rut.DeactivationDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    Rut.DeactivationDate = deactiveDate;
                }
                else
                {
                    Rut.DeactivationDate = null;
                }

                //Forward km
                decimal? ForwardkmDot = null;
                decimal ForwardkmTemp;
                string ForwardkmTempDot;

                if (rut.ForwardLength != null && rut.ForwardLength.Contains("."))
                {
                    ForwardkmTempDot = rut.ForwardLength.Replace('.', ',');
                }
                else if (rut.ForwardLength != null)
                {
                    ForwardkmTempDot = rut.ForwardLength;
                }
                else
                {
                    ForwardkmTempDot = null;
                }

                if (ForwardkmTempDot != null && decimal.TryParse(ForwardkmTempDot, out ForwardkmTemp))
                {
                    ForwardkmDot = decimal.Parse(ForwardkmTempDot);
                }

                Rut.ForwardLength = ForwardkmDot;
                //End

                //Backward km
                decimal? BackwardkmDot = null;
                decimal BackwardkmTemp;
                string BackwardkmTempDot;

                if (rut.BackwardLength != null && rut.BackwardLength.Contains("."))
                {
                    BackwardkmTempDot = rut.BackwardLength.Replace('.', ',');
                }
                else if (rut.BackwardLength != null)
                {
                    BackwardkmTempDot = rut.BackwardLength;
                }
                else
                {
                    BackwardkmTempDot = null;
                }

                if (BackwardkmTempDot != null && decimal.TryParse(BackwardkmTempDot, out BackwardkmTemp))
                {
                    BackwardkmDot = decimal.Parse(BackwardkmTempDot);
                }

                Rut.BackwardLength = BackwardkmDot;
                //End


                Rut.Name = rut.Name;
                Rut.Forward = rut.Forward;
                Rut.Backward = rut.Backward;
                Rut.Number = rut.Number;

                db.Entry(Rut).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("RutInpIndex");
            }
            return RedirectToAction("RutInpIndex");
        }

        //[OperationFilter]
        //public ActionResult RutInpClose(int id)
        //{
        //    ViewBag.Sys = true;
        //    ViewBag.RutId = id;
        //    Routes rut = db.Routes.Find(id);
        //    if (rut == null)
        //    {
        //        return RedirectToAction("RutInpIndex");
        //    }

        //    ViewBag.Rut = rut;
        //    return View();
        //}

        [OperationFilter]
        public ActionResult RutInpDelete(int id)
        {
            Routes rut = db.Routes.Find(id);
           
            if (rut == null)
            {
                return RedirectToAction("RutInpIndex");
            }

            db.Routes.Remove(rut);
            db.SaveChanges();
            return RedirectToAction("RutInpIndex");
        }

        public void ExportRoutesData()
        {
            ViewHome model = new ViewHome();
            model.Routes = db.Routes.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Companies List");

            ws.Range("B2:E2").Merge();
            ws.Cell("B2").Value = "Xətlərin Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Xəttin nömrəsi";
            ws.Cell("D3").Value = "Xəttin adı";
            ws.Cell("E3").Value = "Açılma tarixi";

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


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 10;
            ws.Column("D").Width = 50;
            ws.Column("E").Width = 20;

            int i = 4;
            foreach (var item in model.Routes)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.Number == null ? "" : item.Number);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;

                ws.Cell("D" + i).Value = (item.Name == null ? "" : item.Name);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                DateTime startDate = new DateTime();
                if (item.ActivationDate != null)
                {
                    startDate = DateTime.ParseExact(item.ActivationDate.Value.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    ws.Cell("E" + i).Value = startDate.Date;
                }
                //ws.Cell("E" + i).Value = (item.StartDate == null ? "" : item.StartDate.Value.ToString("dd.MM.yyyy"));
                ws.Cell("E" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"RoutesList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Driver Input

        [OperationFilter]
        public ActionResult EmpInpIndex(SearchEmployee searchModel, string searchModelString, int page = 1)
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
                    searchModel.employeeId = Convert.ToInt32(searchModelList[0]);
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModel.positionId = Convert.ToInt32(searchModelList[1]);
                }
                if (!string.IsNullOrEmpty(searchModelList[2]))
                {
                    searchModel.startRec = searchModelList[2];
                }
                if (!string.IsNullOrEmpty(searchModelList[3]))
                {
                    searchModel.endRec = searchModelList[3];
                }
                if (!string.IsNullOrEmpty(searchModelList[4]))
                {
                    searchModel.startDeact = searchModelList[4];
                }
                if (!string.IsNullOrEmpty(searchModelList[5]))
                {
                    searchModel.endDeact = searchModelList[5];
                }
              
                if (!string.IsNullOrEmpty(searchModelList[6]))
                {
                    searchModel.startDLIssure = searchModelList[6];
                }
                if (!string.IsNullOrEmpty(searchModelList[7]))
                {
                    searchModel.endDLIssure = searchModelList[7];
                }
                if (!string.IsNullOrEmpty(searchModelList[8]))
                {
                    searchModel.startDLExp = searchModelList[8];
                }
                if (!string.IsNullOrEmpty(searchModelList[9]))
                {
                    searchModel.endDLExp = searchModelList[9];
                }
                if (!string.IsNullOrEmpty(searchModelList[10]))
                {
                    searchModel.status = Convert.ToBoolean(searchModelList[10]);
                }
                if (!string.IsNullOrEmpty(searchModelList[11]))
                {
                    searchModel.searchData = searchModelList[11];
                }
            }
            //End



            ViewBag.Sys = true;
            ViewHome model = new ViewHome();
           
            model.Positions = db.Positions.ToList();
            ViewBag.Employees = db.Employees.ToList();

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

            //Datetime variables
            DateTime? startRec = null;
            DateTime? endRec = null;
            DateTime? startDeact = null;
            DateTime? endDeact = null;
            DateTime? startDLIssure = null;
            DateTime? endDLIssure = null;
            DateTime? startDLExp = null;
            DateTime? endDLExp = null;

            //Recruitment date
            if (!string.IsNullOrEmpty(searchModel.startRec))
            {
                startRec = DateTime.ParseExact(searchModel.startRec, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.endRec))
            {
                endRec = DateTime.ParseExact(searchModel.endRec, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Deactivation date
            if (!string.IsNullOrEmpty(searchModel.startDeact))
            {
                startDeact = DateTime.ParseExact(searchModel.startDeact, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.endDeact))
            {
                endDeact = DateTime.ParseExact(searchModel.endDeact, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Driver Licence Issue date
            if (!string.IsNullOrEmpty(searchModel.startDLIssure))
            {
                startDLIssure = DateTime.ParseExact(searchModel.startDLIssure, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.endDLIssure))
            {
                endDLIssure = DateTime.ParseExact(searchModel.endDLIssure, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            //Driver Licence Expire date
            if (!string.IsNullOrEmpty(searchModel.startDLExp))
            {
                startDLExp = DateTime.ParseExact(searchModel.startDLExp, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(searchModel.endDLExp))
            {
                endDLExp = DateTime.ParseExact(searchModel.endDLExp, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            var employees = db.Employees.Where(e => (searchModel.employeeId != null ? e.Id == searchModel.employeeId : true) &&
                                                      (searchModel.positionId != null ? e.PositionId == searchModel.positionId : true) &&
                                                      (startRec != null ? e.RecruitmentDate >= startRec : true) &&
                                                      (endRec != null ? e.RecruitmentDate <= endRec : true) &&
                                                      (startDeact != null ? e.DeactivationDate >= startDeact : true) &&
                                                      (endDeact != null ? e.DeactivationDate <= endDeact : true) &&
                                                      (startDLIssure != null ? e.LicenceIssueDate >= startDLIssure : true) &&
                                                      (endDLIssure != null ? e.LicenceIssueDate <= endDLIssure : true) &&
                                                      (startDLExp != null ? e.LicenceExpireDate >= startDLExp : true) &&
                                                      (endDLExp != null ? e.LicenceExpireDate <= endDLExp : true) &&
                                                      (searchModel.status != null ? e.Status == searchModel.status : true) &&
                                                      (searchModel.searchData != null ? (e.Phone.Contains(searchModel.searchData) || e.LicenceCategory.Contains(searchModel.searchData) || e.LicenceNumber.Contains(searchModel.searchData) || e.LicenceSeries.Contains(searchModel.searchData)) : true)
                                                      ).ToList();



            int limit = 10;
            if (limit > employees.Count)
            {
                limit = employees.Count;
            }

            ViewBag.total = (int)Math.Ceiling(((employees.Count() > 0 ? employees.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul

            model.SearchEmployee = searchModel;
            model.Employees = employees.Skip(((int)page - 1) *limit).Take(limit);
         
            TempData["sortedDrivers"] = model;
            return View(model);
        }

        [OperationFilter]
        public ActionResult EmpInpCreate()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Positions = db.Positions.ToList(),
                Departments=db.Departments.ToList(),
                Sectors=db.Sectors.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult EmpInpCreate(EmployeeModel drv)
        {
            List<Employees> DrvList = db.Employees.ToList();
            foreach (var item in DrvList)
            {
                if (drv.EmployeeCode != "99999" && (item.EmployeeCode == drv.EmployeeCode || drv.EmployeeCode.ToString().Length < 4 || drv.EmployeeCode.ToString().Length > 10))
                {
                    Session["InvalidDrvCode"] = true;
                    return RedirectToAction("EmpInpCreate");
                }
            }
            if (drv == null)
            {
                return RedirectToAction("EmpInpIndex");
            }

            Employees Drv = new Employees();


            //Birth Date, Recruitment, Licence issue date and Licence Expire Date 
            if (drv.BirthDate != null)
            {
                DateTime birthDate = DateTime.ParseExact(drv.BirthDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                Drv.BirthDate = birthDate;
            }
            if (drv.RecruitmentDate != null)
            {
                DateTime recDate = DateTime.ParseExact(drv.RecruitmentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                Drv.RecruitmentDate = recDate;
            }
            if (drv.LicenceIssueDate != null)
            {
                DateTime licIssueDate = DateTime.ParseExact(drv.LicenceIssueDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                Drv.LicenceIssueDate = licIssueDate;
            }
            if (drv.LicenceExpireDate != null)
            {
                DateTime licExpDate = DateTime.ParseExact(drv.LicenceExpireDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                Drv.LicenceExpireDate = licExpDate;
            }
            //End
            Drv.EmployeeCode = drv.EmployeeCode;
            Drv.Name = drv.Name;
            Drv.Surname = drv.Surname;
            Drv.FutherName = drv.FutherName;
            if (drv.DepartmentId!=null)
            {
                Drv.DepartmentId = drv.DepartmentId;
            }
            else
            {
                Drv.DepartmentId = null;
            }
            if (drv.SectorId != null)
            {
                Drv.SectorId = drv.SectorId;
            }
            else
            {
                Drv.SectorId = null;
            }
            if (drv.PositionId != null)
            {
                Drv.PositionId = drv.PositionId;
            }
            else
            {
                Drv.PositionId = null;
            }
            Drv.Phone = drv.Phone;
            Drv.LicenceCategory = drv.LicenceCategory;
            Drv.LicenceSeries = drv.LicenceSeries;
            Drv.LicenceNumber = drv.LicenceNumber;
            Drv.Status = true;

            db.Employees.Add(Drv);
            db.SaveChanges();
            return RedirectToAction("EmpInpIndex");
        }

        [OperationFilter]
        public ActionResult EmpInpUpdate(int id)
        {
            ViewBag.Sys = true;
            ViewBag.DrvId = id;
            Employees drv = db.Employees.Find(id);
            ViewBag.Drv = drv;

            ViewHome model = new ViewHome
            {
                Positions = db.Positions.ToList(),
                Departments=db.Departments.ToList(),
                Sectors=db.Sectors.ToList()
            };
            if (drv == null)
            {
                return RedirectToAction("EmpInpIndex");
            }

            ViewBag.Drv = drv;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult EmpInpUpdate(EmployeeModel drv)
        {
            if (drv == null)
            {
                return RedirectToAction("EmpInpIndex");
            }

            if (ModelState.IsValid)
            {
                Employees Drv = db.Employees.Find(drv.Id);

                //Recruitment and Licence Expire Date
                //Birth date
                if (drv.BirthDate != null)
                {
                    DateTime birthDate = DateTime.ParseExact(drv.BirthDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    Drv.BirthDate = birthDate;
                }
                else
                {
                    Drv.BirthDate = null;
                }

                //Rec date
                if (drv.RecruitmentDate != null)
                {
                    DateTime recDate = DateTime.ParseExact(drv.RecruitmentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    Drv.RecruitmentDate = recDate;
                }
                else
                {
                    Drv.RecruitmentDate = null;
                }

                //Licence issue date
                if (drv.LicenceIssueDate != null)
                {
                    DateTime licIssueDate = DateTime.ParseExact(drv.LicenceIssueDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    Drv.LicenceIssueDate = licIssueDate;
                }
                else
                {
                    Drv.LicenceIssueDate = null;
                }

                //Licence expire date
                if (drv.LicenceExpireDate != null)
                {
                    DateTime licExpDate = DateTime.ParseExact(drv.LicenceExpireDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    Drv.LicenceExpireDate = licExpDate;
                }
                else
                {
                    Drv.LicenceExpireDate = null;
                }

                //Deactivation date
                if (drv.DeactivationDate != null)
                {
                    DateTime deactiveDate = DateTime.ParseExact(drv.DeactivationDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    Drv.DeactivationDate = deactiveDate;
                }
                else
                {
                    Drv.DeactivationDate = null;
                }
                //End

                Drv.Name = drv.Name;
                Drv.Surname = drv.Surname;
                Drv.FutherName = drv.FutherName;
                if (drv.DepartmentId == 0)
                {
                    Drv.DepartmentId = null;
                }
                else
                {
                    Drv.DepartmentId = drv.DepartmentId;
                }

                if (drv.SectorId == 0)
                {
                    Drv.SectorId = null;
                }
                else
                {
                    Drv.SectorId = drv.SectorId;
                }

                if (drv.PositionId == 0)
                {
                    Drv.PositionId = null;
                }
                else
                {
                    Drv.PositionId = drv.PositionId;
                }

                if (drv.Status == true)
                {
                    Drv.Status = false;
                }
                else
                {
                    Drv.Status = true;
                }
                if (drv.PositionId != 0)
                {
                    Drv.PositionId = drv.PositionId;

                }
                Drv.Phone = drv.Phone;
                Drv.LicenceCategory = drv.LicenceCategory;
                Drv.LicenceSeries = drv.LicenceSeries;
                Drv.LicenceNumber = drv.LicenceNumber;

                db.Entry(Drv).State = EntityState.Modified;
                db.Entry(Drv).Property(x => x.EmployeeCode).IsModified = false;

                db.SaveChanges();
                return RedirectToAction("EmpInpIndex");
            }
            return RedirectToAction("EmpInpIndex");
        }

        [OperationFilter]
        public ActionResult EmpInpDelete(int id)
        {
            Employees drv = db.Employees.Find(id);
            
            if (drv == null)
            {
                return RedirectToAction("EmpInpIndex");
            }

            db.Employees.Remove(drv);
            db.SaveChanges();
            return RedirectToAction("EmpInpIndex");
        }

        [OperationFilter]
        public ActionResult EmpInpDetailed(int id)
        {           
            ViewBag.Emp = db.Employees.FirstOrDefault(e=>e.Id==id);
            //ViewBag.Emp1 = db.Employees.FirstOrDefault(e => e.BirthDate == id);

            return View();
        }

        //Download Tamplate
        [OperationFilter]
        public ActionResult EmployeeExcelTemp()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/Uploads/Templates/";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + "EmployeeExcelTemplate.xlsx");
            string fileName = "EmployeeExcelTemplate.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        //Upload Drivers' Excel file
        [OperationFilter]
        public ActionResult UploadEmployeeExcel()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult UploadEmployeeExcel(HttpPostedFileBase UploadedExcel)
        {
            ViewBag.Sys = true;
            if (UploadedExcel != null)
            {
                if (UploadedExcel.ContentType == "application/vnd.ms-excel" || UploadedExcel.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = UploadedExcel.FileName;
                    string targetpath = Server.MapPath("~/Uploads/Temp/");
                    UploadedExcel.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;

                    string sheetName = "Sheet1";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var DriversData = from a in excelFile.Worksheet<UploadEmployees>(sheetName) select a;

                    //Checking for dublicates in uploaded file
                    List<string> VehiclesDataListDriverCode = new List<string>();
                    foreach (var item in DriversData)
                    {
                        if (!string.IsNullOrEmpty(item.İşçi_kodu))
                        {
                            VehiclesDataListDriverCode.Add(item.İşçi_kodu.ToString());
                        }
                    }
                    for (int i = 0; i < VehiclesDataListDriverCode.Count; i++)
                    {
                        if (i > 0 && (VehiclesDataListDriverCode[i] == VehiclesDataListDriverCode[i - 1]))
                        {
                            TempData["RepeatedDriverCode"] += VehiclesDataListDriverCode[i] + " ";
                        }
                    }
                    if (TempData["RepeatedDriverCode"] != null)
                    {
                        Session["RepeatedDriverCode"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadEmployeeExcel");
                    }

                    //Checking for dublicates
                    List<Employees> driverList = db.Employees.ToList();
                    bool result = false;
                    foreach (var item in driverList)
                    {
                        foreach (var item2 in DriversData)
                        {
                            if ((item2.İşçi_kodu == item.EmployeeCode) && (!string.IsNullOrEmpty(item2.İşçi_kodu)))
                            {
                                TempData["RepeatedCodes"] += item2.İşçi_kodu + " ";
                            }
                            if ((item2.İşçi_kodu == "") && (item2.Soyad != "" || item2.Ad != "" || item2.Ata_adı != "" || item2.İşə_qəbul_tarixi != null || item2.Telefon != ""))
                            {
                                result = true;
                            }

                        }
                    }
                    if (TempData["RepeatedCodes"] != null)
                    {
                        Session["RepeatedCodes"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadEmployeeExcel");
                    }

                    DateTime? BirthDate = null;
                    DateTime? RecruitmentDate = null;
                    DateTime? LicenceIssueDate = null;
                    DateTime? LicenceExpireDate = null;

                    //Adding Drivers' data to database
                    foreach (var a in DriversData)
                    {
                        if (!string.IsNullOrEmpty(a.İşçi_kodu))
                        {
                            Employees drv = new Employees();
                            drv.EmployeeCode = a.İşçi_kodu;
                            drv.Surname = a.Soyad;
                            drv.Name = a.Ad;
                            drv.FutherName = a.Ata_adı;

                            //Birth Date
                            string[] datesB;
                            if (a.Doğum_tarixi != null && a.Doğum_tarixi.Contains("."))
                            {
                                datesB = a.Doğum_tarixi.Split('.');
                                BirthDate = Convert.ToDateTime(datesB[0] + "." + datesB[1] + "." + datesB[2]);
                            }
                            else if (a.Doğum_tarixi != null && a.Doğum_tarixi.Contains("/"))
                            {
                                datesB = a.Doğum_tarixi.Split('/');
                                BirthDate = Convert.ToDateTime(datesB[0] + "." + datesB[1] + "." + datesB[2]);
                            }
                            else if (a.Doğum_tarixi != null && a.Doğum_tarixi.Contains("-"))
                            {
                                datesB = a.Doğum_tarixi.Split('-');
                                BirthDate = Convert.ToDateTime(datesB[0] + "." + datesB[1] + "." + datesB[2]);
                            }
                            else
                            {
                                BirthDate = null;
                            }

                            if (a.Doğum_tarixi != null && BirthDate != null)
                            {
                                drv.BirthDate = BirthDate;
                            }
                            //End

                            //Recruitment Date
                            string[] dates;
                            if (a.İşə_qəbul_tarixi != null && a.İşə_qəbul_tarixi.Contains("."))
                            {
                                dates = a.İşə_qəbul_tarixi.Split('.');
                                RecruitmentDate = Convert.ToDateTime(dates[0] + "." + dates[1] + "." + dates[2]);
                            }
                            else if (a.İşə_qəbul_tarixi != null && a.İşə_qəbul_tarixi.Contains("/"))
                            {
                                dates = a.İşə_qəbul_tarixi.Split('/');
                                RecruitmentDate = Convert.ToDateTime(dates[0] + "." + dates[1] + "." + dates[2]);
                            }
                            else if (a.İşə_qəbul_tarixi != null && a.İşə_qəbul_tarixi.Contains("-"))
                            {
                                dates = a.İşə_qəbul_tarixi.Split('-');
                                RecruitmentDate = Convert.ToDateTime(dates[0] + "." + dates[1] + "." + dates[2]);
                            }
                            else
                            {
                                RecruitmentDate = null;
                            }

                            if (a.İşə_qəbul_tarixi != null && RecruitmentDate != null)
                            {
                                drv.RecruitmentDate = RecruitmentDate;
                            }
                            //End

                            //Licence Issue Date
                            string[] datesL;
                            if (a.SV_Verilmə_tarixi != null && a.SV_Verilmə_tarixi.Contains("."))
                            {
                                datesL = a.SV_Verilmə_tarixi.Split('.');
                                LicenceIssueDate = Convert.ToDateTime(datesL[0] + "." + datesL[1] + "." + datesL[2]);
                            }
                            else if (a.SV_Verilmə_tarixi != null && a.SV_Verilmə_tarixi.Contains("/"))
                            {
                                datesL = a.SV_Verilmə_tarixi.Split('/');
                                LicenceIssueDate = Convert.ToDateTime(datesL[0] + "." + datesL[1] + "." + datesL[2]);
                            }
                            else if (a.SV_Verilmə_tarixi != null && a.SV_Verilmə_tarixi.Contains("-"))
                            {
                                datesL = a.SV_Verilmə_tarixi.Split('-');
                                LicenceIssueDate = Convert.ToDateTime(datesL[0] + "." + datesL[1] + "." + datesL[2]);
                            }
                            else
                            {
                                LicenceIssueDate = null;
                            }

                            if (a.SV_Verilmə_tarixi != null && LicenceIssueDate != null)
                            {
                                drv.LicenceIssueDate = LicenceIssueDate;
                            }
                            //End

                            //Licence Expire Date
                            string[] dates2;
                            if (a.SV_Etibarlılıq_tarixi != null && a.SV_Etibarlılıq_tarixi.Contains("."))
                            {
                                dates2 = a.SV_Etibarlılıq_tarixi.Split('.');
                                LicenceExpireDate = Convert.ToDateTime(dates2[0] + "." + dates2[1] + "." + dates2[2]);
                            }
                            else if (a.SV_Etibarlılıq_tarixi != null && a.SV_Etibarlılıq_tarixi.Contains("/"))
                            {
                                dates2 = a.SV_Etibarlılıq_tarixi.Split('/');
                                LicenceExpireDate = Convert.ToDateTime(dates2[0] + "." + dates2[1] + "." + dates2[2]);
                            }
                            else if (a.SV_Etibarlılıq_tarixi != null && a.SV_Etibarlılıq_tarixi.Contains("-"))
                            {
                                dates2 = a.SV_Etibarlılıq_tarixi.Split('-');
                                LicenceExpireDate = Convert.ToDateTime(dates2[0] + "." + dates2[1] + "." + dates2[2]);
                            }
                            else
                            {
                                LicenceExpireDate = null;
                            }

                            if (a.SV_Etibarlılıq_tarixi != null && LicenceExpireDate != null)
                            {
                                drv.LicenceExpireDate = LicenceExpireDate;
                            }
                            //End

                            //Get position id
                            int? posId = null;
                            foreach (var item in db.Positions.ToList())
                            {
                                if (item.Name.Contains(a.Vəzifə))
                                {
                                    posId = item.Id;
                                }
                            }
                            //end
                            drv.PositionId = posId;

                            drv.Phone = a.Telefon;
                            drv.LicenceCategory = a.SV_Kateqoriyası;
                            drv.LicenceSeries = a.SV_Seriyası;
                            drv.LicenceNumber = a.SV_Nömrəsi;
                            drv.Status = true;

                            db.Employees.Add(drv);
                            db.SaveChanges();
                        }
                    }

                    System.IO.File.Delete(pathToExcelFile);
                }

            }
            return RedirectToAction("EmpInpIndex");
        }


        ///////////////////////////////////////////////////

        //Download Tamplate
        [OperationFilter]
        public ActionResult UpdateEmployeeExcelTemp()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/Uploads/Templates/";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + "UpdateEmployeeExcelTemp.xlsx");
            string fileName = "UpdateEmployeeExcelTemp.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        //Update Drivers' Excel file
        [OperationFilter]
        public ActionResult UpdateEmployeeExcel()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult UpdateEmployeeExcel(HttpPostedFileBase UploadedExcel)
        {
            ViewBag.Sys = true;
            if (UploadedExcel != null)
            {
                if (UploadedExcel.ContentType == "application/vnd.ms-excel" || UploadedExcel.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = UploadedExcel.FileName;
                    string targetpath = Server.MapPath("~/Uploads/Temp/");
                    UploadedExcel.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;

                    string sheetName = "Sheet1";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var DriversData = from a in excelFile.Worksheet<UploadEmployees>(sheetName) select a;

                    //Checking for dublicates in uploaded file
                    List<string> VehiclesDataListDriverCode = new List<string>();
                    foreach (var item in DriversData)
                    {
                        if (!string.IsNullOrEmpty(item.İşçi_kodu))
                        {
                            VehiclesDataListDriverCode.Add(item.İşçi_kodu.ToString());
                        }
                    }
                    for (int i = 0; i < VehiclesDataListDriverCode.Count; i++)
                    {
                        if (i > 0 && (VehiclesDataListDriverCode[i] == VehiclesDataListDriverCode[i - 1]))
                        {
                            TempData["RepeatedDriverCode"] += VehiclesDataListDriverCode[i] + " ";
                        }
                    }
                    if (TempData["RepeatedDriverCode"] != null)
                    {
                        Session["RepeatedDriverCode"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UpdateEmployeeExcel");
                    }

                    DateTime? BirthDate = null;
                    DateTime? RecruitmentDate = null;
                    DateTime? DeactivationDate = null;
                    DateTime? LicenceIssueDate = null;
                    DateTime? LicenceExpireDate = null;

                    //Updating Drivers' data in database
                    foreach (var a in DriversData)
                    {
                        if (!string.IsNullOrEmpty(a.İşçi_kodu))
                        {
                            Employees drv = db.Employees.FirstOrDefault(e => e.EmployeeCode == a.İşçi_kodu);

                            //Soyad
                            if (a.Soyad != null)
                            {
                                drv.Surname = a.Soyad;
                            }

                            //Ad
                            if (a.Ad != null)
                            {
                                drv.Name = a.Ad;
                            }

                            //Ata adı
                            if (a.Ata_adı != null)
                            {
                                drv.FutherName = a.Ata_adı;
                            }

                            //Vəzifə
                            if (a.Vəzifə != null)
                            {
                                //Get position id
                                int? posId = null;
                                foreach (var item in db.Positions.ToList())
                                {
                                    if (item.Name.Contains(a.Vəzifə))
                                    {
                                        posId = item.Id;
                                    }
                                }
                                //end
                                drv.PositionId = posId;
                            }

                            //Telefon
                            if (a.Telefon != null)
                            {
                                drv.Phone = a.Telefon;
                            }

                            // SV Kateqoriyası
                            if (a.SV_Kateqoriyası != null)
                            {
                                drv.LicenceCategory = a.SV_Kateqoriyası;
                            }

                            // SV seriyası
                            if (a.SV_Seriyası != null)
                            {
                                drv.LicenceSeries = a.SV_Seriyası;
                            }

                            // SV nömrəsi
                            if (a.SV_Nömrəsi != null)
                            {
                                drv.LicenceNumber = a.SV_Nömrəsi;
                            }

                            //Birth Date
                            string[] datesB;
                            if (a.Doğum_tarixi != null && a.Doğum_tarixi.Contains("."))
                            {
                                datesB = a.Doğum_tarixi.Split('.');
                                BirthDate = Convert.ToDateTime(datesB[0] + "." + datesB[1] + "." + datesB[2]);
                            }
                            else if (a.Doğum_tarixi != null && a.Doğum_tarixi.Contains("/"))
                            {
                                datesB = a.Doğum_tarixi.Split('/');
                                BirthDate = Convert.ToDateTime(datesB[0] + "." + datesB[1] + "." + datesB[2]);
                            }
                            else if (a.Doğum_tarixi != null && a.Doğum_tarixi.Contains("-"))
                            {
                                datesB = a.Doğum_tarixi.Split('-');
                                BirthDate = Convert.ToDateTime(datesB[0] + "." + datesB[1] + "." + datesB[2]);
                            }
                            else
                            {
                                BirthDate = null;
                            }

                            if (a.Doğum_tarixi != null && BirthDate != null)
                            {
                                drv.BirthDate = BirthDate;
                            }
                            //End

                            //Recruitment Date
                            string[] dates;
                            if (a.İşə_qəbul_tarixi != null && a.İşə_qəbul_tarixi.Contains("."))
                            {
                                dates = a.İşə_qəbul_tarixi.Split('.');
                                RecruitmentDate = Convert.ToDateTime(dates[0] + "." + dates[1] + "." + dates[2]);
                            }
                            else if (a.İşə_qəbul_tarixi != null && a.İşə_qəbul_tarixi.Contains("/"))
                            {
                                dates = a.İşə_qəbul_tarixi.Split('/');
                                RecruitmentDate = Convert.ToDateTime(dates[0] + "." + dates[1] + "." + dates[2]);
                            }
                            else if (a.İşə_qəbul_tarixi != null && a.İşə_qəbul_tarixi.Contains("-"))
                            {
                                dates = a.İşə_qəbul_tarixi.Split('-');
                                RecruitmentDate = Convert.ToDateTime(dates[0] + "." + dates[1] + "." + dates[2]);
                            }
                            else
                            {
                                RecruitmentDate = null;
                            }

                            if (a.İşə_qəbul_tarixi != null && RecruitmentDate != null)
                            {
                                drv.RecruitmentDate = RecruitmentDate;
                            }
                            //End

                            //Deactivation Date
                            string[] datesD;
                            if (a.İşdən_çıxma_tarixi != null && a.İşdən_çıxma_tarixi.Contains("."))
                            {
                                datesD = a.İşdən_çıxma_tarixi.Split('.');
                                DeactivationDate = Convert.ToDateTime(datesD[0] + "." + datesD[1] + "." + datesD[2]);
                            }
                            else if (a.İşdən_çıxma_tarixi != null && a.İşdən_çıxma_tarixi.Contains("/"))
                            {
                                datesD = a.İşdən_çıxma_tarixi.Split('/');
                                DeactivationDate = Convert.ToDateTime(datesD[0] + "." + datesD[1] + "." + datesD[2]);
                            }
                            else if (a.İşdən_çıxma_tarixi != null && a.İşdən_çıxma_tarixi.Contains("-"))
                            {
                                datesD = a.İşdən_çıxma_tarixi.Split('-');
                                DeactivationDate = Convert.ToDateTime(datesD[0] + "." + datesD[1] + "." + datesD[2]);
                            }
                            else
                            {
                                DeactivationDate = null;
                            }

                            if (a.İşdən_çıxma_tarixi != null && DeactivationDate != null)
                            {
                                drv.DeactivationDate = DeactivationDate;
                            }
                            //End

                            //Licence Issue Date
                            string[] datesL;
                            if (a.SV_Verilmə_tarixi != null && a.SV_Verilmə_tarixi.Contains("."))
                            {
                                datesL = a.SV_Verilmə_tarixi.Split('.');
                                LicenceIssueDate = Convert.ToDateTime(datesL[0] + "." + datesL[1] + "." + datesL[2]);
                            }
                            else if (a.SV_Verilmə_tarixi != null && a.SV_Verilmə_tarixi.Contains("/"))
                            {
                                datesL = a.SV_Verilmə_tarixi.Split('/');
                                LicenceIssueDate = Convert.ToDateTime(datesL[0] + "." + datesL[1] + "." + datesL[2]);
                            }
                            else if (a.SV_Verilmə_tarixi != null && a.SV_Verilmə_tarixi.Contains("-"))
                            {
                                datesL = a.SV_Verilmə_tarixi.Split('-');
                                LicenceIssueDate = Convert.ToDateTime(datesL[0] + "." + datesL[1] + "." + datesL[2]);
                            }
                            else
                            {
                                LicenceIssueDate = null;
                            }

                            if (a.SV_Verilmə_tarixi != null && LicenceIssueDate != null)
                            {
                                drv.LicenceIssueDate = LicenceIssueDate;
                            }
                            //End

                            //Licence Expire Date
                            string[] dates2;
                            if (a.SV_Etibarlılıq_tarixi != null && a.SV_Etibarlılıq_tarixi.Contains("."))
                            {
                                dates2 = a.SV_Etibarlılıq_tarixi.Split('.');
                                LicenceExpireDate = Convert.ToDateTime(dates2[0] + "." + dates2[1] + "." + dates2[2]);
                            }
                            else if (a.SV_Etibarlılıq_tarixi != null && a.SV_Etibarlılıq_tarixi.Contains("/"))
                            {
                                dates2 = a.SV_Etibarlılıq_tarixi.Split('/');
                                LicenceExpireDate = Convert.ToDateTime(dates2[0] + "." + dates2[1] + "." + dates2[2]);
                            }
                            else if (a.SV_Etibarlılıq_tarixi != null && a.SV_Etibarlılıq_tarixi.Contains("-"))
                            {
                                dates2 = a.SV_Etibarlılıq_tarixi.Split('-');
                                LicenceExpireDate = Convert.ToDateTime(dates2[0] + "." + dates2[1] + "." + dates2[2]);
                            }
                            else
                            {
                                LicenceExpireDate = null;
                            }

                            if (a.SV_Etibarlılıq_tarixi != null && LicenceExpireDate != null)
                            {
                                drv.LicenceExpireDate = LicenceExpireDate;
                            }
                            //End

                            db.Entry(drv).State = EntityState.Modified;
                            db.Entry(drv).Property(x => x.EmployeeCode).IsModified = false;

                            db.SaveChanges();
                        }
                    }

                    System.IO.File.Delete(pathToExcelFile);
                }

            }
            return RedirectToAction("EmpInpIndex");
        }
        ///////////////////////////////////////////////////

        //Export to Excel
        public void ExportEmployeesData()
        {
            ViewHome model = new ViewHome();
            model = (ViewHome)TempData["sortedDrivers"];

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Employee List");

            //Values
            ws.Range("B2:O2").Merge();
            ws.Cell("B2").Value = "İşçilərin Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "İşçi Kodu";
            ws.Cell("D3").Value = "Soyad, ad, ata adı";
            ws.Cell("E3").Value = "Doğum tarixi";
            ws.Cell("F3").Value = "İşə qəbul tarixi";
            ws.Cell("G3").Value = "Vəzifə";
            ws.Cell("H3").Value = "Telefon";
            ws.Cell("I3").Value = "Sürücülük vəsiqəsinin kateqoriyası";
            ws.Cell("J3").Value = "Sürücülük vəsiqəsinin seriyası";
            ws.Cell("K3").Value = "Sürücülük vəsiqəsinin nömrəsi";
            ws.Cell("L3").Value = "Sürücülük vəsiqəsinin verilmə tarixi";
            ws.Cell("M3").Value = "Sürücülük vəsiqəsinin etibarlılıq tarixi";
            ws.Cell("N3").Value = "Status";
            ws.Cell("O3").Value = "İşdən çıxma tarixi";


            //Styles
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

            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 5;
            ws.Column("C").Width = 7;
            ws.Column("D").Width = 30;
            ws.Column("E").Width = 10;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 20;
            ws.Column("H").Width = 30;
            ws.Column("I").Width = 20;
            ws.Column("J").Width = 20;
            ws.Column("K").Width = 20;
            ws.Column("L").Width = 20;
            ws.Column("M").Width = 20;
            ws.Column("N").Width = 7;
            ws.Column("O").Width = 11;

            int i = 4;
            foreach (var item in model.Employees)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.EmployeeCode);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;

                ws.Cell("D" + i).Value = (item.Surname == null ? "" : item.Surname) + " " + (item.Name == null ? "" : item.Name) + " " + (item.FutherName == null ? "" : item.FutherName);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                DateTime birthDate = new DateTime();
                if (item.BirthDate != null)
                {
                    birthDate = DateTime.ParseExact(item.BirthDate.Value.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    ws.Cell("E" + i).Value = birthDate.Date;
                }
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                DateTime recDate = new DateTime();
                if (item.RecruitmentDate != null)
                {
                    recDate = DateTime.ParseExact(item.RecruitmentDate.Value.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    ws.Cell("F" + i).Value = recDate.Date;
                }
                ws.Cell("F" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("F" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("F" + i).Style.Alignment.WrapText = true;

                ws.Cell("G" + i).Value = (item.PositionId == null ? "" : item.Positions.Name);
                ws.Cell("G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("G" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("G" + i).Style.Alignment.WrapText = true;

                ws.Cell("H" + i).Value = (item.Phone == null ? "" : item.Phone);
                ws.Cell("H" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("H" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("H" + i).Style.Alignment.WrapText = true;

                ws.Cell("I" + i).Value = (item.LicenceCategory == null ? "" : item.LicenceCategory);
                ws.Cell("I" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("I" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("I" + i).Style.Alignment.WrapText = true;

                ws.Cell("J" + i).Value = (item.LicenceSeries == null ? "" : item.LicenceSeries);
                ws.Cell("J" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("J" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("J" + i).Style.Alignment.WrapText = true;

                ws.Cell("K" + i).Value = (item.LicenceNumber == null ? "" : (item.LicenceNumber.Length == 5 ? ("'0" + item.LicenceNumber) : "'" + item.LicenceNumber));
                ws.Cell("K" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("K" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("K" + i).Style.Alignment.WrapText = true;

                DateTime LicIssueDate = new DateTime();
                if (item.LicenceIssueDate != null)
                {
                    LicIssueDate = DateTime.ParseExact(item.LicenceIssueDate.Value.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    ws.Cell("L" + i).Value = LicIssueDate.Date;
                }
                ws.Cell("L" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("L" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("L" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("L" + i).Style.Alignment.WrapText = true;

                DateTime LicExpDate = new DateTime();
                if (item.LicenceExpireDate != null)
                {
                    LicExpDate = DateTime.ParseExact(item.LicenceExpireDate.Value.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    ws.Cell("M" + i).Value = LicExpDate.Date;
                }
                ws.Cell("M" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("M" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("M" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("M" + i).Style.Alignment.WrapText = true;

                ws.Cell("N" + i).Value = (item.Status == true ? "Aktiv" : "Passiv");
                ws.Cell("N" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("N" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("N" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("N" + i).Style.Alignment.WrapText = true;

                DateTime DeactDate = new DateTime();
                if (item.DeactivationDate != null)
                {
                    DeactDate = DateTime.ParseExact(item.DeactivationDate.Value.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    ws.Cell("O" + i).Value = DeactDate.Date;
                }
                ws.Cell("O" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("O" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("O" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("O" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"EmployeeList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }



        //Vehicle Input

        [OperationFilter]
        public ActionResult VehInpIndex(string searchData,int page=1)
        {
            ViewBag.Sys = true;
            ViewBag.SearchData = searchData;

            ViewHome model = new ViewHome();

            if (searchData != null)
            {
              model.Vehicles = db.Vehicles.Where(d =>
                                                         d.VehicleCode.ToString().Contains(searchData) == true
                                                      || d.Number.Contains(searchData) == true
                                                      || d.ReleaseYear.ToString().Contains(searchData) == true
                                                      || d.NumberOfSeats.ToString().Contains(searchData) == true
                                                      || d.Capacity.ToString().Contains(searchData) == true
                                                      || d.RegistrationCertificationSeries.Contains(searchData) == true
                                                      || d.RegistrationCertificationNumber.Contains(searchData) == true
                                                      || d.ChassisNumber.Contains(searchData) == true
                                                      || d.VehiclesBrand.Brand.Contains(searchData) == true
                                                       ).ToList();
            }
            else
            {
                model.Vehicles = db.Vehicles.ToList();
            }

            model.VehiclesBrand = db.VehiclesBrand.ToList();

            int limit = 10;
            if (limit > model.Vehicles.Count())
            {
                limit = model.Vehicles.Count();
            }

            ViewBag.total = (int)Math.Ceiling(((model.Vehicles.Count() > 0 ? model.Vehicles.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul


            model.Vehicles = model.Vehicles.Skip(((int)page - 1) * limit).Take(limit);

            TempData["sortedVehicles"] = model;
            return View(model);
        }

        [OperationFilter]
        public ActionResult VehInpCreate()
        {
            ViewBag.Sys = true;

            ViewHome model = new ViewHome
            {
                VehiclesBrand = db.VehiclesBrand.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult VehInpCreate(Vehicles veh)
        {
            List<Vehicles> VehList = db.Vehicles.ToList();
            foreach (var item in VehList)
            {
                if ((item.VehicleCode == veh.VehicleCode))
                {
                    Session["InvalidVehCode"] = true;
                    return RedirectToAction("VehInpCreate");
                }
                if ((item.Number == veh.Number))
                {
                    Session["InvalidVehNum"] = true;
                    return RedirectToAction("VehInpCreate");
                }
                if (item.ChassisNumber != null && item.ChassisNumber == veh.ChassisNumber)
                {
                    Session["InvalidVehCha"] = true;
                    return RedirectToAction("VehInpCreate");
                }

                if (item.EngineCode != null && item.EngineCode == veh.EngineCode)
                {
                    Session["InvalidVehEng"] = true;
                    return RedirectToAction("VehInpCreate");
                }
            }

            if (veh.BrandId == 0)
            {
                veh.BrandId = null;
            }
            if (veh == null)
            {
                return RedirectToAction("VehInpIndex");
            }
            db.Vehicles.Add(veh);
            db.SaveChanges();
            return RedirectToAction("VehInpIndex");
        }

        [OperationFilter]
        public ActionResult VehInpUpdate(int id)
        {
            ViewBag.Sys = true;
            Vehicles veh = db.Vehicles.Find(id);
            if (veh == null)
            {
                return RedirectToAction("VehInpIndex");
            }

            ViewHome model = new ViewHome
            {
                VehiclesBrand = db.VehiclesBrand.ToList()
            };                        
            ViewBag.Veh = veh;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult VehInpUpdate(Vehicles veh)
        {
            Vehicles vcl = db.Vehicles.Find(veh.Id);
            if (vcl == null)
            {
                return RedirectToAction("VehInpIndex");
            }

            List<Vehicles> VehList = db.Vehicles.ToList();
            foreach (var item in VehList)
            {
                if ((item.VehicleCode == veh.VehicleCode) && (item.Id != veh.Id))
                {
                    Session["InvalidVehCode"] = true;
                    return RedirectToAction("VehInpUpdate");
                }
                if ((item.Number == veh.Number) && (item.Id != veh.Id))
                {
                    Session["InvalidVehNum"] = true;
                    return RedirectToAction("VehInpUpdate");
                }
                if (item.ChassisNumber != null && item.ChassisNumber == veh.ChassisNumber && item.Id != veh.Id)
                {
                    Session["InvalidVehCha"] = true;
                    return RedirectToAction("VehInpUpdate");
                }

                if (item.EngineCode != null && item.EngineCode == veh.EngineCode && item.Id != veh.Id)
                {
                    Session["InvalidVehEng"] = true;
                    return RedirectToAction("VehInpUpdate");
                }
            }

            if (veh.BrandId == 0)
            {
                vcl.BrandId = null;
            }
            else
            {
                vcl.BrandId = veh.BrandId;
            }
            vcl.Capacity = veh.Capacity;
            vcl.ChassisNumber = veh.ChassisNumber;
            vcl.EngineCode = veh.EngineCode;
            vcl.Number = veh.Number;
            vcl.NumberOfSeats = veh.NumberOfSeats;
            vcl.RegistrationCertificationNumber = veh.RegistrationCertificationNumber;
            vcl.RegistrationCertificationSeries = veh.RegistrationCertificationSeries;
            vcl.ReleaseYear = veh.ReleaseYear;
            vcl.VehicleCode = veh.VehicleCode;            

            if (ModelState.IsValid)
            {
                db.Entry(vcl).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("VehInpIndex");
            }
            return RedirectToAction("VehInpIndex");
        }

        [OperationFilter]
        public ActionResult VehInpDelete(int id)
        {
            Vehicles veh = db.Vehicles.Find(id);
            InitialControlSchedule InitCon = db.InitialControlSchedule.FirstOrDefault(i => i.VehicleId == id);
            if (veh == null || InitCon!=null)
            {
                return RedirectToAction("VehInpIndex");
            }

            db.Vehicles.Remove(veh);
            db.SaveChanges();
            return RedirectToAction("VehInpIndex");
        }

        //Download Tamplate
        [OperationFilter]
        public ActionResult VehicleExcelTemp()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/Uploads/Templates/";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + "VehicleExcelTemplate.xlsx");
            string fileName = "VehicleExcelTemplate.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        //Upload Vehicles' Excel file
        [OperationFilter]
        public ActionResult UploadVehicleExcel()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult UploadVehicleExcel(HttpPostedFileBase UploadedExcel)
        {
            ViewBag.Sys = true;
            if (UploadedExcel != null)
            {
                if (UploadedExcel.ContentType == "application/vnd.ms-excel" || UploadedExcel.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = UploadedExcel.FileName;
                    string targetpath = Server.MapPath("~/Uploads/Temp/");
                    UploadedExcel.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;

                    string sheetName = "Sheet1";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var VehiclesData = from a in excelFile.Worksheet<UploadVehicles>(sheetName) select a;

                    //Checking for dublicates in uploaded file

                    List<string> VehiclesDataListNVKode = new List<string>();
                    List<string> VehiclesDataListDQN = new List<string>();
                    List<string> VehiclesDataListChassi = new List<string>();
                    List<string> VehiclesDataListEngine = new List<string>();

                    foreach (var item in VehiclesData)
                    {
                        if (item.NV_Kodu != null)
                        {
                            VehiclesDataListNVKode.Add(item.NV_Kodu.ToString());
                        }
                        if (item.DQN != null)
                        {
                            VehiclesDataListDQN.Add(item.DQN.ToString());
                        }
                        if (item.Şassi_Nomresi != null)
                        {
                            VehiclesDataListChassi.Add(item.Şassi_Nomresi.ToString());
                        }
                        if (item.Muherrik_Nomresi != null)
                        {
                            VehiclesDataListEngine.Add(item.Muherrik_Nomresi.ToString());
                        }
                    }

                    for (int i = 0; i < VehiclesDataListNVKode.Count; i++)
                    {
                        if (i > 0 && (VehiclesDataListNVKode[i] == VehiclesDataListNVKode[i - 1]))
                        {
                            TempData["RepeatedNVCode"] += VehiclesDataListNVKode[i] + " ";
                        }
                    }

                    for (int i = 0; i < VehiclesDataListDQN.Count; i++)
                    {
                        if (i > 0 && (VehiclesDataListDQN[i] == VehiclesDataListDQN[i - 1]))
                        {
                            TempData["RepeatedDQN"] += VehiclesDataListDQN[i] + " ";
                        }
                    }

                    for (int i = 0; i < VehiclesDataListChassi.Count; i++)
                    {
                        if (i > 0 && (VehiclesDataListChassi[i] == VehiclesDataListChassi[i - 1]))
                        {
                            TempData["RepeatedChassi"] += VehiclesDataListChassi[i] + " ";
                        }
                    }

                    for (int i = 0; i < VehiclesDataListEngine.Count; i++)
                    {
                        if (i > 0 && (VehiclesDataListEngine[i] == VehiclesDataListEngine[i - 1]))
                        {
                            TempData["RepeatedEngine"] += VehiclesDataListEngine[i] + " ";
                        }
                    }


                    if (TempData["RepeatedNVCode"] != null)
                    {
                        Session["RepeatedNVCode"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }

                    if (TempData["RepeatedDQN"] != null)
                    {
                        Session["RepeatedDQN"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }

                    if (TempData["RepeatedChassi"] != null)
                    {
                        Session["RepeatedChassi"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }

                    if (TempData["RepeatedEngine"] != null)
                    {
                        Session["RepeatedEngine"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }


                    //Checking for dublicates in db
                    List<Vehicles> vehiclesList = db.Vehicles.ToList();
                    bool result = false;

                    foreach (var item in vehiclesList)
                    {
                        foreach (var item2 in VehiclesData)
                        {
                            if ((item2.NV_Kodu == item.VehicleCode) && (item2.NV_Kodu != ""))
                            {
                                TempData["RepeatedCodes"] += item2.NV_Kodu + " ";
                            }
                            if ((item2.DQN == item.Number) && (item2.DQN != ""))
                            {
                                TempData["RepeatedNumber"] += item2.DQN + " ";
                            }

                            if (item2.Şassi_Nomresi == item.ChassisNumber && item2.Şassi_Nomresi != "" && item2.Şassi_Nomresi != null)
                            {
                                TempData["RepeatedChassi"] += item2.Şassi_Nomresi + " ";
                            }

                            if (item2.Muherrik_Nomresi == item.EngineCode && item2.Muherrik_Nomresi != "" && item2.Muherrik_Nomresi != null)
                            {
                                TempData["RepeatedEngine"] += item2.Muherrik_Nomresi + " ";
                            }

                            if (((item2.NV_Kodu == null) && (item2.Marka != null || item2.Buraxılış_ili != 0)) || ((item2.DQN == null) && (item2.Marka != null || item2.Buraxılış_ili != 0)))
                            {
                                result = true;
                            }
                        }
                    }

                    if (TempData["RepeatedCodes"] != null)
                    {
                        Session["RepeatedCodes"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }

                    if (TempData["RepeatedNumber"] != null)
                    {
                        Session["RepeatedNumber"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }

                    if (TempData["RepeatedChassi"] != null)
                    {
                        Session["RepeatedChassi"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }

                    if (TempData["RepeatedEngine"] != null)
                    {
                        Session["RepeatedEngine"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }

                    if (result == true)
                    {
                        Session["EmptyVehicleCode"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadVehicleExcel");
                    }


                    //Adding Drivers' data to database
                    foreach (var a in VehiclesData)
                    {
                        if (a.NV_Kodu != null && a.DQN != null)
                        {
                            Vehicles veh = new Vehicles();
                            if (db.VehiclesBrand.FirstOrDefault(b => b.Brand.Contains(a.Marka)) == null)
                            {
                                veh.BrandId = null;
                            }
                            else
                            {
                                veh.BrandId = db.VehiclesBrand.FirstOrDefault(b => b.Brand.Contains(a.Marka)).Id;
                            }
                            veh.VehicleCode = a.NV_Kodu;
                            veh.Number = a.DQN;
                            veh.ReleaseYear = a.Buraxılış_ili;
                            veh.Capacity = a.Tutum;
                            veh.NumberOfSeats = a.Oturacaq;
                            veh.RegistrationCertificationSeries = a.Texposport_Seriyası;
                            veh.RegistrationCertificationNumber = a.Texposport_Nomresi;
                            veh.ChassisNumber = a.Şassi_Nomresi;

                            db.Vehicles.Add(veh);
                            db.SaveChanges();
                        }
                    }

                    System.IO.File.Delete(pathToExcelFile);
                }
            }
            return RedirectToAction("VehInpIndex");
        }

        //Download to excel
        public void ExportVehiclesData()
        {
            ViewHome model = new ViewHome();
            model = (ViewHome)TempData["sortedVehicles"];

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Vehicles List");

            //Values
            ws.Range("B2:K2").Merge();
            ws.Cell("B2").Value = "Nəqliyyat Vasitələrinin Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "NV Kodu";
            ws.Cell("D3").Value = "NV Nömrəsi";
            ws.Cell("E3").Value = "Marka və Model";
            ws.Cell("F3").Value = "İstehsal İli";
            ws.Cell("G3").Value = "Oturacaq sayı";
            ws.Cell("H3").Value = "Tutumu";
            ws.Cell("I3").Value = "Texpasport seriyası";
            ws.Cell("J3").Value = "Texpasport nömrəsi";
            ws.Cell("K3").Value = "Şassi nömrəsi";


            //Styles
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

            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 12;
            ws.Column("D").Width = 12;
            ws.Column("E").Width = 20;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 10;
            ws.Column("H").Width = 10;
            ws.Column("I").Width = 12;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 20;

            int i = 4;
            foreach (var item in model.Vehicles)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.VehicleCode);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;

                ws.Cell("D" + i).Value = (item.Number == null ? "" : item.Number);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;

                ws.Cell("E" + i).Value = (item.VehiclesBrand.Brand == null ? "" : item.VehiclesBrand.Brand);
                ws.Cell("E" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                ws.Cell("f" + i).Value = (item.ReleaseYear == null ? 0 : item.ReleaseYear);
                ws.Cell("F" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("F" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("F" + i).Style.Alignment.WrapText = true;

                ws.Cell("G" + i).Value = (item.NumberOfSeats == null ? 0 : item.NumberOfSeats);
                ws.Cell("G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("G" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("G" + i).Style.Alignment.WrapText = true;

                ws.Cell("H" + i).Value = (item.Capacity == null ? 0 : item.Capacity);
                ws.Cell("H" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("H" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("H" + i).Style.Alignment.WrapText = true;

                ws.Cell("I" + i).Value = (item.RegistrationCertificationSeries == null ? "" : item.RegistrationCertificationSeries);
                ws.Cell("I" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("I" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("I" + i).Style.Alignment.WrapText = true;

                ws.Cell("J" + i).Value = (item.RegistrationCertificationNumber == null ? "" : item.RegistrationCertificationNumber);
                ws.Cell("J" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("J" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("J" + i).Style.Alignment.WrapText = true;

                ws.Cell("K" + i).Value = (item.ChassisNumber == null ? "" : item.ChassisNumber);
                ws.Cell("K" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("K" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("K" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("K" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"VehiclesList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Vehicle Brand Input

        [OperationFilter]
        public ActionResult BraInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                VehiclesBrand = db.VehiclesBrand.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult BraInpCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BraInpCreate(VehiclesBrand vehBra)
        {
            if (vehBra == null)
            {
                return RedirectToAction("BraInpIndex");
            }
            db.VehiclesBrand.Add(vehBra);
            db.SaveChanges();
            return RedirectToAction("BraInpIndex");
        }

        [OperationFilter]
        public ActionResult BraInpUpdate(int id)
        {
            ViewBag.Sys = true;
            VehiclesBrand vehBra = db.VehiclesBrand.Find(id);
            if (vehBra == null)
            {
                return RedirectToAction("BraInpIndex");
            }

            ViewBag.VehBra = vehBra;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult BraInpUpdate(VehiclesBrand vehBra)
        {
            if (vehBra == null)
            {
                return RedirectToAction("BraInpIndex");
            }

            if (ModelState.IsValid)
            {
                db.Entry(vehBra).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("BraInpIndex");
            }
            return RedirectToAction("BraInpIndex");
        }

        [OperationFilter]
        public ActionResult BraInpDelete(int id)
        {
            VehiclesBrand vehBra = db.VehiclesBrand.Find(id);
            Vehicles veh = db.Vehicles.FirstOrDefault(b => b.BrandId == id);
            if (veh != null)
            {
                return RedirectToAction("BraInpIndex");
            }
            if (vehBra == null)
            {
                return RedirectToAction("BraInpIndex");
            }

            db.VehiclesBrand.Remove(vehBra);
            db.SaveChanges();
            return RedirectToAction("BraInpIndex");
        }

        public void ExportBrandData()
        {
            ViewHome model = new ViewHome();
            model.VehiclesBrand = db.VehiclesBrand.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Vehicle Brands List");

            ws.Range("B2:C2").Merge();
            ws.Cell("B2").Value = "Marka və Model Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Marka və Model";

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


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;

            int i = 4;
            foreach (var item in model.VehiclesBrand)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.Brand == null ? "" : item.Brand);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"BrandsList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Departments
        [OperationFilter]
        public ActionResult DepInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Departments = db.Departments.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult DepInpCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult DepInpCreate(Departments dep)
        {
            db.Departments.Add(dep);
            db.SaveChanges();
            return RedirectToAction("DepInpIndex");
        }

        [OperationFilter]
        public ActionResult DepInpUpdate(int id)
        {
            ViewBag.Sys = true;
            Departments dep = db.Departments.Find(id);
            if (dep == null)
            {
                return RedirectToAction("DepInpIndex");
            }

            ViewBag.Dep = dep;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult DepInpUpdate(Departments dep)
        {
            if (dep == null)
            {
                return RedirectToAction("DepInpIndex");
            }

            if (ModelState.IsValid)
            {
                db.Entry(dep).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DepInpIndex");
            }
            return RedirectToAction("DepInpIndex");
        }

        [OperationFilter]
        public ActionResult DepInpDelete(int id)
        {
            Departments dep = db.Departments.Find(id);
            Employees depEmp = db.Employees.FirstOrDefault(l => l.DepartmentId == id);
            Sectors depSec = db.Sectors.FirstOrDefault(l => l.DepartmentId == id);
            if (depEmp != null || depSec != null || dep == null)
            {
                Session["InvalidDelete"] = true;
                return RedirectToAction("DepInpIndex");
            }

            db.Departments.Remove(dep);
            db.SaveChanges();
            return RedirectToAction("DepInpIndex");
        }

        public void ExportDepartmentsData()
        {
            ViewHome model = new ViewHome();
            model.Departments = db.Departments.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Department List");

            ws.Range("B2:C2").Merge();
            ws.Cell("B2").Value = "Şöbələrin Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Şöbə";

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


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;

            int i = 4;
            foreach (var item in model.Departments)
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

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"DepartmentsList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Sectors
        [OperationFilter]
        public ActionResult SecInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Sectors = db.Sectors.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult SecInpCreate()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome();
            model.Departments = db.Departments.ToList();
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult SecInpCreate(Sectors sec)
        {
            db.Sectors.Add(sec);
            db.SaveChanges();
            return RedirectToAction("SecInpIndex");
        }

        [OperationFilter]
        public ActionResult SecInpUpdate(int id)
        {
            ViewBag.Sys = true;
            Sectors sec = db.Sectors.Find(id);
            ViewHome model = new ViewHome()
            {
                Departments = db.Departments.ToList()
            };

            if (sec == null)
            {
                return RedirectToAction("SecInpIndex");
            }

            ViewBag.Sec = sec;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult SecInpUpdate(Sectors sec)
        {
            if (sec == null)
            {
                return RedirectToAction("SecInpIndex");
            }
            if (sec.DepartmentId==0)
            {
                sec.DepartmentId = null;
            }

            if (ModelState.IsValid)
            {
                db.Entry(sec).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("SecInpIndex");
            }
            return RedirectToAction("SecInpIndex");
        }

        [OperationFilter]
        public ActionResult SecInpDelete(int id)
        {
            Sectors sec = db.Sectors.Find(id);
            Employees depEmp = db.Employees.FirstOrDefault(l => l.SectorId == id);
            if (depEmp != null || sec == null)
            {
                Session["InvalidDelete"] = true;
                return RedirectToAction("SecInpIndex");
            }

            db.Sectors.Remove(sec);
            db.SaveChanges();
            return RedirectToAction("SecInpIndex");
        }

        public void ExportSectorsData()
        {
            ViewHome model = new ViewHome();
            model.Sectors = db.Sectors.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sector List");

            ws.Range("B2:D2").Merge();
            ws.Cell("B2").Value = "Sektorların Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Sektor";
            ws.Cell("D3").Value = "Şöbə";

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
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;
            ws.Column("D").Width = 60;

            int i = 4;
            foreach (var item in model.Sectors)
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

                ws.Cell("D" + i).Value = (item.DepartmentId == null ? "" : item.Departments.Name);
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
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"SecctorsList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Positions
        [OperationFilter]
        public ActionResult PosInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Positions = db.Positions.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult PosInpCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult PosInpCreate(Positions pos)
        {
            db.Positions.Add(pos);
            db.SaveChanges();
            return RedirectToAction("PosInpIndex");
        }

        [OperationFilter]
        public ActionResult PosInpUpdate(int id)
        {
            ViewBag.Sys = true;
            Positions pos = db.Positions.Find(id);
            if (pos == null)
            {
                return RedirectToAction("PosInpIndex");
            }

            ViewBag.Pos = pos;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult PosInpUpdate(Positions pos)
        {
            if (pos == null)
            {
                return RedirectToAction("PosInpIndex");
            }

            if (ModelState.IsValid)
            {
                db.Entry(pos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PosInpIndex");
            }
            return RedirectToAction("PosInpIndex");
        }

        [OperationFilter]
        public ActionResult PosInpDelete(int id)
        {
            Positions pos = db.Positions.Find(id);
            Employees posEmp = db.Employees.FirstOrDefault(l => l.PositionId == id);
            if (posEmp != null)
            {
                Session["InvalidDelete"] = true;
                return RedirectToAction("PosInpIndex");
            }

            db.Positions.Remove(pos);
            db.SaveChanges();
            return RedirectToAction("PosInpIndex");
        }

        public void ExportPositionsData()
        {
            ViewHome model = new ViewHome();
            model.Positions = db.Positions.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Position List");

            ws.Range("B2:C2").Merge();
            ws.Cell("B2").Value = "Vəzifələrin Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Vəzifə";

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


            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;

            int i = 4;
            foreach (var item in model.Positions)
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

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"PositionsList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }


        //Route km Limit
        [OperationFilter]
        public ActionResult KmLimitInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                RouteDailyKM = db.RouteDailyKM.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult KmLimitInpCreate()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome();
            model.Routes = db.Routes.ToList();
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult KmLimitInpCreate(RouteDailyKM limit)
        {
            db.RouteDailyKM.Add(limit);
            db.SaveChanges();
            return RedirectToAction("KmLimitInpIndex");
        }

        [OperationFilter]
        public ActionResult KmLimitInpUpdate(int id)
        {
            ViewBag.Sys = true;
            RouteDailyKM limit = db.RouteDailyKM.Find(id);
            ViewHome model = new ViewHome()
            {
                Routes = db.Routes.ToList()
            };

            if (limit == null)
            {
                return RedirectToAction("KmLimitInpIndex");
            }

            ViewBag.limit = limit;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult KmLimitInpUpdate(RouteDailyKM limit)
        {
            if (limit == null)
            {
                return RedirectToAction("KmLimitInpIndex");
            }
            if (limit.RouteId == 0)
            {
                limit.RouteId = null;
            }

            if (ModelState.IsValid)
            {
                db.Entry(limit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("KmLimitInpIndex");
            }
            return RedirectToAction("KmLimitInpIndex");
        }

        [OperationFilter]
        public ActionResult KmLimitInpDelete(int id)
        {
            RouteDailyKM limit = db.RouteDailyKM.Find(id);
            if (limit == null)
            {
                Session["InvalidDelete"] = true;
                return RedirectToAction("KmLimitInpIndex");
            }

            db.RouteDailyKM.Remove(limit);
            db.SaveChanges();
            return RedirectToAction("KmLimitInpIndex");
        }

        //Rufat
        public void ExportKmLimitData()
        {
            ViewHome model = new ViewHome();
            model.RouteDailyKM = db.RouteDailyKM.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("KmLimit List");

            ws.Range("B2:D2").Merge();
            ws.Cell("B2").Value = "Xətlərin məsafə limiti Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Xətt";
            ws.Cell("D3").Value = "Məsafə Limiti";


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
            foreach (var item in model.RouteDailyKM)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.Routes.Number == null ? "" : item.Routes.Number);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;


                ws.Cell("D" + i).Value = (item.KmLimit == null ? null : item.KmLimit);
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
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"KmLimitList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }
        //Rufat-End


        //Depots
        [OperationFilter]
        public ActionResult DepotInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                Depots = db.Depots.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult DepotInpCreate()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome();
            model.Companies = db.Companies.ToList();
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult DepotInpCreate(Depots depot)
        {
            db.Depots.Add(depot);
            db.SaveChanges();
            return RedirectToAction("DepotInpIndex");
        }

        [OperationFilter]
        public ActionResult DepotInpUpdate(int id)
        {
            ViewBag.Sys = true;
            Depots depot = db.Depots.Find(id);
            ViewHome model = new ViewHome()
            {
                Companies = db.Companies.ToList()
            };

            if (depot == null)
            {
                return RedirectToAction("DepotInpIndex");
            }

            ViewBag.Depot = depot;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult DepotInpUpdate(Depots depot)
        {
            if (depot == null)
            {
                return RedirectToAction("DepotInpIndex");
            }
            if (depot.CompanyId == 0)
            {
                depot.CompanyId = null;
            }

            if (ModelState.IsValid)
            {
                db.Entry(depot).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DepotInpIndex");
            }
            return RedirectToAction("DepotInpIndex");
        }

        [OperationFilter]
        public ActionResult DepotInpDelete(int id)
        {
            Depots depot = db.Depots.Find(id);
            //Employees depEmp = db.Employees.FirstOrDefault(l => l.SectorId == id);
            if (depot == null)
            {
                Session["InvalidDelete"] = true;
                return RedirectToAction("DepotInpIndex");
            }

            db.Depots.Remove(depot);
            db.SaveChanges();
            return RedirectToAction("DepotInpIndex");
        }

        //Rufat
        public void ExportDepotsData()
        {
            ViewHome model = new ViewHome();
            model.Depots = db.Depots.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Depots List");

            ws.Range("B2:D2").Merge();
            ws.Cell("B2").Value = "Depoların Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Depo";
            ws.Cell("D3").Value = "Şirkət";


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
            foreach (var item in model.Depots)
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


                ws.Cell("D" + i).Value = (item.Companies.Name == null ? "" : item.Companies.Name);
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
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"DepotsList.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }
            httpResponse.End();
        }

        //Rufat-End

        //Troubleshoot
        [OperationFilter]
        public ActionResult TroInpIndex()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome
            {
                TroubleShoot = db.TroubleShoot.ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult TroInpCreate()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult TroInpCreate(TroubleShoot tro)
        {
            db.TroubleShoot.Add(tro);
            db.SaveChanges();
            return RedirectToAction("TroInpIndex");
        }

        [OperationFilter]
        public ActionResult TroInpUpdate(int id)
        {
            ViewBag.Sys = true;
            TroubleShoot tro = db.TroubleShoot.Find(id);
            if (tro == null)
            {
                return RedirectToAction("TroInpIndex");
            }

            ViewBag.Tro = tro;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult TroInpUpdate(TroubleShoot tro)
        {
            if (tro == null)
            {
                return RedirectToAction("TroInpIndex");
            }

            if (ModelState.IsValid)
            {
                db.Entry(tro).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("TroInpIndex");
            }
            return RedirectToAction("TroInpIndex");
        }

        [OperationFilter]
        public ActionResult TroInpDelete(int id)
        {
            TroubleShoot tro = db.TroubleShoot.Find(id);

            if (tro == null)
            {
                return RedirectToAction("TroInpIndex");
            }

            db.TroubleShoot.Remove(tro);
            db.SaveChanges();
            return RedirectToAction("TroInpIndex");
        }

        //Rufat
        public void ExportTroData()
        {
            ViewHome model = new ViewHome();
            model.TroubleShoot = db.TroubleShoot.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Troubleshoot List");

            ws.Range("B2:C2").Merge();
            ws.Cell("B2").Value = "Nasazlıqların Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "Nasazlıq növü";



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




            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 20;


            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 40;



            int i = 4;
            foreach (var item in model.TroubleShoot)
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




                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"TroubleShoot.xlsx\"");

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


        //Rufat
        //Done works input
        [OperationFilter]
        public ActionResult DoneWorksIndexInput(SearchModelDoneworks searchModelDoneworks, string searchModelString, int page = 1)
        {
            //Codes for pagination
            string[] searchModelList = null;
            string test = "";
            bool searchModelStatus = false;
            foreach (var item in searchModelDoneworks.GetType().GetProperties())
            {
                if (item.GetValue(searchModelDoneworks, null) != null)
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
                    searchModelDoneworks.Id = Convert.ToInt32(searchModelList[0]);
                }
                if (!string.IsNullOrEmpty(searchModelList[1]))
                {
                    searchModelDoneworks.VehicleBrandId = Convert.ToInt32(searchModelList[1]);
                }
              

            }
            //End


            ViewBag.Sys = true;
            ViewBag.Doneworks = db.DoneWorks.ToList();
            ViewBag.Brands = db.VehiclesBrand.ToList();
            ViewHome model = new ViewHome();

            model.DoneWorks = db.DoneWorks.Where(d=>(searchModelDoneworks.Id != null? d.Id==searchModelDoneworks.Id:true) &&
                                                    (searchModelDoneworks.VehicleBrandId!=null? d.VehicleBrandId==searchModelDoneworks.VehicleBrandId:true)
                                                    ).ToList();


            int limit = 10;
            if (limit > model.DoneWorks.Count())
            {
                limit = model.DoneWorks.Count();
            }

            ViewBag.total = (int)Math.Ceiling(((model.DoneWorks.Count() > 0 ? model.DoneWorks.Count() : 1) / (decimal)(limit > 0 ? limit : 1)));
            ViewBag.page = page;

            if (ViewBag.page > ViewBag.total)
            {
                return HttpNotFound();
            }
            //end of pagination modul


            model.DoneWorks = model.DoneWorks.Skip(((int)page - 1) * limit).Take(limit);
            model.SearchModelDoneworks = searchModelDoneworks;
            return View(model);
        }

        [OperationFilter]
        public ActionResult DoneWorksCreateInput()
        {
            ViewBag.Sys = true;
            ViewHome model = new ViewHome();
            model.VehiclesBrand = db.VehiclesBrand.ToList();
            return View(model);
        }

        [OperationFilter]
        [HttpPost]
        public ActionResult DoneWorksCreateInput(DoneWorks dw)
        {
            if (String.IsNullOrEmpty(dw.WorkCode))
            {
                return RedirectToAction("DoneWorksIndexInput");
            }
            List<DoneWorks> doneWorks = db.DoneWorks.ToList();
            foreach (var item in doneWorks)
            {
                if (item.WorkCode==dw.WorkCode)
                {
                    Session["DoneWorkRepeat"]= true;
                    return RedirectToAction("DoneWorksCreate");
                }
            }
            db.DoneWorks.Add(dw);
            db.SaveChanges();
            return RedirectToAction("DoneWorksIndexInput");
        }

        [OperationFilter]
        public ActionResult DoneWorksUpdateInput(int id)
        {

            ViewBag.Sys = true;
            DoneWorks dw = db.DoneWorks.Find(id);
            if (dw == null)
            {
                return RedirectToAction("DoneWorksIndexInput");
            }
            ViewDoneWorks model = new ViewDoneWorks();
            model.donework = dw;
            model.vehicleBrands = db.VehiclesBrand.ToList();


            return View(model);
        }

        [OperationFilter]
        [HttpPost]
        public ActionResult DoneWorksUpdateInput(DoneWorks dw1)
        {

            ViewBag.Sys = true;
            if (String.IsNullOrEmpty(dw1.WorkCode))
            {
                return RedirectToAction("DoneWorksIndexInput");
            }
            DoneWorks dw2 = db.DoneWorks.Find(dw1.Id);
            dw2.WorkCode = dw1.WorkCode;
            dw2.WorkName = dw1.WorkName;
            dw2.NormativeFactory = dw1.NormativeFactory;
            dw2.NormativeBakubus = dw1.NormativeBakubus;
            if (dw1.VehicleBrandId!=0)
            {
                dw2.VehicleBrandId = dw1.VehicleBrandId;
            }
            else
            {
                dw2.VehicleBrandId = null;
            }

            db.SaveChanges();
            return RedirectToAction("DoneWorksIndexInput");
        }

        [OperationFilter]
        public ActionResult DoneWorksDeleteInput(int id)
        {

            ViewBag.Sys = true;
            DoneWorks dw = db.DoneWorks.Find(id);
            if (dw == null)
            {
                return RedirectToAction("DoneWorksIndexInput");
            }
            db.DoneWorks.Remove(dw);
            db.SaveChanges();

            return RedirectToAction("DoneWorksIndexInput");


        }

        //Download excel template
        [OperationFilter]
        public ActionResult DoneWorksExcelTemp()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/Uploads/Templates/";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + "DoneWorksExcelListTemplate.xlsx");
            string fileName = "DoneWorksExcelListTemplate.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        
        //Upload excel file
        [OperationFilter]
        public ActionResult UploadDoneWorksExcelInput()
        {
            ViewBag.Sys = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult UploadDoneWorksExcelInput(HttpPostedFileBase UploadedExcel)
        {
            ViewBag.Sys = true;
            if (UploadedExcel != null)
            {

                if (UploadedExcel.ContentType == "application/vnd.ms-excel" || UploadedExcel.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    //return RedirectToAction("DoneWorksCreate");

                    string filename = UploadedExcel.FileName;
                    string targetpath = Server.MapPath("~/Uploads/Temp/");
                    UploadedExcel.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;

                    string sheetName = "Sheet1";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var DoneWorksData = from a in excelFile.Worksheet<UploadDoneWorksExcelModel>(sheetName) select a;

                    //Checking for dublicates in uploaded file
                    List<string> DoneWorksDataListWorkCode = new List<string>();
                    
                    foreach (var item in DoneWorksData)
                    {
                        if (!string.IsNullOrEmpty(item.İş_Kodu))
                        {
                            DoneWorksDataListWorkCode.Add(item.İş_Kodu.ToString());
                        }
                    }
                    DoneWorksDataListWorkCode.Sort();

                    for (int i = 0; i < DoneWorksDataListWorkCode.Count; i++)
                    {
                        if (i > 0 && (DoneWorksDataListWorkCode[i] == DoneWorksDataListWorkCode[i - 1]))
                        {
                            TempData["RepeatedWorkCodeFile"] += DoneWorksDataListWorkCode[i] + " ";
                        }
                    }

                    if (TempData["RepeatedWorkCodeFile"] != null)
                    {
                        Session["RepeatedWorkCodeFile"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadDoneWorksExcelInput");
                    }


                    //Checking for dublicates in db
                    List<DoneWorks> doneWorksList = db.DoneWorks.ToList();

                    foreach (var item in doneWorksList)
                    {
                        foreach (var item2 in DoneWorksData)
                        {
                            if ((item2.İş_Kodu == item.WorkCode) && (item2.İş_Kodu != ""))
                            {
                                TempData["RepeatedWorkCode"] += item2.İş_Kodu + " ";
                            }

                        }
                    }

                    if (TempData["RepeatedWorkCode"] != null)
                    {
                        Session["RepeatedWorkCode"] = true;
                        System.IO.File.Delete(pathToExcelFile);
                        return RedirectToAction("UploadDoneWorksExcelInput");
                    }

                    //Adding Drivers' data to database
                    foreach (var a in DoneWorksData)
                    {
                        if (a.İş_Kodu != null)
                        {
                            DoneWorks dw = new DoneWorks();
                            var vehicleId = db.VehiclesBrand.Where(vb => vb.Brand == a.Marka_Model).FirstOrDefault().Id;
                            dw.WorkCode = a.İş_Kodu;
                            dw.WorkName = a.İş_Adı;
                            dw.NormativeFactory = a.Normativ_Zavod;
                            dw.NormativeBakubus = a.Normativ_BakuBus;
                            dw.VehicleBrandId = vehicleId;

                            db.DoneWorks.Add(dw);
                            db.SaveChanges();
                        }
                    }

                    System.IO.File.Delete(pathToExcelFile);
                }
            }
            return RedirectToAction("DoneWorksIndexInput");
        }
        
        //Export to excel
        public void ExportDoneWorksInput()
        {

            ViewHome model = new ViewHome();
            model.DoneWorks = db.DoneWorks.ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Done Works List");

            ws.Range("B2:G2").Merge();
            ws.Cell("B2").Value = "Tamamlanmış işlərin Siyahısı";

            ws.Cell("B3").Value = "S/s";
            ws.Cell("C3").Value = "İş kodu";
            ws.Cell("D3").Value = "İş Adı";
            ws.Cell("E3").Value = "Normativ Zavod ";
            ws.Cell("F3").Value = "Normativ BakuBus ";
            ws.Cell("G3").Value = "Marka & Model";






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



            ws.Row(1).Height = 2.25;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 30;

            ws.Column("A").Width = 0.17;

            ws.Column("B").Width = 7;
            ws.Column("C").Width = 30;
            ws.Column("D").Width = 30;
            ws.Column("E").Width = 30;
            ws.Column("F").Width = 30;
            ws.Column("G").Width = 30;


            int i = 4;
            foreach (var item in model.DoneWorks)
            {
                ws.Cell("B" + i).Value = (i - 3);
                ws.Cell("B" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("B" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("B" + i).Style.Alignment.WrapText = true;

                ws.Cell("C" + i).Value = (item.WorkCode == null ? "" : item.WorkCode);
                ws.Cell("C" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("C" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("C" + i).Style.Alignment.WrapText = true;


                ws.Cell("D" + i).Value = (item.WorkName == null ? "" : item.WorkName);
                ws.Cell("D" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("D" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("D" + i).Style.Alignment.WrapText = true;


                ws.Cell("E" + i).Value = (item.NormativeFactory == null ? null : item.NormativeFactory);
                ws.Cell("E" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("E" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("E" + i).Style.Alignment.WrapText = true;

                ws.Cell("F" + i).Value = (item.NormativeBakubus == null ? null : item.NormativeBakubus);
                ws.Cell("F" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("F" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("F" + i).Style.Alignment.WrapText = true;


                ws.Cell("G" + i).Value = (item.VehiclesBrand.Brand == null ? null : item.VehiclesBrand.Brand);
                ws.Cell("G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Cell("G" + i).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("G" + i).Style.Alignment.WrapText = true;

                i++;
            }

            // Prepare the response
            var httpResponse = Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"DoneWorks.xlsx\"");

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
    }

}
