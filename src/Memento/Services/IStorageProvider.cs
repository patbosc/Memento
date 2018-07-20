using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace Memento.Services
{
    public interface IStorageProvider
    {
        #region Container

        Task<List<CloudBlobContainer>> ListContainerAsync(string account, string key);
        
        #endregion
    }
}