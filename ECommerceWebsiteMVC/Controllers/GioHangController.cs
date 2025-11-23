using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ECommerceWebsiteMVC.Models;

namespace ECommerceWebsiteMVC.Controllers
{
    public class GioHangController : Controller
    {
        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        // LẤY USER ID (tạm thời lấy đại user đầu tiên)
        private int GetUserId()
        {
            if (Session["MaNguoiDung"] != null)
                return (int)Session["MaNguoiDung"];

            return db.NguoiDungs.OrderBy(u => u.MaNguoiDung).First().MaNguoiDung;
        }

        // SESSION PATCH
        private List<CartPatch> GetPatches()
        {
            if (Session["PATCHES"] == null)
                Session["PATCHES"] = new List<CartPatch>();
            return (List<CartPatch>)Session["PATCHES"];
        }

        private void SavePatches(List<CartPatch> patches)
        {
            Session["PATCHES"] = patches;
        }

        // ============================
        // HIỂN THỊ GIỎ HÀNG
        // ============================
        public ActionResult Index()
        {
            int userId = GetUserId();

            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaNguoiDung == userId);
            if (gioHang == null)
            {
                gioHang = new GioHang { MaNguoiDung = userId };
                db.GioHangs.Add(gioHang);
                db.SaveChanges();
            }

            var dbItems = db.ChiTietGioHangs
                            .Where(c => c.MaGioHang == gioHang.MaGioHang)
                            .ToList();

            var patches = GetPatches();

            var items = dbItems
                .Where(dbItem => !patches.Any(p => p.MaCTGH == dbItem.MaCTGH && p.IsDeleted))
                .Select(dbItem =>
                {
                    var patch = patches.FirstOrDefault(p => p.MaCTGH == dbItem.MaCTGH);

                    int maBienThe = patch?.NewBienThe ?? dbItem.MaBienThe;
                    int soLuong = patch?.NewSoLuong ?? dbItem.SoLuong;

                    var variant = db.BienTheSanPhams.Find(maBienThe);

                    return new GioHangItemViewModel
                    {
                        MaCTGH = dbItem.MaCTGH,
                        MaBienThe = maBienThe,
                        MaCuaHang = variant.SanPham.CuaHang.MaCuaHang,
                        TenCuaHang = variant.SanPham.CuaHang.TenCuaHang,
                        TenSanPham = variant.SanPham.TenSanPham,
                        TenBienThe = variant.TenBienThe,
                        HinhAnh = variant.HinhAnh,
                        DonGia = variant.GiaBan ,
                        SoLuong = soLuong,

                        BienTheList = db.BienTheSanPhams
                            .Where(bt => bt.MaSanPham == variant.MaSanPham)
                            .Select(bt => new BienTheSanPhamViewModel
                            {
                                MaBienThe = bt.MaBienThe,
                                TenBienThe = bt.TenBienThe,
                                GiaBan = bt.GiaBan,
                                HinhAnh = bt.HinhAnh
                            })
                            .ToList()
                    };
                })
                .ToList();

            var grouped = items
                .GroupBy(x => x.MaCuaHang)
                .Select(g => new ShopGroup
                {
                    MaCuaHang = g.Key,
                    TenCuaHang = g.First().TenCuaHang,
                    Items = g.ToList()
                })
                .ToList();

            return View(new GioHangViewModel { Shops = grouped });
        }

        // ============================
        // UPDATE SỐ LƯỢNG
        // ============================
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var patches = GetPatches();
            var patch = patches.FirstOrDefault(p => p.MaCTGH == id);

            if (patch == null)
                patches.Add(new CartPatch { MaCTGH = id, NewSoLuong = quantity });
            else
                patch.NewSoLuong = quantity;

