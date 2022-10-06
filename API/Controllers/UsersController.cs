using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
        }
        
        //[Authorize(Roles ="Admin")]
        [HttpGet]
        /*because it's a query string, we're going to need to specify [FromQuery], we're going
        to have to give our API controller this attribute. 
        So it's going to get the user parameters from the query string.
        And this is just because a query string itself doesn't need to be inside an object, so it's not sure
        what to do with this.*/
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams){
            
            var user= await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername=user.UserName;
            if(string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender=user.Gender=="male"?"female":"male";

            var users=await _userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

        //[Authorize(Roles ="Member")]
        //api/users/3
        [HttpGet("{username}", Name ="GetUser")]
        /*list or ienumerable but ienumerable is more proprieted type*/
        public async Task<ActionResult<MemberDto>> GetUser(string username){
            return await _userRepository.GetMemberAsync(username);

            
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            
           var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
            
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {   
            var user=await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            var result= await _photoService.AddPhotoAsync(file);
            if(result.Error !=null) return BadRequest(result.Error.Message);
            
            var photo=new Photo
            {
                Url=result.SecureUrl.AbsoluteUri,
                PublicId=result.PublicId
            };

            if(user.Photos.Count==0)
            {
                photo.IsMain=true;
            }
            user.Photos.Add(photo);
            if(await _userRepository.SaveAllAsync()){
                //return _mapper.Map<PhotoDto>(photo);
                /*So now we're going to return a201 and we're going to return the roots of how to get the user which contains
                the photos.And we're going to still return our photo object.*/
                return CreatedAtRoute("GetUser",new{username=user.UserName},_mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
            
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){
            var user=await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo=user.Photos.FirstOrDefault(x=> x.Id==photoId);

            if(photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain=user.Photos.FirstOrDefault(x=>x.IsMain);
            if(currentMain!=null) currentMain.IsMain=false;
            photo.IsMain=true;

            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId){
            var user=await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo= user.Photos.FirstOrDefault(x=> x.Id==photoId);
            if(photo==null) return  NotFound();
            if(photo.IsMain) return BadRequest("You cannot delete your main photo");
            if(photo.PublicId!=null){
               var result= await _photoService.DeletePhotoAsync(photo.PublicId);
               if(result.Error!= null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete a photo!");
        }
        
    }
}