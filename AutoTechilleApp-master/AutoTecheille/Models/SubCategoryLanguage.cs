using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models
{
    public class SubCategoryLanguage
    {
        [Column("id")]
        public int Id { get; set; }
        public SubCategory SubCategory { get; set; }
        public int SubCategoryId { get; set; }
        public Language Language { get; set; }
        public int LanguageId { get; set; }
    }
}
