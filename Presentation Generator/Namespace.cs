namespace FluidTrade.PresentationGenerator
{

    using System.CodeDom;

	/// <summary>
	/// Generates the namespace used by the Presentation classes.
	/// </summary>
	public class Namespace : CodeNamespace
	{

		/// <summary>
		/// Generates the namespace used by the Presentation classes.
		/// </summary>
		/// <param name="presentationSchema">A description of the presentation layer.</param>
		public Namespace(PresentationSchema presentationSchema)
		{

			//namespace FluidTrade.Sandbox.WorkingOrderHeader
			//{
			this.Name = presentationSchema.TargetNamespace;

			//	Import Namespaces
			foreach (string import in presentationSchema.Imports)
				this.Imports.Add(new CodeNamespaceImport(import));

			// Classes
			foreach (ClassSchema classSchema in presentationSchema.Classes)
				if (classSchema.TargetNamespace == this.Name)
					this.Types.Add(new PresentationClass.PresentationClass(classSchema));

			//}

		}

	}

}
