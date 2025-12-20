using System;
using System.Collections.Generic;
using System.Linq;

namespace ECommerceWebsiteMVC.Models
{
    public static class CampaignHelper
    {
        // Prefix để đánh dấu Campaign trong bảng GiamGia
        private const string CAMPAIGN_PREFIX = "[CAMPAIGN]";
        private const string SEPARATOR = "|||";

        /// <summary>
        /// Tính giá sau khi áp dụng campaign cho một sản phẩm
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="maSanPham">Mã sản phẩm</param>
        /// <param name="giaGoc">Giá gốc của sản phẩm</param>
        /// <returns>Giá sau khi giảm và thông tin campaign</returns>
        public static CampaignPriceResult TinhGiaSauGiam(ECommerceWebsiteEntities db, int maSanPham, decimal giaGoc)
        {
            var now = DateTime.Now;

            // Lấy thông tin sản phẩm để biết danh mục
            var sanPham = db.SanPhams.Find(maSanPham);
            if (sanPham == null)
            {
                return new CampaignPriceResult
                {
                    GiaGoc = giaGoc,
                    GiaSauGiam = giaGoc,
                    CoGiamGia = false,
                    PhanTramGiam = 0,
                    TenCampaign = null
                };
            }

            try
            {
                // Tìm campaign đang hoạt động từ bảng GiamGia (có prefix [CAMPAIGN])
                var campaign = db.GiamGias
                    .Where(g => g.TenMaGG != null && g.TenMaGG.StartsWith(CAMPAIGN_PREFIX)
                        && (g.NgayBD == null || g.NgayBD <= now)
                        && (g.NgayKT == null || g.NgayKT >= now))
                    .OrderByDescending(g => g.NgayBD ?? DateTime.MinValue)
                    .ToList()
                    .Select(g => new
                    {
                        GiamGia = g,
                        Data = ParseCampaignData(g.MoTa)
                    })
                    .Where(x => x.Data != null)
                    .FirstOrDefault(x => 
                        // Kiểm tra sản phẩm có trong danh sách không
                        (x.Data.DanhSachSanPham != null && x.Data.DanhSachSanPham.Contains(maSanPham)) ||
                        // Hoặc danh mục có trong danh sách không
                        (x.Data.DanhSachDanhMuc != null && x.Data.DanhSachDanhMuc.Contains(sanPham.MaDanhMuc))
                    );

                if (campaign == null || campaign.Data == null)
                {
                    return new CampaignPriceResult
                    {
                        GiaGoc = giaGoc,
                        GiaSauGiam = giaGoc,
                        CoGiamGia = false,
                        PhanTramGiam = 0,
                        TenCampaign = null
                    };
                }

                decimal giaSauGiam = giaGoc;
                decimal phanTramGiam = 0;
                string tenCampaign = campaign.GiamGia.TenMaGG.Replace(CAMPAIGN_PREFIX, "").Trim();

                if (campaign.Data.LoaiGiamGia == "PhanTram")
                {
                    // Giảm theo phần trăm
                    phanTramGiam = campaign.Data.GiaTriGiam;
                    giaSauGiam = giaGoc * (1 - phanTramGiam / 100);
                }
                else if (campaign.Data.LoaiGiamGia == "SoTien")
                {
                    // Giảm theo số tiền cố định
                    giaSauGiam = giaGoc - campaign.Data.GiaTriGiam;
                    if (giaSauGiam < 0) giaSauGiam = 0;
                    phanTramGiam = (giaGoc > 0) ? (giaGoc - giaSauGiam) / giaGoc * 100 : 0;
                }

                return new CampaignPriceResult
                {
                    GiaGoc = giaGoc,
                    GiaSauGiam = giaSauGiam,
                    CoGiamGia = true,
                    PhanTramGiam = phanTramGiam,
                    TenCampaign = tenCampaign
                };
            }
            catch
            {
                // Nếu có lỗi, trả về giá gốc
                return new CampaignPriceResult
                {
                    GiaGoc = giaGoc,
                    GiaSauGiam = giaGoc,
                    CoGiamGia = false,
                    PhanTramGiam = 0,
                    TenCampaign = null
                };
            }
        }

        /// <summary>
        /// Tính giá sau khi áp dụng campaign cho một biến thể sản phẩm
        /// </summary>
        public static CampaignPriceResult TinhGiaSauGiamChoBienThe(ECommerceWebsiteEntities db, int maBienThe, decimal giaGoc)
        {
            var bienThe = db.BienTheSanPhams.Find(maBienThe);
            if (bienThe == null)
            {
                return new CampaignPriceResult
                {
                    GiaGoc = giaGoc,
                    GiaSauGiam = giaGoc,
                    CoGiamGia = false,
                    PhanTramGiam = 0,
                    TenCampaign = null
                };
            }

            return TinhGiaSauGiam(db, bienThe.MaSanPham, giaGoc);
        }

        // Helper method để parse dữ liệu Campaign từ MoTa
        private static CampaignData ParseCampaignData(string moTa)
        {
            if (string.IsNullOrEmpty(moTa)) return null;
            try
            {
                // Format: "LoaiGiamGia|GiaTriGiam|MoTaChiTiet|DanhSachSanPham|DanhSachDanhMuc"
                var parts = moTa.Split(new[] { SEPARATOR }, StringSplitOptions.None);
                if (parts.Length < 3) return null;

                string loaiGiamGia = parts[0];
                decimal giaTriGiam;
                if (!decimal.TryParse(parts[1], out giaTriGiam)) return null;

                List<int> danhSachSanPham = new List<int>();
                List<int> danhSachDanhMuc = new List<int>();

                if (parts.Length > 3 && !string.IsNullOrEmpty(parts[3]))
                {
                    danhSachSanPham = parts[3].Split(',').Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => { int.TryParse(s.Trim(), out int id); return id; })
                        .Where(id => id > 0).ToList();
                }

                if (parts.Length > 4 && !string.IsNullOrEmpty(parts[4]))
                {
                    danhSachDanhMuc = parts[4].Split(',').Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => { int.TryParse(s.Trim(), out int id); return id; })
                        .Where(id => id > 0).ToList();
                }

                return new CampaignData
                {
                    LoaiGiamGia = loaiGiamGia,
                    GiaTriGiam = giaTriGiam,
                    DanhSachSanPham = danhSachSanPham,
                    DanhSachDanhMuc = danhSachDanhMuc
                };
            }
            catch
            {
                return null;
            }
        }
    }

    // Class để lưu dữ liệu Campaign
    internal class CampaignData
    {
        public string LoaiGiamGia { get; set; }
        public decimal GiaTriGiam { get; set; }
        public List<int> DanhSachSanPham { get; set; }
        public List<int> DanhSachDanhMuc { get; set; }
    }

    public class CampaignPriceResult
    {
        public decimal GiaGoc { get; set; }
        public decimal GiaSauGiam { get; set; }
        public bool CoGiamGia { get; set; }
        public decimal PhanTramGiam { get; set; }
        public string TenCampaign { get; set; }
    }
}
