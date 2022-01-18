using AutoMapper.Configuration;
using HAFD.Data;
using HAFD.Models;
using HAFD.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HAFD.Services
{
    public interface IHostelService
    {
        Task<ResponseManager> Apply(int id);
    }
    public class HostelServices : IHostelService
    {
        private UserManager<User> _userManager;
        private IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private DatabaseContext _context;
        private IUserServices _userServices;

        public HostelServices(IUserServices userServices, UserManager<User> userManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, DatabaseContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _userServices = userServices;
        }

        public async Task<ResponseManager> Apply(int id)
        {
            var user = await _userServices.GetUser();
            if (user == null)
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "Invalid User",
                };
            else if (user.Hostel != null)
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "User already occupies an allocation",
                };
            var hostel = await GetHostelWithId(id);
            user.Hostel = hostel;
            hostel.IsAvailable = false;

            _context.Hostels.Update(hostel);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return new ResponseManager
                {
                    isSuccess = true,
                    Message = "Application Successful"
                };
            else
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "Application Failed"
                };
        }

        public async Task<Hostel> GetHostelWithId(int id)
        {
            return await _context.Hostels.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
