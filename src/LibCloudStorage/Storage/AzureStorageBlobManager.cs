using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

using Bau.Libraries.LibBlobStorage.Metadata;

namespace Bau.Libraries.LibBlobStorage.Storage
{
	/// <summary>
	///		Manager para blob storage de Azure
	/// </summary>
	public class AzureStorageBlobManager : ICloudStorageManager
	{
		/*
			Azure storage emulator
				Inicializar azure storage contra el servidor local:
					AzureStorageEmulator.exe init /server .
			Cuentas del emulador
					Account name: devstoreaccount1
					Account key: Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==
			Cadena de conexión del emulador
					DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;
					AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;
					BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
					TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;
					QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;
		*/

		/// <summary>
		///		Abre una cuenta de storage
		/// </summary>
		internal bool Open(string storageConnectionString)
		{
			bool opened = false;

				// Crea la cuenta de storage
				if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
				{
					// Guarda la cuenta de storage
					StorageAccount = storageAccount;
					// Indica que se ha abierto correctamente
					opened = true;
				}
				// Devuelve el valor que indica si la cuenta se ha abierto correctamente
				return opened;
		}

		/// <summary>
		///		Obtiene el cliente de Azure Storage
		/// </summary>
		private CloudBlobClient GetClient()
		{
			return StorageAccount.CreateCloudBlobClient();
		}

		/// <summary>
		///		Obtiene un contenedor (lo crea si es necesario)
		/// </summary>
		private async Task<CloudBlobContainer> GetContainerAsync(string container)
		{
			CloudBlobContainer cloudBlobContainer = GetClient().GetContainerReference(container.ToLowerInvariant());

				// Crea el contenedor
				await cloudBlobContainer.CreateIfNotExistsAsync();
				// Devuelve el contenedor
				return cloudBlobContainer;
		}

		/// <summary>
		///		Obtiene la referencia a un blob
		/// </summary>
		private async Task<CloudBlockBlob> GetBlobAsync(string container, string blobFileName)
		{
			return (await GetContainerAsync(container)).GetBlockBlobReference(blobFileName);
		}

		/// <summary>
		///		Obtiene la referencia a un blob (y le asigna parámetros)
		/// </summary>
		private async Task<CloudBlockBlob> GetBlobAsync(string container, string blobFileName, Dictionary<string, string> parameters)
		{
			CloudBlockBlob blob = await GetBlobAsync(container, blobFileName);

				// Asigna las propiedades
				if (parameters != null)
					foreach (KeyValuePair<string, string> parameter in parameters)
						blob.Metadata.Add(parameter.Key, parameter.Value);
				// Devuelve el blob
				return blob;
		}

		/// <summary>
		///		Crea un contenedor (llama a <see cref="GetContainerAsync(string)"/> pero no devuelve nada porque la interface es común para todos
		///	los posibles storage)
		/// </summary>
		public async Task CreateContainerAsync(string container)
		{
			await GetContainerAsync(container);
		}

		/// <summary>
		///		Sube un archivo
		/// </summary>
		public async Task UploadAsync(string container, string blobFileName, string localFileName, Dictionary<string, string> parameters = null)
		{
			await (await GetBlobAsync(container, blobFileName, parameters)).UploadFromFileAsync(localFileName);
		}

		/// <summary>
		///		Sube un archivo desde un stream
		/// </summary>
		public async Task UploadAsync(string container, string blobFileName, System.IO.Stream fileStream, Dictionary<string, string> parameters = null)
		{
			await (await GetBlobAsync(container, blobFileName, parameters)).UploadFromStreamAsync(fileStream);
		}

		/// <summary>
		///		Lista los contenedores
		/// </summary>
		public async Task<List<string>> ListContainersAsync(string prefix)
		{
			BlobContinuationToken continuationToken = null;
			List<string> containers = new List<string>();

				// Obtiene los contenedores
				do
				{
					ContainerResultSegment resultSegment = await GetClient().ListContainersSegmentedAsync(prefix, ContainerListingDetails.Metadata, 100, 
																										  continuationToken, null, null);

						// Enumera los contenedores
						foreach (CloudBlobContainer container in resultSegment.Results)
							containers.Add(container.Name);
						// Obtiene el token de siguiente segmento
						continuationToken = resultSegment.ContinuationToken;
				} 
				while (continuationToken != null);
				// Devuelve la lista de contenedores
				return containers;
		}

