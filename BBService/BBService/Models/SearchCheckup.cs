using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchCheckup
    {
        public int? VehicleId { get; set; }
        public int? JobcardId { get; set; }
        public int? MechanicId { get; set; }
    }
}