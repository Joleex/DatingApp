using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class UserDto
    {
        /*this is the object we're going to return when a user logs in or registers in this case*/
        public string Username { get; set; }
        public string Token { get; set; }
    }
}