using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunctionAppForIoTHub
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([BlobTrigger("devicetwindemo/{name}", Connection = "testblob")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");


            string storageAccount_connectionString = "";
            string iotHubstr = "";
            string deviceName = "";
            string filePath = "";

            string SAStokenURL = new BlobSAStokenHelper().GetblobFileSASToken(storageAccount_connectionString, filePath, name);

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubstr);
            DeviceManager deviceManager = new DeviceManager(registryManager);
            deviceManager.ConfigDevice(deviceName, SAStokenURL).GetAwaiter().GetResult();

            return;
        }

        public class BlobSAStokenHelper
        {
            public string GetblobFileSASToken(string _connectionString,  string _filepath,string _fileName) 
            {
                CloudStorageAccount mycloudStorageAccount = CloudStorageAccount.Parse(_connectionString);
                CloudBlobClient blobClient = mycloudStorageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference(_filepath);
                CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(_fileName);

                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // Set start time to five minutes before now to avoid clock skew.
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read
                };

                return cloudBlockBlob.StorageUri.PrimaryUri.OriginalString + cloudBlockBlob.GetSharedAccessSignature(adHocPolicy);
            }
        }

        public class DeviceManager
        {
            private readonly RegistryManager _registryManager;
            public DeviceManager(RegistryManager registryManager)
            {
                _registryManager = registryManager ?? throw new ArgumentNullException(nameof(registryManager));
            }
            public async Task ConfigDevice(string _deviceName, string _sasToken)
            {
                Console.WriteLine("Seting Device Twin.");

                Twin twin = await _registryManager.GetTwinAsync(_deviceName);

                twin.Properties.Desired["tagFileSASUrl"] = _sasToken;

                await _registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);

                Console.WriteLine("DONE");
            }
        }
    }
}
