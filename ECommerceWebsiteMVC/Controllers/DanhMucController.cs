using ECommerceWebsiteMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class DanhMucController : Controller
    {
        QLBanHang_SPEntities1 db = new QLBanHang_SPEntities1();
        // GET: DanhMuc
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sidebar(int? id)
        {
            ViewBag.DanhMucDangChon = id;
            return PartialView("_SidebarDanhMuc", db.DanhMucs.ToList());
        }

        public ActionResult TheoDanhMuc(int id)
        {
            var dssp = db.SanPhams.Include("AnhSanPhams").Where(x => x.MaDanhMuc == id).ToList();
            return View("~/Views/NguoiMua/Index.cshtml", dssp);
        }
    }
}