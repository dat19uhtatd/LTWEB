using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Services;

var projectRoot = Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
    ? Directory.GetCurrentDirectory()
    : (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "wwwroot"))
        ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."))
        : Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = projectRoot
});

// 1. Kết nối SQL Server qua DbContext (chuỗi kết nối nằm trong appsettings.json)
builder.Services.AddDbContext<QlGiaSuLopHocContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controllers + Views + Web API
builder.Services.AddControllersWithViews(options =>
    {
        // Không tự thêm [Required] tiếng Anh cho kiểu string không-null -> dùng thông báo tiếng Việt tự định nghĩa
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        // Chống lỗi vòng lặp tham chiếu khi serialize Entity -> JSON
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 3. Session, Cache, HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache(); // dùng để giới hạn số lần đăng nhập sai
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = ".EduMatch.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 4. Chống CSRF cho cả form lẫn AJAX (header RequestVerificationToken)
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// 5. Dịch vụ nội bộ
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Trang 404/403... thân thiện
app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Bắt buộc đặt trước UseAuthorization và các Filter dùng Session
app.UseAuthorization();

// Tự tạo CSDL + nạp dữ liệu mẫu nếu CSDL còn trống (có thể tắt trong appsettings: Seed:Enabled)
if (app.Configuration.GetValue("Seed:Enabled", true))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Không thể khởi tạo/nạp dữ liệu mẫu. Hãy kiểm tra chuỗi kết nối SQL Server trong appsettings.json.");
    }
}

// Định tuyến cho Area Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=HomeAdmin}/{action=Index}/{id?}");

// Định tuyến mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
