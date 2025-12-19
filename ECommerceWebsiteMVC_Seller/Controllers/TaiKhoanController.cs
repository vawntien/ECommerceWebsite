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
            var seller = tk.NguoiBans.SingleOrDefault(x => x.TaiKhoan == TaiKhoan && x.MatKhau == MatKhau);

            if (seller != null)
            {
            
                Session["MaNguoiBan"] = seller.MaNguoiBan;
                Session["TenShop"] = seller.HoVaTen; 
                return RedirectToAction("Index", "NguoiBan"); 
            }

            var buyer = tk.NguoiMuas.SingleOrDefault(x => x.TaiKhoan == TaiKhoan && x.MatKhau == MatKhau);

            if (buyer != null)
            {
                return RedirectToAction("HoanTatDangKyNguoiBan", new { idNguoiMua = buyer.MaNguoiMua });
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }
        public ActionResult DangXuatNguoiBan()
        {
            Session.Clear();
            return RedirectToAction("DangNhapNguoiBan");
        }

        [HttpGet]
        public ActionResult HoanTatDangKyNguoiBan(int idNguoiMua)
        {

            var buyer = tk.NguoiMuas.Find(idNguoiMua);

            if (buyer == null) return RedirectToAction("DangNhapNguoiBan");

            ViewBag.NguoiMua = buyer;
            return View();
        }

        [HttpPost]
        public ActionResult HoanTatDangKyNguoiBan(int MaNguoiMua, string CCCD)
        {
            var buyer = tk.NguoiMuas.Find(MaNguoiMua);
            if (buyer == null) return RedirectToAction("DangNhapNguoiBan");

            if (string.IsNullOrEmpty(CCCD))
            {
                ViewBag.Error = "Vui lòng nhập số CCCD!";
                ViewBag.NguoiMua = buyer;
                return View();
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(CCCD, "^[0-9]{12}$"))
            {
                ViewBag.Error = "Số CCCD không hợp lệ! Phải gồm đúng 12 chữ số.";
                ViewBag.NguoiMua = buyer;
                return View();
            }

            bool checkCCCD = tk.NguoiBans.Any(x => x.CCCD == CCCD);
            if (checkCCCD)
            {
                ViewBag.Error = "Số CCCD này đã được đăng ký bởi người bán khác!";
                ViewBag.NguoiMua = buyer;
                return View();
            }

            try
            {
                NguoiBan newSeller = new NguoiBan();
                newSeller.HoVaTen = buyer.HoVaTen;
                newSeller.Email = buyer.Email;
                newSeller.SDT = buyer.SDT;
                newSeller.TaiKhoan = buyer.TaiKhoan;
                newSeller.MatKhau = buyer.MatKhau;

                newSeller.CCCD = CCCD;

                tk.NguoiBans.Add(newSeller);
                tk.SaveChanges();


                Session["MaNguoiBan"] = newSeller.MaNguoiBan;
                Session["TenShop"] = newSeller.HoVaTen;

                TempData["Success"] = "Đăng ký người bán thành công!";
                return RedirectToAction("Index", "NguoiBan");

            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                ViewBag.NguoiMua = buyer;
                return View();
            }
        }

        [HttpGet]
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuenMatKhau(string TaiKhoan, string Email, string SDT)
        {
            if (string.IsNullOrWhiteSpace(TaiKhoan) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(SDT))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin để xác thực!";
                return View();
            }

            // Kiểm tra thông tin người bán trong DB
            var seller = tk.NguoiBans.FirstOrDefault(x => x.TaiKhoan == TaiKhoan.Trim()
                                                       && x.Email == Email.Trim()
                                                       && x.SDT == SDT.Trim());

            if (seller == null)
            {
                ViewBag.Error = "Thông tin xác thực không chính xác hoặc tài khoản không tồn tại!";
                return View();
            }

            
            Session["ResetPassword_SellerId"] = seller.MaNguoiBan;

            return RedirectToAction("DatLaiMatKhau");
        }

        [HttpGet]
        public ActionResult DatLaiMatKhau()
        {
            if (Session["ResetPassword_SellerId"] == null)
                return RedirectToAction("QuenMatKhau");

            return View();
        }

        [HttpPost]
        public ActionResult DatLaiMatKhau(string MatKhauMoi, string XacNhanMatKhau)
        {
            if (Session["ResetPassword_SellerId"] == null)
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

            try
            {
                int sellerId = (int)Session["ResetPassword_SellerId"];
                var seller = tk.NguoiBans.Find(sellerId);

                if (seller != null)
                {
                    seller.MatKhau = MatKhauMoi;
                    tk.SaveChanges();

                    
                    Session.Remove("ResetPassword_SellerId");

                    TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.";
                    return RedirectToAction("DangNhapNguoiBan");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
            }

            return View();
        }


    }
}