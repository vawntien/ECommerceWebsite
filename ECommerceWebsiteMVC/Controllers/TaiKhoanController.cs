using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class TaiKhoanController : Controller
    {
        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        // GET: TaiKhoan
        public ActionResult DangNhap()
        {
            return View();
        }

        //POST: TaiKhoan/DangNhap
        [HttpPost]
        public ActionResult DangNhap(string TaiKhoan, string MatKhau)
        {
            var user = db.NguoiMuas
                         .SingleOrDefault(x => x.TaiKhoan == TaiKhoan
                                            && x.MatKhau == MatKhau);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            //Lưu session người dùng
            Session["MaNguoiMua"] = user.MaNguoiMua;
            Session["HoVaTen"] = user.HoVaTen;
            Session["Email"] = user.Email;
            Session["TaiKhoan"] = user.TaiKhoan;
            Session["SDT"] = user.SDT;

            return RedirectToAction("Index", "NguoiMua");
        }

        public ActionResult ThongTinTaiKhoan()
        {
            if (Session["MaNguoiMua"] == null)
                return RedirectToAction("DangNhap");

            int id = (int)Session["MaNguoiMua"];
            var user = db.NguoiMuas.Find(id);

            return View(user);
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("DangNhap");
        }

        public ActionResult DangKy()
        {
            return View();
        }
    }
}
