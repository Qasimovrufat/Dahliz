using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models
{
    public class AboutLanguage
    {
        public int Id { get; set; }
        [Required]
        public About About { get; set; }
        public int AboutId { get; set; }
        [Required]
        public Language Language { get; set; }
        public int LanguageId { get; set; }
        [Required]
        public string Text { get; set; }
    }
}
