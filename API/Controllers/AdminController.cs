using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController:BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy="RequireAdminRole")]
        [HttpGet("users-with-roles")]  //we want to return the list of users with the roles
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users=await _userManager.Users
                .Include(r=> r.UserRoles) //So we need to get our user roles related list of user roles,
                .ThenInclude(r => r.Role)// but we also need to include the role itself inside there.
                .OrderBy(u=>u.UserName)
                .Select(u=> new 
                {/*So we're going to get an object back with the user's ID, their user name and the roles that they're in.*/
                    u.Id,
                    Username=u.UserName,
                    Roles=u.UserRoles.Select(r=> r.Role.Name).ToList() //And then we're sending that to a list.
                })
                .ToListAsync();
            return Ok(users);    
        }

        [Authorize(Policy ="RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles= roles.Split(",").ToArray();
            var user=await _userManager.FindByNameAsync(username);
            /*if we don't check this we will get an exception because if we try and execute this method on null
            ->  var userRoles=await _userManager.GetRolesAsync(user);
             then this is going to return an exception to us*/
            if(user==null) return NotFound("Could not find user");

            var userRoles=await _userManager.GetRolesAsync(user);

            var result=await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if(!result.Succeeded) return BadRequest("Failed to add to roles");

            result=await _userManager.RemoveFromRolesAsync(user, userRoles.Except(userRoles));
            if(!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }


        [Authorize(Policy="ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")] 
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");    
        }
        
    }
}