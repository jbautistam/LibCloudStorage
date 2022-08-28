using System;

namespace Bau.Libraries.LibBlobStorage.Metadata
{
	/// <summary>
	///		Clase con los datos de un blob
	/// </summary>
	public class BlobModel
	{
		/// <summary>
		///		Contenedor
		/// </summary>
		public string Container { get; set; }

		/// <summary>
		///		Nombre de archivo
		/// </summary>
		public string FullFileName { get; set; }

		/// <summary>
		///		Tamaño del archivo
		/// </summary>
		public long Length { get; set; }

		/// <summary>
		///		Url
		/// </summary>
		public Uri Url { get; set; }

		/// <summary>
		///		Nombre del archivo local (sin el primer directorio)
		/// </summary>
		public string LocalFileName 
		{ 
			get
			{
				string[] parts = FullFileName.Split('/');
				string result = string.Empty;

					// Obtiene el nombre del archivo completo
					if (parts.Length > 1)
						for (int index = 1; index < parts.Length; index++)
						{
							// Añade el separador
							if (index > 1)
								result += '/';
							// Añade el nombre
							result += parts[index];
						}
					else
						result = FullFileName;
					// Devuelve el resultado
					return result;
			}
		}
	}
}
