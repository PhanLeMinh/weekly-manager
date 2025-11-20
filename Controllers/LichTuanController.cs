using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WeeklyScheduleManagement.Data;
using WeeklyScheduleManagement.Models;

namespace WeeklyScheduleManagement.Controllers
{
    [Authorize]
    public class LichTuanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichTuanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LichTuan
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            ViewData["CurrentFilter"] = searchString;
            ViewData["TrangThaiFilter"] = trangThai;

            var lichTuans = _context.LichTuans
                .Include(l => l.NguoiDangKy)
                .Include(l => l.ChuTri)
                .Include(l => l.DiaDiem)
                .Include(l => l.NguoiDuyet)
                .AsQueryable();

            // Phân quyền: Giáo viên chỉ thấy lịch của mình
            if (userRole == "GiaoVien")
            {
                lichTuans = lichTuans.Where(l => l.MaNguoiDangKy == userId);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                lichTuans = lichTuans.Where(l => l.TenLichTuan.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                lichTuans = lichTuans.Where(l => l.TrangThai == trangThai);
            }

            var result = await lichTuans.OrderByDescending(l => l.NgayTao).ToListAsync();
            
            Console.WriteLine($"📋 Index: Tổng số lịch = {result.Count}");
            
            return View(result);
        }

        // GET: LichTuan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lichTuan = await _context.LichTuans
                .Include(l => l.NguoiDangKy)
                .Include(l => l.ChuTri)
                .Include(l => l.DiaDiem)
                .Include(l => l.NguoiDuyet)
                .Include(l => l.ThanhPhanThamGias)
                    .ThenInclude(t => t.NguoiDung)
                .FirstOrDefaultAsync(m => m.MaLichTuan == id);

            if (lichTuan == null) return NotFound();

