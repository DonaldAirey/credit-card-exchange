namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections.Generic;

	/// <include file='Resources/DocComments.xml' path='DocComments/Member[@name="BatchLoader"]/*' />
	public sealed class BatchLoader
	{

		private IDictionary<string, BatchLoadInfo> MyNameInfoMap;
		private DependencyManager<string> MyDependencies;

		internal BatchLoader()
		{
			MyNameInfoMap = new Dictionary<string, BatchLoadInfo>(StringComparer.OrdinalIgnoreCase);
			MyDependencies = new DependencyManager<string>(StringComparer.OrdinalIgnoreCase);
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/Member[@name="BatchLoader.Add"]/*' />
		public void Add(string atomName, string expression, ExpressionContext context)
		{
			Utility.AssertNotNull(atomName, "atomName");
			Utility.AssertNotNull(expression, "expression");
			Utility.AssertNotNull(context, "context");

			BatchLoadInfo info = new BatchLoadInfo(atomName, expression, context);
			MyNameInfoMap.Add(atomName, info);
			MyDependencies.AddTail(atomName);

			ICollection<string> references = this.GetReferences(expression, context);

			foreach (string reference in references)
			{
				MyDependencies.AddTail(reference);
				MyDependencies.AddDepedency(reference, atomName);
			}
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/Member[@name="BatchLoader.Contains"]/*' />
		public bool Contains(string atomName)
		{
			return MyNameInfoMap.ContainsKey(atomName);
		}

		internal BatchLoadInfo[] GetBachInfos()
		{
			string[] tails = MyDependencies.GetTails();
			Queue<string> sources = MyDependencies.GetSources(tails);

			IList<string> result = MyDependencies.TopologicalSort(sources);

			BatchLoadInfo[] infos = new BatchLoadInfo[result.Count];

			for (int i = 0; i <= result.Count - 1; i++)
			{
				infos[i] = MyNameInfoMap[result[i]];
			}

			return infos;
		}

		private ICollection<string> GetReferences(string expression, ExpressionContext context)
		{
			IdentifierAnalyzer analyzer = context.ParseIdentifiers(expression);

			return analyzer.GetIdentifiers(context);
		}
	}
}