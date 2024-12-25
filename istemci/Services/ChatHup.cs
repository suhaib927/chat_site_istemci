using chat_site_istemci.Entities;
using Microsoft.AspNetCore.SignalR;

namespace chat_site_istemci.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(User user, Message message,string sentAt)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, sentAt);
        }
    }

}
