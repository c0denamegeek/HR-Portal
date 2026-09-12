using HR_Portal.Constants;
using HR_Portal.Models;
using HR_Portal.Models.Domain;
using HR_Portal.ViewModel.AccountViewModels;
using HR_Portal.ViewModel.LoginViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR_Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ── Login ─────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(vm);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact HR.");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (user.MustChangePassword)
                    return RedirectToAction(nameof(ChangePassword), new { firstLogin = true });

                if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                    return RedirectToAction("Dashboard", "AdminDashboard");

                return RedirectToAction("Dashboard", "Leave");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(vm);
        }

        // ── Logout ────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // ── Change Password ───────────────────────────────────────────────

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword(bool firstLogin = false)
        {
            ViewBag.FirstLogin = firstLogin;
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm, bool firstLogin = false)
        {
            ViewBag.FirstLogin = firstLogin;

            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            var result = await _userManager.ChangePasswordAsync(
                user, vm.CurrentPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            if (user.MustChangePassword)
            {
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Password changed successfully.";

            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                return RedirectToAction("Dashboard", "AdminDashboard");

            return RedirectToAction("Dashboard", "Leave");
        }

        // ── Reset Password (Admin only) ───────────────────────────────────

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            return View(new ResetPasswordViewModel
            {
                UserId = user.Id,
                FullName = user.FullName
            });
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user is null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Password reset for {user.FullName}. They will be prompted to change it on next login.";
            return RedirectToAction("Users", "Users");
        }
    }
}