using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models.VIewModels
{
    public class ViewCatalogue
    {
        public IEnumerable<ProductLanguage> productLanguages { get; set; }
        public IEnumerable<CategoryLanguage> categories { get; set; }
        public IEnumerable<SubCategoryLanguage> subCategory { get; set; }
    }
}
