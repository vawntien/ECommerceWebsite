-- Tạo bảng Campaign (Chương trình khuyến mãi chủ động)
CREATE TABLE Campaign (
    MaCampaign INT PRIMARY KEY IDENTITY(1,1),
    TenCampaign NVARCHAR(200) NOT NULL,
    MoTa NVARCHAR(500),
    LoaiCampaign NVARCHAR(50) NOT NULL DEFAULT N'Chủ động', -- Chủ động hoặc Thụ động
    LoaiGiamGia NVARCHAR(20) NOT NULL, -- 'PhanTram' hoặc 'SoTien'
    GiaTriGiam DECIMAL(18,2) NOT NULL, -- Nếu PhanTram thì là %, nếu SoTien thì là số tiền
    NgayBD DATETIME,
    NgayKT DATETIME,
    TrangThai BIT NOT NULL DEFAULT 1, -- 1: Đang hoạt động, 0: Đã tắt
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NguoiTao INT, -- MaNhanVien
    CONSTRAINT FK_Campaign_NhanVien FOREIGN KEY (NguoiTao) REFERENCES NhanVien(MaNhanVien)
);

-- Bảng quan hệ Campaign với Sản phẩm (nhiều-nhiều)
CREATE TABLE CampaignSanPham (
    MaCampaign INT NOT NULL,
    MaSanPham INT NOT NULL,
    PRIMARY KEY (MaCampaign, MaSanPham),
    CONSTRAINT FK_CampaignSanPham_Campaign FOREIGN KEY (MaCampaign) REFERENCES Campaign(MaCampaign) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignSanPham_SanPham FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham) ON DELETE CASCADE
);

-- Bảng quan hệ Campaign với Danh mục (nhiều-nhiều)
CREATE TABLE CampaignDanhMuc (
    MaCampaign INT NOT NULL,
    MaDanhMuc INT NOT NULL,
    PRIMARY KEY (MaCampaign, MaDanhMuc),
    CONSTRAINT FK_CampaignDanhMuc_Campaign FOREIGN KEY (MaCampaign) REFERENCES Campaign(MaCampaign) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignDanhMuc_DanhMuc FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc) ON DELETE CASCADE
);

-- Tạo index để tối ưu truy vấn
CREATE INDEX IX_Campaign_NgayBD_NgayKT ON Campaign(NgayBD, NgayKT);
CREATE INDEX IX_Campaign_TrangThai ON Campaign(TrangThai);
CREATE INDEX IX_CampaignSanPham_MaSanPham ON CampaignSanPham(MaSanPham);
CREATE INDEX IX_CampaignDanhMuc_MaDanhMuc ON CampaignDanhMuc(MaDanhMuc);

