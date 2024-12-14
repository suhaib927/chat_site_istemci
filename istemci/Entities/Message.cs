using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using HaircutProject.Entities;

namespace chat_site_istemci.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }

        [Required]
        [ForeignKey(nameof(Sender))]
        public Guid SenderId { get; set; }
        public User Sender { get; set; } // User who sent the message

        [Required]
        [ForeignKey(nameof(Group))]
        public Guid GroupId { get; set; }
        public Group Group { get; set; } // Group the message belongs to

        public string MessageContent { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        public string CssClass { get; set; }
        public string Alignment { get; set; }
        public bool Status { get; set; } = false; // Delivered or not
        public FailedMessage FailedMessage { get; set; } // Optional reference to a failed message

    }
}
