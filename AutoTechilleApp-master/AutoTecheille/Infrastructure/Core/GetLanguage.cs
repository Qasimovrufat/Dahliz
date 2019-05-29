using AutoTecheille.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models.MyClass
{
    public static class GetLanguage
    {
        public static string GetLanguageId(string culture,AutoEntity db)
        {
            var language = db.Languages.Where(l => l.Key == culture).FirstOrDefault();
            return language.Value;
        }
    }
}
