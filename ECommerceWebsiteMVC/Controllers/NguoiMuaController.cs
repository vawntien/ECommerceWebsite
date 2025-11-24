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
            ViewBag.DanhMuc = ql.DanhMucs.ToList();
            List<SanPham> dssp = ql.SanPhams.Include("AnhSanPhams").ToList();
            return View(dssp);
        }

        public ActionResult DonHang(string trangThai = null)
        {
            // Lấy MaNguoiDung từ Session (người dùng đã đăng nhập)
            int? maNguoiDung = Session["MaNguoiDung"] as int?;
            
            // Lấy tất cả đơn hàng của người dùng với các thông tin liên quan
            var query = ql.DonHangs
                .Include("ChiTietDonHangs.BienTheSanPham.SanPham.AnhSanPhams")
                .Include("ChiTietDonHangs.BienTheSanPham.SanPham.CuaHang")
                .Include("DonViVanChuyen")
                .AsQueryable();

            // LỌC THEO NGƯỜI DÙNG - Chỉ hiển thị đơn hàng của người dùng hiện tại
            if (maNguoiDung.HasValue)
            {
                query = query.Where(d => d.MaNguoiDung == maNguoiDung.Value);
            }

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThaiDonHang == trangThai);
            }

            var donHangs = query
                .OrderByDescending(d => d.ThoiGianDat)
                .ToList();

            ViewBag.TrangThaiHienTai = trangThai;
            return View(donHangs);
        }
        public ActionResult ChiTietDonHang(int id)
        {
            // Lấy thông tin chi tiết đơn hàng với tất cả dữ liệu liên quan
            var donHang = ql.DonHangs
                .Include("ChiTietDonHangs.BienTheSanPham.SanPham.AnhSanPhams")
                .Include("ChiTietDonHangs.BienTheSanPham.SanPham.CuaHang")
                .Include("DonViVanChuyen")
                .Include("GiamGia")
                .Include("NguoiDung")
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem đơn hàng có thuộc về người dùng hiện tại không
            int? maNguoiDung = Session["MaNguoiDung"] as int?;
            if (maNguoiDung.HasValue && donHang.MaNguoiDung != maNguoiDung.Value)
            {
                return new HttpUnauthorizedResult();
            }

            return View(donHang);
        }

        [HttpPost]
        public ActionResult HuyDonHang(int maDonHang, string lyDoHuy)
        {
            try
            {
                // Lấy MaNguoiDung từ Session
                int? maNguoiDung = Session["MaNguoiDung"] as int?;
                if (!maNguoiDung.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này." });
                }

                // Tìm đơn hàng
                var donHang = ql.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Kiểm tra quyền sở hữu
                if (donHang.MaNguoiDung != maNguoiDung.Value)
                {
                    return Json(new { success = false, message = "Bạn không có quyền hủy đơn hàng này." });
                }

                // Chỉ cho phép hủy đơn hàng có trạng thái "Chờ xác nhận"
                if (donHang.TrangThaiDonHang != "Chờ xác nhận")
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng đang ở trạng thái 'Chờ xác nhận'." });
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
                .Include("ChiTietDonHangs.BienTheSanPham.SanPham.AnhSanPhams")
                .Include("ChiTietDonHangs.BienTheSanPham.SanPham.CuaHang")
                .Include("DonViVanChuyen")
                .Include("GiamGia")
                .Include("NguoiDung")
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem đơn hàng có thuộc về người dùng hiện tại không
            int? maNguoiDung = Session["MaNguoiDung"] as int?;
            if (maNguoiDung.HasValue && donHang.MaNguoiDung != maNguoiDung.Value)
            {
                return new HttpUnauthorizedResult();
            }

            // Kiểm tra xem đơn hàng có bị hủy không
            if (donHang.TrangThaiDonHang != "Đã hủy")
            {
                return RedirectToAction("ChiTietDonHang", new { id = id });
            }

            return View(donHang);
        }
    }
}