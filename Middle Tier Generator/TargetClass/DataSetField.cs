namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a private field to hold the generic DataSet.
	/// </summary>
	public class DataSetField : CodeMemberField
	{

		/// <summary>
		/// Creates a private field to hold the generic DataSet.
		/// </summary>
		public DataSetField(DataModelSchema dataModelSchema)
		{

			//        // The generic data behind the strongly typed data model.
			//        private static global::System.Data.DataSet dataSet;
			this.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
			this.Type = new CodeTypeReference(string.Format("{0}DataSet", dataModelSchema.Name));
			this.Name = string.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name));

		}

	}

}
