namespace FluidTrade.MiddleTierGenerator
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

	/// <summary>
	/// The root namespace FluidTrade.MiddleTierGenerator the DataSet output file.
	/// </summary>
	public class Namespace : CodeNamespace
	{

		/// <summary>
		/// Creates a CodeDOM namespace FluidTrade.MiddleTierGenerator contains the strongly typed DataSet.
		/// </summary>
		/// <param name="schema">The schema description of the strongly typed DataSet.</param>
		public Namespace(DataModelSchema dataModelSchema)
		{

			//namespace FluidTrade.MiddleTierGenerator
			//{
			this.Name = dataModelSchema.TargetNamespace;

			// Delegates
			List<CodeTypeDelegate> delegateList = new List<CodeTypeDelegate>();
			delegateList.Add(new FilterContextDelegate(dataModelSchema));
			foreach (TableSchema tableSchema in dataModelSchema.Tables.Values)
				delegateList.Add(new RowChangeDelegate(tableSchema));
			delegateList.Sort(
				delegate(CodeTypeDelegate firstDelegate, CodeTypeDelegate secondDelegate)
				{ return firstDelegate.Name.CompareTo(secondDelegate.Name); });
			foreach (CodeTypeDelegate codeTypeDelegate in delegateList)
				this.Types.Add(codeTypeDelegate);

			// Types
			List<CodeTypeDeclaration> typeList = new List<CodeTypeDeclaration>();
			typeList.Add(new DataSetClass.DataSetClass(dataModelSchema));
			typeList.Add(new FieldCollectorClass.FieldCollectorClass(dataModelSchema));
			//typeList.Add(new FluidTrade.Core.RowInterface.RowInterface(dataModelSchema));
			//typeList.Add(new FluidTrade.Core.TableInterface.TableInterface(dataModelSchema));
			typeList.Add(new TargetClass.TargetClass(dataModelSchema));
			//typeList.Add(new TransactionClass.TransactionClass(dataModelSchema));
			typeList.Add(new TransactionLogItemClass.TransactionLogItemClass(dataModelSchema));
			foreach (TableSchema tableSchema in dataModelSchema.Tables.Values)
			{
				typeList.Add(new RowClass.RowClass(tableSchema));
				typeList.Add(new TableClass.TableClass(tableSchema));
				typeList.Add(new FluidTrade.Core.ChangeEventArgsClass.ChangeEventArgsClass(tableSchema));
				typeList.Add(new FluidTrade.Core.IndexInterface.IndexInterface(tableSchema));
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in tableSchema.Constraints)
					if (constraintPair.Value is UniqueConstraintSchema)
					{
						UniqueConstraintSchema uniqueConstraintSchema = constraintPair.Value as UniqueConstraintSchema;
						if (uniqueConstraintSchema.IsPrimaryKey)
							typeList.Add(new ClusteredIndexClass.ClusteredIndexClass(uniqueConstraintSchema));
						else
							typeList.Add(new NonClusteredIndexClass.NonClusteredIndexClass(uniqueConstraintSchema));
					}
			}
			typeList.Sort(
				delegate(CodeTypeDeclaration firstDeclaration, CodeTypeDeclaration secondDeclaration)
				{ return firstDeclaration.Name.CompareTo(secondDeclaration.Name); });
			foreach (CodeTypeDeclaration codeTypeDeclaration in typeList)
				this.Types.Add(codeTypeDeclaration);

			//}

		}

	}

}
