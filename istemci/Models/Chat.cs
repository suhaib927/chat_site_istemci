using System.Collections.Generic;

namespace chat_site_istemci.Models
{
    public class Chat
    {
        public int ChatId { get; set; }
        public string Name { get; set; } = string.Empty; // قيمة افتراضية
        public string ImageUrl { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string LastSeen { get; set; } = string.Empty;
        public List<Message> Messages { get; set; }
    }
}
// مثلا جينا لهون نحن وكتبنا كود جديد
//
//لما اكبس على الزائد رح يضيف كلشي عدلتو انا
//
//
//
//
//
//
//
//
// رح يبين عندي كلشي جديد هون 