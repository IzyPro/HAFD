using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HAFD.DTOs;
using HAFD.Services;
using HAFD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HAFD.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HAFD.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private IUserServices _userService;
        IHostelService _hostelService;
        private IAzureService _azureService;
        private IMapper _mapper;
        public UserController(IUserServices userService, IMapper mapper, IAzureService azureService, IHostelService hostelService)
        {
            _userService = userService;
            _azureService = azureService;
            _hostelService = hostelService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }


        [HttpGet]
        public IActionResult VerifyOccupant()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOccupant(string name)
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null)
                {
                    var (result, response) = await _azureService.IdentifyFacesAsync(files);
                    if (result.isSuccess)
                    {
                        return RedirectToAction(nameof(VerificationResult), response);
                    }
                    else
                    {
                        ViewBag.ErrorMsg = result.Message;
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "Unable to access photo";
                    return View();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpGet]
        public IActionResult VerificationResult(List<User> users)
        {
            return View(users);
        }

        public async Task<IActionResult> GetUserProfile()
        {
            var (result, user) = await _userService.GetUserProfileAsync();
            if (result.isSuccess && user != null)
            {
                var userProfile = _mapper.Map<UserProfileResponseDTO>(user);
                return View(userProfile);
            }
            else
                return View(result);
        }

        public async Task<IActionResult> UpdateProfile([FromForm] UserProfileViewModel model)
        {
            var (result, user) = await _userService.UpdateProfileAsync(model);
            if (result.isSuccess && user != null)
            {
                var userProfile = _mapper.Map<UserProfileResponseDTO>(user);
                return View(userProfile);
            }
            else
                return View(result);
        }

        
        public async Task<IActionResult> HostelApplication(int id)
        {
            var result = await _hostelService.Apply(id);
            if (result.isSuccess)
            {
                ViewBag.Success = result.Message;
                return View(result);
            }
            else
            {
                ViewBag.ErrorMsg = result.Message;
                return View(result);
            }
        }
    }
}
