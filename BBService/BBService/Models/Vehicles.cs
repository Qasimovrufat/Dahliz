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
    
    public partial class Vehicles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Vehicles()
        {
            this.InitialControlSchedule = new HashSet<InitialControlSchedule>();
        }
    
        public int Id { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string VehicleCode { get; set; }
        public string Number { get; set; }
        public Nullable<int> ReleaseYear { get; set; }
        public Nullable<byte> Capacity { get; set; }
        public Nullable<byte> NumberOfSeats { get; set; }
        public string RegistrationCertificationSeries { get; set; }
        public string RegistrationCertificationNumber { get; set; }
        public string ChassisNumber { get; set; }
        public string EngineCode { get; set; }
    
        public virtual VehiclesBrand VehiclesBrand { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InitialControlSchedule> InitialControlSchedule { get; set; }
    }
}
