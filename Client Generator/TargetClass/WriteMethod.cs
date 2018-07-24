namespace FluidTrade.ClientGenerator.TargetClass
{

	using System;
	using System.CodeDom;
	using System.IO;
	using System.IO.IsolatedStorage;
	using System.Security.Cryptography;
	using FluidTrade.Core;

	/// <summary>
	/// Creates a method to write the data model to isolated storage.
	/// </summary>
	class WriteMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to write the data model to isolated storage.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public WriteMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Writes the DataModel to the local cache.
			//		/// </summary>
			//		public static void Write()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Writes the {0} to the local cache.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Name = "Write";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(String)), "path"));

			//			try
			//          {
			CodeTryCatchFinallyStatement tryLock = new CodeTryCatchFinallyStatement();

			//              Monitor.Enter(DataModel.syncRoot);
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Enter", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//				global::System.IO.IsolatedStorage.IsolatedStorageFileStream isolatedStorageFileStream = new global::System.IO.IsolatedStorage.IsolatedStorageFileStream(path, global::System.IO.FileMode.Create);
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(IsolatedStorageFileStream)),
					"isolatedStorageFileStream",
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(IsolatedStorageFileStream)),
						new CodeArgumentReferenceExpression("path"),
						new CodeFieldReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(FileMode)), "Create"))));

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

			//				global::System.Security.Cryptography.CryptoStream cryptoStream = new global::System.Security.Cryptography.CryptoStream(isolatedStorageFileStream, rijnadael.CreateDecryptor(key, iV), global::System.Security.Cryptography.CryptoStreamMode.Write);
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(CryptoStream)),
					"cryptoStream",
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(CryptoStream)),
						new CodeVariableReferenceExpression("isolatedStorageFileStream"),
						new CodeMethodInvokeExpression(
							new CodeVariableReferenceExpression("rijnadael"),
							"CreateEncryptor",
							new CodeVariableReferenceExpression("key"),
							new CodeVariableReferenceExpression("iV")),
						new CodeFieldReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(CryptoStreamMode)), "Write"))));

			//				global::System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(cryptoStream);
			tryLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(BinaryWriter)),
					"binaryWriter",
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(BinaryWriter)),
						new CodeVariableReferenceExpression("cryptoStream"))));

			//				binaryWriter.Write(DataModel.sequence);
			tryLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("binaryWriter"),
					"Write",
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "sequence")));

			//				binaryWriter.Write(DataModel.dataSetId.ToByteArray());
			tryLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("binaryWriter"),
					"Write",
					new CodeMethodInvokeExpression(
						new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSetId"), "ToByteArray")));

			//				DataModel.dataSet.WriteXml(cryptoStream);
			tryLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"),
					"WriteXml",
					new CodeVariableReferenceExpression("cryptoStream")));

			//				binaryWriter.Close();
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("binaryWriter"), "Close"));

			//				cryptoStream.Close();
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("cryptoStream"), "Close"));

			//				isolatedStorageFileStream.Close();
			tryLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("isolatedStorageFileStream"), "Close"));

			//          }

			//			catch (System.Exception )
			//			{
			//			}
			tryLock.CatchClauses.Add(new CodeCatchClause());

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

			//		}

		}

	}
}
