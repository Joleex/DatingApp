using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username))
            return BadRequest("Username is taken");

            var user=_mapper.Map<AppUser>(registerDto);
            /*we guarantee that as soon as we're finished with this class, it's disposed of correctly*/
                user.UserName=registerDto.Username.ToLower();
            //And this pay off creates our user and saves the changes into the database.
            var result=await _userManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);
            /*we're going to put any
            newly registered user into the member's role, and we'll check and see if the role results is successful.*/
            var roleResult=await _userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto{
                Username=user.UserName,
                Token= await _tokenService.CreateToken(user),
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){

            var user =await _userManager.Users
            .Include(p=>p.Photos) //we can still use the include method to bring back the photos for the user
            .SingleOrDefaultAsync(x=>x.UserName==loginDto.Username.ToLower());

            if(user==null) return Unauthorized("Invalid username");

            var result=await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if(!result.Succeeded) return Unauthorized();
          
            return new UserDto{
                Username=user.UserName,
                Token=await _tokenService.CreateToken(user),
                PhotoUrl=user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };
        }
        private async Task<bool> UserExists(string username){
            return await _userManager.Users.AnyAsync(x=> x.UserName == username.ToLower());
        }
    }
}