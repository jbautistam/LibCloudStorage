using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

using Bau.Libraries.LibBlobStorage.Metadata;

namespace Bau.Libraries.LibBlobStorage
{
    /// <summary>
    ///		Manager para blob storage de Azure
    /// </summary>
    public class AzureStorageBlobManager : IDisposable
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
        // Variables privadas
        private BlobServiceClient _serviceClient;

        public AzureStorageBlobManager(string storageConnectionString)
        {
            StorageConnectionString = storageConnectionString;
        }

        /// <summary>
        ///		Obtiene un contenedor (lo crea si es necesario)
        /// </summary>
        private async Task<BlobContainerClient> GetContainerAsync(string container)
        {
            BlobContainerClient blobContainerClient = ServiceClient.GetBlobContainerClient(container.ToLowerInvariant());

                // Crea el contenedor
                await blobContainerClient.CreateIfNotExistsAsync();
                // Devuelve el contenedor
                return blobContainerClient;
        }

        /// <summary>
        ///		Obtiene la referencia a un blob
        /// </summary>
        private async Task<BlobClient> GetBlobClientAsync(string container, string blobFileName)
        {
            return (await GetContainerAsync(container)).GetBlobClient(blobFileName);
        }

        /// <summary>
        ///		Obtiene la referencia a un blob (y le asigna parámetros)
        /// </summary>
        private async Task<BlobClient> GetBlobAsync(string container, string blobFileName, Dictionary<string, string> parameters = null)
        {
            BlobClient blob = await GetBlobClientAsync(container, blobFileName);

                // Asigna las propiedades
                if (parameters != null)
                    foreach (KeyValuePair<string, string> parameter in parameters)
                    {
                        BlobProperties properties = await blob.GetPropertiesAsync();

                            if (properties != null && properties.Metadata != null)
                                properties.Metadata.Add(parameter.Key, parameter.Value);
                    }
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
            await (await GetBlobAsync(container, blobFileName, parameters)).UploadAsync(localFileName);
        }

        /// <summary>
        ///		Sube un archivo desde un stream
        /// </summary>
        public async Task UploadAsync(string container, string blobFileName, System.IO.Stream fileStream, Dictionary<string, string> parameters = null)
        {
            await (await GetBlobAsync(container, blobFileName, parameters)).UploadAsync(fileStream);
        }

