using ECommerceWebsiteMVC_Seller.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC_Seller.Controllers
{
    public class TaiKhoanController : Controller
    {
        private ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        [HttpGet]
        public ActionResult DangNhapNguoiBan()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhapNguoiBan(string TaiKhoan, string MatKhau)
        {
            var seller = db.NguoiBans.SingleOrDefault(x => x.TaiKhoan == TaiKhoan && x.MatKhau == MatKhau);

            if (seller != null)
            {
                if (seller.TrangThai == false)
                {
                    ViewBag.Error = "Tài khoản người bán của bạn đang bị khóa. Vui lòng liên hệ Admin!";
                    return View();
                }

                Session["MaNguoiBan"] = seller.MaNguoiBan;
                Session["TenShop"] = seller.HoVaTen;
                return RedirectToAction("Index", "NguoiBan");
            }

            var buyer = db.NguoiMuas.SingleOrDefault(x => x.TaiKhoan == TaiKhoan && x.MatKhau == MatKhau);
            if (buyer != null)
            {
                if (buyer.TrangThai == false)
                {
                    ViewBag.Error = "Tài khoản của bạn đang bị khóa, không thể đăng ký bán hàng!";
                    return View();
                }

                return RedirectToAction("HoanTatDangKyNguoiBan", new { idNguoiMua = buyer.MaNguoiMua });
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
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
            var buyer = db.NguoiMuas.Find(idNguoiMua);
            if (buyer == null || buyer.TrangThai == false)
                return RedirectToAction("DangNhapNguoiBan");

            ViewBag.NguoiMua = buyer;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HoanTatDangKyNguoiBan(int MaNguoiMua, string CCCD)
        {
            
            var buyer = db.NguoiMuas.Find(MaNguoiMua);
            if (buyer == null) return RedirectToAction("DangNhapNguoiBan");

            if (string.IsNullOrEmpty(CCCD) || !System.Text.RegularExpressions.Regex.IsMatch(CCCD, "^[0-9]{12}$"))
            {
                ViewBag.Error = "Số CCCD không hợp lệ! Phải gồm đúng 12 chữ số.";
                ViewBag.NguoiMua = buyer;
                return View();
            }


            if (db.NguoiBans.Any(x => x.CCCD == CCCD))
            {
                ViewBag.Error = "Số CCCD này đã được đăng ký bởi người bán khác!";
                ViewBag.NguoiMua = buyer;
                return View();
            }

            try
            {

                NguoiBan newSeller = new NguoiBan
                {
                    HoVaTen = buyer.HoVaTen,
                    Email = buyer.Email,
                    SDT = buyer.SDT,
                    TaiKhoan = buyer.TaiKhoan,
                    MatKhau = buyer.MatKhau,
                    CCCD = CCCD,
                    TrangThai = true 
                };

                db.NguoiBans.Add(newSeller);
                db.SaveChanges();
                Session["MaNguoiBan"] = newSeller.MaNguoiBan;
                Session["TenShop"] = newSeller.HoVaTen;


                TempData["Success"] = "Chúc mừng! Bạn đã đăng ký trở thành Người bán thành công.";
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
        public ActionResult QuenMatKhau(string TaiKhoan, string Email, string SDT, string CCCD)
        {
            if (string.IsNullOrWhiteSpace(TaiKhoan) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(SDT) || string.IsNullOrWhiteSpace(CCCD))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin để xác thực!";
                return View();
            }

            var seller = db.NguoiBans.FirstOrDefault(x => x.TaiKhoan == TaiKhoan.Trim()
                                                         && x.Email == Email.Trim()
                                                         && x.SDT == SDT.Trim()
                                                         && x.CCCD == CCCD.Trim());

            if (seller == null || seller.TrangThai == false)
            {
                ViewBag.Error = "Thông tin xác thực không chính xác hoặc tài khoản đã bị khóa!";
                return View();
            }

            Session["ResetPassword_SellerId"] = seller.MaNguoiBan;

            return RedirectToAction("DatLaiMatKhau");
        }

        [HttpGet]
        public ActionResult DatLaiMatKhau()
        {
            if (Session["ResetPassword_SellerId"] == null)
            {
                return RedirectToAction("QuenMatKhau");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatLaiMatKhau(string MatKhauMoi, string XacNhanMatKhau)
        {
            if (Session["ResetPassword_SellerId"] == null) return RedirectToAction("QuenMatKhau");

            int sellerId = (int)Session["ResetPassword_SellerId"];
            var seller = db.NguoiBans.Find(sellerId);

            if (seller != null && seller.TrangThai== true)
            {
                seller.MatKhau = MatKhauMoi;
                db.SaveChanges();
                Session.Remove("ResetPassword_SellerId");
                TempData["Success"] = "Đặt lại mật khẩu thành công!";
            }
            else
            {
                ViewBag.Error = "Không thể thực hiện tác vụ này!";
            }

            return View();
        }

        [HttpGet]
        public ActionResult HoSoNguoiBan()
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan");

            int id = (int)Session["MaNguoiBan"];
            var seller = db.NguoiBans.Find(id); 

            if (seller == null) return HttpNotFound();

            return View(seller);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HoSoNguoiBan(NguoiBan model)
        {
            if (Session["MaNguoiBan"] == null)
            {
                return RedirectToAction("DangNhapNguoiBan");
            }
            int id = Convert.ToInt32(Session["MaNguoiBan"]);
            var seller = db.NguoiBans.Find(id);

            if (seller != null)
            {
                try
                {
                    seller.HoVaTen = model.HoVaTen;
                    seller.Email = model.Email;
                    seller.SDT = model.SDT;

                    db.SaveChanges();
                    Session["TenShop"] = seller.HoVaTen;

                    TempData["Success"] = "Cập nhật thông tin shop thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return RedirectToAction("HoSoNguoiBan");
        }
    }
}