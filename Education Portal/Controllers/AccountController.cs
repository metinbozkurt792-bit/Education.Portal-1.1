using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Education_Portal.Models;
using Education_Portal.Repositories;

namespace Education_Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        public AccountController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Course");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _userRepository.GetByEmailAndPassword(email, password);
            if (user != null)
            {
                if (user.BanEndDate != null && user.BanEndDate > DateTime.Now)
                {
                    ViewBag.Error = $"Hesabınız {user.BanEndDate?.ToString("dd.MM.yyyy HH:mm")} tarihine kadar askıya alınmıştır.";
                    return View();
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                };
                await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Course");
            }
            ViewBag.Error = "Email veya şifre hatalı!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Course");
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Course");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(string fullName, string email, string password)
        {
            if (!email.EndsWith("@user.com"))
            {
                ViewBag.Error = "Sadece '@user.com' uzantılı mail adresleri kayıt olabilir!";
                return View();
            }
            if (_userRepository.IsEmailExists(email))
            {
                ViewBag.Error = "Bu email adresi zaten kayıtlı!";
                return View();
            }
            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                Password = password,
                Role = "User"
            };
            _userRepository.Add(newUser);
            return RedirectToAction("Login");
        }
    }
}