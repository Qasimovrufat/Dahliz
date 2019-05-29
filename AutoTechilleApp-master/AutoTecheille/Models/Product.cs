using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models
{
    public class Product
    {

        public Product()
        {
            ProductCategories = new HashSet<ProductCategory>();
        }
        public int Id { get; set; }
        [Required]
        public SubCategory SubCategory { get; set; }
        public int SubCategoryId { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }
        public ICollection<RealPartNo> RealPartNos { get; set; }
        public string PhotoPath { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }
        
    }
}
