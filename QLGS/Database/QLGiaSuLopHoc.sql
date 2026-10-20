/* =====================================================================
   EduMatch UTC - Script tạo CSDL QLGiaSuLopHoc (SQL Server)
   Cách dùng: mở bằng SSMS -> Execute (F5).
   LƯU Ý: Có thể BỎ QUA script này - ứng dụng tự tạo CSDL + dữ liệu mẫu
   ở lần chạy đầu tiên (Seed:Enabled = true trong appsettings.json).
   Mật khẩu các tài khoản mẫu: 123456 (lưu MD5 cũ; hệ thống tự nâng cấp
   lên PBKDF2 ngay lần đăng nhập đầu tiên).
   ===================================================================== */
IF DB_ID(N'QLGiaSuLopHoc') IS NULL CREATE DATABASE QLGiaSuLopHoc;
GO
USE QLGiaSuLopHoc;
GO

CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15) NOT NULL,
    MatKhauHash VARCHAR(256) NOT NULL,
    AnhDaiDien NVARCHAR(255) NULL DEFAULT '/images/avatars/default.png',
    VaiTro VARCHAR(20) NOT NULL,                 -- Admin | Tutor | Parent
    NhanThongBaoDesktop BIT NULL DEFAULT 1,
    NgayTao DATETIME NULL DEFAULT GETDATE(),
    TrangThai BIT NULL DEFAULT 1
);

CREATE TABLE MonHoc (
    MaMonHoc INT IDENTITY(1,1) PRIMARY KEY,
    TenMonHoc NVARCHAR(100) NOT NULL,
    IconClass VARCHAR(50) NULL DEFAULT 'fa-book',
    MoTa NVARCHAR(255) NULL
);

CREATE TABLE KhoiLop (
    MaKhoiLop INT IDENTITY(1,1) PRIMARY KEY,
    TenKhoiLop NVARCHAR(100) NOT NULL,
    CapHoc NVARCHAR(50) NULL
);

CREATE TABLE HoSoGiaSu (
    MaGiaSu INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT NOT NULL UNIQUE REFERENCES NguoiDung(MaNguoiDung),
    TrinhDo NVARCHAR(50) NOT NULL,
    TruongDaoTao NVARCHAR(150) NULL,
    HocPhiTheoGio DECIMAL(18,0) NOT NULL,
    KhuVucDay NVARCHAR(100) NOT NULL,
    HinhThucDay NVARCHAR(50) NOT NULL,           -- Offline | Online | Cả hai
    GioiThieuBio NVARCHAR(MAX) NULL,
    ThanhTichNoiBat NVARCHAR(MAX) NULL,
    AnhBangCapTheSv NVARCHAR(255) NULL,
    DaXacMinh BIT NULL DEFAULT 0,
    DiemDanhGiaTb FLOAT NULL DEFAULT 5,
    TongSoDanhGia INT NULL DEFAULT 0,
    SoLopDangDay INT NULL DEFAULT 0
);

CREATE TABLE GiaSuMonHoc (
    MaGiaSuMonHoc INT IDENTITY(1,1) PRIMARY KEY,
    MaGiaSu INT NOT NULL REFERENCES HoSoGiaSu(MaGiaSu),
    MaMonHoc INT NOT NULL REFERENCES MonHoc(MaMonHoc),
    MaKhoiLop INT NOT NULL REFERENCES KhoiLop(MaKhoiLop)
);

CREATE TABLE MaTranLichRanh (
    MaLichRanh INT IDENTITY(1,1) PRIMARY KEY,
    MaGiaSu INT NOT NULL REFERENCES HoSoGiaSu(MaGiaSu),
    ThuTrongTuan NVARCHAR(20) NOT NULL,
    BuoiTrongNgay NVARCHAR(20) NOT NULL,
    KhungGioChiTiet VARCHAR(50) NOT NULL,
    ConTrong BIT NULL DEFAULT 1
);

CREATE TABLE LopHocThoiKhoaBieu (
    MaLichHoc INT IDENTITY(1,1) PRIMARY KEY,
    MaGiaSu INT NOT NULL REFERENCES HoSoGiaSu(MaGiaSu),
    MaMonHoc INT NOT NULL REFERENCES MonHoc(MaMonHoc),
    TenHocSinh NVARCHAR(100) NOT NULL,
    ThuTrongTuan NVARCHAR(20) NOT NULL,
    NgayHocTiepTheo DATE NOT NULL,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    PhongHocHoacLink NVARCHAR(255) NOT NULL,
    HocPhiCaDay DECIMAL(18,0) NOT NULL,
    BatNhacNhoTruoc30P BIT NULL DEFAULT 1,
    TrangThai NVARCHAR(30) NULL DEFAULT N'Đang học'
);

