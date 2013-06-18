using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace SignalRChat
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // Register the default hubs route: ~/signalr/hubs
            RouteTable.Routes.MapHubs();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
            PingServer(URL);
        }

        private void AutoExec(object sender, System.Timers.ElapsedEventArgs e)
        {
            return;
        }
        public string URL = "http://myevents.apphb.com/";
        public void PingServer(string url)
        {
            try
            {
                WebClient http = new WebClient();
                string Result = http.DownloadString(url);
            }
            catch (Exception ex)
            {
                string Message = ex.Message;
            }
        }
    }
}
