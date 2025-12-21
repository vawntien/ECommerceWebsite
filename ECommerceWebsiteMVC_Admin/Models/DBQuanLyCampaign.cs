using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class DBQuanLyCampaign
    {
        ECommerceWebsiteEntities db;

        // Prefix để đánh dấu Campaign trong bảng GiamGia
        private const string CAMPAIGN_PREFIX = "[CAMPAIGN]";
        private const string SEPARATOR = "|||";

        public DBQuanLyCampaign()
        {
            db = new ECommerceWebsiteEntities();
        }

        public List<CampaignInfo> DanhSachCampaign()
        {
            // Lấy tất cả GiamGia có TenMaGG bắt đầu bằng CAMPAIGN_PREFIX
            var campaigns = db.GiamGias
                .Where(g => g.TenMaGG != null && g.TenMaGG.StartsWith(CAMPAIGN_PREFIX))
                .OrderByDescending(g => g.NgayBD ?? DateTime.MinValue)
                .ToList();

            var results = new List<CampaignInfo>();
            foreach (var gg in campaigns)
            {
                try
                {
                    var campaignData = ParseCampaignFromGiamGia(gg);
                    if (campaignData != null)
                    {
                        results.Add(campaignData);
                    }
                }
                catch
                {
                    continue;
                }
            }
            return results;
        }

        public CampaignInfo LayCampaignTheoMa(int maCampaign)
        {
            try
            {
                var gg = db.GiamGias.FirstOrDefault(g => g.MaGiamGia == maCampaign 
                    && g.TenMaGG != null && g.TenMaGG.StartsWith(CAMPAIGN_PREFIX));
                
                if (gg == null) return null;

                var result = ParseCampaignFromGiamGia(gg);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private CampaignInfo ParseCampaignFromGiamGia(GiamGia gg)
        {
            if (gg == null || string.IsNullOrEmpty(gg.MoTa) || string.IsNullOrEmpty(gg.TenMaGG)) 
                return null;

            try
            {
                // MoTa format: "LoaiGiamGia|GiaTriGiam|MoTaChiTiet|DanhSachSanPham|DanhSachDanhMuc"
                // Ví dụ: "PhanTram|20|Flash Sale Black Friday|1,2,3|4,5"
                var parts = gg.MoTa.Split(new[] { SEPARATOR }, StringSplitOptions.None);
                if (parts.Length < 3) return null;

                string loaiGiamGia = parts[0] ?? "PhanTram";
                decimal giaTriGiam;
                if (!decimal.TryParse(parts[1], out giaTriGiam)) return null;

                string moTaChiTiet = parts.Length > 2 ? (parts[2] ?? "") : "";
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

                string tenCampaign = gg.TenMaGG.Replace(CAMPAIGN_PREFIX, "").Trim();

                return new CampaignInfo
                {
                    MaCampaign = gg.MaGiamGia,
                    TenCampaign = tenCampaign,
                    MoTa = moTaChiTiet,
                    LoaiCampaign = "Chủ động",
                    LoaiGiamGia = loaiGiamGia,
                    GiaTriGiam = giaTriGiam,
                    NgayBD = gg.NgayBD,
                    NgayKT = gg.NgayKT,
                    TrangThai = true,
                    NgayTao = gg.NgayBD ?? DateTime.Now,
                    NguoiTao = null,
                    SoSanPham = danhSachSanPham.Count,
                    SoDanhMuc = danhSachDanhMuc.Count
                };
            }
            catch
            {
                return null;
            }
        }

        public bool ThemCampaign(CampaignInfo campaign, List<int> danhSachSanPham, List<int> danhSachDanhMuc)
        {
            try
            {
                // Format MoTa: "LoaiGiamGia|GiaTriGiam|MoTaChiTiet|DanhSachSanPham|DanhSachDanhMuc"
                string danhSachSP = danhSachSanPham != null && danhSachSanPham.Any() 
                    ? string.Join(",", danhSachSanPham) 
                    : "";
                string danhSachDM = danhSachDanhMuc != null && danhSachDanhMuc.Any() 
                    ? string.Join(",", danhSachDanhMuc) 
                    : "";

                string moTa = string.Join(SEPARATOR, 
                    campaign.LoaiGiamGia ?? "PhanTram",
                    campaign.GiaTriGiam.ToString(),
                    campaign.MoTa ?? "",
                    danhSachSP,
                    danhSachDM);

                var gg = new GiamGia
                {
                    TenMaGG = CAMPAIGN_PREFIX + campaign.TenCampaign,
                    MoTa = moTa,
                    GiaTriDonHangToiThieu = 0, // Campaign không cần giá trị đơn tối thiểu
                    GiaTriGiamToiDa = 0, // Không dùng cho Campaign
                    GiaTriGiam = (double)campaign.GiaTriGiam,
                    NgayBD = campaign.NgayBD,
                    NgayKT = campaign.NgayKT
                };

                db.GiamGias.Add(gg);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SuaCampaign(CampaignInfo campaign, List<int> danhSachSanPham, List<int> danhSachDanhMuc)
        {
            try
            {
                var gg = db.GiamGias.Find(campaign.MaCampaign);
                if (gg == null || !gg.TenMaGG.StartsWith(CAMPAIGN_PREFIX)) return false;

                string danhSachSP = danhSachSanPham != null && danhSachSanPham.Any() 
                    ? string.Join(",", danhSachSanPham) 
                    : "";
                string danhSachDM = danhSachDanhMuc != null && danhSachDanhMuc.Any() 
                    ? string.Join(",", danhSachDanhMuc) 
                    : "";

                string moTa = string.Join(SEPARATOR, 
                    campaign.LoaiGiamGia ?? "PhanTram",
                    campaign.GiaTriGiam.ToString(),
                    campaign.MoTa ?? "",
                    danhSachSP,
                    danhSachDM);

                gg.TenMaGG = CAMPAIGN_PREFIX + campaign.TenCampaign;
                gg.MoTa = moTa;
                gg.GiaTriGiam = (double)campaign.GiaTriGiam;
                gg.NgayBD = campaign.NgayBD;
                gg.NgayKT = campaign.NgayKT;

                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool XoaCampaign(int maCampaign)
        {
            try
            {
                var gg = db.GiamGias.Find(maCampaign);
                if (gg == null || !gg.TenMaGG.StartsWith(CAMPAIGN_PREFIX)) return false;

                // Chỉ xóa nếu chưa có đơn hàng sử dụng
                if (gg.DonHangs != null && gg.DonHangs.Any()) return false;

                db.GiamGias.Remove(gg);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<int> LayDanhSachSanPham(int maCampaign)
        {
            try
            {
                var gg = db.GiamGias.Find(maCampaign);
                if (gg == null || string.IsNullOrEmpty(gg.MoTa)) return new List<int>();

                var parts = gg.MoTa.Split(new[] { SEPARATOR }, StringSplitOptions.None);
                if (parts.Length > 3 && !string.IsNullOrEmpty(parts[3]))
                {
                    return parts[3].Split(',').Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => { int.TryParse(s.Trim(), out int id); return id; })
                        .Where(id => id > 0).ToList();
                }
            }
            catch { }
            return new List<int>();
        }

        public List<int> LayDanhSachDanhMuc(int maCampaign)
        {
            try
            {
                var gg = db.GiamGias.Find(maCampaign);
                if (gg == null || string.IsNullOrEmpty(gg.MoTa)) return new List<int>();

                var parts = gg.MoTa.Split(new[] { SEPARATOR }, StringSplitOptions.None);
                if (parts.Length > 4 && !string.IsNullOrEmpty(parts[4]))
                {
                    return parts[4].Split(',').Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => { int.TryParse(s.Trim(), out int id); return id; })
                        .Where(id => id > 0).ToList();
                }
            }
            catch { }
            return new List<int>();
        }
    }

    public class CampaignInfo
    {
        public int MaCampaign { get; set; }
        public string TenCampaign { get; set; }
        public string MoTa { get; set; }
        public string LoaiCampaign { get; set; }
        public string LoaiGiamGia { get; set; }
        public decimal GiaTriGiam { get; set; }
        public DateTime? NgayBD { get; set; }
        public DateTime? NgayKT { get; set; }
        public bool TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public int SoSanPham { get; set; }
        public int SoDanhMuc { get; set; }
    }
}
