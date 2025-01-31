﻿using chat_site_istemci.Models;
using chat_site_istemci.Entities;
using chat_site_istemci.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using NETCore.Encrypt.Extensions;

namespace chat_site_istemci.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;
        private readonly SocketService _socketService;
        private Chats _chats;

        public AccountController(DatabaseContext databaseContext, IConfiguration configuration, SocketService socketService,Chats chats)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
            _socketService = socketService;
            _chats = chats;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = DoMD5HashedString(model.Password);

                User user = _databaseContext.Users.SingleOrDefault(x => x.Username.ToLower() == model.Username.ToLower() && x.Password == hashedPassword);

                if (user != null)
                {
                    user.IsOnline = true;
                    _databaseContext.SaveChanges();

                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim("Username", user.Username)
                    };

                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    _socketService.ConnectToServer(user.UserId.ToString());

                    return RedirectToAction("Index", "Chat");
                }
                else
                {
                    ModelState.AddModelError("", "Username or password is incorrect.");
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (_databaseContext.Users.Any(x => x.Username.ToLower() == model.Username.ToLower()))
            {
                ModelState.AddModelError(nameof(model.Username), "Username is already exists.");
                return View(model);
            }

            string hashedPassword = DoMD5HashedString(model.Password);
            if (ModelState.IsValid)
            {
                string profileImageFileName = "/images/default.jpg"; 
                if (model.ProfileImage != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    profileImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, profileImageFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfileImage.CopyTo(fileStream);
                    }
                }

                User user = new()
                {
                    Username = model.Username,
                    Password = hashedPassword,
                    ProfileImageFileName = profileImageFileName
                };
                _databaseContext.Users.Add(user);
                var newMember = new GroupMember
                {
                    GroupMemberId = Guid.NewGuid(),
                    GroupId = Guid.Parse("c977d8ac-691c-41b3-8935-dcf2b0f68c86"),
                    UserId = user.UserId
                };
                _databaseContext.GroupMembers.AddAsync(newMember);

                int affectedRowCount = _databaseContext.SaveChanges();

                if (affectedRowCount == 0)
                {
                    ModelState.AddModelError("", "User cannot be added.");
                }
                else
                {
                    return RedirectToAction(nameof(Login));
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _databaseContext.Users.SingleOrDefault(x => x.UserId == new Guid(userId));
            _chats.chats.Clear();

            if (user != null)
            {
                user.IsOnline = false;
                _databaseContext.SaveChanges();

                // إيقاف الاتصال بالخادم
                _socketService.Disconnect();
            }

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private string DoMD5HashedString(string s)
        {
            string md5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
            string salted = s + md5Salt;
            return salted.MD5();
        }
    }
}
