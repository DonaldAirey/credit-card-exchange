namespace FluidTrade.Core
{
    using System;

    /// <summary>
	/// Summary description for Type.
	/// </summary>
	public class TypeSpecification
	{

		// Constants
		private const System.Int32 typeIndex = 0;
		private const System.Int32 assemblyIndex = 1;

		// Public Read Only
		public readonly String TypeName;
		public readonly String AssemblyName;

		/// <summary>
		/// Creates a specification from a standard string giving the type name and assembly.
		/// </summary>
		/// <param name="specificationString">A string specification for a type.</param>
		public TypeSpecification(string specificationString)
		{

			// Extract the assembly and type name from the single string.
			string[] typeParts = specificationString.Split(new char[] {','});
			this.TypeName = typeParts[TypeSpecification.typeIndex].Trim();
			this.AssemblyName = typeParts[TypeSpecification.assemblyIndex].Trim();

		}

	}

}
