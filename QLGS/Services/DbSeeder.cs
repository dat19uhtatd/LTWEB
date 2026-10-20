using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;

namespace QLGiaSuLopHoc.Services
{
    /// <summary>
    /// Tự tạo CSDL (nếu chưa có) và nạp dữ liệu mẫu để chạy/chấm bài ngay.
    /// Chỉ nạp khi bảng NguoiDung còn trống nên KHÔNG ghi đè dữ liệu sẵn có.
    /// Mật khẩu tất cả tài khoản mẫu: 123456
    /// </summary>
    public class DbSeeder
    {
        private readonly QlGiaSuLopHocContext _db;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(QlGiaSuLopHocContext db, ILogger<DbSeeder> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _db.Database.EnsureCreatedAsync();
            if (await _db.NguoiDung.AnyAsync()) return;

            _logger.LogInformation("CSDL còn trống - đang nạp dữ liệu mẫu...");
            string pw = PasswordService.Hash("123456");

            // ---------- Danh mục ----------
            var toan = new MonHoc { TenMonHoc = "Toán học", IconClass = "fa-calculator", MoTa = "Đại số, hình học, giải tích; luyện thi vào 10 và THPT Quốc gia" };
            var anh = new MonHoc { TenMonHoc = "Tiếng Anh & IELTS", IconClass = "fa-language", MoTa = "Giao tiếp, ngữ pháp, luyện thi IELTS/TOEIC" };
            var ly = new MonHoc { TenMonHoc = "Vật lý", IconClass = "fa-bolt", MoTa = "Cơ, nhiệt, điện, quang; bồi dưỡng học sinh giỏi" };
            var hoa = new MonHoc { TenMonHoc = "Hóa học", IconClass = "fa-flask", MoTa = "Hóa vô cơ, hữu cơ; luyện thi khối A, B" };
            var van = new MonHoc { TenMonHoc = "Ngữ Văn", IconClass = "fa-book-open", MoTa = "Đọc hiểu, nghị luận xã hội, nghị luận văn học" };
            var tin = new MonHoc { TenMonHoc = "Tin học & Lập trình", IconClass = "fa-laptop-code", MoTa = "Tin học văn phòng, Python, C++, thuật toán" };
            var sinh = new MonHoc { TenMonHoc = "Sinh học", IconClass = "fa-dna", MoTa = "Sinh học tế bào, di truyền, tiến hóa" };
            _db.MonHoc.AddRange(toan, anh, ly, hoa, van, tin, sinh);

            var grades = new List<KhoiLop>();
            for (int i = 1; i <= 12; i++)
            {
                grades.Add(new KhoiLop
                {
                    TenKhoiLop = $"Lớp {i}",
                    CapHoc = i <= 5 ? "Tiểu học" : i <= 9 ? "THCS" : "THPT"
                });
            }
            grades.Add(new KhoiLop { TenKhoiLop = "Luyện thi Đại học", CapHoc = "Luyện thi" });
            _db.KhoiLop.AddRange(grades);
            KhoiLop[] G(int from, int to) => grades.Skip(from - 1).Take(to - from + 1).ToArray();

            // ---------- Người dùng ----------
            var admin = new NguoiDung { HoTen = "Quản trị viên UTC", Email = "admin@utc.edu.vn", SoDienThoai = "0900000001", MatKhauHash = pw, VaiTro = Roles.Admin, NhanThongBaoDesktop = true, NgayTao = DateTime.Now.AddDays(-60), TrangThai = true };
            var lan = new NguoiDung { HoTen = "Phạm Thị Lan", Email = "lan.phuhuynh@gmail.com", SoDienThoai = "0912345678", MatKhauHash = pw, VaiTro = Roles.Parent, NhanThongBaoDesktop = true, NgayTao = DateTime.Now.AddDays(-30), TrangThai = true };
            var hung = new NguoiDung { HoTen = "Nguyễn Văn Hùng", Email = "hung.phuhuynh@gmail.com", SoDienThoai = "0987654321", MatKhauHash = pw, VaiTro = Roles.Parent, NhanThongBaoDesktop = true, NgayTao = DateTime.Now.AddDays(-25), TrangThai = true };
            var mai = new NguoiDung { HoTen = "Trần Thị Mai", Email = "mai.phuhuynh@gmail.com", SoDienThoai = "0905123456", MatKhauHash = pw, VaiTro = Roles.Parent, NhanThongBaoDesktop = true, NgayTao = DateTime.Now.AddDays(-20), TrangThai = true };
            _db.NguoiDung.AddRange(admin, lan, hung, mai);

            HoSoGiaSu Tutor(string name, string email, string phone, string level, string school, decimal fee, string area, string mode,
                            string bio, string achievements, bool verified, string? cert)
            {
                var t = new HoSoGiaSu
                {
                    MaNguoiDungNavigation = new NguoiDung
                    {
                        HoTen = name, Email = email, SoDienThoai = phone, MatKhauHash = pw, VaiTro = Roles.Tutor,
                        NhanThongBaoDesktop = true, NgayTao = DateTime.Now.AddDays(-45), TrangThai = true
                    },
                    TrinhDo = level, TruongDaoTao = school, HocPhiTheoGio = fee, KhuVucDay = area, HinhThucDay = mode,
                    GioiThieuBio = bio, ThanhTichNoiBat = achievements, DaXacMinh = verified, AnhBangCapTheSv = cert,
                    DiemDanhGiaTb = 0, TongSoDanhGia = 0, SoLopDangDay = 0
                };
                _db.HoSoGiaSu.Add(t);
                return t;
            }

            var tuan = Tutor("Nguyễn Minh Tuấn", "tuan.giasu@utc.edu.vn", "0911000001", "Sinh viên giỏi", "ĐH Giao thông Vận tải (CNTT)", 180000, "Cầu Giấy, Đống Đa", "Cả hai",
                "<p>Mình là sinh viên năm 3 ngành <strong>Công nghệ thông tin</strong> tại UTC, đam mê giảng dạy Toán và lập trình cho học sinh THCS - THPT.</p><ul><li>Phương pháp: <em>hiểu bản chất - luyện đề - sửa lỗi sai</em></li><li>Có giáo án riêng cho từng học sinh, báo cáo tiến độ hàng tuần cho phụ huynh</li><li>Dạy lập trình Python/C++ từ cơ bản đến thuật toán</li></ul>",
                "Điểm thi ĐH Toán 9.4 • Giải Nhì Olympic Tin học SV • GPA 3.6/4.0", true, null);
            var ha = Tutor("Trần Thu Hà", "ha.giasu@utc.edu.vn", "0911000002", "Giáo viên chuyên", "ĐH Ngoại ngữ - ĐHQGHN", 300000, "Online toàn quốc", "Online",
                "<p>5 năm kinh nghiệm luyện thi <strong>IELTS</strong> cấp tốc. Lộ trình cá nhân hóa 4 kỹ năng, chấm chữa Writing/Speaking chi tiết sau mỗi buổi.</p><p>Học viên của cô đạt trung bình <strong>band 7.0+</strong> sau 3 tháng.</p>",
                "IELTS 8.5 Overall • 5 năm luyện thi IELTS cấp tốc", true, null);
            var nam = Tutor("Lê Hoàng Nam", "nam.giasu@utc.edu.vn", "0911000003", "Cử nhân", "ĐH Sư phạm Hà Nội", 220000, "Thanh Xuân, Hà Đông", "Offline",
                "<p>Cử nhân Sư phạm Vật lý loại Giỏi, từng đạt giải Nhất HSG cấp Tỉnh. Dạy kèm Vật lý & Toán bám sát chương trình, tập trung rèn tư duy giải bài tập.</p>",
                "Giải Nhất HSG Vật lý cấp Tỉnh • Tốt nghiệp Giỏi SP Vật lý", true, null);
            var minh = Tutor("Hoàng Văn Minh", "minh.giasu@utc.edu.vn", "0911000004", "Giáo viên chuyên", "ĐH Sư phạm Hà Nội", 250000, "Cầu Giấy, Nam Từ Liêm", "Cả hai",
                "<p>Hơn <strong>10 năm</strong> kinh nghiệm luyện thi đại học khối A, A1. Chuyên Toán - Hóa nâng cao, bồi dưỡng mục tiêu 9+.</p>",
                "10 năm kinh nghiệm luyện thi ĐH khối A, A1", true, null);
            var linh = Tutor("Nguyễn Mai Linh", "linh.giasu@utc.edu.vn", "0911000005", "Cử nhân", "ĐH KHXH & Nhân văn", 180000, "Đống Đa, Ba Đình", "Cả hai",
                "<p>Giúp học sinh yêu Văn: rèn kỹ năng đọc hiểu, lập dàn ý và viết nghị luận mạch lạc, giàu cảm xúc.</p>",
                "Giải Nhì HSG Quốc gia môn Ngữ Văn", true, null);
            var khuong = Tutor("Vũ Đình Khương", "khuong.giasu@utc.edu.vn", "0911000006", "Giáo viên chuyên", "ĐH Bách Khoa Hà Nội", 280000, "Thanh Xuân, Hoàng Mai", "Offline",
                "<p>Thạc sĩ Vật lý, nhiều năm bồi dưỡng đội tuyển HSG. Phương pháp dạy tinh gọn, trực quan, nhiều thí nghiệm minh họa.</p>",
                "Thạc sĩ Vật lý ĐH Bách Khoa", true, null);
            var bao = Tutor("Phạm Quốc Bảo", "bao.giasu@utc.edu.vn", "0911000007", "Sinh viên giỏi", "ĐH Y Hà Nội", 120000, "Hà Đông", "Cả hai",
                "<p>Sinh viên Y khoa năm 2, nhận dạy Sinh học và Hóa học cho học sinh THCS - THPT. <em>Hồ sơ đang chờ xác minh thẻ sinh viên.</em></p>",
                "Giải Ba HSG Sinh học cấp Thành phố", false, null);

            // ---------- Môn dạy ----------
            void Teach(HoSoGiaSu t, MonHoc m, params KhoiLop[] ks)
            {
                foreach (var k in ks) _db.GiaSuMonHoc.Add(new GiaSuMonHoc { MaGiaSuNavigation = t, MaMonHocNavigation = m, MaKhoiLopNavigation = k });
            }
            Teach(tuan, toan, G(6, 12)); Teach(tuan, tin, G(9, 12));
            Teach(ha, anh, G(6, 13));
            Teach(nam, ly, G(8, 12)); Teach(nam, toan, G(8, 9));
            Teach(minh, toan, G(9, 13)); Teach(minh, hoa, G(10, 13));
            Teach(linh, van, G(6, 12));
            Teach(khuong, ly, G(9, 13)); Teach(khuong, toan, G(10, 12));
            Teach(bao, sinh, G(8, 12)); Teach(bao, hoa, G(8, 10));

            // ---------- Ma trận lịch rảnh ----------
            var tutors = new[] { tuan, ha, nam, minh, linh, khuong, bao };
            for (int ti = 0; ti < tutors.Length; ti++)
            {
                for (int d = 0; d < VnFormat.Days.Length; d++)
                {
                    for (int s = 0; s < VnFormat.Sessions.Length; s++)
                    {
                        _db.MaTranLichRanh.Add(new MaTranLichRanh
                        {
                            MaGiaSuNavigation = tutors[ti],
                            ThuTrongTuan = VnFormat.Days[d],
                            BuoiTrongNgay = VnFormat.Sessions[s],
                            KhungGioChiTiet = VnFormat.SessionTimes[VnFormat.Sessions[s]],
                            ConTrong = (d * 3 + s + ti) % 3 != 0
                        });
                    }
                }
            }

            // ---------- Lớp học / thời khóa biểu ----------
            DateOnly Next(DayOfWeek dow)
            {
                var d = DateOnly.FromDateTime(DateTime.Today);
                while (d.DayOfWeek != dow) d = d.AddDays(1);
                return d;
            }
            LopHocThoiKhoaBieu Class(HoSoGiaSu t, MonHoc m, string student, DayOfWeek dow, int h1, int m1, int h2, int m2, string room, decimal fee)
            {
                var date = Next(dow);
                var c = new LopHocThoiKhoaBieu
                {
                    MaGiaSuNavigation = t, MaMonHocNavigation = m, TenHocSinh = student,
                    NgayHocTiepTheo = date, ThuTrongTuan = VnFormat.ThuTuNgay(date),
                    GioBatDau = new TimeOnly(h1, m1), GioKetThuc = new TimeOnly(h2, m2),
                    PhongHocHoacLink = room, HocPhiCaDay = fee, BatNhacNhoTruoc30P = true, TrangThai = "Đang học"
                };
                _db.LopHocThoiKhoaBieu.Add(c);
                return c;
            }

            var c1 = Class(tuan, toan, "Nguyễn Gia Bảo", DayOfWeek.Monday, 19, 0, 20, 30, "Phòng 302 - 12 Trần Thái Tông", 270000);
            var c2 = Class(tuan, tin, "Lê Thùy Chi", DayOfWeek.Wednesday, 19, 0, 20, 30, "https://meet.google.com/abc-defg-hij", 300000);
            var c3 = Class(tuan, toan, "Phạm Quang Huy", DayOfWeek.Friday, 18, 30, 20, 0, "Tại nhà - 25 Dịch Vọng Hậu", 270000);
            var c4 = Class(tuan, toan, "Trần Khánh An", DayOfWeek.Saturday, 9, 0, 10, 30, "Thư viện UTC - Phòng học nhóm 2", 250000);
            var c5 = Class(tuan, tin, "Vũ Minh Đức", DayOfWeek.Sunday, 15, 0, 16, 30, "https://meet.google.com/xyz-uvwx-rst", 300000);
            var c6 = Class(ha, anh, "Đỗ Thảo Nguyên", DayOfWeek.Tuesday, 20, 0, 21, 30, "https://zoom.us/j/123456789", 450000);
            var c7 = Class(ha, anh, "Bùi Hải Yến", DayOfWeek.Thursday, 20, 0, 21, 30, "https://zoom.us/j/987654321", 450000);
            var c8 = Class(nam, ly, "Lý Quốc Khánh", DayOfWeek.Tuesday, 18, 30, 20, 0, "Nhà học sinh - 8 Nguyễn Trãi", 330000);
            var c9 = Class(minh, hoa, "Hoàng Bảo Ngọc", DayOfWeek.Saturday, 14, 0, 16, 0, "Phòng 5 - Trung tâm Cầu Giấy", 500000);

            // ---------- Bài tập ----------
            BaiTap Hw(LopHocThoiKhoaBieu c, string title, string content, int dueDays, string status)
            {
                var b = new BaiTap
                {
                    MaLichHocNavigation = c, TieuDe = title, NoiDungBaiTap = content,
                    HanNop = DateTime.Today.AddDays(dueDays).AddHours(21), TrangThai = status, NgayGiao = DateTime.Now.AddDays(-2)
                };
                _db.BaiTap.Add(b);
                return b;
            }
            Hw(c1, "Bài tập Hàm số bậc hai", "Làm bài 1-10 trang 45 SGK và 5 bài nâng cao do gia sư giao.", 2, "Pending");
            Hw(c1, "Đề kiểm tra Chương 1", "Hoàn thành đề 45 phút, chụp ảnh bài làm gửi gia sư.", -1, "Completed");
            Hw(c2, "Viết chương trình Python tính điểm TB", "Dùng vòng lặp for và danh sách để nhập/tính điểm trung bình lớp.", 3, "Pending");
            Hw(c3, "Ôn tập Hình học không gian", "Giải 8 bài về khoảng cách và thể tích khối chóp.", 4, "Pending");
            Hw(c5, "Bài toán thuật toán sắp xếp", "Cài đặt Bubble Sort và Quick Sort, so sánh thời gian chạy.", 5, "Pending");
            Hw(c6, "IELTS Writing Task 2 - Education", "Viết bài 250 từ về chủ đề 'Online learning'.", 2, "Pending");
            Hw(c8, "Bài tập Định luật Ôm", "Làm 10 bài trắc nghiệm và 3 bài tự luận.", 3, "Completed");

            // ---------- Đăng ký học thử ----------
            DangKyHocThu Book(HoSoGiaSu t, MonHoc m, NguoiDung? parent, string name, string phone, string level, int days, string status, string? note)
            {
                var b = new DangKyHocThu
                {
                    MaGiaSuNavigation = t, MaMonHocNavigation = m, MaPhuHuynhNavigation = parent,
                    HoTenPhuHuynh = name, SoDienThoai = phone, TrinhDoHienTaiHs = level,
                    KhungGioMongMuon = DateTime.Today.AddDays(days).AddHours(19), GhiChuYeuCau = note,
                    TrangThai = status, NgayTao = DateTime.Now.AddHours(-days * 7 - 3)
                };
                _db.DangKyHocThu.Add(b);
                return b;
            }
            Book(tuan, toan, lan, "Phạm Thị Lan", "0912345678", "Khá - Cần nâng cao", 2, "Chờ duyệt", "Cháu hay mất tập trung, cần gia sư kiên nhẫn.");
            Book(ha, anh, lan, "Phạm Thị Lan", "0912345678", "Giỏi - Ôn thi chuyên/ĐH", 3, "Đã xác nhận", "Mục tiêu IELTS 6.5 trong 6 tháng.");
            Book(nam, ly, null, "Chị Ngô Thu Hằng", "0936111222", "Trung bình", 4, "Chờ duyệt", null);
            Book(minh, hoa, hung, "Nguyễn Văn Hùng", "0987654321", "Mất gốc - Cần lấy lại căn bản", 1, "Từ chối", "Cần học vào buổi sáng cuối tuần.");
            Book(linh, van, mai, "Trần Thị Mai", "0905123456", "Khá - Cần nâng cao", 5, "Chờ duyệt", "Con học lớp 9, chuẩn bị thi vào 10.");

            await _db.SaveChangesAsync();

            // ---------- Đánh giá ----------
            void Review(HoSoGiaSu t, NguoiDung p, int stars, string text, int daysAgo)
            {
                _db.DanhGia.Add(new DanhGia
                {
                    MaGiaSuNavigation = t, MaPhuHuynhNavigation = p, SoSao = stars, NoiDungBinhLuan = text,
                    NgayDanhGia = DateTime.Now.AddDays(-daysAgo), HienThi = true
                });
            }
            Review(tuan, lan, 5, "Anh Tuấn dạy rất dễ hiểu, con mình tiến bộ rõ rệt chỉ sau 1 tháng. Rất đúng giờ và nhiệt tình!", 10);
            Review(tuan, hung, 5, "Gia sư trẻ nhưng rất có phương pháp, hay giao bài tập và chữa kỹ từng lỗi sai.", 8);
            Review(tuan, mai, 4, "Dạy tốt, con thích học. Mong anh có thêm nhiều ca vào cuối tuần.", 5);
            Review(ha, lan, 5, "Cô Hà chấm Writing cực kỳ chi tiết, band điểm của em tăng thấy rõ.", 12);
            Review(ha, hung, 5, "Lộ trình học rõ ràng, học online nhưng rất hiệu quả.", 9);
            Review(ha, mai, 5, "Cô rất tận tâm, bài giảng sinh động.", 4);
            Review(nam, mai, 5, "Thầy giảng Vật lý dễ hiểu, nhiều ví dụ thực tế.", 7);
            Review(nam, lan, 4, "Thầy nhiệt tình, bài tập vừa sức.", 6);
            Review(minh, hung, 5, "Thầy dạy chắc kiến thức, luyện đề rất sát cấu trúc thi.", 15);
            Review(linh, lan, 5, "Cô giúp con viết văn tự tin hơn nhiều.", 11);
            Review(linh, hung, 4, "Cô dạy hay, nhẹ nhàng, con không còn sợ môn Văn.", 3);
            Review(khuong, mai, 5, "Thầy có chuyên môn rất cao, bài giảng trực quan.", 14);
            await _db.SaveChangesAsync();

            foreach (var t in tutors)
            {
                await _db.RecalculateRatingAsync(t.MaGiaSu);
                await _db.RecalculateClassCountAsync(t.MaGiaSu);
            }

            _logger.LogInformation("Đã nạp xong dữ liệu mẫu. Tài khoản: admin@utc.edu.vn / tuan.giasu@utc.edu.vn / lan.phuhuynh@gmail.com - mật khẩu 123456");
        }
    }
}
