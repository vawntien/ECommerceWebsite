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
        /// Mã người mua của đơn hàng (lấy từ giỏ hàng => người mua)
        /// </summary>
        public int? MaNguoiMua { get; set; }

        /// <summary>
        /// Navigation property đến thực thể người mua
        /// </summary>
        public virtual NguoiMua NguoiMua { get; set; }
    }
}

