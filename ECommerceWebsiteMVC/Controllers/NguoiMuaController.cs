using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class NguoiMuaController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
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