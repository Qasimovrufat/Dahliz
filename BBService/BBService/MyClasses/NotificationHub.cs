using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BBService.Models;
using Microsoft.AspNet.SignalR;

namespace BBService.MyClasses
{
    public class NotificationHub : Hub
    {
        /**/

        //Chat task
        public static Dictionary<string, string> LstAllConnections = new Dictionary<string, string>();
        public static Dictionary<string, string> LstAllConnectedUsers = new Dictionary<string, string>();
        NotificationComponents NC = new NotificationComponents();
        
        
       public override Task OnConnected()
        {
            LstAllConnections.Add(Context.ConnectionId, "");
            Clients.All.BroadcastConnections(LstAllConnections);

            string userConnectionId = Context.ConnectionId;
            string userId = Context.QueryString["myUserId"].ToString();
            if (!LstAllConnectedUsers.Keys.Contains(userId))
            {
                LstAllConnectedUsers.Add(userId, userConnectionId);
            }

            return base.OnConnected();
       }


       public override Task OnDisconnected(bool stopCalled)
       {
           LstAllConnections.Remove(Context.ConnectionId);
           Clients.All.BroadcastConnections(LstAllConnections);

           string userConnectionId = Context.ConnectionId;
           string userId = Context.QueryString["myUserId"].ToString();
           if (LstAllConnectedUsers.Keys.Contains(userId))
           {
               LstAllConnectedUsers.Remove(userId);
           }

           return base.OnDisconnected(stopCalled);
       }

       public Dictionary<string, string> GetLstAllConnectedUsers()
       {
           return LstAllConnectedUsers;
       }
       

        /**/
    }
}