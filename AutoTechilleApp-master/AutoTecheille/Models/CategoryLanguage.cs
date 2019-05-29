using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models
{
    public class CategoryLanguage
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
        public Language Language { get; set; }
        public int LanguageId { get; set; }
    }
}
