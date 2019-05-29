using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchModelJobcard
    {
        public int? JobCardNo { get; set; }
        public bool? InWaitRoute { get; set; }
        public bool? InWaitDepot { get; set; }
        public bool? InUnderRepair { get; set; }
        public bool? IsOpen { get; set; }
        public int? vehicleId { get; set; }
        public string receiveDateStart { get; set; }
        public string receiveDateEnd { get; set; }

        public string deliveredDateStart { get; set; }
        public string deliveredDateEnd { get; set; }

    }
}