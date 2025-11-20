using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class TaiKhoanController : Controller
    {
        ECommerceWebsiteEntities eCommerceWebsiteEntities = new ECommerceWebsiteEntities();

        // GET: TaiKhoan
        public ActionResult DangKy()
        {
            return View();
        }

        public ActionResult DangNhap()
        {
            return View();
        }
    }
}