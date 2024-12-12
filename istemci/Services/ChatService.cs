using System;
using System.Collections.Generic;
using chat_site_istemci.Models;
using chat_site_istemci.Services;

namespace chat_site_istemci.Services
{
    public class ChatService : IChatService
    {
        // هنا يمكن أن تضيف اتصالاً بقاعدة البيانات
        public List<Chat> GetAllChats()
        {
            // بيانات وهمية لتجربة الخدمة
            return new List<Chat>
            {
                new Chat { ChatId = 1, Name = "Vincent Porter", ImageUrl = "https://bootdey.com/img/Content/avatar/avatar1.png", StatusIcon = "fa-circle offline", StatusText = "left 7 mins ago" },
                new Chat { ChatId = 2, Name = "Aiden Chavez", ImageUrl = "https://bootdey.com/img/Content/avatar/avatar2.png", StatusIcon = "fa-circle online", StatusText = "online" }
            };
        }

        public Chat GetChatById(int id)
        {
            // بيانات وهمية لتجربة الخدمة
            return new Chat
            {
                ChatId = id,
                Name = "Aiden Chavez",
                ImageUrl = "https://bootdey.com/img/Content/avatar/avatar2.png",
                LastSeen = "Last seen: 2 hours ago",
                Messages = new List<Message>
                {
                    new Message { Alignment = "text-right", CssClass = "other-message float-right", Text = "Hi Aiden, how are you? How is the project coming along?", Time = "10:10 AM, Today", UserImageUrl = "https://bootdey.com/img/Content/avatar/avatar7.png" },
                    new Message { Alignment = "", CssClass = "my-message", Text = "Are we meeting today?", Time = "10:12 AM, Today" },
                    new Message { Alignment = "", CssClass = "my-message", Text = "Project has been already finished and I have results to show you.", Time = "10:15 AM, Today" }
                }
            };
        }
    }
}
