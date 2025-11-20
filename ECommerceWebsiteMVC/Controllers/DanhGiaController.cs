using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class DanhGiaController : Controller
    {
        // GET: DanhGia
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]

        public ActionResult ThemDanhGia(DanhGiaSanPham model, HttpPostedFileBase HinhAnhFile)
        {
            // model.SoSao, model.TieuDe, model.NoiDung, model.MaSanPham... đã có
            // HinhAnhFile: lưu file, lấy đường dẫn gán vào model.HinhAnh
            // Lưu model vào DB...

            // Sau đó redirect lại trang đơn mua
            return RedirectToAction("DonMua");
        }

    }
}