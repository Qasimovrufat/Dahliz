//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BBService.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class JobcardToDoneWorks
    {
        public int Id { get; set; }
        public Nullable<int> JobcardId { get; set; }
        public Nullable<int> WorkId { get; set; }
        public Nullable<int> ServisOfficer { get; set; }
        public Nullable<int> Master { get; set; }
        public Nullable<System.DateTime> WorkStartTime { get; set; }
        public Nullable<System.DateTime> WorkEndTime { get; set; }
        public Nullable<bool> IsOpen { get; set; }
    
        public virtual DoneWorks DoneWorks { get; set; }
        public virtual Employees Employees { get; set; }
        public virtual Employees Employees1 { get; set; }
        public virtual JobCards JobCards { get; set; }
    }
}
