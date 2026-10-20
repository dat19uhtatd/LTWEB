using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class BaiTap
{
    public int MaBaiTap { get; set; }

    public int MaLichHoc { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? NoiDungBaiTap { get; set; }

    public DateTime HanNop { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayGiao { get; set; }

    public virtual LopHocThoiKhoaBieu MaLichHocNavigation { get; set; } = null!;
}
