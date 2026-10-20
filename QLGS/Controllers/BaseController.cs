using Microsoft.AspNetCore.Mvc;
using QLGiaSuLopHoc.Helpers;

namespace QLGiaSuLopHoc.Controllers
{
    /// <summary>
    /// Controller cơ sở: tự động kiểm tra Anti-Forgery Token cho mọi yêu cầu POST/PUT/DELETE
    /// và cung cấp các thuộc tính truy cập nhanh thông tin người dùng đang đăng nhập (Session).
    /// </summary>
    [AutoValidateAntiforgeryToken]
    public abstract class BaseController : Controller
    {
        protected int? CurrentUserId => HttpContext.Session.GetUserId();
        protected string? CurrentRole => HttpContext.Session.GetUserRole();

        protected IActionResult RedirectToLocalOr(string? returnUrl, string action, string controller, object? routeValues = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction(action, controller, routeValues);
        }
    }
}