        /// <summary>
        ///		Crea un archivo en un blob a partir de un texto
        /// </summary>
        public async Task UploadTextAsync(string container, string blobFileName, string text, System.Text.Encoding encoding = null, Dictionary<string, string> parameters = null)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream((encoding ?? System.Text.Encoding.UTF8).GetBytes(text)))
            {
                await UploadAsync(container, blobFileName, stream, parameters);
            }
        }

        /// <summary>
        ///		Lista los contenedores
        /// </summary>
        public async Task<List<string>> ListContainersAsync(string prefix = null)
        {
            List<string> containers = new List<string>();

                // Carga los contenedores
                await foreach (BlobContainerItem container in ServiceClient.GetBlobContainersAsync(prefix: prefix))
                    containers.Add(container.Name);
                // Devuelve la lista de contenedores
                return containers;
        }

        /// <summary>
        ///		Lista los archivos de un contenedor
        /// </summary>
        public async Task<List<BlobModel>> ListBlobsAsync(string container, string prefix)
        {
            BlobContainerClient containerClient = await GetContainerAsync(container);
            List<BlobModel> items = new List<BlobModel>();

                // Obtiene los archivos de un contenedor
                await foreach (BlobItem blob in containerClient.GetBlobsAsync())
                    if (string.IsNullOrWhiteSpace(prefix) || blob.Name.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                    {
                        BlobClient blobClient = await GetBlobClientAsync(container, blob.Name);

                            // Añade los datos del blob
                            items.Add(new BlobModel
                                                {
                                                    Container = container,
                                                    FullFileName = blob.Name,
                                                    Length = blob.Properties.ContentLength ?? 0,
                                                    Url = blobClient.Uri
                                                }
                                     );
                    }
                // Devuelve la colección de archivos
                return items;
        }

        /// <summary>
        ///		Descarga un archivo
        /// </summary>
        public async Task DownloadAsync(string container, string blobFileName, string localFileName)
        {
            await (await GetBlobClientAsync(container, blobFileName)).DownloadToAsync(localFileName);
        }

        /// <summary>
        ///		Descarga un archivo
        /// </summary>
        public async Task DownloadAsync(string container, string blobFileName, System.IO.Stream target)
        {
            await (await GetBlobClientAsync(container, blobFileName)).DownloadToAsync(target);
        }

        /// <summary>
        ///		Borra un archivo
        /// </summary>
        public async Task DeleteAsync(string container, string blobFileName)
        {
            await (await GetBlobClientAsync(container, blobFileName)).DeleteIfExistsAsync();
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
            BlobContainerClient cloudBlobContainer = await GetContainerAsync(container);
            BlobContainerProperties properties = await cloudBlobContainer.GetPropertiesAsync();

                // Asigna los metadatos
                metadata.Name = cloudBlobContainer.Name;
                metadata.Uri = cloudBlobContainer.Uri;
                metadata.ETag = properties.ETag.ToString();
                metadata.LastModified = properties.LastModified;
                // Asigna las propiedades
                foreach (KeyValuePair<string, string> item in properties.Metadata)
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
            BlobClient blobClient = await GetBlobClientAsync(container, blobFileName);
            BlobProperties properties = await blobClient.GetPropertiesAsync();

                // Asigna los metadatos
                metadata.Name = blobClient.Name;
                metadata.Uri = blobClient.Uri;
                metadata.ETag = properties.ETag.ToString();
                metadata.LastModified = properties.LastModified;
                metadata.Length = properties.ContentLength;
                metadata.ContainerName = container;
                metadata.Type = Convert(properties.BlobType);
                metadata.IsSealed = properties.IsSealed;
                // Añade las propiedades del blob
                foreach (KeyValuePair<string, string> item in properties.Metadata)
                    metadata.Properties.Add(item.Key, item.Value);
                // Devuelve el objeto
                return metadata;
        }

        /// <summary>
        ///		Convierte el tipo de blob
        /// </summary>
        private MetadataBlobModel.BlobType Convert(BlobType blobType)
        {
            switch (blobType)
            {
                case BlobType.Block:
                    return MetadataBlobModel.BlobType.BlockBlob;
                case BlobType.Page:
                    return MetadataBlobModel.BlobType.PageBlob;
                case BlobType.Append:
                    return MetadataBlobModel.BlobType.AppendBlob;
                default:
                    return MetadataBlobModel.BlobType.Unspecified;
            }
        }

        /// <summary>
        ///		Mueve un archivo de un contenedor a otro
        /// </summary>
        public async Task MoveAsync(string containerSource, string blobFileNameSource, string containerTarget, string blobFileNameTarget,
                                    CancellationToken? cancellationToken = null)
        {
            // Copia el blob de origen al contenedor destino
            await CopyAsync(containerSource, blobFileNameSource, containerTarget, blobFileNameTarget, cancellationToken);
            // Elimina el blob origen
            await DeleteAsync(containerSource, blobFileNameSource);
        }

        /// <summary>
        ///		Copia un archivo de un contenedor a otro
        /// </summary>
        public async Task CopyAsync(string containerSource, string blobFileNameSource, string containerTarget, string blobFileNameTarget,
                                    CancellationToken? cancellationToken = null)
        {
            CancellationToken cancellation = cancellationToken ?? new CancellationToken();
            BlobClient source = await GetBlobClientAsync(containerSource, blobFileNameSource);
            BlobClient target = await GetBlobClientAsync(containerTarget, blobFileNameTarget);

                // Copia el blob de origen al contenedor destino
                await target.StartCopyFromUriAsync(source.Uri, new BlobCopyFromUriOptions(), cancellation);
        }

        /// <summary>
        ///		Abre un stream de lectura sobre un blob
        /// </summary>
        public async Task<System.IO.Stream> OpenReadAsync(string container, string blobFileName, CancellationToken cancellationToken)
        {
            return await (await GetBlobClientAsync(container, blobFileName)).OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken);
        }

        /// <summary>
        ///		Abre un stream de escritura sobre un blob
        /// </summary>
        public async Task<System.IO.Stream> OpenWriteAsync(string container, string blobFileName, bool overwrite, CancellationToken cancellationToken)
        {
            BlockBlobClient blockBlobClient = new BlockBlobClient(StorageConnectionString, container, blobFileName);

                // Abre un stream de escritura
                return await blockBlobClient.OpenWriteAsync(overwrite, null, cancellationToken);
        }

        /// <summary>
        ///		Cierra una cuenta de storage
        /// </summary>
        internal void Close()
        {
            if (ServiceClient != null)
                ServiceClient = null;
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
        ///		Cadena de conexión al storage
        /// </summary>
        public string StorageConnectionString { get; }

        /// <summary>
        ///		Cliente del servicio
        /// </summary>
        private BlobServiceClient ServiceClient
        {
            get
            {
                // Crea el cliente si no existía
                if (_serviceClient == null)
                    _serviceClient = new BlobServiceClient(StorageConnectionString);
                // Devuelve el cliente
                return _serviceClient;
            }
            set { _serviceClient = value; }
        }

        /// <summary>
        ///		Indica si se ha liberado
        /// </summary>
        public bool Disposed { get; private set; }
    }
}