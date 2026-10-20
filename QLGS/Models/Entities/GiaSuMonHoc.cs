using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class GiaSuMonHoc
{
    public int MaGiaSuMonHoc { get; set; }

    public int MaGiaSu { get; set; }

    public int MaMonHoc { get; set; }

    public int MaKhoiLop { get; set; }

    public virtual HoSoGiaSu MaGiaSuNavigation { get; set; } = null!;

    public virtual KhoiLop MaKhoiLopNavigation { get; set; } = null!;

    public virtual MonHoc MaMonHocNavigation { get; set; } = null!;
}
