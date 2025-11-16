using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ECommerceWebsiteMVC.Models;

namespace ECommerceWebsiteMVC.Controllers
{
    public class GioHangController : Controller
    {
        QLBanHangEntities db = new QLBanHangEntities();

        // HÀM LẤY USER (KHÔNG CẦN LOGIN) — fallback dùng user đầu DB
        private int GetUserId()
        {
            if (Session["MaNguoiMua"] != null)
                return (int)Session["MaNguoiMua"];

            // fallback để không làm ảnh hưởng phần đăng nhập người khác
            return db.NguoiMuas.OrderBy(u => u.MaNguoiMua).First().MaNguoiMua;
        }
        //session patch get+ save
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

        // HIỂN THỊ GIỎ HÀNG
        public ActionResult Index()
        {
            int userId = GetUserId();

            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaNguoiMua == userId);
            if (gioHang == null)
            {
                gioHang = new GioHang { MaNguoiMua = userId, SoLuong = 0 };
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
                    int soLuong = patch?.NewSoLuong ?? dbItem.SoLuong ?? 1;

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
                        DonGia = variant.GiaBan ?? 0,
                        SoLuong = soLuong,
                        BienTheList = db.BienTheSanPhams
                            .Where(bt => bt.MaSanPham == variant.MaSanPham)
                            .Select(bt => new BienTheSanPhamViewModel
                            {
                                MaBienThe = bt.MaBienThe,
                                TenBienThe = bt.TenBienThe,
                                GiaBan = bt.GiaBan ?? 0,
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

        // CẬP NHẬT SỐ LƯỢNG (SESSION)
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

        
        //CẬP NHẬT BIẾN THỂ (POPUP)
        
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

        
        // XÓA MỘT ITEM (SESSION)
        
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

        
        // XÓA NHIỀU ITEM
        
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

        
        // CHECKOUT → CHỌN SẢN PHẨM THANH TOÁN
        
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

        
        // CHECKOUT CONFIRM (HIỂN THỊ)
        
        public ActionResult CheckoutConfirm()
        {
            if (TempData["SelectedIds"] == null)
                return RedirectToAction("Index");

            var ids = TempData["SelectedIds"].ToString().Split(',').Select(int.Parse).ToList();
            TempData.Keep("SelectedIds");

            int userId = GetUserId();
            var user = db.NguoiMuas.Find(userId);

            var dbItems = db.ChiTietGioHangs.Where(c => ids.Contains(c.MaCTGH)).ToList();
            var patches = GetPatches();

            var items = dbItems.Select(i =>
            {
                var patch = patches.FirstOrDefault(p => p.MaCTGH == i.MaCTGH);

                int maBienThe = patch?.NewBienThe ?? i.MaBienThe;
                int soLuong = patch?.NewSoLuong ?? i.SoLuong ?? 1;

                var variant = db.BienTheSanPhams.Find(maBienThe);

                return new CheckoutItemVM
                {
                    MaCTGH = i.MaCTGH,
                    TenSanPham = variant.SanPham.TenSanPham,
                    PhanLoai = variant.TenBienThe,
                    DonGia = variant.GiaBan ?? 0,
                    SoLuong = soLuong,
                    HinhAnh = variant.HinhAnh
                };
            })
            .ToList();

            decimal fee = 38000; // phí ship tạm

            var model = new CheckoutViewModel
            {
                TenNguoiNhan = user.HoVaTen,
                SDT = user.SDT,
                DiaChi = user.DiaChi,
                DanhSachVoucher = db.GiamGias.ToList(),
                Items = items,
                TongTienHang = items.Sum(x => x.ThanhTien),
                PhiVanChuyen = fee,
                GiamGia = 0
            };

            model.TongThanhToan = model.TongTienHang + model.PhiVanChuyen;

            return View("Checkout", model);
        }

        [HttpPost]
        public ActionResult ApplyVoucher(string maVoucher, decimal tongTienHang)
        {
            decimal giamGia = 0;

            if (!string.IsNullOrEmpty(maVoucher))
            {
                var vc = db.GiamGias.Find(int.Parse(maVoucher));

                if (vc != null)
                {
                    if (tongTienHang >= (decimal)(vc.GiaTriDonHangToiThieu ?? 0))
                        giamGia = (decimal)(vc.GiaTriGiam ?? 0);
                }
            }

            decimal phiShip = 38000; // cố định
            decimal tongThanhToan = tongTienHang + phiShip - giamGia;

            return Json(new
            {
                success = true,
                giamGia = giamGia,
                tongThanhToan = tongThanhToan
            });
        }

        public ActionResult OrderSuccess()
        {
            return View();
        }

    }
}
