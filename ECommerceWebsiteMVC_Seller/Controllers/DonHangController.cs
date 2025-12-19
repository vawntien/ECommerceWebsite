using ECommerceWebsiteMVC_Seller.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC_Seller.Controllers
{
    public class DonHangController : Controller
    {
        private readonly ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        // GET: DonHang
        // searchType: MaDonHang | TenNguoiMua | SanPham
        public ActionResult Index(string keyword = null, string searchType = "MaDonHang", int? maDonViVanChuyen = null, string trangThai = null)
        {
            if (Session["MaNguoiBan"] == null)
            {
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");
            }

            int maNguoiBan = (int)Session["MaNguoiBan"];

            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
            {
                return Content("Bạn chưa tạo cửa hàng.");
            }

            int maCuaHang = cuaHang.MaCuaHang;

            var query = db.DonHangs
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham.AnhSanPhams))
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.GioHang.NguoiMua))
                .Include(dh => dh.DonViVanChuyen)
                .Where(dh => dh.ChiTietDonHangs
                    .Any(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == maCuaHang));

            // Tìm kiếm theo loại (mã đơn, tên người mua, tên sản phẩm)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                if (string.Equals(searchType, "TenNguoiMua", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(dh =>
                        dh.ChiTietDonHangs.Any(ct =>
                            ct.ChiTietGioHang.GioHang.NguoiMua.HoVaTen.Contains(keyword)));
                }
                else if (string.Equals(searchType, "SanPham", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(dh =>
                        dh.ChiTietDonHangs.Any(ct =>
                            ct.ChiTietGioHang.BienTheSanPham.SanPham.TenSanPham.Contains(keyword)));
                }
                else // Mã đơn hàng
                {
                    if (int.TryParse(keyword, out int maDonHangSearch))
                    {
                        query = query.Where(dh => dh.MaDonHang == maDonHangSearch);
                    }
                    else
                    {
                        // Nếu người dùng nhập không phải số, cho kết quả rỗng một cách an toàn
                        query = query.Where(dh => false);
                    }
                }
            }

            if (maDonViVanChuyen.HasValue)
            {
                query = query.Where(dh => dh.MaDVVC == maDonViVanChuyen.Value);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (trangThai == "DaHuy")
                {
                    query = query.Where(dh => dh.TrangThaiDonHang == "Đã hủy");
                }
                else if (trangThai == "ChoXacNhan")
                {
                    query = query.Where(dh => dh.TrangThaiDonHang == "Chờ xác nhận");
                }
                else if (trangThai == "ChoLayHang")
                {
                    query = query.Where(dh => dh.TrangThaiDonHang == "Chờ lấy hàng");
                }
                else if (trangThai == "ChoGiaoHang")
                {
                    query = query.Where(dh => dh.TrangThaiDonHang == "Chờ giao hàng");
                }
                else if (trangThai == "DaGiao")
                {
                    query = query.Where(dh => dh.TrangThaiDonHang == "Đã giao");
                }
            }

            var donHangs = query
                .OrderByDescending(dh => dh.ThoiGianDat)
                .ToList();

            var model = new List<SellerOrderItemViewModel>();

            foreach (var dh in donHangs)
            {
                var chiTietChoCuaHang = dh.ChiTietDonHangs
                    .Where(ct => ct.ChiTietGioHang != null &&
                                 ct.ChiTietGioHang.BienTheSanPham != null &&
                                 ct.ChiTietGioHang.BienTheSanPham.SanPham != null &&
                                 ct.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == maCuaHang)
                    .ToList();

                if (!chiTietChoCuaHang.Any())
                    continue;

                var ctFirst = chiTietChoCuaHang.First();
                var chiTietGioHang = ctFirst.ChiTietGioHang;
                var bienThe = chiTietGioHang?.BienTheSanPham;
                var sanPham = bienThe?.SanPham;
                var nguoiMua = chiTietGioHang?.GioHang?.NguoiMua;

                string anhUrl = null;
                if (sanPham != null)
                {
                    // MacDinh là Nullable<bool> nên cần so sánh == true thay vì dùng trực tiếp trong predicate
                    var anhMacDinh = sanPham.AnhSanPhams?.FirstOrDefault(a => a.MacDinh == true);
                    if (anhMacDinh != null)
                    {
                        anhUrl = $"https://ecommerceshopstorage.blob.core.windows.net/products/{sanPham.MaSanPham}/{anhMacDinh.HinhAnh}";
                    }
                }

                var item = new SellerOrderItemViewModel
                {
                    MaDonHang = dh.MaDonHang,
                    TenNguoiMua = nguoiMua?.HoVaTen ?? dh.TenNguoiNhan ?? "Khách hàng",
                    TenNguoiNhan = dh.TenNguoiNhan,
                    DiaChi = dh.DiaChi,
                    SDT = dh.SDT,
                    ThoiGianDat = dh.ThoiGianDat,
                    TongTien = dh.TongTien,
                    TongSoLuong = chiTietChoCuaHang.Sum(ct => ct.SoLuong),
                    TrangThaiDonHang = dh.TrangThaiDonHang,
                    TrangThaiVanChuyen = dh.TrangThaiVanChuyen,
                    TrangThaiThanhToan = dh.TrangThaiThanhToan,
                    DonViVanChuyen = dh.DonViVanChuyen?.TenDVVC ?? "Không rõ",
                    TenSanPhamDauTien = sanPham?.TenSanPham ?? "Sản phẩm",
                    TenBienTheDauTien = bienThe?.TenBienThe,
                    SoLuongSanPhamDauTien = ctFirst.SoLuong,
                    DonGiaSanPhamDauTien = ctFirst.DonGia,
                    AnhSanPhamDauTien = anhUrl
                };

                model.Add(item);
            }

            // Dữ liệu cho bộ lọc & hiển thị lại từ khóa
            ViewBag.DonVisVanChuyen = db.DonViVanChuyens.ToList();
            ViewBag.CurrentTrangThai = trangThai ?? "TatCa";
            ViewBag.SearchType = searchType;
            ViewBag.Keyword = keyword;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HuyDonHangSeller(int id)
        {
            if (Session["MaNguoiBan"] == null)
            {
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");
            }

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
            {
                return Content("Bạn chưa tạo cửa hàng.");
            }

            int maCuaHang = cuaHang.MaCuaHang;

            var donHang = db.DonHangs
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham))
                .SingleOrDefault(dh => dh.MaDonHang == id);

            if (donHang == null ||
                !donHang.ChiTietDonHangs.Any(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == maCuaHang))
            {
                return HttpNotFound("Đơn hàng không thuộc cửa hàng của bạn.");
            }

            if (donHang.TrangThaiDonHang != "Chờ xác nhận")
            {
                TempData["UpdateSuccess"] = "Chỉ có thể hủy đơn ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction("Index");
            }

            donHang.TrangThaiDonHang = "Đã hủy";
            db.SaveChanges();

            TempData["UpdateSuccess"] = "Đơn hàng đã được hủy.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanDonHangSeller(int id)
        {
            if (Session["MaNguoiBan"] == null)
            {
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");
            }

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
            {
                return Content("Bạn chưa tạo cửa hàng.");
            }

            int maCuaHang = cuaHang.MaCuaHang;

            var donHang = db.DonHangs
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham))
                .SingleOrDefault(dh => dh.MaDonHang == id);

            if (donHang == null ||
                !donHang.ChiTietDonHangs.Any(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == maCuaHang))
            {
                return HttpNotFound("Đơn hàng không thuộc cửa hàng của bạn.");
            }

            if (donHang.TrangThaiDonHang != "Chờ xác nhận")
            {
                TempData["UpdateSuccess"] = "Chỉ có thể xác nhận đơn ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction("Index");
            }

            donHang.TrangThaiDonHang = "Chờ lấy hàng";
            db.SaveChanges();

            TempData["UpdateSuccess"] = "Đơn hàng đã được xác nhận. Vui lòng chuẩn bị hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DaBanGiaoHang(int id)
        {
            if (Session["MaNguoiBan"] == null)
            {
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");
            }

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
            {
                return Content("Bạn chưa tạo cửa hàng.");
            }

            int maCuaHang = cuaHang.MaCuaHang;

            var donHang = db.DonHangs
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham))
                .SingleOrDefault(dh => dh.MaDonHang == id);

            if (donHang == null ||
                !donHang.ChiTietDonHangs.Any(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == maCuaHang))
            {
                return HttpNotFound("Đơn hàng không thuộc cửa hàng của bạn.");
            }

            if (donHang.TrangThaiDonHang != "Chờ lấy hàng")
            {
                TempData["UpdateSuccess"] = "Chỉ có thể đánh dấu 'Đã bàn giao hàng' ở trạng thái 'Chờ lấy hàng'.";
                return RedirectToAction("Index");
            }

            donHang.TrangThaiDonHang = "Chờ giao hàng";
            db.SaveChanges();

            TempData["UpdateSuccess"] = "Đơn hàng đã được bàn giao cho đơn vị vận chuyển.";
            return RedirectToAction("Index");
        }

        // GET: DonHang/ChiTiet/5
        public ActionResult ChiTiet(int id)
        {
            if (Session["MaNguoiBan"] == null)
            {
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");
            }

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
            {
                return Content("Bạn chưa tạo cửa hàng.");
            }

            int maCuaHang = cuaHang.MaCuaHang;

            var donHang = db.DonHangs
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham.AnhSanPhams))
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.GioHang.NguoiMua))
                .Include(dh => dh.DonViVanChuyen)
                .SingleOrDefault(dh => dh.MaDonHang == id);

            if (donHang == null)
            {
                return HttpNotFound("Đơn hàng không tồn tại.");
            }

            // Lấy chi tiết đơn thuộc cửa hàng này
            var chiTietChoCuaHang = donHang.ChiTietDonHangs
                .Where(ct => ct.ChiTietGioHang != null &&
                             ct.ChiTietGioHang.BienTheSanPham != null &&
                             ct.ChiTietGioHang.BienTheSanPham.SanPham != null &&
                             ct.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == maCuaHang)
                .ToList();

            if (!chiTietChoCuaHang.Any())
            {
                return HttpNotFound("Đơn hàng không thuộc cửa hàng của bạn.");
            }

            var model = new SellerOrderDetailViewModel
            {
                MaDonHang = donHang.MaDonHang,
                TrangThaiDonHang = donHang.TrangThaiDonHang,
                TrangThaiVanChuyen = donHang.TrangThaiVanChuyen,
                TrangThaiThanhToan = donHang.TrangThaiThanhToan,
                DonViVanChuyen = donHang.DonViVanChuyen?.TenDVVC ?? "Không rõ",
                TenNguoiNhan = donHang.TenNguoiNhan,
                DiaChi = donHang.DiaChi,
                SDT = donHang.SDT,
                ThoiGianDat = donHang.ThoiGianDat,
                PhiVanChuyen = donHang.PhiVanChuyen,
                TongTienDonHang = donHang.TongTien,
                GhiChuTuNguoiMua = donHang.GhiChu
            };

            foreach (var ct in chiTietChoCuaHang)
            {
                var chiTietGioHang = ct.ChiTietGioHang;
                var bienThe = chiTietGioHang?.BienTheSanPham;
                var sanPham = bienThe?.SanPham;

                string anhUrl = null;
                if (sanPham != null)
                {
                    var anhMacDinh = sanPham.AnhSanPhams?.FirstOrDefault(a => a.MacDinh == true);
                    if (anhMacDinh != null)
                    {
                        anhUrl = $"https://ecommerceshopstorage.blob.core.windows.net/products/{sanPham.MaSanPham}/{anhMacDinh.HinhAnh}";
                    }
                }

                var itemVm = new SellerOrderItemDetailViewModel
                {
                    MaSanPham = sanPham?.MaSanPham ?? 0,
                    MaBienThe = bienThe?.MaBienThe ?? 0,
                    TenSanPham = sanPham?.TenSanPham,
                    TenBienThe = bienThe?.TenBienThe,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    AnhSanPham = anhUrl
                };

                model.Items.Add(itemVm);
            }

            model.TongTienSanPham = model.Items.Sum(i => i.ThanhTien);

            if (string.Equals(model.TrangThaiDonHang, "Đã hủy", StringComparison.OrdinalIgnoreCase))
            {
                model.LyDoHuy = "Đã hủy bởi Người mua";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanGiaoHangThanhCong(int id)
        {
            if (Session["MaNguoiBan"] == null)
            {
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");
            }

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null) return Content("Bạn chưa tạo cửa hàng.");

            int maCuaHang = cuaHang.MaCuaHang;

            var donHang = db.DonHangs
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham))
                .SingleOrDefault(dh => dh.MaDonHang == id);

            // Kiểm tra quyền sở hữu
            if (donHang == null || !donHang.ChiTietDonHangs.Any(ct => ct.ChiTietGioHang.BienTheSanPham.SanPham.MaCuaHang == maCuaHang))
            {
                // Sử dụng TempData để báo lỗi thất bại
                TempData["UpdateError"] = "Đơn hàng không tồn tại hoặc không thuộc cửa hàng của bạn.";
                return RedirectToAction("Index");
            }

            // Kiểm tra trạng thái hợp lệ
            if (donHang.TrangThaiDonHang != "Chờ giao hàng")
            {
                TempData["UpdateError"] = "Chỉ có thể xác nhận thành công cho đơn đang ở trạng thái 'Chờ giao hàng'.";
                return RedirectToAction("Index");
            }

            // Cập nhật trạng thái
            donHang.TrangThaiDonHang = "Đã giao";
            donHang.TrangThaiThanhToan = true;
            

            db.SaveChanges();

            TempData["UpdateSuccess"] = "Xác nhận giao hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}