using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using API.Helpers;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {

        /*But what we'll use extension methods for a bit later on is to save us from typing repetitive code where
        we can put it inside an extension method and reuse this method over and over again.
        But the purpose of this was to try to keep our startup class as clean as possible, and all of the services
        that we create will just put inside our application services.*/
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
             services.AddScoped<ITokenService, TokenService>();
             services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddDbContext<DataContext>(options=>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}