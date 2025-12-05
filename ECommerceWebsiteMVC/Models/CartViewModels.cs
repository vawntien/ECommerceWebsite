using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC.Models
{

    public class BienTheSanPhamViewModel
    {
        int _MaBienThe;
        string _TenBienThe;
        decimal _GiaBan;
        string _HinhAnh;
        public int MaBienThe { get; set; }
        public string TenBienThe { get; set; }
        public decimal GiaBan { get; set; }
        public string HinhAnh { get; set; }
        public BienTheSanPhamViewModel() { }    
    }

    public class GioHangItemViewModel
    {
        int _MaCTGH;
        int _MaBienThe;
        int _MaCuaHang;
        string _TenCuaHang; 
        string _TenSanPham;
        string _TenBienThe;
        string _HinhAnh;
        decimal _DonGia;
        int _SoLuong;
        int _SoLuongTonKho; 
        public int MaCTGH { get; set; }
        public int MaBienThe { get; set; }

        public int MaCuaHang { get; set; }
        public string TenCuaHang { get; set; }

        public string TenSanPham { get; set; }
        public string TenBienThe { get; set; }
        public string HinhAnh { get; set; }

        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }

        public int SoLuongTonKho { get; set; }

        public decimal ThanhTien => DonGia * SoLuong;


        public List<BienTheSanPhamViewModel> BienTheList { get; set; }

        public GioHangItemViewModel() { }
    }

    public class ShopGroup
    {
        int _MaCuaHang;
        string _TenCuaHang;
        public int MaCuaHang { get; set; }
        public string TenCuaHang { get; set; }
        public List<GioHangItemViewModel> Items { get; set; }
        public ShopGroup() { }
    }

    public class GioHangViewModel
    {
        public List<ShopGroup> Shops { get; set; }
    }

    public class CheckoutItemVM
    {
        int _MaCTGH;    
        int _MaBienThe;
        string _TenSanPham;
        string _PhanLoai;
        decimal _DonGia;
        int _SoLuong;
        string _HinhAnh;

        public int MaCTGH { get; set; }
        public int MaBienThe { get; set; }
        public string TenSanPham { get; set; }
        public string PhanLoai { get; set; }

        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public string HinhAnh { get; set; }

        public decimal ThanhTien => DonGia * SoLuong;
        public CheckoutItemVM() { }
    }

    public class CheckoutViewModel
    {
        string _TenNguoiNhan;
        string _SDT;
        string _DiaChi;
        string _MaVoucher;
        decimal _TongTienHang;
        decimal _PhiVanChuyen;
        decimal _GiamGia;
        decimal _TongThanhToan;
        public CheckoutViewModel() { }
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