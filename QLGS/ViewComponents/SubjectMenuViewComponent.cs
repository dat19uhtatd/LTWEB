using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLGiaSuLopHoc.Models.Entities;

namespace QLGiaSuLopHoc.ViewComponents
{
    public class SubjectMenuViewComponent : ViewComponent
    {
        private readonly QlGiaSuLopHocContext _context;
        public SubjectMenuViewComponent(QlGiaSuLopHocContext context) => _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var subjects = await _context.MonHoc.AsNoTracking().OrderBy(m => m.TenMonHoc).ToListAsync();
            return View(subjects);
        }
    }
}