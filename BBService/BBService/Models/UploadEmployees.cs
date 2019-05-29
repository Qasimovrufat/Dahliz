using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class UploadEmployees
    {
        public int Id { get; set; }
        public string İşçi_kodu { get; set; }
        public string Soyad { get; set; }
        public string Ad { get; set; }
        public string Ata_adı { get; set; }
        public string Doğum_tarixi { get; set; }
        public string İşə_qəbul_tarixi { get; set; }
        public string İşdən_çıxma_tarixi { get; set; }
        public string Telefon { get; set; }
        public string Vəzifə { get; set; }
        public string SV_Kateqoriyası { get; set; }
        public string SV_Seriyası { get; set; }
        public string SV_Nömrəsi { get; set; }
        public string SV_Verilmə_tarixi { get; set; }
        public string SV_Etibarlılıq_tarixi { get; set; }
    }
}