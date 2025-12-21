using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ECommerceWebsiteMVC.Models;
using System.Data.Entity;
using System.IO;

namespace ECommerceWebsiteMVC.Controllers
{
    public class GioHangController : Controller
    {
        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        private int GetUserId()
        {
            if (Session["MaNguoiMua"] == null) return -1;
            return (int)Session["MaNguoiMua"];
        }

        // 1. HIỂN THỊ GIỎ HÀNG

        public ActionResult Index()
        {
            int userId = GetUserId();
            if (userId == -1) return RedirectToAction("DangNhap", "TaiKhoan");

            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaNguoiMua == userId);
            if (gioHang == null)
            {
                gioHang = new GioHang { MaNguoiMua = userId };
                db.GioHangs.Add(gioHang);
                db.SaveChanges();
            }

            var dbItems = db.ChiTietGioHangs
                            .Where(c => c.MaGioHang == gioHang.MaGioHang && c.TrangThai == true)
                            .Include(c => c.BienTheSanPham)
                            .Include(c => c.BienTheSanPham.SanPham)
                            .Include(c => c.BienTheSanPham.SanPham.CuaHang)
                            .OrderByDescending(c => c.MaCTGH) // Sắp xếp theo MaCTGH giảm dần để sản phẩm mới thêm vào hiển thị đầu tiên
                            .ToList();

            var items = dbItems.Select(item => {
                // Áp dụng campaign discount nếu có
                decimal giaGoc = (decimal)item.BienTheSanPham.GiaBan;
                var priceResult = Models.CampaignHelper.TinhGiaSauGiamChoBienThe(db, (int)item.MaBienThe, giaGoc);
                decimal giaApDung = priceResult.CoGiamGia ? priceResult.GiaSauGiam : giaGoc;
                
                return new GioHangItemViewModel
                {
                    MaCTGH = item.MaCTGH,
                    MaBienThe = (int)item.MaBienThe,
                    MaSanPham = item.BienTheSanPham.SanPham.MaSanPham, // Thêm MaSanPham để link đến trang chi tiết
                    MaCuaHang = item.BienTheSanPham.SanPham.CuaHang.MaCuaHang,
                    TenCuaHang = item.BienTheSanPham.SanPham.CuaHang.TenCuaHang,
                    TenSanPham = item.BienTheSanPham.SanPham.TenSanPham,
                    TenBienThe = item.BienTheSanPham.TenBienThe,
                    HinhAnh = item.BienTheSanPham.HinhAnh,
                    DonGia = giaApDung, // Giá sau khi áp dụng campaign
                    GiaGoc = giaGoc, // Giá gốc
                    CoGiamGia = priceResult.CoGiamGia,
                    PhanTramGiam = priceResult.PhanTramGiam,
                    SoLuong = (int)item.SoLuong,
                    SoLuongTonKho = (int)item.BienTheSanPham.SoLuongTonKho,
                    BienTheList = db.BienTheSanPhams
                                    .Where(bt => bt.MaSanPham == item.BienTheSanPham.MaSanPham)
                                    .Select(bt => new BienTheSanPhamViewModel
                                    {
                                        MaBienThe = bt.MaBienThe,
                                        TenBienThe = bt.TenBienThe,
                                        GiaBan = (decimal)bt.GiaBan,
                                        HinhAnh = bt.HinhAnh
                                    }).ToList()
                };
            }).ToList();

            var grouped = items.GroupBy(x => x.MaCuaHang)
                .Select(g => new ShopGroup 
                { 
                    MaCuaHang = g.Key, 
                    TenCuaHang = g.First().TenCuaHang, 
                    Items = g.OrderByDescending(i => i.MaCTGH).ToList() // Sắp xếp items trong mỗi shop theo MaCTGH giảm dần
                })
                .ToList();

            return View(new GioHangViewModel { Shops = grouped });
        }

