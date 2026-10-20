using System.Globalization;

namespace QLGiaSuLopHoc.Helpers
{
    /// <summary>Các hàm định dạng tiếng Việt dùng chung cho View và Controller.</summary>
    public static class VnFormat
    {
        public static readonly CultureInfo Vi = new("vi-VN");

        public static readonly string[] Days = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        public static readonly string[] Sessions = { "Sáng", "Chiều", "Tối" };

        public static readonly Dictionary<string, string> SessionTimes = new()
        {
            ["Sáng"] = "08:00 - 11:30",
            ["Chiều"] = "14:00 - 17:30",
            ["Tối"] = "18:30 - 21:00"
        };

        /// <summary>180000 → "180.000đ"</summary>
        public static string Vnd(this decimal value) => value.ToString("N0", Vi) + "đ";

        /// <summary>Định dạng gọn: 1.250.000 → "1,25 tr"</summary>
        public static string VndShort(this decimal value)
        {
            if (value >= 1_000_000m) return (value / 1_000_000m).ToString("0.##", Vi) + " triệu";
            if (value >= 1_000m) return (value / 1_000m).ToString("0.#", Vi) + "k";
            return value.ToString("N0", Vi);
        }

        public static string ThuTuNgay(DateOnly d) => d.DayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            _ => "Chủ nhật"
        };

        /// <summary>Chuẩn hóa "thứ 2", "T2", "Thứ Hai"... về khóa chuẩn "Thứ 2".."Chủ nhật" để so khớp ma trận lịch.</summary>
        public static string NormalizeDay(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            var s = raw.Trim().ToLowerInvariant();
            if (s.Contains("chủ nhật") || s == "cn" || s.Contains("chu nhat")) return "Chủ nhật";
            foreach (var ch in s)
            {
                if (ch >= '2' && ch <= '7') return "Thứ " + ch;
            }
            return raw.Trim();
        }

        public static string NormalizeSession(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            var s = raw.Trim().ToLowerInvariant();
            if (s.StartsWith("sáng") || s.StartsWith("sang")) return "Sáng";
            if (s.StartsWith("chiều") || s.StartsWith("chieu")) return "Chiều";
            if (s.StartsWith("tối") || s.StartsWith("toi")) return "Tối";
            return raw.Trim();
        }

        /// <summary>Thứ tự sắp xếp 0..6 cho Thứ 2..Chủ nhật (dùng để sort thời khóa biểu).</summary>
        public static int DayOrder(string? raw)
        {
            var idx = Array.IndexOf(Days, NormalizeDay(raw));
            return idx < 0 ? 99 : idx;
        }

        public static string Initials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }

        private static readonly string[] AvatarColors =
            { "#2563eb", "#059669", "#7c3aed", "#d97706", "#db2777", "#0891b2", "#4f46e5", "#dc2626" };

        public static string AvatarColor(int seed) => AvatarColors[Math.Abs(seed) % AvatarColors.Length];

        public static string BookingBadge(string? status) => status switch
        {
            "Chờ duyệt" => "bg-warning-subtle text-warning-emphasis",
            "Đã xác nhận" => "bg-success-subtle text-success-emphasis",
            "Từ chối" => "bg-danger-subtle text-danger-emphasis",
            "Đã hủy" => "bg-secondary-subtle text-secondary-emphasis",
            _ => "bg-info-subtle text-info-emphasis"
        };

        public static string TimeAgo(DateTime? time)
        {
            if (time == null) return "";
            var span = DateTime.Now - time.Value;
            if (span.TotalMinutes < 1) return "vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays} ngày trước";
            return time.Value.ToString("dd/MM/yyyy");
        }
    }
}
