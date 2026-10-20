using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class DangKyHocThu
{
    public int MaDangKy { get; set; }

    public int MaGiaSu { get; set; }

    public int? MaPhuHuynh { get; set; }

    public string HoTenPhuHuynh { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public int MaMonHoc { get; set; }

    public string TrinhDoHienTaiHs { get; set; } = null!;

    public DateTime KhungGioMongMuon { get; set; }

    public string? GhiChuYeuCau { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual HoSoGiaSu MaGiaSuNavigation { get; set; } = null!;

    public virtual MonHoc MaMonHocNavigation { get; set; } = null!;

    public virtual NguoiDung? MaPhuHuynhNavigation { get; set; }
}
