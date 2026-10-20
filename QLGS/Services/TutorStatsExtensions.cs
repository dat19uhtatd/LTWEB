using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Models.Entities;

namespace QLGiaSuLopHoc.Services
{
    /// <summary>Các hàm tính lại số liệu thống kê phi chuẩn hóa lưu trong bảng HoSoGiaSu.</summary>
    public static class TutorStatsExtensions
    {
        /// <summary>Tính lại điểm trung bình & tổng số đánh giá (chỉ tính đánh giá đang hiển thị).</summary>
        public static async Task RecalculateRatingAsync(this QlGiaSuLopHocContext db, int maGiaSu)
        {
            var stars = await db.DanhGia
                .Where(d => d.MaGiaSu == maGiaSu && d.HienThi == true)
                .Select(d => d.SoSao)
                .ToListAsync();

            var tutor = await db.HoSoGiaSu.FindAsync(maGiaSu);
            if (tutor == null) return;

            tutor.TongSoDanhGia = stars.Count;
            tutor.DiemDanhGiaTb = stars.Count == 0 ? 0 : Math.Round(stars.Average(), 1);
            await db.SaveChangesAsync();
        }

        /// <summary>Đếm lại số lớp đang dạy của gia sư.</summary>
        public static async Task RecalculateClassCountAsync(this QlGiaSuLopHocContext db, int maGiaSu)
        {
            var count = await db.LopHocThoiKhoaBieu
                .CountAsync(l => l.MaGiaSu == maGiaSu && l.TrangThai == "Đang học");
            var tutor = await db.HoSoGiaSu.FindAsync(maGiaSu);
            if (tutor == null) return;
            tutor.SoLopDangDay = count;
            await db.SaveChangesAsync();
        }
    }
}
