using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config){

             services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer( options=>{
                options.TokenValidationParameters=new TokenValidationParameters{
                    ValidateIssuerSigningKey=true,
                    IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                    /*the issuer of the token is our API server*/
                    ValidateIssuer=false,
                    /*audience of the token is our Angular application*/
                    ValidateAudience=false,

                    
                };
            });
            return services;
        }
        
    }
}