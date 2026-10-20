using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;
using QLGiaSuLopHoc.Services;

namespace QLGiaSuLopHoc.Controllers
{
    /// <summary>Không gian làm việc của Gia sư: dashboard, thời khóa biểu, bài tập, lịch rảnh, yêu cầu học thử, hồ sơ.</summary>
    [RoleAuthorize(Roles.Tutor)]
    public class TutorPortalController : BaseController
    {
        private readonly QlGiaSuLopHocContext _context;
        private readonly FileUploadService _upload;

        public TutorPortalController(QlGiaSuLopHocContext context, FileUploadService upload)
        {
            _context = context;
            _upload = upload;
        }

        // Gắn số yêu cầu học thử đang chờ để hiển thị huy hiệu trên thanh menu gia sư
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int? userId = CurrentUserId;
            if (userId != null)
            {
                ViewBag.PendingCount = await _context.DangKyHocThu
                    .CountAsync(b => b.MaGiaSuNavigation.MaNguoiDung == userId && b.TrangThai == "Chờ duyệt");
            }
            await next();
        }

        // Lấy hồ sơ gia sư của CHÍNH người đang đăng nhập (không bao giờ lấy hồ sơ của người khác)
        private async Task<HoSoGiaSu> GetTutorAsync()
        {
            int userId = CurrentUserId!.Value;
            var tutor = await _context.HoSoGiaSu
                .Include(t => t.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(t => t.MaNguoiDung == userId);

            if (tutor == null)
            {
                tutor = new HoSoGiaSu
                {
                    MaNguoiDung = userId,
                    TrinhDo = "Sinh viên giỏi",
                    HocPhiTheoGio = 150000,
                    KhuVucDay = "Hà Nội",
                    HinhThucDay = "Cả hai",
                    GioiThieuBio = "<p>Hãy cập nhật phần giới thiệu bản thân.</p>",
                    DaXacMinh = false,
                    DiemDanhGiaTb = 0,
                    TongSoDanhGia = 0,
                    SoLopDangDay = 0
                };
                _context.HoSoGiaSu.Add(tutor);
                await _context.SaveChangesAsync();
                await _context.Entry(tutor).Reference(t => t.MaNguoiDungNavigation).LoadAsync();
            }
            return tutor;
        }

        // ============ 1. DASHBOARD ============
        public async Task<IActionResult> Index()
        {
            var tutor = await GetTutorAsync();
            var classes = await _context.LopHocThoiKhoaBieu
                .Include(l => l.MaMonHocNavigation)
                .Include(l => l.BaiTap)
                .Where(l => l.MaGiaSu == tutor.MaGiaSu)
                .AsNoTracking()
                .ToListAsync();

            var active = classes.Where(l => l.TrangThai == "Đang học").ToList();
            // Lớp học lặp lại hàng tuần: nếu "ngày học tiếp theo" đã qua thì tính sang buổi kế tiếp (chỉ trong bộ nhớ, không ghi DB)
            var today = DateOnly.FromDateTime(DateTime.Today);
            foreach (var l in active)
            {
                if (l.NgayHocTiepTheo < today)
                {
                    int weeks = (today.DayNumber - l.NgayHocTiepTheo.DayNumber + 6) / 7;
                    l.NgayHocTiepTheo = l.NgayHocTiepTheo.AddDays(weeks * 7);
                }
            }
            var allAssignments = classes.SelectMany(l => l.BaiTap).ToList();
            decimal weekly = active.Sum(l => l.HocPhiCaDay);

            var vm = new TutorDashboardViewModel
            {
                Tutor = tutor,
                Classes = active
                    .OrderBy(l => l.NgayHocTiepTheo).ThenBy(l => l.GioBatDau)
                    .ToList(),
                PendingAssignments = allAssignments
                    .Where(b => b.TrangThai != "Completed")
                    .OrderBy(b => b.HanNop).Take(5).ToList(),
                PendingBookings = await _context.DangKyHocThu.CountAsync(b => b.MaGiaSu == tutor.MaGiaSu && b.TrangThai == "Chờ duyệt"),
                WeeklyRevenue = weekly,
                MonthlyRevenue = weekly * 4, // mỗi lớp học 1 buổi/tuần ≈ 4 buổi/tháng
                AssignmentsTotal = allAssignments.Count,
                AssignmentsDone = allAssignments.Count(b => b.TrangThai == "Completed")
            };
            return View(vm);
        }

        // ============ 2. THỜI KHÓA BIỂU ============
        public async Task<IActionResult> Schedule()
        {
            var tutor = await GetTutorAsync();
            var classes = await _context.LopHocThoiKhoaBieu
                .Include(l => l.MaMonHocNavigation)
                .Where(l => l.MaGiaSu == tutor.MaGiaSu)
                .AsNoTracking()
                .ToListAsync();

            return View(classes
                .OrderBy(l => VnFormat.DayOrder(l.ThuTrongTuan)).ThenBy(l => l.GioBatDau)
                .ToList());
        }

        [HttpGet]
        public async Task<IActionResult> ClassForm(int? id)
        {
            var tutor = await GetTutorAsync();
            var vm = new ClassFormPageViewModel { Subjects = await SubjectsOfAsync(tutor) };

            if (id != null)
            {
                var c = await _context.LopHocThoiKhoaBieu.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.MaLichHoc == id && l.MaGiaSu == tutor.MaGiaSu);
                if (c == null) return NotFound();

                vm.IsEdit = true;
                vm.Form = new ClassScheduleViewModel
                {
                    MaLichHoc = c.MaLichHoc,
                    TenHocSinh = c.TenHocSinh,
                    MaMonHoc = c.MaMonHoc,
                    NgayHocTiepTheo = c.NgayHocTiepTheo,
                    GioBatDau = c.GioBatDau,
                    GioKetThuc = c.GioKetThuc,
                    PhongHocHoacLink = c.PhongHocHoacLink,
                    HocPhiCaDay = c.HocPhiCaDay,
                    BatNhacNhoTruoc30P = c.BatNhacNhoTruoc30P ?? true,
                    TrangThai = c.TrangThai ?? "Đang học"
                };
            }
            else
            {
                vm.Form.MaMonHoc = vm.Subjects.FirstOrDefault()?.MaMonHoc ?? 0;
                vm.Form.HocPhiCaDay = tutor.HocPhiTheoGio * 1.5m;
            }
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SaveClass([Bind(Prefix = "Form")] ClassScheduleViewModel model)
        {
            var tutor = await GetTutorAsync();
            bool isEdit = model.MaLichHoc > 0;
            LopHocThoiKhoaBieu? entity = null;

            if (isEdit)
            {
                entity = await _context.LopHocThoiKhoaBieu
                    .FirstOrDefaultAsync(l => l.MaLichHoc == model.MaLichHoc && l.MaGiaSu == tutor.MaGiaSu);
                if (entity == null) return NotFound();
            }

            if (!await _context.MonHoc.AnyAsync(m => m.MaMonHoc == model.MaMonHoc))
                ModelState.AddModelError("Form.MaMonHoc", "Môn học không hợp lệ.");

            if (!isEdit && model.NgayHocTiepTheo < DateOnly.FromDateTime(DateTime.Today))
                ModelState.AddModelError("Form.NgayHocTiepTheo", "Ngày học không được ở trong quá khứ.");

            // Chống trùng lịch: cùng thứ, giờ giao nhau, cả hai đều đang học
            if (ModelState.IsValid && model.TrangThai == "Đang học")
            {
                string thu = VnFormat.ThuTuNgay(model.NgayHocTiepTheo);
                var start = model.GioBatDau;
                var end = model.GioKetThuc;
                int self = model.MaLichHoc;
                int tutorId = tutor.MaGiaSu;

                var sameDay = await _context.LopHocThoiKhoaBieu.AsNoTracking()
                    .Where(l => l.MaGiaSu == tutorId && l.MaLichHoc != self && l.TrangThai == "Đang học" && l.ThuTrongTuan == thu)
                    .ToListAsync();
                var clash = sameDay.FirstOrDefault(l => l.GioBatDau < end && start < l.GioKetThuc);
                if (clash != null)
                    ModelState.AddModelError("Form.GioBatDau",
                        $"Trùng lịch với lớp của học sinh {clash.TenHocSinh} ({clash.GioBatDau:HH\\:mm} - {clash.GioKetThuc:HH\\:mm}) vào {thu}.");
            }

            if (!ModelState.IsValid)
            {
                return View("ClassForm", new ClassFormPageViewModel
                {
                    Form = model,
                    IsEdit = isEdit,
                    Subjects = await SubjectsOfAsync(tutor)
                });
            }

            entity ??= new LopHocThoiKhoaBieu { MaGiaSu = tutor.MaGiaSu };
            entity.TenHocSinh = model.TenHocSinh.Trim();
            entity.MaMonHoc = model.MaMonHoc;
            entity.NgayHocTiepTheo = model.NgayHocTiepTheo;
            entity.ThuTrongTuan = VnFormat.ThuTuNgay(model.NgayHocTiepTheo);
            entity.GioBatDau = model.GioBatDau;
            entity.GioKetThuc = model.GioKetThuc;
            entity.PhongHocHoacLink = model.PhongHocHoacLink.Trim();
            entity.HocPhiCaDay = model.HocPhiCaDay;
            entity.BatNhacNhoTruoc30P = model.BatNhacNhoTruoc30P;
            entity.TrangThai = model.TrangThai;

            if (!isEdit) _context.LopHocThoiKhoaBieu.Add(entity);
            await _context.SaveChangesAsync();
            await _context.RecalculateClassCountAsync(tutor.MaGiaSu);

            TempData["SuccessMsg"] = isEdit ? "Đã cập nhật lớp học." : "Đã thêm lớp học mới vào thời khóa biểu.";
            return RedirectToAction(nameof(Schedule));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var tutor = await GetTutorAsync();
            var c = await _context.LopHocThoiKhoaBieu.Include(l => l.BaiTap)
                .FirstOrDefaultAsync(l => l.MaLichHoc == id && l.MaGiaSu == tutor.MaGiaSu);
            if (c == null) return NotFound();

            _context.BaiTap.RemoveRange(c.BaiTap);
            _context.LopHocThoiKhoaBieu.Remove(c);
            await _context.SaveChangesAsync();
            await _context.RecalculateClassCountAsync(tutor.MaGiaSu);

            TempData["SuccessMsg"] = "Đã xóa lớp học và các bài tập liên quan.";
            return RedirectToAction(nameof(Schedule));
        }

        // ============ 3. BÀI TẬP ============
        public async Task<IActionResult> Assignments(string filter = "all")
        {
            return View(await BuildAssignmentsAsync(await GetTutorAsync(), filter));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment([Bind(Prefix = "Form")] AssignmentViewModel model)
        {
            var tutor = await GetTutorAsync();

            // Chỉ được giao bài cho lớp của chính mình
            bool owns = await _context.LopHocThoiKhoaBieu.AnyAsync(l => l.MaLichHoc == model.MaLichHoc && l.MaGiaSu == tutor.MaGiaSu);
            if (!owns) ModelState.AddModelError("Form.MaLichHoc", "Lớp học không hợp lệ.");

            if (!ModelState.IsValid)
            {
                var page = await BuildAssignmentsAsync(tutor, "all");
                page.Form = model;
                return View("Assignments", page);
            }

            _context.BaiTap.Add(new BaiTap
            {
                MaLichHoc = model.MaLichHoc,
                TieuDe = model.TieuDe.Trim(),
                NoiDungBaiTap = string.IsNullOrWhiteSpace(model.NoiDungBaiTap) ? null : model.NoiDungBaiTap.Trim(),
                HanNop = model.HanNop,
                TrangThai = "Pending",
                NgayGiao = DateTime.Now
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã giao bài tập mới kèm Deadline cho học sinh!";
            return RedirectToAction(nameof(Assignments));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var tutor = await GetTutorAsync();
            var bt = await _context.BaiTap.Include(b => b.MaLichHocNavigation)
                .FirstOrDefaultAsync(b => b.MaBaiTap == id && b.MaLichHocNavigation.MaGiaSu == tutor.MaGiaSu);
            if (bt == null) return NotFound();

            _context.BaiTap.Remove(bt);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã xóa bài tập.";
            return RedirectToAction(nameof(Assignments));
        }

        // ============ 4. YÊU CẦU HỌC THỬ ============
        public async Task<IActionResult> Bookings(string status = "all")
        {
            var tutor = await GetTutorAsync();
            var query = _context.DangKyHocThu
                .Include(b => b.MaMonHocNavigation)
                .Where(b => b.MaGiaSu == tutor.MaGiaSu);
            if (status != "all") query = query.Where(b => b.TrangThai == status);

            return View(new BookingsPageViewModel
            {
                Status = status,
                Items = await query.OrderBy(b => b.TrangThai == "Chờ duyệt" ? 0 : 1).ThenByDescending(b => b.NgayTao).AsNoTracking().ToListAsync(),
                PendingCount = await _context.DangKyHocThu.CountAsync(b => b.MaGiaSu == tutor.MaGiaSu && b.TrangThai == "Chờ duyệt")
            });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int id) => await ChangeBookingAsync(id, "Đã xác nhận", "Đã xác nhận buổi học thử. Hãy liên hệ phụ huynh qua số điện thoại đã đăng ký.");

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int id) => await ChangeBookingAsync(id, "Từ chối", "Đã từ chối yêu cầu học thử.");

        private async Task<IActionResult> ChangeBookingAsync(int id, string newStatus, string message)
        {
            var tutor = await GetTutorAsync();
            var booking = await _context.DangKyHocThu.FirstOrDefaultAsync(b => b.MaDangKy == id && b.MaGiaSu == tutor.MaGiaSu);
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
            return RedirectToAction(nameof(Bookings));
        }

        // ============ 5. MA TRẬN LỊCH RẢNH ============
        public async Task<IActionResult> Availability()
        {
            var tutor = await GetTutorAsync();
            var rows = await _context.MaTranLichRanh.AsNoTracking().Where(m => m.MaGiaSu == tutor.MaGiaSu).ToListAsync();
            var free = rows.Where(r => r.ConTrong == true)
                .Select(r => $"{VnFormat.NormalizeDay(r.ThuTrongTuan)}|{VnFormat.NormalizeSession(r.BuoiTrongNgay)}")
                .ToHashSet();
            ViewBag.Free = free;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveAvailability(string[]? slots)
        {
            var tutor = await GetTutorAsync();
            var chosen = (slots ?? Array.Empty<string>()).ToHashSet();

            var old = await _context.MaTranLichRanh.Where(m => m.MaGiaSu == tutor.MaGiaSu).ToListAsync();
            _context.MaTranLichRanh.RemoveRange(old);

            foreach (var day in VnFormat.Days)
            {
                foreach (var session in VnFormat.Sessions)
                {
                    _context.MaTranLichRanh.Add(new MaTranLichRanh
                    {
                        MaGiaSu = tutor.MaGiaSu,
                        ThuTrongTuan = day,
                        BuoiTrongNgay = session,
                        KhungGioChiTiet = VnFormat.SessionTimes[session],
                        ConTrong = chosen.Contains($"{day}|{session}")
                    });
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã cập nhật ma trận lịch rảnh. Phụ huynh sẽ thấy lịch mới trên hồ sơ của bạn.";
            return RedirectToAction(nameof(Availability));
        }

        // ============ 6. HỒ SƠ NĂNG LỰC & MÔN DẠY ============
        [HttpGet]
        public async Task<IActionResult> Profile() => View(await BuildProfilePageAsync(await GetTutorAsync()));

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "Form")] TutorProfileUpdateViewModel model)
        {
            var tutor = await GetTutorAsync();

            if (model.FileAnhBangCap is { Length: > 0 } && ModelState.IsValid)
            {
                var (path, error) = await _upload.SaveImageAsync(model.FileAnhBangCap, "certificates", $"cert{tutor.MaGiaSu}");
                if (error != null) ModelState.AddModelError("Form.FileAnhBangCap", error);
                else
                {
                    tutor.AnhBangCapTheSv = path;
                    if (tutor.DaXacMinh == true)
                    {
                        tutor.DaXacMinh = false; // bằng cấp mới cần được Admin xác minh lại
                        TempData["InfoMsg"] = "Bạn vừa thay ảnh bằng cấp nên Tích xanh sẽ được Admin xác minh lại.";
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var page = await BuildProfilePageAsync(tutor);
                page.Form = model;
                page.Form.AnhBangCapHienTai = tutor.AnhBangCapTheSv;
                return View("Profile", page);
            }

            tutor.TrinhDo = model.TrinhDo;
            tutor.TruongDaoTao = string.IsNullOrWhiteSpace(model.TruongDaoTao) ? null : model.TruongDaoTao.Trim();
            tutor.HocPhiTheoGio = model.HocPhiTheoGio;
            tutor.KhuVucDay = model.KhuVucDay.Trim();
            tutor.HinhThucDay = model.HinhThucDay;
            tutor.GioiThieuBio = SafeHtml.Clean(model.GioiThieuBio); // làm sạch HTML từ CKEditor
            tutor.ThanhTichNoiBat = string.IsNullOrWhiteSpace(model.ThanhTichNoiBat) ? null : model.ThanhTichNoiBat.Trim();

            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã cập nhật hồ sơ năng lực thành công!";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubjects(string[]? pairs)
        {
            var tutor = await GetTutorAsync();
            var validMon = await _context.MonHoc.Select(m => m.MaMonHoc).ToListAsync();
            var validKhoi = await _context.KhoiLop.Select(k => k.MaKhoiLop).ToListAsync();

            var wanted = new HashSet<(int mon, int khoi)>();
            foreach (var p in pairs ?? Array.Empty<string>())
            {
                var parts = p.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[0], out var m) && int.TryParse(parts[1], out var k)
                    && validMon.Contains(m) && validKhoi.Contains(k))
                    wanted.Add((m, k));
            }

            var current = await _context.GiaSuMonHoc.Where(g => g.MaGiaSu == tutor.MaGiaSu).ToListAsync();
            _context.GiaSuMonHoc.RemoveRange(current.Where(g => !wanted.Contains((g.MaMonHoc, g.MaKhoiLop))));
            var existing = current.Select(g => (g.MaMonHoc, g.MaKhoiLop)).ToHashSet();
            foreach (var (mon, khoi) in wanted.Where(w => !existing.Contains(w)))
            {
                _context.GiaSuMonHoc.Add(new GiaSuMonHoc { MaGiaSu = tutor.MaGiaSu, MaMonHoc = mon, MaKhoiLop = khoi });
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Đã cập nhật danh sách môn học & khối lớp bạn nhận dạy.";
            return RedirectToAction(nameof(Profile));
        }

        // ---------- helpers ----------
        private async Task<List<MonHoc>> SubjectsOfAsync(HoSoGiaSu tutor)
        {
            var mine = await _context.GiaSuMonHoc.Where(g => g.MaGiaSu == tutor.MaGiaSu)
                .Select(g => g.MaMonHocNavigation).Distinct().OrderBy(m => m.TenMonHoc).AsNoTracking().ToListAsync();
            return mine.Count > 0 ? mine : await _context.MonHoc.OrderBy(m => m.TenMonHoc).AsNoTracking().ToListAsync();
        }

        private async Task<AssignmentsPageViewModel> BuildAssignmentsAsync(HoSoGiaSu tutor, string filter)
        {
            var query = _context.BaiTap
                .Include(b => b.MaLichHocNavigation).ThenInclude(l => l.MaMonHocNavigation)
                .Where(b => b.MaLichHocNavigation.MaGiaSu == tutor.MaGiaSu);
            if (filter == "pending") query = query.Where(b => b.TrangThai != "Completed");
            else if (filter == "done") query = query.Where(b => b.TrangThai == "Completed");

            var classes = await _context.LopHocThoiKhoaBieu.Include(l => l.MaMonHocNavigation)
                .Where(l => l.MaGiaSu == tutor.MaGiaSu).AsNoTracking().ToListAsync();

            return new AssignmentsPageViewModel
            {
                Filter = filter,
                Items = await query.OrderBy(b => b.HanNop).AsNoTracking().ToListAsync(),
                Classes = classes,
                Form = new AssignmentViewModel { MaLichHoc = classes.FirstOrDefault()?.MaLichHoc ?? 0 }
            };
        }

        private async Task<TutorProfilePageViewModel> BuildProfilePageAsync(HoSoGiaSu tutor)
        {
            var pairRows = await _context.GiaSuMonHoc.Where(g => g.MaGiaSu == tutor.MaGiaSu)
                .Select(g => new { g.MaMonHoc, g.MaKhoiLop }).ToListAsync();
            var pairs = pairRows.Select(r => $"{r.MaMonHoc}_{r.MaKhoiLop}").ToList();

            return new TutorProfilePageViewModel
            {
                Tutor = tutor,
                Subjects = await _context.MonHoc.AsNoTracking().OrderBy(m => m.TenMonHoc).ToListAsync(),
                Grades = await _context.KhoiLop.AsNoTracking().OrderBy(k => k.MaKhoiLop).ToListAsync(),
                SelectedPairs = pairs.ToHashSet(),
                Form = new TutorProfileUpdateViewModel
                {
                    TrinhDo = tutor.TrinhDo,
                    TruongDaoTao = tutor.TruongDaoTao,
                    HocPhiTheoGio = tutor.HocPhiTheoGio,
                    KhuVucDay = tutor.KhuVucDay,
                    HinhThucDay = tutor.HinhThucDay,
                    GioiThieuBio = tutor.GioiThieuBio ?? "",
                    ThanhTichNoiBat = tutor.ThanhTichNoiBat,
                    AnhBangCapHienTai = tutor.AnhBangCapTheSv
                }
            };
        }
    }
}
