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
    public class HomeAdminController : BaseController
    {
        private readonly QlGiaSuLopHocContext _context;
        public HomeAdminController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var visible = _context.DanhGia.Where(d => d.HienThi == true);
            int reviewCount = await visible.CountAsync();

            var statusRows = await _context.DangKyHocThu
                .GroupBy(b => b.TrangThai)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var subjectRows = await _context.GiaSuMonHoc
                .GroupBy(g => g.MaMonHocNavigation.TenMonHoc)
                .Select(g => new { Name = g.Key, Count = g.Select(x => x.MaGiaSu).Distinct().Count() })
                .ToListAsync();

            var since = DateTime.Today.AddDays(-6);
            var recentDates = await _context.DangKyHocThu
                .Where(b => b.NgayTao >= since)
                .Select(b => b.NgayTao!.Value)
                .ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalTutors = await _context.HoSoGiaSu.CountAsync(),
                VerifiedTutors = await _context.HoSoGiaSu.CountAsync(t => t.DaXacMinh == true),
                PendingTutors = await _context.HoSoGiaSu.CountAsync(t => t.DaXacMinh != true),
                TotalParents = await _context.NguoiDung.CountAsync(u => u.VaiTro != Roles.Admin && u.VaiTro != Roles.Tutor),
                TotalClasses = await _context.LopHocThoiKhoaBieu.CountAsync(),
                TotalBookings = await _context.DangKyHocThu.CountAsync(),
                PendingBookings = await _context.DangKyHocThu.CountAsync(b => b.TrangThai == "Chờ duyệt"),
                TotalReviews = reviewCount,
                AverageRating = reviewCount > 0 ? Math.Round(await visible.AverageAsync(d => (double)d.SoSao), 1) : 0,
                WeeklyRevenue = await _context.LopHocThoiKhoaBieu.Where(l => l.TrangThai == "Đang học").SumAsync(l => l.HocPhiCaDay),
                BookingsByStatus = statusRows.ToDictionary(r => r.Status ?? "Khác", r => r.Count),
                TutorsBySubject = subjectRows.OrderByDescending(r => r.Count).ToDictionary(r => r.Name, r => r.Count),
                TutorsToVerify = await _context.HoSoGiaSu
                    .Include(t => t.MaNguoiDungNavigation)
                    .Where(t => t.DaXacMinh != true)
                    .OrderByDescending(t => t.AnhBangCapTheSv != null)
                    .Take(5).AsNoTracking().ToListAsync(),
                RecentBookings = await _context.DangKyHocThu
                    .Include(b => b.MaGiaSuNavigation).ThenInclude(g => g.MaNguoiDungNavigation)
                    .Include(b => b.MaMonHocNavigation)
                    .OrderByDescending(b => b.NgayTao).Take(6).AsNoTracking().ToListAsync()
            };

            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                vm.LastDaysLabels.Add(day.ToString("dd/MM"));
                vm.LastDaysCounts.Add(recentDates.Count(d => d.Date == day));
            }
            return View(vm);
        }
    }
}
