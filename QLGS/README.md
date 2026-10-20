# EduMatch UTC — Hệ thống Quản lý Gia sư & Lớp học

Bài tập lớn học phần **Lập trình Web** — ASP.NET Core MVC (.NET 10) • Entity Framework Core 8 • Web API • SQL Server.

## Chạy dự án
1. Cài **.NET 10 SDK** và **SQL Server / SQL Server Express**.
2. Sửa chuỗi kết nối `DefaultConnection` trong `appsettings.json` nếu máy bạn không dùng `.\SQLEXPRESS`.
3. Mở `QLGiaSuLopHoc.csproj` bằng Visual Studio 2022+ rồi nhấn **F5** (hoặc `dotnet run`).
4. Lần chạy đầu tiên, ứng dụng **tự tạo CSDL và nạp dữ liệu mẫu** (tắt bằng `Seed:Enabled=false`).
   Nếu muốn tạo thủ công, chạy `Database/QLGiaSuLopHoc.sql` trong SSMS.

## Tài khoản demo (mật khẩu `123456`)
| Vai trò | Email |
|---|---|
| Admin | admin@utc.edu.vn |
| Gia sư | tuan.giasu@utc.edu.vn |
| Phụ huynh | lan.phuhuynh@gmail.com |

## Chức năng
**Khách / Phụ huynh:** tìm kiếm & lọc gia sư (AJAX + Web API, phân trang, gợi ý tên), xem hồ sơ, ma trận lịch rảnh, đăng ký học thử 01 buổi, đánh giá 1–5 sao, theo dõi/hủy yêu cầu, đăng ký tài khoản, quản lý tài khoản.
**Gia sư:** dashboard (doanh thu, biểu đồ Chart.js), thời khóa biểu tuần (CRUD, chống trùng lịch), bài tập (giao/xóa/đổi trạng thái bằng AJAX), xử lý yêu cầu học thử, chỉnh ma trận lịch rảnh, hồ sơ năng lực với CKEditor 5, upload bằng cấp, chọn môn × khối lớp, nhắc lịch dạy trước 30 phút bằng Desktop Notification.
**Admin (Area):** dashboard thống kê + 3 biểu đồ, xác minh/Tích xanh gia sư (xem ảnh bằng cấp), điều phối đăng ký học thử, kiểm duyệt đánh giá, CRUD Môn học / Khối lớp, quản lý người dùng (khóa, đặt lại mật khẩu).

## Kiến thức môn học được áp dụng
MVC • Razor (Layout, Partial, ViewComponent `SubjectMenu`, Tag Helpers) • Model Binding & Validation (DataAnnotations, custom `[FutureDate]` có cả client-side) • EF Core (Include, AsSplitQuery, projection, phân trang) • Area • Filter (`RoleAuthorize`) • Session • Cookie (Remember me, gia sư xem gần nhất) • Web API REST • AJAX/Fetch • Upload file • Anti-forgery • Error handling.

## Các cải tiến so với bản cũ
- Trang chủ nay lấy dữ liệu thật từ DB qua API (trước đây là mảng JS cứng, link chi tiết không khớp DB).
- Sửa lỗi view của Area Admin đặt sai thư mục; thêm action `Error`, trang 404/403.
- Bảo mật: PBKDF2 thay MD5 (tự nâng cấp tài khoản cũ), chống XSS cho Bio (`SafeHtml`), chống CSRF (kể cả AJAX), kiểm tra quyền sở hữu mọi thao tác (chống IDOR), khóa đăng nhập sai nhiều lần, kiểm tra chữ ký file upload, bỏ chuỗi kết nối hard-code.
- Bỏ các fallback nguy hiểm `?? 5`, `?? 2` (người lạ không còn đánh giá/xem hồ sơ thay người khác).
- Thêm đăng ký, hồ sơ tài khoản, khu vực phụ huynh, toàn bộ khu Admin, seed dữ liệu.

## Bản sửa lỗi (NangCap_2)
- Phiên đăng nhập được đối chiếu lại với CSDL: tài khoản bị khóa/xóa hoặc đổi vai trò sẽ mất hiệu lực ngay (trước đây vẫn dùng được tới khi hết phiên 60 phút).
- Sửa liên kết "Đăng nhập" trên trang chi tiết gia sư sau khi gửi form bị lỗi (trước đây quay về đường dẫn POST → 404).
- Trang lỗi `Error`/`HttpStatus` không còn bị chặn bởi anti-forgery khi lỗi xảy ra ở yêu cầu POST.
- Thêm kiểm tra môn học hợp lệ khi lưu lớp học (tránh lỗi khóa ngoại → 500); kiểm tra trình độ gia sư hợp lệ.
- Dashboard gia sư: lớp học lặp hàng tuần luôn hiển thị buổi kế tiếp (không bị "kẹt" ở ngày đã qua).
- Bộ lọc trang chủ bổ sung "Thạc sĩ"; thêm CSS `min-w-0`; nút submit được mở khóa khi bấm Back; lỗi upload ảnh đại diện không làm mất dữ liệu đã nhập.
