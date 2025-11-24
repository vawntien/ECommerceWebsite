using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class DanhGiaController : Controller
    {
        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        XuLyAnhAzure pcLuuAnh = new XuLyAnhAzure();
        // GET: DanhGia


        //public ActionResult Index()
        //{
        //    int MaChiTietDonHang = 1;
        //    var ct = db.ChiTietDonHangs
        //               .Include("BienTheSanPham.SanPham.AnhSanPhams")
        //               .FirstOrDefault(x => x.MaCTDH == MaChiTietDonHang);

        //    if (ct == null)
        //        return HttpNotFound();

        //    DanhGiaSanPham model = new DanhGiaSanPham();
        //    model.MaCTDH = MaChiTietDonHang;
        //    model.ChiTietDonHang = ct;  

        //    return View(model);
        //}

        //themdonhang

        public ActionResult Index(int MaChiTietDonHang)
        {
            // 1. Load ChiTietDonHang + sản phẩm + ảnh sản phẩm
            var ct = db.ChiTietDonHangs
                       .Include("BienTheSanPham.SanPham.AnhSanPhams")
                       .FirstOrDefault(x => x.MaCTDH == MaChiTietDonHang);

            if (ct == null)
                return HttpNotFound();

            // 2. Kiểm tra CHI TIẾT ĐƠN HÀNG này đã có đánh giá chưa?
            var danhGia = db.DanhGiaSanPhams
                            .FirstOrDefault(x => x.MaCTDH == MaChiTietDonHang);

            if (danhGia != null)
            {
                // ✔ Có đánh giá rồi → gán lại ChiTietDonHang để View dùng
                danhGia.ChiTietDonHang = ct;

                // → trả model đánh giá cũ để user sửa
                return PartialView("_ProductReview", danhGia);
            }

            // 3. CHƯA có đánh giá → tạo model mới
            DanhGiaSanPham model = new DanhGiaSanPham
            {
                MaCTDH = MaChiTietDonHang,
                ChiTietDonHang = ct
            };
            
            return PartialView("_ProductReview", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ThemDanhGia(DanhGiaSanPham model, HttpPostedFileBase HinhAnhFile)
        {
            if (!ModelState.IsValid)
            {
                // Nếu bạn muốn show lại modal, có thể cần load lại ChiTietDonHang ở đây
                // model.ChiTietDonHang = db.ChiTietDonHangs
                //     .Include("BienTheSanPham.SanPham.AnhSanPhams")
                //     .FirstOrDefault(x => x.MaCTDH == model.MaCTDH);

                return PartialView("_ProductReview", model);
            }

            // 🔹 1. Kiểm tra xem MaCTDH này đã có đánh giá chưa
            var danhGia = db.DanhGiaSanPhams.FirstOrDefault(x => x.MaCTDH == model.MaCTDH);

            if (danhGia == null)
            {
                // 👉 CHƯA có đánh giá -> tạo mới
                danhGia = new DanhGiaSanPham
                {
                    MaCTDH = model.MaCTDH
                };
                db.DanhGiaSanPhams.Add(danhGia);
            }

            // 🔹 2. Cập nhật thông tin đánh giá (cả thêm mới và sửa)
            danhGia.SoSao = model.SoSao;
            danhGia.TieuDe = model.TieuDe;
            danhGia.NoiDung = model.NoiDung;
            danhGia.ThoiGian = DateTime.Now;   

            db.SaveChanges(); // đảm bảo danhGia.MaDG có giá trị (cho case thêm mới)

            // 🔹 3. Xử lý upload ảnh (nếu có)
            if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(HinhAnhFile.FileName);   // .jpg, .png,...
                string newName = danhGia.MaDG + ext;                    // dùng MaDG để đặt tên

                try
                {
                    // (Optional) Nếu muốn, bạn có thể xóa ảnh cũ trước:
                    // if (!string.IsNullOrEmpty(danhGia.HinhAnh))
                    // {
                    //     await pcLuuAnh.DeleteReviewImageAsync(danhGia.HinhAnh);
                    // }

                    await pcLuuAnh.UploadReviewImageAsync(HinhAnhFile, newName);

                    // Lưu lại tên ảnh mới vào DB
                    danhGia.HinhAnh = newName;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Lỗi upload ảnh: " + ex.Message;
                }
            }

            return RedirectToAction("Index", "NguoiMua");
        }


    }
}