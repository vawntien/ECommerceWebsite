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
        DBQuanLyKhuyenMai dbKhuyenMai = new DBQuanLyKhuyenMai();
        DBQuanLyCampaign dbCampaign = new DBQuanLyCampaign();
        ECommerceWebsiteEntities dtbs = new ECommerceWebsiteEntities();
        // GET: QuanLyKhachHang
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ChiTietDanhGiaCuaHang(int pMaCH)
        {
            CuaHang ch = db.DanhSachCuaHang().Where(t => t.MaCuaHang == pMaCH).First();
            List<DonHang> dsdh = db.DanhSachDonHangTheoCuaHang(ch.MaCuaHang);


            ViewBag.DanhSachDanhGia = dtbs.DanhGiaSanPhams.Where(t => t.ChiTietDonHang.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == pMaCH).ToList();
            return View(ch);
        }
        public ActionResult ChiTietDanhGia(int pMaDG)
        {
            var danhGia = dtbs.DanhGiaSanPhams
                .FirstOrDefault(t => t.MaDG == pMaDG);
            
            if (danhGia == null)
                return HttpNotFound();
            
            // Nếu là AJAX request, trả về partial view
            if (Request.IsAjaxRequest())
            {
                return PartialView("ChiTietDanhGia", danhGia);
            }
            
            // Nếu không, trả về view thông thường
            return View(danhGia);
        }
        [HttpPost]
        public JsonResult XoaDanhGia(int pMaDG)
        {
            var danhGia = dtbs.DanhGiaSanPhams.FirstOrDefault(x => x.MaDG == pMaDG);
            if (danhGia == null)
                return Json(new { success = false });

            dtbs.DanhGiaSanPhams.Remove(danhGia);
            dtbs.SaveChanges();

            return Json(new { success = true });
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
        // QUẢN LÝ KHUYẾN MÃI
        // Danh sách khuyến mãi (bao gồm cả Voucher và Campaign)
        public ActionResult QuanLyKhuyenMai(int page = 1, string search = "", string status = "", string tab = "voucher")
        {
            Response.Charset = "utf-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            int pageSize = 10;
            DateTime now = DateTime.Now; // Khai báo một lần ở đầu method

            List<GiamGia> lst = dbKhuyenMai.DanhSachKhuyenMai();

            // Lọc chỉ lấy Voucher (không phải Campaign) - Campaign có prefix [CAMPAIGN]
            lst = lst.Where(x => x.TenMaGG == null || !x.TenMaGG.StartsWith("[CAMPAIGN]")).ToList();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                lst = lst.Where(x =>
                    x.TenMaGG.ToLower().Contains(search.ToLower()) ||
                    (x.MoTa != null && x.MoTa.ToLower().Contains(search.ToLower()))
                ).ToList();
            }

            // Lọc theo trạng thái (sử dụng biến now đã khai báo ở đầu method)
            if (status == "active")
            {
                // Đang hiệu lực: NgayBD <= now và NgayKT >= now (hoặc null)
                lst = lst.Where(x =>
                    (x.NgayBD == null || x.NgayBD <= now) &&
                    (x.NgayKT == null || x.NgayKT >= now)
                ).ToList();
            }
            else if (status == "expired")
            {
                // Hết hạn: NgayKT < now
                lst = lst.Where(x => x.NgayKT != null && x.NgayKT < now).ToList();
            }
            else if (status == "upcoming")
            {
                // Sắp diễn ra: NgayBD > now
                lst = lst.Where(x => x.NgayBD != null && x.NgayBD > now).ToList();
            }
            else if (status == "hidden")
            {
                // Đã ẩn: NgayKT < now (đã hết hạn)
                lst = lst.Where(x => x.NgayKT != null && x.NgayKT < now).ToList();
            }

            // Tổng số item sau khi lọc
            int totalItems = lst.Count;

            // Phân trang
            lst = lst.OrderByDescending(x => x.MaGiamGia)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToList();

            // Gửi dữ liệu sang View
            ViewBag.Page = page;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.From = totalItems == 0 ? 0 : ((page - 1) * pageSize + 1);
            ViewBag.To = Math.Min(page * pageSize, totalItems);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Tab = tab; // Tab hiện tại: "voucher" hoặc "campaign"

            // Nếu tab là campaign, lấy danh sách campaign
            if (tab == "campaign")
            {
                List<CampaignInfo> lstCampaign = dbCampaign.DanhSachCampaign();

                // Lọc theo tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    lstCampaign = lstCampaign.Where(x =>
                        x.TenCampaign.ToLower().Contains(search.ToLower()) ||
                        (x.MoTa != null && x.MoTa.ToLower().Contains(search.ToLower()))
                    ).ToList();
                }

                // Lọc theo trạng thái (sử dụng biến now đã khai báo ở đầu method)
                if (status == "active")
                {
                    lstCampaign = lstCampaign.Where(x =>
                        x.TrangThai == true &&
                        (x.NgayBD == null || x.NgayBD <= now) &&
                        (x.NgayKT == null || x.NgayKT >= now)
                    ).ToList();
                }
                else if (status == "expired")
                {
                    lstCampaign = lstCampaign.Where(x => x.NgayKT != null && x.NgayKT < now).ToList();
                }
                else if (status == "upcoming")
                {
                    lstCampaign = lstCampaign.Where(x => x.NgayBD != null && x.NgayBD > now).ToList();
                }
                else if (status == "inactive")
                {
                    lstCampaign = lstCampaign.Where(x => x.TrangThai == false).ToList();
                }

                // Tổng số item sau khi lọc
                int totalItemsCampaign = lstCampaign.Count;

                // Phân trang
                lstCampaign = lstCampaign.Skip((page - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToList();

                ViewBag.Page = page;
                ViewBag.TotalItems = totalItemsCampaign;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalItemsCampaign / pageSize);
                ViewBag.From = totalItemsCampaign == 0 ? 0 : ((page - 1) * pageSize + 1);
                ViewBag.To = Math.Min(page * pageSize, totalItemsCampaign);
                ViewBag.Campaigns = lstCampaign;
            }
            else
            {
                ViewBag.Vouchers = lst;
            }

            return View(lst);
        }

        // GET: Thêm khuyến mãi
        [HttpGet]
        public ActionResult ThemKhuyenMai()
        {
            return View();
        }

        // POST: Thêm khuyến mãi
        [HttpPost]
        public ActionResult ThemKhuyenMai(GiamGia gg, string NgayBD, string NgayKT, string GiaTriGiam)
        {
            // Xử lý giá trị giảm từ form (parse với InvariantCulture để chấp nhận dấu chấm)
            if (!string.IsNullOrEmpty(GiaTriGiam))
            {
                double parsedValue;
                if (double.TryParse(GiaTriGiam, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedValue))
                {
                    gg.GiaTriGiam = parsedValue;
                    // Xóa lỗi ModelState nếu có
                    ModelState.Remove("GiaTriGiam");
                }
            }

            // Xử lý datetime từ form
            if (!string.IsNullOrEmpty(NgayBD))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayBD, out parsedDate))
                {
                    gg.NgayBD = parsedDate;
                }
            }

            if (!string.IsNullOrEmpty(NgayKT))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayKT, out parsedDate))
                {
                    gg.NgayKT = parsedDate;
                }
            }

            // Xóa lỗi ModelState cho GiaTriGiam nếu có
            if (ModelState.ContainsKey("GiaTriGiam"))
            {
                ModelState["GiaTriGiam"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                // Validate logic
                if (gg.GiaTriGiam == null || gg.GiaTriGiam <= 0)
                {
                    ViewBag.Error = "Giá trị giảm phải lớn hơn 0!";
                    return View(gg);
                }

                if (gg.NgayBD.HasValue && gg.NgayKT.HasValue && gg.NgayBD > gg.NgayKT)
                {
                    ViewBag.Error = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc!";
                    return View(gg);
                }

                if (dbKhuyenMai.ThemKhuyenMai(gg))
                {
                    TempData["Success"] = "Thêm khuyến mãi thành công!";
                    return RedirectToAction("QuanLyKhuyenMai");
                }
                else
                {
                    ViewBag.Error = "Có lỗi xảy ra khi thêm khuyến mãi!";
                }
            }

            return View(gg);
        }

        // GET: Sửa khuyến mãi
        [HttpGet]
        public ActionResult SuaKhuyenMai(int id)
        {
            var gg = dbKhuyenMai.LayKhuyenMaiTheoMa(id);
            if (gg == null)
            {
                return HttpNotFound();
            }
            return View(gg);
        }

        // POST: Sửa khuyến mãi
        [HttpPost]
        public ActionResult SuaKhuyenMai(GiamGia gg, string NgayBD, string NgayKT, string GiaTriGiam)
        {
            // Xử lý giá trị giảm từ form (parse với InvariantCulture để chấp nhận dấu chấm)
            if (!string.IsNullOrEmpty(GiaTriGiam))
            {
                double parsedValue;
                if (double.TryParse(GiaTriGiam, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedValue))
                {
                    gg.GiaTriGiam = parsedValue;
                    // Xóa lỗi ModelState nếu có
                    ModelState.Remove("GiaTriGiam");
                }
            }

            // Xử lý datetime từ form
            if (!string.IsNullOrEmpty(NgayBD))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayBD, out parsedDate))
                {
                    gg.NgayBD = parsedDate;
                }
            }
            else
            {
                gg.NgayBD = null;
            }

            if (!string.IsNullOrEmpty(NgayKT))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayKT, out parsedDate))
                {
                    gg.NgayKT = parsedDate;
                }
            }
            else
            {
                gg.NgayKT = null;
            }

            // Xóa lỗi ModelState cho GiaTriGiam nếu có
            if (ModelState.ContainsKey("GiaTriGiam"))
            {
                ModelState["GiaTriGiam"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                // Validate logic
                if (gg.GiaTriGiam == null || gg.GiaTriGiam <= 0)
                {
                    ViewBag.Error = "Giá trị giảm phải lớn hơn 0!";
                    return View(gg);
                }

                if (gg.NgayBD.HasValue && gg.NgayKT.HasValue && gg.NgayBD > gg.NgayKT)
                {
                    ViewBag.Error = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc!";
                    return View(gg);
                }

                if (dbKhuyenMai.SuaKhuyenMai(gg))
                {
                    TempData["Success"] = "Sửa khuyến mãi thành công!";
                    return RedirectToAction("QuanLyKhuyenMai");
                }
                else
                {
                    ViewBag.Error = "Có lỗi xảy ra khi sửa khuyến mãi!";
                }
            }

            return View(gg);
        }

        // Xóa khuyến mãi
        [HttpPost]
        public ActionResult XoaKhuyenMai(int id)
        {
            if (dbKhuyenMai.XoaKhuyenMai(id))
            {
                TempData["Success"] = "Xóa khuyến mãi thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa khuyến mãi này vì đã có đơn hàng sử dụng!";
            }
            return RedirectToAction("QuanLyKhuyenMai");
        }

        // Ẩn khuyến mãi
        [HttpPost]
        public ActionResult AnKhuyenMai(int id)
        {
            if (dbKhuyenMai.AnKhuyenMai(id))
            {
                TempData["Success"] = "Ẩn khuyến mãi thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi ẩn khuyến mãi!";
            }
            return RedirectToAction("QuanLyKhuyenMai");
        }

        // Hiện khuyến mãi
        [HttpPost]
        public ActionResult HienKhuyenMai(int id, DateTime? ngayKT)
        {
            if (dbKhuyenMai.HienKhuyenMai(id, ngayKT))
            {
                TempData["Success"] = "Hiện khuyến mãi thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi hiện khuyến mãi!";
            }
            return RedirectToAction("QuanLyKhuyenMai");
        }

        // Chi tiết khuyến mãi
        public ActionResult ChiTietKhuyenMai(int id)
        {
            var gg = dbKhuyenMai.LayKhuyenMaiTheoMa(id);
            if (gg == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách đơn hàng đã sử dụng voucher này
            var donHangs = gg.DonHangs?.ToList() ?? new List<DonHang>();
            ViewBag.DonHangs = donHangs;
            ViewBag.HieuLuc = dbKhuyenMai.KiemTraHieuLuc(id);

            return View(gg);
        }

        // Thống kê khuyến mãi
        public ActionResult ThongKeKhuyenMai(string tuNgay = "", string denNgay = "", int? maVoucher = null)
        {
            Response.Charset = "utf-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;

            DateTime? tuNgayFilter = null;
            DateTime? denNgayFilter = null;

            if (!string.IsNullOrEmpty(tuNgay))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(tuNgay, out parsedDate))
                {
                    tuNgayFilter = parsedDate;
                }
            }

            if (!string.IsNullOrEmpty(denNgay))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(denNgay, out parsedDate))
                {
                    denNgayFilter = parsedDate.AddDays(1);
                }
            }

            var thongKe = dbKhuyenMai.LayThongKeKhuyenMai(tuNgayFilter, denNgayFilter, maVoucher);

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.MaVoucher = maVoucher;
            
            // Lấy thông tin voucher được chọn để hiển thị loại giảm giá
            ViewBag.VoucherSelected = null; // Reset về null
            if (maVoucher.HasValue && maVoucher.Value > 0)
            {
                var voucher = dbKhuyenMai.LayKhuyenMaiTheoMa(maVoucher.Value);
                if (voucher != null)
                {
                    ViewBag.VoucherSelected = voucher;
                }
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            ViewBag.DuLieuVoucherJson = serializer.Serialize(thongKe.XepHangVoucher?.Take(10).ToList() ?? new List<VoucherHieuQua>());

            return View(thongKe);
        }

        // ======================
        // QUẢN LÝ CAMPAIGN (Chương trình khuyến mãi chủ động)
        // ======================

        // Danh sách Campaign
        public ActionResult QuanLyCampaign(int page = 1, string search = "", string status = "")
        {
            Response.Charset = "utf-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            int pageSize = 10;
            DateTime now = DateTime.Now; // Khai báo ở đầu method

            List<CampaignInfo> lst = dbCampaign.DanhSachCampaign();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                lst = lst.Where(x =>
                    x.TenCampaign.ToLower().Contains(search.ToLower()) ||
                    (x.MoTa != null && x.MoTa.ToLower().Contains(search.ToLower()))
                ).ToList();
            }

            // Lọc theo trạng thái (sử dụng biến now đã khai báo ở đầu method)
            if (status == "active")
            {
                lst = lst.Where(x =>
                    x.TrangThai == true &&
                    (x.NgayBD == null || x.NgayBD <= now) &&
                    (x.NgayKT == null || x.NgayKT >= now)
                ).ToList();
            }
            else if (status == "expired")
            {
                lst = lst.Where(x => x.NgayKT != null && x.NgayKT < now).ToList();
            }
            else if (status == "upcoming")
            {
                lst = lst.Where(x => x.NgayBD != null && x.NgayBD > now).ToList();
            }
            else if (status == "inactive")
            {
                lst = lst.Where(x => x.TrangThai == false).ToList();
            }

            // Tổng số item sau khi lọc
            int totalItems = lst.Count;

            // Phân trang
            lst = lst.Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToList();

            // Gửi dữ liệu sang View
            ViewBag.Page = page;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.From = totalItems == 0 ? 0 : ((page - 1) * pageSize + 1);
            ViewBag.To = Math.Min(page * pageSize, totalItems);
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(lst);
        }

        // GET: Thêm Campaign
        [HttpGet]
        public ActionResult ThemCampaign()
        {
            ViewBag.DanhMucs = db.DanhSachDanhMuc();
            ViewBag.SanPhams = db.DanhSachSanPham();
            return View();
        }

        // POST: Thêm Campaign
        [HttpPost]
        public ActionResult ThemCampaign(CampaignInfo campaign, string[] selectedSanPhams, string[] selectedDanhMucs, string NgayBD, string NgayKT)
        {
            ViewBag.DanhMucs = db.DanhSachDanhMuc();
            ViewBag.SanPhams = db.DanhSachSanPham();

            // Xử lý datetime
            if (!string.IsNullOrEmpty(NgayBD))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayBD, out parsedDate))
                {
                    campaign.NgayBD = parsedDate;
                }
            }

            if (!string.IsNullOrEmpty(NgayKT))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayKT, out parsedDate))
                {
                    campaign.NgayKT = parsedDate;
                }
            }

            // Validate
            if (string.IsNullOrEmpty(campaign.TenCampaign))
            {
                ViewBag.Error = "Tên campaign không được để trống!";
                return View(campaign);
            }

            if (campaign.NgayBD.HasValue && campaign.NgayKT.HasValue && campaign.NgayBD > campaign.NgayKT)
            {
                ViewBag.Error = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc!";
                return View(campaign);
            }

            if ((selectedSanPhams == null || selectedSanPhams.Length == 0) && 
                (selectedDanhMucs == null || selectedDanhMucs.Length == 0))
            {
                ViewBag.Error = "Vui lòng chọn ít nhất một sản phẩm hoặc một danh mục!";
                return View(campaign);
            }

            // Lấy mã nhân viên từ session
            var nhanVien = Session["NhanVien"] as NhanVien;
            if (nhanVien != null)
            {
                campaign.NguoiTao = nhanVien.MaNhanVien;
            }

            campaign.LoaiCampaign = "Chủ động";
            campaign.TrangThai = true;

            // Chuyển đổi danh sách
            List<int> danhSachSanPham = selectedSanPhams != null 
                ? selectedSanPhams.Select(int.Parse).ToList() 
                : new List<int>();
            List<int> danhSachDanhMuc = selectedDanhMucs != null 
                ? selectedDanhMucs.Select(int.Parse).ToList() 
                : new List<int>();

            if (dbCampaign.ThemCampaign(campaign, danhSachSanPham, danhSachDanhMuc))
            {
                TempData["Success"] = "Thêm campaign thành công!";
                return RedirectToAction("QuanLyCampaign");
            }
            else
            {
                ViewBag.Error = "Có lỗi xảy ra khi thêm campaign!";
                return View(campaign);
            }
        }

        // GET: Sửa Campaign
        [HttpGet]
        public ActionResult SuaCampaign(int id)
        {
            var campaign = dbCampaign.LayCampaignTheoMa(id);
            if (campaign == null)
            {
                return HttpNotFound();
            }

            ViewBag.DanhMucs = db.DanhSachDanhMuc();
            ViewBag.SanPhams = db.DanhSachSanPham();
            ViewBag.SelectedSanPhams = dbCampaign.LayDanhSachSanPham(id);
            ViewBag.SelectedDanhMucs = dbCampaign.LayDanhSachDanhMuc(id);

            return View(campaign);
        }

        // POST: Sửa Campaign
        [HttpPost]
        public ActionResult SuaCampaign(CampaignInfo campaign, string[] selectedSanPhams, string[] selectedDanhMucs, string NgayBD, string NgayKT)
        {
            ViewBag.DanhMucs = db.DanhSachDanhMuc();
            ViewBag.SanPhams = db.DanhSachSanPham();
            
            // Parse selectedSanPhams an toàn
            List<int> selectedSP = new List<int>();
            if (selectedSanPhams != null)
            {
                foreach (var sp in selectedSanPhams)
                {
                    int id;
                    if (int.TryParse(sp, out id))
                        selectedSP.Add(id);
                }
            }
            ViewBag.SelectedSanPhams = selectedSP;
            
            // Parse selectedDanhMucs an toàn
            List<int> selectedDM = new List<int>();
            if (selectedDanhMucs != null)
            {
                foreach (var dm in selectedDanhMucs)
                {
                    int id;
                    if (int.TryParse(dm, out id))
                        selectedDM.Add(id);
                }
            }
            ViewBag.SelectedDanhMucs = selectedDM;

            // Xử lý datetime
            if (!string.IsNullOrEmpty(NgayBD))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayBD, out parsedDate))
                {
                    campaign.NgayBD = parsedDate;
                }
            }
            else
            {
                campaign.NgayBD = null;
            }

            if (!string.IsNullOrEmpty(NgayKT))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(NgayKT, out parsedDate))
                {
                    campaign.NgayKT = parsedDate;
                }
            }
            else
            {
                campaign.NgayKT = null;
            }

            // Validate
            if (string.IsNullOrEmpty(campaign.TenCampaign))
            {
                ViewBag.Error = "Tên campaign không được để trống!";
                return View(campaign);
            }

            if (campaign.NgayBD.HasValue && campaign.NgayKT.HasValue && campaign.NgayBD > campaign.NgayKT)
            {
                ViewBag.Error = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc!";
                return View(campaign);
            }

            if (selectedSP.Count == 0 && selectedDM.Count == 0)
            {
                ViewBag.Error = "Vui lòng chọn ít nhất một sản phẩm hoặc một danh mục!";
                return View(campaign);
            }

            // Sử dụng danh sách đã parse an toàn
            List<int> danhSachSanPham = selectedSP;
            List<int> danhSachDanhMuc = selectedDM;

            if (dbCampaign.SuaCampaign(campaign, danhSachSanPham, danhSachDanhMuc))
            {
                TempData["Success"] = "Sửa campaign thành công!";
                return RedirectToAction("QuanLyCampaign");
            }
            else
            {
                ViewBag.Error = "Có lỗi xảy ra khi sửa campaign!";
                return View(campaign);
            }
        }

        // Xóa Campaign
        [HttpPost]
        public ActionResult XoaCampaign(int id)
        {
            if (dbCampaign.XoaCampaign(id))
            {
                TempData["Success"] = "Xóa campaign thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa campaign!";
            }
            return RedirectToAction("QuanLyCampaign");
        }

        // Chi tiết Campaign
        public ActionResult ChiTietCampaign(int id)
        {
            var campaign = dbCampaign.LayCampaignTheoMa(id);
            if (campaign == null)
            {
                return HttpNotFound();
            }

            ViewBag.DanhSachSanPham = dbCampaign.LayDanhSachSanPham(id);
            ViewBag.DanhSachDanhMuc = dbCampaign.LayDanhSachDanhMuc(id);
            ViewBag.SanPhams = db.DanhSachSanPham();
            ViewBag.DanhMucs = db.DanhSachDanhMuc();

            return View(campaign);
        }

    }
}