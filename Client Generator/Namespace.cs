namespace FluidTrade.ClientGenerator
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

	/// <summary>
	/// The root namespace FluidTrade.ClientGenerator the DataSet output file.
	/// </summary>
	public class Namespace : CodeNamespace
	{

		/// <summary>
		/// Creates a CodeDOM namespace FluidTrade.ClientGenerator contains the strongly typed DataSet.
		/// </summary>
		/// <param name="schema">The schema description of the strongly typed DataSet.</param>
		public Namespace(DataModelSchema dataModelSchema)
		{

			//namespace FluidTrade.ClientGenerator
			//{
			this.Name = dataModelSchema.TargetNamespace;

			// Delegates
			foreach (TableSchema tableSchema in dataModelSchema.Tables.Values)
				this.Types.Add(new RowChangeDelegate(tableSchema));

			// Classes
			foreach (TableSchema tableSchema in dataModelSchema.Tables.Values)
			{
				this.Types.Add(new TableClass.TableClass(tableSchema));
				this.Types.Add(new RowClass.RowClass(tableSchema));
				this.Types.Add(new FluidTrade.Core.ChangeEventArgsClass.ChangeEventArgsClass(tableSchema));
				this.Types.Add(new FluidTrade.Core.IndexInterface.IndexInterface(tableSchema));
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in tableSchema.Constraints)
					if (constraintPair.Value is UniqueConstraintSchema)
					{
						UniqueConstraintSchema uniqueConstraintSchema = constraintPair.Value as UniqueConstraintSchema;
						if (uniqueConstraintSchema.IsPrimaryKey)
							this.Types.Add(new ClusteredIndexClass.ClusteredIndexClass(uniqueConstraintSchema));
						else
							this.Types.Add(new NonClusteredIndexClass.NonClusteredIndexClass(uniqueConstraintSchema));
					}
			}
			this.Types.Add(new FluidTrade.ClientGenerator.TargetClass.TargetClass(dataModelSchema));

			//}

		}

	}

}
