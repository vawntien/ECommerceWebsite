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
    }
}