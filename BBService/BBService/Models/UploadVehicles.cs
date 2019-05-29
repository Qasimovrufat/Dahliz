using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class UploadVehicles
    {
        public int Id { get; set; }
        public string Marka { get; set; }
        public string NV_Kodu { get; set; }
        public string DQN { get; set; }
        public int Buraxılış_ili { get; set; }
        public byte Tutum { get; set; }
        public byte Oturacaq { get; set; }
        public string Texposport_Seriyası { get; set; }
        public string Texposport_Nomresi { get; set; }
        public string Şassi_Nomresi { get; set; }
        public string Muherrik_Nomresi { get; set; }
    }
}