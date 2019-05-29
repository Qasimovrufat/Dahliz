using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class JobcardToDoneWorksModel
    {
        public int Id { get; set; }
        public int? JobcardId { get; set; }
        public int? WorkId { get; set; }
        public int? ServisOfficer { get; set; }
        public int? Master { get; set; }
        public string WorkStartTime { get; set; }
        public string WorkEndTime { get; set; }
    }
}