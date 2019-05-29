using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using BBService.Filters;
using BBService.Models;

namespace BBService.Controllers
{
    [LogOut]
    public class ProfileController : Controller
    {
        // GET: Profile
        BBServiceEntities db = new BBServiceEntities();
        
        public ActionResult PrfIndex()
        {
            ViewBag.Home = true;
            if (Session["UserId"] != null)
            {
                int UserId = (int)Session["UserId"];
                ViewBag.User = db.Users.Find(UserId);
            }

            return View();
        }

        [HttpPost]
        public ActionResult PrfUpdate(UserUpdate userData)
        {
            if (ModelState.IsValid)
            {
                if (userData.Image != null)
                {
                    if (userData.Image.FileName.Length > 28 || (userData.Image.ContentLength / 1024) > 2048)
                    {
                        return RedirectToAction("PrfIndex");
                    }
                }


                Users usr = db.Users.Find(userData.Id);
                if (usr != null)
                {
                    //Update Image
                    if (userData.Image != null)
                    {
                        string imageName = DateTime.Now.ToString("ddMMyyyyHHmmssffff") + userData.Image.FileName;
                        string imagePath = Path.Combine(Server.MapPath("~/Uploads"), imageName);
                        string oldImagePath = Path.Combine(Server.MapPath("~/Uploads"), userData.oldImage);

                        System.IO.File.Delete(oldImagePath);
                        userData.Image.SaveAs(imagePath);
                        usr.Image = imageName;

                    }
                    else
                    {
                        usr.Image = userData.oldImage;
                    }

                    //Update password
                    string Pass = db.Users.Find(userData.Id).Password;
                    if (userData.oldPassword != null && userData.newPassword != null && userData.newPasswordRepeat != null)
                    {
                        if (Crypto.VerifyHashedPassword(Pass, userData.oldPassword) == true && userData.newPassword == userData.newPasswordRepeat)
                        {
                            string newPass = Crypto.HashPassword(userData.newPassword);
                            usr.Password = newPass;
                        }
                        else
                        {
                            Session["NewPasswordInValid"] = true;
                            usr.Password = Pass;
                        }
                    }

                    //Update phone
                    if (userData.Phone != null)
                    {
                        usr.Phone = userData.Phone;
                    }
                    else
                    {
                        usr.Phone = userData.oldPhone;
                    }

                    db.Entry(usr).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("PrfIndex");
                }
                else
                {
                    return RedirectToAction("PrfIndex");
                }
            }

            return RedirectToAction("PrfIndex");
        }
    }
}