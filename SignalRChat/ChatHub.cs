using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using SignalRChat.Common;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        public ChatHub()
        {
            CurrentMessage = new List<MessageDetail>();
            using (var dbc = new dbMessageEntities())
            {
                CurrentMessage = (from msg in dbc.Messages.OrderByDescending(m => m.Sent).Take(100)
                                  select new MessageDetail()
                                  {
                                      Message = msg.Content,
                                      UserName = msg.UserName,
                                      Sent = (System.DateTime)msg.Sent,
                                      GroupName = msg.GroupName
                                  }).ToList();

                var distincUsers = CurrentMessage.GroupBy(x => x.UserName + x.GroupName).Select(x => x.First()).Where(u => u.UserName != null);
                if (ConnectedUsers.Count == 0)
                {
                    ConnectedUsers = (from msg in distincUsers
                                      select new UserDetail()
                                          {
                                              Event = msg.GroupName,
                                              UserName = msg.UserName
                                          }).ToList();
                }
            }
        }
        #region Data Members

        static List<UserDetail> ConnectedUsers = new List<UserDetail>();
        static List<MessageDetail> CurrentMessage = new List<MessageDetail>();

        public List<MessageDetail> SameGroupMessage(string username)
        {
            string eventName = ConnectedUsers.FirstOrDefault(u => u.UserName == username).Event;
            return CurrentMessage.Where(m => m.GroupName == eventName).ToList();
        }


        public string[] GetOtherGroupUsers(string userName)
        {
            var result = ConnectedUsers.Where(u => u.Event != ConnectedUsers.FirstOrDefault(un => un.UserName == userName).Event).Select(u => u.UserName).ToList();
            //result.Add(userName);
            return result.ToArray();
        }

        public List<UserDetail> GetSameGroupUsers(string userName)
        {
            var result = ConnectedUsers.Where(u=>u.Event != null).Where(u => u.Event == ConnectedUsers.FirstOrDefault(un => un.UserName == userName).Event).ToList();
            return result;
        }

        #endregion

        #region Methods

        public void Connect(string userName, string eventName)
        {
            var id = Context.ConnectionId;
            if (ConnectedUsers.Count(x => x.ConnectionId == id) > 0) return;

            UserDetail user = new UserDetail { ConnectionId = id, UserName = userName, Event = eventName };

            if (ConnectedUsers.FirstOrDefault(u => u.UserName == userName) == null)
            {
                ConnectedUsers.Add(user);
            }
            else
            {
                ConnectedUsers.FirstOrDefault(u => u.UserName == userName).ConnectionId = id;
                ConnectedUsers.FirstOrDefault(u => u.UserName == userName).Event = eventName;
            }
            
            this.Groups.Add(id, eventName);
            // send to caller
            Clients.Caller.onConnected(id, userName, GetSameGroupUsers(userName), SameGroupMessage(userName));

            // send to all except caller client
            //Clients.AllExcept(GetOtherGroupUsers(userName)).onNewUserConnected(id, userName);

        }

        public void SendMessageToAll(string userName, string message)
        {
            // store last 100 messages in cache
            AddMessageinCache(userName, message);
            // Broad cast message
            var user = ConnectedUsers.FirstOrDefault(un => un.UserName == userName);
            
            string groupname = (user != null)?user.Event:"";
            //Clients.Group(groupname).receiveMessage(userName, message);
            //Clients.Group("same", GetOtherGroupUsers(userName)).messageReceived(userName, message);
            Clients.Caller.messageReceived(userName, message);
            Clients.OthersInGroup(groupname).messageReceived(userName, message);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {

            string fromUserId = Context.ConnectionId;

            var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser != null && fromUser != null)
            {
                // send to 
                Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.UserName, message);

                // send to caller user
                Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message);
            }

        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                //Clients.All.onUserDisconnected(id, item.UserName);

            }

            return base.OnDisconnected();
        }


        #endregion

        #region private Messages

        private void AddMessageinCache(string userName, string message)
        {
            string groupName = ConnectedUsers.FirstOrDefault(u => u.UserName == userName) == null ? "" : ConnectedUsers.FirstOrDefault(u => u.UserName == userName).Event;
            var m = new MessageDetail()
            {
                UserName = userName,
                Message = message,
                Sent = System.DateTime.Now,
                GroupName =groupName
            };
            CurrentMessage.Add(m);
            m.Save();
            if (CurrentMessage.Count > 100)
                CurrentMessage.RemoveAt(0);
        }

        #endregion
    }

}