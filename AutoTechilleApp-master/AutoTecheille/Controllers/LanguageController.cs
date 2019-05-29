using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoTecheille.Data;
using AutoTecheille.Models.MyClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTecheille.Controllers
{
    public class LanguageController : Controller
    {
        private readonly AutoEntity _db;
        public LanguageController(AutoEntity db)
        {
            _db = db;
        }

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            var languageId = Convert.ToInt32(GetLanguage.GetLanguageId(culture, _db));
            HttpContext.Session.SetString("languageId", languageId.ToString());
            return LocalRedirect(returnUrl);
        }

    }
}