using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bau.Libraries.LibBlobStorage
{
	/// <summary>
	///		Interface para los manager de sistemas de storage para cloud
	/// </summary>
	public interface ICloudStorageManager : IDisposable
	{
		/// <summary>
		///		Crea un contenedor
		/// </summary>
		Task CreateContainerAsync(string container);

		/// <summary>
		///		Lista los contenedores
		/// </summary>
		Task<List<string>> ListContainersAsync(string prefix = null);

		/// <summary>
		///		Lista los archivos de un contenedor
		/// </summary>
		Task<List<Metadata.BlobModel>> ListBlobsAsync(string container, string prefix);

		/// <summary>
		///		Sube un archivo a un contenedor
		/// </summary>
		Task UploadAsync(string container, string blobFileName, string fileName, Dictionary<string, string> parameters = null);

		/// <summary>
		///		Sube un archivo a un contenedor utilizando un stream
		/// </summary>
		Task UploadAsync(string container, string blobFileName, Stream fileStream, Dictionary<string, string> parameters = null);

		/// <summary>
		///		Crea un archivo en un blob a partir de un texto
		/// </summary>
		Task UploadTextAsync(string container, string blobFileName, string text, System.Text.Encoding encoding = null, Dictionary<string, string> parameters = null);

		/// <summary>
		///		Descarga un archivo utilizando un stream
		/// </summary>
		Task DownloadAsync(string container, string blobFileName, Stream target);

		/// <summary>
		///		Descarga un archivo 
		/// </summary>
		Task DownloadAsync(string container, string blobFileName, string localFileName);

		/// <summary>
		///		Borra un contenedor
		/// </summary>
		Task DeleteAsync(string container);

		/// <summary>
		///		Borra un archivo
		/// </summary>
		Task DeleteAsync(string container, string blobFileName);

       /// <summary>
        ///		Obtiene los datos de un contenedor
        /// </summary>
        Task<Metadata.MetadataModel> GetMetadataAsync(string container);

       /// <summary>
        ///		Obtiene los datos de un blob
        /// </summary>
        Task<Metadata.MetadataBlobModel> GetMetadataAsync(string container, string blobFileName);

		/// <summary>
		///		Abre un stream de lectura sobre un blob
		/// </summary>
        Task<Stream> OpenReadAsync(string container, string blobFileName);

		/// <summary>
		///		Abre un stream de escritura sobre un blob
		/// </summary>
        Task<Stream> OpenWriteAsync(string container, string blobFileName, bool overwrite);

		/// <summary>
		///		Mueve un archivo de un contenedor a otro
		/// </summary>
		Task MoveAsync(string containerSource, string blobFileNameSource, string containerTarget, string blobFileNameTarget, 
					   System.Threading.CancellationToken? cancellationToken = null);

		/// <summary>
		///		Copia un archivo de un contenedor a otro
		/// </summary>
		Task CopyAsync(string containerSource, string blobFileNameSource, string containerTarget, string blobFileNameTarget, 
					   System.Threading.CancellationToken? cancellationToken = null);
	}
}