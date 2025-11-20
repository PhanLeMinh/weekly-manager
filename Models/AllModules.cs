using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeeklyScheduleManagement.Models
{
    // ===== KHOA =====
    public class Khoa
    {
        [Key]
        public int MaKhoa { get; set; }

        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [Display(Name = "Tên Khoa")]
        public string TenKhoa { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public virtual ICollection<PhongBan> PhongBans { get; set; }
    }

    // ===== PHÒNG BAN =====
    public class PhongBan
    {
        [Key]
        public int MaPhongBan { get; set; }

        [Required(ErrorMessage = "Tên phòng ban không được để trống")]
        [Display(Name = "Tên Phòng Ban")]
        public string TenPhongBan { get; set; }

        [Display(Name = "Khoa")]
        public int MaKhoa { get; set; }

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("MaKhoa")]
        public virtual Khoa Khoa { get; set; }
        public virtual ICollection<NguoiDung> NguoiDungs { get; set; }
    }

    // ===== VAI TRÒ =====
    public class VaiTro
    {
        [Key]
        public int MaVaiTro { get; set; }

        [Required]
        [Display(Name = "Tên Vai Trò")]
        public string TenVaiTro { get; set; } // Admin, Manager, GiaoVien

        public virtual ICollection<NguoiDung> NguoiDungs { get; set; }
    }

    // ===== NGƯỜI DÙNG =====
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Phòng Ban")]
        public int? MaPhongBan { get; set; }

        [Display(Name = "Vai Trò")]
        public int MaVaiTro { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("MaPhongBan")]
        public virtual PhongBan PhongBan { get; set; }

        [ForeignKey("MaVaiTro")]
        public virtual VaiTro VaiTro { get; set; }

        public virtual ICollection<LichTuan> LichTuansDangKy { get; set; }
        public virtual ICollection<LichTuan> LichTuansChuTri { get; set; }
        public virtual ICollection<ThanhPhanThamGia> ThanhPhanThamGias { get; set; }
    }

    // ===== ĐỊA ĐIỂM =====
    public class DiaDiem
    {
        [Key]
        public int MaDiaDiem { get; set; }

        [Required(ErrorMessage = "Tên địa điểm không được để trống")]
        [Display(Name = "Tên Địa Điểm")]
        public string TenDiaDiem { get; set; }

        [Display(Name = "Loại Địa Điểm")]
        public string LoaiDiaDiem { get; set; } // PhongHoc, HoiTruong, PhongHop

        [Display(Name = "Sức chứa")]
        public int? SucChua { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public virtual ICollection<LichTuan> LichTuans { get; set; }
    }

    // ===== LỊCH TUẦN =====
public class LichTuan
{
    [Key]
    public int MaLichTuan { get; set; }

    [Required(ErrorMessage = "Tên lịch không được để trống")]
    [Display(Name = "Tên Lịch")]
    public string TenLichTuan { get; set; }

    [Display(Name = "Người Đăng Ký")]
    public int MaNguoiDangKy { get; set; }

    [Display(Name = "Người Chủ Trì")]
    public int MaChuTri { get; set; }

    [Display(Name = "Địa Điểm")]
    public int MaDiaDiem { get; set; }

    [Required(ErrorMessage = "Nội dung không được để trống")]
    [Display(Name = "Nội Dung")]
    [DataType(DataType.MultilineText)]
    public string NoiDung { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
    [Display(Name = "Ngày Bắt Đầu")]
    [DataType(DataType.DateTime)]
    public DateTime NgayBatDau { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
    [Display(Name = "Ngày Kết Thúc")]
    [DataType(DataType.DateTime)]
    public DateTime NgayKetThuc { get; set; }

    [Display(Name = "Trạng Thái")]
    public string TrangThai { get; set; } = "ChoDuyet";

    [Display(Name = "Lý Do Từ Chối")]
    [DataType(DataType.MultilineText)]
    public string? LyDoTuChoi { get; set; } // THÊM ? để cho phép null

    [Display(Name = "Người Duyệt")]
    public int? MaNguoiDuyet { get; set; }

    [Display(Name = "Ngày Duyệt")]
    public DateTime? NgayDuyet { get; set; }

    [Display(Name = "Ngày Tạo")]
    public DateTime NgayTao { get; set; } = DateTime.Now;

    [Display(Name = "Ngày Cập Nhật")]
    public DateTime NgayCapNhat { get; set; } = DateTime.Now;

    [ForeignKey("MaNguoiDangKy")]
    public virtual NguoiDung? NguoiDangKy { get; set; }

    [ForeignKey("MaChuTri")]
    public virtual NguoiDung? ChuTri { get; set; }

    [ForeignKey("MaDiaDiem")]
    public virtual DiaDiem? DiaDiem { get; set; }

    [ForeignKey("MaNguoiDuyet")]
    public virtual NguoiDung? NguoiDuyet { get; set; }

    public virtual ICollection<ThanhPhanThamGia>? ThanhPhanThamGias { get; set; }
}

    // ===== THÀNH PHẦN THAM GIA =====
    public class ThanhPhanThamGia
    {
        [Key]
        public int MaThanhPhan { get; set; }

        [Display(Name = "Lịch Tuần")]
        public int MaLichTuan { get; set; }

        [Display(Name = "Người Tham Gia")]
        public int MaNguoiDung { get; set; }

        [Display(Name = "Vai Trò")]
        public string VaiTro { get; set; }

        [Display(Name = "Ghi Chú")]
        [DataType(DataType.MultilineText)]
        public string GhiChu { get; set; }

        [Display(Name = "Ngày Tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("MaLichTuan")]
        public virtual LichTuan LichTuan { get; set; }

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung NguoiDung { get; set; }
    }

    // ===== VIEW MODELS (cho Login/Register) =====
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; }

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
    }
}