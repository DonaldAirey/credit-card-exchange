namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Represents a standard collection of custom attributes for properties.
	/// </summary>
	public class CodeCustomAttributesForMethods : CodeAttributeDeclarationCollection
	{

		/// <summary>
		/// Creates a standard collection of custom attributes for properties.
		/// </summary>
		public CodeCustomAttributesForMethods()
		{

			//        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            // HACK - Put this line back in for official releases
            //this.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.Diagnostics.DebuggerNonUserCodeAttribute))));

		}

	}

}
