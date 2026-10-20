using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;

namespace QLGiaSuLopHoc.Controllers.Api
{
    /// <summary>Web API (RESTful) phục vụ trang tìm kiếm gia sư và các thao tác AJAX của Gia sư.</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTutorController : ControllerBase
    {
        private readonly QlGiaSuLopHocContext _context;
        public ApiTutorController(QlGiaSuLopHocContext context) => _context = context;

        /// <summary>
        /// GET api/ApiTutor/filter — lọc, tìm kiếm, sắp xếp và phân trang gia sư.
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> FilterTutors(
            string? q, int? maMon, int? maKhoi, string? khuVuc, string? mucGia,
            string? trinhDo, bool daXacMinh = false, string? sortBy = "rating_desc",
            int page = 1, int pageSize = 6)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 24);

            // Chỉ hiển thị gia sư có tài khoản đang hoạt động
            var query = _context.HoSoGiaSu
                .AsNoTracking()
                .Where(t => t.MaNguoiDungNavigation.TrangThai != false);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(t => t.MaNguoiDungNavigation.HoTen.Contains(q)
                                         || (t.TruongDaoTao != null && t.TruongDaoTao.Contains(q))
                                         || (t.ThanhTichNoiBat != null && t.ThanhTichNoiBat.Contains(q))
                                         || t.GiaSuMonHoc.Any(m => m.MaMonHocNavigation.TenMonHoc.Contains(q)));
            }

            if (maMon is > 0)
                query = query.Where(t => t.GiaSuMonHoc.Any(m => m.MaMonHoc == maMon.Value));

            if (maKhoi is > 0)
                query = query.Where(t => t.GiaSuMonHoc.Any(k => k.MaKhoiLop == maKhoi.Value));

            // Lọc đồng thời Môn + Khối trên cùng 1 dòng (gia sư dạy đúng môn đó ở đúng khối đó)
            if (maMon is > 0 && maKhoi is > 0)
                query = query.Where(t => t.GiaSuMonHoc.Any(m => m.MaMonHoc == maMon.Value && m.MaKhoiLop == maKhoi.Value));

            if (!string.IsNullOrWhiteSpace(khuVuc))
            {
                if (khuVuc == "Online")
                    query = query.Where(t => t.HinhThucDay == "Online" || t.HinhThucDay == "Cả hai");
                else
                    query = query.Where(t => t.KhuVucDay.Contains(khuVuc) && t.HinhThucDay != "Online");
            }

            query = mucGia switch
            {
                "duoi150" => query.Where(t => t.HocPhiTheoGio < 150000),
                "150_250" => query.Where(t => t.HocPhiTheoGio >= 150000 && t.HocPhiTheoGio <= 250000),
                "tren250" => query.Where(t => t.HocPhiTheoGio > 250000),
                _ => query
            };

            if (!string.IsNullOrWhiteSpace(trinhDo))
                query = query.Where(t => t.TrinhDo == trinhDo);

            if (daXacMinh)
                query = query.Where(t => t.DaXacMinh == true);

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(t => t.HocPhiTheoGio).ThenBy(t => t.MaGiaSu),
                "price_desc" => query.OrderByDescending(t => t.HocPhiTheoGio).ThenBy(t => t.MaGiaSu),
                "review_desc" => query.OrderByDescending(t => t.TongSoDanhGia).ThenBy(t => t.MaGiaSu),
                _ => query.OrderByDescending(t => t.DiemDanhGiaTb).ThenByDescending(t => t.TongSoDanhGia).ThenBy(t => t.MaGiaSu)
            };

            int totalItems = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));

            var rows = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.MaGiaSu,
                    HoTen = t.MaNguoiDungNavigation.HoTen,
                    Avatar = t.MaNguoiDungNavigation.AnhDaiDien,
                    t.TrinhDo,
                    t.TruongDaoTao,
                    t.HocPhiTheoGio,
                    t.KhuVucDay,
                    t.HinhThucDay,
                    t.DaXacMinh,
                    DiemDanhGiaTB = t.DiemDanhGiaTb,
                    t.TongSoDanhGia,
                    t.SoLopDangDay,
                    t.ThanhTichNoiBat,
                    MonHocs = t.GiaSuMonHoc.Select(m => m.MaMonHocNavigation.TenMonHoc).ToList()
                })
                .ToListAsync();

            var items = rows.Select(r => new
            {
                r.MaGiaSu,
                r.HoTen,
                Initials = VnFormat.Initials(r.HoTen),
                AvatarColor = VnFormat.AvatarColor(r.MaGiaSu),
                // ảnh mặc định /images/avatars/default.png không tồn tại -> để giao diện tự vẽ avatar chữ cái
                Avatar = (r.Avatar != null && r.Avatar.StartsWith("/uploads/")) ? r.Avatar : null,
                r.TrinhDo,
                r.TruongDaoTao,
                r.HocPhiTheoGio,
                r.KhuVucDay,
                r.HinhThucDay,
                DaXacMinh = r.DaXacMinh == true,
                r.DiemDanhGiaTB,
                r.TongSoDanhGia,
                r.SoLopDangDay,
                r.ThanhTichNoiBat,
                MonHocs = r.MonHocs.Distinct().ToList()
            });

            return Ok(new { totalItems, totalPages, currentPage = page, pageSize, items });
        }

        /// <summary>GET api/ApiTutor/suggest?term=... — gợi ý nhanh tên gia sư (autocomplete).</summary>
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest(string? term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2) return Ok(Array.Empty<object>());
            term = term.Trim();
            var result = await _context.HoSoGiaSu.AsNoTracking()
                .Where(t => t.MaNguoiDungNavigation.TrangThai != false && t.MaNguoiDungNavigation.HoTen.Contains(term))
                .OrderByDescending(t => t.DiemDanhGiaTb)
                .Take(6)
                .Select(t => new { id = t.MaGiaSu, name = t.MaNguoiDungNavigation.HoTen })
                .ToListAsync();
            return Ok(result);
        }

        /// <summary>PUT api/ApiTutor/assignment-status/{id} — đổi trạng thái bài tập (Pending ⇄ Completed). Chỉ gia sư sở hữu mới được thao tác.</summary>
        [HttpPut("assignment-status/{id:int}")]
        [RoleAuthorize(Roles.Tutor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAssignmentStatus(int id)
        {
            int userId = HttpContext.Session.GetUserId()!.Value;
            var bt = await _context.BaiTap
                .Include(b => b.MaLichHocNavigation).ThenInclude(l => l.MaGiaSuNavigation)
                .FirstOrDefaultAsync(b => b.MaBaiTap == id);

            if (bt == null) return NotFound(new { success = false, message = "Không tìm thấy bài tập." });
            if (bt.MaLichHocNavigation.MaGiaSuNavigation.MaNguoiDung != userId)
                return StatusCode(403, new { success = false, message = "Bài tập này không thuộc lớp của bạn." });

            bt.TrangThai = bt.TrangThai == "Completed" ? "Pending" : "Completed";
            await _context.SaveChangesAsync();
            return Ok(new { success = true, newStatus = bt.TrangThai });
        }

        /// <summary>PUT api/ApiTutor/class-reminder/{id} — bật/tắt nhắc giờ trước 30 phút cho một ca dạy.</summary>
        [HttpPut("class-reminder/{id:int}")]
        [RoleAuthorize(Roles.Tutor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleReminder(int id)
        {
            int userId = HttpContext.Session.GetUserId()!.Value;
            var lop = await _context.LopHocThoiKhoaBieu
                .Include(l => l.MaGiaSuNavigation)
                .FirstOrDefaultAsync(l => l.MaLichHoc == id);

            if (lop == null) return NotFound(new { success = false, message = "Không tìm thấy lớp học." });
            if (lop.MaGiaSuNavigation.MaNguoiDung != userId)
                return StatusCode(403, new { success = false, message = "Lớp học này không thuộc về bạn." });

            lop.BatNhacNhoTruoc30P = !(lop.BatNhacNhoTruoc30P ?? true);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, enabled = lop.BatNhacNhoTruoc30P });
        }
    }
}
