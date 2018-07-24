namespace FluidTrade.ClientGenerator.TargetClass
{

    using System;
    using System.CodeDom;
	using System.IO;
	using System.IO.IsolatedStorage;
	using System.Security.Cryptography;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to load the cached data from isolated storage.
	/// </summary>
	class ReadMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to load the cached data from isolated storage.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public ReadMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Reads the DataModel from the local cache.
			//		/// </summary>
			//		public static void Read()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Reads the {0} from the local cache.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Name = "Read";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(String)), "path"));

			//			try
			//          {
			CodeTryCatchFinallyStatement tryLock = new CodeTryCatchFinallyStatement();

			//              Monitor.Enter(DataModel.syncRoot);
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Enter", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//				global::System.IO.IsolatedStorage.IsolatedStorageFileStream isolatedStorageFileStream = new global::System.IO.IsolatedStorage.IsolatedStorageFileStream(path, global::System.IO.FileMode.Open);
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(IsolatedStorageFileStream)),
					"isolatedStorageFileStream",
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(IsolatedStorageFileStream)),
						new CodeArgumentReferenceExpression("path"),
						new CodeFieldReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(FileMode)), "Open"))));

			//				global::System.Security.Cryptography.Rijndael rijnadael = global::System.Security.Cryptography.Rijndael.Create();
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Rijndael)),
					"rijnadael",
					new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(Rijndael)), "Create")));

			//				global::System.Byte[] iV = new global::System.Byte[] { 30, 35, 156, 47, 45, 115, 62, 120, 15, 107, 194, 83, 209, 127, 82, 95 };
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Byte[])),
					"iV",
					new CodeArrayCreateExpression(
						new CodeGlobalTypeReference(typeof(Byte[])),
						new CodePrimitiveExpression(30),
						new CodePrimitiveExpression(35),
						new CodePrimitiveExpression(156),
						new CodePrimitiveExpression(47),
						new CodePrimitiveExpression(45),
						new CodePrimitiveExpression(115),
						new CodePrimitiveExpression(62),
						new CodePrimitiveExpression(120),
						new CodePrimitiveExpression(15),
						new CodePrimitiveExpression(107),
						new CodePrimitiveExpression(194),
						new CodePrimitiveExpression(83),
						new CodePrimitiveExpression(209),
						new CodePrimitiveExpression(127),
						new CodePrimitiveExpression(82),
						new CodePrimitiveExpression(95))));

			//				global::System.Byte[] key = new global::System.Byte[] { 89, 91, 187, 183, 109, 169, 67, 155, 150, 145, 142, 46, 110, 148, 91, 156, 48, 8, 158, 199, 2, 107, 227, 160, 125, 235, 174, 109, 149, 180, 31, 140 };
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Byte[])),
					"key",
					new CodeArrayCreateExpression(
						new CodeGlobalTypeReference(typeof(Byte[])),
						new CodePrimitiveExpression(89),
						new CodePrimitiveExpression(91),
						new CodePrimitiveExpression(187),
						new CodePrimitiveExpression(183),
						new CodePrimitiveExpression(109),
						new CodePrimitiveExpression(169),
						new CodePrimitiveExpression(67),
						new CodePrimitiveExpression(155),
						new CodePrimitiveExpression(150),
						new CodePrimitiveExpression(145),
						new CodePrimitiveExpression(142),
						new CodePrimitiveExpression(46),
						new CodePrimitiveExpression(110),
						new CodePrimitiveExpression(148),
						new CodePrimitiveExpression(91),
						new CodePrimitiveExpression(156),
						new CodePrimitiveExpression(48),
						new CodePrimitiveExpression(8),
						new CodePrimitiveExpression(158),
						new CodePrimitiveExpression(199),
						new CodePrimitiveExpression(2),
						new CodePrimitiveExpression(107),
						new CodePrimitiveExpression(227),
						new CodePrimitiveExpression(160),
						new CodePrimitiveExpression(125),
						new CodePrimitiveExpression(235),
						new CodePrimitiveExpression(174),
						new CodePrimitiveExpression(109),
						new CodePrimitiveExpression(149),
						new CodePrimitiveExpression(180),
						new CodePrimitiveExpression(31),
						new CodePrimitiveExpression(140))));

			//				global::System.Security.Cryptography.CryptoStream cryptoStream = new global::System.Security.Cryptography.CryptoStream(isolatedStorageFileStream, rijnadael.CreateDecryptor(key, iV), global::System.Security.Cryptography.CryptoStreamMode.Read);
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(CryptoStream)),
					"cryptoStream",
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(CryptoStream)),
						new CodeVariableReferenceExpression("isolatedStorageFileStream"),
						new CodeMethodInvokeExpression(
							new CodeVariableReferenceExpression("rijnadael"),
							"CreateDecryptor",
							new CodeVariableReferenceExpression("key"),
							new CodeVariableReferenceExpression("iV")),
						new CodeFieldReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(CryptoStreamMode)), "Read"))));

			//				global::System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(cryptoStream);
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(BinaryReader)),
					"binaryReader",
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(BinaryReader)),
						new CodeVariableReferenceExpression("cryptoStream"))));

			//				DataModel.sequence = binaryReader.ReadInt64();
			tryLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "sequence"),
					new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("binaryReader"), "ReadInt64")));

			//				DataModel.dataSetId = new Guid(binaryReader.ReadBytes(16));
			tryLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSetId"),
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(Guid)),
						new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("binaryReader"), "ReadBytes", new CodePrimitiveExpression(16)))));

			//				DataModel.dataSet.ReadXml(cryptoStream);
			tryLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"),
					"ReadXml",
					new CodeVariableReferenceExpression("cryptoStream")));

			//				binaryReader.Close();
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("binaryReader"), "Close"));

			//				cryptoStream.Close();
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("cryptoStream"), "Close"));

			//				isolatedStorageFileStream.Close();
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("isolatedStorageFileStream"), "Close"));

			//			catch (System.Exception )
			//			{
			CodeCatchClause catchClause = new CodeCatchClause();
	
			//				DataModel.sequence = -1;
			catchClause.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "sequence"),
					new CodePrimitiveExpression(-1L)));

			//				DataModel.dataSetId = global::System.Guid.Empty;
			catchClause.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSetId"),
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Guid)), "Empty")));
			
			//			}
			tryLock.CatchClauses.Add(catchClause);

			//          }
			//          finally
			//          {
			//              Monitor.Exit(DataModel.syncRoot);
			tryLock.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)),
					"Exit",
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//          }
			this.Statements.Add(tryLock);

			//			DataModel.dataSet.AcceptChanges();
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"),
					"AcceptChanges"));

			//		}

		}

	}
}
