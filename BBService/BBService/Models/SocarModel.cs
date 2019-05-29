using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SocarModel
    {
        public int Id { get; set; }
        public int InitContId { get; set; }
        public string DateSOCAR { get; set; }
        public string FuelAmount1 { get; set; }
        public string FuelAmount2 { get; set; }
        public string FuelAmount3 { get; set; }
        public string FuelTime1 { get; set; }
        public string FuelTime2 { get; set; }
        public string FuelTime3 { get; set; }
    }
}