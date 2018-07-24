namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Diagnostics;
	using System.Reflection.Emit;
	using System.Reflection;

	// Element that represents all arithmetic operations
	internal class ArithmeticElement : BinaryExpressionElement
	{

		private static MethodInfo OurPowerMethodInfo;
		private static MethodInfo OurStringConcatMethodInfo;
		private static MethodInfo OurObjectConcatMethodInfo;
		private BinaryArithmeticOperation MyOperation;

		static ArithmeticElement()
		{
			OurPowerMethodInfo = typeof(Math).GetMethod("Pow", BindingFlags.Public | BindingFlags.Static);
			OurStringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, null);
			OurObjectConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(object) }, null);
		}

		public ArithmeticElement()
		{

		}

		protected override void GetOperation(object operation)
		{
			MyOperation = (BinaryArithmeticOperation)operation;
		}

		protected override System.Type GetResultType(System.Type leftType, System.Type rightType)
		{
			Type binaryResultType = ImplicitConverter.GetBinaryResultType(leftType, rightType);
			MethodInfo overloadedMethod = this.GetOverloadedArithmeticOperator();

			// Is an overloaded operator defined for our left and right children?
			if ((overloadedMethod != null))
			{
				// Yes, so use its return type
				return overloadedMethod.ReturnType;
			}
			else if ((binaryResultType != null))
			{
				// Operands are primitive types. Return computed result type unless we are doing a power operation
				if (MyOperation == BinaryArithmeticOperation.Power)
				{
					return typeof(double);
				}
				else
				{
					return binaryResultType;
				}
			}
			else if (this.IsEitherChildOfType(typeof(string)) == true & (MyOperation == BinaryArithmeticOperation.Add))
			{
				// String concatenation
				return typeof(string);
			}
			else
			{
				// Invalid types
				return null;
			}
		}

		private MethodInfo GetOverloadedArithmeticOperator()
		{
			// Get the name of the operator
			string name = GetOverloadedOperatorFunctionName(MyOperation);
			return base.GetOverloadedBinaryOperator(name, MyOperation);
		}

		private static string GetOverloadedOperatorFunctionName(BinaryArithmeticOperation op)
		{
			switch (op)
			{
			case BinaryArithmeticOperation.Add:
				return "Addition";
			case BinaryArithmeticOperation.Subtract:
				return "Subtraction";
			case BinaryArithmeticOperation.Multiply:
				return "Multiply";
			case BinaryArithmeticOperation.Divide:
				return "Division";
			case BinaryArithmeticOperation.Mod:
				return "Modulus";
			case BinaryArithmeticOperation.Power:
				return "Exponent";
			default:
				Debug.Assert(false, "unknown operator type");
				return null;
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			MethodInfo overloadedMethod = this.GetOverloadedArithmeticOperator();

			if ((overloadedMethod != null))
			{
				// Emit a call to an overloaded operator
				this.EmitOverloadedOperatorCall(overloadedMethod, ilg, services);
			}
			else if (this.IsEitherChildOfType(typeof(string)) == true)
			{
				// One of our operands is a string so emit a concatenation
				this.EmitStringConcat(ilg, services);
			}
			else
			{
				// Emit a regular arithmetic operation
				EmitChildWithConvert(MyLeftChild, this.ResultType, ilg, services);
				EmitChildWithConvert(MyRightChild, this.ResultType, ilg, services);
				EmitArithmeticOperation(MyOperation, ilg, services);
			}
		}

		private static bool IsUnsignedForArithmetic(Type t)
		{
			return object.ReferenceEquals(t, typeof(UInt32)) | object.ReferenceEquals(t, typeof(UInt64));
		}

		// Emit an arithmetic operation with handling for unsigned and checked contexts
		private void EmitArithmeticOperation(BinaryArithmeticOperation op, FleeILGenerator ilg, IServiceProvider services)
		{
			ExpressionOptions options = services.GetService(typeof(ExpressionOptions)) as ExpressionOptions;
			bool unsigned = IsUnsignedForArithmetic(MyLeftChild.ResultType) & IsUnsignedForArithmetic(MyRightChild.ResultType);
			bool integral = Utility.IsIntegralType(MyLeftChild.ResultType) & Utility.IsIntegralType(MyRightChild.ResultType);
			bool emitOverflow = integral & options.Checked;

			switch (op)
			{
			case BinaryArithmeticOperation.Add:
				if (emitOverflow == true)
				{
					if (unsigned == true)
					{
						ilg.Emit(OpCodes.Add_Ovf_Un);
					}
					else
					{
						ilg.Emit(OpCodes.Add_Ovf);
					}
				}
				else
				{
					ilg.Emit(OpCodes.Add);
				}

				break;
			case BinaryArithmeticOperation.Subtract:
				if (emitOverflow == true)
				{
					if (unsigned == true)
					{
						ilg.Emit(OpCodes.Sub_Ovf_Un);
					}
					else
					{
						ilg.Emit(OpCodes.Sub_Ovf);
					}
				}
				else
				{
					ilg.Emit(OpCodes.Sub);
				}

				break;
			case BinaryArithmeticOperation.Multiply:
				if (emitOverflow == true)
				{
					if (unsigned == true)
					{
						ilg.Emit(OpCodes.Mul_Ovf_Un);
					}
					else
					{
						ilg.Emit(OpCodes.Mul_Ovf);
					}
				}
				else
				{
					ilg.Emit(OpCodes.Mul);
				}

				break;
			case BinaryArithmeticOperation.Divide:
				if (unsigned == true)
				{
					ilg.Emit(OpCodes.Div_Un);
				}
				else
				{
					ilg.Emit(OpCodes.Div);
				}

				break;
			case BinaryArithmeticOperation.Mod:
				if (unsigned == true)
				{
					ilg.Emit(OpCodes.Rem_Un);
				}
				else
				{
					ilg.Emit(OpCodes.Rem);
				}

				break;
			case BinaryArithmeticOperation.Power:
				ilg.Emit(OpCodes.Call, OurPowerMethodInfo);
				break;
			default:
				Debug.Fail("Unknown op type");
				break;
			}
		}

		// Emit a string concatenation
		private void EmitStringConcat(FleeILGenerator ilg, IServiceProvider services)
		{
			Type argType = default(Type);
			MethodInfo concatMethodInfo = default(MethodInfo);

			// Pick the most specific concat method
			if (this.AreBothChildrenOfType(typeof(string)) == true)
			{
				concatMethodInfo = OurStringConcatMethodInfo;
				argType = typeof(string);
			}
			else
			{
				Debug.Assert(this.IsEitherChildOfType(typeof(string)), "one child must be a string");
				concatMethodInfo = OurObjectConcatMethodInfo;
				argType = typeof(object);
			}

			// Emit the operands and call the function
			MyLeftChild.Emit(ilg, services);
			ImplicitConverter.EmitImplicitConvert(MyLeftChild.ResultType, argType, ilg);
			MyRightChild.Emit(ilg, services);
			ImplicitConverter.EmitImplicitConvert(MyRightChild.ResultType, argType, ilg);
			ilg.Emit(OpCodes.Call, concatMethodInfo);
		}
	}

}