using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            /*And we've got two choices here add identity and add identity core.
            Now, if we were building an MVC type of application where our client side of the application was being
            served by nets, and we would use something called razor pages, which are then served by the dot net
            server, then we could use the default identity and this would give us a full setup.
            It would give us the pages we need, it would give us cookie based authentication because then the user
            is always maintaining a session with the server because the server pages or the HTML is being generated
            from the server itself.*/
            /*We've got a different kind of application.
            We've gone through a single page application using Angular and we're using token based authorization.
            So what we're going to use is add identity core and anything extra we're going to add.
            So we get the basics, but we'll also add on some extra stuff inside here and we'll change that on to
            our configuration.*/
            services.AddIdentityCore<AppUser>(opt=>
            {/*So if you wanted to configure this and you're using or you're not interested in complex passwords,
            then you can come in here and specify what you want.*/
                opt.Password.RequireNonAlphanumeric=false;
                
            }).AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>() //The type of the role manager to add.; 
                //RoleManager-Provides the APIs for managing roles in a persistence store.
                .AddSignInManager<SignInManager<AppUser>>()
                .AddRoleValidator<RoleValidator<AppRole>>()
                .AddEntityFrameworkStores<DataContext>();

             services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer( options=>
             {
                options.TokenValidationParameters=new TokenValidationParameters{
                    ValidateIssuerSigningKey=true,
                    IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                    /*the issuer of the token is our API server*/
                    ValidateIssuer=false,
                    /*audience of the token is our Angular application*/
                    ValidateAudience=false,

                    
                };
                 options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && 
                                path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
            });

            services.AddAuthorization(opt=>
            {
                opt.AddPolicy("RequireAdminRole", policy=> policy.RequireRole("Admin"));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }
        
    }
}