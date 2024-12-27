using chat_site_istemci.Entities;

namespace chat_site_istemci.Models
{
    public class ChatViewModel
    {
        public Guid myId {  get; set; }
        public User user { get; set; }
        public Group group { get; set; }
         public List<Message> Messages { get; set; } = new List<Message>();
    }
}
