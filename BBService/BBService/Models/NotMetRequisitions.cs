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
    
    public partial class NotMetRequisitions
    {
        public int Id { get; set; }
        public Nullable<int> SPId { get; set; }
        public Nullable<decimal> RemainingQuantity { get; set; }
        public Nullable<int> OrijinalJCId { get; set; }
        public Nullable<int> SubstituteJCId { get; set; }
        public Nullable<bool> IsOpen { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ClosedDate { get; set; }
    
        public virtual JobCards JobCards { get; set; }
        public virtual JobCards JobCards1 { get; set; }
        public virtual TempWarehouse TempWarehouse { get; set; }
    }
}
