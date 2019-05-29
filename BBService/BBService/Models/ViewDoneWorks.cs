using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class ViewDoneWorks
    {
        public DoneWorks donework { get; set; }
        public List<VehiclesBrand> vehicleBrands { get; set; }
    }
}