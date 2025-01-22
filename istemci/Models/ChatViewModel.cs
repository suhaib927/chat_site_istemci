using chat_site_istemci.Entities;

namespace chat_site_istemci.Models
{
    public class ChatViewModel
    {
        public Guid myId {  get; set; }
        public User user { get; set; }
        public Group group { get; set; }
        public string broadcast { get; set; } = "c977d8ac-691c-41b3-8935-dcf2b0f68c86";
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
