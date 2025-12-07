using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class ThongKeDoanhThuNguoiBan
    {
        string _TenCuaHang;
        int _DoanhThu;
        int _TongSoDon;


        public string TenCuaHang { get => _TenCuaHang; set => _TenCuaHang = value; }
        public int DoanhThu { get => _DoanhThu; set => _DoanhThu = value; }
        public int TongSoDon { get => _TongSoDon; set => _TongSoDon = value; }
    }
}