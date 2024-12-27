using chat_site_istemci.Entities;
using Microsoft.AspNetCore.SignalR;

namespace chat_site_istemci.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(Message message,string sentAt,Guid myId)
        {
            await Clients.All.SendAsync("ReceiveMessage", message, sentAt , myId);
        }
    }

}
