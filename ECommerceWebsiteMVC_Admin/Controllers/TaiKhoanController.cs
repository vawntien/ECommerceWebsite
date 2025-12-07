using System.Linq;
using System.Web.Mvc;
using ECommerceWebsiteMVC_Admin.Models;

namespace ECommerceWebsiteMVC_Admin.Controllers
{
    public class TaiKhoanController : Controller
    {
        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(string TaiKhoan, string MatKhau)
        {
            // Tìm nhân viên theo Email hoặc Tài khoản
            var nv = db.NhanViens
                       .FirstOrDefault(x => (x.TaiKhoan == TaiKhoan || x.Email == TaiKhoan)
                                         && x.MatKhau == MatKhau);

            if (nv == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                return View();
            }
            else
            {
                // Lưu session đăng nhập
                Session["NhanVien"] = nv;
                if(nv.QuyenHeThong.MaQuyenHeThong == 1)
                {
                    return RedirectToAction("TongQuan", "QuanLyNguoiDungHeThong");
                }
                else if (nv.QuyenHeThong.MaQuyenHeThong == 2)
                {
                    return RedirectToAction("Index", "QuanLyThongKe");
                }
                else
                {
                    return RedirectToAction("Index", "QuanLyNhanVien");

                }
            }

               
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("DangNhap");
        }
    }
}
