using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;
using QLGiaSuLopHoc.Services;

namespace QLGiaSuLopHoc.Controllers
{
    public class HomeController : BaseController
    {
        private readonly QlGiaSuLopHocContext _context;
        public HomeController(QlGiaSuLopHocContext context) => _context = context;

        // ============ TRANG CHỦ: tìm kiếm gia sư (dữ liệu lấy từ Web API) ============
        public async Task<IActionResult> Index()
        {
            var visibleReviews = _context.DanhGia.Where(d => d.HienThi == true);
            int reviewCount = await visibleReviews.CountAsync();

            var vm = new HomeIndexViewModel
            {
                TutorCount = await _context.HoSoGiaSu.CountAsync(),
                VerifiedCount = await _context.HoSoGiaSu.CountAsync(t => t.DaXacMinh == true),
                ReviewCount = reviewCount,
                ClassCount = await _context.LopHocThoiKhoaBieu.CountAsync(),
                AverageRating = reviewCount > 0 ? Math.Round(await visibleReviews.AverageAsync(d => (double)d.SoSao), 1) : 0,
                Grades = await _context.KhoiLop.AsNoTracking().OrderBy(k => k.MaKhoiLop).ToListAsync(),
                Subjects = await _context.MonHoc.AsNoTracking().OrderBy(m => m.TenMonHoc).ToListAsync(),
                LastViewedTutor = Request.Cookies["LastViewedTutor"],
                LastViewedTutorId = Request.Cookies["LastViewedTutorId"]
            };
            return View(vm);
        }

        // ============ CHI TIẾT GIA SƯ ============
        public async Task<IActionResult> Details(int id)
        {
            var vm = await BuildDetailsAsync(id);
            if (vm == null) return NotFound();

            // Cookie: ghi nhớ gia sư xem gần nhất trong 7 ngày
            var options = new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7), HttpOnly = true, IsEssential = true };
            Response.Cookies.Append("LastViewedTutor", vm.Tutor.MaNguoiDungNavigation.HoTen, options);
            Response.Cookies.Append("LastViewedTutorId", vm.Tutor.MaGiaSu.ToString(), options);

