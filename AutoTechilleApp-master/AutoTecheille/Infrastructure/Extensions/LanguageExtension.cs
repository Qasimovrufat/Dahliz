using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille
{
    public static class LanguageExtension
    {
        public static int GetLanguage(this HttpContext context,string session_name)
        {
            var languageId = Convert.ToInt32(context.Session.GetString(session_name));
            if (languageId == 0)
            {
                languageId = 1;
                context.Session.SetString(session_name, languageId.ToString());
            }

            return languageId;
        }
    }
}
