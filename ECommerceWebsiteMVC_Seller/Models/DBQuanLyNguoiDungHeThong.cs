using ECommerceWebsiteMVC_Seller.Models;
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

        public bool ThayDoiTrangThaiNguoiMua(int maNguoiMua)
        {
            try
            {
                var nm = db.NguoiMuas.FirstOrDefault(x => x.MaNguoiMua == maNguoiMua);
                if (nm == null) return false;

                nm.TrangThai = !nm.TrangThai;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
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
        public bool ThayDoiTrangThaiCuaHang(int pMaCuaHang, out string message)
        {
            message = "";

            try
            {
                // 1. Lấy cửa hàng
                CuaHang ch = db.CuaHangs.FirstOrDefault(t => t.MaCuaHang == pMaCuaHang);
                if (ch == null)
                {
                    message = "Cửa hàng không tồn tại!";
                    return false;
                }

                // 2. Nếu đang MỞ → cho KHÓA luôn
                if (ch.TrangThai == true)
                {
                    ch.TrangThai = false;
                    db.SaveChanges();
                    message = "Đã khóa cửa hàng thành công.";
                    return true;
                }

                // 3. Nếu đang KHÓA → kiểm tra người bán
                var nguoiBan = db.NguoiBans.FirstOrDefault(nb => nb.MaNguoiBan == ch.MaNguoiBan);

                if (nguoiBan == null || nguoiBan.TrangThai == false)
                {
                    message = "Không thể mở cửa hàng vì người bán đang bị khóa. Vui lòng mở người bán trước!";
                    return false;
                }

                // 4. Người bán đang hoạt động → cho mở cửa hàng
                ch.TrangThai = true;
                db.SaveChanges();
                message = "Đã mở khóa cửa hàng thành công.";
                return true;
            }
            catch
            {
                message = "Có lỗi xảy ra khi thay đổi trạng thái cửa hàng!";
                return false;
            }
        }


        public bool ThayDoiTrangThaiNguoiBan(int maNguoiBan)
        {
            try
            {
                // 1. Lấy người bán
                var nguoiBan = db.NguoiBans.FirstOrDefault(x => x.MaNguoiBan == maNguoiBan);
                if (nguoiBan == null) return false;

                // 2. Đảo trạng thái người bán
                bool trangThaiMoi = !nguoiBan.TrangThai;
                nguoiBan.TrangThai = trangThaiMoi;

                // 3. Nếu KHÓA người bán → KHÓA toàn bộ cửa hàng
                if (trangThaiMoi == false)
                {
                    var dsCuaHang = db.CuaHangs
                                      .Where(ch => ch.MaNguoiBan == maNguoiBan)
                                      .ToList();

                    foreach (var ch in dsCuaHang)
                    {
                        ch.TrangThai = false;
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
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