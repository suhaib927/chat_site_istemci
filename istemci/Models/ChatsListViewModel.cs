using chat_site_istemci.Entities;

namespace chat_site_istemci.Models
{
    public class ChatsListViewModel
    {
        public List<User> users { get; set; }
        public List<Group> groups { get; set; }
        public string Broadcast { get; set; } = "c977d8ac-691c-41b3-8935-dcf2b0f68c86";

    }
}
