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
    public class _LayoutViewComponent:ViewComponent
    {
        private readonly AutoEntity db;
        public _LayoutViewComponent(AutoEntity _db)
        {
            db = _db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Language> languages =await db.Languages.ToListAsync();
            return View(languages);
        }
    }
}
