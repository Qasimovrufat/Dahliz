using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoTecheille.Models;
using AutoTecheille.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AutoTecheille.Controllers
{
    public class HomeController : Controller
    {
        private readonly AutoEntity _db;
        public HomeController(AutoEntity db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var languageId = HttpContext.GetLanguage("languageId");
            if(languageId == 0)
            {
                return RedirectToAction("SetLanguage","Language",new { culture = "en" , returnUrl = "/"});
            }
            var abouts = await _db.AboutLanguages.Include(a=>a.About)
                                                    .Where(a => a.LanguageId == languageId)
                                                       .ToListAsync();

            return View(abouts);
        }


    }
}
