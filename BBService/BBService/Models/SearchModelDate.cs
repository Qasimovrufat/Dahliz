using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchModelDate
    {
        public int? IdA { get; set; }
        public int? employeeId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int? routeId { get; set; }
        public int? vehicleId { get; set; }
        public int? vehicleBrandId { get; set; }
        public int? notificationId { get; set; }
    }
}