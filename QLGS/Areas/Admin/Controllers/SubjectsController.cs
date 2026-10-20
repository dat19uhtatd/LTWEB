using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Controllers;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;

namespace QLGiaSuLopHoc.Areas.Admin.Controllers
{
    /// <summary>CRUD danh mục Môn học.</summary>
    [Area("Admin")]
    [RoleAuthorize(Roles.Admin)]
    public class SubjectsController : BaseController
    {
        private readonly QlGiaSuLopHocContext _context;
        public SubjectsController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var subjects = await _context.MonHoc
                .OrderBy(m => m.TenMonHoc)
                .Select(m => new SubjectRow
                {
                    Subject = m,
                    TutorCount = m.GiaSuMonHoc.Select(g => g.MaGiaSu).Distinct().Count(),
                    ClassCount = m.LopHocThoiKhoaBieu.Count()
                })
                .AsNoTracking().ToListAsync();
            return View(subjects);
        }

        [HttpGet]
        public async Task<IActionResult> Form(int? id)
        {
            if (id == null) return View(new SubjectFormViewModel());
            var m = await _context.MonHoc.AsNoTracking().FirstOrDefaultAsync(x => x.MaMonHoc == id);
            if (m == null) return NotFound();
            return View(new SubjectFormViewModel { MaMonHoc = m.MaMonHoc, TenMonHoc = m.TenMonHoc, IconClass = m.IconClass, MoTa = m.MoTa });
        }

        [HttpPost]
        public async Task<IActionResult> Save(SubjectFormViewModel model)
        {
            string name = model.TenMonHoc?.Trim() ?? "";
            if (await _context.MonHoc.AnyAsync(m => m.TenMonHoc == name && m.MaMonHoc != model.MaMonHoc))
                ModelState.AddModelError(nameof(model.TenMonHoc), "Tên môn học đã tồn tại.");
            if (!ModelState.IsValid) return View("Form", model);

            MonHoc? entity = model.MaMonHoc > 0 ? await _context.MonHoc.FindAsync(model.MaMonHoc) : new MonHoc();
            if (entity == null) return NotFound();

            entity.TenMonHoc = name;
            entity.IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? "fa-book" : model.IconClass.Trim();
            entity.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
            if (model.MaMonHoc == 0) _context.MonHoc.Add(entity);

            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = model.MaMonHoc == 0 ? "Đã thêm môn học mới." : "Đã cập nhật môn học.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.MonHoc.FindAsync(id);
            if (m == null) return NotFound();

            bool used = await _context.GiaSuMonHoc.AnyAsync(g => g.MaMonHoc == id)
                        || await _context.LopHocThoiKhoaBieu.AnyAsync(l => l.MaMonHoc == id)
                        || await _context.DangKyHocThu.AnyAsync(d => d.MaMonHoc == id);
            if (used)
            {
                TempData["ErrorMsg"] = $"Không thể xóa môn \"{m.TenMonHoc}\" vì đang được gia sư/lớp học/đăng ký sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            _context.MonHoc.Remove(m);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã xóa môn học.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class SubjectRow
    {
        public MonHoc Subject { get; set; } = null!;
        public int TutorCount { get; set; }
        public int ClassCount { get; set; }
    }
}