            return View(vm);
        }

        [HttpPost]
        public IActionResult ClearHistory()
        {
            Response.Cookies.Delete("LastViewedTutor");
            Response.Cookies.Delete("LastViewedTutorId");
            return RedirectToAction(nameof(Index));
        }

        // ============ ĐĂNG KÝ HỌC THỬ 01 BUỔI ============
        [HttpPost]
        public async Task<IActionResult> BookTrial([Bind(Prefix = "Booking")] TrialBookingViewModel model)
        {
            var vm = await BuildDetailsAsync(model.MaGiaSu, booking: model);
            if (vm == null) return NotFound();

            if (vm.IsOwnProfile)
                ModelState.AddModelError("Booking.MaGiaSu", "Bạn không thể đăng ký học thử với chính hồ sơ của mình.");

            if (!vm.BookableSubjects.Any(m => m.MaMonHoc == model.MaMonHoc))
                ModelState.AddModelError("Booking.MaMonHoc", "Gia sư này không dạy môn học bạn đã chọn.");

            bool duplicate = await _context.DangKyHocThu.AnyAsync(b =>
                b.MaGiaSu == model.MaGiaSu && b.SoDienThoai == model.SoDienThoai && b.TrangThai == "Chờ duyệt");
            if (duplicate)
                ModelState.AddModelError("Booking.SoDienThoai", "Số điện thoại này đã có yêu cầu học thử đang chờ gia sư xác nhận.");

            if (!ModelState.IsValid)
            {
                ViewBag.ScrollTo = "booking";
                return View("Details", vm);
            }

            _context.DangKyHocThu.Add(new DangKyHocThu
            {
                MaGiaSu = model.MaGiaSu,
                MaPhuHuynh = CurrentRole == Roles.Parent ? CurrentUserId : null,
                HoTenPhuHuynh = model.HoTenPhuHuynh.Trim(),
                SoDienThoai = model.SoDienThoai,
                MaMonHoc = model.MaMonHoc,
                TrinhDoHienTaiHs = model.TrinhDoHienTaiHS,
                KhungGioMongMuon = model.KhungGioMongMuon,
                GhiChuYeuCau = string.IsNullOrWhiteSpace(model.GhiChuYeuCau) ? null : model.GhiChuYeuCau.Trim(),
                TrangThai = "Chờ duyệt",
                NgayTao = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Đăng ký học thử 01 buổi thành công! Gia sư sẽ liên hệ xác nhận sớm nhất.";
            return RedirectToAction(nameof(Details), new { id = model.MaGiaSu });
        }

        // ============ ĐÁNH GIÁ GIA SƯ (chỉ Phụ huynh đã đăng nhập, mỗi gia sư 1 lần) ============
        [HttpPost]
        [RoleAuthorize(Roles.Parent)]
        public async Task<IActionResult> AddReview([Bind(Prefix = "Review")] ReviewViewModel model)
        {
            var vm = await BuildDetailsAsync(model.MaGiaSu, review: model);
            if (vm == null) return NotFound();

            int userId = CurrentUserId!.Value;
            if (await _context.DanhGia.AnyAsync(d => d.MaGiaSu == model.MaGiaSu && d.MaPhuHuynh == userId))
                ModelState.AddModelError("Review.NoiDungBinhLuan", "Bạn đã đánh giá gia sư này rồi.");

            if (!ModelState.IsValid)
            {
                ViewBag.ScrollTo = "reviews";
                return View("Details", vm);
            }

            _context.DanhGia.Add(new DanhGia
            {
                MaGiaSu = model.MaGiaSu,
                MaPhuHuynh = userId,
                SoSao = model.SoSao,
                NoiDungBinhLuan = model.NoiDungBinhLuan.Trim(),
                NgayDanhGia = DateTime.Now,
                HienThi = true
            });
            await _context.SaveChangesAsync();
            await _context.RecalculateRatingAsync(model.MaGiaSu);

            TempData["SuccessMsg"] = "Cảm ơn bạn đã gửi đánh giá cho gia sư!";
            return RedirectToAction(nameof(Details), new { id = model.MaGiaSu });
        }

        public IActionResult Privacy() => View();

        [IgnoreAntiforgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Trang lỗi 404/403/... (được UseStatusCodePagesWithReExecute gọi)
        [IgnoreAntiforgeryToken]
        public IActionResult HttpStatus(int code)
        {
            Response.StatusCode = code;
            ViewBag.Code = code;
            return View();
        }

        // ---------- helpers ----------
        private async Task<TutorDetailsViewModel?> BuildDetailsAsync(int id, TrialBookingViewModel? booking = null, ReviewViewModel? review = null)
        {
            var tutor = await _context.HoSoGiaSu
                .Include(t => t.MaNguoiDungNavigation)
                .Include(t => t.GiaSuMonHoc).ThenInclude(gm => gm.MaMonHocNavigation)
                .Include(t => t.GiaSuMonHoc).ThenInclude(gm => gm.MaKhoiLopNavigation)
                .Include(t => t.MaTranLichRanh)
                .Include(t => t.DanhGia).ThenInclude(d => d.MaPhuHuynhNavigation)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.MaGiaSu == id);

            if (tutor == null || tutor.MaNguoiDungNavigation.TrangThai == false) return null;

            var subjects = tutor.GiaSuMonHoc
                .Select(g => g.MaMonHocNavigation)
                .DistinctBy(m => m.MaMonHoc)
                .OrderBy(m => m.TenMonHoc)
                .ToList();
            if (subjects.Count == 0) subjects = await _context.MonHoc.AsNoTracking().OrderBy(m => m.TenMonHoc).ToListAsync();

            var subjectIds = tutor.GiaSuMonHoc.Select(g => g.MaMonHoc).Distinct().ToList();
            var similar = await _context.HoSoGiaSu
                .Include(t => t.MaNguoiDungNavigation)
                .Where(t => t.MaGiaSu != id && t.MaNguoiDungNavigation.TrangThai != false
                            && t.GiaSuMonHoc.Any(g => subjectIds.Contains(g.MaMonHoc)))
                .OrderByDescending(t => t.DiemDanhGiaTb)
                .Take(3)
                .AsNoTracking()
                .ToListAsync();

            var visible = tutor.DanhGia.Where(d => d.HienThi == true).ToList();
            int? userId = CurrentUserId;
            var vm = new TutorDetailsViewModel
            {
                Tutor = tutor,
                BookableSubjects = subjects,
                SimilarTutors = similar,
                StarCounts = Enumerable.Range(1, 5).ToDictionary(s => s, s => visible.Count(d => d.SoSao == s)),
                IsLoggedIn = userId != null,
                IsParent = CurrentRole == Roles.Parent,
                IsOwnProfile = userId != null && tutor.MaNguoiDung == userId,
                HasReviewed = userId != null && tutor.DanhGia.Any(d => d.MaPhuHuynh == userId),
                Booking = booking ?? new TrialBookingViewModel { MaGiaSu = id, KhungGioMongMuon = DateTime.Now.AddDays(1) },
                Review = review ?? new ReviewViewModel { MaGiaSu = id }
            };

            // Điền sẵn thông tin liên hệ nếu đã đăng nhập
            if (booking == null && userId != null)
            {
                var user = await _context.NguoiDung.AsNoTracking().FirstOrDefaultAsync(u => u.MaNguoiDung == userId);
                if (user != null)
                {
                    vm.Booking.HoTenPhuHuynh = user.HoTen;
                    vm.Booking.SoDienThoai = user.SoDienThoai;
                }
            }
            return vm;
        }
    }
}
