using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController:BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        public LikesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId=User.GetUserId();
            var likedUser=await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser= await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

            if(likedUser==null)
            return NotFound();

            if(sourceUser.UserName==username) return BadRequest("You cannot like yourself");
            //alow the users to like another user, but not gonna allow or not going to implement a toggle
            var userLike=await _unitOfWork.LikesRepository.GetUserLike(sourceUserId,likedUser.Id);
            //if you want to implement toggle, then you could od something to remove the like and give the users a toggle option
            if(userLike!=null) return BadRequest("You already like this user");

            userLike=new UserLike
            {
                SourceUserId=sourceUser.Id,
                LikedUserId=likedUser.Id
                
            };

            sourceUser.LikedUsers.Add(userLike);
            /*this is temporary because now we got more than one repositoru.
            we need to think about how many instances of the data context we have and what we're doing here,
             but we are not going to break anything from now */
            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to like user");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId=User.GetUserId();
            var users= await _unitOfWork.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }
    }
}