using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoTecheille.Data;
using AutoTecheille.Models;
using AutoTecheille.Models.VIewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoTecheille.Controllers
{
    public class CatalogueController : Controller
    {
        private readonly AutoEntity _db;
        public CatalogueController(AutoEntity db)
        {
            _db = db;
        }


        //Filter
        public async Task<IActionResult> Index(string categoryName="All",int? categoryId=0, int? subcategoryId=0)
        {
            ViewCatalogue model = new ViewCatalogue();
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = categoryName;
            ViewBag.SubcategoryId = subcategoryId;
            var languageId = Convert.ToInt32(HttpContext.Session.GetString("languageId"));

            //All categories all subcategories
            if((categoryId == 0 && subcategoryId == 0 )|| (categoryId == -1 && subcategoryId ==0))
            {
                  var productLanguages = await _db.ProductLanguages.Include(p => p.Product)
                                                          .Include(p => p.Product.RealPartNos)
                                                              .Include(p=>p.Product.ProductCategories)
                                                                   .Where(p => p.LanguageId == languageId)
                                                                                    .ToListAsync();
                model.productLanguages = productLanguages;
            }
            //Specific category all subcategory
            else if(categoryId !=0 && categoryId!=-1 && subcategoryId == 0)
            {
                var productLanguages = await _db.ProductLanguages.Include(p => p.Product)
                                                      .Include(p => p.Product.RealPartNos)
                                                           .Include(p => p.Product.ProductCategories)
                                                               .Where(p => p.LanguageId == languageId && p.Product.ProductCategories.Any(c => c.CategoryId == categoryId))
                                                                                .ToListAsync();
                model.productLanguages = productLanguages;
            }
            //Specific Category and Specific Subcategory
            else if (categoryId !=0 && categoryId != -1&& subcategoryId != 0)
            {
                var productLanguages = await _db.ProductLanguages.Include(p => p.Product)
                                                       .Include(p => p.Product.RealPartNos)
                                                          .Include(p => p.Product.ProductCategories)
                                                                .Where(p => p.LanguageId == languageId && p.Product.ProductCategories.Any(c => c.CategoryId == categoryId) && p.Product.SubCategoryId == subcategoryId)
                                                                                 .ToListAsync();
                model.productLanguages = productLanguages;
            }
            //All category and specific subcategory
            else if((categoryId == -1 || categoryId ==0) && subcategoryId !=0 )
            {
                var productLanguages = await _db.ProductLanguages.Include(p => p.Product)
                                                       .Include(p => p.Product.RealPartNos)
                                                            .Include(p => p.Product.ProductCategories)
                                                                .Where(p => p.LanguageId == languageId && p.Product.SubCategoryId == subcategoryId)
                                                                                 .ToListAsync();
                model.productLanguages = productLanguages;
            }
            model.categories = await _db.CategoryLanguages
                                                  .Where(c=>c.LanguageId == languageId)
                                                      .Include(c=>c.Category)
                                                          .ToListAsync();


            model.subCategory = await _db.SubCategoryLanguages
                                              .Where(sb=>sb.LanguageId== languageId)
                                                    .Include(sb=>sb.SubCategory)
                                                       .ToListAsync();
            return View(model);

            }


        [HttpPost]
        public async Task<JsonResult> GetCategories(SearchModel model)
        {
            if (model.LangId != null)
            {
                Language language = await _db.Languages.Where(l => l.Key == model.LangId).FirstOrDefaultAsync();
                if(model.Description != null)
                {
                    var products = await _db.ProductLanguages.Where(pr => pr.LanguageId == language.Id && (pr.Description.ToLower().StartsWith(model.Description) || pr.Description.Contains(model.Description))).ToListAsync();
                    return Json(new { status = 200, data = products });
                }
                else
                {
                    var products = await _db.ProductLanguages.Where(pr => pr.LanguageId == language.Id).ToListAsync();
                    return Json(new { status = 200, data = products });
                }
            }
            else
            {
                return Json(new { status = 400 });
            }

        }


        public async Task<IActionResult> Search(string desc, string lang)
        {
            ViewBag.CategoryName = "All";
           if(desc != null && lang != null)
            {
                SearchViewModel model = new SearchViewModel();
                Language language = await _db.Languages.Where(L => L.Key == lang).FirstOrDefaultAsync();
                var products = await _db.ProductLanguages.Where(pr => pr.LanguageId == language.Id && pr.Description == desc).Include(pr => pr.Product).ThenInclude(p => p.ProductCategories).Include(s => s.Product.SubCategory).Include(r => r.Product.RealPartNos).ToListAsync();
                var categories = await _db.CategoryLanguages.Where(c => c.LanguageId == language.Id).Include(c => c.Category).ToListAsync();
                var subcategories = await _db.SubCategoryLanguages.Where(s => s.LanguageId == language.Id).Include(s => s.SubCategory).ToListAsync();


                model.CategoryLanguages = categories;
                model.SubCategoryLanguages = subcategories;
                model.ProductLanguages = products;
                return View(model);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

    }
}