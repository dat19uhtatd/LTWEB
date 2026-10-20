using System;
using System.Collections.Generic;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class HoSoGiaSu
{
    public int MaGiaSu { get; set; }

    public int MaNguoiDung { get; set; }

    public string TrinhDo { get; set; } = null!;

    public string? TruongDaoTao { get; set; }

    public decimal HocPhiTheoGio { get; set; }

    public string KhuVucDay { get; set; } = null!;

    public string HinhThucDay { get; set; } = null!;

    public string? GioiThieuBio { get; set; }

    public string? ThanhTichNoiBat { get; set; }

    public string? AnhBangCapTheSv { get; set; }

    public bool? DaXacMinh { get; set; }

    public double? DiemDanhGiaTb { get; set; }

    public int? TongSoDanhGia { get; set; }

    public int? SoLopDangDay { get; set; }

    public virtual ICollection<DangKyHocThu> DangKyHocThu { get; set; } = new List<DangKyHocThu>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<GiaSuMonHoc> GiaSuMonHoc { get; set; } = new List<GiaSuMonHoc>();

    public virtual ICollection<LopHocThoiKhoaBieu> LopHocThoiKhoaBieu { get; set; } = new List<LopHocThoiKhoaBieu>();

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual ICollection<MaTranLichRanh> MaTranLichRanh { get; set; } = new List<MaTranLichRanh>();
}
