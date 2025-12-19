using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class DBQuanLyKhuyenMai
    {
        ECommerceWebsiteEntities db;
        
        public DBQuanLyKhuyenMai()
        {
            db = new ECommerceWebsiteEntities();
        }

        public List<GiamGia> DanhSachKhuyenMai()
        {
            return db.GiamGias.OrderByDescending(x => x.MaGiamGia).ToList();
        }

        public GiamGia LayKhuyenMaiTheoMa(int maGiamGia)
        {
            return db.GiamGias.FirstOrDefault(x => x.MaGiamGia == maGiamGia);
        }

        public bool ThemKhuyenMai(GiamGia gg)
        {
            try
            {
                db.GiamGias.Add(gg);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool SuaKhuyenMai(GiamGia gg)
        {
            try
            {
                var existing = db.GiamGias.Find(gg.MaGiamGia);
                if (existing == null) return false;

                existing.TenMaGG = gg.TenMaGG;
                existing.MoTa = gg.MoTa;
                existing.GiaTriDonHangToiThieu = gg.GiaTriDonHangToiThieu;
                existing.GiaTriGiamToiDa = gg.GiaTriGiamToiDa;
                existing.GiaTriGiam = gg.GiaTriGiam;
                existing.NgayBD = gg.NgayBD;
                existing.NgayKT = gg.NgayKT;

                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool XoaKhuyenMai(int maGiamGia)
        {
            try
            {
                var gg = db.GiamGias.Find(maGiamGia);
                if (gg == null) return false;

                // Kiểm tra xem có đơn hàng nào đang sử dụng voucher này không
                if (gg.DonHangs != null && gg.DonHangs.Any())
                {
                    return false; // Không thể xóa nếu có đơn hàng đã sử dụng
                }

                db.GiamGias.Remove(gg);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        // Ẩn khuyến mãi bằng cách đặt ngày kết thúc về quá khứ
        public bool AnKhuyenMai(int maGiamGia)
        {
            try
            {
                var gg = db.GiamGias.Find(maGiamGia);
                if (gg == null) return false;

                // Đặt ngày kết thúc về quá khứ để ẩn
                gg.NgayKT = DateTime.Now.AddDays(-1);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        // Hiện khuyến mãi bằng cách đặt ngày kết thúc về tương lai
        public bool HienKhuyenMai(int maGiamGia, DateTime? ngayKT)
        {
            try
            {
                var gg = db.GiamGias.Find(maGiamGia);
                if (gg == null) return false;

                // Đặt ngày kết thúc về tương lai hoặc null
                if (ngayKT.HasValue && ngayKT.Value > DateTime.Now)
                {
                    gg.NgayKT = ngayKT.Value;
                }
                else
                {
                    gg.NgayKT = DateTime.Now.AddMonths(1); // Mặc định 1 tháng
                }
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        // Kiểm tra khuyến mãi có đang hiệu lực không
        public bool KiemTraHieuLuc(int maGiamGia)
        {
            var gg = db.GiamGias.Find(maGiamGia);
            if (gg == null) return false;

            DateTime now = DateTime.Now;
            bool validStart = gg.NgayBD == null || gg.NgayBD <= now;
            bool validEnd = gg.NgayKT == null || gg.NgayKT >= now;

            return validStart && validEnd;
        }
    }
}

