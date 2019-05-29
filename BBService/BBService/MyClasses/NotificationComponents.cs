using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BBService.Models;
using TableDependency.SqlClient.Base.EventArgs;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient;

namespace BBService.MyClasses
{
    public class NotificationComponents
    {

        List<DBChangeNotificationModel> dBChangeNotificationModelList = new List<DBChangeNotificationModel>();
        public static Dictionary<string, string> LstAllConnectedUsers = new Dictionary<string, string>();
        
        //Here we will add a function for register notification (will add sql dependency) where [AddingDate] > @AddedOn
        public void RegisterNotification(DateTime currentTime)
        {
            string conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
            string sqlCommand = @"SELECT [Id],[NotificationTitleId],[DataId],[UserId] ,[Status] ,[AddingDate]  from [dbo].[Notifications] where [AddingDate] > @AddedOn";
            //string sqlCommand = @"SELECT TOP 1 * FROM [dbo].[Notifications] ORDER BY [Id] DESC";
            //string sqlCommand = @"SELECT [Id],[NotificationTitleId],[DataId],[UserId] ,[Status] ,[AddingDate]  from [dbo].[Notifications]";
            //you can notice here I have added table name like this [dbo].[Contacts] with [dbo], its mendatory when you use Sql Dependency
            using (SqlConnection con = new SqlConnection(conStr))
            {
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                SqlCommand cmd = new SqlCommand(sqlCommand, con);
                cmd.Parameters.AddWithValue("@AddedOn", currentTime);
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                cmd.Notification = null;
                SqlDependency sqlDep = new SqlDependency(cmd);
                sqlDep.OnChange += sqlDep_OnChange;
                //we must have to execute the command here
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dBChangeNotificationModelList.Clear();
                    // nothing need to add here now
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DBChangeNotificationModel dBChangeNotificationModel = new DBChangeNotificationModel()
                            {
                                Id = reader.GetInt32(0),
                                NotificationTitleId = reader.GetInt32(1),
                                DataId = reader.GetInt32(2),
                                UserId = reader.GetInt32(3),
                                //Status = reader.GetBoolean(4)==null?false:true,
                                AddingDate = reader.GetDateTime(5)
                            };

                            dBChangeNotificationModelList.Add(dBChangeNotificationModel);
                        }
                        notificationHub.Clients.All.test2("test2");
                    }
                    else
                    {
                        notificationHub.Clients.All.test3("test3");
                    }
                }
            }
        }

        void sqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            //or you can also check => if (e.Info == SqlNotificationInfo.Insert) , if you want notification only for inserted record
            if (e.Type == SqlNotificationType.Change)
            {
                //from here we will send notification message to client
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                //Get Connected users
                NotificationHub notificationHub2 = new NotificationHub();
                Dictionary<string, string> lstAllConnectedUsers = notificationHub2.GetLstAllConnectedUsers();                

                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange;


                List<string> clientList = new List<string>();
                foreach (var item in dBChangeNotificationModelList)
                {
                    foreach (KeyValuePair<string, string> entry in lstAllConnectedUsers)
                    {
                        if (item.UserId == Convert.ToInt32(entry.Key))
                        {
                            clientList.Add(entry.Value);
                        }
                    }
                }
                clientList.Distinct();

                notificationHub.Clients.All.test(dBChangeNotificationModelList.Count, lstAllConnectedUsers.Count, clientList.Count);
                //notificationHub.Clients.All.test1(1, 2, 3);
                //notificationHub.Clients.Clients(clientList).notify("added");

                //notificationHub.Clients.All.notify("added");



                //re-register notification
                RegisterNotification(DateTime.Now);

            }
        }

        //Methods
        public List<Notifications> GetContacts(DateTime afterDate)
        {
            using (BBServiceEntities dc = new BBServiceEntities())
            {
                return dc.Notifications.Where(a => a.AddingDate > afterDate).OrderByDescending(a => a.AddingDate).ToList();
            }
        }

        public List<DBChangeNotificationModel> GetDBChangeNotificationModelList()
        {
            return dBChangeNotificationModelList;
        }

        /**/


        /*    
        public void test()
        {

            DBChangeNotificationModel dBChangeNotificationModel = new DBChangeNotificationModel()
            {
                Id = 1,
                NotificationTitleId = 1,
                DataId = 16,
                UserId = 1,
                AddingDate = DateTime.Now
            };

            dBChangeNotificationModelList.Add(dBChangeNotificationModel);
        }

        public void getLastAdded()
        {
            string conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
            using (var tableDemendency = new SqlTableDependency<Notifications>(conStr, "Notifications"))
            {
                tableDemendency.OnChanged += TableDependency_Changed;
                tableDemendency.Start();
                tableDemendency.Stop();
            }
        }

        void TableDependency_Changed(object sender, RecordChangedEventArgs<Notifications> e)
        {
            if (e.ChangeType != ChangeType.None)
            {
                var changeEntity = e.Entity;

                DBChangeNotificationModel dBChangeNotificationModel = new DBChangeNotificationModel()
                {
                    Id = changeEntity.Id,
                    NotificationTitleId = changeEntity.NotificationTitleId,
                    DataId = changeEntity.DataId,
                    UserId = changeEntity.UserId,
                    AddingDate = changeEntity.AddingDate,
                };

                dBChangeNotificationModelList.Add(dBChangeNotificationModel);

            }
        }

        */
    }
}