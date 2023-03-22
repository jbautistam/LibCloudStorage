using System;

namespace Bau.Libraries.LibBlobStorage.Metadata
{
	/// <summary>
	///		Modelo de metadatos
	/// </summary>
	public class MetadataModel
	{
		/// <summary>
		///		Nombre
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		Url
		/// </summary>
		public Uri Uri { get; set; }

		/// <summary>
		///		Etiqueta
		/// </summary>
		public string ETag { get; set; }

		/// <summary>
		///		Fecha de última modificación
		/// </summary>
		public DateTimeOffset LastModified { get; set; }

		/// <summary>
		///		Propiedades
		/// </summary>
		public System.Collections.Generic.Dictionary<string, string> Properties { get; } = new System.Collections.Generic.Dictionary<string, string>();
	}
}
