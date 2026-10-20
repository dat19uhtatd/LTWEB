using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class KhoiLop
{
    public int MaKhoiLop { get; set; }

    public string TenKhoiLop { get; set; } = null!;

    public string? CapHoc { get; set; }

    public virtual ICollection<GiaSuMonHoc> GiaSuMonHoc { get; set; } = new List<GiaSuMonHoc>();
}
