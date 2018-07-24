namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection.Emit;
	using System.Reflection;

	// Base class for expression elements that operate on two child elements
	internal abstract class BinaryExpressionElement : ExpressionElement
	{

		protected ExpressionElement MyLeftChild;
		protected ExpressionElement MyRightChild;
		private Type MyResultType;

		protected BinaryExpressionElement()
		{

		}

		// Converts a list of binary elements into a binary tree
		public static BinaryExpressionElement CreateElement(IList childValues, Type elementType)
		{
			BinaryExpressionElement firstElement = Activator.CreateInstance(elementType) as BinaryExpressionElement;
			firstElement.Configure(childValues[0] as ExpressionElement, childValues[2] as ExpressionElement, childValues[1]);

			BinaryExpressionElement lastElement = firstElement;

			for (int i = 3; i <= childValues.Count - 1; i += 2)
			{
				BinaryExpressionElement element = Activator.CreateInstance(elementType) as BinaryExpressionElement;
				element.Configure(lastElement as ExpressionElement, childValues[i + 1] as ExpressionElement, childValues[i]);
				lastElement = element;
			}

			return lastElement;
		}

		protected abstract void GetOperation(object operation);

		protected void ValidateInternal(object op)
		{
			MyResultType = this.GetResultType(MyLeftChild.ResultType, MyRightChild.ResultType);

			if (MyResultType == null)
			{
				this.ThrowOperandTypeMismatch(op, MyLeftChild.ResultType, MyRightChild.ResultType);
			}
		}

		protected MethodInfo GetOverloadedBinaryOperator(string name, object operation)
		{
			Type leftType = MyLeftChild.ResultType;
			Type rightType = MyRightChild.ResultType;
			BinaryOperatorBinder binder = new BinaryOperatorBinder(leftType, rightType);

			// If both arguments are of the same type, pick either as the owner type
			if (object.ReferenceEquals(leftType, rightType))
			{
				return Utility.GetOverloadedOperator(name, leftType, binder, leftType, rightType);
			}

			// Get the operator for both types
			MethodInfo leftMethod = default(MethodInfo);
			MethodInfo rightMethod = default(MethodInfo);
			leftMethod = Utility.GetOverloadedOperator(name, leftType, binder, leftType, rightType);
			rightMethod = Utility.GetOverloadedOperator(name, rightType, binder, leftType, rightType);

			// Pick the right one
			if (leftMethod == null & rightMethod == null)
			{
				// No operator defined for either
				return null;
			}
			else if (leftMethod == null)
			{
				return rightMethod;
			}
			else if (rightMethod == null)
			{
				return leftMethod;
			}
			else
			{
				// Ambiguous call
				base.ThrowAmbiguousCallException(leftType, rightType, operation);
				return null;
			}
		}

		protected void EmitOverloadedOperatorCall(MethodInfo method, FleeILGenerator ilg, IServiceProvider services)
		{
			ParameterInfo[] parameters = method.GetParameters();
			ParameterInfo pLeft = parameters[0];
			ParameterInfo pRight = parameters[1];

			EmitChildWithConvert(MyLeftChild, pLeft.ParameterType, ilg, services);
			EmitChildWithConvert(MyRightChild, pRight.ParameterType, ilg, services);
			ilg.Emit(OpCodes.Call, method);
		}

		protected void ThrowOperandTypeMismatch(object operation, Type leftType, Type rightType)
		{
			base.ThrowCompileException(CompileErrorResourceKeys.OperationNotDefinedForTypes, CompileExceptionReason.TypeMismatch, operation, leftType.Name, rightType.Name);
		}

		protected abstract Type GetResultType(Type leftType, Type rightType);

		protected static void EmitChildWithConvert(ExpressionElement child, Type resultType, FleeILGenerator ilg, IServiceProvider services)
		{
			child.Emit(ilg, services);
			bool converted = ImplicitConverter.EmitImplicitConvert(child.ResultType, resultType, ilg);
			Debug.Assert(converted, "convert failed");
		}

		protected bool AreBothChildrenOfType(Type target)
		{
			return IsChildOfType(MyLeftChild, target) & IsChildOfType(MyRightChild, target);
		}

		protected bool IsEitherChildOfType(Type target)
		{
			return IsChildOfType(MyLeftChild, target) || IsChildOfType(MyRightChild, target);
		}

		protected static bool IsChildOfType(ExpressionElement child, Type t)
		{
			return object.ReferenceEquals(child.ResultType, t);
		}

		// Set the left and right operands, get the operation, and get the result type
		private void Configure(ExpressionElement leftChild, ExpressionElement rightChild, object op)
		{
			MyLeftChild = leftChild;
			MyRightChild = rightChild;
			this.GetOperation(op);

			this.ValidateInternal(op);
		}

		public override sealed System.Type ResultType
		{
			get { return MyResultType; }
		}
	}

}