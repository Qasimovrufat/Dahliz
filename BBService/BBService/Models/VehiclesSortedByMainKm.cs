using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class VehiclesSortedByMainKm
    {
        public int Id { get; set; }
        public int? BrandId { get; set; }
        public int? VehicleId { get; set; }
        public string VehicleCode { get; set; }
        public string Number { get; set; }
        public int? ReleaseYear { get; set; }
        public int? Capacity { get; set; }
        public int? NumberOfSeats { get; set; }
        public string RegistrationCertificationSeries { get; set; }
        public string RegistrationCertificationNumber { get; set; }
        public string ChassisNumber { get; set; }
        public string EngineCode { get; set; }
        public decimal? KmToMaint { get; set; }        
        public decimal? RemainingKmToMaint { get; set; }

        public SearchModelTQ SearchModelTQ { get; set; }
    }
}