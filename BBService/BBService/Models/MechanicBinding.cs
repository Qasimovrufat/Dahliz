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
    
    public partial class MechanicBinding
    {
        public int Id { get; set; }
        public Nullable<int> SeniorRecMechId { get; set; }
        public Nullable<int> RecMechId { get; set; }
        public Nullable<System.DateTime> AddedDate { get; set; }
        public Nullable<bool> Status { get; set; }
    
        public virtual Employees Employees { get; set; }
        public virtual Employees Employees1 { get; set; }
    }
}
