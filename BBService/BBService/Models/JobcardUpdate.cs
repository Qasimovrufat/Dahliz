using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class JobcardUpdate
    {
        //Recieve Jobcard
        public int Id { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public  bool IsMaintenance { get; set; }
        public  bool IsAccident { get; set; }
        public  bool IsRepair { get; set; }
        public  bool IsGuarantee { get; set; }
        public  bool MaintenanceDone { get; set; }
        public  bool InWaitRoute { get; set; }        
        public  string status { get; set; }
        public  int? TakeOverServisOfficer { get; set; }
    }
}