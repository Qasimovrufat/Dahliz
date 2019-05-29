using AutoTecheille.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Areas.Admin.Models
{
    public class ProductViewModel
    {
        public List<Language> languages { get; set; }

        public string Description { get; set; }
        public string LanguageId { get; set; }
        public List<string> Categories { get; set; }
        public string SubCategory { get; set; }
        public List<string> RealPartNos { get; set; }
        public IFormFile Photo { get; set; }
    }
}
