using ECommerceWebsiteMVC_Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC_Admin.Controllers
{
    public class QuanLyNhanVienController : Controller
    {
        DBQuanLyNhanVien db = new DBQuanLyNhanVien();

        public ActionResult Index()
        {
            List<QuyenHeThong> dsq = db.DanhSachQuyenHeThong();
            return View(dsq);
        }

        // ======================
        // TRANG DANH SÁCH
        // ======================
        public ActionResult NhanVien(string roleFilter = "all")
        {
            List<NhanVien> dsnv = db.DanhSachNhanVien();

            switch (roleFilter)
            {
                case "user":
                    dsnv = dsnv.Where(x => x.QuyenHeThong.TenQuyenHeThong == "Quản trị người dùng (User Management)").ToList();
                    break;

                case "analyst":
                    dsnv = dsnv.Where(x => x.QuyenHeThong.TenQuyenHeThong == "Thống kê - Báo cáo (Analyst)").ToList();
                    break;

                case "role":
                    dsnv = dsnv.Where(x => x.QuyenHeThong.TenQuyenHeThong == "Phân quyền nhân viên (Role Management)").ToList();
                    break;
            }

            ViewBag.RoleFilter = roleFilter;
            ViewBag.ListQuyen = db.DanhSachQuyenHeThong();
            return View(dsnv);
        }

        // ======================
        // THÊM NHÂN VIÊN
        // ======================
        [HttpGet]
        public ActionResult ThemNhanVien()
        {
            ViewBag.ListQuyen = db.DanhSachQuyenHeThong();
            return View();
        }

        [HttpPost]
        public ActionResult ThemNhanVien(NhanVien nv)
        {
            ViewBag.ListQuyen = db.DanhSachQuyenHeThong();

            // --- Validate rỗng ---
            if (nv == null
                || string.IsNullOrEmpty(nv.HoVaTen)
                || string.IsNullOrEmpty(nv.Email)
                || string.IsNullOrEmpty(nv.TaiKhoan)
                || string.IsNullOrEmpty(nv.MatKhau)
                || string.IsNullOrEmpty(nv.SDT)
                || string.IsNullOrEmpty(nv.CCCD))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View(nv);
            }

            // --- Validate SDT 10 số ---
            if (nv.SDT.Length != 10 || !nv.SDT.All(char.IsDigit))
            {
                ViewBag.Error = "Số điện thoại phải gồm đúng 10 chữ số!";
                return View(nv);
            }

            // --- Validate CCCD 12 số ---
            if (nv.CCCD.Length != 12 || !nv.CCCD.All(char.IsDigit))
            {
                ViewBag.Error = "CCCD phải gồm đúng 12 chữ số!";
                return View(nv);
            }

            bool rs = db.ThemNhanVien(nv);

            if (!rs)
            {
                ViewBag.Error = "Lỗi khi lưu nhân viên! (Có thể trùng Email/SĐT/CCCD)";
                return View(nv);
            }

            return RedirectToAction("NhanVien");
        }

        // ======================
        // SỬA NHÂN VIÊN
        // ======================
        [HttpGet]
        public ActionResult SuaNhanVien(int id)
        {
            NhanVien nv = db.DanhSachNhanVien().FirstOrDefault(x => x.MaNhanVien == id);
            if (nv == null)
                return HttpNotFound();

            ViewBag.ListQuyen = db.DanhSachQuyenHeThong();
            return View(nv);
        }

        [HttpPost]
        public ActionResult SuaNhanVien(NhanVien nv)
        {
            ViewBag.ListQuyen = db.DanhSachQuyenHeThong();

            // Validate rỗng
            if (string.IsNullOrEmpty(nv.HoVaTen)
                || string.IsNullOrEmpty(nv.Email)
                || string.IsNullOrEmpty(nv.TaiKhoan)
                || string.IsNullOrEmpty(nv.SDT)
                || string.IsNullOrEmpty(nv.CCCD))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin!");
                return View(nv);
            }

            // Validate SDT
            if (nv.SDT.Length != 10 || !nv.SDT.All(char.IsDigit))
            {
                ModelState.AddModelError("", "Số điện thoại phải gồm đúng 10 chữ số!");
                return View(nv);
            }

            // Validate CCCD
            if (nv.CCCD.Length != 12 || !nv.CCCD.All(char.IsDigit))
            {
                ModelState.AddModelError("", "CCCD phải gồm đúng 12 chữ số!");
                return View(nv);
            }

            bool result = db.SuaNhanVien(nv);

            if (!result)
            {
                ModelState.AddModelError("", "Không thể cập nhật (Có thể trùng Email/SĐT/CCCD).");
                return View(nv);
            }

            TempData["Success"] = "Cập nhật nhân viên thành công!";
            return RedirectToAction("NhanVien");
        }

        // ======================
        // XÓA NHÂN VIÊN
        // ======================
        public ActionResult XoaNhanVien(int id)
        {
            bool result = db.XoaNhanVien(id);

            TempData["Success"] = result
                ? "Xóa nhân viên thành công!"
                : "Không thể xóa nhân viên!";

            return RedirectToAction("NhanVien");
        }
    }
}
