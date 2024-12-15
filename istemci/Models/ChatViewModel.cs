using chat_site_istemci.Entities;

namespace chat_site_istemci.Models
{
    public class ChatViewModel
    {
        public User user { get; set; }
         public List<Message> Messages { get; set; }
    }
}
