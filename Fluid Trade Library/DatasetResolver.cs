namespace FluidTrade.Core
{

	using System;
    using System.Data;
	using System.IO;
	using System.Xml;

	public class DatasetResolver : XmlResolver
	{

		// Public Instance Fields
		public System.Data.DataTable DataTable;
		public System.Data.DataView DataView;
		public System.Data.DataColumn XmlDocumentColumn;

		/// <summary>
		/// Creates an XML Resolver that will find XML streams in a table.
		/// </summary>
		/// <param name="dataTable"></param>
		public DatasetResolver(DataTable dataTable, DataView dataView)
		{

			// Initialize the object.
			this.DataTable = dataTable;
			this.DataView = dataView;
			this.XmlDocumentColumn = this.DataTable.Columns["XmlDocument"];

		}

		/// <summary>
		/// maps a URI to an object containing the actual resource
		/// </summary>
		/// <param name="absoluteUri">The URI returned from ResolveUri.</param>
		/// <param name="role">The current version does not use this parameter when resolving URIs. </param>
		/// <param name="ofObjectToReturn">The type of object to return.</param>
		/// <returns>A System.IO.Stream object or null if a type other than stream is specified.</returns>
		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
		{

			try
			{

				int index = this.DataView.Find(absoluteUri.AbsoluteUri);
				if (index != -1)
					return new StringReader(this.DataView[index].Row[this.XmlDocumentColumn] as string);

			}
			catch { }

			return null;

		}

		public override Uri ResolveUri(Uri baseUri, string relativeUri)
		{

			UriBuilder uriBuilder = new UriBuilder(relativeUri);
			uriBuilder.Host = string.Empty;
			uriBuilder.Scheme = "dataSet";
			uriBuilder.Path = relativeUri;
			return uriBuilder.Uri;

		}

		public override System.Net.ICredentials Credentials
		{
			set { throw new NotImplementedException(); }
		}

	}
}
