using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeeklyScheduleManagement.Data;
using WeeklyScheduleManagement.Models;

namespace WeeklyScheduleManagement.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DiaDiemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiaDiemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DiaDiem
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var diaDiems = _context.DiaDiems.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                diaDiems = diaDiems.Where(d => d.TenDiaDiem.Contains(searchString) || d.LoaiDiaDiem.Contains(searchString));
            }

            return View(await diaDiems.OrderByDescending(d => d.NgayTao).ToListAsync());
        }

        // GET: DiaDiem/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(m => m.MaDiaDiem == id);

            if (diaDiem == null) return NotFound();

            return View(diaDiem);
        }

        // GET: DiaDiem/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DiaDiem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiaDiem diaDiem)
        {
            if (ModelState.IsValid)
            {
                diaDiem.NgayTao = DateTime.Now;
                diaDiem.TrangThai = true;
                
                _context.Add(diaDiem);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Thêm địa điểm thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(diaDiem);
        }

        // GET: DiaDiem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var diaDiem = await _context.DiaDiems.FindAsync(id);
            if (diaDiem == null) return NotFound();

            return View(diaDiem);
        }

        // POST: DiaDiem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiaDiem diaDiem)
        {
            if (id != diaDiem.MaDiaDiem) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(diaDiem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật địa điểm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiaDiemExists(diaDiem.MaDiaDiem))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(diaDiem);
        }

        // GET: DiaDiem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(m => m.MaDiaDiem == id);

            if (diaDiem == null) return NotFound();

            return View(diaDiem);
        }

        // POST: DiaDiem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diaDiem = await _context.DiaDiems.FindAsync(id);
            if (diaDiem != null)
            {
                // Kiểm tra có lịch tuần đang dùng không
                var hasSchedule = await _context.LichTuans.AnyAsync(l => l.MaDiaDiem == id);
                if (hasSchedule)
                {
                    TempData["ErrorMessage"] = "Không thể xóa địa điểm đang có lịch tuần!";
                    return RedirectToAction(nameof(Index));
                }

                _context.DiaDiems.Remove(diaDiem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa địa điểm thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DiaDiemExists(int id)
        {
            return _context.DiaDiems.Any(e => e.MaDiaDiem == id);
        }
    }
}