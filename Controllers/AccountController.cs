using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WeeklyScheduleManagement.Data;
using WeeklyScheduleManagement.Models;

namespace WeeklyScheduleManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _context.NguoiDungs
                    .Include(u => u.VaiTro)
                    .Include(u => u.PhongBan)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.MatKhau == model.MatKhau && u.TrangThai);

                if (user != null)
                {
                    // Tạo claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.VaiTro.TenVaiTro),
                        new Claim("MaVaiTro", user.MaVaiTro.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    Console.WriteLine($"✅ Đăng nhập thành công: {user.HoTen} ({user.VaiTro.TenVaiTro})");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng!");
            }

            return View(model);
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký!");
                    return View(model);
                }

                // Tạo user mới (mặc định là Giáo viên)
                var newUser = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    MatKhau = model.MatKhau, // Trong thực tế nên hash password
                    SoDienThoai = model.SoDienThoai,
                    MaVaiTro = 3, // GiaoVien
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };

                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Đăng ký thành công: {newUser.HoTen}");

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Console.WriteLine("✅ Đăng xuất thành công");
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}