using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models
{
    public class RealPartNo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Product Product { get; set; }
        public int ProductId { get; set; }
    }
}
