using System;
using System.Collections.Generic;

namespace ECommerceWebsiteMVC_Seller.Models
{
    public class SellerOrderItemViewModel
    {
        public int MaDonHang { get; set; }
        public string MaDonHangHienThi => $"Mã đơn hàng {MaDonHang}";

        public string TenNguoiMua { get; set; }
        public string TenNguoiNhan { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }

        public DateTime ThoiGianDat { get; set; }

        public decimal TongTien { get; set; }
        public int TongSoLuong { get; set; }

        public string TrangThaiDonHang { get; set; }
        public string TrangThaiVanChuyen { get; set; }
        public bool TrangThaiThanhToan { get; set; }

        public string DonViVanChuyen { get; set; }

        // Thông tin sản phẩm hiển thị chính trên dòng đơn
        public string TenSanPhamDauTien { get; set; }
        public string TenBienTheDauTien { get; set; }
        public int SoLuongSanPhamDauTien { get; set; }
        public decimal DonGiaSanPhamDauTien { get; set; }
        public string AnhSanPhamDauTien { get; set; }

        public string PhuongThucThanhToanHienThi =>
            TrangThaiThanhToan ? "Thanh toán online" : "Thanh toán khi nhận hàng";
    }

    public class SellerOrderItemDetailViewModel
    {
        public int MaSanPham { get; set; }
        public int MaBienThe { get; set; }
        public string TenSanPham { get; set; }
        public string TenBienThe { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
        public string AnhSanPham { get; set; }
    }

    public class SellerOrderDetailViewModel
    {
        public int MaDonHang { get; set; }
        public string TrangThaiDonHang { get; set; }
        public string TrangThaiVanChuyen { get; set; }
        public bool TrangThaiThanhToan { get; set; }
        public string DonViVanChuyen { get; set; }

        public string TenNguoiNhan { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }

        public DateTime ThoiGianDat { get; set; }

        public decimal TongTienSanPham { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TongTienDonHang { get; set; }

        public string LyDoHuy { get; set; }
        public string GhiChuTuNguoiMua { get; set; }

        public List<SellerOrderItemDetailViewModel> Items { get; set; } = new List<SellerOrderItemDetailViewModel>();

        public string PhuongThucThanhToanHienThi =>
            TrangThaiThanhToan ? "Thanh toán online" : "Thanh toán khi nhận hàng";
    }
}


