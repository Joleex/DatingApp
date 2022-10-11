using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query= _context.Users.AsQueryable();
               /* .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()//So all we need to do is read this.We don't need to do anything else.
                .AsQueryable();*/

                query=query.Where(u=>u.UserName!=userParams.CurrentUsername);
                query=query.Where(u=>u.Gender==userParams.Gender);
                //2022-150 - first year of someone's birth
                var minDob=DateTime.Today.AddYears(-userParams.MaxAge-1);
                //2022-18 -last year of someone's birth
                var maxDob=DateTime.Today.AddYears(-userParams.MinAge);
                query=query.Where(u=> u.DateOfBirth >=minDob && u.DateOfBirth <=maxDob);

                query=userParams.OrderBy switch
                {
                    "created"=> query.OrderByDescending(u=>u.Created), // case for created
                    _ => query.OrderByDescending(u=>u.LastActive)//case for default
                };
                
                /*And because we created a static method on our page list called create async, this gives us the facility 
                to create a paged list at this stage in our repository.*/
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.
            ConfigurationProvider).AsNoTracking(),
             userParams.pageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<string> GetUserGander(string username)
        {
            return await _context.Users
                    .Where(x=>x.UserName==username)
                    .Select(x=>x.Gender)
                    .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}