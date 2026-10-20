using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;
using QLGiaSuLopHoc.Services;

namespace QLGiaSuLopHoc.Controllers
{
    public class AccountController : BaseController
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

        private readonly QlGiaSuLopHocContext _context;
        private readonly IMemoryCache _cache;
        private readonly FileUploadService _upload;

        public AccountController(QlGiaSuLopHocContext context, IMemoryCache cache, FileUploadService upload)
        {
            _context = context;
            _cache = cache;
            _upload = upload;
        }

        // ============ ĐĂNG NHẬP ============
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (CurrentUserId != null) return RedirectToRole(CurrentRole);

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            // Đọc Cookie nếu trước đó người dùng chọn "Ghi nhớ đăng nhập"
            if (Request.Cookies.TryGetValue("RememberedEmail", out string? savedEmail))
            {
                model.Email = savedEmail ?? "";
                model.RememberMe = true;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string email = model.Email.Trim().ToLowerInvariant();
            string lockKey = $"login-fail:{email}:{HttpContext.Connection.RemoteIpAddress}";

            if (_cache.TryGetValue(lockKey, out int fails) && fails >= MaxFailedAttempts)
            {
                ModelState.AddModelError(string.Empty, $"Bạn đã nhập sai quá {MaxFailedAttempts} lần. Vui lòng thử lại sau {LockDuration.TotalMinutes:0} phút.");
                return View(model);
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email == email);
            bool valid = user != null && PasswordService.Verify(model.Password, user.MatKhauHash, out _);

            if (!valid || user == null)
            {
                _cache.Set(lockKey, fails + 1, LockDuration);
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
                return View(model);
            }

            if (user.TrangThai == false)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            _cache.Remove(lockKey);

            // Tự động nâng cấp mật khẩu MD5 cũ lên PBKDF2
            PasswordService.Verify(model.Password, user.MatKhauHash, out bool needsUpgrade);
            if (needsUpgrade)
            {
                user.MatKhauHash = PasswordService.Hash(model.Password);
                await _context.SaveChangesAsync();
            }

            // 1. Lưu thông tin vào Session (làm mới phiên để tránh session fixation)
            HttpContext.Session.Clear();
            string role = Roles.Normalize(user.VaiTro);
            HttpContext.Session.SetInt32(SessionKeys.UserId, user.MaNguoiDung);
            HttpContext.Session.SetString(SessionKeys.FullName, user.HoTen);
            HttpContext.Session.SetString(SessionKeys.UserRole, role);

            // 2. Cookie "Remember Me"
            if (model.RememberMe)
            {
                Response.Cookies.Append("RememberedEmail", user.Email, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(15),
                    HttpOnly = true,
                    IsEssential = true
                });
            }
            else
            {
                Response.Cookies.Delete("RememberedEmail");
            }

