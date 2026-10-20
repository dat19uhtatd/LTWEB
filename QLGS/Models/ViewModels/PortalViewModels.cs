using QLGiaSuLopHoc.Models.Entities;

namespace QLGiaSuLopHoc.Models.ViewModels
{
    /// <summary>Dữ liệu cho partial _Avatar (ảnh đại diện hoặc chữ cái đầu).</summary>
    public record AvatarModel(string Name, string? Url, int Seed, int Size = 48);

    public class ParentDashboardViewModel
    {
        public List<DangKyHocThu> Bookings { get; set; } = new();
        public List<DanhGia> Reviews { get; set; } = new();
    }

    public class TutorProfilePageViewModel
    {
        public HoSoGiaSu Tutor { get; set; } = null!;
        public TutorProfileUpdateViewModel Form { get; set; } = new();
        public List<MonHoc> Subjects { get; set; } = new();
        public List<KhoiLop> Grades { get; set; } = new();
        /// <summary>Tập các cặp "maMon_maKhoi" gia sư đang dạy.</summary>
        public HashSet<string> SelectedPairs { get; set; } = new();
    }

    public class AssignmentsPageViewModel
    {
        public List<BaiTap> Items { get; set; } = new();
        public List<LopHocThoiKhoaBieu> Classes { get; set; } = new();
        public AssignmentViewModel Form { get; set; } = new();
        public string Filter { get; set; } = "all";
    }

    public class ClassFormPageViewModel
    {
        public ClassScheduleViewModel Form { get; set; } = new();
        public List<MonHoc> Subjects { get; set; } = new();
        public bool IsEdit { get; set; }
    }

    public class BookingsPageViewModel
    {
        public List<DangKyHocThu> Items { get; set; } = new();
        public string Status { get; set; } = "all";
        public int PendingCount { get; set; }
    }

    // ===== ADMIN =====
    public class AdminDashboardViewModel
    {
        public int TotalTutors { get; set; }
        public int VerifiedTutors { get; set; }
        public int PendingTutors { get; set; }
        public int TotalParents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public Dictionary<string, int> BookingsByStatus { get; set; } = new();
        public List<string> LastDaysLabels { get; set; } = new();
        public List<int> LastDaysCounts { get; set; } = new();
        public Dictionary<string, int> TutorsBySubject { get; set; } = new();
        public List<HoSoGiaSu> TutorsToVerify { get; set; } = new();
        public List<DangKyHocThu> RecentBookings { get; set; } = new();
    }

    public class SubjectFormViewModel
    {
        public int MaMonHoc { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tên môn học")]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Tên môn học tối đa 100 ký tự")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Tên môn học")]
        public string TenMonHoc { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(50, ErrorMessage = "Tối đa 50 ký tự")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^fa-[a-z0-9-]+$", ErrorMessage = "Icon phải có dạng fa-xxx (Font Awesome), ví dụ fa-calculator")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Icon Font Awesome")]
        public string? IconClass { get; set; } = "fa-book";

        [System.ComponentModel.DataAnnotations.StringLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
    }

    public class GradeFormViewModel
    {
        public int MaKhoiLop { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tên khối lớp")]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Tên khối lớp tối đa 100 ký tự")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Tên khối lớp")]
        public string TenKhoiLop { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(50, ErrorMessage = "Tối đa 50 ký tự")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Cấp học")]
        public string? CapHoc { get; set; }
    }
}
