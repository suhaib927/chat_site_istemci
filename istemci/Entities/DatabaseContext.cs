using Microsoft.EntityFrameworkCore;
using chat_site_istemci.Entities;

namespace chat_site_istemci.Entities
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .Property(m => m.SenderId)
                .IsRequired(false); 

            modelBuilder.Entity<Message>()
                .Property(m => m.ReceiverId)
                .IsRequired(false);

            modelBuilder.Entity<Message>()
                .Property(m => m.GroupId)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(u => u.ProfileImageFileName)
                .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-P6DGD12\\SQLEXPRESS;Database=chat_site_istemci;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }
    }
}
