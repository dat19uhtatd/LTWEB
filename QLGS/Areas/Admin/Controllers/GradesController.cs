using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Controllers;
using QLGiaSuLopHoc.Filters;
using QLGiaSuLopHoc.Helpers;
using QLGiaSuLopHoc.Models.Entities;
using QLGiaSuLopHoc.Models.ViewModels;

namespace QLGiaSuLopHoc.Areas.Admin.Controllers
{
    /// <summary>CRUD danh mục Khối lớp.</summary>
    [Area("Admin")]
    [RoleAuthorize(Roles.Admin)]
    public class GradesController : BaseController
    {
        private readonly QlGiaSuLopHocContext _context;
        public GradesController(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var rows = await _context.KhoiLop
                .OrderBy(k => k.MaKhoiLop)
                .Select(k => new GradeRow { Grade = k, TutorCount = k.GiaSuMonHoc.Select(g => g.MaGiaSu).Distinct().Count() })
                .AsNoTracking().ToListAsync();
            return View(rows);
        }

        [HttpGet]
        public async Task<IActionResult> Form(int? id)
        {
            if (id == null) return View(new GradeFormViewModel());
            var k = await _context.KhoiLop.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhoiLop == id);
            if (k == null) return NotFound();
            return View(new GradeFormViewModel { MaKhoiLop = k.MaKhoiLop, TenKhoiLop = k.TenKhoiLop, CapHoc = k.CapHoc });
        }

        [HttpPost]
        public async Task<IActionResult> Save(GradeFormViewModel model)
        {
            string name = model.TenKhoiLop?.Trim() ?? "";
            if (await _context.KhoiLop.AnyAsync(k => k.TenKhoiLop == name && k.MaKhoiLop != model.MaKhoiLop))
                ModelState.AddModelError(nameof(model.TenKhoiLop), "Tên khối lớp đã tồn tại.");
            if (!ModelState.IsValid) return View("Form", model);

            KhoiLop? entity = model.MaKhoiLop > 0 ? await _context.KhoiLop.FindAsync(model.MaKhoiLop) : new KhoiLop();
            if (entity == null) return NotFound();

            entity.TenKhoiLop = name;
            entity.CapHoc = string.IsNullOrWhiteSpace(model.CapHoc) ? null : model.CapHoc.Trim();
            if (model.MaKhoiLop == 0) _context.KhoiLop.Add(entity);

            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = model.MaKhoiLop == 0 ? "Đã thêm khối lớp mới." : "Đã cập nhật khối lớp.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var k = await _context.KhoiLop.FindAsync(id);
            if (k == null) return NotFound();

            if (await _context.GiaSuMonHoc.AnyAsync(g => g.MaKhoiLop == id))
            {
                TempData["ErrorMsg"] = $"Không thể xóa \"{k.TenKhoiLop}\" vì đang có gia sư nhận dạy khối này.";
                return RedirectToAction(nameof(Index));
            }

            _context.KhoiLop.Remove(k);
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã xóa khối lớp.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class GradeRow
    {
        public KhoiLop Grade { get; set; } = null!;
        public int TutorCount { get; set; }
    }
}
