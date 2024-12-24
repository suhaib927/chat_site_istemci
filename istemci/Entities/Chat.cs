namespace chat_site_istemci.Entities
{
    public class Chat
    {
        public string ChatKey { get; set; }
        public List<Message> Messages = new List<Message>();
    }
}
