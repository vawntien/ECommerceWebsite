using ECommerceWebsiteMVC_Seller.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC_Seller.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        [HttpGet]
        public ActionResult DangNhapNguoiBan()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhapNguoiBan(string TaiKhoan, string MatKhau)
        {
            var seller = db.NguoiBans
                .SingleOrDefault(x => x.TaiKhoan == TaiKhoan && x.MatKhau == MatKhau);

            if (seller != null)
            {
                if (!seller.TrangThai)
                {
                    ViewBag.Error = "Tài khoản người bán đang bị khóa!";
                    return View();
                }

                Session["MaNguoiBan"] = seller.MaNguoiBan;
                return RedirectToAction("KiemTraCuaHang");
            }

            var buyer = db.NguoiMuas
                .SingleOrDefault(x => x.TaiKhoan == TaiKhoan && x.MatKhau == MatKhau);

            if (buyer != null)
            {
                if (!buyer.TrangThai)
                {
                    ViewBag.Error = "Tài khoản đang bị khóa, không thể đăng ký bán!";
                    return View();
                }

                return RedirectToAction("HoanTatDangKyNguoiBan", new { idNguoiMua = buyer.MaNguoiMua });
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }

        public ActionResult KiemTraCuaHang()
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan");

            int sellerId = (int)Session["MaNguoiBan"];

            using (var shopDb = new DB_CuaHang())
            {
                if (shopDb.HasShop(sellerId))
                {
                    Session["TenShop"] = shopDb.GetShopName(sellerId);
                    return RedirectToAction("Index", "NguoiBan");
                }
                else
                {
                    return RedirectToAction("DangKyCuaHang");
                }
            }
        }

        [HttpGet]
        public ActionResult DangKyCuaHang()
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKyCuaHang(string TenCuaHang, string DiaChi, string MaSoThue)
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan");

            if (string.IsNullOrWhiteSpace(TenCuaHang))
            {
                ViewBag.Error = "Tên cửa hàng không được để trống!";
                return View();
            }

            int sellerId = (int)Session["MaNguoiBan"];
            string message;

            using (var shopDb = new DB_CuaHang())
            {
                bool ok = shopDb.RegisterNewShop(
                    sellerId,
                    TenCuaHang,
                    DiaChi,
                    MaSoThue,
                    out message
                );

                if (!ok)
                {
                    ViewBag.Error = message;
                    return View();
                }

                Session["TenShop"] = TenCuaHang;
                TempData["Success"] = message;
                return RedirectToAction("Index", "NguoiBan");
            }
        }

        [HttpGet]
        public ActionResult HoanTatDangKyNguoiBan(int idNguoiMua)
        {
            var buyer = db.NguoiMuas.Find(idNguoiMua);
            if (buyer == null || !buyer.TrangThai)
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

            if (string.IsNullOrEmpty(CCCD) || !Regex.IsMatch(CCCD, "^[0-9]{12}$"))
            {
                ViewBag.Error = "CCCD phải gồm đúng 12 chữ số!";
                ViewBag.NguoiMua = buyer;
                return View();
            }

            if (db.NguoiBans.Any(x => x.CCCD == CCCD))
            {
                ViewBag.Error = "CCCD đã tồn tại!";
                ViewBag.NguoiMua = buyer;
                return View();
            }

            try
            {
                var newSeller = new NguoiBan
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
                return RedirectToAction("KiemTraCuaHang");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                ViewBag.NguoiMua = buyer;
                return View();
            }
        }

        public ActionResult DangXuatNguoiBan()
        {
            Session.Clear();
            return RedirectToAction("DangNhapNguoiBan");
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
                return RedirectToAction("DangNhapNguoiBan");

            int id = (int)Session["MaNguoiBan"];
            var seller = db.NguoiBans.Find(id);

            if (seller != null)
            {
                seller.HoVaTen = model.HoVaTen;
                seller.Email = model.Email;
                seller.SDT = model.SDT;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }

            return RedirectToAction("HoSoNguoiBan");
        }
    }
}
