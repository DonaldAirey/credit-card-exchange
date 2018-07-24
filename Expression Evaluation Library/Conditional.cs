namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections.Generic;
	using System.Reflection.Emit;
	using System.Reflection;

	// Conditional operator
	internal class ConditionalElement : ExpressionElement
	{

		private ExpressionElement MyCondition;
		private ExpressionElement MyWhenTrue;
		private ExpressionElement MyWhenFalse;
		private Type MyResultType;

		public ConditionalElement(ExpressionElement condition, ExpressionElement whenTrue, ExpressionElement whenFalse)
		{
			MyCondition = condition;
			MyWhenTrue = whenTrue;
			MyWhenFalse = whenFalse;

			if ((!object.ReferenceEquals(MyCondition.ResultType, typeof(bool))))
			{
				base.ThrowCompileException(CompileErrorResourceKeys.FirstArgNotBoolean, CompileExceptionReason.TypeMismatch);
			}

			// The result type is the type that is common to the true/false operands
			if (ImplicitConverter.EmitImplicitConvert(MyWhenFalse.ResultType, MyWhenTrue.ResultType, null) == true)
			{
				MyResultType = MyWhenTrue.ResultType;
			}
			else if (ImplicitConverter.EmitImplicitConvert(MyWhenTrue.ResultType, MyWhenFalse.ResultType, null) == true)
			{
				MyResultType = MyWhenFalse.ResultType;
			}
			else
			{
				base.ThrowCompileException(CompileErrorResourceKeys.NeitherArgIsConvertibleToTheOther, CompileExceptionReason.TypeMismatch, MyWhenTrue.ResultType.Name, MyWhenFalse.ResultType.Name);
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			BranchManager bm = new BranchManager();
			bm.GetLabel("falseLabel", ilg);
			bm.GetLabel("endLabel", ilg);

			if (ilg.IsTemp == true)
			{
				// If this is a fake emit, then do a fake emit and return
				this.EmitConditional(ilg, services, bm);
				return;
			}

			FleeILGenerator ilgTemp = this.CreateTempFleeILGenerator(ilg);
			Utility.SyncFleeILGeneratorLabels(ilg, ilgTemp);

			// Emit fake conditional to get branch target positions
			this.EmitConditional(ilgTemp, services, bm);

			bm.ComputeBranches();

			// Emit real conditional now that we have the branch target locations
			this.EmitConditional(ilg, services, bm);
		}

		private void EmitConditional(FleeILGenerator ilg, IServiceProvider services, BranchManager bm)
		{
			Label falseLabel = bm.FindLabel("falseLabel");
			Label endLabel = bm.FindLabel("endLabel");

			// Emit the condition
			MyCondition.Emit(ilg, services);

			// On false go to the false operand
			if (ilg.IsTemp == true)
			{
				bm.AddBranch(ilg, falseLabel);
				ilg.Emit(OpCodes.Brfalse_S, falseLabel);
			}
			else if (bm.IsLongBranch(ilg, falseLabel) == false)
			{
				ilg.Emit(OpCodes.Brfalse_S, falseLabel);
			}
			else
			{
				ilg.Emit(OpCodes.Brfalse, falseLabel);
			}

			// Emit the true operand
			MyWhenTrue.Emit(ilg, services);
			ImplicitConverter.EmitImplicitConvert(MyWhenTrue.ResultType, MyResultType, ilg);

			// Jump to end
			if (ilg.IsTemp == true)
			{
				bm.AddBranch(ilg, endLabel);
				ilg.Emit(OpCodes.Br_S, endLabel);
			}
			else if (bm.IsLongBranch(ilg, endLabel) == false)
			{
				ilg.Emit(OpCodes.Br_S, endLabel);
			}
			else
			{
				ilg.Emit(OpCodes.Br, endLabel);
			}

			bm.MarkLabel(ilg, falseLabel);
			ilg.MarkLabel(falseLabel);

			// Emit the false operand
			MyWhenFalse.Emit(ilg, services);
			ImplicitConverter.EmitImplicitConvert(MyWhenFalse.ResultType, MyResultType, ilg);
			// Fall through to end
			bm.MarkLabel(ilg, endLabel);
			ilg.MarkLabel(endLabel);
		}

		public override System.Type ResultType
		{
			get { return MyResultType; }
		}
	}

}