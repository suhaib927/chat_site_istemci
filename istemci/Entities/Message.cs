﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

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

        public string ReceiverId { get; set; }

        public string? GroupId { get; set; }


        public string MessageContent { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        public string Type { get; set; }

        public bool Status { get; set; } = false; // Delivered or not

    }
}
