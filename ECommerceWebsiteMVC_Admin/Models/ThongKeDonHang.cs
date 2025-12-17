using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class ThongKeDonHang
    {
        string _Thang;
        int _SoLuong;

        public ThongKeDonHang() { }

        public string Thang { get => _Thang; set => _Thang = value; }
        public int SoLuong { get => _SoLuong; set => _SoLuong = value; }
    }
}