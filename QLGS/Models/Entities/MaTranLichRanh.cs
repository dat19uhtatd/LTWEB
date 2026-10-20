using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class MaTranLichRanh
{
    public int MaLichRanh { get; set; }

    public int MaGiaSu { get; set; }

    public string ThuTrongTuan { get; set; } = null!;

    public string BuoiTrongNgay { get; set; } = null!;

    public string KhungGioChiTiet { get; set; } = null!;

    public bool? ConTrong { get; set; }

    public virtual HoSoGiaSu MaGiaSuNavigation { get; set; } = null!;
}
