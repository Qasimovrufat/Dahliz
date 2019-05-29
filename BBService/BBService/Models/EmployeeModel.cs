using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class EmployeeModel
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public string RecruitmentDate { get; set; }
        public string BirthDate { get; set; }
        public string LicenceIssueDate { get; set; }
        public string LicenceExpireDate { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string FutherName { get; set; }
        public int? PositionId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SectorId { get; set; }
        public string Phone { get; set; }
        public string LicenceCategory { get; set; }
        public string LicenceSeries { get; set; }
        public string LicenceNumber { get; set; }
        public string DeactivationDate { get; set; }
        public bool Status { get; set; }
    }
}