namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// A table of viewer information that can be accessed using the object type with which the viewer is associated.
	/// </summary>
	public class ToolSection
	{

		// Private Members
		private ArrayList arrayList;

		/// <summary>
		/// Create a new section for the tools.
		/// </summary>
		public ToolSection() {this.arrayList = new ArrayList();}

		/// <summary>
		/// Adds a ToolInfo item to the list.
		/// </summary>
		/// <param name="key">The name of the persistent store.</param>
		/// <param name="persistentStoreInfo">The data related to managing a persistent store.</param>
		public ToolInfo Add(ToolInfo viewerInfo) {this.arrayList.Add(viewerInfo); return viewerInfo;}

		/// <summary>
		/// Enumerates through a collection of tools.
		/// </summary>
		/// <returns>An enumerator for a collection of tools.</returns>
		public IEnumerator GetEnumerator() {return this.arrayList.GetEnumerator();}
		
	}

}