        // 2. CÁC HÀM XỬ LÝ GIỎ HÀNG (THÊM/SỬA/XÓA)
        public ActionResult AddToCart(int maBienThe, int soLuong = 1)
        {
            int userId = GetUserId();
            if (userId == -1)
            {
                // Lưu thông tin sản phẩm vào Session để thêm vào giỏ hàng sau khi đăng nhập
                Session["PendingAddToCart_MaBienThe"] = maBienThe;
                Session["PendingAddToCart_SoLuong"] = soLuong;
                Session["PendingAddToCart_ActionType"] = "add";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var bt = db.BienTheSanPhams.Find(maBienThe);
            if (bt == null || bt.SoLuongTonKho < soLuong)
            {
                TempData["Error"] = "Số lượng không đủ!";
                return RedirectToAction("Index", "SanPham");
            }

            var gio = db.GioHangs.FirstOrDefault(x => x.MaNguoiMua == userId);
            if (gio == null)
            {
                gio = new GioHang { MaNguoiMua = userId };
                db.GioHangs.Add(gio);
                db.SaveChanges();
            }

            var item = db.ChiTietGioHangs.FirstOrDefault(x => x.MaGioHang == gio.MaGioHang && x.MaBienThe == maBienThe && x.TrangThai == true);
            if (item == null)
            {
                item = new ChiTietGioHang { MaGioHang = gio.MaGioHang, MaBienThe = maBienThe, SoLuong = soLuong, TrangThai = true };
                db.ChiTietGioHangs.Add(item);
            }
            else
            {
                if (item.SoLuong + soLuong > bt.SoLuongTonKho)
                {
                    TempData["Error"] = "Vượt quá tồn kho!";
                    return RedirectToAction("Index");
                }
                item.SoLuong += soLuong;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult MuaNgay(int maBienThe, int soLuong = 1)
        {
            int userId = GetUserId();
            if (userId == -1)
            {
                // Lưu thông tin sản phẩm vào Session để thêm vào giỏ hàng sau khi đăng nhập
                Session["PendingAddToCart_MaBienThe"] = maBienThe;
                Session["PendingAddToCart_SoLuong"] = soLuong;
                Session["PendingAddToCart_ActionType"] = "buy";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }
            
            // Mua Ngay chỉ thêm vào giỏ hàng, không redirect đến thanh toán
            return AddToCart(maBienThe, soLuong);
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int maCTGH, int soLuongMoi)
        {
            var ct = db.ChiTietGioHangs.Find(maCTGH);
            if (ct == null) return HttpNotFound();
            var bt = db.BienTheSanPhams.Find(ct.MaBienThe);

            if (bt.SoLuongTonKho < soLuongMoi)
                return Json(new { success = false, message = $"Kho chỉ còn {bt.SoLuongTonKho}!" });

            ct.SoLuong = soLuongMoi;
            db.SaveChanges();
            return Json(new { success = true, thanhTien = (decimal)bt.GiaBan * soLuongMoi });
        }

        [HttpPost]
        public ActionResult UpdateVariant(int maCTGH, int maBienTheMoi)
        {
            var currentItem = db.ChiTietGioHangs.Find(maCTGH);
            var btMoi = db.BienTheSanPhams.Find(maBienTheMoi);

            var existingItem = db.ChiTietGioHangs.FirstOrDefault(x => x.MaGioHang == currentItem.MaGioHang && x.MaBienThe == maBienTheMoi && x.TrangThai == true && x.MaCTGH != maCTGH);

            if (existingItem != null)
            {
                if ((existingItem.SoLuong + currentItem.SoLuong) > btMoi.SoLuongTonKho) return Json(new { success = false, message = "Không đủ hàng!" });
                existingItem.SoLuong += currentItem.SoLuong;
                db.ChiTietGioHangs.Remove(currentItem);
            }
            else
            {
                if (currentItem.SoLuong > btMoi.SoLuongTonKho) return Json(new { success = false, message = "Không đủ hàng!" });
                currentItem.MaBienThe = maBienTheMoi;
            }
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteItem(int id)
        {
            var item = db.ChiTietGioHangs.Find(id);
            if (item != null) { item.TrangThai = false; db.SaveChanges(); }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteSelected(int[] ids)
        {
            if (ids != null)
            {
                var items = db.ChiTietGioHangs.Where(x => ids.Contains(x.MaCTGH)).ToList();
                foreach (var item in items) item.TrangThai = false;
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        // 2 API LẤY SỐ LƯỢNG SẢN PHẨM TRONG GIỎ
        [HttpGet]
        public ActionResult GetCartCount()
        {
            int userId = GetUserId();
            if (userId == -1) return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);

            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaNguoiMua == userId);
            if (gioHang == null) return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);

            // Tính tổng số lượng (Sum of SoLuong)
            int count = db.ChiTietGioHangs
                          .Where(c => c.MaGioHang == gioHang.MaGioHang && c.TrangThai == true)
                          .Sum(c => (int?)c.SoLuong) ?? 0;

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

        // 2API LẤY DỮ LIỆU ĐỊA CHỈ VIỆT NAM
        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetVietnamAddresses()
        {
            try
            {
                var filePath = Server.MapPath("~/Scripts/data/vietnam-addresses.json");
                if (System.IO.File.Exists(filePath))
                {
                    var bytes = System.IO.File.ReadAllBytes(filePath);
                    return File(bytes, "application/json");
                }
                return Json(new { error = "File not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 3. CHECKOUT & THANH TOÁN
        [HttpPost]
        public ActionResult Checkout(string chon)
        {
            if (string.IsNullOrEmpty(chon))
            {
                TempData["Error"] = "Chưa chọn sản phẩm!";
                return RedirectToAction("Index");
            }
            TempData["SelectedIds"] = chon;
            return RedirectToAction("CheckoutConfirm");
        }

        public ActionResult CheckoutConfirm()
        {
            if (TempData["SelectedIds"] == null) return RedirectToAction("Index");

            var ids = TempData["SelectedIds"].ToString().Split(',').Select(int.Parse).ToList();
            TempData.Keep("SelectedIds");

            int userId = GetUserId();
            var user = db.NguoiMuas.Find(userId);

            var dbItems = db.ChiTietGioHangs
                            .Where(c => ids.Contains(c.MaCTGH))
                            .Include(c => c.BienTheSanPham)
                            .Include(c => c.BienTheSanPham.SanPham)
                            .ToList();

            var items = dbItems.Select(i => {
                decimal giaGoc = (decimal)i.BienTheSanPham.GiaBan;
                // Áp dụng campaign nếu có
                var priceResult = Models.CampaignHelper.TinhGiaSauGiamChoBienThe(db, i.MaBienThe, giaGoc);
                decimal giaApDung = priceResult.CoGiamGia ? priceResult.GiaSauGiam : giaGoc;
                
                return new CheckoutItemVM
                {
                    MaCTGH = i.MaCTGH,
                    MaBienThe = (int)i.MaBienThe,
                    TenSanPham = i.BienTheSanPham.SanPham.TenSanPham,
                    PhanLoai = i.BienTheSanPham.TenBienThe,
                    DonGia = giaApDung, // Giá sau khi áp dụng campaign
                    SoLuong = (int)i.SoLuong,
                    HinhAnh = i.BienTheSanPham.HinhAnh
                };
            }).ToList();

            decimal tongTienHang = items.Sum(x => x.DonGia * x.SoLuong);

            // Lấy thông tin từ đơn hàng gần nhất nếu user chưa có địa chỉ
            string tenNguoiNhan = user != null ? user.HoVaTen : "";
            string sdt = user != null ? user.SDT : "";
            string diaChi = "";

            // Nếu user có SDT, tìm đơn hàng gần nhất để lấy thông tin
            if (user != null && !string.IsNullOrEmpty(user.SDT))
            {
                var donHangGanNhat = db.DonHangs
                    .Where(d => d.SDT == user.SDT && !string.IsNullOrEmpty(d.DiaChi))
                    .OrderByDescending(d => d.ThoiGianDat)
                    .FirstOrDefault();

                if (donHangGanNhat != null)
                {
                    // Lấy thông tin từ đơn hàng gần nhất
                    tenNguoiNhan = donHangGanNhat.TenNguoiNhan ?? tenNguoiNhan;
                    sdt = donHangGanNhat.SDT ?? sdt;
                    diaChi = donHangGanNhat.DiaChi ?? diaChi;
                }
            }

            var model = new CheckoutViewModel
            {
                TenNguoiNhan = tenNguoiNhan,
                SDT = sdt,
                DiaChi = diaChi, // Địa chỉ từ đơn hàng gần nhất hoặc để trống
                Items = items,

                // Lấy Voucher có thể áp dụng
                // Điều kiện: 
                // 1. NgayBD <= DateTime.Now (voucher đã bắt đầu) hoặc NgayBD null
                // 2. NgayKT >= DateTime.Now (voucher chưa hết hạn) hoặc NgayKT null
                // 3. GiaTriDonHangToiThieu <= tongTienHang (đơn hàng đủ giá trị tối thiểu)
                DanhSachVoucher = db.GiamGias
                                    .Where(v => (v.NgayBD == null || v.NgayBD <= DateTime.Now) 
                                             && (v.NgayKT == null || v.NgayKT >= DateTime.Now) 
                                             && v.GiaTriDonHangToiThieu <= tongTienHang)
                                    .ToList(),
                
                // Lấy danh sách đơn vị vận chuyển
                DanhSachDVVC = db.DonViVanChuyens.ToList(),

                TongTienHang = tongTienHang,
                PhiVanChuyen = 17000, 
                GiamGia = 0
            };
            model.TongThanhToan = model.TongTienHang + model.PhiVanChuyen;

            return View("Checkout", model);
        }

        // --- HÀM API CHO VOUCHER (Mới thêm) ---
        [HttpPost]
        public ActionResult ApplyVoucher(int maVoucher, decimal tongTienHang, decimal phiShip)
        {
            var vc = db.GiamGias.Find(maVoucher);
            if (vc == null) return Json(new { success = false, message = "Voucher không tồn tại." });
            
            // Kiểm tra điều kiện ngày bắt đầu
            if (vc.NgayBD != null && vc.NgayBD > DateTime.Now)
                return Json(new { success = false, message = "Voucher chưa đến thời gian áp dụng." });
            
            // Kiểm tra điều kiện ngày kết thúc
            if (vc.NgayKT != null && vc.NgayKT < DateTime.Now)
                return Json(new { success = false, message = "Voucher đã hết hạn." });
            
            // Kiểm tra giá trị đơn hàng tối thiểu
            if (vc.GiaTriDonHangToiThieu > tongTienHang) 
                return Json(new { success = false, message = "Đơn hàng chưa đủ điều kiện áp dụng voucher này." });

            decimal giam = 0;
            if (vc.GiaTriGiam <= 1)
            {
                giam = tongTienHang * (decimal)vc.GiaTriGiam;
                if (vc.GiaTriGiamToiDa > 0 && giam > vc.GiaTriGiamToiDa) giam = (decimal)vc.GiaTriGiamToiDa;
            }
            else giam = (decimal)vc.GiaTriGiam;

            decimal tong = tongTienHang + phiShip - giam;
            if (tong < 0) tong = 0;

            return Json(new { success = true, giamGia = giam, tongThanhToan = tong });
        }

        //[HttpPost]
        //public ActionResult OrderSuccess(string NguoiNhan, string SDT, string DiaChi, string MaVoucher)
        //{
        //    int userId = GetUserId();
        //    if (TempData["SelectedIds"] == null) return RedirectToAction("Index");
        //    var idList = TempData["SelectedIds"].ToString().Split(',').Select(int.Parse).ToList();

        //    using (var transaction = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var cartItems = db.ChiTietGioHangs.Where(x => idList.Contains(x.MaCTGH) && x.TrangThai == true).ToList();
        //            if (!cartItems.Any()) { transaction.Rollback(); return RedirectToAction("Index"); }

        //            // Tính lại tiền
        //            decimal tongTienHang = 0;
        //            foreach (var item in cartItems)
        //            {
        //                var bt = db.BienTheSanPhams.Find(item.MaBienThe);
        //                tongTienHang += (decimal)(bt.GiaBan * item.SoLuong);
        //            }

        //            // Tính giảm giá
        //            decimal giamGia = 0;
        //            int? maVoucherId = null;
        //            if (!string.IsNullOrEmpty(MaVoucher))
        //            {
        //                int vId = int.Parse(MaVoucher);
        //                var vc = db.GiamGias.Find(vId);
        //                if (vc != null)
        //                {
        //                    maVoucherId = vId;
        //                    if (vc.GiaTriGiam <= 1)
        //                    {
        //                        giamGia = tongTienHang * (decimal)vc.GiaTriGiam;
        //                        if (vc.GiaTriGiamToiDa > 0 && giamGia > vc.GiaTriGiamToiDa) giamGia = (decimal)vc.GiaTriGiamToiDa;
        //                    }
        //                    else giamGia = (decimal)vc.GiaTriGiam;
        //                }
        //            }

        //            var dh = new DonHang
        //            {
        //                TenNguoiNhan = NguoiNhan,
        //                SDT = SDT,
        //                DiaChi = DiaChi,
        //                ThoiGianDat = DateTime.Now,
        //                TongTien = tongTienHang + 38000 - giamGia,
        //                MaGiamGia = maVoucherId, // Lưu voucher
        //                TrangThaiDonHang = "DangXuLy",
        //                TrangThaiVanChuyen = "ChuaGiao",
        //                TrangThaiThanhToan = false
        //            };
        //            db.DonHangs.Add(dh);
        //            db.SaveChanges();

        //            foreach (var item in cartItems)
        //            {
        //                var bt = db.BienTheSanPhams.Find(item.MaBienThe);
        //                if (bt.SoLuongTonKho < item.SoLuong) { transaction.Rollback(); return RedirectToAction("Index"); }

        //                bt.SoLuongTonKho -= item.SoLuong;

        //                var ctdh = new ChiTietDonHang
        //                {
        //                    MaDonHang = dh.MaDonHang,
        //                    MaCTGH = item.MaCTGH,
        //                    SoLuong = item.SoLuong,
        //                    DonGia = bt.GiaBan,
        //                    ThanhTien = (decimal)(bt.GiaBan * item.SoLuong)
        //                };
        //                db.ChiTietDonHangs.Add(ctdh);
        //                item.TrangThai = false;
        //            }

        //            db.SaveChanges();
        //            transaction.Commit();
        //            return View("OrderSuccess", dh);
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //            return RedirectToAction("Index");
        //        }
        //    }
        //}
        // 9. API LẤY LỊCH SỬ ĐỊA CHỈ (Dựa trên SDT vì DonHang không có MaNguoiMua)
        [HttpGet]
        public ActionResult GetDeliveryHistory()
        {
            int userId = GetUserId();
            if (userId == -1) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            // Lấy thông tin User hiện tại để lấy SDT
            var user = db.NguoiMuas.Find(userId);
            var list = new List<object>();

            if (user != null)
            {
                // 1. Thêm địa chỉ hiện tại trong hồ sơ (nếu có)
                list.Add(new
                {
                    HoTen = user.HoVaTen,
                    SDT = user.SDT,
                    DiaChi = (user.GetType().GetProperty("DiaChi") != null) ? (string)user.GetType().GetProperty("DiaChi").GetValue(user, null) : "",
                    IsDefault = true
                });

                var history = db.DonHangs
                                .Where(d => d.SDT == user.SDT && d.DiaChi != null)
                                .OrderByDescending(d => d.ThoiGianDat) // CHUẨN: ThoiGianDat
                                .Select(d => new
                                {
                                    HoTen = d.TenNguoiNhan,
                                    SDT = d.SDT,
                                    DiaChi = d.DiaChi
                                })
                                .Distinct()
                                .Take(5)
                                .ToList();
                foreach (var item in history)
                {
                    string userAddr = (user.GetType().GetProperty("DiaChi") != null) ? (string)user.GetType().GetProperty("DiaChi").GetValue(user, null) : "";

                    bool isDuplicate = (item.HoTen == user.HoVaTen && item.SDT == user.SDT && item.DiaChi == userAddr);

                    if (!isDuplicate)
                    {
                        list.Add(new
                        {
                            HoTen = item.HoTen,
                            SDT = item.SDT,
                            DiaChi = item.DiaChi,
                            IsDefault = false
                        });
                    }
                }
            }

            return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
        }

        // 7. THANH TOÁN (ORDER SUCCESS) - CHUẨN DB MỚI
        [HttpPost]
        public ActionResult OrderSuccess(string NguoiNhan, string SDT, string DiaChi, string MaVoucher, int MaDVVC, decimal PhiShip, string GhiChu)
        {
            int userId = GetUserId();

            if (TempData["SelectedIds"] == null) return RedirectToAction("Index");
            var idList = TempData["SelectedIds"].ToString().Split(',').Select(int.Parse).ToList();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var cartItems = db.ChiTietGioHangs
                                      .Where(x => idList.Contains(x.MaCTGH) && x.TrangThai == true)
                                      .ToList();

                    if (!cartItems.Any())
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Giỏ hàng lỗi.";
                        return RedirectToAction("Index");
                    }

                    // --- TÍNH TIỀN (Áp dụng Campaign nếu có) ---
                    decimal tongTienHang = 0;
                    foreach (var item in cartItems)
                    {
                        var bt = db.BienTheSanPhams.Find(item.MaBienThe);
                        decimal giaGoc = (decimal)bt.GiaBan;
                        // Áp dụng campaign nếu có
                        var priceResult = Models.CampaignHelper.TinhGiaSauGiamChoBienThe(db, item.MaBienThe, giaGoc);
                        decimal giaApDung = priceResult.CoGiamGia ? priceResult.GiaSauGiam : giaGoc;
                        tongTienHang += giaApDung * item.SoLuong;
                    }

                    // --- XỬ LÝ VOUCHER ---
                    decimal giamGia = 0;
                    int? maGiamGiaID = null;

                    if (!string.IsNullOrEmpty(MaVoucher))
                    {
                        int vID;
                        if (int.TryParse(MaVoucher, out vID))
                        {
                            var vc = db.GiamGias.Find(vID);
                            if (vc != null)
                            {
                                // Kiểm tra ngày bắt đầu
                                if (vc.NgayBD != null && vc.NgayBD > DateTime.Now)
                                {
                                    transaction.Rollback();
                                    TempData["Error"] = "Voucher chưa đến thời gian áp dụng.";
                                    return RedirectToAction("Index");
                                }

                                // Kiểm tra ngày kết thúc
                                if (vc.NgayKT != null && vc.NgayKT < DateTime.Now)
                                {
                                    transaction.Rollback();
                                    TempData["Error"] = "Voucher đã hết hạn.";
                                    return RedirectToAction("Index");
                                }

                                // Kiểm tra giá trị đơn hàng tối thiểu
                                if (vc.GiaTriDonHangToiThieu > tongTienHang)
                                {
                                    transaction.Rollback();
                                    TempData["Error"] = "Đơn hàng chưa đủ điều kiện áp dụng voucher này.";
                                    return RedirectToAction("Index");
                                }

                                // Tính toán giảm giá
                                maGiamGiaID = vID;
                                if (vc.GiaTriGiam <= 1)
                                {
                                    giamGia = tongTienHang * (decimal)vc.GiaTriGiam;
                                    if (vc.GiaTriGiamToiDa > 0 && giamGia > vc.GiaTriGiamToiDa)
                                        giamGia = (decimal)vc.GiaTriGiamToiDa;
                                }
                                else
                                {
                                    giamGia = (decimal)vc.GiaTriGiam;
                                }
                            }
                        }
                    }

                    var dh = new DonHang
                    {

                        MaDVVC = MaDVVC,

                        TenNguoiNhan = NguoiNhan,
                        SDT = SDT,
                        DiaChi = DiaChi,

                        ThoiGianDat = DateTime.Now,

                        TongTien = Math.Max(0, tongTienHang + PhiShip - giamGia), // Đảm bảo tổng tiền không âm
                        PhiVanChuyen = PhiShip,

                        MaGiamGia = maGiamGiaID,

                        TrangThaiDonHang = "Chờ xác nhận",
                        TrangThaiVanChuyen = "Chưa giao", 
                        TrangThaiThanhToan = false,

                        GhiChu = GhiChu ?? ""
                    };

                    db.DonHangs.Add(dh);
                    db.SaveChanges();

                    // --- CHI TIẾT ĐƠN HÀNG ---
                    foreach (var item in cartItems)
                    {
                        var bt = db.BienTheSanPhams.Find(item.MaBienThe);
                        if (bt == null)
                        {
                            transaction.Rollback();
                            TempData["Error"] = "Sản phẩm không tồn tại!";
                            return RedirectToAction("Index");
                        }

                        // Kiểm tra số lượng tồn kho
                        if (bt.SoLuongTonKho < item.SoLuong)
                        {
                            transaction.Rollback();
                            TempData["Error"] = $"Sản phẩm {bt.TenBienThe} chỉ còn {bt.SoLuongTonKho} sản phẩm!";
                            return RedirectToAction("Index");
                        }

                        // Trừ số lượng tồn kho
                        bt.SoLuongTonKho -= item.SoLuong;

                        // Tính giá sau khi áp dụng campaign
                        decimal giaGoc = (decimal)bt.GiaBan;
                        var priceResult = Models.CampaignHelper.TinhGiaSauGiamChoBienThe(db, item.MaBienThe, giaGoc);
                        decimal giaApDung = priceResult.CoGiamGia ? priceResult.GiaSauGiam : giaGoc;
                        decimal thanhTien = giaApDung * item.SoLuong;

                        var ctdh = new ChiTietDonHang
                        {
                            MaDonHang = dh.MaDonHang,
                            MaCTGH = item.MaCTGH,
                            SoLuong = item.SoLuong,
                        };

                        // Lưu giá sau khi áp dụng campaign
                        try { ctdh.GetType().GetProperty("DonGia").SetValue(ctdh, giaApDung); } catch { }
                        try { ctdh.GetType().GetProperty("ThanhTien").SetValue(ctdh, thanhTien); } catch { }

                        db.ChiTietDonHangs.Add(ctdh);
                        item.TrangThai = false;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return View("OrderSuccess", dh.MaDonHang);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        // 9. XEM ĐƠN HÀNG SAU KHI THANH TOÁN THÀNH CÔNG
        public ActionResult ViewOrderSuccess(int id)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("Index");
            }
            return View("OrderSuccess", donHang);
        }
    }
}