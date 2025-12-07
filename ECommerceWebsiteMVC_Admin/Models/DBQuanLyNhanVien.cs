using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class DBQuanLyNhanVien
    {
        ECommerceWebsiteEntities db;

        public DBQuanLyNhanVien()
        {
            db = new ECommerceWebsiteEntities();
        }

        public List<QuyenHeThong> DanhSachQuyenHeThong()
        {
            return db.QuyenHeThongs.ToList();
        }
        public List<NhanVien> DanhSachNhanVien()
        {
            return db.NhanViens.ToList();
        }
        public bool ThemNhanVien(NhanVien nv)
        {
            try
            {
                db.NhanViens.Add(nv);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SuaNhanVien(NhanVien nv)
        {
            try
            {
                var old = db.NhanViens.FirstOrDefault(x => x.MaNhanVien == nv.MaNhanVien);
                if (old == null) return false;

                // Kiểm tra trùng Email
                if (db.NhanViens.Any(x => x.Email == nv.Email && x.MaNhanVien != nv.MaNhanVien))
                    return false;

                // Kiểm tra trùng Username
                if (db.NhanViens.Any(x => x.TaiKhoan == nv.TaiKhoan && x.MaNhanVien != nv.MaNhanVien))
                    return false;

                // Kiểm tra trùng SDT
                if (db.NhanViens.Any(x => x.SDT == nv.SDT && x.MaNhanVien != nv.MaNhanVien))
                    return false;

                // Kiểm tra trùng CCCD
                if (db.NhanViens.Any(x => x.CCCD == nv.CCCD && x.MaNhanVien != nv.MaNhanVien))
                    return false;

                // Cập nhật dữ liệu
                old.HoVaTen = nv.HoVaTen;
                old.Email = nv.Email;
                old.TaiKhoan = nv.TaiKhoan;
                old.SDT = nv.SDT;
                old.CCCD = nv.CCCD;

                // Nếu có thay đổi mật khẩu (trường hợp sửa)
                if (!string.IsNullOrEmpty(nv.MatKhau))
                    old.MatKhau = nv.MatKhau;

                old.MaQuyenHeThong = nv.MaQuyenHeThong;

                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool XoaNhanVien(int id)
        {
            try
            {
                var nv = db.NhanViens.FirstOrDefault(x => x.MaNhanVien == id);
                if (nv == null) return false;

                db.NhanViens.Remove(nv);
                db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }



    }
}