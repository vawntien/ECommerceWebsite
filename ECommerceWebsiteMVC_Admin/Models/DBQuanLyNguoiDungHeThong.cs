using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class DBQuanLyNguoiDungHeThong
    {
        ECommerceWebsiteEntities db;
        
        public DBQuanLyNguoiDungHeThong()
        {
            db = new ECommerceWebsiteEntities();
        }

        public int TongSoNguoiBan()
        {
            return db.NguoiBans.Count(); 
        }
        public int TongSoNguoiMua()
        {
            return db.NguoiMuas.Count();
        }
        public int SoCuaHangBiKhoa()
        {
            return db.CuaHangs.Where(t=>t.TrangThai == false).Count();
        }
        public int TongSoNhanVien()
        {
            return db.NhanViens.Count();
        }
        public List<CuaHang> DanhSachCuaHang()
        {
            return db.CuaHangs.ToList();
        }
        public List<DonHang> DanhSachDonHangTheoCuaHang(int pMaCuaHang)
        {
            var rs = from dh in db.DonHangs
                                  join ctdh in db.ChiTietDonHangs on dh.MaDonHang equals ctdh.MaDonHang
                                  join ctgh in db.ChiTietGioHangs on ctdh.MaCTGH equals ctgh.MaCTGH
                                  join bt in db.BienTheSanPhams on ctgh.MaBienThe equals bt.MaBienThe
                                  join sp in db.SanPhams on bt.MaSanPham equals sp.MaSanPham
                                  where sp.MaCuaHang == pMaCuaHang
                     select dh;
            return rs.ToList();
        }
        public List<DonHang> DanhSachDonHangTheoNguoiMua(int pMaNM)
        {
            var rs = from dh in db.DonHangs
                     join ctdh in db.ChiTietDonHangs on dh.MaDonHang equals ctdh.MaDonHang
                     join ctgh in db.ChiTietGioHangs on ctdh.MaCTGH equals ctgh.MaCTGH
                     join gh in db.GioHangs on ctgh.MaGioHang equals gh.MaGioHang
                     
                     where gh.MaNguoiMua == pMaNM
                     select dh;
            return rs.ToList();
        }
        public bool ThayDoiTrangThaiCuaHang(int pMaCuaHang)
        {
            try
            {
                CuaHang ch = db.CuaHangs.FirstOrDefault(t => t.MaCuaHang == pMaCuaHang);
                ch.TrangThai = !ch.TrangThai;
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }
        public List<NguoiMua> DanhSachNguoiMua()
        {
            return db.NguoiMuas.ToList();
        }

        public List<DanhMuc> DanhSachDanhMuc()
        {
            return db.DanhMucs.ToList();
        }

        public List<SanPham> DanhSachSanPham()
        {
            return db.SanPhams.ToList();
        }
    }
}