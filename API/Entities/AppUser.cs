using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser:IdentityUser<int>
    {/*
    If you did call username something of a user with a capital N and a name, then you'll have a bit more
    refactoring to do.
    This is the reason I chose to use lists earlier to keep the refactoring at this stage down to an absolute minimum.
    So what we don't need anymore is any of these top four fields because the password, hash and password
    salt, albeit identity, doesn't use a password salt column.
    But this is created for us.
    */
       /* public int Id { get; set; } 
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }*/
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }=DateTime.Now;

        public DateTime LastActive { get; set; }=DateTime.Now;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }

        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<Photo> Photos { get; set; }
/*
        public int GetAge(){
            return DateOfBirth.CalculateAge();
        }*/
        //users which liked me (me=current logged in user)
        public ICollection<UserLike> LikedByUsers { get; set; }
        //users which i liked
        public ICollection<UserLike> LikedUsers { get; set; }

        public ICollection<Message> MessagesSent{ get; set;}
        public ICollection<Message> MessagesReceived { get; set; }
        
        public ICollection<AppUserRole> UserRoles{get;set;}

    }
}