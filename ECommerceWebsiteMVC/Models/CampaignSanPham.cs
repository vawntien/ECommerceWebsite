namespace ECommerceWebsiteMVC.Models
{
    public partial class CampaignSanPham
    {
        public int MaCampaign { get; set; }
        public int MaSanPham { get; set; }

        public virtual Campaign Campaign { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}

