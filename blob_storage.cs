using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace raceday_api.database
{//start of namespace

    //what the api needs from the file storage (lets the unit tests use a fake one)
    public interface i_blob_storage
    {
        bool is_configured { get; }
        Task upload(string blob_name, Stream content, string content_type);
        Task<(Stream content, string content_type)?> download(string blob_name);
        Task delete(string blob_name);
    }

    //stores the images in Azure Blob Storage
    public class blob_storage : i_blob_storage
    {//start of class

        private readonly IConfiguration _config;
        private BlobContainerClient? _container;

        public blob_storage(IConfiguration config) { _config = config; }

        //true when a connection string was added (user secrets / appsettings)
        public bool is_configured => !string.IsNullOrWhiteSpace(_config["AzureBlobStorage:ConnectionString"]);

        //connect to the container, and create it (private) the first time
        private async Task<BlobContainerClient> get_container()
        {//start of method
            if (_container != null) return _container;

            string connection = _config["AzureBlobStorage:ConnectionString"] ?? "";
            string name = _config["AzureBlobStorage:ContainerName"] ?? "raceday-images";

            BlobContainerClient client = new BlobContainerClient(connection, name);
            await client.CreateIfNotExistsAsync(PublicAccessType.None);

            _container = client;
            return client;
        }//end of method

        public async Task upload(string blob_name, Stream content, string content_type)
        {//start of method
            BlobContainerClient container = await get_container();
            BlobClient blob = container.GetBlobClient(blob_name);
            await blob.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = content_type }
            });
        }//end of method

        public async Task<(Stream content, string content_type)?> download(string blob_name)
        {//start of method
            BlobContainerClient container = await get_container();
            BlobClient blob = container.GetBlobClient(blob_name);
            try
            {//start of try
                Response<BlobDownloadStreamingResult> response = await blob.DownloadStreamingAsync();
                return (response.Value.Content, response.Value.Details.ContentType);
            }//end of try
            catch (RequestFailedException error) when (error.Status == 404)
            {//start of catch
                return null;
            }//end of catch
        }//end of method

        public async Task delete(string blob_name)
        {//start of method
            BlobContainerClient container = await get_container();
            await container.GetBlobClient(blob_name).DeleteIfExistsAsync();
        }//end of method

    }//end of class
}//end of namespace
