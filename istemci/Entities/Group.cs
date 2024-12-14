using System.ComponentModel.DataAnnotations;

namespace chat_site_istemci.Entities
{
    public class Group
    {
        [Key]
        public Guid GroupId { get; set; }
        [Required]
        [StringLength(50)]
        public string GroupName { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string GroupImageUrl { get; set; } = "images.jpg";
        public int MaxMembers { get; set; }

        public ICollection<GroupMember> Members { get; set; } // Members of the group
        public ICollection<Message> Messages { get; set; } // Messages sent to the group

    }
}
