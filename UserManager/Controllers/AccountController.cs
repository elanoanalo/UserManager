using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManager.Models;
using UserManager.Repositories;
using UserManager.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UserManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                Name = model.Name,
                Email = model.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                Status = UserStatus.Unverified,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _userRepository.AddAsync(user);

                TempData["SuccessMessage"] = "Регистрация успешна! Ваш статус: Unverified.";
                return RedirectToAction("Login");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Email", "Пользователь с таким Email уже зарегистрирован.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Users");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userRepository.GetByEmailAsync(model.Email.ToLower().Trim());

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Неверный Email или пароль.");
                return View(model);
            }

            if (user.Status == UserStatus.Blocked)
            {
                ModelState.AddModelError("", "Ваш аккаунт заблокирован администратором.");
                return View(model);
            }

            user.LastLogin = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Users");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}