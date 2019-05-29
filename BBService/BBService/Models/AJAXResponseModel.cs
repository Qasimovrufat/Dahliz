using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class AJAXResponseModel
    {
        public string status { get; set; }

        //Enter proses
        public string currentKm { get; set; }
        public string kmLimit { get; set; }

        //Reqisitions
        public string Name { get; set; }
        public string PartCode { get; set; }
        public int wId { get; set; }

        //Comparing requsitions with warehouse
        public bool statusCompare { get; set; }
        public decimal? balance { get; set; }
        public decimal? inreq { get; set; }

        //Enter fuel checking
        public string leaveFuel { get; set; }

        //Check for maintenance
        public string maintenanceStatus { get; set; }        
        public string maintenanceData { get; set; }

        //Spare parts
        public int sparePartId { get; set; }
        public string sparePartCode { get; set; }
        public string sparePartName { get; set; }

        //Submit closed jobcards by Takeover Officer        
        public string statusChecked { get; set; }
        public string statusUnchecked { get; set; }


    }
}