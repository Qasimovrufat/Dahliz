using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Areas.Admin.Models
{
    public static class MyFile
    {
        public static bool isImage(IFormFile photo)
        {
            if(photo.ContentType == "image/jpg" || photo.ContentType == "image/jpeg" || photo.ContentType == "image/png")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async static Task Upload(IFormFile file,string path)
        {
            using (FileStream stream = new FileStream(path,FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
    }
}
