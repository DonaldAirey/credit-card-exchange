namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
    using System.Xml.Schema;

	/// <summary>
	/// Describes a constraint on a table in a data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ConstraintSchema
	{

		// Private Instance Fields
		private ColumnSchema[] columns;
		private TableSchema table;
		private System.Boolean isNullable;
		private String name;

		/// <summary>
		/// Create a description of a constraint on a table.
		/// </summary>
		/// <param name="schema">The Schema of the entire data model.</param>
		/// <param name="xmlSchemaIdentityConstraint">The schema of a constraint.</param>
		public ConstraintSchema(DataModelSchema dataModelSchema, XmlSchemaIdentityConstraint xmlSchemaIdentityConstraint)
		{

			// Initialize the object.
			this.name = xmlSchemaIdentityConstraint.Name;

			// Pull apart the selector and construct a table name from the XPath specification.  Since we flatten out the 
			// hierarchy of the XmlSchema, there is no need to navigate a tree of nodes.  Only the table name is important for the
			// selector and there are no namespaces to be sorted out.
			string[] selectorXPath = xmlSchemaIdentityConstraint.Selector.XPath.Split(':');
			string tableName = selectorXPath.Length == 1 ? selectorXPath[0] : selectorXPath[1];
			tableName = tableName.Replace(".", string.Empty);
			tableName = tableName.Replace("/", string.Empty);
			this.table = dataModelSchema.Tables[tableName];

			// The fields of the constraint are collected in this object.
			List<ColumnSchema> columnList = new List<ColumnSchema>();

			// The raw schema contains the qualified names of all the columns in this constraint.  The tables are defined before 
			// the constraints are evaluated, so it's safe to reference the table and its columns while collecting the fields in
			// the constraint.
			foreach (XmlSchemaXPath xmlSchemaXPath in xmlSchemaIdentityConstraint.Fields)
			{

				// Pull apart the field and construct a qualified name from the XPath specification.  The qualified name can be used
				// to find the equivalent column in the selector's table.
				string[] fieldXPath = xmlSchemaXPath.XPath.Split(':');
				string columnName = fieldXPath.Length == 1 ? fieldXPath[0] : fieldXPath[1];
				if (!this.table.Columns.ContainsKey(columnName))
					throw new Exception(string.Format("The column {0} doesn't belong to the table {1} in constraint {2}", columnName, this.table.Name, xmlSchemaIdentityConstraint.Name));
				columnList.Add(this.table.Columns[columnName]);

			}

			// The list of fields is converted to an array of columns for the lifetime of this object.
			this.columns = columnList.ToArray();

			// This indicates that all of the elements of the constraint can be nulled.
			this.isNullable = true;
			foreach (ColumnSchema columnSchema in this.columns)
				if (!columnSchema.IsNullable)
					this.isNullable = false;

		}

		/// <summary>
		/// Gets the set of columns that describe the constraint.
		/// </summary>
		public ColumnSchema[] Columns
		{
			get { return this.columns; }
		}

		/// <summary>
		/// Gets an indication of whether any of the columns in the key can be null.
		/// </summary>
		public Boolean IsNullable
		{
			get { return this.isNullable; }
		}

		/// <summary>
		/// Gets the name of the constraint.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Gets the table to which the contraint is applied.
		/// </summary>
		public TableSchema Table
		{
			get { return this.table; }
		}

		/// <summary>
		/// The display text of the object.
		/// </summary>
		/// <returns></returns>
		public override string ToString() { return this.Name; }

	}

}
