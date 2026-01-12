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

                if (gg.DonHangs != null && gg.DonHangs.Any())
                {
                    return false;
                }

                db.GiamGias.Remove(gg);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool AnKhuyenMai(int maGiamGia)
        {
            try
            {
                var gg = db.GiamGias.Find(maGiamGia);
                if (gg == null) return false;

                gg.NgayKT = DateTime.Now.AddDays(-1);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool HienKhuyenMai(int maGiamGia, DateTime? ngayKT)
        {
            try
            {
                var gg = db.GiamGias.Find(maGiamGia);
                if (gg == null) return false;

                if (ngayKT.HasValue && ngayKT.Value > DateTime.Now)
                {
                    gg.NgayKT = ngayKT.Value;
                }
                else
                {
                    gg.NgayKT = DateTime.Now.AddMonths(1);
                }
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool KiemTraHieuLuc(int maGiamGia)
        {
            var gg = db.GiamGias.Find(maGiamGia);
            if (gg == null) return false;

            DateTime now = DateTime.Now;
            bool validStart = gg.NgayBD == null || gg.NgayBD <= now;
            bool validEnd = gg.NgayKT == null || gg.NgayKT >= now;

            return validStart && validEnd;
        }

        private decimal TinhSoTienGiamGia(GiamGia voucher, decimal tongTienHang)
        {
            if (voucher == null || voucher.GiaTriGiam == null) return 0;

            decimal giamGia = 0;
            if (voucher.GiaTriGiam <= 1)
            {
                giamGia = tongTienHang * (decimal)voucher.GiaTriGiam;
                if (voucher.GiaTriGiamToiDa > 0 && giamGia > voucher.GiaTriGiamToiDa)
                    giamGia = voucher.GiaTriGiamToiDa;
            }
            else
            {
                giamGia = (decimal)voucher.GiaTriGiam;
            }

            return giamGia;
        }

        private decimal TinhTongTienHang(DonHang donHang)
        {
            return donHang.ChiTietDonHangs?.Sum(ct => ct.ThanhTien) ?? 0;
        }

        public ThongKeKhuyenMai LayThongKeKhuyenMai(DateTime? tuNgay = null, DateTime? denNgay = null, int? maVoucher = null)
        {
            var viewModel = new ThongKeKhuyenMai
            {
                LichSuSuDung = new List<LichSuSuDungVoucher>(),
                XepHangVoucher = new List<VoucherHieuQua>(),
                DuLieuBieuDoThoiGian = new List<ThongKeTheoThoiGian>(),
                DanhSachVoucher = db.GiamGias
                    .Where(g => g.TenMaGG != null)
                    .OrderBy(g => g.TenMaGG.StartsWith("[CAMPAIGN]") ? 1 : 0)
                    .ThenBy(g => g.TenMaGG)
                    .ToList()
            };

            var donHangsCoVoucher = db.DonHangs
                .Where(dh => dh.MaGiamGia != null)
                .ToList();

            if (maVoucher.HasValue)
            {
                donHangsCoVoucher = donHangsCoVoucher
                    .Where(dh => dh.MaGiamGia == maVoucher.Value)
                    .ToList();
            }

            if (tuNgay.HasValue)
            {
                donHangsCoVoucher = donHangsCoVoucher
                    .Where(dh => dh.ThoiGianDat >= tuNgay.Value)
                    .ToList();
            }

            if (denNgay.HasValue)
            {
                donHangsCoVoucher = donHangsCoVoucher
                    .Where(dh => dh.ThoiGianDat < denNgay.Value)
                    .ToList();
            }

            decimal tongTienGiam = 0;
            decimal tongDoanhThu = 0;

            foreach (var donHang in donHangsCoVoucher)
            {
                var voucher = donHang.GiamGia;
                if (voucher == null) continue;

                decimal tongTienHang = TinhTongTienHang(donHang);
                decimal soTienGiam = TinhSoTienGiamGia(voucher, tongTienHang);
                string tenKhuyenMai = voucher.TenMaGG ?? "Không có tên";

                tongTienGiam += soTienGiam;
                tongDoanhThu += donHang.TongTien;

                var nguoiMua = donHang.ChiTietDonHangs
                    .Select(ct => ct.ChiTietGioHang?.GioHang?.NguoiMua)
                    .FirstOrDefault(nm => nm != null);

                viewModel.LichSuSuDung.Add(new LichSuSuDungVoucher
                {
                    MaDonHang = donHang.MaDonHang,
                    TenKhachHang = nguoiMua?.HoVaTen ?? donHang.TenNguoiNhan ?? "Không xác định",
                    Email = nguoiMua?.Email ?? "Không xác định",
                    TenVoucher = tenKhuyenMai,
                    SoTienGiam = soTienGiam,
                    TongTienDonHang = donHang.TongTien,
                    ThoiGianDat = donHang.ThoiGianDat,
                    TrangThaiDonHang = donHang.TrangThaiDonHang ?? "Không xác định"
                });
            }

            viewModel.TongTienGiamChoKhachHang = tongTienGiam;
            viewModel.TongDoanhThuTuVoucher = tongDoanhThu;
            viewModel.TongSoDonHangSuDungVoucher = donHangsCoVoucher.Count;

            var voucherStats = donHangsCoVoucher
                .Where(dh => dh.GiamGia != null)
                .GroupBy(dh => dh.GiamGia)
                .Select(g => new
                {
                    Voucher = g.Key,
                    SoLanSuDung = g.Count(),
                    DonHangs = g.ToList()
                })
                .OrderByDescending(x => x.SoLanSuDung)
                .ToList();

            DateTime now = DateTime.Now;
            foreach (var stat in voucherStats)
            {
                decimal tongTienGiamVoucher = 0;
                decimal tongDoanhThuVoucher = 0;

                foreach (var donHang in stat.DonHangs)
                {
                    decimal tongTienHang = TinhTongTienHang(donHang);
                    decimal soTienGiam = TinhSoTienGiamGia(stat.Voucher, tongTienHang);
                    tongTienGiamVoucher += soTienGiam;
                    tongDoanhThuVoucher += donHang.TongTien;
                }

                string trangThai = "Không xác định";
                if (stat.Voucher.NgayBD != null && stat.Voucher.NgayBD > now)
                    trangThai = "Sắp diễn ra";
                else if (stat.Voucher.NgayKT != null && stat.Voucher.NgayKT < now)
                    trangThai = "Hết hạn";
                else if ((stat.Voucher.NgayBD == null || stat.Voucher.NgayBD <= now) &&
                         (stat.Voucher.NgayKT == null || stat.Voucher.NgayKT >= now))
                    trangThai = "Đang hiệu lực";

                viewModel.XepHangVoucher.Add(new VoucherHieuQua
                {
                    MaGiamGia = stat.Voucher.MaGiamGia,
                    TenMaGG = stat.Voucher.TenMaGG ?? "Không có tên",
                    SoLanSuDung = stat.SoLanSuDung,
                    TongTienGiam = tongTienGiamVoucher,
                    TongDoanhThu = tongDoanhThuVoucher,
                    NgayBD = stat.Voucher.NgayBD,
                    NgayKT = stat.Voucher.NgayKT,
                    TrangThai = trangThai
                });
            }

            viewModel.LichSuSuDung = viewModel.LichSuSuDung
                .OrderByDescending(ls => ls.ThoiGianDat)
                .ToList();

            var thongKeTheoNgay = donHangsCoVoucher
                .GroupBy(dh => dh.ThoiGianDat.Date)
                .Select(g => new
                {
                    Ngay = g.Key,
                    DonHangs = g.ToList()
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            foreach (var item in thongKeTheoNgay)
            {
                decimal tongTienGiamNgay = 0;
                decimal tongDoanhThuNgay = 0;

                foreach (var donHang in item.DonHangs)
                {
                    var voucher = donHang.GiamGia;
                    if (voucher != null)
                    {
                        decimal tongTienHang = TinhTongTienHang(donHang);
                        decimal soTienGiam = TinhSoTienGiamGia(voucher, tongTienHang);
                        tongTienGiamNgay += soTienGiam;
                    }
                    tongDoanhThuNgay += donHang.TongTien;
                }

                viewModel.DuLieuBieuDoThoiGian.Add(new ThongKeTheoThoiGian
                {
                    Ngay = item.Ngay.ToString("dd/MM/yyyy"),
                    SoLuongDonHang = item.DonHangs.Count,
                    TongTienGiam = tongTienGiamNgay,
                    TongDoanhThu = tongDoanhThuNgay
                });
            }

            return viewModel;
        }
    }
}


