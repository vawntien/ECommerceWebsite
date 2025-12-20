using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient; 


namespace ECommerceWebsiteMVC.Controllers
{
    public class SanPhamController : Controller
    {
        // GET: SanPham
        ECommerceWebsiteEntities ql = new ECommerceWebsiteEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DSSanPham()
        {
            return View();
        }

        public ActionResult ChiTietSanPham(int id)
        {
            var sp = ql.SanPhams.Include("AnhSanPhams").Include("BienTheSanPhams").Where(s => s.MaSanPham == id).FirstOrDefault();
            if (sp == null) return HttpNotFound();

            var sanPhamLienQuan = ql.SanPhams
        .Include("AnhSanPhams")
        .Where(x =>
            x.MaDanhMuc == sp.MaDanhMuc &&
            x.MaSanPham != sp.MaSanPham
        )
        .OrderBy(x => Guid.NewGuid()) 
        .Take(5)
        .ToList();

            ViewBag.SanPhamLienQuan = sanPhamLienQuan;
            return View(sp);
        }

        // File: Controllers/SanPhamController.cs

        [HttpGet]
        public JsonResult GetPriceWithCampaign(int maBienThe, decimal giaGoc)
        {
            try
            {
                var priceResult = Models.CampaignHelper.TinhGiaSauGiamChoBienThe(ql, maBienThe, giaGoc);
                return Json(new
                {
                    coGiamGia = priceResult.CoGiamGia,
                    giaGoc = priceResult.GiaGoc,
                    giaSauGiam = priceResult.GiaSauGiam,
                    phanTramGiam = priceResult.PhanTramGiam,
                    tenCampaign = priceResult.TenCampaign
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { coGiamGia = false, giaGoc = giaGoc, giaSauGiam = giaGoc }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult KiemTraTonKho(int maBienThe, int soLuong)
        {
            try
            {
                // 1. Lấy số lượng tồn kho thực tế từ DB
                // (Truy vấn trực tiếp để lấy số chính xác nhất)
                int tonKhoThucTe = ql.Database.SqlQuery<int>(
                    "SELECT SoLuongTonKho FROM BienTheSanPham WHERE MaBienThe = @Ma",
                    new System.Data.SqlClient.SqlParameter("@Ma", maBienThe)
                ).FirstOrDefault();

                // 2. So sánh
                bool duHang = tonKhoThucTe >= soLuong;

                // 3. Trả về kết quả gồm: Có hàng không? Và Còn bao nhiêu?
                return Json(new
                {
                    coHang = duHang,
                    soLuongConLai = tonKhoThucTe
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { coHang = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}