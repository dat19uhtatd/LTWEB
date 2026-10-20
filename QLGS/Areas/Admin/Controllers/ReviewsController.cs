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
    /// <summary>Kiểm duyệt đánh giá: ẩn/hiện hoặc xóa nhận xét không phù hợp.</summary>
    [Area("Admin")]
    [RoleAuthorize(Roles.Admin)]
    public class ReviewsController : BaseController
    {
        private const int PageSize = 10;
        private readonly QlGiaSuLopHocContext _context;
        public ReviewsController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> Index(string status = "all", int page = 1)
        {
            var query = _context.DanhGia
                .Include(d => d.MaGiaSuNavigation).ThenInclude(g => g.MaNguoiDungNavigation)
                .Include(d => d.MaPhuHuynhNavigation)
                .AsQueryable();
            if (status == "visible") query = query.Where(d => d.HienThi == true);
            else if (status == "hidden") query = query.Where(d => d.HienThi != true);

            int total = await query.CountAsync();
            var result = new PagedResult<DanhGia> { Page = Math.Max(1, page), PageSize = PageSize, TotalItems = total };
            result.Page = Math.Min(result.Page, result.TotalPages);
            result.Items = await query.OrderByDescending(d => d.NgayDanhGia)
                .Skip((result.Page - 1) * PageSize).Take(PageSize)
                .AsNoTracking().ToListAsync();

            ViewBag.Status = status;
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var review = await _context.DanhGia.FindAsync(id);
            if (review == null) return NotFound();

            review.HienThi = !(review.HienThi ?? true);
            await _context.SaveChangesAsync();
            await _context.RecalculateRatingAsync(review.MaGiaSu);

            TempData["SuccessMsg"] = review.HienThi == true ? "Đã hiển thị lại đánh giá." : "Đã ẩn đánh giá khỏi trang gia sư.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.DanhGia.FindAsync(id);
            if (review == null) return NotFound();

            int tutorId = review.MaGiaSu;
            _context.DanhGia.Remove(review);
            await _context.SaveChangesAsync();
            await _context.RecalculateRatingAsync(tutorId);

            TempData["SuccessMsg"] = "Đã xóa đánh giá.";
            return RedirectToAction(nameof(Index));
        }
    }
}
