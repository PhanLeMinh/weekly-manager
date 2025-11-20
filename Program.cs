using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using WeeklyScheduleManagement.Data;
using WeeklyScheduleManagement.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Authentication với Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed dữ liệu mẫu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        if (!context.NguoiDungs.Any())
        {
            Console.WriteLine("🌱 Bắt đầu thêm dữ liệu mẫu...");

            // ⭐ KHÔNG CẦN THÊM VAI TRÒ - ĐÃ CÓ TỪ MIGRATION
            // VaiTro đã được seed trong ApplicationDbContext.OnModelCreating
            
            // Kiểm tra VaiTro
            var vaiTroCount = context.VaiTros.Count();
            Console.WriteLine($"✅ Đã có {vaiTroCount} vai trò trong database");
            
            // Thêm Khoa
            var khoas = new[]
            {
                new Khoa { TenKhoa = "Khoa Công nghệ Thông tin", MoTa = "Khoa đào tạo về CNTT", NgayTao = DateTime.Now },
                new Khoa { TenKhoa = "Khoa Kinh tế", MoTa = "Khoa đào tạo về Kinh tế", NgayTao = DateTime.Now }
            };
            context.Khoas.AddRange(khoas);
            context.SaveChanges();
            Console.WriteLine("✅ Đã thêm Khoa");

            // Thêm Phòng Ban
            var phongBans = new[]
            {
                new PhongBan { TenPhongBan = "Bộ môn Công nghệ Phần mềm", MaKhoa = khoas[0].MaKhoa, MoTa = "Bộ môn CNPM", NgayTao = DateTime.Now },
                new PhongBan { TenPhongBan = "Bộ môn Mạng máy tính", MaKhoa = khoas[0].MaKhoa, MoTa = "Bộ môn MMT", NgayTao = DateTime.Now }
            };
            context.PhongBans.AddRange(phongBans);
            context.SaveChanges();
            Console.WriteLine("✅ Đã thêm Phòng ban");

            // Thêm Người dùng (mật khẩu: 123456)
            var nguoiDungs = new[]
            {
                new NguoiDung { 
                    HoTen = "Admin System", 
                    Email = "admin@university.edu.vn", 
                    MatKhau = "123456", 
                    SoDienThoai = "0900000001", 
                    MaPhongBan = phongBans[0].MaPhongBan, 
                    MaVaiTro = 1, // Admin
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new NguoiDung { 
                    HoTen = "Nguyễn Văn Manager", 
                    Email = "manager@university.edu.vn", 
                    MatKhau = "123456", 
                    SoDienThoai = "0900000002", 
                    MaPhongBan = phongBans[0].MaPhongBan, 
                    MaVaiTro = 2, // Manager
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new NguoiDung { 
                    HoTen = "Trần Thị Bình", 
                    Email = "giaovien1@university.edu.vn", 
                    MatKhau = "123456", 
                    SoDienThoai = "0901234567", 
                    MaPhongBan = phongBans[0].MaPhongBan, 
                    MaVaiTro = 3, // GiaoVien
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new NguoiDung { 
                    HoTen = "Lê Văn Công", 
                    Email = "giaovien2@university.edu.vn", 
                    MatKhau = "123456", 
                    SoDienThoai = "0901234568", 
                    MaPhongBan = phongBans[1].MaPhongBan, 
                    MaVaiTro = 3,
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new NguoiDung { 
                    HoTen = "Phạm Thị Dung", 
                    Email = "giaovien3@university.edu.vn", 
                    MatKhau = "123456", 
                    SoDienThoai = "0901234569", 
                    MaPhongBan = phongBans[0].MaPhongBan, 
                    MaVaiTro = 3,
                    TrangThai = true,
                    NgayTao = DateTime.Now
                }
            };
            context.NguoiDungs.AddRange(nguoiDungs);
            context.SaveChanges();
            Console.WriteLine("✅ Đã thêm Người dùng");

            // Thêm Địa điểm
            var diaDiems = new[]
            {
                new DiaDiem { 
                    TenDiaDiem = "Phòng A101", 
                    LoaiDiaDiem = "PhongHoc", 
                    SucChua = 50, 
                    MoTa = "Phòng học lý thuyết tầng 1",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new DiaDiem { 
                    TenDiaDiem = "Phòng A201", 
                    LoaiDiaDiem = "PhongHoc", 
                    SucChua = 50, 
                    MoTa = "Phòng học lý thuyết tầng 2",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new DiaDiem { 
                    TenDiaDiem = "Phòng B202", 
                    LoaiDiaDiem = "PhongHoc", 
                    SucChua = 40, 
                    MoTa = "Phòng thực hành máy tính",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new DiaDiem { 
                    TenDiaDiem = "Hội trường C", 
                    LoaiDiaDiem = "HoiTruong", 
                    SucChua = 200, 
                    MoTa = "Hội trường lớn",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                },
                new DiaDiem { 
                    TenDiaDiem = "Phòng họp 301", 
                    LoaiDiaDiem = "PhongHop", 
                    SucChua = 20, 
                    MoTa = "Phòng họp nhỏ",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                }
            };
            context.DiaDiems.AddRange(diaDiems);
            context.SaveChanges();
            Console.WriteLine("✅ Đã thêm Địa điểm");

            Console.WriteLine("✅ Đã thêm dữ liệu mẫu thành công!");
            Console.WriteLine("📧 Tài khoản test:");
            Console.WriteLine("   Admin: admin@university.edu.vn / 123456");
            Console.WriteLine("   Manager: manager@university.edu.vn / 123456");
            Console.WriteLine("   Giáo viên: giaovien1@university.edu.vn / 123456");
        }
        else
        {
            Console.WriteLine("✅ Database đã có dữ liệu!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Lỗi khi seed data: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
        }
    }
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();