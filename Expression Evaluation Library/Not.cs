﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Reflection.Emit;

	internal class NotElement : UnaryElement
	{

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			if (object.ReferenceEquals(MyChild.ResultType, typeof(bool)))
			{
				this.EmitLogical(ilg, services);
			}
			else
			{
				MyChild.Emit(ilg, services);
				ilg.Emit(OpCodes.Not);
			}
		}

		private void EmitLogical(FleeILGenerator ilg, IServiceProvider services)
		{
			MyChild.Emit(ilg, services);
			ilg.Emit(OpCodes.Ldc_I4_0);
			ilg.Emit(OpCodes.Ceq);
		}

		protected override System.Type GetResultType(System.Type childType)
		{
			if (object.ReferenceEquals(childType, typeof(bool)))
			{
				return typeof(bool);
			}
			else if (Utility.IsIntegralType(childType) == true)
			{
				return childType;
			}
			else
			{
				return null;
			}
		}
	}

}