CREATE TABLE BaiTap (
    MaBaiTap INT IDENTITY(1,1) PRIMARY KEY,
    MaLichHoc INT NOT NULL REFERENCES LopHocThoiKhoaBieu(MaLichHoc),
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDungBaiTap NVARCHAR(MAX) NULL,
    HanNop DATETIME NOT NULL,
    TrangThai VARCHAR(20) NULL DEFAULT 'Pending',  -- Pending | Completed
    NgayGiao DATETIME NULL DEFAULT GETDATE()
);

CREATE TABLE DangKyHocThu (
    MaDangKy INT IDENTITY(1,1) PRIMARY KEY,
    MaGiaSu INT NOT NULL REFERENCES HoSoGiaSu(MaGiaSu),
    MaPhuHuynh INT NULL REFERENCES NguoiDung(MaNguoiDung),
    HoTenPhuHuynh NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(15) NOT NULL,
    MaMonHoc INT NOT NULL REFERENCES MonHoc(MaMonHoc),
    TrinhDoHienTaiHS NVARCHAR(50) NOT NULL,
    KhungGioMongMuon DATETIME NOT NULL,
    GhiChuYeuCau NVARCHAR(500) NULL,
    TrangThai NVARCHAR(30) NULL DEFAULT N'Chờ duyệt', -- Chờ duyệt | Đã xác nhận | Từ chối | Đã hủy
    NgayTao DATETIME NULL DEFAULT GETDATE()
);

CREATE TABLE DanhGia (
    MaDanhGia INT IDENTITY(1,1) PRIMARY KEY,
    MaGiaSu INT NOT NULL REFERENCES HoSoGiaSu(MaGiaSu),
    MaPhuHuynh INT NOT NULL REFERENCES NguoiDung(MaNguoiDung),
    SoSao INT NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
    NoiDungBinhLuan NVARCHAR(500) NOT NULL,
    NgayDanhGia DATETIME NULL DEFAULT GETDATE(),
    HienThi BIT NULL DEFAULT 1
);
GO

/* ---------- Dữ liệu tối thiểu (mật khẩu 123456 - MD5) ---------- */
INSERT INTO MonHoc (TenMonHoc, IconClass, MoTa) VALUES
 (N'Toán học','fa-calculator',N'Đại số, hình học, giải tích'),
 (N'Tiếng Anh & IELTS','fa-language',N'Giao tiếp, ngữ pháp, luyện thi IELTS'),
 (N'Vật lý','fa-bolt',N'Cơ, nhiệt, điện, quang'),
 (N'Hóa học','fa-flask',N'Hóa vô cơ, hữu cơ'),
 (N'Ngữ Văn','fa-book-open',N'Đọc hiểu, nghị luận'),
 (N'Tin học & Lập trình','fa-laptop-code',N'Python, C++, thuật toán'),
 (N'Sinh học','fa-dna',N'Tế bào, di truyền');

INSERT INTO KhoiLop (TenKhoiLop, CapHoc) VALUES
 (N'Lớp 1',N'Tiểu học'),(N'Lớp 2',N'Tiểu học'),(N'Lớp 3',N'Tiểu học'),(N'Lớp 4',N'Tiểu học'),(N'Lớp 5',N'Tiểu học'),
 (N'Lớp 6',N'THCS'),(N'Lớp 7',N'THCS'),(N'Lớp 8',N'THCS'),(N'Lớp 9',N'THCS'),
 (N'Lớp 10',N'THPT'),(N'Lớp 11',N'THPT'),(N'Lớp 12',N'THPT'),(N'Luyện thi Đại học',N'Luyện thi');

INSERT INTO NguoiDung (HoTen, Email, SoDienThoai, MatKhauHash, VaiTro) VALUES
 (N'Quản trị viên UTC','admin@utc.edu.vn','0900000001','e10adc3949ba59abbe56e057f20f883e','Admin'),
 (N'Nguyễn Minh Tuấn','tuan.giasu@utc.edu.vn','0911000001','e10adc3949ba59abbe56e057f20f883e','Tutor'),
 (N'Phạm Thị Lan','lan.phuhuynh@gmail.com','0912345678','e10adc3949ba59abbe56e057f20f883e','Parent');

INSERT INTO HoSoGiaSu (MaNguoiDung, TrinhDo, TruongDaoTao, HocPhiTheoGio, KhuVucDay, HinhThucDay, GioiThieuBio, ThanhTichNoiBat, DaXacMinh, DiemDanhGiaTb, TongSoDanhGia, SoLopDangDay)
VALUES (2, N'Sinh viên giỏi', N'ĐH Giao thông Vận tải (CNTT)', 180000, N'Cầu Giấy, Đống Đa', N'Cả hai',
        N'<p>Sinh viên CNTT UTC, dạy Toán và lập trình cho học sinh THCS - THPT.</p>',
        N'Điểm thi ĐH Toán 9.4 • Giải Nhì Olympic Tin học SV', 1, 0, 0, 0);

INSERT INTO GiaSuMonHoc (MaGiaSu, MaMonHoc, MaKhoiLop) VALUES (1,1,10),(1,1,11),(1,1,12),(1,6,10),(1,6,11);
GO
