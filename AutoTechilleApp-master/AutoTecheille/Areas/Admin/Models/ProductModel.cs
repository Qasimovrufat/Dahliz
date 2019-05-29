using AutoTecheille.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Areas.Admin.Models
{
    public class ProductModel
    {
        public string Description { get; set; }
        public List<string> CategoryNames { get; set; }
        public SubCategory SubCategory { get; set; }
        [DataType(DataType.Upload)]
        public IFormFile Photo { get; set; }
        public List<SubCategoryLanguage> SubCategories { get; set; }
        public List<RealPartNo> RealPartNos { get; set; }
        public string OldPhoto { get; set; }
        public int Id { get; set; }
        public int langId { get; set; }
    }
}
