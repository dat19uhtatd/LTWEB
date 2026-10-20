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
    public class BookingsController : BaseController
    {
        private const int PageSize = 10;
        private readonly QlGiaSuLopHocContext _context;
        public BookingsController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> Index(string? q, string status = "all", int page = 1)
        {
            var query = _context.DangKyHocThu
                .Include(b => b.MaGiaSuNavigation).ThenInclude(g => g.MaNguoiDungNavigation)
                .Include(b => b.MaMonHocNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(b => b.HoTenPhuHuynh.Contains(q) || b.SoDienThoai.Contains(q)
                                         || b.MaGiaSuNavigation.MaNguoiDungNavigation.HoTen.Contains(q));
            }
            if (status != "all") query = query.Where(b => b.TrangThai == status);

            int total = await query.CountAsync();
            var result = new PagedResult<DangKyHocThu> { Page = Math.Max(1, page), PageSize = PageSize, TotalItems = total };
            result.Page = Math.Min(result.Page, result.TotalPages);
            result.Items = await query.OrderByDescending(b => b.NgayTao)
                .Skip((result.Page - 1) * PageSize).Take(PageSize)
                .AsNoTracking().ToListAsync();

            ViewBag.Q = q;
            ViewBag.Status = status;
            return View(result);
        }

        [HttpPost]
        public Task<IActionResult> Approve(int id, string? returnUrl) => Change(id, "Đã xác nhận", "Đã duyệt yêu cầu đăng ký học thử!", returnUrl);

        [HttpPost]
        public Task<IActionResult> Reject(int id, string? returnUrl) => Change(id, "Từ chối", "Đã từ chối yêu cầu đăng ký học thử.", returnUrl);

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.DangKyHocThu.FindAsync(id);
            if (booking == null) return NotFound();
            _context.DangKyHocThu.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã xóa yêu cầu đăng ký học thử.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> Change(int id, string newStatus, string message, string? returnUrl)
        {
            var booking = await _context.DangKyHocThu.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.TrangThai != "Chờ duyệt")
            {
                TempData["ErrorMsg"] = "Yêu cầu này đã được xử lý trước đó.";
            }
            else
            {
                booking.TrangThai = newStatus;
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = message;
            }
            return RedirectToLocalOr(returnUrl, nameof(Index), "Bookings", new { area = "Admin" });
        }
    }
}
