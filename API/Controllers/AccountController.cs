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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username))
            return BadRequest("Username is taken");

            var user=_mapper.Map<AppUser>(registerDto);
            /*we guarantee that as soon as we're finished with this class, it's disposed of correctly*/
            using var hmac=new HMACSHA512();
                user.UserName=registerDto.Username.ToLower();
                user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
                user.PasswordSalt=hmac.Key;
            
            /*And if we look at the description of this, it says it begins tracking the given entity and any other
            reachable entities that are not already being tracked.
            So we're not actually adding anything to the database here.
            All we're doing is tracking this now in entity framework.*/
            _context.Users.Add(user);
            /*We do actually call our database and we save our user into our user's table.*/
            await _context.SaveChangesAsync();

            return new UserDto{
                Username=user.UserName,
                Token=_tokenService.CreateToken(user),
                KnownAs=user.KnownAs
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){

            var user =await _context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName==loginDto.Username);

            if(user==null) return Unauthorized("Invalid username");

            using var hmac=new HMACSHA512(user.PasswordSalt);

            var computedHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0;i<computedHash.Length;i++){
                if(computedHash[i]!=user.PasswordHash[i])
                    return Unauthorized("Invalid password");
            }
            return new UserDto{
                Username=user.UserName,
                Token=_tokenService.CreateToken(user),
                PhotoUrl=user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs=user.KnownAs
            };
        }
        private async Task<bool> UserExists(string username){
            return await _context.Users.AnyAsync(x=> x.UserName == username.ToLower());
        }
    }
}