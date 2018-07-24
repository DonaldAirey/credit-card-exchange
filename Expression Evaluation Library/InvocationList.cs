namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;

	internal class InvocationListElement : ExpressionElement
	{

		private MemberElement MyTail;

		public InvocationListElement(IList elements, IServiceProvider services)
		{
			this.HandleFirstElement(elements, services);
			LinkElements(elements);
			Resolve(elements, services);
			MyTail = elements[elements.Count - 1] as MemberElement;
		}

		// Arrange elements as a linked list
		private static void LinkElements(IList elements)
		{
			for (int i = 0; i <= elements.Count - 1; i++)
			{
				MemberElement current = elements[i] as MemberElement;
				MemberElement nextElement = null;
				if (i + 1 < elements.Count)
				{
					nextElement = elements[i + 1] as MemberElement;
				}
				current.Link(nextElement);
			}
		}

		private void HandleFirstElement(IList elements, IServiceProvider services)
		{
			ExpressionElement first = elements[0] as ExpressionElement;

			// If the first element is not a member element, then we assume it is an expression and replace it with the correct member element
			if (!(first is MemberElement))
			{
				ExpressionMemberElement actualFirst = new ExpressionMemberElement(first);
				elements[0] = actualFirst;
			}
			else
			{
				this.ResolveNamespaces(elements, services);
			}
		}

		private void ResolveNamespaces(IList elements, IServiceProvider services)
		{
			ExpressionContext context = services.GetService(typeof(ExpressionContext)) as ExpressionContext;
			ImportBase currentImport = context.Imports.RootImport;

			while (true)
			{
				string name = GetName(elements);

				if (name == null)
				{
					break; // TODO: might not be correct. Was : Exit While
				}

				ImportBase import = currentImport.FindImport(name);

				if (import == null)
				{
					break; // TODO: might not be correct. Was : Exit While
				}

				currentImport = import;
				elements.RemoveAt(0);

				if (elements.Count > 0)
				{
					MemberElement newFirst = (MemberElement)elements[0];
					newFirst.SetImport(currentImport);
				}
			}

			if (elements.Count == 0)
			{
				base.ThrowCompileException(CompileErrorResourceKeys.NamespaceCannotBeUsedAsType, CompileExceptionReason.TypeMismatch, currentImport.Name);
			}
		}

		private static string GetName(IList elements)
		{
			if (elements.Count == 0)
			{
				return null;
			}

			// Is the first member a field/property element?
			IdentifierElement fpe = elements[0] as IdentifierElement;

			if (fpe == null)
			{
				return null;
			}
			else
			{
				return fpe.MemberName;
			}
		}

		private static void Resolve(IList elements, IServiceProvider services)
		{
			foreach (MemberElement element in elements)
			{
				element.Resolve(services);
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			MyTail.Emit(ilg, services);
		}

		public override System.Type ResultType
		{
			get { return MyTail.ResultType; }
		}
	}

}