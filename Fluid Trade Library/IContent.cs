namespace FluidTrade.Core
{


    /// <summary>
	/// This interface is used to identify and copy all content elements of the report.
	/// </summary>
	public interface IContent
	{

		/// <summary>
		/// Gets or sets the unique identifier of an element of the report.
		/// </summary>
		object Key { get; set; }

		/// <summary>
		/// Sorting value
		/// </summary>
		//object Value { get; set; }

		/// <summary>
		/// Copies content from the source into the target element of the report.
		/// </summary>
		/// <param name="iContent">The source of the content.</param>
		void Copy(IContent iContent);

	}
}
