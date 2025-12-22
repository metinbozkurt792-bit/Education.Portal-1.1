using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Education_Portal.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Education_Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                if (user.BanEndDate != null && user.BanEndDate > DateTime.Now)
                {
                    ViewBag.Error = $"Hesabınız {user.BanEndDate?.ToString("dd.MM.yyyy HH:mm")} tarihine kadar askıya alınmıştır.";
                    return View();
                }
                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return user.Role == "Admin"
                        ? RedirectToAction("Index", "Admin")
                        : RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Error = "Email veya şifre hatalı!";
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            if (!email.EndsWith("@user.com"))
            {
                ViewBag.Error = "Sadece '@user.com' uzantılı mail adresleri kayıt olabilir!";
                return View();
            }
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ViewBag.Error = "Bu email adresi zaten kayıtlı!";
                return View();
            }
            var newUser = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Role = "User"
            };
            var result = await _userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "User");

                TempData["SuccessMessage"] = "Kayıt işleminiz başarılı! Lütfen giriş yapınız.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = result.Errors.FirstOrDefault()?.Description ?? "Kayıt sırasında bir hata oluştu.";
            return View();
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}