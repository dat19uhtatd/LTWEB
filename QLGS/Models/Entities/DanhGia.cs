using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class DanhGia
{
    public int MaDanhGia { get; set; }

    public int MaGiaSu { get; set; }

    public int MaPhuHuynh { get; set; }

    public int SoSao { get; set; }

    public string NoiDungBinhLuan { get; set; } = null!;

    public DateTime? NgayDanhGia { get; set; }

    public bool? HienThi { get; set; }

    public virtual HoSoGiaSu MaGiaSuNavigation { get; set; } = null!;

    public virtual NguoiDung MaPhuHuynhNavigation { get; set; } = null!;
}
