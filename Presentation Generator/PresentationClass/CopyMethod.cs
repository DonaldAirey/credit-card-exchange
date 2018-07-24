namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method that copies the values from one object to another.
	/// </summary>
	public class CopyMethod : System.CodeDom.CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that copies the values from one object to another.
		/// </summary>
		/// <param name="classSchema">A description of the type.</param>
		public CopyMethod(ClassSchema classSchema)
		{

			//		/// <summary>
			//		/// Copies one FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader to another.
			//		/// </summary>
			//		public void Copy(FluidTrade.Core.IContent iContent)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Copies one {0} to another.", classSchema.Type), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "Copy";
			this.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IContent), "iContent"));

			//			FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader workingOrderHeader = ((FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader)(iContent));
			CodeVariableReferenceExpression parentRecordExpression = new CodeVariableReferenceExpression(CommonConversion.ToCamelCase(classSchema.Name));
			this.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(classSchema.Type), parentRecordExpression.VariableName,
				new CodeCastExpression(new CodeTypeReference(classSchema.Type), new CodeArgumentReferenceExpression("iContent"))));

			// This creates instructions to copy each of the properties from the source to the target.
			foreach (PropertySchema propertySchema in classSchema.Properties)
			{

				// Normally, the 'Set' operator would be used to copy the source value to the target value.  However, two-way data
				// binding presented a problem: when the user set the data in the user interface element, the data element would
				// also be modified.  When the data element was modified, the next pass of refreshed data would find the value
				// modified by the user and change it back to the original value (this only happened when the data entered by the
				// user had not filtered back through the data model refresh).  This would cause the user interface value to 'jump'
				// spastically.  The solution is to make the 'Set' operation basically do nothing.  It still needs to be provided
				// so the binding will find it.  Then the reponsibility of setting the value and triggering and update falls to 
				// this function.
				if (propertySchema.IsSimpleType)
				{

					//			if ((this.isEnabled != askPrice.isEnabled))
					//			{
					//				this.isEnabled = askPrice.isEnabled;
					//				if ((this.PropertyChanged != null))
					//				{
					//					this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("IsEnabled"));
					//				}
					//			}
					CodeConditionStatement ifValueStatement = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(propertySchema.Name)), CodeBinaryOperatorType.IdentityInequality, new CodeFieldReferenceExpression(parentRecordExpression, CommonConversion.ToCamelCase(propertySchema.Name))));
					ifValueStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(propertySchema.Name)), new CodePropertyReferenceExpression(parentRecordExpression, CommonConversion.ToCamelCase(propertySchema.Name))));
					CodeConditionStatement ifEventStatement = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "PropertyChanged"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)));
					ifEventStatement.TrueStatements.Add(new CodeDelegateInvokeExpression(new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "PropertyChanged"), new CodeThisReferenceExpression(),
						new CodeObjectCreateExpression(typeof(System.ComponentModel.PropertyChangedEventArgs), new CodePrimitiveExpression(propertySchema.Name))));
					ifValueStatement.TrueStatements.Add(ifEventStatement);
					this.Statements.Add(ifValueStatement);

				}
				else
				{

					// As long as the element isn't a list, the 'Copy' member of the 'IContent' interface is called to recurse into
					// the structure and copy all the child elements into the new structure.
					if (propertySchema.MinOccurs == 1)
					{

						//			this.askPrice.Copy(workingOrderHeader.askPrice);
						this.Statements.Add(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(propertySchema.Name)), "Copy",
							new CodeFieldReferenceExpression(parentRecordExpression, CommonConversion.ToCamelCase(propertySchema.Name))));

					}

				}

			}

			//		}

		}

	}

}
