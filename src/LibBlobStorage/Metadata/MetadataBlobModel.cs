using System;

namespace Bau.Libraries.LibBlobStorage.Metadata
{
	/// <summary>
	///		Metadatos de un blob
	/// </summary>
	public class MetadataBlobModel : MetadataModel
	{
		/// <summary>
		///		Tipo de blob
		/// </summary>
		public enum BlobType
		{
			/// <summary>No especificado</summary>
			Unspecified,
			/// <summary>Blob de página</summary>
			PageBlob,
			/// <summary>Blob de bloque</summary>
			BlockBlob,
			/// <summary>Blob para añadir</summary>
			AppendBlob
		}

		/// <summary>
		///		Tamaño del blob
		/// </summary>
		public long Length { get; set; }

		/// <summary>
		///		Nombre del contenedor
		/// </summary>
		public string ContainerName { get; set; }

		/// <summary>
		///		Tipo
		/// </summary>
		public BlobType Type { get; set; }

		/// <summary>
		///		Indica si se ha marcado como sellado el blob
		/// </summary>
		public bool IsSealed { get; set; }
	}
}
