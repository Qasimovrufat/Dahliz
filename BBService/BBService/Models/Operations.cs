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
    
    public partial class Operations
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Operations()
        {
            this.Actions = new HashSet<Actions>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<bool> Create { get; set; }
        public Nullable<bool> Read { get; set; }
        public Nullable<bool> Update { get; set; }
        public Nullable<bool> Delete { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Actions> Actions { get; set; }
    }
}
