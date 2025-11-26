using ECommerceWebsiteMVC_Seller.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class NguoiBanController : Controller
    {
        // GET: NguoiBan

        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DonHang()
        {
            return View();
        }

        public ActionResult TatCaSanPham()
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            int maNguoiBan = (int)Session["MaNguoiBan"];

            // 1. Lấy cửa hàng của người bán
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);

            if (cuaHang == null)
                return Content("Bạn chưa tạo cửa hàng.");

            int maCuaHang = cuaHang.MaCuaHang;

            // 2. Lấy sản phẩm thuộc cửa hàng đó với các thông tin liên quan
            var dsSanPham = db.SanPhams
                              .Include("AnhSanPhams")
                              .Include("BienTheSanPhams")
                              .Where(sp => sp.MaCuaHang == maCuaHang)
                              .OrderByDescending(sp => sp.MaSanPham)
                              .ToList();

            // 3. Tính doanh số cho mỗi sản phẩm
            var productSales = new Dictionary<int, int>();
            
            if (dsSanPham.Any())
            {
                var allBienTheIds = dsSanPham.SelectMany(sp => sp.BienTheSanPhams?.Select(bt => bt.MaBienThe) ?? new List<int>()).ToList();
                
                if (allBienTheIds.Any())
                {
                    var salesData = db.ChiTietDonHangs
                        .Include("DonHang")
                        .Include("BienTheSanPham")
                        .Where(ct => allBienTheIds.Contains(ct.MaBienThe) 
                            && ct.DonHang != null
                            && (ct.DonHang.TrangThaiDonHang == "Đã giao" || ct.DonHang.TrangThaiDonHang == "Đang giao"))
                        .ToList()
                        .GroupBy(ct => ct.BienTheSanPham?.MaSanPham ?? 0)
                        .Where(g => g.Key > 0)
                        .Select(g => new { MaSanPham = g.Key, SoLuongBan = g.Sum(x => x.SoLuong) })
                        .ToList();

                    foreach (var sale in salesData)
                    {
                        productSales[sale.MaSanPham] = sale.SoLuongBan;
                    }
                }
            }

            ViewBag.ProductSales = productSales;

            return View(dsSanPham);
        }

        public ActionResult CapNhatSanPham(int id)
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            int maNguoiBan = (int)Session["MaNguoiBan"];

            // Kiểm tra sản phẩm có thuộc về người bán này không
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
                return Content("Bạn chưa tạo cửa hàng.");

            var sanPham = db.SanPhams
                .Include("AnhSanPhams")
                .Include("BienTheSanPhams")
                .Include("DanhMuc")
                .SingleOrDefault(sp => sp.MaSanPham == id && sp.MaCuaHang == cuaHang.MaCuaHang);

            if (sanPham == null)
                return HttpNotFound("Sản phẩm không tồn tại hoặc không thuộc về bạn.");

            ViewBag.DanhMucs = db.DanhMucs.ToList();

            return View(sanPham);
        }

    }
}