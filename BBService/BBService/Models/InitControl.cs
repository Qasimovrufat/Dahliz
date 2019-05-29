using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class InitControl
    {
        //Leaving data
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int RouteId { get; set; }
        public int FirstDriverId { get; set; }
        public int SecondDriverId { get; set; }

        //Leave data
        public string LeavingKilometer { get; set; }
        public string LeavingFuel { get; set; }
        public string LeavingNote { get; set; }        
        public int LeavingRecMechId { get; set; }
        public int LeavingSeniorRecMechId { get; set; }

        //Enter data
        public int EnterRecMechId { get; set; }
        public int EnterSeniorRecMechId { get; set; }
        public string EnterKilometer { get; set; }
        public string EnterFuel { get; set; }
        public string EnterNote { get; set; }

        //First drv data
        public string FirstDrvKm { get; set; }
        public string FirstDrvFuel { get; set; }

        //Checkup card
        public bool CheckUp { get; set; }
        public string Description { get; set; }


        //Checking enter fuel
        public int CheckFuelId { get; set; }

        //Add socar fule
        public string DateSOCAR { get; set; }
        public string SOCARFuel { get; set; }
        public int InitContId { get; set; }

        //Route
        public List<Routes> Routes { get; set; }
    }
}