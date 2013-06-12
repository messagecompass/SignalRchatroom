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
        #region Data Members

        static List<UserDetail> ConnectedUsers = new List<UserDetail>();
        static List<MessageDetail> CurrentMessage = new List<MessageDetail>();

        public List<MessageDetail> SameGroupMessage(string username)
        {
            string eventName = ConnectedUsers.FirstOrDefault(u => u.UserName == username).Event;
            var userlist = ConnectedUsers.Where(u => u.Event == eventName).Select(u=>u.UserName);
            return CurrentMessage.Where(m => userlist.Contains(m.UserName)).ToList();
        }


        public string[] GetOtherGroupUsers(string userName)
        {
            var result =  ConnectedUsers.Where(u => u.Event != ConnectedUsers.FirstOrDefault(un => un.UserName == userName).Event).Select(u => u.UserName).ToList();
            //result.Add(userName);
            return result.ToArray();
        }

        public List<UserDetail> GetSameGroupUsers(string userName)
        {
            var result = ConnectedUsers.Where(u => u.Event == ConnectedUsers.FirstOrDefault(un => un.UserName == userName).Event).ToList();
            return result;
        }

        #endregion

        #region Methods

        public void Connect(string userName)
        {
            var id = Context.ConnectionId;


            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserDetail { ConnectionId = id, UserName = userName });

                // send to caller
                Clients.Caller.onConnected(id, userName, ConnectedUsers, SameGroupMessage(userName));

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, userName);

            }

        }
        public void Connect(string userName, string eventName)
        {
            var id = Context.ConnectionId;


            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserDetail { ConnectionId = id, UserName = userName, Event= eventName});

                this.Groups.Add(id, eventName);
                // send to caller
                Clients.Caller.onConnected(id, userName, GetSameGroupUsers(userName), SameGroupMessage(userName));

                // send to all except caller client
                //Clients.AllExcept(GetOtherGroupUsers(userName)).onNewUserConnected(id, userName);

            }

        }

        public void SendMessageToAll(string userName, string message)
        {
            // store last 100 messages in cache
            AddMessageinCache(userName, message);
            // Broad cast message
            string groupname = ConnectedUsers.FirstOrDefault(un => un.UserName == userName).Event;
            //Clients.Group(groupname).receiveMessage(userName, message);
            //Clients.Group("same", GetOtherGroupUsers(userName)).messageReceived(userName, message);
            Clients.Caller.messageReceived(userName, message);
            Clients.OthersInGroup(groupname).messageReceived(userName, message);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {

            string fromUserId = Context.ConnectionId;

            var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId) ;
            var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser != null && fromUser!=null)
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
            CurrentMessage.Add(new MessageDetail { UserName = userName, Message = message });

            if (CurrentMessage.Count > 100)
                CurrentMessage.RemoveAt(0);
        }

        #endregion
    }

}