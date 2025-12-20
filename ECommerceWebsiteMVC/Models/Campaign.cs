//------------------------------------------------------------------------------
// Campaign Model - Chương trình khuyến mãi chủ động
//------------------------------------------------------------------------------

namespace ECommerceWebsiteMVC.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Campaign
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Campaign()
        {
            this.CampaignSanPhams = new HashSet<CampaignSanPham>();
            this.CampaignDanhMucs = new HashSet<CampaignDanhMuc>();
        }

        public int MaCampaign { get; set; }
        
        [Required(ErrorMessage = "Tên campaign không được để trống")]
        [StringLength(200)]
        public string TenCampaign { get; set; }
        
        [StringLength(500)]
        public string MoTa { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LoaiCampaign { get; set; } // Chủ động hoặc Thụ động
        
        [Required]
        [StringLength(20)]
        public string LoaiGiamGia { get; set; } // PhanTram hoặc SoTien
        
        [Required]
        public decimal GiaTriGiam { get; set; }
        
        public Nullable<System.DateTime> NgayBD { get; set; }
        public Nullable<System.DateTime> NgayKT { get; set; }
        
        public bool TrangThai { get; set; }
        
        public System.DateTime NgayTao { get; set; }
        
        public Nullable<int> NguoiTao { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CampaignSanPham> CampaignSanPhams { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CampaignDanhMuc> CampaignDanhMucs { get; set; }
    }
}

