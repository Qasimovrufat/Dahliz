using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class RequisitionList
    {
        public int Id { get; set; }
        public int? TempWarehouseId { get; set; }
        public int? JobCardId { get; set; }
        public string RequiredQuantity { get; set; }
    }
}