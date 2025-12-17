using ECommerceWebsiteMVC_Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC_Admin.Controllers
{
    public class QuanLyNguoiDungHeThongController : Controller
    {
        DBQuanLyNguoiDungHeThong db = new DBQuanLyNguoiDungHeThong();
        // GET: QuanLyKhachHang
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TongQuan()
        {
            ViewBag.TongSoNguoiBan = db.TongSoNguoiBan();
            ViewBag.TongSoNguoiMua = db.TongSoNguoiMua();
            ViewBag.SoCuaHangBiKhoa = db.SoCuaHangBiKhoa();
            ViewBag.TongSoNhanVien = db.TongSoNhanVien();
            return View();
        }
        //public ActionResult QuanLyCuaHang(int page = 1)
        //{
        //    int pageSize = 10;

        //    // Lấy toàn bộ danh sách từ DB như cũ
        //    List<CuaHang> lstCuaHang = db.DanhSachCuaHang();

        //    int totalItems = lstCuaHang.Count;
        //    int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        //    // Cắt danh sách theo trang
        //    var pagedData = lstCuaHang
        //                        .Skip((page - 1) * pageSize)
        //                        .Take(pageSize)
        //                        .ToList();

        //    // Gửi thông tin phân trang qua ViewBag
        //    ViewBag.Page = page;
        //    ViewBag.TotalPages = totalPages;
        //    ViewBag.TotalItems = totalItems;
        //    ViewBag.From = (page - 1) * pageSize + 1;
        //    ViewBag.To = Math.Min(page * pageSize, totalItems);

        //    return View(pagedData);
        //}
        public ActionResult QuanLyCuaHang(int page = 1, string search = "", string status = "")
        {
            int pageSize = 10;

            List<CuaHang> lst = db.DanhSachCuaHang();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                lst = lst.Where(x =>
                    x.TenCuaHang.ToLower().Contains(search.ToLower()) ||
                    x.NguoiBan.Email.ToLower().Contains(search.ToLower())
                ).ToList();
            }

            // Lọc theo trạng thái
            if (status == "active")
            {
                lst = lst.Where(x => x.TrangThai == true).ToList();
            }
            else if (status == "locked")
            {
                lst = lst.Where(x => x.TrangThai == false).ToList();
            }

            // Tổng số item sau khi lọc
            int totalItems = lst.Count;

            // Phân trang
            lst = lst.OrderBy(x => x.MaCuaHang)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToList();

            // Gửi dữ liệu sang View
            ViewBag.Page = page;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.From = (page - 1) * pageSize + 1;
            ViewBag.To = Math.Min(page * pageSize, totalItems);

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(lst);
        }

        public ActionResult ChiTietCuaHang(int pMaCH)
        {
            CuaHang ch = db.DanhSachCuaHang().Where(t=>t.MaCuaHang == pMaCH).First();
            List<DonHang> dsdh = db.DanhSachDonHangTheoCuaHang(ch.MaCuaHang);


            ViewBag.LichSuDonHang = dsdh;
            return View(ch);
        }
        public ActionResult ThayDoiTrangThaiCuaHang(int pMaCH, string pURL)
        {
            db.ThayDoiTrangThaiCuaHang(pMaCH);
            return Redirect(pURL);
        }
        public ActionResult QuanLyNguoiMua(string keyword = "", string filterType = "email", int page = 1)
        {
            int pageSize = 10;  // mỗi trang 10 người mua

            // Lấy danh sách người mua từ DB
            List<NguoiMua> dsNguoiMua = db.DanhSachNguoiMua();  // Bạn đang có hàm này tương tự DanhSachCuaHang

            // TÌM KIẾM
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();

                if (filterType == "email")
                {
                    dsNguoiMua = dsNguoiMua
                        .Where(n => n.Email != null && n.Email.ToLower().Contains(keyword))
                        .ToList();
                }
                else if (filterType == "phone")
                {
                    dsNguoiMua = dsNguoiMua
                        .Where(n => n.SDT != null && n.SDT.Contains(keyword))
                        .ToList();
                }
            }

            // TỔNG SỐ
            int totalItems = dsNguoiMua.Count;

            // TÍNH SỐ TRANG
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // GIỚI HẠN PAGE
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages == 0 ? 1 : totalPages;

            // PHÂN TRANG
            List<NguoiMua> nguoiMuaTrang =
                dsNguoiMua.Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToList();

            // Gửi dữ liệu qua ViewBag
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            ViewBag.From = totalItems == 0 ? 0 : ((page - 1) * pageSize + 1);
            ViewBag.To = Math.Min(page * pageSize, totalItems);

            // Gửi lại từ khóa & loại tìm kiếm để giữ lại giá trị trên View
            ViewBag.Keyword = keyword;
            ViewBag.FilterType = filterType;

            return View(nguoiMuaTrang);
        }

        //public ActionResult ChiTietNguoiMua(int pMaNM)
        //{
        //    NguoiMua nm = db.DanhSachNguoiMua().Where(t => t.MaNguoiMua == pMaNM).First();
        //    List<DonHang> dsdh = db.DanhSachDonHangTheoNguoiMua(nm.MaNguoiMua);


        //    ViewBag.LichSuDonHang = dsdh;
        //    return View(nm);
        //}
        public ActionResult ChiTietNguoiMua(int pMaNM, string keyword = "", string trangthai = "")
        {
            // Lấy thông tin người mua
            NguoiMua nm = db.DanhSachNguoiMua()
                            .FirstOrDefault(t => t.MaNguoiMua == pMaNM);

            if (nm == null)
                return HttpNotFound();

            // Lấy tất cả đơn hàng của người mua
            List<DonHang> dsdh = db.DanhSachDonHangTheoNguoiMua(nm.MaNguoiMua);

            // TÌM KIẾM THEO MÃ ĐƠN HÀNG
            if (!string.IsNullOrEmpty(keyword))
            {
                dsdh = dsdh.Where(d => d.MaDonHang.ToString().Contains(keyword)).ToList();
            }

            // LỌC THEO TRẠNG THÁI
            if (!string.IsNullOrEmpty(trangthai))
            {
                dsdh = dsdh.Where(d => d.TrangThaiDonHang.Equals(trangthai, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.LichSuDonHang = dsdh;
            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangthai;

            return View(nm);
        }

    }
}