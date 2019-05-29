using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBService.Models
{
    public class UserUpdate
    {
        public int Id { get; set; }
        public HttpPostedFileBase Image { get; set; }
        public string oldImage { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string newPasswordRepeat { get; set; }
        public string Phone { get; set; }
        public string oldPhone { get; set; }
    }
}