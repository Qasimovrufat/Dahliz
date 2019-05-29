using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchMainBinding
    {
        public int? maintenanceTypeId { get; set; }
        public int? warehouseId { get; set; }
        public int? brandId { get; set; }
    }
}