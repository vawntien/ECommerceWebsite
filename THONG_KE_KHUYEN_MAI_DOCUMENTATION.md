# Tài liệu: Tính năng Thống kê Khuyến mãi

## 📋 Tổng quan

Tính năng thống kê khuyến mãi cho phép quản trị viên theo dõi và phân tích hiệu quả của các chương trình khuyến mãi (voucher) trên hệ thống. Tính năng này cung cấp 3 chức năng chính:

1. **Thống kê doanh thu từ khuyến mãi**: Tổng số tiền đã giảm cho khách hàng và tổng doanh thu thu về từ các đơn hàng có sử dụng mã
2. **Theo dõi lịch sử dùng mã**: Truy vấn từ kho Đơn hàng để xem khách hàng nào đã dùng mã vào thời gian nào
3. **Xếp hạng Voucher hiệu quả**: Mã nào được người dùng yêu thích và sử dụng nhiều nhất để làm cơ sở cho các chiến dịch sau

---

## 📁 Các file đã tạo/sửa đổi

### 1. **Models/ThongKeKhuyenMaiViewModel.cs** (File mới)
   - **Mục đích**: Định nghĩa các ViewModel để truyền dữ liệu từ Controller sang View
   - **Các class được định nghĩa**:
     - `ThongKeKhuyenMaiViewModel`: ViewModel chính chứa tất cả dữ liệu thống kê
     - `LichSuSuDungVoucher`: Thông tin chi tiết từng lần sử dụng voucher
     - `VoucherHieuQua`: Thông tin hiệu quả của từng voucher

### 2. **Models/DBQuanLyKhuyenMai.cs** (Sửa đổi)
   - **Thêm các method mới**:
     - `TinhSoTienGiamGia(GiamGia voucher, decimal tongTienHang)`: Tính số tiền giảm giá từ voucher
     - `TinhTongTienHang(DonHang donHang)`: Tính tổng tiền hàng từ chi tiết đơn hàng
     - `LayThongKeKhuyenMai(DateTime? tuNgay, DateTime? denNgay)`: Method chính lấy thống kê

### 3. **Controllers/QuanLyNguoiDungHeThongController.cs** (Sửa đổi)
   - **Thêm action mới**: `ThongKeKhuyenMai(string tuNgay, string denNgay)`
   - **Chức năng**: Xử lý request từ View, parse ngày tháng, gọi method thống kê và trả về View

### 4. **Views/QuanLyNguoiDungHeThong/ThongKeKhuyenMai.cshtml** (File mới)
   - **Mục đích**: Giao diện hiển thị thống kê
   - **Các phần chính**:
     - Bộ lọc theo thời gian
     - Tổng quan doanh thu (3 card thống kê)
     - Bảng xếp hạng voucher hiệu quả
     - Bảng lịch sử sử dụng voucher

### 5. **Views/QuanLyNguoiDungHeThong/QuanLyKhuyenMai.cshtml** (Sửa đổi)
   - **Thêm**: Nút "Thống kê" để điều hướng đến trang thống kê

### 6. **ECommerceWebsiteMVC_Admin.csproj** (Sửa đổi)
   - **Thêm**: Include file `ThongKeKhuyenMaiViewModel.cs` vào project

---

## 🔄 Luồng hoạt động chi tiết

### **Luồng 1: Người dùng truy cập trang thống kê**

```
1. User click nút "Thống kê" trong trang Quản lý Khuyến mãi
   ↓
2. Browser gửi GET request đến: /QuanLyNguoiDungHeThong/ThongKeKhuyenMai
   ↓
3. Controller.ThongKeKhuyenMai() được gọi
   ↓
4. Controller parse tham số tuNgay và denNgay (nếu có)
   ↓
5. Controller gọi dbKhuyenMai.LayThongKeKhuyenMai(tuNgay, denNgay)
   ↓
6. Method LayThongKeKhuyenMai() thực hiện:
   a. Tạo ViewModel mới
   b. Query tất cả đơn hàng có MaGiamGia != null
   c. Lọc theo ngày (nếu có)
   d. Duyệt qua từng đơn hàng:
      - Tính tổng tiền hàng từ ChiTietDonHang
      - Tính số tiền giảm giá từ voucher
      - Lấy thông tin khách hàng
      - Thêm vào danh sách lịch sử
   e. Tính tổng: tổng tiền giảm, tổng doanh thu, số đơn hàng
   f. Nhóm đơn hàng theo voucher để xếp hạng
   g. Tính thống kê cho từng voucher
   ↓
7. Controller trả về View với ViewModel
   ↓
8. View render HTML hiển thị thống kê
```

### **Luồng 2: Tính số tiền giảm giá**

