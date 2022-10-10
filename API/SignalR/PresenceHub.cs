using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub: Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            if(isOnline)
            //these are the clients that are connected to this hub, and what we'r going to do
            //here is send the message to all of the others
            //so this is everybody except the connection that triggered the current invocation
            //
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            var currentUsers=await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);//send updated list of users whoes connected
           
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline= await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            if(isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            /*var currentUsers=await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);//send updated list of users whoes connected
       */
            await base.OnDisconnectedAsync(exception);
            
       
        }
    }
}