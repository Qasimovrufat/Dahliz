using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class SearchEmployee
    {
        public int? employeeId { get; set; }
        public int? positionId { get; set; }
        public string startRec { get; set; }
        public string endRec { get; set; }
        public string startDeact { get; set; }
        public string endDeact { get; set; }
        public string startDLIssure { get; set; }
        public string endDLIssure { get; set; }
        public string startDLExp { get; set; }
        public string endDLExp { get; set; }
        public bool? status { get; set; }

        public string searchData { get; set; }
    }
}