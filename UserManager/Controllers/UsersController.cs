using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManager.Repositories;
using UserManager.Models;

namespace UserManager.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Block([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest();

            foreach (var id in ids)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    user.Status = UserStatus.Blocked;
                    await _userRepository.UpdateAsync(user);
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Unblock([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest();

            foreach (var id in ids)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user != null && user.Status == UserStatus.Blocked)
                {
                    user.Status = UserStatus.Active;
                    await _userRepository.UpdateAsync(user);
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest();

            foreach (var id in ids)
            {
                await _userRepository.DeleteAsync(id);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnverified([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest();

            foreach (var id in ids)
            {
                var user = await _userRepository.GetByIdAsync(id);
                // Удаляем только если у пользователя статус Unverified
                if (user != null && user.Status == UserStatus.Unverified)
                {
                    await _userRepository.DeleteAsync(id);
                }
            }

            return Ok();
        }
    }
}