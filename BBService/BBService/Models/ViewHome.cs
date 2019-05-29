using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class ViewHome
    {
        public IEnumerable<Vehicles> Vehicles { get; set; }
        public List<Routes> Routes { get; set; }
        public IEnumerable<Employees> Employees { get; set; }
        public List<Companies> Companies { get; set; }
        public List<Users> Users { get; set; }
        public List<Operations> Operations { get; set; }
        public List<VehiclesBrand> VehiclesBrand { get; set; }
        public List<Actions> Actions { get; set; }
        public List<Permissions> Permissions { get; set; }
        public List<Departments> Departments { get; set; }
        public List<Sectors> Sectors { get; set; }
        public List<Positions> Positions { get; set; }
        public List<Notifications> Notifications { get; set; }
        public List<NotificationTitles> NotificationTitles { get; set; }
        public List<NotificationContent> NotificationContent { get; set; }
        public List<MechanicBinding> MechanicBinding { get; set; }
        public IEnumerable<InitialControlSchedule> InitialControlSchedule { get; set; }
        public List<CheckUpCard> CheckUpCard { get; set; }
        public List<FuelSOCAR> FuelSOCAR { get; set; }
        public List<RouteDailyKM> RouteDailyKM { get; set; }
        public List<Depots> Depots { get; set; }
        public List<TroubleShoot> TroubleShoot { get; set; }
        public IEnumerable<JobCards> JobCards { get; set; }
        public List<Requisitions> Requisitions { get; set; }
        public List<MaintenanceType> MaintenanceType { get; set; }
        public List<MaintenanceHistory> MaintenanceHistory { get; set; }
        public List<Settings> Settings { get; set; }
        public List<WarehouseToMaintenance> WarehouseToMaintenance { get; set; }
        public List<TempWarehouse> TempWarehouse { get; set; }
        public List<VehiclesSortedByMainKm> VehiclesSortedByMainKm { get; set; }
        public List<RequisitionsModel> RequisitionsModel { get; set; }
        public IEnumerable<DoneWorks> DoneWorks { get; set; }
        public List<JobcardToDoneWorks> JobcardToDoneWorks { get; set; }
        public List<NotMetRequisitions> NotMetRequisitions { get; set; }
        
        public SearchModelDate SearchDate { get; set; }
        public SearchModelInitCont SearchModelInitCont { get; set; }
        public UniversalSearch UniversalSearch { get; set; }
        public SearchEmployee SearchEmployee { get; set; }
        public SearchCheckup SearchCheckup { get; set; }
        public SearchModelJobcard SearchModelJobcard { get; set; }
        public SearchMainBinding SearchMainBinding { get; set; }
        public JobcardDetailsModel JobcardDetailsModel { get; set; } 
        public SearchModelTQ SearchModelTQ { get; set; }
        public SearchModelDoneworks SearchModelDoneworks { get; set; }
        

        public InitControl InitControl { get; set; }
        public SocarModel SocarModel { get; set; }
    }
}