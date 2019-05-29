using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class WarehouseToMaintenanceModel
    {
        public int Id { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public int? WarehouseId { get; set; }
        public string Quantity { get; set; }
        public string NotRequireSPLimit { get; set; }
    }
}