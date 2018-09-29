using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using WebServer;
using System.Web;
using System.Threading;
namespace OWIN_SignalR
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://+:6667/";
            WebServer.WebApiApplication.Application_Start();
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
                while (true)
                {
                    Thread.Sleep(1440000); //a day
                }
            }
        }
    }
}
