using chat_site_istemci.Entities;

namespace chat_site_istemci.Models
{
    public class ChatsListViewModel
    {
        public List<User> users { get; set; }
        public List<Group> groups { get; set; }
    }
}
