using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class MonHoc
{
    public int MaMonHoc { get; set; }

    public string TenMonHoc { get; set; } = null!;

    public string? IconClass { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<DangKyHocThu> DangKyHocThu { get; set; } = new List<DangKyHocThu>();

    public virtual ICollection<GiaSuMonHoc> GiaSuMonHoc { get; set; } = new List<GiaSuMonHoc>();

    public virtual ICollection<LopHocThoiKhoaBieu> LopHocThoiKhoaBieu { get; set; } = new List<LopHocThoiKhoaBieu>();
}
