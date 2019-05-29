using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models
{
    public class Category
    {
        public Category()
        {
            ProductCategories = new HashSet<ProductCategory>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
