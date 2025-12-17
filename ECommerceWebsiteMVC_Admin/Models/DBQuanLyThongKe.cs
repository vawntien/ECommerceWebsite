using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class DBQuanLyThongKe
    {
        ECommerceWebsiteEntities db;
        DBQuanLyNguoiDungHeThong qlndht = new DBQuanLyNguoiDungHeThong();

        public DBQuanLyThongKe()
        {
            db = new ECommerceWebsiteEntities();
        }
        public int TongDoanhThu()
        {
            return (int)db.DonHangs.Where(t=>t.TrangThaiDonHang == "Đã giao").Sum(t=>t.TongTien);
        }
        public int DonHangThanhCong()
        {
            return (int)db.DonHangs.Where(t => t.TrangThaiDonHang == "Đã giao").Count();
        }
        public int DonHangThatBai()
        {
            return (int)db.DonHangs.Where(t => t.TrangThaiDonHang == "Đã hủy").Count();
        }
        public List<ThongKeThang> GetDoanhThu6Thang()
        {
            DateTime now = DateTime.Now;
            DateTime startDate = now.AddMonths(-5);

            var data = db.DonHangs
                .Where(d => d.ThoiGianDat >= startDate && d.TrangThaiDonHang == "Đã giao")
                .GroupBy(d => new { d.ThoiGianDat.Year, d.ThoiGianDat.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.TongTien)
                })
                .ToList();

            List<ThongKeThang> result = new List<ThongKeThang>();

            for (int i = 5; i >= 0; i--)
            {
                int m = now.AddMonths(-i).Month;
                int y = now.AddMonths(-i).Year;

                var found = data.FirstOrDefault(x => x.Month == m && x.Year == y);

                result.Add(new ThongKeThang
                {
                    Thang = $"{m}/{y}",
                    TongTien = found != null ? (int)found.Total : 0
                });
            }

            return result;
        }

        public class ThongKeDonHang
        {
            public string Thang { get; set; }
            public int SoLuong { get; set; }
        }

        public List<ThongKeDonHang> GetSoDon6Thang()
        {
            DateTime now = DateTime.Now;
            DateTime startDate = now.AddMonths(-5);

            var data = db.DonHangs
                .Where(d => d.ThoiGianDat >= startDate)
                .GroupBy(d => new { d.ThoiGianDat.Year, d.ThoiGianDat.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Count()
                })
                .ToList();

            List<ThongKeDonHang> result = new List<ThongKeDonHang>();

            for (int i = 5; i >= 0; i--)
            {
                int m = now.AddMonths(-i).Month;
                int y = now.AddMonths(-i).Year;

                var found = data.FirstOrDefault(x => x.Month == m && x.Year == y);

                result.Add(new ThongKeDonHang
                {
                    Thang = $"{m}/{y}",
                    SoLuong = found != null ? found.Total : 0
                });
            }

            return result;
        }

        public List<DonHang> DanhSachDonHangTheoCuaHang(int pMaCH)
        {

            return qlndht.DanhSachDonHangTheoCuaHang(pMaCH);
        }

        public List<CuaHang> DanhSachCuaHang()
        {
            return qlndht.DanhSachCuaHang();
        }
        public List<DonHang> DanhSachDonHangTheoNganhHang(int pMaDM)
        {
            var rs = from dh in db.DonHangs
                         join ctdh in db.ChiTietDonHangs on dh.MaDonHang equals ctdh.MaDonHang
                         join ctgh in db.ChiTietGioHangs on ctdh.MaCTGH equals ctgh.MaCTGH
                         join bt in db.BienTheSanPhams on ctgh.MaBienThe equals bt.MaBienThe
                         join sp in db.SanPhams on bt.MaSanPham equals sp.MaSanPham
                         where sp.MaDanhMuc == pMaDM
                     select dh;

            return rs.ToList();
        }
        public List<DanhMuc> DanhSachDanhMuc()
        {
            return db.DanhMucs.ToList();
        }
    }
}