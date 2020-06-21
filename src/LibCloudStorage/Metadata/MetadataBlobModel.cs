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
		///		Tamaño mínimo de lectura de un stream
		/// </summary>
		public int StreamMinimumReadSizeInBytes { get; set; }

		/// <summary>
		///		Nombre del contenedor
		/// </summary>
		public string ContainerName { get; set; }

		/// <summary>
		///		Tipo
		/// </summary>
		public BlobType Type { get; set; }

		/// <summary>
		///		Indica si es un snaphsot
		/// </summary>
		public bool IsSnaphot { get; set; }

		/// <summary>
		///		Hora en la que se tomó el snapshot
		/// </summary>
		public DateTimeOffset? SnapshotTime { get; set; }

		/// <summary>
		///		Uri del snapshot
		/// </summary>
		public Uri SnapshotQualifiedUri { get; set; }

		/// <summary>
		///		Indica si se ha borrado el blob
		/// </summary>
		public bool IsDeleted { get; set; }
	}
}
