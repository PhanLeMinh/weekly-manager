using Microsoft.EntityFrameworkCore;
using WeeklyScheduleManagement.Models;

namespace WeeklyScheduleManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DiaDiem> DiaDiems { get; set; }
        public DbSet<LichTuan> LichTuans { get; set; }
        public DbSet<ThanhPhanThamGia> ThanhPhanThamGias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Cấu hình relationship cho LichTuan
    modelBuilder.Entity<LichTuan>()
        .HasOne(l => l.NguoiDangKy)
        .WithMany(n => n.LichTuansDangKy)
        .HasForeignKey(l => l.MaNguoiDangKy)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<LichTuan>()
        .HasOne(l => l.ChuTri)
        .WithMany(n => n.LichTuansChuTri)
        .HasForeignKey(l => l.MaChuTri)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<LichTuan>()
        .HasOne(l => l.NguoiDuyet)
        .WithMany()
        .HasForeignKey(l => l.MaNguoiDuyet)
        .OnDelete(DeleteBehavior.Restrict);
    
    // ĐỔI THÀNH Restrict thay vì Cascade
    modelBuilder.Entity<LichTuan>()
        .HasOne(l => l.DiaDiem)
        .WithMany(d => d.LichTuans)
        .HasForeignKey(l => l.MaDiaDiem)
        .OnDelete(DeleteBehavior.Restrict);

    // Cấu hình ThanhPhanThamGia
    modelBuilder.Entity<ThanhPhanThamGia>()
        .HasOne(t => t.LichTuan)
        .WithMany(l => l.ThanhPhanThamGias)
        .HasForeignKey(t => t.MaLichTuan)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<ThanhPhanThamGia>()
        .HasOne(t => t.NguoiDung)
        .WithMany(n => n.ThanhPhanThamGias)
        .HasForeignKey(t => t.MaNguoiDung)
        .OnDelete(DeleteBehavior.Restrict);
        
    // Seed data cho VaiTro
    modelBuilder.Entity<VaiTro>().HasData(
        new VaiTro { MaVaiTro = 1, TenVaiTro = "Admin" },
        new VaiTro { MaVaiTro = 2, TenVaiTro = "Manager" },
        new VaiTro { MaVaiTro = 3, TenVaiTro = "GiaoVien" }
    );
}
    }
}