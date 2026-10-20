using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;

namespace QLGiaSuLopHoc.Filters
{
    /// <summary>
    /// Filter phân quyền dựa trên Session (Chương 7 - Filters).
    /// - Chưa đăng nhập: chuyển tới trang Login (kèm returnUrl) hoặc trả 401 JSON nếu là yêu cầu API/AJAX.
    /// - Sai vai trò: chuyển tới trang "Không có quyền truy cập" hoặc trả 403 JSON.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;
            var userId = http.Session.GetUserId();
            var role = http.Session.GetUserRole();
            bool isAjax = http.Request.Path.StartsWithSegments("/api")
                          || http.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // Đối chiếu với CSDL: tài khoản bị khóa/xóa hoặc đổi vai trò sau khi đăng nhập thì phiên cũ không còn hiệu lực
            if (userId != null && !string.IsNullOrEmpty(role))
            {
                var db = http.RequestServices.GetRequiredService<QlGiaSuLopHocContext>();
                var current = await db.NguoiDung.AsNoTracking()
                    .Where(u => u.MaNguoiDung == userId.Value)
                    .Select(u => new { u.TrangThai, u.VaiTro })
                    .FirstOrDefaultAsync();

                if (current == null || current.TrangThai == false)
                {
                    http.Session.Clear();
                    userId = null;
                    role = null;
                }
                else
                {
                    var dbRole = Roles.Normalize(current.VaiTro);
                    if (dbRole != role)
                    {
                        http.Session.SetString(SessionKeys.UserRole, dbRole);
                        role = dbRole;
                    }
                }
            }

            if (userId == null || string.IsNullOrEmpty(role))
            {
                if (isAjax)
                {
                    context.Result = new JsonResult(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." }) { StatusCode = 401 };
                    return;
                }

                string returnUrl = "/";
                if (HttpMethods.IsGet(http.Request.Method))
                {
                    returnUrl = http.Request.Path + http.Request.QueryString;
                }
                else if (Uri.TryCreate(http.Request.Headers.Referer.ToString(), UriKind.Absolute, out var referer))
                {
                    returnUrl = referer.PathAndQuery;
                }

                context.Result = new RedirectToActionResult("Login", "Account", new { area = "", returnUrl });
                return;
            }

            if (!_allowedRoles.Contains(role))
            {
                context.Result = isAjax
                    ? new JsonResult(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." }) { StatusCode = 403 }
                    : new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
            }
        }
    }
}
