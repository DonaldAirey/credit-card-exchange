namespace FluidTrade.Core
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

	/// <summary>
	/// These are the parsing states used to read the arguments on the command line.
	/// </summary>
	enum ArgumentState { None, TargetNamespace, InputFileName, OutputFileName };

	/// <summary>
	/// Creates an executable wrapper around the IDE tool used to generate a middle tier.
	/// </summary>
	class SchemaScrubber
	{

		// Private Static Fields
		private static XNamespace dm;
		private static XNamespace xs;
		private static XNamespace mstns;
		private static XNamespace msdata;
		private static XNamespace msprop;

		// Private Instance Fields
		private static ArgumentState argumentState;
		private static String inputFilePath;
		private static String outputFileName;

		static SchemaScrubber()
		{

			// These are the namespaces used in an XML Schema document.
			SchemaScrubber.dm = "http://tempuri.org/DataModel.xsd";
			SchemaScrubber.xs = "http://www.w3.org/2001/XMLSchema";
			SchemaScrubber.mstns = "http://tempuri.org/DataModel.xsd";
			SchemaScrubber.msdata = "urn:schemas-microsoft-com:xml-msdata";
			SchemaScrubber.msprop = "urn:schemas-microsoft-com:xml-msprop";

		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{

			try
			{

				// The command line parser is driven by different states that are triggered by the flags read.  Unless a flag has
				// been read, the command line parser assumes that it's reading the file name from the command line.
				argumentState = ArgumentState.InputFileName;

				// Parse the command line for arguments.
				foreach (string argument in args)
				{

					// Decode the current argument into a state change (or some other action).
					if (argument == "-i") { argumentState = ArgumentState.InputFileName; continue; }
					if (argument == "-out") { argumentState = ArgumentState.OutputFileName; continue; }

					// The parsing state will determine which variable is read next.
					switch (argumentState)
					{
					case ArgumentState.InputFileName:
						inputFilePath = argument;
						break;
					case ArgumentState.OutputFileName:
						outputFileName = argument;
						break;
					}

					// The default state is to look for the input file name on the command line.
					argumentState = ArgumentState.InputFileName;

				}

				// Expand the environment variables for the input file path.
				if (inputFilePath == null)
					throw new Exception("Usage: DataSetGenerator -i <InputFileName>");
				inputFilePath = Environment.ExpandEnvironmentVariables(inputFilePath);

				// Expand the environment variables for the outpt file path.
				if (outputFileName == null)
					outputFileName = string.Format("{0}.cs", Path.GetFileNameWithoutExtension(inputFilePath));
				outputFileName = Environment.ExpandEnvironmentVariables(outputFileName);

				// The main idea here is to emulate the interface that Visual Studio uses to generate a file.  The first step is to
				// read the schema from the input file into a string.  This emulates the way that the IDE would normally call a
				// code generator.  Then create a 'MiddleTierGenerator' to compile the schema.
				StreamReader streamReader = new StreamReader(inputFilePath);
				DataModelSchema dataModelSchema = new DataModelSchema(streamReader.ReadToEnd());

				// Regurgitate the schema as an XDocument
				XDocument xDocument = new XDocument();
				XElement schemaElement = new XElement(
					SchemaScrubber.xs + "schema",
					new XAttribute("id", dataModelSchema.Name),
					new XAttribute("targetNamespace", dataModelSchema.TargetNamespace),
					new XAttribute(XNamespace.Xmlns + "mstns", "http://tempuri.org/DataModel.xsd"),
					new XAttribute("xmlns", "http://tempuri.org/DataModel.xsd"),
					new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"),
					new XAttribute(XNamespace.Xmlns + "msdata", "urn:schemas-microsoft-com:xml-msdata"),
					new XAttribute(XNamespace.Xmlns + "msprop", "urn:schemas-microsoft-com:xml-msprop"),
					new XAttribute("attributeFormDefault", "qualified"),
					new XAttribute("elementFormDefault", "qualified")
					);


				// Emit Annotations
				foreach (ObjectSchema itemSchema0 in dataModelSchema.Items)
				{
					if (itemSchema0 is AnnotationSchema)
					{
						AnnotationSchema annotationSchema = itemSchema0 as AnnotationSchema;
						XElement annotationElement = new XElement(SchemaScrubber.xs + "annotation");
						foreach (ObjectSchema itemSchema1 in annotationSchema.Items)
						{
							if (itemSchema1 is AppInfoSchema)
							{
								AppInfoSchema appInfoSchema = itemSchema1 as AppInfoSchema;
								XElement appInfoElement = new XElement(SchemaScrubber.xs + "appinfo");
								if (appInfoSchema.Source != String.Empty)
									appInfoElement.Add(new XAttribute("source", appInfoSchema.Source));
								foreach (XmlNode xmlNode in appInfoSchema.Markup)
									appInfoElement.Add(XElement.Parse(xmlNode.OuterXml));
								annotationElement.Add(appInfoElement);
							}
						}
						schemaElement.Add(annotationElement);
					}
				}

				// Data Model
				//   <xs:element name="DataModel" msdata:IsDataSet="true" msdata:UseCurrentLocale="true" msprop:Generator_UserDSName="DataModel"
				//		msprop:Generator_DataSetName="DataModel" msprop:EnableTableAdapterManager="true">
				XElement dataModelElement = new XElement(
					SchemaScrubber.xs + "element",
					new XAttribute("name", dataModelSchema.Name),
					new XAttribute(msdata + "IsDataSet", true),
					new XAttribute(msdata + "UseCurrentLocale", true),
					new XAttribute(SchemaScrubber.msprop + "Generator_UserDSName", dataModelSchema.Name),
					new XAttribute(SchemaScrubber.msprop + "Generator_DataSetName", dataModelSchema.Name),
					new XAttribute(SchemaScrubber.msprop + "EnableTableAdapterManager", true));

				//    <xs:complexType>
				XElement dataModelComlexTypeElement = new XElement(SchemaScrubber.xs + "complexType");

				//      <xs:choice minOccurs="0" maxOccurs="unbounded">
				XElement dataModelChoices = new XElement(
					SchemaScrubber.xs + "choice",
					new XAttribute("minOccurs", 0),
					new XAttribute("maxOccurs", "unbounded"));

				// This will scrub and add each of the tables to the schema in alphabetical order.
				foreach (KeyValuePair<String, TableSchema> keyValuePair in dataModelSchema.Tables)
					dataModelChoices.Add(CreateTable(keyValuePair.Value));

				// The complex types that define the tables.
				dataModelComlexTypeElement.Add(dataModelChoices);
				dataModelElement.Add(dataModelComlexTypeElement);

				// This will order the primary keys.
				List<UniqueConstraintSchema> primaryKeyList = new List<UniqueConstraintSchema>();
				foreach (KeyValuePair<String, TableSchema> keyValuePair in dataModelSchema.Tables)
					primaryKeyList.AddRange(keyValuePair.Value.UniqueConstraintSchemas);
				primaryKeyList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
				foreach (UniqueConstraintSchema uniqueConstraintSchema in primaryKeyList)
					dataModelElement.Add(CreateUniqueKey(uniqueConstraintSchema));

				// This will order the foreign primary keys.
				List<ForeignKeyConstraintSchema> foreignKeyList = new List<ForeignKeyConstraintSchema>();
				foreach (KeyValuePair<String, TableSchema> keyValuePair in dataModelSchema.Tables)
					foreignKeyList.AddRange(keyValuePair.Value.ForeignKeyConstraintSchemas);
				foreignKeyList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
				foreach (ForeignKeyConstraintSchema foreignConstraintSchema in foreignKeyList)
					dataModelElement.Add(CreateForeignKey(foreignConstraintSchema));

				// Add the data model element to the document.
				schemaElement.Add(dataModelElement);

				// Create the document from the root.
				xDocument.Add(schemaElement);

				// Save the regurgitated output.

				xDocument.Save(outputFileName);

			}
			catch (Exception exception)
			{

				// Dump any exceptions to the console.
				Console.WriteLine(exception.Message);

			}

			// At this point the code generated created the code-behind for the source schema successfully.
			return 0;

		}

		/// <summary>
		/// Creates the element that describes a table.
		/// </summary>
		/// <param name="tableSchema">A description of a table.</param>
		/// <returns>An element that represents a table in an XML Schema document.</returns>
		private static XElement CreateTable(TableSchema tableSchema)
		{

			//        <xs:element name="AccessControl" msprop:Generator_UserTableName="AccessControl" msprop:Generator_RowDeletedName="AccessControlRowDeleted"
			//			msprop:Generator_RowChangedName="AccessControlRowChanged" msprop:Generator_RowClassName="AccessControlRow"
			//			msprop:Generator_RowChangingName="AccessControlRowChanging" msprop:Generator_RowEvArgName="AccessControlRowChangeEvent"
			//			msprop:Generator_RowEvHandlerName="AccessControlRowChangeEventHandler" msprop:Generator_TableClassName="AccessControlDataTable"
			//			msprop:Generator_TableVarName="tableAccessControl" msprop:Generator_RowDeletingName="AccessControlRowDeleting" msprop:Generator_TablePropName="AccessControl">
			//          <xs:complexType>
			//            <xs:sequence>
			XElement tableElement = new XElement(
				SchemaScrubber.xs + "element",
				new XAttribute("name", tableSchema.Name),
				new XAttribute(SchemaScrubber.msprop + "Generator_UserTableName", tableSchema.Name),
				new XAttribute(SchemaScrubber.msprop + "Generator_RowDeletedName", String.Format("{0}RowDeleted", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_RowChangedName", String.Format("{0}RowChanged", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_RowClassName", String.Format("{0}Row", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_RowChangingName", String.Format("{0}RowChanging", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_RowEvArgName", String.Format("{0}RowChangeEvent", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_RowEvHandlerName", String.Format("{0}RowChangeEventHandler", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_TableClassName", String.Format("{0}DataTable", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_TableVarName", String.Format("table{0}", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_RowDeletingName", String.Format("{0}RowDeleting", tableSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_TablePropName", tableSchema.Name));

			// The table columns are placed in the sequence of an implicit complex type.  This will create, scrub and add each of the columns to the XML Schema
			// description of the table.
			XElement complexTypeElement = new XElement(SchemaScrubber.xs + "complexType");
			XElement sequenceElement = new XElement(SchemaScrubber.xs + "sequence");
			foreach (KeyValuePair<String, ColumnSchema> keyValuePair in tableSchema.Columns)
				if (!keyValuePair.Value.IsRowVersion)
					sequenceElement.Add(CreateColumn(keyValuePair.Value));
			complexTypeElement.Add(sequenceElement);
			tableElement.Add(complexTypeElement);

			// This element is a complete XML Schema description of a table.
			return tableElement;

		}

		/// <summary>
		/// Creates the element that describes a column in a table.
		/// </summary>
		/// <param name="columnSchema">A description of a column.</param>
		/// <returns>An element that can be used in an XML Schema document to describe a column.</returns>
		private static XElement CreateColumn(ColumnSchema columnSchema)
		{

			string dataType = String.Empty;
			string xmlType = String.Empty;
			object defaultValue = null;
			switch (columnSchema.DataType.ToString())
			{
			case "System.Object":

				xmlType = "xs:anyType";
				break;

			case "System.Int32":

				defaultValue = 0;
				xmlType = "xs:int";
				break;

			case "System.Int64":

				defaultValue = 0L;
				xmlType = "xs:long";
				break;

			case "System.Decimal":

				defaultValue = 0.0M;
				xmlType = "xs:decimal";
				break;

			case "System.Boolean":

				defaultValue = false;
				xmlType = "xs:boolean";
				break;

			case "System.String":

				defaultValue = String.Empty;
				xmlType = "xs:string";
				break;

			case "System.DateTime":

				xmlType = "xs:dateTime";
				break;

			case "System.Byte[]":

				xmlType = "xs:base64Binary";
				break;

			default:

				dataType = columnSchema.DataType.AssemblyQualifiedName;
				xmlType = "xs:anyType";
				break;
			}

			//			              <xs:element name="UserId" msdata:DataType="System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			//							msprop:Generator_UserColumnName="UserId" msprop:Generator_ColumnVarNameInTable="columnUserId" msprop:Generator_ColumnPropNameInRow="UserId"
			//							msprop:Generator_ColumnPropNameInTable="UserIdColumn" type="xs:string" minOccurs="0" />
			XElement columnElement = new XElement(
				SchemaScrubber.xs + "element",
				new XAttribute("name", columnSchema.Name));

			// Microsoft uses a custom decoration to describe data types that are not part of the cannon XML Schema datatypes.
			if (dataType != string.Empty)
				columnElement.Add(new XAttribute(SchemaScrubber.msdata + "DataType", dataType));

			// These are Microsoft added decorations for the schema that describe how a generated DataSet should name the internal and external values.
			columnElement.Add(				
				new XAttribute(SchemaScrubber.msprop + "Generator_UserColumnName", columnSchema.Name),
				new XAttribute(SchemaScrubber.msprop + "Generator_ColumnPropNameInRow", columnSchema.Name),
				new XAttribute(SchemaScrubber.msprop + "Generator_ColumnVarNameInTable", String.Format("column{0}", columnSchema.Name)),
				new XAttribute(SchemaScrubber.msprop + "Generator_ColumnPropNameInTable", String.Format("{0}Column", columnSchema.Name)));

			if (columnSchema.MaximumLength == int.MaxValue)
			{
				columnElement.Add(new XAttribute("type", xmlType));
			}
			else
			{

				//                <xs:simpleType>
				//                  <xs:restriction base="xs:string">
				//                    <xs:maxLength value="128" />
				//                  </xs:restriction>
				//                </xs:simpleType>
				columnElement.Add(
					new XElement(
						SchemaScrubber.xs + "simpleType",
						new XElement(
							SchemaScrubber.xs + "restriction",
							new XAttribute("base", xmlType),
							new XElement(
								SchemaScrubber.xs + "maxLength",
								new XAttribute("value", columnSchema.MaximumLength)))));
			}

			// An optional column is identified with a 'minOccurs=0' attribute.
			if (columnSchema.IsNullable)
				columnElement.Add(new XAttribute("minOccurs", 0));

			// Provide an explicit default value for all column elements.
			if (!columnSchema.IsNullable && defaultValue != null)
				columnElement.Add(new XAttribute("default", defaultValue));

			// This describes the column of a table.
			return columnElement;

		}

		/// <summary>
		/// Creates the element that describes a unique constraint.
		/// </summary>
		/// <param name="uniqueConstraintSchema">A description of a unique constraint.</param>
		/// <returns>An element that can be used in an XML Schema document to describe a unique constraint.</returns>
		private static XElement CreateUniqueKey(UniqueConstraintSchema uniqueConstraintSchema)
		{

			//    <xs:unique name="AccessControlKey" msdata:PrimaryKey="true">
			//      <xs:selector xpath=".//mstns:AccessControl" />
			//      <xs:field xpath="mstns:UserId" />
			//      <xs:field xpath="mstns:EntityId" />
			//    </xs:unique>
			XElement uniqueElement = new XElement(
				SchemaScrubber.xs + "unique",
				new XAttribute("name", uniqueConstraintSchema.Name));

			if (uniqueConstraintSchema.IsPrimaryKey)
				uniqueElement.Add(new XAttribute(SchemaScrubber.msdata + "PrimaryKey", true));

			uniqueElement.Add(
				new XElement(
					SchemaScrubber.xs + "selector",
					new XAttribute("xpath", String.Format(".//mstns:{0}", uniqueConstraintSchema.Table.Name))));

			foreach (ColumnSchema columnSchema in uniqueConstraintSchema.Columns)
				uniqueElement.Add(
					new XElement(
						SchemaScrubber.xs + "field",
						new XAttribute("xpath", String.Format("mstns:{0}", columnSchema.Name))));

			// This describes a unique constraint on a table.
			return uniqueElement;

		}

		/// <summary>
		/// Creates the element that describes a foreign constraint.
		/// </summary>
		/// <param name="foreignConstraintSchema">A description of a foreign constraint.</param>
		/// <returns>An element that can be used in an XML Schema document to describe a foreign constraint.</returns>
		private static XElement CreateForeignKey(ForeignKeyConstraintSchema foreignKeyConstraintSchema)
		{

			//    <xs:keyref name="FK_Entity_AccessControl" refer="EntityKey" msprop:rel_Generator_UserRelationName="FK_Entity_AccessControl"
			//		msprop:rel_Generator_RelationVarName="relationFK_Entity_AccessControl" msprop:rel_Generator_UserChildTable="AccessControl"
			//		msprop:rel_Generator_UserParentTable="Entity" msprop:rel_Generator_ParentPropName="EntityRow"
			//		msprop:rel_Generator_ChildPropName="GetAccessControlRows">
			//      <xs:selector xpath=".//mstns:AccessControl" />
			//      <xs:field xpath="mstns:EntityId" />
			//    </xs:keyref>
			XElement foreignElement = new XElement(
					SchemaScrubber.xs + "keyref",
					new XAttribute("name", foreignKeyConstraintSchema.Name),
					new XAttribute("refer", foreignKeyConstraintSchema.RelatedTable.GetUniqueConstraint(foreignKeyConstraintSchema.RelatedColumns).Name),
					new XAttribute(SchemaScrubber.msprop + "rel_Generator_UserRelationName", foreignKeyConstraintSchema.Name),
					new XAttribute(SchemaScrubber.msprop + "rel_Generator_RelationVarName", String.Format("relation{0}", foreignKeyConstraintSchema.Name)),
					new XAttribute(SchemaScrubber.msprop + "rel_Generator_UserChildTable", foreignKeyConstraintSchema.Table.Name),
					new XAttribute(SchemaScrubber.msprop + "rel_Generator_UserParentTable", foreignKeyConstraintSchema.RelatedTable.Name),
					new XAttribute(SchemaScrubber.msprop + "rel_Generator_ParentPropName", String.Format("{0}Row", foreignKeyConstraintSchema.RelatedTable.Name)),
					new XAttribute(SchemaScrubber.msprop + "rel_Generator_ChildPropName", String.Format("Get{0}Rows", foreignKeyConstraintSchema.Table.Name)));

			foreignElement.Add(
				new XElement(
					SchemaScrubber.xs + "selector",
					new XAttribute("xpath", String.Format(".//mstns:{0}", foreignKeyConstraintSchema.Table.Name))));

			foreach (ColumnSchema columnSchema in foreignKeyConstraintSchema.Columns)
				foreignElement.Add(
					new XElement(
						SchemaScrubber.xs + "field",
						new XAttribute("xpath", String.Format("mstns:{0}", columnSchema.Name))));

			// This describes a foreign constraint on a table.
			return foreignElement;

		}

	}

}
