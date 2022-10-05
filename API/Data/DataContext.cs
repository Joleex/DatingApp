using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }

        public DbSet<UserLike> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }

        /*And what we also need to do inside here is give the entities some configuration. 
        And the way that we do that is we need to override a method inside the DB context 
        and what we do to achieve this we will say protected.*/
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            /*So what we'll do is we'll work on our user like entity here and we'll say builder and then we say entity,
            and then we pass in the entity as a type parameter of what we want to configure.
            And this is going to be our user like.*/
            builder.Entity<UserLike>()
                .HasKey(k=> new {k.SourceUserId, k.LikedUserId});
                /*And what we'll do then is we'll specify that this has a key because we didn't identify a primary key
                for this particular entity.
                Then we're going to configure this key ourselves and it's going to be a combination of the source user
                ID and the liked user ID.*/
            builder.Entity<UserLike>() /*trenutno ulogovan korisnik povezujemo sa onima koje on lajkuje*/
                .HasOne(s=> s.SourceUser)
                .WithMany(l=>l.LikedUsers) /*So a source user can, like many other users, is what we're seeing here.*/
                .HasForeignKey(s=>s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

             builder.Entity<UserLike>()/*korisnike koje je ulogovani lajkovao su povezani sa onima koji su ga lajkovali*/
                .HasOne(s=> s.LikedUser)
                .WithMany(l=>l.LikedByUsers) /*So a source user can, like many other users, is what we're seeing here.*/
                .HasForeignKey(s=>s.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
            .HasOne(u=>u.Recipient)
            .WithMany(m=>m.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
            .HasOne(u=>u.Sender)
            .WithMany(m=>m.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}