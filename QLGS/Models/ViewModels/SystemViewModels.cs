using System.ComponentModel.DataAnnotations;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.Validation;

namespace QLGiaSuLopHoc.Models.ViewModels
{
    // ============ TÀI KHOẢN ============

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ Email đăng nhập")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ tên phải từ 3 đến 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại gồm 10 chữ số hợp lệ (03, 05, 07, 08, 09...)")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 50 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Nhập lại mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại tài khoản")]
        [RegularExpression("^(Parent|Tutor)$", ErrorMessage = "Loại tài khoản không hợp lệ")]
        [Display(Name = "Loại tài khoản")]
        public string VaiTro { get; set; } = "Parent";
    }

    public class AccountProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ tên phải từ 3 đến 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại gồm 10 chữ số hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Display(Name = "Nhận thông báo nhắc lịch trên màn hình")]
        public bool NhanThongBaoDesktop { get; set; }

        public IFormFile? FileAnhDaiDien { get; set; }

        public string? AnhDaiDien { get; set; }
        public string VaiTro { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6 đến 50 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class AccountPageViewModel
    {
        public AccountProfileViewModel Profile { get; set; } = new();
        public ChangePasswordViewModel Password { get; set; } = new();
    }

    // ============ PHỤ HUYNH: ĐĂNG KÝ HỌC THỬ & ĐÁNH GIÁ ============

    public class TrialBookingViewModel
    {
        [Required]
        public int MaGiaSu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên phụ huynh/học sinh")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ tên phải từ 3 đến 100 ký tự")]
        [Display(Name = "Họ tên Phụ huynh / Học sinh")]
        public string HoTenPhuHuynh { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại gồm 10 chữ số hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public int MaMonHoc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trình độ hiện tại của học sinh")]
        [Display(Name = "Trình độ hiện tại")]
        public string TrinhDoHienTaiHS { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thời gian học thử")]
        [FutureDate(ErrorMessage = "Thời gian học thử phải lớn hơn hiện tại ít nhất 1 giờ", MinHoursAhead = 1)]
        [Display(Name = "Ngày & giờ mong muốn")]
        public DateTime KhungGioMongMuon { get; set; } = DateTime.Now.AddDays(1);

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? GhiChuYeuCau { get; set; }
    }

    public class ReviewViewModel
    {
        [Required]
        public int MaGiaSu { get; set; }

        [Range(1, 5, ErrorMessage = "Vui lòng chọn số sao từ 1 đến 5")]
        [Display(Name = "Số sao")]
        public int SoSao { get; set; } = 5;

        [Required(ErrorMessage = "Vui lòng nhập nhận xét")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Nhận xét phải từ 10 đến 500 ký tự")]
        [Display(Name = "Nhận xét")]
        public string NoiDungBinhLuan { get; set; } = string.Empty;
    }

    /// <summary>Dữ liệu cho trang chi tiết gia sư.</summary>
    public class TutorDetailsViewModel
    {
        public HoSoGiaSu Tutor { get; set; } = null!;
        public TrialBookingViewModel Booking { get; set; } = new();
        public ReviewViewModel Review { get; set; } = new();
        public List<MonHoc> BookableSubjects { get; set; } = new();
        public List<HoSoGiaSu> SimilarTutors { get; set; } = new();
        public Dictionary<int, int> StarCounts { get; set; } = new();
        public bool IsLoggedIn { get; set; }
        public bool IsParent { get; set; }
        public bool HasReviewed { get; set; }
        public bool IsOwnProfile { get; set; }
    }

    public class HomeIndexViewModel
    {
        public int TutorCount { get; set; }
        public int VerifiedCount { get; set; }
        public int ReviewCount { get; set; }
        public int ClassCount { get; set; }
        public double AverageRating { get; set; }
        public List<KhoiLop> Grades { get; set; } = new();
        public List<MonHoc> Subjects { get; set; } = new();
        public string? LastViewedTutor { get; set; }
        public string? LastViewedTutorId { get; set; }
    }

    // ============ GIA SƯ ============

    public class TutorProfileUpdateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn trình độ")]
        [RegularExpression("^(Sinh viên giỏi|Cử nhân|Thạc sĩ|Giáo viên chuyên)$", ErrorMessage = "Trình độ không hợp lệ")]
        [Display(Name = "Trình độ")]
        public string TrinhDo { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Tên trường tối đa 150 ký tự")]
        [Display(Name = "Trường đào tạo")]
        public string? TruongDaoTao { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mức học phí")]
        [Range(50000, 2000000, ErrorMessage = "Học phí/giờ từ 50.000đ đến 2.000.000đ")]
        [Display(Name = "Học phí / giờ")]
        public decimal HocPhiTheoGio { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khu vực dạy")]
        [StringLength(100, ErrorMessage = "Khu vực dạy tối đa 100 ký tự")]
        [Display(Name = "Khu vực dạy")]
        public string KhuVucDay { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn hình thức dạy")]
        [RegularExpression("^(Offline|Online|Cả hai)$", ErrorMessage = "Hình thức dạy không hợp lệ")]
        [Display(Name = "Hình thức dạy")]
        public string HinhThucDay { get; set; } = "Cả hai";

        [Required(ErrorMessage = "Vui lòng nhập phần giới thiệu bản thân")]
        [Display(Name = "Giới thiệu bản thân")]
        public string GioiThieuBio { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Thành tích tối đa 500 ký tự")]
        [Display(Name = "Thành tích nổi bật")]
        public string? ThanhTichNoiBat { get; set; }

        public IFormFile? FileAnhBangCap { get; set; }
        public string? AnhBangCapHienTai { get; set; }
    }

    public class ClassScheduleViewModel : IValidatableObject
    {
        public int MaLichHoc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên học sinh")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên học sinh từ 2 đến 100 ký tự")]
        [Display(Name = "Tên học sinh")]
        public string TenHocSinh { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public int MaMonHoc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày học")]
        [Display(Name = "Ngày học tiếp theo")]
        public DateOnly NgayHocTiepTheo { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu")]
        [Display(Name = "Giờ bắt đầu")]
        public TimeOnly GioBatDau { get; set; } = new TimeOnly(19, 0);

        [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc")]
        [Display(Name = "Giờ kết thúc")]
        public TimeOnly GioKetThuc { get; set; } = new TimeOnly(20, 30);

        [Required(ErrorMessage = "Vui lòng nhập phòng học hoặc link học online")]
        [StringLength(255, ErrorMessage = "Tối đa 255 ký tự")]
        [Display(Name = "Phòng học / Link Meet")]
        public string PhongHocHoacLink { get; set; } = string.Empty;

        [Range(0, 5000000, ErrorMessage = "Học phí mỗi ca từ 0 đến 5.000.000đ")]
        [Display(Name = "Học phí / ca")]
        public decimal HocPhiCaDay { get; set; } = 250000;

        [Display(Name = "Nhắc trước 30 phút")]
        public bool BatNhacNhoTruoc30P { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [RegularExpression("^(Đang học|Tạm nghỉ|Đã kết thúc)$", ErrorMessage = "Trạng thái không hợp lệ")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đang học";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (GioKetThuc <= GioBatDau)
                yield return new ValidationResult("Giờ kết thúc phải sau giờ bắt đầu", new[] { nameof(GioKetThuc) });
        }
    }

    public class AssignmentViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn lớp học")]
        [Display(Name = "Lớp học")]
        public int MaLichHoc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài tập")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tiêu đề từ 3 đến 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Display(Name = "Nội dung")]
        public string? NoiDungBaiTap { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn nộp")]
        [FutureDate(ErrorMessage = "Hạn nộp phải sau thời điểm hiện tại", MinHoursAhead = 0)]
        [Display(Name = "Hạn nộp")]
        public DateTime HanNop { get; set; } = DateTime.Now.AddDays(3);
    }

    public class TutorDashboardViewModel
    {
        public HoSoGiaSu Tutor { get; set; } = null!;
        public List<LopHocThoiKhoaBieu> Classes { get; set; } = new();
        public List<BaiTap> PendingAssignments { get; set; } = new();
        public int PendingBookings { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int AssignmentsDone { get; set; }
        public int AssignmentsTotal { get; set; }
    }

    // ============ DÙNG CHUNG ============

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
    }

    public class PagerViewModel
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string Action { get; set; } = "Index";
        public string? Controller { get; set; }
        public string? Area { get; set; }
        public Dictionary<string, string?> Query { get; set; } = new();
    }
}
