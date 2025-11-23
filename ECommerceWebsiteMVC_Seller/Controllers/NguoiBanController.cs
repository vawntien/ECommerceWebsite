using ECommerceWebsiteMVC_Seller.Models;
using System;
using System.Collections.Generic;
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
            if (Session["MaNguoiDung"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            int maNguoiDung = (int)Session["MaNguoiDung"];

            // 1. Lấy cửa hàng của người bán
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiDung == maNguoiDung);

            if (cuaHang == null)
                return Content("Bạn chưa tạo cửa hàng.");

            int maCuaHang = cuaHang.MaCuaHang;

            // 2. Lấy sản phẩm thuộc cửa hàng đó
            var dsSanPham = db.SanPhams
                              .Where(sp => sp.MaCuaHang == maCuaHang)
                              .ToList();

            return View(dsSanPham);
        }


    }
}