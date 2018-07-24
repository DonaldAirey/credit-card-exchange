namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.Collections.Generic;

	internal class ArgumentList
	{

		private IList<ExpressionElement> MyElements;

		public ArgumentList(ICollection elements)
		{
			ExpressionElement[] arr = new ExpressionElement[elements.Count];
			elements.CopyTo(arr, 0);
			MyElements = arr;
		}

		private string[] GetArgumentTypeNames()
		{
			List<string> l = new List<string>();

			foreach (ExpressionElement e in MyElements)
			{
				l.Add(e.ResultType.Name);
			}

			return l.ToArray();
		}

		public Type[] GetArgumentTypes()
		{
			List<Type> l = new List<Type>();

			foreach (ExpressionElement e in MyElements)
			{
				l.Add(e.ResultType);
			}

			return l.ToArray();
		}

		public override string ToString()
		{
			string[] typeNames = this.GetArgumentTypeNames();
			return Utility.FormatList(typeNames);
		}

		public ExpressionElement[] ToArray()
		{
			ExpressionElement[] arr = new ExpressionElement[MyElements.Count];
			MyElements.CopyTo(arr, 0);
			return arr;
		}

		public ExpressionElement this[int index]
		{
			get { return MyElements[index]; }
		}

		public int Count
		{
			get { return MyElements.Count; }
		}
	}

}