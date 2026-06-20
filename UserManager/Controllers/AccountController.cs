using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManager.Models;
using UserManager.Repositories;
using UserManager.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using UserManager.Services;

namespace UserManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public AccountController(IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
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
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = new User
                {
                    Name = model.Name.Trim(),
                    Email = model.Email.ToLower().Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Status = UserStatus.Unverified,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);

                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id },
                    protocol: HttpContext.Request.Scheme) ?? string.Empty;

                _ = _emailService.SendConfirmationEmailAsync(user.Email, confirmationLink);

                TempData["SuccessMessage"] = "Регистрация успешна! На вашу почту отправлено письмо для подтверждения аккаунта.";

                return RedirectToAction("Login");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
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

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            if (user.Status == UserStatus.Unverified)
            {
                user.Status = UserStatus.Active; 
                await _userRepository.UpdateAsync(user);
                TempData["SuccessMessage"] = "Электронная почта успешно подтверждена! Теперь вы можете войти.";
            }
            else if (user.Status == UserStatus.Blocked)
            {
                TempData["ErrorMessage"] = "Этот аккаунт заблокирован администратором. Подтверждение невозможно.";
            }
            else if (user.Status == UserStatus.Active)
            {
                TempData["SuccessMessage"] = "Ваш Email уже был подтвержден ранее.";
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}