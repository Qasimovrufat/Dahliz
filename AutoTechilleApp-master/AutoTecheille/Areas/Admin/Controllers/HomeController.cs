using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoTecheille.Areas.Admin.Models;
using AutoTecheille.Data;
using AutoTecheille.Models;
using AutoTecheille.Models.VIewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoTecheille.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AutoEntity db;

        public HomeController(AutoEntity _db)
        {
            db = _db;
        }
        public async Task<IActionResult> Index()
        {
            //Get Lang Id From Extension
            int languageId = HttpContext.GetLanguage("adminLanguageId");

            var languages = await db.Languages.ToListAsync();
            var products = await db.ProductLanguages.Where(p => p.LanguageId == languageId)
                                                      .Include(p => p.Product.RealPartNos)
                                                        .Include(p => p.Product.ProductCategories)
                                                          .Include(p => p.Product.SubCategory)
                                                            .ToListAsync();
            var productCategories = await db.ProductCategories.Include(c => c.Category).Include(c => c.Category).ToListAsync();
            ListModel model = new ListModel();
            model.languages = languages;
            model.productLanguages = products;
            model.productCategories = productCategories;
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            ProductViewModel pm = new ProductViewModel();
            pm.languages = await db.Languages.ToListAsync();
            return View(pm);
        }

        [HttpPost]
        public async Task<JsonResult> Add(ProductViewModel model)
        {
            //Check All Models
            foreach (string item in model.Categories)
            {
                if (item == null)
                {
                    return Json(new { status = 400, errorMessage = "En azi 1 category elave edin" });
                }
            }


            if (model.Description == "")
            {
                return Json(new { status = 400, errorMessage = "Description elave edin" });
            }

            if (model.SubCategory == "")
            {
                return Json(new { status = 400, errorMessage = "En azi 1 Subcategory elave edin" });
            }

            if (model.Photo == null)
            {
                return Json(new { status = 400, errorMessage = "Sekil elave edin" });
            }

            foreach (string item in model.RealPartNos)
            {
                if (item == null)
                {
                    return Json(new { status = 400, errorMessage = "En azi 1 realpartno elave edin" });
                }
            }

            if (model.LanguageId == "")
            {
                return Json(new { status = 400, errorMessage = "Dil elave edile bilmir" });
            }




            //Find Language
            Language language = db.Languages.Where(l => l.Key == model.LanguageId).FirstOrDefault();
            var sub = model.SubCategory;
            var description = model.Description;

            //Make Category and RealPartNo Arrays
            List<string> categories = MakeArray(model.Categories);
            List<string> realPartNos = MakeArray(model.RealPartNos);


            //Upload Photo
            var photo = model.Photo;
            var file_name = photo.FileName;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Products", file_name);
                if (MyFile.isImage(photo))
                {
                    await MyFile.Upload(photo, path);
                }

            //Create Product
            Product product = new Product();
            product.SubCategory = await db.SubCategories.Where(s => s.Name == sub).FirstOrDefaultAsync();
            product.PhotoPath = photo.FileName;

            //Creat RealPartNo
            foreach (var realPartNo in realPartNos)
            {
                    RealPartNo number = new RealPartNo();
                    number.Name = realPartNo;
                    number.Product = product;
                    await db.RealPartNos.AddAsync(number);
            }


            //Create Category
            foreach (var item in categories)
            {
                if (item != null)
                {
                    Category category = await db.Categories.Where(c => c.Name == item).FirstOrDefaultAsync();
                    ProductCategory productCategory = new ProductCategory();
                    productCategory.Category = category;
                    productCategory.Product = product;
                    db.ProductCategories.Add(productCategory);
                }
            }


            //Create MultiLanguage Product
            ProductLanguage productLanguage = new ProductLanguage();
            productLanguage.LanguageId = language.Id;
            productLanguage.Product = product;
            productLanguage.Description = model.Description;
            await db.ProductLanguages.AddAsync(productLanguage);
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();

            return Json(new { status = 200, message = "Mehsul Elave Edildi" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await db.ProductLanguages.Where(p => p.ProductId == id)
                                                             .Include(p => p.Product)
                                                             .Include(p => p.Product.ProductCategories)
                                                             .Include(p => p.Product.SubCategory)
                                                             .Include(p => p.Product.RealPartNos)
                                                             .FirstOrDefaultAsync();

            ProductModel pm = new ProductModel();
            pm.Id = product.ProductId;
            ViewBag.langId = product.LanguageId;
            pm.Description = product.Description;
            pm.SubCategory = product.Product.SubCategory;
            pm.SubCategories = await db.SubCategoryLanguages.Where(sb => sb.LanguageId == product.LanguageId).Include(s => s.SubCategory).ToListAsync();
            pm.CategoryNames = await db.CategoryLanguages.Where(c => c.LanguageId == product.LanguageId).Include(c => c.Category).Select(c => c.Category.Name).ToListAsync();
            pm.OldPhoto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Products", product.Product.PhotoPath);
            return View(pm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductModel model, int id, int langId)
        {
            bool change = false;
            Product product = await db.Products.Where(pr => pr.Id == id).FirstOrDefaultAsync();
            ProductLanguage productLanguage = await db.ProductLanguages.Where(pr => pr.ProductId == product.Id && pr.LanguageId == langId).FirstOrDefaultAsync();
            List<ProductCategory> productCategories = await db.ProductCategories.Where(p => p.ProductId == product.Id).ToListAsync();

            //Update Description
            if (model.Description != null)
            {
                change = true;
                productLanguage.Description = model.Description;
            }


            //Update Categories
            if (model.CategoryNames.Count() != 0)
            {
                change = true;
                //Delete Product Categories
                db.ProductCategories.RemoveRange(productCategories);
                foreach (var item in model.CategoryNames)
                {
                    if (item != null)
                    {
                        var category = await db.Categories.Where(c => c.Name == item).FirstOrDefaultAsync();
                        ProductCategory pr = new ProductCategory();
                        pr.Category = category;
                        pr.Product = product;
                        await db.ProductCategories.AddAsync(pr);
                    }
                }
            }

            //Update Subcategory
            if (model.SubCategory != null)
            {
                change = true;
                product.SubCategory = model.SubCategory;
            }


            //Update Photo
            if (Request.Form.Files.Count > 0)
            {
                change = true;
                var file = Request.Form.Files[0];
                if (MyFile.isImage(file))
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Products", file.FileName);
                    if (!System.IO.File.Exists(path))
                    {
                        await MyFile.Upload(file, path);
                    }
                    product.PhotoPath = file.FileName;
                }

            }

            if (change)
            {
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> EditRealPartNo(EditModel model)
        {
            var product = await db.ProductLanguages.Where(p => p.ProductId == model.id && p.LanguageId == model.langId).Include(p => p.Product.RealPartNos).FirstOrDefaultAsync();

            db.RealPartNos.RemoveRange(product.Product.RealPartNos);
            var newArray = MakeArray(model.array);
            foreach (var item in newArray)
            {
                RealPartNo partNo = new RealPartNo();
                partNo.Name = item;
                partNo.ProductId = product.ProductId;
                db.RealPartNos.Add(partNo);
                db.SaveChanges();
            }
            return Json(new { status = 200 });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            if (id != 0)
            {
                //Find Product
                Product product = await db.Products.Where(p => p.Id == id).FirstOrDefaultAsync();
                //Find ProductCategory
                ProductCategory pc = await db.ProductCategories.Where(p => p.Product == product).FirstOrDefaultAsync();
                //Find ProductLanguage
                ProductLanguage pl = await db.ProductLanguages.Where(p => p.Product == product).FirstOrDefaultAsync();

                //Delete Product and All Relations
                db.Products.Remove(product);
                db.ProductLanguages.Remove(pl);
                db.ProductCategories.Remove(pc);
                await db.SaveChangesAsync();

                return Json(new { status = 200 });
            }
            return Json(new { status = 400 });
        }


        public IActionResult SetLanguage(string id)
        {
            string languageId = id;
            HttpContext.Session.SetString("adminLanguageId", languageId);
            return RedirectToAction("Index", "Home");
        }



        public List<string> MakeArray(List<string> element)
        {
            List<string> array = new List<string>();
            var counter = 0;
            for (var i = 0; i < element.Count; i++)
            {
                var newElement = String.Empty;
                foreach (var item in element[i])
                {
                    if (item != ',')
                    {
                        newElement += item;
                    }
                    else
                    {
                        array.Add(newElement);
                        newElement = "";
                    }
                    counter++;
                    if (counter == element[i].Length)
                    {
                        array.Add(newElement);
                    }
                }
            }
            return array;
        }
    }
}