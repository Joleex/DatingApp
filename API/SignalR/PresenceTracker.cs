using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        /* every time a user connects to the hub.
        They're going to be given a connection ID.
        Now there's nothing to stop a user from connecting to the same application from a different device and
        they would get a different connection ID for each different connection that they're having or making
        to our application*/
        //store a list of connection IDs-string
        private static readonly Dictionary<string, List<string>> OnlineUsers=
            new Dictionary<string, List<string>>();

            /*And then what we need to do is create a couple of methods to add a user to the dictionary when they
            connect along with their connection ID. And also handle the occasion when they disconnect.*/

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline=false;
            /*Now we need to be careful here because this dictionary is going to be shared amongst everyone who connects
            to our server.
            And the dictionary is not a threat safe resource.
            So if we had concurrent users trying to update this at the same time, then we're probably going to
            run into problems.
            So what we need to do to get around that is we need to lock the dictionary.*/
            /*So we're effectively locking the dictionary until we've finished doing what we're doing inside here
            and what we want to check to see if that we already have.
            A dictionary element with a key of the currently logged in username.
            And if it is, then we're going to add the connection ID.
            Otherwise, we're going to create a new dictionary entry for this particular username with that connection
            ID.*/
            lock(OnlineUsers)
            {
                if(OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else 
                {
                    OnlineUsers.Add(username, new List<string>{connectionId});
                    isOnline=true;
                }
            }

            return Task.FromResult(isOnline);
        }


        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline=false;
            lock(OnlineUsers)
            {
                if(!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                OnlineUsers[username].Remove(connectionId);
                if(OnlineUsers[username].Count==0)
                {
                    OnlineUsers.Remove(username);
                    isOffline=true;
                }
            }

            return Task.FromResult(isOffline);
        }
        /*And what we also need inside here is a method to get all of the users that are currently connected.*/

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock(OnlineUsers)
            {
                onlineUsers=OnlineUsers.OrderBy(k=>k.Key).Select(k=>k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }
        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionIds);
        }
    }
}