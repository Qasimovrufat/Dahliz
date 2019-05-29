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
    
    public partial class TempWarehouse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TempWarehouse()
        {
            this.Requisitions = new HashSet<Requisitions>();
            this.WarehouseToMaintenance = new HashSet<WarehouseToMaintenance>();
            this.NotMetRequisitions = new HashSet<NotMetRequisitions>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<int> VehicleId { get; set; }
        public string SparePartCode { get; set; }
        public string InternalCode { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Requisitions> Requisitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WarehouseToMaintenance> WarehouseToMaintenance { get; set; }
        public virtual VehiclesBrand VehiclesBrand { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotMetRequisitions> NotMetRequisitions { get; set; }
    }
}
