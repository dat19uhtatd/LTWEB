using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;

namespace QLGiaSuLopHoc.Controllers
{
    /// <summary>Khu vực dành cho Phụ huynh / Học sinh: theo dõi các buổi học thử và đánh giá của mình.</summary>
    [RoleAuthorize(Roles.Parent)]
    public class ParentController : BaseController
    {
        private readonly QlGiaSuLopHocContext _context;
        public ParentController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> MyBookings()
        {
            int userId = CurrentUserId!.Value;
            var vm = new ParentDashboardViewModel
            {
                Bookings = await _context.DangKyHocThu
                    .Include(b => b.MaMonHocNavigation)
                    .Include(b => b.MaGiaSuNavigation).ThenInclude(g => g.MaNguoiDungNavigation)
                    .Where(b => b.MaPhuHuynh == userId)
                    .OrderByDescending(b => b.NgayTao)
                    .AsNoTracking()
                    .ToListAsync(),
                Reviews = await _context.DanhGia
                    .Include(d => d.MaGiaSuNavigation).ThenInclude(g => g.MaNguoiDungNavigation)
                    .Where(d => d.MaPhuHuynh == userId)
                    .OrderByDescending(d => d.NgayDanhGia)
                    .AsNoTracking()
                    .ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            int userId = CurrentUserId!.Value;
            var booking = await _context.DangKyHocThu.FirstOrDefaultAsync(b => b.MaDangKy == id && b.MaPhuHuynh == userId);
            if (booking == null) return NotFound();

            if (booking.TrangThai != "Chờ duyệt")
            {
                TempData["ErrorMsg"] = "Chỉ có thể hủy yêu cầu đang ở trạng thái \"Chờ duyệt\".";
            }
            else
            {
                booking.TrangThai = "Đã hủy";
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Đã hủy yêu cầu đăng ký học thử.";
            }
            return RedirectToAction(nameof(MyBookings));
        }
    }
}