		/// <summary>
		///		Lista los archivos de un contenedor
		/// </summary>
		public async Task<List<BlobModel>> ListBlobsAsync(string container, string prefix)
		{
			BlobContinuationToken blobContinuationToken = null;
			List<BlobModel> items = new List<BlobModel>();

				// Obtiene los archivos de un contenedor
				do
				{
					BlobResultSegment segment = await (await GetContainerAsync(container)).ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.None, 200, blobContinuationToken, null, null, System.Threading.CancellationToken.None);

						// Obtiene el token de continuación (para continuar con el listado)
						blobContinuationToken = segment.ContinuationToken;
						// Añade los elementos a la lista
						foreach (IListBlobItem item in segment.Results)
							switch (item)
							{
								case CloudBlob blob:
										items.Add(new BlobModel
															{
																Container = container,
																FullFileName = blob.Name,
																Length = blob.Properties.Length,
																Url = item.Uri
															}
												 );
									break;
							}
				}
				while (blobContinuationToken != null);
				// Devuelve la colección de archivos
				return items;
		}

		/// <summary>
		///		Descarga un archivo
		/// </summary>
		public async Task DownloadAsync(string container, string blobFileName, string localFileName)
		{
			await (await GetBlobAsync(container, blobFileName)).DownloadToFileAsync(localFileName, System.IO.FileMode.Create);
		}

		/// <summary>
		///		Descarga un archivo
		/// </summary>
		public async Task DownloadAsync(string container, string blobFileName, System.IO.Stream target)
		{
			await (await GetBlobAsync(container, blobFileName)).DownloadToStreamAsync(target);
		}

		/// <summary>
		///		Borra un archivo
		/// </summary>
		public async Task DeleteAsync(string container, string blobFileName)
		{
			await (await GetBlobAsync(container, blobFileName)).DeleteIfExistsAsync();
		}

		/// <summary>
		///		Borra un contenedor
		/// </summary>
		public async Task DeleteAsync(string container)
		{
			await (await GetContainerAsync(container)).DeleteIfExistsAsync();
		}

       /// <summary>
        ///		Obtiene los datos de un contenedor
        /// </summary>
        public async Task<MetadataModel> GetMetadataAsync(string container)
        {
			MetadataModel metadata = new MetadataModel();
			CloudBlobContainer cloudBlobContainer = await GetContainerAsync(container);

				// Captura los atributos
				await cloudBlobContainer.FetchAttributesAsync();
				// Asigna los metadatos
				metadata.Name = cloudBlobContainer.Name;
				metadata.Uri = cloudBlobContainer.Uri;
				metadata.ETag = cloudBlobContainer.Properties.ETag;
				metadata.LastModified = cloudBlobContainer.Properties.LastModified ?? DateTimeOffset.UtcNow;
				// Asigna las propiedades
				foreach (KeyValuePair<string, string> item in cloudBlobContainer.Metadata)
					metadata.Properties.Add(item.Key, item.Value);
				// Devuelve los metadatos
				return metadata;
        }

