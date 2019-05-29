using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class JobcardDetailsModel
    {
        public int Id { get; set; }
        public int? JobcardNo { get; set; }
        public string Company { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleBrand { get; set; }
        public string VehicleCode { get; set; }
        public string ReleaseYear { get; set; }
        public string ChassisNumber { get; set; }
        public string EngineCode { get; set; }
        public string MaxKm { get; set; }
        public string FailureType { get; set; }
        public string SeniorMech { get; set; }
        public string Mech { get; set; }
        public string ReceivTime { get; set; }
        public string ReceivingController { get; set; }
        public string HandOverTime { get; set; }
        public string HandOverController { get; set; }
        public string TakeOverController { get; set; }
        public List<Requisitions> Requisitions { get; set; }
        public List<JobcardToDoneWorks> JobcardToDoneWorks { get; set; }
    }
}