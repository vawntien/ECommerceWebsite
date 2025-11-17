using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class NguoiMuaController : Controller
    {
        QLBanHang_SPEntities1 ql = new QLBanHang_SPEntities1();
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.DanhMuc = ql.DanhMucs.ToList();
            List<SanPham> dssp = ql.SanPhams.Include("AnhSanPhams").ToList();
            return View(dssp);
        }

        public ActionResult DonHang()
        {
           
            return View();
        }
        public ActionResult ChiTietDonHang()
        {
            return View();
        }
    }
}