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
    public class CategoryViewComponent:ViewComponent
    {
        private readonly AutoEntity db;
        public CategoryViewComponent(AutoEntity _db)
        {
            db = _db;
        }
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            List<CategoryLanguage> categoryLanguages = await db.CategoryLanguages.Include(c => c.Category)
                                                    .ToListAsync();
            ViewBag.CLagId = id;
            return View(categoryLanguages);
        }
    }
}
