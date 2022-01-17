using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HAFD.DTOs;
using HAFD.Models;
using HAFD.Services;
using HAFD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using HAFD.Enums;
using HAFD.Helpers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HAFD.Controllers
{
    public class AuthController : Controller
    {
        private IUserServices _userService;
        private readonly IMapper _mapper;
        private IAzureService _azureService;

        public AuthController(IUserServices userService, IMapper mapper, IAzureService azureService)
        {
            _userService = userService;
            _azureService = azureService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register([FromForm] RegisterUserViewModel model)
        {
            TempUser.NewUser = model;
            return RedirectToAction(nameof(CaptureImage));
        }

        [HttpGet]
        public IActionResult OnboardAdminUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OnboardAdminUser([FromForm] RegisterUserViewModel model)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                Email = model.Email,
                Department = model.Department,
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                UserStatus = UserStatusEnum.Active,
                DateCreated = DateTime.Now,
            };
            var result = await _userService.RegisterUserAsync(model, user, true);
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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            var (result, user, token) = await _userService.LoginUserAsync(model);
            if (result.isSuccess && user != null)
            {
				if (user.UserStatus == Enums.UserStatusEnum.Deactivated)
				{
                    ViewBag.ErrorMsg = "Your account has been deactivated please contact customer support";
                    return View();
                }
				else
				{
                    var loginResponse = _mapper.Map<LoginResponseDTO>(user);
                    loginResponse.UserRoles = await _userService.GetUserRolesAsync(user);
                    loginResponse.token = token;

                    HttpContext.Session.SetString("JWToken", token);
                    return RedirectToAction("Dashboard", "User", loginResponse);
                }
            }
            else
            {
                ViewBag.ErrorMsg = result.Message;
                return View();
            }
        }

        //public IActionResult SaveAndProceed(RegisterUserViewModel model)
        //{
        //    TempData["newUser"] = model;
        //    return RedirectToAction(nameof(CaptureImage));
        //}


        [HttpGet]
        public IActionResult CaptureImage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CaptureImage(string name)
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null)
                {
                    var result = await _azureService.AddPersonAsync(TempUser.NewUser, files);
                    if (result.isSuccess)
                    {
                        return RedirectToAction("Dashboard", "Auth");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = result.Message;
                        TempData["ErrorMsg"] = result.Message;
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "Unable to detect photo";
                    return View();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IActionResult> ConfirmEmail(string userID, string token)
        {
            if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(token))
                return View();
            var result = await _userService.ConfirmEmailAsync(userID, token);
            if (result.isSuccess)
            {
                return View(result);
            }
            return View(result);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var result = await _userService.ForgotPasswordAsync(email);
            if (result.isSuccess)
            {
                ViewBag.Success = result.Message;
                return View(result);
            }
            else
            {
                ViewBag.ErrorMsg = result.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordViewModel model)
        {
            var result = await _userService.ResetPasswordAsync(model);
            if (result.isSuccess)
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                ViewBag.ErrorMsg = result.Message;
                return View();
            }
        }

        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel model)
        {
            var result = await _userService.ChangePasswordAsync(model);
            if (result.isSuccess)
            {
                return View(result);
            }
            return View(result);
        }

        public async Task<IActionResult> DeactivateAccount()
        {
            var result = await _userService.DeactivateAccountAsync();
            if (result.isSuccess)
                return View(result);
            return View(result);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