```
Input: DonHang (đơn hàng) và GiamGia (voucher)
   ↓
1. Tính tổng tiền hàng:
   tongTienHang = Sum(ChiTietDonHang.ThanhTien)
   ↓
2. Kiểm tra loại giảm giá của voucher:
   ↓
   Nếu GiaTriGiam <= 1:
      → Giảm theo phần trăm
      giamGia = tongTienHang * GiaTriGiam
      Nếu có GiaTriGiamToiDa và giamGia > GiaTriGiamToiDa:
         → giamGia = GiaTriGiamToiDa
   ↓
   Nếu GiaTriGiam > 1:
      → Giảm theo số tiền cố định
      giamGia = GiaTriGiam
   ↓
Output: Số tiền giảm giá (decimal)
```

### **Luồng 3: Xếp hạng voucher**

```
1. Nhóm tất cả đơn hàng theo MaGiamGia
   ↓
2. Với mỗi nhóm (voucher):
   a. Đếm số lần sử dụng (Count)
   b. Tính tổng tiền giảm (Sum của số tiền giảm từng đơn)
   c. Tính tổng doanh thu (Sum của TongTien từng đơn)
   d. Xác định trạng thái voucher:
      - "Sắp diễn ra": NgayBD > DateTime.Now
      - "Hết hạn": NgayKT < DateTime.Now
      - "Đang hiệu lực": NgayBD <= Now <= NgayKT
   ↓
3. Sắp xếp theo số lần sử dụng (giảm dần)
   ↓
4. Trả về danh sách VoucherHieuQua
```

### **Luồng 4: Lọc theo thời gian**

```
1. User nhập "Từ ngày" và "Đến ngày" trong form
   ↓
2. Click nút "Lọc"
   ↓
3. Form submit GET request với parameters: tuNgay, denNgay
   ↓
4. Controller parse ngày:
   - Nếu tuNgay không rỗng → Parse thành DateTime
   - Nếu denNgay không rỗng → Parse thành DateTime
   ↓
5. Trong method LayThongKeKhuyenMai():
   - Nếu tuNgay có giá trị:
     → Lọc: ThoiGianDat >= tuNgay
   - Nếu denNgay có giá trị:
     → Lọc: ThoiGianDat <= denNgay.AddDays(1)
   ↓
6. Chỉ tính toán thống kê cho các đơn hàng thỏa điều kiện
```

---

## 📊 Cấu trúc dữ liệu

### **ThongKeKhuyenMaiViewModel**
```csharp
{
    TongTienGiamChoKhachHang: decimal,      // Tổng số tiền đã giảm
    TongDoanhThuTuVoucher: decimal,         // Tổng doanh thu từ voucher
    TongSoDonHangSuDungVoucher: int,        // Số đơn hàng đã dùng voucher
    LichSuSuDung: List<LichSuSuDungVoucher>, // Lịch sử sử dụng
    XepHangVoucher: List<VoucherHieuQua>    // Xếp hạng voucher
}
```

### **LichSuSuDungVoucher**
```csharp
{
    MaDonHang: int,
    TenKhachHang: string,
    Email: string,
    TenVoucher: string,
    SoTienGiam: decimal,
    TongTienDonHang: decimal,
    ThoiGianDat: DateTime,
    TrangThaiDonHang: string
}
```

### **VoucherHieuQua**
```csharp
{
    MaGiamGia: int,
    TenMaGG: string,
    SoLanSuDung: int,
    TongTienGiam: decimal,
    TongDoanhThu: decimal,
    NgayBD: DateTime?,
    NgayKT: DateTime?,
    TrangThai: string
}
```

---

## 🎯 Các tính năng chính

### **1. Thống kê tổng quan doanh thu**
- **Tổng tiền đã giảm**: Tổng số tiền mà hệ thống đã giảm cho khách hàng thông qua các voucher
- **Tổng doanh thu**: Tổng số tiền thu về từ các đơn hàng có sử dụng voucher (sau khi đã trừ giảm giá)
- **Số đơn hàng**: Tổng số đơn hàng đã sử dụng voucher

### **2. Xếp hạng voucher hiệu quả**
- Hiển thị danh sách voucher được sắp xếp theo số lần sử dụng (nhiều nhất trước)
- Mỗi voucher hiển thị:
  - Tên voucher (có link đến chi tiết)
  - Số lần sử dụng
  - Tổng tiền đã giảm
  - Tổng doanh thu từ voucher đó
  - Thời gian hiệu lực
  - Trạng thái (Đang hiệu lực/Hết hạn/Sắp diễn ra)

### **3. Lịch sử sử dụng voucher**
- Hiển thị chi tiết từng đơn hàng đã sử dụng voucher
- Thông tin bao gồm:
  - Mã đơn hàng
  - Thông tin khách hàng (tên, email)
  - Tên voucher đã sử dụng
  - Số tiền giảm
  - Tổng tiền đơn hàng
  - Thời gian đặt hàng
  - Trạng thái đơn hàng

### **4. Lọc theo thời gian**
- Cho phép lọc thống kê theo khoảng thời gian
- Có thể chỉ chọn "Từ ngày" hoặc "Đến ngày" hoặc cả hai
- Có nút "Xóa bộ lọc" để xem tất cả dữ liệu

---

## 🔧 Các method quan trọng