            SavePatches(patches);
            return Json(new { success = true });
        }

        // ============================
        // UPDATE BIẾN THỂ
        // ============================
        [HttpPost]
        public ActionResult UpdateVariant(int maCTGH, int maBienThe)
        {
            var patches = GetPatches();
            var patch = patches.FirstOrDefault(p => p.MaCTGH == maCTGH);

            if (patch == null)
                patches.Add(new CartPatch { MaCTGH = maCTGH, NewBienThe = maBienThe });
            else
                patch.NewBienThe = maBienThe;

            SavePatches(patches);
            return Json(new { success = true });
        }

        // ============================
        // XÓA ITEM
        // ============================
        [HttpPost]
        public ActionResult DeleteItem(int id)
        {
            var patches = GetPatches();
            var p = patches.FirstOrDefault(x => x.MaCTGH == id);

            if (p == null)
                patches.Add(new CartPatch { MaCTGH = id, IsDeleted = true });
            else
                p.IsDeleted = true;

            SavePatches(patches);
            return Json(new { success = true });
        }

        // ============================
        // XÓA NHIỀU ITEM
        // ============================
        [HttpPost]
        public ActionResult DeleteSelected(int[] ids)
        {
            var patches = GetPatches();

            foreach (var id in ids)
            {
                var p = patches.FirstOrDefault(x => x.MaCTGH == id);
                if (p == null)
                    patches.Add(new CartPatch { MaCTGH = id, IsDeleted = true });
                else
                    p.IsDeleted = true;
            }

            SavePatches(patches);
            return Json(new { success = true });
        }

        // ============================
        // CHUYỂN QUA CHECKOUT
        // ============================
        [HttpPost]
        public ActionResult Checkout(string chon)
        {
            if (string.IsNullOrEmpty(chon))
            {
                TempData["Error"] = "Bạn chưa chọn sản phẩm nào.";
                return RedirectToAction("Index");
            }

            TempData["SelectedIds"] = chon;
            return RedirectToAction("CheckoutConfirm");
        }

        // ============================
        // HIỂN THỊ CHECKOUT
        // ============================
        public ActionResult CheckoutConfirm()
        {
            if (TempData["SelectedIds"] == null)
                return RedirectToAction("Index");

            var ids = TempData["SelectedIds"].ToString()
                        .Split(',')
                        .Select(int.Parse)
                        .ToList();

            TempData.Keep("SelectedIds");

            int userId = GetUserId();
            var user = db.NguoiDungs.Find(userId);

            var dbItems = db.ChiTietGioHangs.Where(c => ids.Contains(c.MaCTGH)).ToList();
            var patches = GetPatches();

            var items = dbItems.Select(i =>
            {
                var patch = patches.FirstOrDefault(p => p.MaCTGH == i.MaCTGH);

                int maBienThe = patch?.NewBienThe ?? i.MaBienThe;
                int soLuong = patch?.NewSoLuong ?? i.SoLuong;

                var variant = db.BienTheSanPhams.Find(maBienThe);

                return new CheckoutItemVM
                {
                    MaCTGH = i.MaCTGH,
                    TenSanPham = variant.SanPham.TenSanPham,
                    PhanLoai = variant.TenBienThe,
                    DonGia = variant.GiaBan ,
                    SoLuong = soLuong,
                    HinhAnh = variant.HinhAnh
                };
            })
            .ToList();

            decimal fee = 38000m;

            var model = new CheckoutViewModel
            {
                TenNguoiNhan = user.HoVaTen,
                SDT = user.SDT,
                DiaChi = "",  // Vì bảng NguoiDung KHÔNG có DiaChi

                DanhSachVoucher = db.GiamGias.ToList(),
                Items = items,

                TongTienHang = items.Sum(x => x.ThanhTien),
                PhiVanChuyen = fee,
                GiamGia = 0
            };

            model.TongThanhToan = model.TongTienHang + model.PhiVanChuyen;

            return View("Checkout", model);
        }

        // ============================
        // ÁP DỤNG VOUCHER
        // ============================
        [HttpPost]
        public ActionResult ApplyVoucher(string maVoucher, decimal tongTienHang)
        {
            decimal giamGia = 0m;
            decimal phiShip = 38000m;
            decimal tongThanhToan = 0m;

            // ================================
            // 1) Không chọn voucher
            // ================================
            if (string.IsNullOrEmpty(maVoucher))
            {
                tongThanhToan = tongTienHang + phiShip;

                return Json(new
                {
                    success = true,
                    giamGia = 0,
                    tongThanhToan = tongThanhToan
                });
            }

            // ================================
            // 2) Lấy thông tin voucher
            // ================================
            var vc = db.GiamGias.Find(int.Parse(maVoucher));
            if (vc == null)
                return Json(new { success = false });

            // ================================
            // 3) Kiểm tra điều kiện đơn tối thiểu
            // ================================
            if (tongTienHang < vc.GiaTriDonHangToiThieu)
            {
                tongThanhToan = tongTienHang + phiShip;

                return Json(new
                {
                    success = true,
                    giamGia = 0,
                    tongThanhToan = tongThanhToan
                });
            }

            // ================================
            // 4) Tính giảm giá
            // ================================
            if (vc.GiaTriGiam < 1) // giảm theo %
            {
                giamGia = tongTienHang * (decimal)vc.GiaTriGiam;

                if (giamGia > vc.GiaTriGiamToiDa)
                    giamGia = vc.GiaTriGiamToiDa;
            }
            else
            {
                giamGia = (decimal)vc.GiaTriGiam;
            }

            // ================================
            // 5) Tính tổng thanh toán
            // ================================
            tongThanhToan = tongTienHang + phiShip - giamGia;

            return Json(new
            {
                success = true,
                giamGia = giamGia,
                tongThanhToan = tongThanhToan
            });
        }


        [HttpPost]
        public ActionResult OrderSuccess(string maVoucher)
        {
            // ===== 0) Kiểm tra TempData =====
            if (TempData["SelectedIds"] == null)
                return RedirectToAction("Index", "GioHang");

            var ids = TempData["SelectedIds"].ToString()
                            .Split(',')
                            .Select(int.Parse)
                            .ToList();

            int userId = GetUserId();
            var user = db.NguoiDungs.Find(userId);

            var cartItems = db.ChiTietGioHangs
                                .Where(c => ids.Contains(c.MaCTGH))
                                .ToList();

            // ===== 1) Tính tiền =====
            decimal phiShip = 38000;
            decimal tongTienHang = cartItems.Sum(x => x.BienTheSanPham.GiaBan * x.SoLuong);
            decimal giamGia = 0;
            int? maGG = null;

            if (!string.IsNullOrEmpty(maVoucher))
            {
                var vc = db.GiamGias.Find(int.Parse(maVoucher));

                if (vc != null && tongTienHang >= vc.GiaTriDonHangToiThieu)
                {
                    if (vc.GiaTriGiam < 1) // giảm theo %
                    {
                        giamGia = tongTienHang * (decimal)vc.GiaTriGiam;
                        if (giamGia > vc.GiaTriGiamToiDa)
                            giamGia = vc.GiaTriGiamToiDa;
                    }
                    else // giảm tiền trực tiếp
                    {
                        giamGia = (decimal)vc.GiaTriGiam;
                    }

                    maGG = vc.MaGiamGia;
                }
            }

            decimal tongThanhToan = tongTienHang + phiShip - giamGia;


            // ===== 2) Tạo đơn hàng =====
            DonHang dh = new DonHang
            {
                MaNguoiDung = user.MaNguoiDung,
                MaDVVC = 1,

                TongTien = tongTienHang,
                TrangThaiVanChuyen = "Chưa giao",
                TrangThaiDonHang = "Đang xử lý",
                PhiVanChuyen = phiShip,

                TenNguoiNhan = user.HoVaTen,
                DiaChi = "Không có địa chỉ",
                SDT = user.SDT,

                GhiChu = "",
                TrangThaiThanhToan = false,
                ThoiGianDat = DateTime.Now,
                MaGiamGia = maGG
            };

            db.DonHangs.Add(dh);
            db.SaveChanges();  // => sinh MaDonHang


            // ===== 3) Tạo chi tiết đơn hàng — để DB tự sinh MaCTDH =====
            // ===== 3) Chỉ XÓA GIỎ HÀNG KHÔNG LƯU CHI TIẾT =====
            foreach (var item in cartItems)
            {
                // XÓA khỏi bảng ChiTietGioHang
                db.ChiTietGioHangs.Remove(item);
            }

            // Lưu thay đổi duy nhất: xóa giỏ hàng
            db.SaveChanges();

            return RedirectToAction("OrderSuccess", new { id = dh.MaDonHang });

        }



        [HttpGet]
        public ActionResult OrderSuccess(int id)
        {
            var don = db.DonHangs.Find(id);
            if (don == null) return HttpNotFound();

            return View(don);
        }


        public ActionResult AddToCart(int maBienThe)
        {
            int userId = 1; // test

            var gio = db.GioHangs.FirstOrDefault(g => g.MaNguoiDung == userId);

            if (gio == null)
            {
                gio = new GioHang { MaNguoiDung = userId };
                db.GioHangs.Add(gio);
                db.SaveChanges();
            }

            var ct = db.ChiTietGioHangs
                        .FirstOrDefault(c => c.MaGioHang == gio.MaGioHang
                                          && c.MaBienThe == maBienThe);

            if (ct == null)
            {
                ct = new ChiTietGioHang
                {
                    MaGioHang = gio.MaGioHang,
                    MaBienThe = maBienThe,
                    SoLuong = 1
                };
                db.ChiTietGioHangs.Add(ct);
            }
            else
            {
                ct.SoLuong++;
            }

            db.SaveChanges();
            return RedirectToAction("Index", "GioHang");
        }



        public ActionResult MuaNgay(int maBienThe)
        {
            int userId = GetUserId();

            // Lấy giỏ của user
            var gio = db.GioHangs.FirstOrDefault(g => g.MaNguoiDung == userId);
            if (gio == null)
            {
                gio = new GioHang { MaNguoiDung = userId };
                db.GioHangs.Add(gio);
                db.SaveChanges();
            }

            // Kiểm tra biến thể
            var bt = db.BienTheSanPhams.Find(maBienThe);
            if (bt == null)
                return RedirectToAction("Index", "SanPham");

            // Lấy dòng giỏ hàng
            var item = db.ChiTietGioHangs
                         .FirstOrDefault(c => c.MaGioHang == gio.MaGioHang &&
                                              c.MaBienThe == maBienThe);

            if (item == null)
            {
                item = new ChiTietGioHang
                {
                    MaGioHang = gio.MaGioHang,
                    MaBienThe = maBienThe,
                    SoLuong = 1
                };
                db.ChiTietGioHangs.Add(item);
            }
            else
            {
                item.SoLuong += 1;
            }

            db.SaveChanges();

            // → Mua ngay = chuyển đến giỏ hàng
            return RedirectToAction("Index", "GioHang");
        }

    }
}
