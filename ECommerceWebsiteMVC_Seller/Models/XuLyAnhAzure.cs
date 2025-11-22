using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;

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

    // ==========================================================
    // 1) USERS
    // ==========================================================

    // Thêm ảnh user vào container "users"
    // fileName: nếu null => dùng tên gốc từ file
    public async Task<string> UploadUserImageAsync(HttpPostedFileBase file, string fileName = null)
    {
        if (file == null || file.ContentLength == 0)
            throw new Exception("File không hợp lệ!");

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = Path.GetFileName(file.FileName);

        BlobServiceClient service = new BlobServiceClient(_connectionString);
        BlobContainerClient container = service.GetBlobContainerClient(USERS_CONTAINER);
        BlobClient blob = container.GetBlobClient(fileName);

        await blob.UploadAsync(file.InputStream, new BlobHttpHeaders
        {
            ContentType = file.ContentType
        });

        return blob.Uri.ToString();
    }

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
    // fileName: nếu null => dùng tên gốc từ file
    public async Task<string> UploadProductImageAsync(HttpPostedFileBase file, string maSanPham, string fileName = null)
    {
        if (file == null || file.ContentLength == 0)
            throw new Exception("File không hợp lệ!");

        if (string.IsNullOrEmpty(maSanPham))
            throw new Exception("Mã sản phẩm không hợp lệ!");

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = Path.GetFileName(file.FileName);

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

    public async Task DeleteProductFolderAsync(string maSanPham)
    {
        if (string.IsNullOrEmpty(maSanPham))
            throw new Exception("Mã sản phẩm không hợp lệ!");

        BlobServiceClient service = new BlobServiceClient(_connectionString);
        BlobContainerClient container = service.GetBlobContainerClient(PRODUCTS_CONTAINER);

        var blobs = container.GetBlobs(prefix: $"{maSanPham}/");

        foreach (var item in blobs)
        {
            BlobClient blob = container.GetBlobClient(item.Name);
            await blob.DeleteIfExistsAsync();
        }
    }

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

    public async Task<string> UploadVariantImageAsync(HttpPostedFileBase file, string fileName = null)
    {
        if (file == null || file.ContentLength == 0)
            throw new Exception("File không hợp lệ!");

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = Path.GetFileName(file.FileName);

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

    public async Task<string> UploadStoreImageAsync(HttpPostedFileBase file, string fileName = null)
    {
        if (file == null || file.ContentLength == 0)
            throw new Exception("File không hợp lệ!");

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = Path.GetFileName(file.FileName);

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

    public async Task<string> UploadReviewImageAsync(HttpPostedFileBase file, string fileName = null)
    {
        if (file == null || file.ContentLength == 0)
            throw new Exception("File không hợp lệ!");

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = Path.GetFileName(file.FileName);

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

    public async Task<string> UploadComplaintImageAsync(HttpPostedFileBase file, string fileName = null)
    {
        if (file == null || file.ContentLength == 0)
            throw new Exception("File không hợp lệ!");

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = Path.GetFileName(file.FileName);

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
