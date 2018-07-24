namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class ReadXmlMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public ReadXmlMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Reads an XML file into the data model.
			//		/// </summary>
			//		/// <param name="fileName">The name of the file to read.</param>
			//		public static void ReadXml(string fileName)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Reads an XML file into the data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"fileName\">The name of the file to read.</param>", true));
			this.Name = "ReadXml";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(String)), "fileName"));

			//			DataModel.dataSet.ReadXml(fileName);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(
						new CodeTypeReferenceExpression(dataModelSchema.Name),
						String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
					"ReadXml",
					new CodeArgumentReferenceExpression("fileName")));

			//		}

		}

	}
}
