namespace FluidTrade.Core
{

	using System;
    using System.IO;
	using System.Xml;

	/// <summary>
	/// Creates a resolver for file based URI references.
	/// </summary>
	public class FileResolver : XmlResolver
	{

		// Public Instance Fields
		public String path;

		/// <summary>
		/// Creates an XML Resolver that will find XML streams in a table.
		/// </summary>
		/// <param name="dataTable"></param>
		public FileResolver(String path)
		{

			// Initialize the object.
			this.path = path;

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

				// Open up a stream to the referenced file.
				return new StreamReader(absoluteUri.LocalPath);

			}
			catch { }

			// If we reached here, then the file couldn't be loaded and the URI is not resolved.
			return null;

		}

		/// <summary>
		/// Resolves references to the file system.
		/// </summary>
		/// <param name="baseUri"></param>
		/// <param name="relativeUri"></param>
		/// <returns>A URI to a local file built from the path information.</returns>
		public override Uri ResolveUri(Uri baseUri, string relativeUri)
		{

			string absolutePath = Path.Combine(this.path, relativeUri);
			UriBuilder uriBuilder = new UriBuilder(absolutePath);
			uriBuilder.Host = string.Empty;
			uriBuilder.Scheme = "file";
			uriBuilder.Path = absolutePath;
			return uriBuilder.Uri;

		}

		/// <summary>
		/// Sets the credentials required for secure operations (not used).
		/// </summary>
		public override System.Net.ICredentials Credentials
		{
			set { throw new NotImplementedException(); }
		}

	}
}
