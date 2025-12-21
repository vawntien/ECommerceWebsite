using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class ThanhToanController : Controller
    {

        // GET: ThanhToan
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ProcessMomoPayment(CheckoutViewModel model)
        {
            // 1. Kiểm tra dữ liệu
            if (model.TongThanhToan <= 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // 2. TẠO ĐƠN HÀNG TRƯỚC
            DonHang order = new DonHang
            {
                TongTien = model.TongThanhToan,
                TrangThaiThanhToan = false,
                ThoiGianDat = DateTime.Now
            };

            db.DonHangs.Add(order);
            db.SaveChanges(); // 🔴 CẦN để lấy MaDonHang

            // 3. CHUYỂN SANG ACTION TẠO REQUEST MOMO
            return RedirectToAction("CreateMomoPayment", new { orderId = order.MaDonHang });
        }

    }
}