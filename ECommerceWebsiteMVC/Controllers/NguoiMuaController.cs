using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class NguoiMuaController : Controller
    {
        ECommerceWebsiteEntities ql = new ECommerceWebsiteEntities();
        // GET: Home
        public ActionResult Index()
        {
            //ViewBag.SanPhamHot = GetTopSanPhamHot(10);

            //ViewBag.DanhMuc = ql.DanhMucs.ToList();


            //var allIds = ql.SanPhams
            //    .Select(sp => sp.MaSanPham)
            //    .ToList();

            //if (!allIds.Any())
            //    return View(new List<SanPham>());


            //Random rnd = new Random();
            //var randomIds = allIds
            //    .OrderBy(x => rnd.Next())
            //    .Take(24) 
            //    .ToList();

            //List<SanPham> dssp = ql.SanPhams
            //    .Include("AnhSanPhams")
            //    .Where(sp => randomIds.Contains(sp.MaSanPham))
            //    .ToList();

            //dssp = dssp
            //    .OrderBy(sp => randomIds.IndexOf(sp.MaSanPham))
            //    .ToList();


            //return View(dssp);


            ViewBag.SanPhamHot = GetTopSanPhamHot(10);
            ViewBag.DanhMuc = ql.DanhMucs.ToList();

            // 🔥 RANDOM 1 LẦN DUY NHẤT
            if (Session["RandomProductIds"] == null)
            {
                var allIds = ql.SanPhams
                    .Select(sp => sp.MaSanPham)
                    .ToList();

                if (!allIds.Any())
                    return View(new List<SanPham>());

                Random rnd = new Random();
                var shuffledIds = allIds
                    .OrderBy(x => rnd.Next())
                    .ToList();

                Session["RandomProductIds"] = shuffledIds;
            }

            var randomIds = Session["RandomProductIds"] as List<int>;

            var firstIds = randomIds.Take(24).ToList();

            List<SanPham> dssp = ql.SanPhams
                .Include("AnhSanPhams")
                .Where(sp => firstIds.Contains(sp.MaSanPham))
                .ToList();

            // giữ đúng thứ tự random
            dssp = dssp
                .OrderBy(sp => firstIds.IndexOf(sp.MaSanPham))
                .ToList();

            return View(dssp);
        }


        public ActionResult DonHang(string trangThai = null)
        {
            // Lấy MaNguoiMua từ Session (người dùng đã đăng nhập)
            int? maNguoiMua = Session["MaNguoiMua"] as int?;

            // Lấy tất cả đơn hàng với các thông tin liên quan
            var query = ql.DonHangs
    .Include("ChiTietDonHangs.ChiTietGioHang.BienTheSanPham.SanPham.AnhSanPhams")
    .Include("DonViVanChuyen")
    .Where(d => d.ChiTietDonHangs
                .Any(ct => ct.ChiTietGioHang.GioHang.MaNguoiMua == maNguoiMua.Value));

            // Lọc theo người dùng hiện tại thông qua giỏ hàng của từng chi tiết
            if (maNguoiMua.HasValue)
            {
                query = query.Where(d => d.ChiTietDonHangs
                    .Any(ct => ct.ChiTietGioHang.GioHang.MaNguoiMua == maNguoiMua.Value));
            }

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThaiDonHang == trangThai);
            }

            var donHangs = query
                .OrderByDescending(d => d.ThoiGianDat)
                .ToList();

            // Gán MaNguoiMua cho từng đơn hàng để dùng ở View hoặc các xử lý khác
            foreach (var donHang in donHangs)
            {
                donHang.MaNguoiMua = donHang.ChiTietDonHangs
                    .Select(ct => (int?)ct.ChiTietGioHang.GioHang.MaNguoiMua)
                    .FirstOrDefault();
            }

            ViewBag.TrangThaiHienTai = trangThai;
            return View(donHangs);
        }
        public ActionResult ChiTietDonHang(int id)
        {
            // Lấy thông tin chi tiết đơn hàng với tất cả dữ liệu liên quan
            var donHang = ql.DonHangs
                .Include("ChiTietDonHangs.ChiTietGioHang.BienTheSanPham.SanPham.AnhSanPhams")
                .Include("ChiTietDonHangs.ChiTietGioHang.BienTheSanPham.SanPham.CuaHang")
                .Include("DonViVanChuyen")
                .Include("GiamGia")
                .Include("ChiTietDonHangs.ChiTietGioHang.GioHang.NguoiMua")
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem đơn hàng có thuộc về người dùng hiện tại không
            int? maNguoiMua = Session["MaNguoiMua"] as int?;
            //if (maNguoiMua.HasValue && donHang.MaNguoiMua != maNguoiMua.Value)
            //{
            //    return new HttpUnauthorizedResult();
            //}

            return View(donHang);
        }

        [HttpPost]
        public ActionResult HuyDonHang(int maDonHang, string lyDoHuy)
        {
            try
            {
                // Lấy MaNguoiMua từ Session
                int? maNguoiMua = Session["MaNguoiMua"] as int?;
                if (!maNguoiMua.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này." });
                }

                // ---Include các bảng liên quan để lấy được thông tin Biến thể sản phẩm ---
                var donHang = ql.DonHangs
                    .Include("ChiTietDonHangs.ChiTietGioHang.BienTheSanPham")
                    .FirstOrDefault(d => d.MaDonHang == maDonHang);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Kiểm tra quyền sở hữu (bạn có thể bỏ comment nếu cần)
                //if (donHang.ChiTietDonHangs.FirstOrDefault()?.ChiTietGioHang.GioHang.MaNguoiMua != maNguoiMua)
                //{
                //    return Json(new { success = false, message = "Bạn không có quyền hủy đơn hàng này." });
                //}

                // Chỉ cho phép hủy đơn hàng có trạng thái "Chờ xác nhận"
                if (donHang.TrangThaiDonHang != "Chờ xác nhận")
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng đang ở trạng thái 'Chờ xác nhận'." });
                }

                if (donHang.ChiTietDonHangs != null)
                {
                    foreach (var chiTiet in donHang.ChiTietDonHangs)
                    {
                        // Lấy ra biến thể sản phẩm tương ứng
                        var bienThe = chiTiet.ChiTietGioHang.BienTheSanPham;

                        // Lấy số lượng khách đã mua trong đơn hàng này
                        int soLuongMua = chiTiet.ChiTietGioHang.SoLuong;


                        if (bienThe != null)
                        {
                            // Cộng lại vào kho
                            bienThe.SoLuongTonKho = bienThe.SoLuongTonKho + soLuongMua;
                        }
                    }
                }

                // Cập nhật trạng thái đơn hàng
                donHang.TrangThaiDonHang = "Đã hủy";
                donHang.GhiChu = string.IsNullOrEmpty(lyDoHuy) ? "Người mua đã hủy đơn hàng" : $"Lý do hủy: {lyDoHuy}";

                ql.SaveChanges();

                return Json(new { success = true, message = "Hủy đơn hàng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        public ActionResult ChiTietHuyDon(int id)
        {
            // Lấy thông tin chi tiết đơn hàng đã hủy
            var donHang = ql.DonHangs
                .Include("ChiTietDonHangs.ChiTietGioHang.BienTheSanPham.SanPham.AnhSanPhams")
                .Include("ChiTietDonHangs.ChiTietGioHang.BienTheSanPham.SanPham.CuaHang")
                .Include("DonViVanChuyen")
                .Include("GiamGia")
                .Include("ChiTietDonHangs.ChiTietGioHang.GioHang.NguoiMua")
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem đơn hàng có thuộc về người dùng hiện tại không
            //int? maNguoiMua = Session["MaNguoiMua"] as int?;
            //if (maNguoiMua.HasValue && donHang.MaNguoiMua != maNguoiMua.Value)
            //{
            //    return new HttpUnauthorizedResult();
            //}

            // Kiểm tra xem đơn hàng có bị hủy không
            if (donHang.TrangThaiDonHang != "Đã hủy")
            {
                return RedirectToAction("ChiTietDonHang", new { id = id });
            }

            return View(donHang);
        }

        public ActionResult TimKiem(string keyword)
        {
            ViewBag.DanhMuc = ql.DanhMucs.ToList();
            ViewBag.Keyword = keyword;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                var tatCa = ql.SanPhams
                    .Include("AnhSanPhams")
                    .ToList();

                return View("Index", tatCa);
            }

            keyword = keyword.Trim();

            var sanPhamTimDuoc = ql.SanPhams
                .Include("AnhSanPhams")
                .Include("DanhMuc")
                .Where(sp =>
                    sp.TenSanPham.Contains(keyword) ||
                    sp.DanhMuc.TenDanhMuc.Contains(keyword)
                )
                .ToList();

            return View("Index", sanPhamTimDuoc);
        }

        private List<SanPham> GetTopSanPhamHot(int top = 7)
        {
            var sanPhamHot = ql.ChiTietDonHangs
                .Where(ct =>
                    ct.DonHang.TrangThaiDonHang == "Chờ giao hàng" ||
                    ct.DonHang.TrangThaiDonHang == "Đã giao"
                )
                .GroupBy(ct => ct.ChiTietGioHang.BienTheSanPham.MaSanPham)
                .Select(g => new
                {
                    MaSanPham = g.Key,
                    SoLuongBan = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(top)
                .Join(
                    ql.SanPhams.Include("AnhSanPhams"),
                    hot => hot.MaSanPham,
                    sp => sp.MaSanPham,
                    (hot, sp) => sp
                )
                .ToList();

            return sanPhamHot;
        }


        [HttpGet]
        public ActionResult LoadMoreProducts(int skip)
        {
            var randomIds = Session["RandomProductIds"] as List<int>;
            if (randomIds == null)
                return Content("");

            var nextIds = randomIds
                .Skip(skip)
                .Take(24)
                .ToList();

            if (!nextIds.Any())
                return Content("");

            var products = ql.SanPhams
                .Include("AnhSanPhams")
                .Where(sp => nextIds.Contains(sp.MaSanPham))
                .ToList();

            products = products
                .OrderBy(sp => nextIds.IndexOf(sp.MaSanPham))
                .ToList();

            return PartialView("_ProductListPartial", products);
        }



    }
}