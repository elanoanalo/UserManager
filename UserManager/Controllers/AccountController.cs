using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManager.Models;
using UserManager.Repositories;
using UserManager.ViewModels;

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
            return View();
        }
    }
}