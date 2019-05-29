using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchModelDoneworks
    {
        public int? Id { get; set; }
        public string WorkCode { get; set; }
        public string WorkName { get; set; }
        public Nullable<int> NormativeFactory { get; set; }
        public Nullable<int> NormativeBakubus { get; set; }
        public Nullable<int> VehicleBrandId { get; set; }
    }
}