### **1. TinhSoTienGiamGia()**
```csharp
private decimal TinhSoTienGiamGia(GiamGia voucher, decimal tongTienHang)
```
- **Input**: Voucher và tổng tiền hàng
- **Output**: Số tiền giảm giá
- **Logic**:
  - Nếu `GiaTriGiam <= 1`: Giảm theo phần trăm
  - Nếu `GiaTriGiam > 1`: Giảm theo số tiền cố định
  - Áp dụng giới hạn `GiaTriGiamToiDa` nếu có

### **2. TinhTongTienHang()**
```csharp
private decimal TinhTongTienHang(DonHang donHang)
```
- **Input**: Đơn hàng
- **Output**: Tổng tiền hàng (từ chi tiết đơn hàng)
- **Logic**: Sum tất cả `ThanhTien` từ `ChiTietDonHangs`

### **3. LayThongKeKhuyenMai()**
```csharp
public ThongKeKhuyenMaiViewModel LayThongKeKhuyenMai(DateTime? tuNgay, DateTime? denNgay)
```
- **Input**: Ngày bắt đầu và ngày kết thúc (optional)
- **Output**: ViewModel chứa tất cả thống kê
- **Các bước**:
  1. Query đơn hàng có voucher
  2. Lọc theo ngày (nếu có)
  3. Tính toán thống kê cho từng đơn hàng
  4. Nhóm và xếp hạng voucher
  5. Sắp xếp lịch sử theo thời gian

---

## 🚀 Cách sử dụng

### **Truy cập trang thống kê:**
1. Vào trang **Quản lý Khuyến mãi** (`/QuanLyNguoiDungHeThong/QuanLyKhuyenMai`)
2. Click nút **"Thống kê"** ở góc trên bên phải
3. Hoặc truy cập trực tiếp: `/QuanLyNguoiDungHeThong/ThongKeKhuyenMai`

### **Lọc theo thời gian:**
1. Nhập "Từ ngày" và/hoặc "Đến ngày"
2. Click nút **"Lọc"**
3. Kết quả sẽ được cập nhật theo khoảng thời gian đã chọn
4. Click **"Xóa bộ lọc"** để xem lại tất cả dữ liệu

### **Xem chi tiết voucher:**
- Click vào tên voucher trong bảng "Xếp hạng Voucher hiệu quả"
- Sẽ chuyển đến trang chi tiết voucher

---

## 📝 Lưu ý kỹ thuật

1. **Tính toán số tiền giảm giá**: 
   - Sử dụng dữ liệu từ `ChiTietDonHang.ThanhTien` để tính tổng tiền hàng
   - Tính lại số tiền giảm giá dựa trên logic của voucher (không lưu trực tiếp trong DB)

2. **Lấy thông tin khách hàng**:
   - Lấy từ `ChiTietDonHang → ChiTietGioHang → GioHang → NguoiMua`
   - Nếu không tìm thấy, sử dụng thông tin từ `DonHang.TenNguoiNhan`

3. **Xử lý null**:
   - Sử dụng null-conditional operators (`?.`) và null-coalescing (`??`) để tránh lỗi

4. **Performance**:
   - Query tất cả đơn hàng có voucher một lần
   - Lọc và tính toán trong memory (phù hợp với số lượng đơn hàng vừa phải)

---

## 🎨 Giao diện

- **3 Card thống kê**: Màu đỏ (tiền giảm), xanh lá (doanh thu), xanh dương (số đơn)
- **Bảng xếp hạng**: Sắp xếp theo số lần sử dụng, có badge màu sắc cho trạng thái
- **Bảng lịch sử**: Hiển thị đầy đủ thông tin, có badge màu cho trạng thái đơn hàng
- **Responsive**: Hỗ trợ dark mode, responsive trên mobile

---

## ✅ Checklist hoàn thành

- [x] Tạo ViewModel cho thống kê
- [x] Thêm methods tính toán vào DBQuanLyKhuyenMai
- [x] Thêm action vào Controller
- [x] Tạo View hiển thị thống kê
- [x] Thêm link điều hướng
- [x] Include file vào project (.csproj)
- [x] Xử lý lọc theo thời gian
- [x] Tính toán chính xác số tiền giảm giá
- [x] Xếp hạng voucher theo hiệu quả
- [x] Hiển thị lịch sử sử dụng voucher

---

## 🔄 Cải tiến có thể thêm trong tương lai

1. **Export Excel/PDF**: Xuất báo cáo thống kê ra file
2. **Biểu đồ**: Thêm biểu đồ cột/đường để trực quan hóa dữ liệu
3. **Phân trang**: Phân trang cho bảng lịch sử nếu dữ liệu quá nhiều
4. **Lọc nâng cao**: Lọc theo voucher cụ thể, theo khách hàng, theo trạng thái đơn hàng
5. **Cache**: Cache kết quả thống kê để tăng performance
6. **Real-time**: Cập nhật thống kê real-time khi có đơn hàng mới

---

**Ngày tạo**: 2024
**Phiên bản**: 1.0
**Tác giả**: AI Assistant


