using HAFD.Data;
using HAFD.Models;
using HAFD.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAFD.Services
{
    public interface IHostelService
    {
        Task<ResponseManager> Apply(int id);
        Task<List<Hostel>> GetAllAvailableHostels();
        Task<ResponseManager> CreateHostel(Hostel model);
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
            var result = await _userManager.UpdateAsync(user);

            hostel.IsAvailable = false;
            _context.Hostels.Update(hostel);
            var save = await _context.SaveChangesAsync();
            if (save >= 1 && result.Succeeded)
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

        public async Task<List<Hostel>> GetAllAvailableHostels()
        {
            return await _context.Hostels.Where(x => x.IsAvailable).ToListAsync();
        }

        public async Task<ResponseManager> CreateHostel(Hostel model)
        {
            var existingHostel = await _context.Hostels.FirstOrDefaultAsync(x => x.Corner.ToLower() == model.Corner.ToLower() && x.Name.ToLower() == model.Name.ToLower() && x.Room.ToLower() == model.Room.ToLower());
            if (existingHostel != null)
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "Bedspace already exists"
                };
            var hostel = new Hostel
            {
                Name = model.Name,
                Room = model.Room,
                Corner = model.Corner,
                IsAvailable = true
            };
            await _context.Hostels.AddAsync(hostel);
            var result = await _context.SaveChangesAsync();
            if (result >= 1)
                return new ResponseManager
                {
                    isSuccess = true,
                    Message = "Hostel Created Successfully"
                };
            else
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "Unable to create hostel"
                };
        }
    }
}
