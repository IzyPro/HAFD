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
using HAFD.Helpers;

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
                        TempUser.VerificationResult = response;
                        return Json(new { text = Url.Action(nameof(VerificationResult), "User"), success = true });
                        //return RedirectToAction("VerificationResult", "User");
                    }
                    else
                    {
                        return Json(new { text = result.Message, success = false });
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
        public IActionResult VerificationResult()
        {
            return View();
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

        [HttpGet]
        public async Task<IActionResult> HostelApplication()
        {
            return View(await _hostelService.GetAllAvailableHostels());
        }

        [HttpPost]
        public async Task<IActionResult> HostelApplication([FromForm]HostelApplicationViewModel model)
        {
            var result = await _hostelService.Apply(model.HostelID);
            if (result.isSuccess)
            {
                ViewBag.Success = result.Message;
                return View();
            }
            else
            {
                ViewBag.ErrorMsg = result.Message;
                return View();
            }
        }

        
        [HttpGet]
        public IActionResult CreateHostel()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateHostel(Hostel model)
        {
            var result = await _hostelService.CreateHostel(model);
            if (result.isSuccess)
            {
                TempData["Success"] = result.Message;
                return View();
            }
            else
            {
                TempData["Error"] = result.Message;
                return View();
            }
        }
    }
}
