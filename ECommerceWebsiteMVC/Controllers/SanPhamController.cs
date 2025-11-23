using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class SanPhamController : Controller
    {
        // GET: SanPham
        ECommerceWebsiteEntities ql = new ECommerceWebsiteEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DSSanPham()
        {
            return View();
        }

        public ActionResult ChiTietSanPham(int id)
        {
            var sp = ql.SanPhams.Include("AnhSanPhams").Include("BienTheSanPhams").Where(s => s.MaSanPham == id).FirstOrDefault();
            if (sp == null) return HttpNotFound();
            return View(sp);
        }


    }
}