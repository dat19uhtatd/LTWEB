using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class LopHocThoiKhoaBieu
{
    public int MaLichHoc { get; set; }

    public int MaGiaSu { get; set; }

    public int MaMonHoc { get; set; }

    public string TenHocSinh { get; set; } = null!;

    public string ThuTrongTuan { get; set; } = null!;

    public DateOnly NgayHocTiepTheo { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public string PhongHocHoacLink { get; set; } = null!;

    public decimal HocPhiCaDay { get; set; }

    public bool? BatNhacNhoTruoc30P { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<BaiTap> BaiTap { get; set; } = new List<BaiTap>();

    public virtual HoSoGiaSu MaGiaSuNavigation { get; set; } = null!;

    public virtual MonHoc MaMonHocNavigation { get; set; } = null!;
}
