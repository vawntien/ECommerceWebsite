using ECommerceWebsiteMVC_Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC_Admin.Controllers
{
    public class QuanLyThongKeController : Controller
    {
        // GET: QuanLyThongKe
        DBQuanLyThongKe db = new DBQuanLyThongKe();
        public ActionResult Index()
        {
            ViewBag.DoanhThu6Thang = db.GetDoanhThu6Thang();
            ViewBag.DonHang6Thang = db.GetSoDon6Thang();
            ViewBag.TongDoanhThu = db.TongDoanhThu();
            ViewBag.DonHangThanhCong = db.DonHangThanhCong();
            ViewBag.DonHangThatBai = db.DonHangThatBai();
            return View();
        }
        public ActionResult ThongKeDoanhThuNguoiBan(int page = 1)
        {
            int pageSize = 15;

            List<ThongKeDoanhThuNguoiBan> lsttk = new List<ThongKeDoanhThuNguoiBan>();

            foreach (CuaHang it in db.DanhSachCuaHang())
            {
                ThongKeDoanhThuNguoiBan tkch = new ThongKeDoanhThuNguoiBan();
                tkch.TenCuaHang = it.TenCuaHang;

                var dsDon = db.DanhSachDonHangTheoCuaHang(it.MaCuaHang);

                tkch.DoanhThu = (int)dsDon
                    .Where(t => t.TrangThaiDonHang == "Đã giao")
                    .Sum(t => (int?)t.TongTien ?? 0);

                tkch.TongSoDon = dsDon.Count();

                lsttk.Add(tkch);
            }

            // SẮP XẾP GIẢM DẦN THEO DOANH THU
            lsttk = lsttk.OrderByDescending(t => t.DoanhThu).ToList();

            // TÍNH TOÁN PHÂN TRANG
            int totalItems = lsttk.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages == 0 ? 1 : totalPages;

            var pagedData = lsttk
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Gửi dữ liệu sang ViewBag
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.From = (page - 1) * pageSize + 1;
            ViewBag.To = Math.Min(page * pageSize, totalItems);

            return View(pagedData);
        }
        public ActionResult ThongKeDoanhThuNganhHang(int page = 1)
        {
            int pageSize = 10; // mỗi trang 10 ngành hàng

            List<ThongKeDoanhThuNganhHang> lsttk = new List<ThongKeDoanhThuNganhHang>();

            foreach (DanhMuc it in db.DanhSachDanhMuc())
            {
                ThongKeDoanhThuNganhHang tk = new ThongKeDoanhThuNganhHang();

                List<DonHang> dsDon = db.DanhSachDonHangTheoNganhHang(it.MaDanhMuc);

                tk.TenNganhHang = it.TenDanhMuc;

                tk.DoanhThu = (int)dsDon
                    .Where(t => t.TrangThaiDonHang == "Đã giao")
                    .Sum(t => (int?)t.TongTien ?? 0);

                tk.SoLuongBan = dsDon.Count();

                lsttk.Add(tk);
            }

            // Sắp xếp giảm dần theo doanh thu
            lsttk = lsttk.OrderByDescending(t => t.DoanhThu).ToList();

            // Tổng số item
            int totalItems = lsttk.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Giới hạn page
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages == 0 ? 1 : totalPages;

            // Cắt danh sách theo trang
            var pagedData = lsttk
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Gửi ViewBag phân trang
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.From = totalItems == 0 ? 0 : (page - 1) * pageSize + 1;
            ViewBag.To = Math.Min(page * pageSize, totalItems);

            return View(pagedData);
        }



    }
}