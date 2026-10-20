using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Controllers;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;

namespace QLGiaSuLopHoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RoleAuthorize(Roles.Admin)]
    public class TutorsController : BaseController
    {
        private const int PageSize = 8;
        private readonly QlGiaSuLopHocContext _context;
        public TutorsController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> Index(string? q, string status = "all", int page = 1)
        {
            var query = _context.HoSoGiaSu.Include(t => t.MaNguoiDungNavigation).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(t => t.MaNguoiDungNavigation.HoTen.Contains(q) || t.MaNguoiDungNavigation.Email.Contains(q));
            }
            query = status switch
            {
                "verified" => query.Where(t => t.DaXacMinh == true),
                "pending" => query.Where(t => t.DaXacMinh != true),
                "locked" => query.Where(t => t.MaNguoiDungNavigation.TrangThai == false),
                _ => query
            };

            int total = await query.CountAsync();
            var result = new PagedResult<HoSoGiaSu>
            {
                Page = Math.Max(1, page),
                PageSize = PageSize,
                TotalItems = total
            };
            result.Page = Math.Min(result.Page, result.TotalPages);
            result.Items = await query
                .OrderBy(t => t.DaXacMinh == true ? 1 : 0).ThenBy(t => t.MaGiaSu)
                .Skip((result.Page - 1) * PageSize).Take(PageSize)
                .AsNoTracking().ToListAsync();

            ViewBag.Q = q;
            ViewBag.Status = status;
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVerify(int id, string? returnUrl)
        {
            var tutor = await _context.HoSoGiaSu.FindAsync(id);
            if (tutor == null) return NotFound();

            tutor.DaXacMinh = !(tutor.DaXacMinh ?? false);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = tutor.DaXacMinh == true
                ? "Đã cấp Tích xanh xác minh cho gia sư."
                : "Đã thu hồi Tích xanh của gia sư.";
            return RedirectToLocalOr(returnUrl, nameof(Index), "Tutors", new { area = "Admin" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var tutor = await _context.HoSoGiaSu.Include(t => t.MaNguoiDungNavigation).FirstOrDefaultAsync(t => t.MaGiaSu == id);
            if (tutor == null) return NotFound();

            var user = tutor.MaNguoiDungNavigation;
            user.TrangThai = !(user.TrangThai ?? true);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = user.TrangThai == true
                ? $"Đã mở khóa tài khoản {user.HoTen}."
                : $"Đã khóa tài khoản {user.HoTen}. Gia sư sẽ không hiển thị trên trang tìm kiếm.";
            return RedirectToAction(nameof(Index));
        }
    }
}
