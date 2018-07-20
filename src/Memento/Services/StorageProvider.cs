using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;


namespace Memento.Services
{
    //https://github.com/Azure/azure-storage-net
    //https://github.com/sebagomez/azurestorageexplorer/blob/master/src/StorageLibrary/Container.cs
    //https://github.com/Azure/azure-storage-net/tree/master/Documentation
    /// <summary>
    /// Implementation of the Azure Storage Provider
    /// </summary>
    public class StorageProvider : IStorageProvider
    {
        #region Container

        /// <summary>
        /// Retrieves a List of Containers within a BlobStorage Account
        /// </summary>
        /// <param name="account">The Name of the Blob Storage Account</param>
        /// <param name="key">The Primary or Secondary Access Key</param>
        /// <returns>List of CloudBlobContainer</returns>
        public async Task<List<CloudBlobContainer>> ListContainerAsync(string account, string key)
        {
            StorageCredentials sc = new StorageCredentials(account, key);
            //Initializes a new instance of the CloudStorageAccount class using the specified credentials, and specifies whether to use HTTP or HTTPS to connect to the storage services. 
            CloudStorageAccount csa = new CloudStorageAccount(sc,true);

            //I guess this is the legacy way ^^ cant remember so 2012
            //string sas = "";
            //if (key.StartsWith("?"))
            //    sas = key;

            ////Set URI
            //Uri blobUri = new Uri($"http://{account}.blob.core.windows.net/{sas}");
            
            //Initializes a new instance of the CloudBlobClient class using the specified Blob service endpoint and account credentials.
            var blobClient = new CloudBlobClient(csa.BlobStorageUri,sc);

            BlobContinuationToken continuationToken = null;

            List<CloudBlobContainer> results = new List<CloudBlobContainer>();

            //TODO: Code Review it is not nice ^^
            try
            {
                do
                {
                    var response = await blobClient.ListContainersSegmentedAsync(continuationToken);
                    continuationToken = response.ContinuationToken;
                    results.AddRange(response.Results);
                }
                while (continuationToken != null);
            }
            catch (Exception e)
            {
                throw new Exception("7191 List Containers Failed",e);
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="key"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static CloudBlobContainer GetContainer(string account, string key, string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                return null;

            StorageCredentials sc = new StorageCredentials(account, key);
            //Initializes a new instance of the CloudStorageAccount class using the specified credentials, and specifies whether to use HTTP or HTTPS to connect to the storage services. 
            CloudStorageAccount csa = new CloudStorageAccount(sc, true);

            //Initializes a new instance of the CloudBlobClient class using the specified Blob service endpoint and account credentials.
            var blobClient = new CloudBlobClient(csa.BlobStorageUri, sc);

            return blobClient.GetContainerReference(containerName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static CloudBlobContainer GetRootContainer(string account, string key)
        {
            StorageCredentials sc = new StorageCredentials(account, key);
            //Initializes a new instance of the CloudStorageAccount class using the specified credentials, and specifies whether to use HTTP or HTTPS to connect to the storage services. 
            CloudStorageAccount csa = new CloudStorageAccount(sc, true);

            //Initializes a new instance of the CloudBlobClient class using the specified Blob service endpoint and account credentials.
            var blobClient = new CloudBlobClient(csa.BlobStorageUri, sc);

            return blobClient.GetRootContainerReference();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="key"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static async Task DeleteAsync(string account, string key, string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                return;

            CloudBlobContainer container = GetContainer(account, key, containerName);

            try
            {
                if (container.Name == containerName)
                {
                    await container.DeleteAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception("7192 Error while Deleting Container", e);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="key"></param>
        /// <param name="containerName"></param>
        /// <param name="publicAccess"></param>
        /// <returns></returns>
        public static async Task CreateAsync(string account, string key, string containerName, bool publicAccess)
        {
            if (string.IsNullOrEmpty(containerName))
                return;

            StorageCredentials sc = new StorageCredentials(account, key);
            //Initializes a new instance of the CloudStorageAccount class using the specified credentials, and specifies whether to use HTTP or HTTPS to connect to the storage services. 
            CloudStorageAccount csa = new CloudStorageAccount(sc, true);

            //Initializes a new instance of the CloudBlobClient class using the specified Blob service endpoint and account credentials.
            var blobClient = new CloudBlobClient(csa.BlobStorageUri, sc);
            
            Uri uri = new Uri($"{blobClient.BaseUri}{containerName}");
            CloudBlobContainer cont = new CloudBlobContainer(uri, blobClient.Credentials);
            await cont.CreateAsync();

            if (publicAccess)
            {
                BlobContainerPermissions permissions =
                    new BlobContainerPermissions {PublicAccess = BlobContainerPublicAccessType.Container};

                await cont.SetPermissionsAsync(permissions);
            }
        }

        #endregion

        #region Blob



        #endregion
    }
}
