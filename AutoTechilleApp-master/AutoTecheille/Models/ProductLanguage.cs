using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models
{
    public class ProductLanguage
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public int ProductId { get;set; }
        public Language Language { get; set; }
        public int LanguageId { get; set; }
        public string Description { get; set; }
        
    
    }
}
