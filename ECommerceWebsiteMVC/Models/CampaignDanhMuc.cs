namespace ECommerceWebsiteMVC.Models
{
    public partial class CampaignDanhMuc
    {
        public int MaCampaign { get; set; }
        public int MaDanhMuc { get; set; }

        public virtual Campaign Campaign { get; set; }
        public virtual DanhMuc DanhMuc { get; set; }
    }
}

