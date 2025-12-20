using ECommerceWebsiteMVC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class TaiKhoanController : Controller
    {
        private ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();


        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(string TaiKhoan, string MatKhau)
        {
            if (string.IsNullOrWhiteSpace(TaiKhoan) || string.IsNullOrWhiteSpace(MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                return View();
            }

            var user = db.NguoiMuas
                         .SingleOrDefault(x => x.TaiKhoan == TaiKhoan && x.MatKhau == MatKhau);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            if (user.TrangThai == false)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!";
                return View();
            }

            Session["MaNguoiMua"] = user.MaNguoiMua;
            Session["HoVaTen"] = user.HoVaTen;
            Session["Email"] = user.Email;
            Session["TaiKhoan"] = user.TaiKhoan;
            Session["SDT"] = user.SDT;

            return RedirectToAction("Index", "NguoiMua");
        }

        [HttpPost]
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
            if (model == null)
            {
                ViewBag.Error = "Dữ liệu không hợp lệ!";
                return View();
            }

            if (model.MatKhau != XacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View(model);
            }

            if (db.NguoiMuas.Any(x => x.SDT == model.SDT))
            {
                ViewBag.Error = "Số điện thoại đã được đăng ký!";
                return View(model);
            }

            if (db.NguoiMuas.Any(x => x.TaiKhoan == model.TaiKhoan))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View(model);
            }

            try
            {
                model.TrangThai = true;
                db.NguoiMuas.Add(model);
                db.SaveChanges();

                TempData["RegisterSuccess"] = true;
                return View();
            }
            catch
            {
                ViewBag.Error = "Lỗi hệ thống! Vui lòng thử lại.";
                return View(model);
            }
        }


        [HttpGet]
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuenMatKhau(string TenDangNhap, string Email, string SoDienThoai)
        {
            if (string.IsNullOrWhiteSpace(TenDangNhap) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(SoDienThoai))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            var user = db.NguoiMuas.FirstOrDefault(x =>
                x.TaiKhoan == TenDangNhap.Trim() &&
                x.Email == Email.Trim() &&
                x.SDT == SoDienThoai.Trim());

            if (user == null)
            {
                ViewBag.Error = "Thông tin xác thực không chính xác!";
                return View();
            }

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

            if (string.IsNullOrWhiteSpace(MatKhauMoi) || MatKhauMoi.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return View();
            }

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            int userId = (int)Session["ResetPassword_UserId"];
            var user = db.NguoiMuas.Find(userId);

            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản!";
                return View();
            }

            user.MatKhau = MatKhauMoi;
            db.SaveChanges();

            Session.Remove("ResetPassword_UserId");

            TempData["Success"] = "Đặt lại mật khẩu thành công!";
            return View();
        }

        [HttpGet]
        public ActionResult ThongTinTaiKhoan()
        {
            if (Session["MaNguoiMua"] == null)
                return RedirectToAction("DangNhap");

            int id = (int)Session["MaNguoiMua"];
            var user = db.NguoiMuas.Find(id);

            if (user == null) return HttpNotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThongTinTaiKhoan(NguoiMua model)
        {
            if (Session["MaNguoiMua"] == null) return RedirectToAction("DangNhap");

            int id = (int)Session["MaNguoiMua"];
            var user = db.NguoiMuas.Find(id);

            if (user != null)
            {
                user.HoVaTen = model.HoVaTen;
                user.Email = model.Email;
                user.SDT = model.SDT;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            return RedirectToAction("ThongTinTaiKhoan");
        }
    }
}
