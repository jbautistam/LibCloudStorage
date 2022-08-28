using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Bau.Libraries.LibBlobStorage;
using Bau.Libraries.LibBlobStorage.Metadata;

namespace TestLibBlobStorage
{
	class Program
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
		static async Task Main(string[] args)
		{
			string storageConnection = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";

				try
				{
					// Crea el manager
					using (ICloudStorageManager manager = new StorageManager().OpenAzureStorageBlob(storageConnection))
					{
						string container = "firstfolder";
						string sourceFileName = System.IO.Path.Combine(PathData, "ChartSample.pdf");
						string targetFileName = System.IO.Path.Combine(PathData, "ChartSample.download.pdf");
						string blobFileName = System.IO.Path.GetFileName(sourceFileName);

							// Borra el archivo de descarga
							KillFile(targetFileName);
							// Sube el archivo
							Console.WriteLine("Subiendo archivo ...");
							await manager.UploadAsync(container, blobFileName, sourceFileName, GetParameters());
							// Sube una serie de archivos
							Console.WriteLine("Subiendo otros archivos ...");
							for (int index = 0; index < 5; index++)
								await manager.UploadAsync(container, blobFileName + Guid.NewGuid().ToString(), sourceFileName);
							// Descarga el archivo
							Console.WriteLine("Descargando archivo ...");
							await manager.DownloadAsync(container, blobFileName, targetFileName);
							if (CompareFile(sourceFileName, targetFileName))
								Console.WriteLine("El archivo descargado es igual que el subido");
							else
								Console.WriteLine("El archivo descargado no coincide con el subido");
							// Muestra las propiedades de contenedor y archivo
							ShowMetadata(await manager.GetMetadataAsync(container));
							ShowMetadata(await manager.GetMetadataAsync(container, blobFileName));
							// Obtiene los contenedores
							Console.WriteLine("Lista de contenedores");
							foreach (string item in await manager.ListContainersAsync(string.Empty))
								Console.WriteLine($"\t{item}");
							// Obtiene los archivos
							Console.WriteLine("Lista de archivos");
							foreach (BlobModel item in await manager.ListBlobsAsync(container, string.Empty))
								Console.WriteLine($"\t{item.FullFileName}");
							// Borra el archivo subido
							Console.WriteLine("Eliminando el blob ...");
							await manager.DeleteAsync(container, blobFileName);
							// Borra el contenedor
							Console.WriteLine("Eliminando el contenedor ...");
							await manager.DeleteAsync(container);
				}
				}
				catch (Exception exception)
				{
					Console.WriteLine("Error: " + exception.Message);
				}
				Console.WriteLine("Pulse una tecla para terminar ...");
				Console.ReadLine();
		}

		/// <summary>
		///		Obtiene una serie de parámetros
		/// </summary>
		private static Dictionary<string, string> GetParameters()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();

				// Añade parámetros
				for (int index = 0; index < 10; index++)
					parameters.Add($"Key{index}", index.ToString());
				for (int index = 0; index < 10; index++)
					parameters.Add($"Date{index}", DateTime.Now.AddDays(index).ToString());
				// Devuelve la colección de parámetros
				return parameters;
		}

		/// <summary>
		///		Muestra los metadatos de un contenedor
		/// </summary>
		private static void ShowMetadata(MetadataModel metadata)
		{
			Console.WriteLine($"Nombre: {metadata.Name}");
			Console.WriteLine($"Uri: {metadata.Uri}");
			Console.WriteLine($"Etag: {metadata.ETag}");
			Console.WriteLine($"LastModified: {metadata.LastModified}");
			ShowMetadataProperties(metadata.Properties);
		}

		/// <summary>
		///		Muestra los metadatos de un blob
		/// </summary>
		private static void ShowMetadata(MetadataBlobModel metadata)
		{
			Console.WriteLine($"Nombre: {metadata.Name}");
			Console.WriteLine($"Uri: {metadata.Uri}");
			Console.WriteLine($"Length: {metadata.Length}");
			Console.WriteLine($"Etag: {metadata.ETag}");
			Console.WriteLine($"LastModified: {metadata.LastModified}");
			ShowMetadataProperties(metadata.Properties);
		}

		/// <summary>
		///		Muestra las propiedades de los metadatos
		/// </summary>
		private static void ShowMetadataProperties(Dictionary<string, string> properties)
		{
			if (properties.Count == 0)
				Console.WriteLine("Sin propiedades");
			else
				foreach (KeyValuePair<string, string> property in properties)
					Console.WriteLine($"Key: {property.Key} - Value: {property.Value}");
		}

		/// <summary>
		///		Compara dos archivos
		/// </summary>
		private static bool CompareFile(string source, string target)
		{
			if (!System.IO.File.Exists(target))
				return false;
			else
			{
				System.IO.FileInfo fileSource = new System.IO.FileInfo(source);
				System.IO.FileInfo fileTarget = new System.IO.FileInfo(target);

					return fileSource.Length == fileTarget.Length;
			}
		}

		/// <summary>
		///		Borra el archivo
		/// </summary>
		private static void KillFile(string fileName)
		{
			try
			{
				System.IO.File.Delete(fileName);
			}
			catch {}
		}

		/// <summary>
		///		Directorio de datos
		/// </summary>
		private static string PathData
		{
			get { return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Data"); }
		}
	}
}
