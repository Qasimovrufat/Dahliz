using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using BBService.Controllers;
using BBService.MyClasses;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(BBService.OWINStartup))]

namespace BBService
{
    public class OWINStartup
    {
        public void Configuration(IAppBuilder app)
        {
            //Localhost
            GlobalConfiguration.Configuration.UseSqlServerStorage(@"data source=PC068;initial catalog=BBService;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework");


            //Test Server
            //GlobalConfiguration.Configuration.UseSqlServerStorage(@"data source=10.1.20.114,1433\BB-PASHA\SQLEXPRESS;initial catalog=BBServiceTest;persist security info=True;user id=sa;password=550AA218ss252;MultipleActiveResultSets=True;App=EntityFramework");

            //Server
            //GlobalConfiguration.Configuration.UseSqlServerStorage(@"data source=10.1.20.114,1433\BB-PASHA\SQLEXPRESS;initial catalog=BBService;persist security info=True;user id=sa;password=550AA218ss252;MultipleActiveResultSets=True;App=EntityFramework");


            //Background jobs
            BackgroundJobsMeController bgJobs = new BackgroundJobsMeController();
            RecurringJob.AddOrUpdate(() => bgJobs.MaintenanceFirstCheckUp(), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => bgJobs.MaintenanceSecondCheckUp(), Cron.Minutely);

            app.UseHangfireDashboard();
            app.UseHangfireServer();


            //SignalR
            //var idProvider = new CustomUserIdProvider();
            //GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);
            app.MapSignalR();





            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}