using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Seller.Models
{
    public class XemDanhGiaSanPhamViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public List<DanhGiaSanPham> DanhGias { get; set; }
    }
}