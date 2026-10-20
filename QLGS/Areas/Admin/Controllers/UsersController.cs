using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Controllers;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;
using QLGiaSuLopHoc.Services;

namespace QLGiaSuLopHoc.Areas.Admin.Controllers
{
    /// <summary>Quản lý tài khoản người dùng: tìm kiếm, khóa/mở khóa, đặt lại mật khẩu.</summary>
    [Area("Admin")]
    [RoleAuthorize(Roles.Admin)]
    public class UsersController : BaseController
    {
        private const int PageSize = 10;
        private readonly QlGiaSuLopHocContext _context;
        public UsersController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> Index(string? q, string role = "all", int page = 1)
        {
            var query = _context.NguoiDung.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u => u.HoTen.Contains(q) || u.Email.Contains(q) || u.SoDienThoai.Contains(q));
            }
            query = role switch
            {
                "Admin" => query.Where(u => u.VaiTro == Roles.Admin),
                "Tutor" => query.Where(u => u.VaiTro == Roles.Tutor),
                "Parent" => query.Where(u => u.VaiTro != Roles.Admin && u.VaiTro != Roles.Tutor),
                _ => query
            };

            int total = await query.CountAsync();
            var result = new PagedResult<NguoiDung> { Page = Math.Max(1, page), PageSize = PageSize, TotalItems = total };
            result.Page = Math.Min(result.Page, result.TotalPages);
            result.Items = await query.OrderBy(u => u.MaNguoiDung)
                .Skip((result.Page - 1) * PageSize).Take(PageSize)
                .AsNoTracking().ToListAsync();

            ViewBag.Q = q;
            ViewBag.Role = role;
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.NguoiDung.FindAsync(id);
            if (user == null) return NotFound();

            if (user.MaNguoiDung == CurrentUserId || Roles.Normalize(user.VaiTro) == Roles.Admin)
            {
                TempData["ErrorMsg"] = "Không thể khóa tài khoản Quản trị viên.";
                return RedirectToAction(nameof(Index));
            }

            user.TrangThai = !(user.TrangThai ?? true);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = user.TrangThai == true ? $"Đã mở khóa tài khoản {user.HoTen}." : $"Đã khóa tài khoản {user.HoTen}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.NguoiDung.FindAsync(id);
            if (user == null) return NotFound();

            string temp = PasswordService.GenerateTemporary();
            user.MatKhauHash = PasswordService.Hash(temp);
            await _context.SaveChangesAsync();

            TempData["InfoMsg"] = $"Mật khẩu tạm thời của {user.Email} là: {temp} — hãy gửi cho người dùng và yêu cầu đổi mật khẩu sau khi đăng nhập.";
            return RedirectToAction(nameof(Index));
        }
    }
}
