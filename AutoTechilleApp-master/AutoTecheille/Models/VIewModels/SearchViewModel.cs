using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models.VIewModels
{
    public class SearchViewModel
    {
        public List<SubCategoryLanguage> SubCategoryLanguages { get; set; }
        public List<CategoryLanguage> CategoryLanguages { get; set; }
        public List<ProductLanguage> ProductLanguages { get; set; }
    }
}
