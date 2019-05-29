using BBService.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BBService.Models;
using System.IO;
using System.Web.Helpers;
using System.Data.Entity;

namespace BBService.Controllers
{
    [LogOut]
    public class UserOperationsController : Controller
    {
        BBServiceEntities db = new BBServiceEntities();
        // GET: UserOperations


        //User monipulations
        [OperationFilter]
        public ActionResult UsrOprIndex()
        {
            ViewBag.Administrative = true;

            ViewHome model = new ViewHome
            {
                Users = db.Users.ToList(),
            };

            return View(model);
        }

        [OperationFilter]
        public ActionResult UsrOprCreate()
        {
            ViewBag.Administrative = true;
            ViewHome model = new ViewHome()
            {
                Employees = db.Employees.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult UsrOprCreate(Users usr, string Password, HttpPostedFileBase Image)
        {
            if (ModelState.IsValid)
            {
                if (usr == null)
                {
                    return Content("Oppan");
                }
                if (Image != null && Image.ContentType != "image/jpeg" && Image.ContentType != "image/png" && Image.ContentType != "image/gif")
                {
                    Session["UploadError"] = "Image format is not correct";
                    return RedirectToAction("UsrOprCreate", "userOperations");
                }
                if (Image != null && ((Image.ContentLength / 1024) > 5120))
                {
                    Session["UploadError"] = "Your file size is over 5Mb";
                    return RedirectToAction("UsrOprCreate", "userOperations");
                }

                //Image
                if (Image != null)
                {
                    string fileName = DateTime.Now.ToString("ddMMyyyyHHmmssffff") + Image.FileName;
                    string path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                    Image.SaveAs(path);
                    usr.Image = fileName;
                }


                //Password
                if (Password != null && Password != String.Empty)
                {
                    string pass = Crypto.HashPassword(Password);
                    usr.Password = pass;
                }

                db.Users.Add(usr);
                db.SaveChanges();
                return RedirectToAction("UsrOprIndex");
            }

            return View();
        }

        [OperationFilter]
        public ActionResult UsrOprUpdate(int id)
        {
            ViewBag.Administrative = true;

            Users usr = db.Users.Find(id);
            if (usr == null)
            {
                return RedirectToAction("UsrOprIndex");
            }
            ViewHome model = new ViewHome()
            {
                Employees=db.Employees.ToList()
            };
            ViewBag.User = usr;

            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult UsrOprUpdate(Users usr, string Password, HttpPostedFileBase Image, string OldImage)
        {
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    if (Image.ContentType != "image/png" && Image.ContentType != "image/jpeg" && Image.ContentType != "image/gif")
                    {
                        Session["uploadError"] = "Your file must be jpg,png or gif";
                        return RedirectToAction("UpdatingBlogImage", "blog", new { id = usr.Id });
                    }
                    if ((Image.ContentLength / 1024) > 5120)
                    {
                        Session["uploadError"] = "Your file size must be max 5mb";
                        return RedirectToAction("UpdatingBlogImage", "blog", new { id = usr.Id });
                    }

                    string filename = DateTime.Now.ToString("ddMMyyyyHHmmssffff") + Image.FileName;
                    string path = Path.Combine(Server.MapPath("~/Uploads"), filename);

                    string oldpath = Path.Combine(Server.MapPath("~/Uploads"), OldImage);

                    if (oldpath != null)
                    {
                        System.IO.File.Delete(oldpath);
                    }
                    Image.SaveAs(path);
                    usr.Image = filename;
                }
                else
                {
                    usr.Image = OldImage;
                }

                //Password
                if (Password != null && Password != String.Empty)
                {
                    string pass = Crypto.HashPassword(Password);
                    usr.Password = pass;
                }
                if (usr.EmployeeId==0)
                {
                    usr.EmployeeId = null;
                }

                db.Entry(usr).State = EntityState.Modified;
                if (Password == null || Password == String.Empty)
                {
                    db.Entry(usr).Property(x => x.Password).IsModified = false;
                }

                db.SaveChanges();
                return RedirectToAction("UsrOprIndex");
            }
            return View();
        }

        [OperationFilter]
        public ActionResult UsrOprDelete(int id)
        {
            Users usr = db.Users.Find(id);
            Permissions userPer = db.Permissions.FirstOrDefault(l => l.UserId == id);
            List<Permissions> userPerList = db.Permissions.ToList();
                       
            if (userPer != null)
            {
                foreach (var item in userPerList)
                {
                    if (item.UserId == id)
                    {
                        db.Permissions.Remove(item);
                        db.SaveChanges();
                    }
                }
            }
            db.Users.Remove(usr);
            db.SaveChanges();
            return RedirectToAction("UsrOprIndex");
        }



        //Access monipulations
        [OperationFilter]
        public ActionResult AccIndex(int? id)
        {
            ViewBag.Administrative = true;
            ViewBag.UserId = id;
            ViewBag.User = db.Users.Find(id);

            ViewHome model = new ViewHome()
            {
                Permissions = db.Permissions.Where(p => p.UserId == id).ToList()
            };
            return View(model);
        }

        [OperationFilter]
        public ActionResult AccCreate(int? id)
        {
            ViewBag.Administrative = true;
            ViewBag.UserId = id;
            ViewHome model = new ViewHome()
            {
                Operations = db.Operations.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [OperationFilter]
        public ActionResult AccCreate(int UserId, int[] ActionId)
        {
            ViewBag.Administrative = true;
            if (ActionId == null)
            {
                return RedirectToAction("AccCreate", new { id = UserId });
            }

            List<Permissions> perListAll = db.Permissions.Where(u => u.UserId == UserId).ToList();
            foreach (var item in perListAll)
            {
                foreach (var item2 in ActionId)
                {
                    if (item.ActionId == item2)
                    {
                        TempData["userActions"] += item.Actions.Name + " ";
                    }
                }
            }

            if (TempData["userActions"] != null)
            {
                Session["userActions"] = true;
                return RedirectToAction("AccCreate", new { id = UserId });
            }

            foreach (var item in ActionId)
            {
                Permissions perList = new Permissions();
                perList.UserId = UserId;
                perList.ActionId = item;
                db.Permissions.Add(perList);
                db.SaveChanges();
            }
            return RedirectToAction("AccIndex", new { id = UserId });
        }

        [OperationFilter]
        public JsonResult GetActions(int id)
        {
            var actions = db.Actions.Where(a => a.OperationId == id).Select(a => new {
                a.Id,
                a.Name
            }).ToList();
            return Json(actions, JsonRequestBehavior.AllowGet);
        }

        [OperationFilter]
        public ActionResult AccDelete(int? id, int? UserId)
        {
            Permissions permission = db.Permissions.Find(id);
            if (permission == null)
            {
                return RedirectToAction("AccIndex", new { id = UserId });
            }

            db.Permissions.Remove(permission);
            db.SaveChanges();

            return RedirectToAction("AccIndex", new { id = UserId });
        }
    }
}