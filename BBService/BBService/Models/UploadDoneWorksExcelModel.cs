using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class UploadDoneWorksExcelModel
    {
        public string İş_Kodu { get; set; }
        public string İş_Adı { get; set; }
        public int Normativ_Zavod { get; set; }
        public int Normativ_BakuBus { get; set; }
        public string Marka_Model { get; set; }
    }
}