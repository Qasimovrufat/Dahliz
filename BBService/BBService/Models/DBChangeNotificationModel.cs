using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class DBChangeNotificationModel
    {
        public int Id { get; set; }
        public int? NotificationTitleId { get; set; }
        public int? DataId { get; set; }
        public int? UserId { get; set; }
        public bool Status { get; set; }
        public DateTime? AddingDate { get; set; }
    }
}