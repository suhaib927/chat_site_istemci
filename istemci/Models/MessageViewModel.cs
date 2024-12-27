using chat_site_istemci.Entities;

namespace chat_site_istemci.Models
{
    public class MessageViewModel
    {
        public User Sender { get; set; }
        public Message Message { get; set; }
    }
}
