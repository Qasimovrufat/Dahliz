using BBService.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Models;
using System.Data.Entity;

namespace BBService.Controllers
{
    [LogOut]
    public class ManageOperationsController : Controller
    {
        BBServiceEntities db = new BBServiceEntities();
        // GET: ManageOperations

        //Operations Manage
        [OperationFilter]
        public ActionResult MngOprIndex()
        {
            ViewBag.Administrative = true;
            ViewHome model = new ViewHome
            {
                Operations = db.Operations.ToList(),
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult MngOprCreate()
        {
            ViewBag.Administrative = true;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult MngOprCreate(Operations opr)
        {
            if (opr == null)
            {
                return RedirectToAction("MngOprIndex");
            }
            db.Operations.Add(opr);
            db.SaveChanges();
            return RedirectToAction("MngOprIndex");
        }

        [OperationFilter]
        public ActionResult MngOprUpdate(int id)
        {
            ViewBag.Administrative = true;
            Operations opr = db.Operations.Find(id);
            ViewBag.Operation = opr;
            return View();
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult MngOprUpdate(Operations opr)
        {
            if (ModelState.IsValid)
            {
                db.Entry(opr).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MngOprIndex");
            }
            return RedirectToAction("MngOprIndex");
        }

        [OperationFilter]
        public ActionResult MngOprDelete(int id)
        {
            Operations opr = db.Operations.Find(id);
            Actions act = db.Actions.FirstOrDefault(a => a.OperationId == id);

            if (act!=null)
            {
                return RedirectToAction("MngOprIndex");
            }

            db.Operations.Remove(opr);
            db.SaveChanges();
            return RedirectToAction("MngOprIndex");
        }




        //Actions Manage
        [OperationFilter]
        public ActionResult ActOprMngIndex(string searchData)
        {
            ViewBag.Administrative = true;
            ViewBag.SearchData = searchData;
            ViewHome model = new ViewHome();
            if (searchData != null)
            {
                model.Actions = db.Actions.Where(d => d.Name.ToString().Contains(searchData) == true).ToList();
            }
            else
            {
                model.Actions = db.Actions.ToList();
            }

            return View(model);
        }

        [OperationFilter]
        public ActionResult ActOprMngCreate()
        {
            ViewBag.Administrative = true;

            ViewHome model = new ViewHome()
            {
                Operations = db.Operations.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult ActOprMngCreate(Actions act)
        {
            if (act == null)
            {
                return RedirectToAction("ActOprMngIndex");
            }
            db.Actions.Add(act);
            db.SaveChanges();
            return RedirectToAction("ActOprMngIndex");
        }

        [OperationFilter]
        public ActionResult ActOprMngUpdate(int id)
        {
            ViewBag.Administrative = true;

            ViewHome model = new ViewHome()
            {
                Operations = db.Operations.ToList()
            };
            Actions act = db.Actions.Find(id);
            ViewBag.Act = act;
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult ActOprMngUpdate(Actions act)
        {
            if (act == null)
            {
                return RedirectToAction("ActOprMngIndex");
            }

            if (ModelState.IsValid)
            {
                Actions Act = db.Actions.Find(act.Id);
                Act.Name = act.Name;
                if (act.OperationId == 0)
                {
                    Act.OperationId = null;
                }
                else
                {
                    Act.OperationId = act.OperationId;
                }

                db.Entry(Act).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ActOprMngIndex");
            }
            return RedirectToAction("ActOprMngIndex");
        }

        [OperationFilter]
        public ActionResult ActOprMngDelete(int id)
        {
            Actions act = db.Actions.Find(id);
            Permissions per = db.Permissions.FirstOrDefault(p => p.ActionId == id);

            if (per!=null)
            {
                return RedirectToAction("ActOprMngIndex");
            }

            db.Actions.Remove(act);
            db.SaveChanges();
            return RedirectToAction("ActOprMngIndex");
        }



        //Action üzrə istifadəçi axtar
        [OperationFilter]
        public ActionResult searchUserIndex(UniversalSearch universalSearch)
        {
            ViewBag.Administrative = true;
            ViewHome model = new ViewHome();
            model.Actions = db.Actions.ToList();

            if (universalSearch.actionId != null && universalSearch.actionId != 0)
            {
                model.Users = db.Users.Where(u => u.Permissions.FirstOrDefault(p => p.ActionId == universalSearch.actionId).ActionId == universalSearch.actionId).ToList();
            }

            model.UniversalSearch = universalSearch;

            return View(model);
        }
    }
}