            // 3. Điều hướng theo đúng vai trò
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);
            return RedirectToRole(role);
        }

        // ============ ĐĂNG KÝ ============
        [HttpGet]
        public IActionResult Register(string? role = null)
        {
            if (CurrentUserId != null) return RedirectToRole(CurrentRole);
            return View(new RegisterViewModel { VaiTro = role == Roles.Tutor ? Roles.Tutor : Roles.Parent });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string email = model.Email.Trim().ToLowerInvariant();
            if (await _context.NguoiDung.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký.");
                return View(model);
            }

            var user = new NguoiDung
            {
                HoTen = model.HoTen.Trim(),
                Email = email,
                SoDienThoai = model.SoDienThoai,
                MatKhauHash = PasswordService.Hash(model.Password),
                VaiTro = model.VaiTro,
                NhanThongBaoDesktop = true,
                NgayTao = DateTime.Now,
                TrangThai = true
            };

            if (model.VaiTro == Roles.Tutor)
            {
                // Tạo hồ sơ gia sư tối thiểu, chờ Admin xác minh bằng cấp
                user.HoSoGiaSu = new HoSoGiaSu
                {
                    TrinhDo = "Sinh viên giỏi",
                    HocPhiTheoGio = 150000,
                    KhuVucDay = "Hà Nội",
                    HinhThucDay = "Cả hai",
                    GioiThieuBio = "<p>Gia sư mới tham gia EduMatch. Hãy cập nhật phần giới thiệu bản thân để phụ huynh hiểu rõ hơn về bạn.</p>",
                    DaXacMinh = false,
                    DiemDanhGiaTb = 0,
                    TongSoDanhGia = 0,
                    SoLopDangDay = 0
                };
            }

            _context.NguoiDung.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = model.VaiTro == Roles.Tutor
                ? "Đăng ký gia sư thành công! Hãy đăng nhập, hoàn thiện hồ sơ và tải lên bằng cấp để được xác minh."
                : "Đăng ký thành công! Hãy đăng nhập để bắt đầu tìm gia sư.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMsg"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();

        // ============ HỒ SƠ TÀI KHOẢN ============
        [RoleAuthorize(Roles.Admin, Roles.Tutor, Roles.Parent)]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _context.NguoiDung.AsNoTracking().FirstOrDefaultAsync(u => u.MaNguoiDung == CurrentUserId);
            if (user == null) return RedirectToAction(nameof(Login));
            return View(BuildPage(user));
        }

        [RoleAuthorize(Roles.Admin, Roles.Tutor, Roles.Parent)]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "Profile")] AccountProfileViewModel model)
        {
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == CurrentUserId);
            if (user == null) return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
            {
                var page = BuildPage(user);
                page.Profile.HoTen = model.HoTen;
                page.Profile.SoDienThoai = model.SoDienThoai;
                page.Profile.NhanThongBaoDesktop = model.NhanThongBaoDesktop;
                return View("Profile", page);
            }

            if (model.FileAnhDaiDien is { Length: > 0 })
            {
                var (path, error) = await _upload.SaveImageAsync(model.FileAnhDaiDien, "avatars", $"u{user.MaNguoiDung}");
                if (error != null)
                {
                    ModelState.AddModelError("Profile.FileAnhDaiDien", error);
                    var errPage = BuildPage(user);
                    errPage.Profile.HoTen = model.HoTen;
                    errPage.Profile.SoDienThoai = model.SoDienThoai;
                    errPage.Profile.NhanThongBaoDesktop = model.NhanThongBaoDesktop;
                    return View("Profile", errPage);
                }
                user.AnhDaiDien = path;
            }

            user.HoTen = model.HoTen.Trim();
            user.SoDienThoai = model.SoDienThoai;
            user.NhanThongBaoDesktop = model.NhanThongBaoDesktop;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString(SessionKeys.FullName, user.HoTen);
            TempData["SuccessMsg"] = "Đã cập nhật thông tin tài khoản.";
            return RedirectToAction(nameof(Profile));
        }

        [RoleAuthorize(Roles.Admin, Roles.Tutor, Roles.Parent)]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangePasswordViewModel model)
        {
            var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == CurrentUserId);
            if (user == null) return RedirectToAction(nameof(Login));

            if (ModelState.IsValid && !PasswordService.Verify(model.CurrentPassword, user.MatKhauHash, out _))
                ModelState.AddModelError("Password.CurrentPassword", "Mật khẩu hiện tại không đúng.");

            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = "password";
                return View("Profile", BuildPage(user));
            }

            user.MatKhauHash = PasswordService.Hash(model.NewPassword);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }

        // ---------- helpers ----------
        private IActionResult RedirectToRole(string? role) => role switch
        {
            Roles.Admin => RedirectToAction("Index", "HomeAdmin", new { area = "Admin" }),
            Roles.Tutor => RedirectToAction("Index", "TutorPortal", new { area = "" }),
            _ => RedirectToAction("Index", "Home", new { area = "" })
        };

        private static AccountPageViewModel BuildPage(NguoiDung user) => new()
        {
            Profile = new AccountProfileViewModel
            {
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                NhanThongBaoDesktop = user.NhanThongBaoDesktop ?? true,
                AnhDaiDien = user.AnhDaiDien,
                VaiTro = Roles.Normalize(user.VaiTro)
            }
        };
    }
}
