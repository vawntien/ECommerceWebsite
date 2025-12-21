using ECommerceWebsiteMVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ECommerceWebsiteMVC.Controllers
{
    public class ThanhToanController : Controller
    {
        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        // GET: ThanhToan
        ECommerceWebsiteEntities db = new ECommerceWebsiteEntities();

        public ActionResult Index()
        {
            return View();
        }
        private int GetUserId()
        {
            if (Session["MaNguoiMua"] == null) return -1;
            return (int)Session["MaNguoiMua"];
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult MomoNotify()
        {
            try
            {
                string body;
                using (var sr = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    body = sr.ReadToEnd();
                }

                LogMomoNotify("RAW IPN", body);

                dynamic data = JsonConvert.DeserializeObject(body);

                if (data == null)
                    return new HttpStatusCodeResult(400);

                int orderId = int.Parse((string)data.orderId);
                int resultCode = (int)data.resultCode;

                var dh = db.DonHangs.Find(orderId);
                if (dh == null)
                    return new HttpStatusCodeResult(404);

                // ✅ CHỈ CẬP NHẬT KHI THANH TOÁN THÀNH CÔNG
                if (resultCode == 0 && dh.TrangThaiThanhToan == false)
                {
                    dh.TrangThaiThanhToan = true;
                    db.SaveChanges();

                    LogMomoNotify("PAID", "OrderId=" + orderId);
                }

                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                LogMomoNotify("EXCEPTION", ex.ToString());
                return new HttpStatusCodeResult(500);
            }
        }



        private void LogMomoNotify(string title, string content)
        {
            try
            {
                var logPath = Server.MapPath("~/App_Data/momo_notify_log.txt");
                var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + title + "\n" + (content ?? "") + "\n--------------------------------\n";
                System.IO.File.AppendAllText(logPath, line, Encoding.UTF8);
            }
            catch { }
        }

        public ActionResult MomoReturn()
        {
            // Receive redirect GET from MoMo containing query parameters
            try
            {
                var q = Request.QueryString;
                // copy relevant params to ViewBag for the view
                ViewBag.OrderId = q["orderId"] ?? q["orderid"] ?? "";
                ViewBag.RequestId = q["requestId"] ?? q["requestid"] ?? "";
                ViewBag.ResultCode = q["resultCode"] ?? q["resultcode"] ?? "";
                ViewBag.Message = q["message"] ?? "";
                ViewBag.TransId = q["transId"] ?? q["transid"] ?? "";
                ViewBag.RawQuery = Request.QueryString.ToString();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();

        }

        [HttpPost]
        public ActionResult ProcessMomoPayment(
            string NguoiNhan,
            string SDT,
            string DiaChi,
            string MaVoucher,
            int MaDVVC,
            decimal PhiShip,
            string GhiChu)
        {
            int userId = GetUserId();
            if (userId == -1)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            if (TempData["SelectedIds"] == null)
            {
                return Json(new { success = false, message = "Giỏ hàng trống" });
            }

            var idList = TempData["SelectedIds"].ToString()
                            .Split(',')
                            .Select(int.Parse)
                            .ToList();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var cartItems = db.ChiTietGioHangs
                                      .Where(x => idList.Contains(x.MaCTGH) && x.TrangThai == true)
                                      .ToList();

                    if (!cartItems.Any())
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = "Giỏ hàng lỗi" });
                    }

                    // ===== TÍNH TIỀN =====
                    decimal tongTienHang = 0;
                    foreach (var item in cartItems)
                    {
                        var bt = db.BienTheSanPhams.Find(item.MaBienThe);
                        tongTienHang += (decimal)(bt.GiaBan * item.SoLuong);
                    }

                    // ===== XỬ LÝ VOUCHER (GIỮ NGUYÊN LOGIC CỦA BẠN) =====
                    decimal giamGia = 0;
                    int? maGiamGiaID = null;

                    if (!string.IsNullOrEmpty(MaVoucher))
                    {
                        int vID;
                        if (int.TryParse(MaVoucher, out vID))
                        {
                            var vc = db.GiamGias.Find(vID);
                            if (vc != null)
                            {
                                if (vc.NgayBD != null && vc.NgayBD > DateTime.Now)
                                    return Json(new { success = false, message = "Voucher chưa áp dụng" });

                                if (vc.NgayKT != null && vc.NgayKT < DateTime.Now)
                                    return Json(new { success = false, message = "Voucher hết hạn" });

                                if (vc.GiaTriDonHangToiThieu > tongTienHang)
                                    return Json(new { success = false, message = "Không đủ điều kiện voucher" });

                                maGiamGiaID = vID;

                                if (vc.GiaTriGiam <= 1)
                                {
                                    giamGia = tongTienHang * (decimal)vc.GiaTriGiam;
                                    if (vc.GiaTriGiamToiDa > 0 && giamGia > vc.GiaTriGiamToiDa)
                                        giamGia = (decimal)vc.GiaTriGiamToiDa;
                                }
                                else
                                {
                                    giamGia = (decimal)vc.GiaTriGiam;
                                }
                            }
                        }
                    }

                    // ===== TẠO ĐƠN HÀNG (CHƯA THANH TOÁN) =====
                    var dh = new DonHang
                    {
                        MaDVVC = MaDVVC,
                        TenNguoiNhan = NguoiNhan,
                        SDT = SDT,
                        DiaChi = DiaChi,
                        ThoiGianDat = DateTime.Now,

                        TongTien = Math.Max(0, tongTienHang + PhiShip - giamGia),
                        PhiVanChuyen = PhiShip,
                        MaGiamGia = maGiamGiaID,

                        TrangThaiDonHang = "Chờ xác nhận",
                        TrangThaiVanChuyen = "Chưa giao",
                        TrangThaiThanhToan = false, // 🔴 QUAN TRỌNG

                        GhiChu = GhiChu ?? ""
                    };

                    db.DonHangs.Add(dh);
                    db.SaveChanges();

                    // ===== CHI TIẾT ĐƠN HÀNG + TRỪ KHO =====
                    foreach (var item in cartItems)
                    {
                        var bt = db.BienTheSanPhams.Find(item.MaBienThe);
                        if (bt.SoLuongTonKho < item.SoLuong)
                        {
                            transaction.Rollback();
                            return Json(new { success = false, message = "Hết hàng" });
                        }

                        bt.SoLuongTonKho -= item.SoLuong;

                        var ctdh = new ChiTietDonHang
                        {
                            MaDonHang = dh.MaDonHang,
                            MaCTGH = item.MaCTGH,
                            SoLuong = item.SoLuong
                        };

                        try { ctdh.GetType().GetProperty("DonGia").SetValue(ctdh, bt.GiaBan); } catch { }
                        try { ctdh.GetType().GetProperty("ThanhTien").SetValue(ctdh, (decimal)(bt.GiaBan * item.SoLuong)); } catch { }

                        db.ChiTietDonHangs.Add(ctdh);
                        item.TrangThai = false;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    // ===== GỌI ACTION TẠO LINK MOMO =====
                    string momoPayUrl = Url.Action(
                        "CreateMomoPayment",
                        "ThanhToan",
                        new { orderId = dh.MaDonHang },
                        Request.Url.Scheme
                    );

                    return Json(new
                    {
                        success = true,
                        redirectUrl = momoPayUrl
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }
        public ActionResult CreateMomoPayment(int orderId)
        {
            // 1. Lấy đơn hàng
            var dh = db.DonHangs.Find(orderId);
            if (dh == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            // 2. Chỉ cho phép tạo MoMo khi CHƯA thanh toán
            if (dh.TrangThaiThanhToan == true)
            {
                TempData["Error"] = "Đơn hàng đã thanh toán";
                return RedirectToAction("Index");
            }

            // 3. Lấy config MoMo
            string partnerCode = ConfigurationManager.AppSettings["Momo_PartnerCode"];
            string accessKey = ConfigurationManager.AppSettings["Momo_AccessKey"];
            string secretKey = ConfigurationManager.AppSettings["Momo_SecretKey"];
            string endpoint = ConfigurationManager.AppSettings["Momo_Endpoint"];
            string returnUrl = ConfigurationManager.AppSettings["Momo_ReturnUrl"];
            string notifyUrl = ConfigurationManager.AppSettings["Momo_NotifyUrl"];

            // 4. Dữ liệu gửi MoMo
            string requestId = Guid.NewGuid().ToString();
            string orderInfo = "Thanh toán đơn hàng #" + dh.MaDonHang;
            string amount = ((long)dh.TongTien).ToString(); // MoMo dùng số nguyên

            // 5. Tạo chuỗi ký
            string rawHash =
                "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" +
                "&ipnUrl=" + notifyUrl +
                "&orderId=" + dh.MaDonHang +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + returnUrl +
                "&requestId=" + requestId +
                "&requestType=captureWallet";

            string signature = MomoSecurity.SignSHA256(rawHash, secretKey);

            // 6. Body gửi MoMo
            var body = new
            {
                partnerCode = partnerCode,
                accessKey = accessKey,
                requestId = requestId,
                amount = amount,
                orderId = dh.MaDonHang.ToString(),
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                extraData = "",
                requestType = "captureWallet",
                signature = signature
            };

            string jsonBody = JsonConvert.SerializeObject(body);

            // 7. Gửi HTTP POST
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteData = Encoding.UTF8.GetBytes(jsonBody);

            request.ContentLength = byteData.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(byteData, 0, byteData.Length);
            }


            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var responseText = reader.ReadToEnd();
                dynamic momoRes = JsonConvert.DeserializeObject(responseText);

                // 8. Redirect sang MoMo
                if (momoRes != null && momoRes.payUrl != null)
                {
                    return Redirect(momoRes.payUrl.ToString());
                }
                else
                {
                    TempData["Error"] = "Không lấy được link MoMo";
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult DebugMomoSignature()
        {
            try
            {
                string bodyJson = string.Empty;
                using (var sr = new StreamReader(Request.InputStream))
                {
                    bodyJson = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(bodyJson))
                    return Json(new { success = false, message = "Empty body" }, JsonRequestBehavior.AllowGet);

                var model = JsonConvert.DeserializeObject<MomoIPNModel>(bodyJson);
                string secretKey = ConfigurationManager.AppSettings["Momo_SecretKey"];

                // Variant 1: original MoMo doc order (no encoding)
                string raw1 =
                    "amount=" + model.amount +
                    "&extraData=" + model.extraData +
                    "&message=" + model.message +
                    "&orderId=" + model.orderId +
                    "&orderInfo=" + model.orderInfo +
                    "&orderType=" + model.orderType +
                    "&partnerCode=" + model.partnerCode +
                    "&payType=" + model.payType +
                    "&requestId=" + model.requestId +
                    "&responseTime=" + model.responseTime +
                    "&resultCode=" + model.resultCode +
                    "&transId=" + model.transId;

                // Variant 2: partnerCode-first (no encoding)
                string raw2 =
                    "partnerCode=" + model.partnerCode +
                    "&orderId=" + model.orderId +
                    "&requestId=" + model.requestId +
                    "&amount=" + model.amount +
                    "&orderInfo=" + model.orderInfo +
                    "&orderType=" + model.orderType +
                    "&transId=" + model.transId +
                    "&resultCode=" + model.resultCode +
                    "&message=" + model.message +
                    "&payType=" + model.payType +
                    "&responseTime=" + model.responseTime +
                    "&extraData=" + model.extraData;

                string sig1 = MomoSecurity.SignSHA256(raw1, secretKey);
                string sig2 = MomoSecurity.SignSHA256(raw2, secretKey);

                return Json(new
                {
                    success = true,
                    modelSignature = model.signature,
                    raw1 = raw1,
                    sig1 = sig1,
                    raw2 = raw2,
                    sig2 = sig2,
                    body = model
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}