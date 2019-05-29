using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BBService.Models;

namespace BBService.MyClasses
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        BBServiceEntities db = new BBServiceEntities();
        public string GetUserId(IRequest request)
        {
            // your logic to fetch a user identifier goes here.

            //// for example:
            //request.Cookies["test"].Value;

            //var userId = db.Users.Find(request.User.Identity.Name);
            return request.Cookies["userName"].Value; ;
        }
    }
}