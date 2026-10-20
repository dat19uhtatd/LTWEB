using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLGiaSuLopHoc.Models.Entities;

public partial class QlGiaSuLopHocContext : DbContext
{
    public QlGiaSuLopHocContext(DbContextOptions<QlGiaSuLopHocContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiTap> BaiTap { get; set; }

    public virtual DbSet<DangKyHocThu> DangKyHocThu { get; set; }

    public virtual DbSet<DanhGia> DanhGia { get; set; }

    public virtual DbSet<GiaSuMonHoc> GiaSuMonHoc { get; set; }

    public virtual DbSet<HoSoGiaSu> HoSoGiaSu { get; set; }

    public virtual DbSet<KhoiLop> KhoiLop { get; set; }

    public virtual DbSet<LopHocThoiKhoaBieu> LopHocThoiKhoaBieu { get; set; }

    public virtual DbSet<MaTranLichRanh> MaTranLichRanh { get; set; }

    public virtual DbSet<MonHoc> MonHoc { get; set; }

    public virtual DbSet<NguoiDung> NguoiDung { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiTap>(entity =>
        {
            entity.HasKey(e => e.MaBaiTap).HasName("PK__BaiTap__3AF6A91574EFDED8");

            entity.Property(e => e.HanNop).HasColumnType("datetime");
            entity.Property(e => e.NgayGiao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TieuDe).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.MaLichHocNavigation).WithMany(p => p.BaiTap)
                .HasForeignKey(d => d.MaLichHoc)
                .HasConstraintName("FK__BaiTap__MaLichHo__04E4BC85");
        });

        modelBuilder.Entity<DangKyHocThu>(entity =>
        {
            entity.HasKey(e => e.MaDangKy).HasName("PK__DangKyHo__BA90F02D08E335C6");

            entity.Property(e => e.GhiChuYeuCau).HasMaxLength(500);
            entity.Property(e => e.HoTenPhuHuynh).HasMaxLength(100);
            entity.Property(e => e.KhungGioMongMuon).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Chờ duyệt");
            entity.Property(e => e.TrinhDoHienTaiHs)
                .HasMaxLength(50)
                .HasColumnName("TrinhDoHienTaiHS");

            entity.HasOne(d => d.MaGiaSuNavigation).WithMany(p => p.DangKyHocThu)
                .HasForeignKey(d => d.MaGiaSu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DangKyHoc__MaGia__787EE5A0");

            entity.HasOne(d => d.MaMonHocNavigation).WithMany(p => p.DangKyHocThu)
                .HasForeignKey(d => d.MaMonHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DangKyHoc__MaMon__7A672E12");

            entity.HasOne(d => d.MaPhuHuynhNavigation).WithMany(p => p.DangKyHocThu)
                .HasForeignKey(d => d.MaPhuHuynh)
                .HasConstraintName("FK__DangKyHoc__MaPhu__797309D9");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BFA8F66E13");

            entity.Property(e => e.HienThi).HasDefaultValue(true);
            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDungBinhLuan).HasMaxLength(500);

            entity.HasOne(d => d.MaGiaSuNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaGiaSu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaGiaSu__09A971A2");

            entity.HasOne(d => d.MaPhuHuynhNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaPhuHuynh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaPhuHu__0A9D95DB");
        });

        modelBuilder.Entity<GiaSuMonHoc>(entity =>
        {
            entity.HasKey(e => e.MaGiaSuMonHoc).HasName("PK__GiaSu_Mo__2455AA0282E5B40B");

            entity.ToTable("GiaSuMonHoc");

            entity.HasOne(d => d.MaGiaSuNavigation).WithMany(p => p.GiaSuMonHoc)
                .HasForeignKey(d => d.MaGiaSu)
                .HasConstraintName("FK__GiaSu_Mon__MaGia__6FE99F9F");

            entity.HasOne(d => d.MaKhoiLopNavigation).WithMany(p => p.GiaSuMonHoc)
                .HasForeignKey(d => d.MaKhoiLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiaSu_Mon__MaKho__71D1E811");

            entity.HasOne(d => d.MaMonHocNavigation).WithMany(p => p.GiaSuMonHoc)
                .HasForeignKey(d => d.MaMonHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiaSu_Mon__MaMon__70DDC3D8");
        });

        modelBuilder.Entity<HoSoGiaSu>(entity =>
        {
            entity.HasKey(e => e.MaGiaSu).HasName("PK__HoSoGiaS__5A2511EEA0C9FEDA");

            entity.HasIndex(e => e.MaNguoiDung, "UQ__HoSoGiaS__C539D763E9A69ECF").IsUnique();

            entity.Property(e => e.AnhBangCapTheSv)
                .HasMaxLength(255)
                .HasColumnName("AnhBangCapTheSv");
            entity.Property(e => e.DaXacMinh).HasDefaultValue(false);
            entity.Property(e => e.DiemDanhGiaTb)
                .HasDefaultValue(5.0)
                .HasColumnName("DiemDanhGiaTb");
            entity.Property(e => e.HinhThucDay).HasMaxLength(50);
            entity.Property(e => e.HocPhiTheoGio).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.KhuVucDay).HasMaxLength(100);
            entity.Property(e => e.SoLopDangDay).HasDefaultValue(0);
            entity.Property(e => e.TongSoDanhGia).HasDefaultValue(0);
            entity.Property(e => e.TrinhDo).HasMaxLength(50);
            entity.Property(e => e.TruongDaoTao).HasMaxLength(150);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.HoSoGiaSu)
                .HasForeignKey<HoSoGiaSu>(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoSoGiaSu__MaNgu__693CA210");
        });

        modelBuilder.Entity<KhoiLop>(entity =>
        {
            entity.HasKey(e => e.MaKhoiLop).HasName("PK__KhoiLop__ACB753A2F60A1244");

            entity.Property(e => e.CapHoc).HasMaxLength(50);
            entity.Property(e => e.TenKhoiLop).HasMaxLength(100);
        });

        modelBuilder.Entity<LopHocThoiKhoaBieu>(entity =>
        {
            entity.HasKey(e => e.MaLichHoc).HasName("PK__LopHoc_T__150EBC2170C79307");

            entity.ToTable("LopHocThoiKhoaBieu");

            entity.Property(e => e.BatNhacNhoTruoc30P).HasDefaultValue(true);
            entity.Property(e => e.HocPhiCaDay).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PhongHocHoacLink).HasMaxLength(255);
            entity.Property(e => e.TenHocSinh).HasMaxLength(100);
            entity.Property(e => e.ThuTrongTuan).HasMaxLength(20);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đang học");

            entity.HasOne(d => d.MaGiaSuNavigation).WithMany(p => p.LopHocThoiKhoaBieu)
                .HasForeignKey(d => d.MaGiaSu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LopHoc_Th__MaGia__7F2BE32F");

            entity.HasOne(d => d.MaMonHocNavigation).WithMany(p => p.LopHocThoiKhoaBieu)
                .HasForeignKey(d => d.MaMonHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LopHoc_Th__MaMon__00200768");
        });

        modelBuilder.Entity<MaTranLichRanh>(entity =>
        {
            entity.HasKey(e => e.MaLichRanh).HasName("PK__MaTranLi__0942D64680F5096C");

            entity.Property(e => e.BuoiTrongNgay).HasMaxLength(20);
            entity.Property(e => e.ConTrong).HasDefaultValue(true);
            entity.Property(e => e.KhungGioChiTiet)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ThuTrongTuan).HasMaxLength(20);

            entity.HasOne(d => d.MaGiaSuNavigation).WithMany(p => p.MaTranLichRanh)
                .HasForeignKey(d => d.MaGiaSu)
                .HasConstraintName("FK__MaTranLic__MaGia__74AE54BC");
        });

        modelBuilder.Entity<MonHoc>(entity =>
        {
            entity.HasKey(e => e.MaMonHoc).HasName("PK__MonHoc__4127737F14D8A4D5");

            entity.Property(e => e.IconClass)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("fa-book");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenMonHoc).HasMaxLength(100);
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762A62F7B87");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534EBF7ED3A").IsUnique();

            entity.Property(e => e.AnhDaiDien)
                .HasMaxLength(255)
                .HasDefaultValue("/images/avatars/default.png");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NhanThongBaoDesktop).HasDefaultValue(true);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.VaiTro)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
