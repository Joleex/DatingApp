using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            /*So since we want to not do this before the user is actually doing whatever they're doing, we're gonna
            wait until they've done that and then we're going to execute an update this last active property.
            So what we do inside here is we'll get a hold of the context that we get from the next.*/
           var resultContext= await next();
           /*But what we want to do is check to see if the user's authenticated because we don't want to try and
            execute something.*/
            if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;
    /*
            var username=resultContext.HttpContext.User.GetUsername();
            var repo=resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
            var user=await repo.GetUserByUsernameAsync(username);*/
            /*GetUserId je funkcija u ClaimsPrincipleExtensions.cs*/
            var userId=resultContext.HttpContext.User.GetUserId();
            var uow=resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
            var user=await uow.UserRepository.GetUserByIdAsync(userId);
            user.LastActive=DateTime.UtcNow;
            await uow.Complete();
        }
    }
}