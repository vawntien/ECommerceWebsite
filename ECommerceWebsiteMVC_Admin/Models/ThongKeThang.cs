using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC_Admin.Models
{
    public class ThongKeThang
    {
        string _Thang;
        int _TongTien;
        public ThongKeThang(){ }

        public string Thang { get => _Thang; set => _Thang = value; }
        public int TongTien { get => _TongTien; set => _TongTien = value; }
    }
}