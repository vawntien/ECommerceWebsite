using System;
using System.Linq;

namespace ECommerceWebsiteMVC_Seller.Models
{
    public class DB_CuaHang : IDisposable
    {
        private readonly ECommerceWebsiteEntities _db;

        public DB_CuaHang()
        {
            _db = new ECommerceWebsiteEntities();
        }

        public bool RegisterNewShop(
            int userId,
            string tenCuaHang,
            string diaChi,
            string maSoThue,
            out string message)
        {
            message = "";

            try
            {
                // 1. Kiểm tra đã có shop chưa
                if (_db.CuaHangs.Any(x => x.MaNguoiBan == userId))
                {
                    message = "Tài khoản này đã sở hữu cửa hàng.";
                    return false;
                }

                // 2. Tạo cửa hàng (KHÔNG xử lý hình ảnh)
                var newShop = new CuaHang
                {
                    MaNguoiBan = userId,
                    TenCuaHang = tenCuaHang,
                    DiaChi = diaChi,
                    MaSoThue = maSoThue,
                    HinhAnh = null,
                    NgayDangKy = DateTime.Now,
                    TrangThai = true
                };

                _db.CuaHangs.Add(newShop);
                _db.SaveChanges();

                message = "Đăng ký cửa hàng thành công!";
                return true;
            }
            catch (Exception)
            {
                message = "Lỗi hệ thống. Vui lòng thử lại sau.";
                return false;
            }
        }

        // Kiểm tra nhanh
        public bool HasShop(int userId)
        {
            return _db.CuaHangs.Any(x => x.MaNguoiBan == userId);
        }

        // Lấy tên shop
        public string GetShopName(int userId)
        {
            return _db.CuaHangs
                      .Where(x => x.MaNguoiBan == userId)
                      .Select(x => x.TenCuaHang)
                      .FirstOrDefault();
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
