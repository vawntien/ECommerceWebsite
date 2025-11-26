using ECommerceWebsiteMVC_Seller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC_Seller.Controllers
{
    public class TaiKhoanController : Controller
    {
        ECommerceWebsiteEntities tk = new ECommerceWebsiteEntities();

        // GET: TaiKhoan/DangNhapNguoiBan
        public ActionResult DangNhapNguoiBan()
        {
            return View();
        }

        // POST: TaiKhoan/DangNhapNguoiBan
        [HttpPost]
        public ActionResult DangNhapNguoiBan(string TaiKhoan, string MatKhau)
        {
            var user = tk.NguoiBans.SingleOrDefault(x => x.TaiKhoan == TaiKhoan
                                            && x.MatKhau == MatKhau);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            // Nếu vai trò = true → tài khoản đã đăng ký bán hàng
            //if (user.VaiTro)
            //{
            //    // Lưu session
            //    Session["MaNguoiDung"] = user.MaNguoiDung;
            //    Session["HoVaTen"] = user.HoVaTen;
            //    Session["TaiKhoan"] = user.TaiKhoan;
            //    Session["VaiTro"] = user.VaiTro;
            //    Session["AnhDaiDien"] = user.HinhAnh;

            //    return RedirectToAction("Index", "NguoiBan");
            //}

            // Nếu vai trò = false → chuyển sang trang đăng ký người bán
            //return RedirectToAction("DangKyNguoiBan", "TaiKhoan");
            Session["MaNguoiBan"] = user.MaNguoiBan;
            Session["HoVaTen"] = user.HoVaTen;
            Session["TaiKhoan"] = user.TaiKhoan;
            return RedirectToAction("Index", "NguoiBan");
        }

        public ActionResult DangXuatNguoiBan()
        {
            Session.Clear();
            return RedirectToAction("DangNhapNguoiBan");
        }

        public ActionResult DangKyNguoiBan()
        {
            return View();
        }
    }
}