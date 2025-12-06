using ECommerceWebsiteMVC_Seller.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class NguoiBanController : Controller
    {
        // GET: NguoiBan

        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();
        public ActionResult Index()
        {
            return View();
        }
        

        public ActionResult TatCaSanPham()
        {
            //csdl
            #region csdl
            //if (Session["MaNguoiBan"] == null)
            //    return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            //int maNguoiBan = (int)Session["MaNguoiBan"];
            //var dsSanPham = db.Database.SqlQuery<SanPham>(
            //    "EXEC sp_GetSanPhamByNguoiBan @MaNguoiBan",
            //    new SqlParameter("@MaNguoiBan", maNguoiBan)
            //).ToList();

            //return View(dsSanPham);
            #endregion

            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");
            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);

            if (cuaHang == null)
                return Content("Bạn chưa tạo cửa hàng.");
            int maCuaHang = cuaHang.MaCuaHang;

            //goi sp de canh bao sap het san pham ton kho

            var warnings = db.Database.SqlQuery<WarningViewModel>("EXEC sp_GetLowStockWarnings").ToList();
            var filteredWarnings = warnings.Where(x => x.MaCuaHang == maCuaHang).ToList();

            ViewBag.Warnings = filteredWarnings;


            //lay san pham thuoc cua hang do
            var dsSanPham = db.SanPhams.Include("AnhSanPhams").Include("BienTheSanPhams").Where(sp => sp.MaCuaHang == maCuaHang).OrderByDescending(sp => sp.MaSanPham).ToList();

            //goi fn de tinh tong ton kho
            var tonk = new Dictionary<int, int>();

            foreach (var sp in dsSanPham)
            {
                int tonKho = db.Database.SqlQuery<int>("SELECT dbo.fn_TongTonKho(@MaSanPham)",new SqlParameter("@MaSanPham", sp.MaSanPham)).FirstOrDefault();
                tonk[sp.MaSanPham] = tonKho;
            }
            ViewBag.TonKho = tonk;


            return View(dsSanPham);
        }

        public ActionResult CapNhatSanPham(int id)
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            int maNguoiBan = (int)Session["MaNguoiBan"];

            // Kiểm tra sản phẩm có thuộc về người bán này không
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
                return Content("Bạn chưa tạo cửa hàng.");

            var sanPham = db.SanPhams
                .Include("AnhSanPhams")
                .Include("BienTheSanPhams")
                .Include("DanhMuc")
                .SingleOrDefault(sp => sp.MaSanPham == id && sp.MaCuaHang == cuaHang.MaCuaHang);

            if (sanPham == null)
                return HttpNotFound("Sản phẩm không tồn tại hoặc không thuộc về bạn.");

            ViewBag.DanhMucs = db.DanhMucs.ToList();

            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatSanPham(SanPham model, FormCollection form)
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
                return Content("Bạn chưa tạo cửa hàng.");

            var sanPham = db.SanPhams
                .Include("BienTheSanPhams")
                .SingleOrDefault(sp => sp.MaSanPham == model.MaSanPham && sp.MaCuaHang == cuaHang.MaCuaHang);

            if (sanPham == null)
                return HttpNotFound("Sản phẩm không tồn tại hoặc không thuộc về bạn.");

            if (!ModelState.IsValid)
            {
                ViewBag.DanhMucs = db.DanhMucs.ToList();
                return View(sanPham);
            }

            sanPham.TenSanPham = model.TenSanPham?.Trim();
            sanPham.MoTa = model.MoTa;
            sanPham.MaDanhMuc = model.MaDanhMuc;

            var variantIds = form.GetValues("VariantIds[]") ?? Array.Empty<string>();
            var variantPrices = form.GetValues("VariantGiaBan[]") ?? Array.Empty<string>();
            var variantStocks = form.GetValues("VariantSoLuong[]") ?? Array.Empty<string>();


            for (int i = 0; i < variantIds.Length; i++)
            {
                if (!int.TryParse(variantIds[i], out int variantId))
                    continue;

                var variant = sanPham.BienTheSanPhams.FirstOrDefault(v => v.MaBienThe == variantId);
                if (variant == null)
                    continue;

                if (i < variantPrices.Length && decimal.TryParse(variantPrices[i], out decimal price))
                {
                    variant.GiaBan = price;
                    db.Entry(variant).Property(v => v.GiaBan).IsModified = true;
                }

                if (i < variantStocks.Length && int.TryParse(variantStocks[i], out int stock))
                {
                    variant.SoLuongTonKho = stock;
                    variant.SoLuongTonKho = stock;
                    db.Entry(variant).State = EntityState.Modified;

                }
            }

            db.Entry(sanPham).State = EntityState.Modified;
            db.SaveChanges();

            TempData["UpdateSuccess"] = "Sản phẩm đã được cập nhật.";
            return RedirectToAction("CapNhatSanPham", new { id = sanPham.MaSanPham });

        }

        public ActionResult ThemSanPham()
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            var viewModel = new CreateProductViewModel
            {
                DanhMucs = GetDanhMucSelectList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ThemSanPham(CreateProductViewModel model)
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);

            if (cuaHang == null)
            {
                ModelState.AddModelError("", "Bạn chưa tạo cửa hàng.");
            }

            var attributeOneValues = Request.Form.GetValues("AttributeOneValues") ?? Array.Empty<string>();
            var variantNames = Request.Form.GetValues("VariantNames") ?? Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(model.AttributeOneName))
            {
                ModelState.AddModelError("AttributeOneName", "Vui lòng nhập tên phân loại 1.");
            }

            if (!attributeOneValues.Any())
            {
                ModelState.AddModelError("AttributeOneValues", "Phân loại 1 cần ít nhất 1 tùy chọn.");
            }

            if (!variantNames.Any())
            {
                ModelState.AddModelError("VariantNames", "Vui lòng cấu hình ít nhất 1 biến thể.");
            }

            // Validate variant images are required
            for (int i = 0; i < variantNames.Length; i++)
            {
                var fileKey = $"VariantImage_{i}";
                var file = Request.Files[fileKey];
                if (file == null || file.ContentLength == 0)
                {
                    ModelState.AddModelError($"VariantImage_{i}", $"Vui lòng tải lên ảnh cho biến thể: {variantNames[i]}");
                }
            }

            var productImageKeys = Request.Files.AllKeys
                .Where(k => !string.IsNullOrEmpty(k) && k.StartsWith("ProductImages", StringComparison.OrdinalIgnoreCase))
                .ToList();

            bool hasProductImage = productImageKeys.Any(k =>
            {
                var file = Request.Files[k];
                return file != null && file.ContentLength > 0;
            });

            if (!hasProductImage)
            {
                ModelState.AddModelError("ProductImages", "Vui lòng tải lên ít nhất 1 hình ảnh sản phẩm.");
            }

            if (!ModelState.IsValid)
            {
                model.DanhMucs = GetDanhMucSelectList();
                return View(model);
            }

            var sanPham = new SanPham
            {
                TenSanPham = model.TenSanPham?.Trim(),
                MaDanhMuc = model.MaDanhMuc ?? 0,
                MaCuaHang = cuaHang.MaCuaHang,
                MoTa = model.MoTa,
                LoaiPhanLoai = string.IsNullOrWhiteSpace(model.AttributeTwoName)
                    ? model.AttributeOneName?.Trim()
                    : $"{model.AttributeOneName?.Trim()}|{model.AttributeTwoName?.Trim()}"
            };

            try
            {
                db.SanPhams.Add(sanPham);
                db.SaveChanges();

                await LuuAnhSanPhamAsync(sanPham.MaSanPham, productImageKeys);
                await LuuBienTheSanPhamAsync(sanPham.MaSanPham);

                TempData["UpdateSuccess"] = "Sản phẩm mới đã được tạo.";
                return RedirectToAction("CapNhatSanPham", new { id = sanPham.MaSanPham });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, xóa sản phẩm đã tạo (nếu có)
                if (sanPham.MaSanPham > 0)
                {
                    var productToDelete = db.SanPhams.Find(sanPham.MaSanPham);
                    if (productToDelete != null)
                    {
                        db.SanPhams.Remove(productToDelete);
                        db.SaveChanges();
                    }
                }

                ModelState.AddModelError("", $"Lỗi khi tạo sản phẩm: {ex.Message}");
                model.DanhMucs = GetDanhMucSelectList();
                return View(model);
            }
        }

        private List<DanhMuc> GetDanhMucSelectList()
        {
            return db.DanhMucs
                .OrderBy(dm => dm.TenDanhMuc)
                .ToList();
        }

        private async Task LuuAnhSanPhamAsync(int maSanPham, List<string> productImageKeys)
        {
            var azureHelper = new XuLyAnhAzure();
            int.TryParse(Request.Form["DefaultImageIndex"], out int defaultIndex);

            int currentIndex = 0;
            foreach (var key in productImageKeys)
            {
                var file = Request.Files[key];
                if (file == null || file.ContentLength == 0)
                    continue;

                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                await azureHelper.UploadProductImageAsync(file, maSanPham.ToString(), fileName);

                db.AnhSanPhams.Add(new AnhSanPham
                {
                    MaSanPham = maSanPham,
                    HinhAnh = fileName,
                    MacDinh = currentIndex == defaultIndex
                });

                currentIndex++;
            }

            db.SaveChanges();
        }

        private async Task LuuBienTheSanPhamAsync(int maSanPham)
        {
            var azureHelper = new XuLyAnhAzure();
            var variantNames = Request.Form.GetValues("VariantNames") ?? Array.Empty<string>();
            var variantPrices = Request.Form.GetValues("VariantPrices") ?? Array.Empty<string>();
            var variantStocks = Request.Form.GetValues("VariantStocks") ?? Array.Empty<string>();
            var variantSkus = Request.Form.GetValues("VariantSkus") ?? Array.Empty<string>();

            for (int i = 0; i < variantNames.Length; i++)
            {
                var variant = new BienTheSanPham
                {
                    MaSanPham = maSanPham,
                    TenBienThe = variantNames[i],
                    GiaBan = ParseDecimalSafe(variantPrices, i),
                    SoLuongTonKho = ParseIntSafe(variantStocks, i)
                };

                // Ảnh biến thể là bắt buộc
                var fileKey = $"VariantImage_{i}";
                var file = Request.Files[fileKey];
                if (file == null || file.ContentLength == 0)
                {
                    throw new Exception($"Ảnh biến thể là bắt buộc cho: {variantNames[i]}");
                }

                // Upload ảnh biến thể vào Azure (cùng folder với ảnh sản phẩm: products/{maSanPham}/)
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"variant_{Guid.NewGuid()}{fileExtension}";
                await azureHelper.UploadProductImageAsync(file, maSanPham.ToString(), fileName);
                variant.HinhAnh = fileName;

                db.BienTheSanPhams.Add(variant);
            }

            db.SaveChanges();
        }

        private decimal ParseDecimalSafe(string[] sources, int index)
        {
            if (sources.Length > index && decimal.TryParse(sources[index], NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return 0;
        }

        private int ParseIntSafe(string[] sources, int index)
        {
            if (sources.Length > index && int.TryParse(sources[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return 0;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> XoaSanPham(int id)
        {
            if (Session["MaNguoiBan"] == null)
                return RedirectToAction("DangNhapNguoiBan", "TaiKhoan");

            int maNguoiBan = (int)Session["MaNguoiBan"];
            var cuaHang = db.CuaHangs.SingleOrDefault(x => x.MaNguoiBan == maNguoiBan);
            if (cuaHang == null)
                return Content("Bạn chưa tạo cửa hàng.");

            var sanPham = db.SanPhams
                .Include("AnhSanPhams")
                .Include("BienTheSanPhams")
                .SingleOrDefault(sp => sp.MaSanPham == id && sp.MaCuaHang == cuaHang.MaCuaHang);

            if (sanPham == null)
                return HttpNotFound("Sản phẩm không tồn tại hoặc không thuộc về bạn.");

            try
            {
                var azureHelper = new XuLyAnhAzure();
                await azureHelper.DeleteProductFolderAsync(sanPham.MaSanPham.ToString());

                if (sanPham.AnhSanPhams?.Any() == true)
                {
                    db.AnhSanPhams.RemoveRange(sanPham.AnhSanPhams.ToList());
                }

                if (sanPham.BienTheSanPhams?.Any() == true)
                {
                    db.BienTheSanPhams.RemoveRange(sanPham.BienTheSanPhams.ToList());
                }

                db.SanPhams.Remove(sanPham);
                db.SaveChanges();

                TempData["UpdateSuccess"] = "Đã xóa sản phẩm.";
            }
            catch (Exception ex)
            {
                TempData["UpdateSuccess"] = $"Không thể xóa sản phẩm: {ex.Message}";
            }

            return RedirectToAction("TatCaSanPham");
        }

    }
}