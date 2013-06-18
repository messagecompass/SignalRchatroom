using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRChat.Common
{
    public class MessageDetail
    {

        public string UserName { get; set; }
        public System.DateTime Sent { get; set; }
        public string Message { get; set; }
        public string GroupName { get; set; }

        public void Save()
        {
            using (var dbc = new dbMessageEntities())
            {
                dbc.Messages.Add(new Message() {
                    Sent = System.DateTime.Now,
                    Content = Message,
                    UserName = UserName,
                    GroupName = GroupName
               });
                dbc.SaveChanges();
            }
        }
    }
}