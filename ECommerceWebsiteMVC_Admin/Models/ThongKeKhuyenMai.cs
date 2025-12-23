using System;
using System.Collections.Generic;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class ThongKeKhuyenMai
    {
        public decimal TongTienGiamChoKhachHang { get; set; } 
        public decimal TongDoanhThuTuVoucher { get; set; } 
        public int TongSoDonHangSuDungVoucher { get; set; } 

        public List<LichSuSuDungVoucher> LichSuSuDung { get; set; }

        public List<VoucherHieuQua> XepHangVoucher { get; set; }

        public List<ThongKeTheoThoiGian> DuLieuBieuDoThoiGian { get; set; }

        public List<GiamGia> DanhSachVoucher { get; set; }
    }

    public class ThongKeTheoThoiGian
    {
        public string Ngay { get; set; }
        public int SoLuongDonHang { get; set; }
        public decimal TongTienGiam { get; set; }
        public decimal TongDoanhThu { get; set; }
    }

    public class LichSuSuDungVoucher
    {
        public int MaDonHang { get; set; }
        public string TenKhachHang { get; set; }
        public string Email { get; set; }
        public string TenVoucher { get; set; }
        public decimal SoTienGiam { get; set; }
        public decimal TongTienDonHang { get; set; }
        public DateTime ThoiGianDat { get; set; }
        public string TrangThaiDonHang { get; set; }
    }

    public class VoucherHieuQua
    {
        public int MaGiamGia { get; set; }
        public string TenMaGG { get; set; }
        public int SoLanSuDung { get; set; } 
        public decimal TongTienGiam { get; set; } 
        public decimal TongDoanhThu { get; set; } 
        public DateTime? NgayBD { get; set; }
        public DateTime? NgayKT { get; set; }
        public string TrangThai { get; set; } 
    }
}

