using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public string? AnhDaiDien { get; set; }

    public string VaiTro { get; set; } = null!;

    public bool? NhanThongBaoDesktop { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<DangKyHocThu> DangKyHocThu { get; set; } = new List<DangKyHocThu>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual HoSoGiaSu? HoSoGiaSu { get; set; }
}
