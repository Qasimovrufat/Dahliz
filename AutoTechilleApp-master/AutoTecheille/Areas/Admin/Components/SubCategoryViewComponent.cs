using AutoTecheille.Data;
using AutoTecheille.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Areas.Admin.Components
{
    public class SubCategoryViewComponent:ViewComponent
    {
        private readonly AutoEntity db;
        public SubCategoryViewComponent(AutoEntity _db)
        {
            db = _db;
        }
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            List<SubCategoryLanguage> subCategoryLanguages = await db.SubCategoryLanguages
                                                                         .Include(sb=>sb.SubCategory)
                                                                            .ToListAsync();
            ViewBag.CLagId = id;
            return View(subCategoryLanguages);
        }
    }
}
