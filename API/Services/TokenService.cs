using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser user)
        {
            /*adding our claims*/
            var claims=new List<Claim> {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            /*creating some credentials*/
            var creds=new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            /*describing how our tokens are going to look*/
            var tokenDescriptor=new SecurityTokenDescriptor{
                Subject=new ClaimsIdentity(claims),
                Expires=DateTime.Now.AddDays(7),
                SigningCredentials=creds
            };

            var tokenHandler=new JwtSecurityTokenHandler();

            /*create a token*/
            var token=tokenHandler.CreateToken(tokenDescriptor);

            /*return the written token to whoever needs it*/
            return tokenHandler.WriteToken(token);
        }
    }
}