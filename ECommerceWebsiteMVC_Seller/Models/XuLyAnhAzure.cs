using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ECommerceWebsiteMVC.Models
{
    public class XuLyAnhAzure
    {
        private readonly string _connectionString;

        // Tên các container
        private const string USERS_CONTAINER = "users";
        private const string PRODUCTS_CONTAINER = "products";
        private const string VARIANTS_CONTAINER = "variants";
        private const string STORES_CONTAINER = "stores";
        private const string REVIEWS_CONTAINER = "reviews";
        private const string COMPLAINTS_CONTAINER = "complaints";

        public XuLyAnhAzure()
        {
            _connectionString =
                ConfigurationManager.ConnectionStrings["AzureBlobConnection"].ConnectionString;
        }

        // =============================
        // HÀM DÙNG RIÊNG CHO PRODUCTS
        // Tạo "folder" sản phẩm nếu chưa có
        // =============================

        // ==========================================================
        // 1) USERS
        // ==========================================================

        // Thêm ảnh user vào container "users"
        public async Task<string> UploadUserImageAsync(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                throw new Exception("File không hợp lệ!");

            string fileName = Path.GetFileName(file.FileName);


            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(USERS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.UploadAsync(file.InputStream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString();
        }

        // Xóa ảnh trong container "users"
        public async Task DeleteUserImageAsync(string fileName)
        {
            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(USERS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.DeleteIfExistsAsync();
        }

        // ==========================================================
        // 2) PRODUCTS  (Có folder theo mã sản phẩm)
        // ==========================================================

        // Thêm ảnh sản phẩm: products/{maSP}/{fileName}
        public async Task<string> UploadProductImageAsync(HttpPostedFileBase file, string maSanPham)
        {
            if (file == null || file.ContentLength == 0)
                throw new Exception("File không hợp lệ!");

            if (string.IsNullOrEmpty(maSanPham))
                throw new Exception("Mã sản phẩm không hợp lệ!");

            string fileName = Path.GetFileName(file.FileName);

            // Tạo folder mã sản phẩm nếu chưa có
            //await CreateProductFolderIfNotExistsAsync(maSanPham);

            string blobPath = $"{maSanPham}/{fileName}";

            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(PRODUCTS_CONTAINER);
            BlobClient blob = container.GetBlobClient(blobPath);

            await blob.UploadAsync(file.InputStream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString(); // https://.../products/1/xxx.jpg
        }
        // XÓA TOÀN BỘ FOLDER SẢN PHẨM + TOÀN BỘ ẢNH BÊN TRONG
        public async Task DeleteProductFolderAsync(string maSanPham)
        {
            if (string.IsNullOrEmpty(maSanPham))
                throw new Exception("Mã sản phẩm không hợp lệ!");

            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(PRODUCTS_CONTAINER);

            // Lấy danh sách blob theo prefix (thư mục sản phẩm)
            var blobs = container.GetBlobs(prefix: $"{maSanPham}/");

            foreach (var item in blobs)
            {
                BlobClient blob = container.GetBlobClient(item.Name);
                await blob.DeleteIfExistsAsync();
            }
        }


        // Xóa ảnh sản phẩm theo mã sản phẩm + tên file
        public async Task DeleteProductImageAsync(string maSanPham, string fileName)
        {
            string blobPath = $"{maSanPham}/{fileName}";

            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(PRODUCTS_CONTAINER);
            BlobClient blob = container.GetBlobClient(blobPath);

            await blob.DeleteIfExistsAsync();
        }

        // ==========================================================
        // 3) VARIANTS
        // ==========================================================

        public async Task<string> UploadVariantImageAsync(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                throw new Exception("File không hợp lệ!");

            string fileName = Path.GetFileName(file.FileName);

            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(VARIANTS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.UploadAsync(file.InputStream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString();
        }

        public async Task DeleteVariantImageAsync(string fileName)
        {
            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(VARIANTS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.DeleteIfExistsAsync();
        }

        // ==========================================================
        // 4) STORES
        // ==========================================================

        public async Task<string> UploadStoreImageAsync(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                throw new Exception("File không hợp lệ!");

            string fileName = Path.GetFileName(file.FileName);

            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(STORES_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.UploadAsync(file.InputStream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString();
        }

        public async Task DeleteStoreImageAsync(string fileName)
        {
            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(STORES_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.DeleteIfExistsAsync();
        }

        // ==========================================================
        // 5) REVIEWS
        // ==========================================================

        public async Task<string> UploadReviewImageAsync(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                throw new Exception("File không hợp lệ!");

            string fileName = Path.GetFileName(file.FileName);

            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(REVIEWS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.UploadAsync(file.InputStream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString();
        }

        public async Task DeleteReviewImageAsync(string fileName)
        {
            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(REVIEWS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.DeleteIfExistsAsync();
        }

        // ==========================================================
        // 6) COMPLAINTS
        // ==========================================================

        public async Task<string> UploadComplaintImageAsync(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                throw new Exception("File không hợp lệ!");

            string fileName = Path.GetFileName(file.FileName);

            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(COMPLAINTS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.UploadAsync(file.InputStream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString();
        }

        public async Task DeleteComplaintImageAsync(string fileName)
        {
            BlobServiceClient service = new BlobServiceClient(_connectionString);
            BlobContainerClient container = service.GetBlobContainerClient(COMPLAINTS_CONTAINER);
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.DeleteIfExistsAsync();
        }
    }
}