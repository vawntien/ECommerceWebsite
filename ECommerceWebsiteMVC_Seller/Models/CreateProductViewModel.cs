using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsiteMVC_Seller.Models
{
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(120, ErrorMessage = "Tên sản phẩm tối đa 120 kí tự.")]
        public string TenSanPham { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngành hàng.")]
        public int? MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
        public string MoTa { get; set; }

        public string AttributeOneName { get; set; }
        public string AttributeTwoName { get; set; }

        public IEnumerable<DanhMuc> DanhMucs { get; set; }
    }
}





