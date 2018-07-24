namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.ComponentModel;

	internal class ExpressionMemberElement : MemberElement
	{

		private ExpressionElement MyElement;

		public ExpressionMemberElement(ExpressionElement element)
		{
			MyElement = element;
		}

		protected override void ResolveInternal()
		{

		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			base.Emit(ilg, services);
			MyElement.Emit(ilg, services);
			if (MyElement.ResultType.IsValueType == true)
			{
				EmitValueTypeLoadAddress(ilg, this.ResultType);
			}
		}

		protected override bool SupportsInstance
		{
			get { return true; }
		}

		protected override bool IsPublic
		{
			get { return true; }
		}

		public override bool IsStatic
		{
			get { return false; }
		}

		public override System.Type ResultType
		{
			get { return MyElement.ResultType; }
		}
	}

}