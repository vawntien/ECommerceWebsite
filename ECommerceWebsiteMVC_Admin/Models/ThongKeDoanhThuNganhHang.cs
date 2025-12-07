using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class ThongKeDoanhThuNganhHang
    {
        string _TenNganhHang;
        int _SoLuongBan;
        int _DoanhThu;

        public ThongKeDoanhThuNganhHang() { }

        public string TenNganhHang { get => _TenNganhHang; set => _TenNganhHang = value; }
        public int SoLuongBan { get => _SoLuongBan; set => _SoLuongBan = value; }
        public int DoanhThu { get => _DoanhThu; set => _DoanhThu = value; }
    }
}