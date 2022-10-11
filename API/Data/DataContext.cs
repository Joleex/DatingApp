using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, 
                IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
                IdentityRoleClaim<int>, IdentityUserToken<int>>
    {/*Now, if we weren't interested in dealing with roles and getting a list of roles, this would be all
    we'd need to do.
    However, because we want to get a list of the user roles, then we need to go a bit further and we
    need to identify every single type. Unfortunately, that we need to add to identity.*/
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserLike> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }

        /*And what we also need to do inside here is give the entities some configuration. 
        And the way that we do that is we need to override a method inside the DB context 
        and what we do to achieve this we will say protected.*/
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Group>()
                .HasMany(x=>x.Connections)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur =>ur.UserId)
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur =>ur.RoleId)
                .IsRequired();

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

            builder.ApplyUtcDateTimeConverter();
        }
    }

    public static class UtcDateAnnotation
{
  private const String IsUtcAnnotation = "IsUtc";
  private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
    new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

  private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
    new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

  public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
    builder.HasAnnotation(IsUtcAnnotation, isUtc);

  public static Boolean IsUtc(this IMutableProperty property) =>
    ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

  /// <summary>
  /// Make sure this is called after configuring all your entities.
  /// </summary>
  public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
  {
    foreach (var entityType in builder.Model.GetEntityTypes())
    {
      foreach (var property in entityType.GetProperties())
      {
        if (!property.IsUtc())
        {
          continue;
        }

        if (property.ClrType == typeof(DateTime))
        {
          property.SetValueConverter(UtcConverter);
        }

        if (property.ClrType == typeof(DateTime?))
        {
          property.SetValueConverter(UtcNullableConverter);
        }
      }
    }
  }
}
}