using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchModelInitCont
    {
        public int? VehicleId { get; set; }
        public int? RouteId { get; set; }

        public int? FirstDriverId { get; set; }
        public int? SecondDriverId { get; set; }

        public int? LeavingRecMechId { get; set; }
        public int? LeavingSeniorRecMechId { get; set; }
        public int? EnterRecMechId { get; set; }
        public int? EnterSeniorRecMechId { get; set; }

        public string LeaveDateStart { get; set; }
        public string LeaveDateEnd { get; set; }
        public string EnterDateStart { get; set; }
        public string EnterDateEnd { get; set; }

        public string LeaveHourStart { get; set; }
        public string LeaveHourEnd { get; set; }
        public string EnterHourStart { get; set; }
        public string EnterHourEnd { get; set; }

        public string LeaveKmMax { get; set; }
        public string LeaveKmMin { get; set; }
        public string EnterKmMax { get; set; }
        public string EnterKmMin { get; set; }

        public string LeaveFuelMax { get; set; }
        public string LeaveFuelMin { get; set; }
        public string EnterFuelMax { get; set; }
        public string EnterFuelMin { get; set; }

        public string FinalKmMax { get; set; }
        public string FinalKmMin { get; set; }

        public string FinalFuelMax { get; set; }
        public string FinalFuelMin { get; set; }

        public string FinalHourStart { get; set; }
        public string FinalHourEnd { get; set; }

        public string SOCARFuelMin { get; set; }
        public string SOCARFuelMax { get; set; }
    }
}