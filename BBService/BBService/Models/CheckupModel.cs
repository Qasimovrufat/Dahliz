using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class CheckupModel
    {
        public int Id { get; set; }
        public int? InitContId { get; set; }
        public int? VehicleId { get; set; }
        public string Description { get; set; }        
        public string MaintenanceStatus { get; set; }        
        public int? MaintenanceTypeId { get; set; }        
    }
}