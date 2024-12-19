﻿using chat_site_istemci.Entities;
using Microsoft.EntityFrameworkCore;

namespace chat_site_istemci.Entities
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<FailedMessage> FailedMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring relationships for GroupMember
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId);

            // Configuring relationships for Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            // Add configuration for Receiver
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages) // Assuming you have `ReceivedMessages` in the User model
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade); // Prevent cascading delete


            // Configuring relationships for FailedMessage
            modelBuilder.Entity<FailedMessage>()
                .HasOne(fm => fm.Message)
                .WithOne(m => m.FailedMessage)
                .HasForeignKey<FailedMessage>(fm => fm.MessageId);

            modelBuilder.Entity<User>()
    .Property(u => u.Ip)
    .IsRequired(false); // Allow NULL values


            base.OnModelCreating(modelBuilder);
        }
    }
}
