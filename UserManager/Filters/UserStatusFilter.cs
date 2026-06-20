using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserManager.Models;
using UserManager.Repositories;

namespace UserManager.Filters
{
    public class UserStatusFilter : IAsyncActionFilter
    {
        private readonly IUserRepository _userRepository;

        public UserStatusFilter(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var user = await _userRepository.GetByIdAsync(userId);

                    if (user == null || user.Status == UserStatus.Blocked)
                    {
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                        var message = user == null
                            ? "Ваш аккаунт был удален администратором."
                            : "Ваш аккаунт был заблокирован.";

                        if (context.Controller is Controller controller)
                        {
                            controller.TempData["ErrorMessage"] = message;
                        }

                        context.Result = new RedirectToActionResult("Login", "Account", null);
                        return;
                    }
                }
            }

            await next();
        }
    }
}