using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Seller.Models
{
    public class WarningViewModel
    {
        public int MaBienThe { get; set; }
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int TonKho { get; set; }
        public int MaCuaHang { get; set; }
        public string ThongBao { get; set; }
    }
}