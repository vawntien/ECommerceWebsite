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
        QLBanHang_SPEntities ql = new QLBanHang_SPEntities();
        // GET: Home
        public ActionResult Index()
        {
            return View(ql.SanPhams.Include("AnhSanPhams").ToList());
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