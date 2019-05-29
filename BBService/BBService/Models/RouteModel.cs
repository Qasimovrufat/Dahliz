using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class RouteModel
    {
        public int Id { get; set; }
        public string ActivationDate { get; set; }
        public string DeactivationDate { get; set; }        
        public string Name { get; set; }
        public string Forward { get; set; }
        public string ForwardLength { get; set; }
        public string Backward { get; set; }
        public string BackwardLength { get; set; }
        public string Number { get; set; }
    }
}