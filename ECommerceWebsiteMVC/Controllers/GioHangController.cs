using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ECommerceWebsiteMVC.Models;
using System.Data.Entity;

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

        // ==========================================
        // 1. HIỂN THỊ GIỎ HÀNG
        // ==========================================
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
                            .ToList();

            var items = dbItems.Select(item => new GioHangItemViewModel
            {
                MaCTGH = item.MaCTGH,
                MaBienThe = (int)item.MaBienThe,
                MaCuaHang = item.BienTheSanPham.SanPham.CuaHang.MaCuaHang,
                TenCuaHang = item.BienTheSanPham.SanPham.CuaHang.TenCuaHang,
                TenSanPham = item.BienTheSanPham.SanPham.TenSanPham,
                TenBienThe = item.BienTheSanPham.TenBienThe,
                HinhAnh = item.BienTheSanPham.HinhAnh,
                DonGia = (decimal)item.BienTheSanPham.GiaBan,
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
            }).ToList();

            var grouped = items.GroupBy(x => x.MaCuaHang)
                .Select(g => new ShopGroup { MaCuaHang = g.Key, TenCuaHang = g.First().TenCuaHang, Items = g.ToList() })
                .ToList();

            return View(new GioHangViewModel { Shops = grouped });
        }

        // ==========================================
        // 2. CÁC HÀM XỬ LÝ GIỎ HÀNG (THÊM/SỬA/XÓA)
        // ==========================================
        public ActionResult AddToCart(int maBienThe, int soLuong = 1)
        {
            int userId = GetUserId();
            if (userId == -1) return RedirectToAction("DangNhap", "TaiKhoan");

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

        public ActionResult MuaNgay(int maBienThe)
        {
            AddToCart(maBienThe, 1);
            return RedirectToAction("Index");
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

        // ==========================================
        // 3. CHECKOUT & THANH TOÁN
        // ==========================================
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

            var items = dbItems.Select(i => new CheckoutItemVM
            {
                MaCTGH = i.MaCTGH,
                MaBienThe = (int)i.MaBienThe,
                TenSanPham = i.BienTheSanPham.SanPham.TenSanPham,
                PhanLoai = i.BienTheSanPham.TenBienThe,
                DonGia = (decimal)i.BienTheSanPham.GiaBan,
                SoLuong = (int)i.SoLuong,
                HinhAnh = i.BienTheSanPham.HinhAnh
            }).ToList();

            var model = new CheckoutViewModel
            {
                TenNguoiNhan = user != null ? user.HoVaTen : "",
                SDT = user != null ? user.SDT : "",

                // --- SỬA LỖI Ở ĐÂY: Gán chuỗi rỗng ---
                DiaChi = "",

                Items = items,

                // Lấy Voucher
                DanhSachVoucher = db.GiamGias.Where(v => v.NgayKT >= DateTime.Now).ToList(),

                TongTienHang = items.Sum(x => x.ThanhTien),
                PhiVanChuyen = 38000,
                GiamGia = 0
            };
            model.TongThanhToan = model.TongTienHang + model.PhiVanChuyen;

            return View("Checkout", model);
        }

        // --- HÀM API CHO VOUCHER (Mới thêm) ---
        [HttpPost]
        public ActionResult ApplyVoucher(int maVoucher, decimal tongTienHang)
        {
            var vc = db.GiamGias.Find(maVoucher);
            if (vc == null) return Json(new { success = false });
            if (vc.GiaTriDonHangToiThieu > tongTienHang) return Json(new { success = false });

            decimal giam = 0;
            if (vc.GiaTriGiam <= 1)
            {
                giam = tongTienHang * (decimal)vc.GiaTriGiam;
                if (vc.GiaTriGiamToiDa > 0 && giam > vc.GiaTriGiamToiDa) giam = (decimal)vc.GiaTriGiamToiDa;
            }
            else giam = (decimal)vc.GiaTriGiam;

            decimal tong = tongTienHang + 38000 - giam;
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
        // ==========================================
        // 9. API LẤY LỊCH SỬ ĐỊA CHỈ (Dựa trên SDT vì DonHang không có MaNguoiMua)
        // ==========================================
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
                    // Nếu bảng NguoiMua trong code chưa cập nhật cột DiaChi thì dòng dưới có thể lỗi
                    // Nếu lỗi, hãy xóa đoạn "user.DiaChi" thay bằng ""
                    DiaChi = (user.GetType().GetProperty("DiaChi") != null) ? (string)user.GetType().GetProperty("DiaChi").GetValue(user, null) : "",
                    IsDefault = true
                });

                // 2. Lấy lịch sử từ đơn hàng cũ
                // Logic: Tìm đơn hàng có cùng SDT với người dùng hiện tại
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

                // 3. Lọc trùng
                foreach (var item in history)
                {
                    // Lấy địa chỉ user (xử lý an toàn nếu null)
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

        // ==========================================
        // 7. THANH TOÁN (ORDER SUCCESS) - CHUẨN DB MỚI
        // ==========================================
        [HttpPost]
        public ActionResult OrderSuccess(string NguoiNhan, string SDT, string DiaChi, string MaVoucher)
        {
            int userId = GetUserId(); // Chỉ dùng để check đăng nhập, không lưu vào DonHang

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

                    // --- TÍNH TIỀN ---
                    decimal tongTienHang = 0;
                    foreach (var item in cartItems)
                    {
                        var bt = db.BienTheSanPhams.Find(item.MaBienThe);
                        tongTienHang += (decimal)(bt.GiaBan * item.SoLuong);
                    }

                    // --- XỬ LÝ VOUCHER ---
                    decimal giamGia = 0;
                    int? maGiamGiaID = null;

                    if (!string.IsNullOrEmpty(MaVoucher))
                    {
                        int vID = int.Parse(MaVoucher);
                        var vc = db.GiamGias.Find(vID);
                        if (vc != null)
                        {
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

                    // --- TẠO ĐƠN HÀNG (Khớp 100% ảnh image_b0ff7d.png) ---
                    var dh = new DonHang
                    {
                        // Không gán MaNguoiMua vì bảng không có
                        MaDVVC = 1, // Bắt buộc (int, not null) - Giả sử 1 là Giao hàng nhanh

                        TenNguoiNhan = NguoiNhan,
                        SDT = SDT,
                        DiaChi = DiaChi,

                        ThoiGianDat = DateTime.Now, // CHUẨN: ThoiGianDat

                        TongTien = tongTienHang + 38000 - giamGia,
                        PhiVanChuyen = 38000,

                        MaGiamGia = maGiamGiaID, // CHUẨN: MaGiamGia (Allow Nulls)

                        TrangThaiDonHang = "DangXuLy",
                        TrangThaiVanChuyen = "ChuaGiao", // Allow Nulls nhưng nên gán
                        TrangThaiThanhToan = false,      // CHUẨN: bit (boolean)

                        GhiChu = "" // Allow Nulls
                    };

                    db.DonHangs.Add(dh);
                    db.SaveChanges();

                    // --- CHI TIẾT ĐƠN HÀNG ---
                    foreach (var item in cartItems)
                    {
                        var bt = db.BienTheSanPhams.Find(item.MaBienThe);

                        // Check tồn kho (SoLuongTonKho)
                        if (bt.SoLuongTonKho < item.SoLuong)
                        {
                            transaction.Rollback();
                            TempData["Error"] = $"Sản phẩm {bt.TenBienThe} hết hàng!";
                            return RedirectToAction("Index");
                        }

                        bt.SoLuongTonKho -= item.SoLuong;

                        var ctdh = new ChiTietDonHang
                        {
                            MaDonHang = dh.MaDonHang,
                            MaCTGH = item.MaCTGH, // Có cột này trong ChiTietDonHang
                            // MaBienThe = item.MaBienThe, // XÓA DÒNG NÀY nếu bảng ChiTietDonHang không có
                            SoLuong = item.SoLuong,
                            // DonGia = bt.GiaBan, // Nếu bảng có cột DonGia thì mở ra
                            // ThanhTien = ... // Nếu bảng có cột ThanhTien thì mở ra
                        };

                        // Kiểm tra lại bảng ChiTietDonHang xem có cột DonGia/ThanhTien không?
                        // Dựa vào code cũ của bạn thì có, tôi sẽ thêm vào:
                        // Nếu báo lỗi đỏ thì xóa đi nhé.
                        try { ctdh.GetType().GetProperty("DonGia").SetValue(ctdh, bt.GiaBan); } catch { }
                        try { ctdh.GetType().GetProperty("ThanhTien").SetValue(ctdh, (decimal)(bt.GiaBan * item.SoLuong)); } catch { }

                        db.ChiTietDonHangs.Add(ctdh);
                        item.TrangThai = false;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return View("OrderSuccess", dh);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }
    }
}