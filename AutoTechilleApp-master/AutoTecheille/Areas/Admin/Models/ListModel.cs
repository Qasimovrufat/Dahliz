using AutoTecheille.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Areas.Admin.Models
{
    public class ListModel
    {
        public List<ProductLanguage> productLanguages { get; set; }
        public List<ProductCategory> productCategories { get; set; }
        public List<Language> languages { get; set; }
    }
}
