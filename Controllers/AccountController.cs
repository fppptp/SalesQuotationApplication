using ACMSModel.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SQTWeb.ViewModels;

namespace SQTWeb.Controllers;

public class AccountController : Controller
{
    private readonly ACMSContext _acmsContext;

    public AccountController(ACMSContext acmsContext)
    {
        _acmsContext = acmsContext;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var username = model.Username?.Trim() ?? string.Empty;
        var user = await _acmsContext.SysUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        // ตรวจสอบสถานะ
        if (user.IsVoid) // ปรับ property ชื่อถ้าใน entity ต่างกัน
        {
            ModelState.AddModelError("", "This account is inactive.");
            return View(model);
        }

        // ตรวจสอบรหัสผ่าน (plain หรือ MD5 ตามฐานข้อมูลเดิม)
        var input = model.Password ?? string.Empty;
        bool ok = (user.Password == input
                  || string.Equals(user.Password, ComputeMd5(input), StringComparison.OrdinalIgnoreCase));

        if (!ok)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("DisplayName", user.Fullname ?? user.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        { return Redirect(model.ReturnUrl); }

        return RedirectToAction("Index", "Home");
    }

    private static string ComputeMd5(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = md5.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        var model = new ViewModels.LoginViewModel { ReturnUrl = returnUrl };
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}