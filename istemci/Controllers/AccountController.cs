using chat_site_istemci.Models;
using chat_site_istemci.Entities;
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

            public AccountController(DatabaseContext databaseContext, IConfiguration configuration)
            {
                _databaseContext = databaseContext;
                _configuration = configuration;
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

                        List<Claim> claims = new List<Claim>();//Claims are stored in secure locations such as the Identity object, JWT tokens, or session/cookies. They are primarily used for authentication (verifying the user's identity) and authorization (determining what actions or resources the user is allowed to access). Claims provide a flexible and secure way to store and transfer user-specific information, which can then be utilized by the system for decision-making during user requests.
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
                        claims.Add(new Claim("Username", user.Username));

                        ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        return RedirectToAction("Index", "Chat");
                    }

                    else
                    {
                        ModelState.AddModelError("", "Username or password is incorrect.");
                    }

                }
                return View(model);
            }

            private string DoMD5HashedString(string s)
            {
                string md5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
                string salted = s + md5Salt;
                string hashed = salted.MD5();
                return hashed;
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
                    User user = new()
                    {
                        Username = model.Username,
                        Password = hashedPassword
                    };
                    _databaseContext.Users.Add(user);
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

            public IActionResult Profile()
            {
                ProfileInfoLoader();

                return View();
            }

            private void ProfileInfoLoader()
            {
                Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = _databaseContext.Users.SingleOrDefault(x => x.UserId == userid);

                ViewData["ProfileImage"] = user.ProfileImageFileName;
            }

            [HttpPost]

            public IActionResult ProfileChangeFullName([Required][StringLength(50)] string? fullname)
            {
                if (ModelState.IsValid)
                {
                    Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    User user = _databaseContext.Users.SingleOrDefault(x => x.UserId == userid);
                    _databaseContext.SaveChanges();

                    return RedirectToAction(nameof(Profile));
                }
                ProfileInfoLoader();
                return View("Profile");
            }


            public IActionResult ProfileChangePassword([Required][MinLength(6)][MaxLength(16)] string? password)
            {
                if (ModelState.IsValid)
                {
                    Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    User user = _databaseContext.Users.SingleOrDefault(x => x.UserId == userid);
                    string hashedPassword = DoMD5HashedString(password);
                    user.Password = hashedPassword;
                    _databaseContext.SaveChanges();

                    ViewData["result"] = "PasswordChanged";
                }
                ProfileInfoLoader();
                return View("Profile");
            }


            [HttpPost]
            public IActionResult ProfileChangeImage([Required] IFormFile file)
            {
                if (file == null || file.Length == 0)
                {
                    ModelState.AddModelError("file", "Please select a valid file.");
                    return View("Profile");
                }

                if (ModelState.IsValid)
                {
                    // Fetch the current user
                    Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    User user = _databaseContext.Users.SingleOrDefault(x => x.UserId == userid);

                    // Define the file name and the path where the image will be saved
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder); // Create the folder if it doesn't exist
                    }

                    // Generate the unique file name
                    string fileName = $"p_{userid}{Path.GetExtension(file.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    //string fileName = $"p_{userid}{file.ContentType.Split('/')[1]}";  //  img/png        image/jpg

                    // Save the uploaded file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    // Update the user's profile image filename in the database
                    user.ProfileImageFileName = fileName;
                    _databaseContext.SaveChanges();

                    // Set the new file name to ViewData to display in the view
                    ProfileInfoLoader();
                    return RedirectToAction(nameof(Profile));
                }

                return View("Profile");
            }


            public IActionResult Logout()
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _databaseContext.Users.SingleOrDefault(x => x.UserId == new Guid(userId));
                if (user != null)
                {
                    user.IsOnline = false;
                    user.CreatedAt = DateTime.Now;
                    _databaseContext.SaveChanges(); 
                }
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }


        }

    }

