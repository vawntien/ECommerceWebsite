using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerceWebsiteMVC.Models
{
    /// <summary>
    /// Partial class để mở rộng model DonHang
    /// Thêm các properties và methods tùy chỉnh mà không ảnh hưởng đến auto-generated code
    /// </summary>
    public partial class DonHang
    {
        /// <summary>
        /// Mã người dùng (người mua) của đơn hàng
        /// LƯU Ý: Cần thêm cột này vào database và update Entity Framework model
        /// Xem file Database_Update_Instructions.sql để biết chi tiết
        /// </summary>
        public int? MaNguoiDung { get; set; }

        /// <summary>
        /// Navigation property đến người dùng (người mua)
        /// Chỉ hoạt động sau khi đã update database và EF model
        /// </summary>
        public virtual NguoiDung NguoiDung { get; set; }
    }
}

