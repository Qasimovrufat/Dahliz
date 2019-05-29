using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class RequisitionsModel
    {
        public int Id { get; set; }
        public int? JobCardId { get; set; }
        public int? TempWarehouseId { get; set; }
        public decimal? RequiredQuantity { get; set; }
        public decimal? MeetingQuantity { get; set; }
        public DateTime? AddedDate { get; set; }
        public bool? IsOpen { get; set; }

        public string sparePartName { get; set; }
        public decimal? warehouse { get; set; }
        public decimal? requiredBefore { get; set; }
    }
}