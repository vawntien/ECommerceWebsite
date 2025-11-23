using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC.Models
{
    public class CartPatch
    {
        public int MaCTGH { get; set; }
        public int? NewBienThe { get; set; }
        public int? NewSoLuong { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class BienTheSanPhamViewModel
    {
        public int MaBienThe { get; set; }
        public string TenBienThe { get; set; }
        public decimal GiaBan { get; set; }
        public string HinhAnh { get; set; }
    }

    public class GioHangItemViewModel
    {
        public int MaCTGH { get; set; }
        public int MaBienThe { get; set; }

        public int MaCuaHang { get; set; }
        public string TenCuaHang { get; set; }

        public string TenSanPham { get; set; }
        public string TenBienThe { get; set; }
        public string HinhAnh { get; set; }

        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }

        public List<BienTheSanPhamViewModel> BienTheList { get; set; }
    }

    public class ShopGroup
    {
        public int MaCuaHang { get; set; }
        public string TenCuaHang { get; set; }
        public List<GioHangItemViewModel> Items { get; set; }
    }

    public class GioHangViewModel
    {
        public List<ShopGroup> Shops { get; set; }
    }

    public class CheckoutItemVM
    {
        public int MaCTGH { get; set; }
        public string TenSanPham { get; set; }
        public string PhanLoai { get; set; }

        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public string HinhAnh { get; set; }

        public decimal ThanhTien => DonGia * SoLuong;
    }

    public class CheckoutViewModel
    {
        public string TenNguoiNhan { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }

        public string MaVoucher { get; set; }

        public List<GiamGia> DanhSachVoucher { get; set; }
        public List<CheckoutItemVM> Items { get; set; }

        public decimal TongTienHang { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongThanhToan { get; set; }
    }
}