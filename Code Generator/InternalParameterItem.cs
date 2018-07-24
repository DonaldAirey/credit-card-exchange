namespace FluidTrade.Core
{

	using System;

    /// <summary>
	/// A parameter that is mapped to the parameters of the internal methods.
	/// </summary>
	public class InternalParameterItem
	{

		// Public Instance Fields
		public System.Boolean IsAutoGuid;
		public String Name;
		public ColumnSchema ColumnSchema;
		public System.CodeDom.CodeExpression Expression;

		/// <summary>
		/// Create a parameter that is mapped to the internal parameters.
		/// </summary>
		public InternalParameterItem()
		{

			// Initialize the object.
			IsAutoGuid = false;

		}

	}

}