            return View(lichTuan);
        }

        // GET: LichTuan/Create
        public IActionResult Create()
        {
            LoadDropdownData();
            return View();
        }

        // POST: LichTuan/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(LichTuan lichTuan, int[] thanhPhanIds, string[] vaiTros)
{
    Console.WriteLine($"📝 Create POST: {lichTuan.TenLichTuan}");
    Console.WriteLine($"📝 MaChuTri: {lichTuan.MaChuTri}");
    Console.WriteLine($"📝 MaDiaDiem: {lichTuan.MaDiaDiem}");
    Console.WriteLine($"📝 NgayBatDau: {lichTuan.NgayBatDau}");
    Console.WriteLine($"📝 NgayKetThuc: {lichTuan.NgayKetThuc}");

    // KIỂM TRA ModelState
    if (!ModelState.IsValid)
    {
        Console.WriteLine("❌ ModelState INVALID:");
        foreach (var modelState in ModelState.Values)
        {
            foreach (var error in modelState.Errors)
            {
                Console.WriteLine($"   - {error.ErrorMessage}");
            }
        }
        LoadDropdownData();
        return View(lichTuan);
    }

    try
    {
        // Kiểm tra trùng lịch địa điểm
        var conflictSchedule = await _context.LichTuans
            .Where(l => l.MaDiaDiem == lichTuan.MaDiaDiem
                && l.TrangThai != "TuChoi"
                && ((l.NgayBatDau <= lichTuan.NgayBatDau && l.NgayKetThuc >= lichTuan.NgayBatDau)
                    || (l.NgayBatDau <= lichTuan.NgayKetThuc && l.NgayKetThuc >= lichTuan.NgayKetThuc)
                    || (l.NgayBatDau >= lichTuan.NgayBatDau && l.NgayKetThuc <= lichTuan.NgayKetThuc)))
            .FirstOrDefaultAsync();

        if (conflictSchedule != null)
        {
            ModelState.AddModelError("MaDiaDiem", "Địa điểm đã có lịch trùng trong khoảng thời gian này!");
            LoadDropdownData();
            return View(lichTuan);
        }

        // Lấy user hiện tại
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        // Set các giá trị
        lichTuan.MaNguoiDangKy = userId;
        lichTuan.TrangThai = "ChoDuyet";
        lichTuan.LyDoTuChoi = ""; // ĐẶT GIÁ TRỊ RỖNG thay vì null
        lichTuan.NgayTao = DateTime.Now;
        lichTuan.NgayCapNhat = DateTime.Now;
        lichTuan.MaNguoiDuyet = null;
        lichTuan.NgayDuyet = null;

        Console.WriteLine("💾 Đang lưu lịch tuần...");
        _context.Add(lichTuan);
        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ Đã lưu lịch ID: {lichTuan.MaLichTuan}");

        // Thêm thành phần tham gia
        if (thanhPhanIds != null && thanhPhanIds.Length > 0)
        {
            Console.WriteLine($"👥 Đang thêm {thanhPhanIds.Length} thành viên...");
            
            for (int i = 0; i < thanhPhanIds.Length; i++)
            {
                if (thanhPhanIds[i] > 0) // Chỉ thêm nếu có chọn người dùng
                {
                    var thanhPhan = new ThanhPhanThamGia
                    {
                        MaLichTuan = lichTuan.MaLichTuan,
                        MaNguoiDung = thanhPhanIds[i],
                        VaiTro = vaiTros != null && i < vaiTros.Length ? vaiTros[i] : "Tham gia",
                        GhiChu = "", // ĐẶT GIÁ TRỊ RỖNG
                        NgayTao = DateTime.Now
                    };
                    _context.ThanhPhanThamGias.Add(thanhPhan);
                }
            }
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Đã thêm thành viên");
        }

        TempData["SuccessMessage"] = "Đăng ký lịch tuần thành công! Chờ Manager duyệt.";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ LỖI: {ex.Message}");
        Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
        }
        
        ModelState.AddModelError("", $"Lỗi khi lưu: {ex.Message}");
        LoadDropdownData();
        return View(lichTuan);
    }
}

        // GET: LichTuan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lichTuan = await _context.LichTuans.FindAsync(id);
            if (lichTuan == null) return NotFound();

            // Chỉ cho phép sửa khi chờ duyệt
            if (lichTuan.TrangThai != "ChoDuyet")
            {
                TempData["ErrorMessage"] = "Chỉ có thể chỉnh sửa lịch đang chờ duyệt!";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdownData();
            return View(lichTuan);
        }

        // POST: LichTuan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichTuan lichTuan)
        {
            if (id != lichTuan.MaLichTuan) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    lichTuan.NgayCapNhat = DateTime.Now;
                    _context.Update(lichTuan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật lịch tuần thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LichTuanExists(lichTuan.MaLichTuan))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            LoadDropdownData();
            return View(lichTuan);
        }

        // GET: LichTuan/Approve/5 (Chỉ Manager)
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null) return NotFound();

            var lichTuan = await _context.LichTuans
                .Include(l => l.NguoiDangKy)
                .Include(l => l.ChuTri)
                .Include(l => l.DiaDiem)
                .Include(l => l.ThanhPhanThamGias)
                    .ThenInclude(t => t.NguoiDung)
                .FirstOrDefaultAsync(m => m.MaLichTuan == id);

            if (lichTuan == null) return NotFound();

            if (lichTuan.TrangThai != "ChoDuyet")
            {
                TempData["ErrorMessage"] = "Lịch này không ở trạng thái chờ duyệt!";
                return RedirectToAction(nameof(Index));
            }

            return View(lichTuan);
        }

        // POST: LichTuan/ApproveSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveSchedule(int id)
        {
            var lichTuan = await _context.LichTuans.FindAsync(id);
            if (lichTuan == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            lichTuan.TrangThai = "DaDuyet";
            lichTuan.MaNguoiDuyet = userId;
            lichTuan.NgayDuyet = DateTime.Now;
            lichTuan.NgayCapNhat = DateTime.Now;

            _context.Update(lichTuan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Duyệt lịch thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: LichTuan/RejectSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> RejectSchedule(int id, string lyDoTuChoi)
        {
            var lichTuan = await _context.LichTuans.FindAsync(id);
            if (lichTuan == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            lichTuan.TrangThai = "TuChoi";
            lichTuan.MaNguoiDuyet = userId;
            lichTuan.NgayDuyet = DateTime.Now;
            lichTuan.LyDoTuChoi = lyDoTuChoi;
            lichTuan.NgayCapNhat = DateTime.Now;

            _context.Update(lichTuan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Từ chối lịch thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: LichTuan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lichTuan = await _context.LichTuans
                .Include(l => l.NguoiDangKy)
                .Include(l => l.DiaDiem)
                .FirstOrDefaultAsync(m => m.MaLichTuan == id);

            if (lichTuan == null) return NotFound();

            return View(lichTuan);
        }

        // POST: LichTuan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichTuan = await _context.LichTuans.FindAsync(id);
            if (lichTuan != null)
            {
                _context.LichTuans.Remove(lichTuan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa lịch tuần thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdownData()
        {
            ViewBag.NguoiDungs = new SelectList(_context.NguoiDungs.Where(n => n.TrangThai), "MaNguoiDung", "HoTen");
            ViewBag.DiaDiems = new SelectList(_context.DiaDiems.Where(d => d.TrangThai), "MaDiaDiem", "TenDiaDiem");
        }

        private bool LichTuanExists(int id)
        {
            return _context.LichTuans.Any(e => e.MaLichTuan == id);
        }
    }
}