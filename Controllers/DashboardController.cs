using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WeeklyScheduleManagement.Data;

namespace WeeklyScheduleManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Thống kê chung
            ViewBag.TotalUsers = await _context.NguoiDungs.CountAsync();
            ViewBag.TotalDiaDiem = await _context.DiaDiems.Where(d => d.TrangThai).CountAsync();
            
            if (userRole == "Manager" || userRole == "Admin")
            {
                // Manager/Admin thấy tất cả lịch
                ViewBag.TotalLichTuan = await _context.LichTuans.CountAsync();
                ViewBag.LichChoDuyet = await _context.LichTuans.Where(l => l.TrangThai == "ChoDuyet").CountAsync();
                ViewBag.LichDaDuyet = await _context.LichTuans.Where(l => l.TrangThai == "DaDuyet").CountAsync();
                ViewBag.LichTuChoi = await _context.LichTuans.Where(l => l.TrangThai == "TuChoi").CountAsync();
            }
            else
            {
                // Giáo viên chỉ thấy lịch của mình
                ViewBag.TotalLichTuan = await _context.LichTuans.Where(l => l.MaNguoiDangKy == userId).CountAsync();
                ViewBag.LichChoDuyet = await _context.LichTuans.Where(l => l.MaNguoiDangKy == userId && l.TrangThai == "ChoDuyet").CountAsync();
                ViewBag.LichDaDuyet = await _context.LichTuans.Where(l => l.MaNguoiDangKy == userId && l.TrangThai == "DaDuyet").CountAsync();
                ViewBag.LichTuChoi = await _context.LichTuans.Where(l => l.MaNguoiDangKy == userId && l.TrangThai == "TuChoi").CountAsync();
            }

            // Lịch sắp diễn ra (7 ngày tới)
            var upcomingSchedules = await _context.LichTuans
                .Include(l => l.NguoiDangKy)
                .Include(l => l.DiaDiem)
                .Where(l => l.NgayBatDau >= DateTime.Now && l.NgayBatDau <= DateTime.Now.AddDays(7))
                .OrderBy(l => l.NgayBatDau)
                .Take(5)
                .ToListAsync();

            ViewBag.UpcomingSchedules = upcomingSchedules;

            return View();
        }
    }
}