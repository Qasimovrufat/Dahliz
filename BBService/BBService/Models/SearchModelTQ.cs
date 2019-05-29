using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchModelTQ
    {
        public int? VehicleId { get; set; }

        public string MainKmMax { get; set; }
        public string MainKmMin { get; set; }
        public string RemainingKmMax { get; set; }
        public string RemainingKmMin { get; set; }
    }
}