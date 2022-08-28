using System;

namespace Bau.Libraries.LibBlobStorage
{
	/// <summary>
	///		Manager para operaciones sobre Azure blob storage
	/// </summary>
	public class StorageManager
	{
		/// <summary>
		///		Abre un storage blob de Azure
		/// </summary>
		public ICloudStorageManager OpenAzureStorageBlob(string storageConnectionString)
		{
			return new Storage.AzureStorageBlobManager(storageConnectionString);
		}
	}
}
