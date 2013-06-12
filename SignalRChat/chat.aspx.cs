using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SignalRChat
{
    public partial class Test : System.Web.UI.Page
    {
        public string Title = "deevent:";
        public string UserName = "Test";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["event"] != null) this.Title = Request.QueryString["event"];
            if (Request.QueryString["UserName"] != null) this.UserName = Request.QueryString["UserName"];
        }
    }
}