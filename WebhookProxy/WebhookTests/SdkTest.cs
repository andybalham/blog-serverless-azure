using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookTests;

public class SdkTest
{
    [Fact]
    public async Task ReadFromBlobContainer()
    {
        string connectionString = "UseDevelopmentStorage=true"; // Local emulator connection string
        string containerName = "webhook-payloads-valid";

        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        string blobName = $"test/sample-{Guid.NewGuid()}.json";
        string jsonString = "{\"name\":\"John Doe\",\"age\":30}";
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
        using (var stream = new System.IO.MemoryStream(byteArray))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        // Assuming you have blobs named like 'folder1/file.txt', 'folder2/file.txt'
        string? prefix = null;  // Use null to list all blobs in the container, or "folder1/" to list blobs in a specific folder

        var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: prefix);
        //var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/");

        await foreach (var item in blobs)
        {
            if (item.IsPrefix)
            {
                Console.WriteLine($"Folder: {item.Prefix}");
            }
            else
            {
                Console.WriteLine($"Blob: {item.Blob.Name}");
            }
        }
    }
}