		/// <summary>
        ///		Obtiene los metadatos de un blob
        /// </summary>
        public async Task<MetadataBlobModel> GetMetadataAsync(string container, string blobFileName)
        {
			MetadataBlobModel metadata = new MetadataBlobModel();
			CloudBlob blob = await GetBlobAsync(container, blobFileName);

				// Captura los atributos
				await blob.FetchAttributesAsync();
				// Asigna los metadatos
				metadata.Name = blob.Name;
				metadata.Uri = blob.Uri;
				metadata.ETag = blob.Properties.ETag;
				metadata.LastModified = blob.Properties.LastModified ?? DateTime.UtcNow;
				metadata.Length = blob.Properties.Length;
				metadata.StreamMinimumReadSizeInBytes = blob.StreamMinimumReadSizeInBytes;
				metadata.ContainerName = blob.Container.Name;
				metadata.Type = Convert(blob.Properties.BlobType);
				metadata.IsSnaphot = blob.IsSnapshot;
				if (blob.IsSnapshot)
				{
					metadata.SnapshotTime = blob.SnapshotTime;
					metadata.SnapshotQualifiedUri = blob.SnapshotQualifiedUri;
				}
				metadata.IsDeleted = blob.IsDeleted;
				// Añade las propiedades del blob
				foreach (KeyValuePair<string, string> item in blob.Metadata)
					metadata.Properties.Add(item.Key, item.Value);
				// Devuelve el objeto
				return metadata;

            //Console.WriteLine("\t LeaseState: {0}", blob.Properties.LeaseState);
            //// If the blob has been leased, write out lease properties.
            //if (blob.Properties.LeaseState != LeaseState.Available)
            //{
            //    Console.WriteLine("\t LeaseDuration: {0}", blob.Properties.LeaseDuration);
            //    Console.WriteLine("\t LeaseStatus: {0}", blob.Properties.LeaseStatus);
            //}
            //Console.WriteLine("\t CacheControl: {0}", blob.Properties.CacheControl);
            //Console.WriteLine("\t ContentDisposition: {0}", blob.Properties.ContentDisposition);
            //Console.WriteLine("\t ContentEncoding: {0}", blob.Properties.ContentEncoding);
            //Console.WriteLine("\t ContentLanguage: {0}", blob.Properties.ContentLanguage);
            //Console.WriteLine("\t ContentMD5: {0}", blob.Properties.ContentMD5);
            //Console.WriteLine("\t ContentType: {0}", blob.Properties.ContentType);
            // Write out properties specific to blob type.
            //switch (blob.BlobType)
            //{
            //    case BlobType.AppendBlob:
            //        CloudAppendBlob appendBlob = blob as CloudAppendBlob;
            //        Console.WriteLine("\t AppendBlobCommittedBlockCount: {0}", appendBlob.Properties.AppendBlobCommittedBlockCount);
            //        Console.WriteLine("\t StreamWriteSizeInBytes: {0}", appendBlob.StreamWriteSizeInBytes);
            //        break;
            //    case BlobType.BlockBlob:
            //        CloudBlockBlob blockBlob = blob as CloudBlockBlob;
            //        Console.WriteLine("\t StreamWriteSizeInBytes: {0}", blockBlob.StreamWriteSizeInBytes);
            //        break;
            //    case BlobType.PageBlob:
            //        CloudPageBlob pageBlob = blob as CloudPageBlob;
            //        Console.WriteLine("\t PageBlobSequenceNumber: {0}", pageBlob.Properties.PageBlobSequenceNumber);
            //        Console.WriteLine("\t StreamWriteSizeInBytes: {0}", pageBlob.StreamWriteSizeInBytes);
            //        break;
            //    default:
            //        break;
        }

		/// <summary>
		///		Convierte el tipo de blob
		/// </summary>
		private MetadataBlobModel.BlobType Convert(BlobType blobType)
		{
			switch (blobType)
			{
				case BlobType.BlockBlob:
					return MetadataBlobModel.BlobType.BlockBlob;
				case BlobType.PageBlob:
					return MetadataBlobModel.BlobType.PageBlob;
				case BlobType.AppendBlob:
					return MetadataBlobModel.BlobType.AppendBlob;
				default:
					return MetadataBlobModel.BlobType.Unspecified;
			}
		}

		/// <summary>
		///		Mueve un archivo de un contenedor a otro
		/// </summary>
		public async Task MoveAsync(string containerSource, string blobFileNameSource, string containerTarget, string blobFileNameTarget, 
									System.Threading.CancellationToken? cancellationToken = null)
		{
			System.Threading.CancellationToken cancellation = cancellationToken ?? new System.Threading.CancellationToken();
			CloudBlockBlob source = await GetBlobAsync(containerSource, blobFileNameSource);
			CloudBlockBlob target = await GetBlobAsync(containerTarget, blobFileNameTarget);

				// Copia el blob de origen al contenedor destino
				await target.StartCopyAsync(source, cancellation);
				// Elimina el blob origen
				await source.DeleteAsync(cancellation);
		}

		/// <summary>
		///		Mueve un archivo de un contenedor a otro
		/// </summary>
		public async Task CopyAsync(string containerSource, string blobFileNameSource, string containerTarget, string blobFileNameTarget, 
									System.Threading.CancellationToken? cancellationToken = null)
		{
			System.Threading.CancellationToken cancellation = cancellationToken ?? new System.Threading.CancellationToken();
			CloudBlockBlob source = await GetBlobAsync(containerSource, blobFileNameSource);
			CloudBlockBlob target = await GetBlobAsync(containerTarget, blobFileNameTarget);

				// Copia el blob de origen al contenedor destino
				await target.StartCopyAsync(source, cancellation);
		}

		/// <summary>
		///		Cierra una cuenta de storage
		/// </summary>
		internal void Close()
		{
			if (StorageAccount != null)
				StorageAccount = null;
		}

		/// <summary>
		///		Libera el objeto
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				// Libera la memoria
				if (disposing)
					Close();
				// Indica que se ha liberado la memoria
				Disposed = true;
			}
		}

		/// <summary>
		///		Libera el objeto
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///		Cuenta de almacenamiento
		/// </summary>
		private CloudStorageAccount StorageAccount { get; set; }

		/// <summary>
		///		Indica si se ha liberado
		/// </summary>
		public bool Disposed { get; private set; }
	}
}
