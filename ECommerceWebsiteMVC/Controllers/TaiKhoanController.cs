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

        [HttpPost]
        public ActionResult CapNhatThongTin(NguoiMua model)
        {
            if (Session["MaNguoiMua"] == null) return RedirectToAction("DangNhap");

            int id = (int)Session["MaNguoiMua"];
            var user = db.NguoiMuas.Find(id);

            if (user != null)
            {
                try
                {
                    user.Email = model.Email;
                    user.SDT = model.SDT;
                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật hồ sơ thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi cập nhật: " + ex.Message;
                }
            }

            return RedirectToAction("ThongTinTaiKhoan");
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Index", "NguoiMua");
        }


        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }


        [HttpPost]
        public ActionResult DangKy(NguoiMua model, string XacNhanMatKhau)
        {
            // 1. Kiểm tra xác nhận mật khẩu
            if (model.MatKhau != XacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View(model);
            }

            // 2. Kiểm tra Số điện thoại đã tồn tại chưa
            var checkSdt = db.NguoiMuas.FirstOrDefault(s => s.SDT == model.SDT);
            if (checkSdt != null)
            {
                ViewBag.Error = "Số điện thoại này đã được đăng ký! Vui lòng đăng nhập.";
                return View(model);
            }

            // 3. Kiểm tra Tên tài khoản đã tồn tại chưa
            var checkUser = db.NguoiMuas.FirstOrDefault(s => s.TaiKhoan == model.TaiKhoan);
            if (checkUser != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại, vui lòng chọn tên khác.";
                return View(model);
            }

            try
            {

                db.NguoiMuas.Add(model);
                db.SaveChanges();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                // 5. Bắt lỗi từ SQL (Trigger)
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null) message = ex.InnerException.InnerException.Message;
                }

                if (message.Contains("Email khong hop le")) ViewBag.Error = "Email không đúng định dạng!";
                else if (message.Contains("So dien thoai")) ViewBag.Error = "Số điện thoại phải đủ 10 số!";
                else ViewBag.Error = "Lỗi hệ thống: " + message;

                return View(model);
            }
        }


        [HttpGet]
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuenMatKhau(string thongTinLienHe)
        {
            if (string.IsNullOrWhiteSpace(thongTinLienHe))
            {
                ViewBag.Error = "Vui lòng nhập Email hoặc Số điện thoại!";
                return View();
            }

            string input = thongTinLienHe.Trim();

            // Tìm trong DB giống đăng nhập
            var user = db.NguoiMuas
                         .FirstOrDefault(s => s.Email == input || s.SDT == input);

            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản trong hệ thống!";
                return View();
            }

            // Lưu ID vào session phục vụ bước reset
            Session["ResetPassword_UserId"] = user.MaNguoiMua;

            return RedirectToAction("DatLaiMatKhau");
        }

        [HttpGet]
        public ActionResult DatLaiMatKhau()
        {
            if (Session["ResetPassword_UserId"] == null)
                return RedirectToAction("QuenMatKhau");

            return View();
        }

        [HttpPost]
        public ActionResult DatLaiMatKhau(string MatKhauMoi, string XacNhanMatKhau)
        {
            if (Session["ResetPassword_UserId"] == null)
                return RedirectToAction("QuenMatKhau");

            // 1. Kiểm tra nhập đủ
            if (string.IsNullOrWhiteSpace(MatKhauMoi) || string.IsNullOrWhiteSpace(XacNhanMatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            // 2. Kiểm tra mật khẩu giống đăng ký
            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            int id = (int)Session["ResetPassword_UserId"];
            var user = db.NguoiMuas.FirstOrDefault(x => x.MaNguoiMua == id);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại!";
                return View();
            }

            try
            {
                // 3. Lưu mật khẩu mới vào DB
                user.MatKhau = MatKhauMoi;
                db.SaveChanges();

                // 4. Xóa session reset password
                Session.Remove("ResetPassword_UserId");

                TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                return View();
            }
        }

    }
}
