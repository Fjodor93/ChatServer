using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
public class Announcement
{
    public string role { get; set; }
    public string user { get; set; }
    public string message { get; set; }
}

public class ChattHUB : Hub
{
        private static readonly List<Announcement> Announcements = new();
        private static readonly Dictionary<string, string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (ConnectedUsers.TryGetValue(Context.ConnectionId, out string userName))
            {
                ConnectedUsers.Remove(Context.ConnectionId);
                await Clients.All.SendAsync("UserLeft", $"{userName} har lämnat chatten.");

                await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Values.ToList());
            }
            await base.OnDisconnectedAsync(exception);
        }

    public async Task JoinChat(string userName, string role)
    {
        ConnectedUsers[Context.ConnectionId] = role + " " + userName;
        await Clients.All.SendAsync("UserJoined", $"{role} {userName} har anslutit till chatten.");

        await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Values.ToList());
        BroadcastAnnouncements();
    }
    

    public async Task SendMessage(string role, string userName, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", role, userName, message);
    }

    public async Task SendAnnouncement(string role, string userName, string message)
    {
        if (role == "Lärare")
        {
            var an = new Announcement { role =role, user = userName, message = message };
            Announcements.Add(an);
            BroadcastAnnouncements();
        }
    }

   
    private Task BroadcastAnnouncements()
    {
        return Clients.All.SendAsync("ReceiveAnnouncement", Announcements);
    }
}