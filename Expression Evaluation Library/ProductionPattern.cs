﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.Text;

	//* 
	// * A production pattern. This class represents a set of production 
	// * alternatives that together forms a single production. A 
	// * production pattern is identified by an integer id and a name, 
	// * both provided upon creation. The pattern id is used for 
	// * referencing the production pattern from production pattern 
	// * elements. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class ProductionPattern
	{

		//* 
		// * The production pattern identity. 
		// 

		private int m_id;

		//* 
		// * The production pattern name. 
		// 

		private string m_name;

		//* 
		// * The synthectic production flag. If this flag is set, the 
		// * production identified by this pattern has been artificially 
		// * inserted into the grammar. 
		// 

		private bool m_synthetic;

		//* 
		// * The list of production pattern alternatives. 
		// 

		private ArrayList alternatives;

		//* 
		// * The default production pattern alternative. This alternative 
		// * is used when no other alternatives match. It may be set to 
		// * -1, meaning that there is no default (or fallback) alternative. 
		// 

		private int defaultAlt;

		//* 
		// * The look-ahead set associated with this pattern. 
		// 

		private LookAheadSet m_lookAhead;

		//* 
		// * Creates a new production pattern. 
		// * 
		// * @param id the production pattern id 
		// * @param name the production pattern name 
		// 

		public ProductionPattern(int id, string name)
		{
			this.m_id = id;
			this.m_name = name;
			this.alternatives = new ArrayList();
			this.defaultAlt = -1;
		}

		//* 
		// * The production pattern identity property (read-only). This 
		// * property contains the unique identity value. 
		// * 
		// * @since 1.5 
		// 

		public int Id
		{
			get { return m_id; }
		}

		//* 
		// * Returns the unique production pattern identity value. 
		// * 
		// * @return the production pattern id 
		// * 
		// * @see #Id 
		// * 
		// * @deprecated Use the Id property instead. 
		// 

		public int GetId()
		{
			return Id;
		}

		//* 
		// * The production pattern name property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public string Name
		{
			get { return m_name; }
		}

		//* 
		// * Returns the production pattern name. 
		// * 
		// * @return the production pattern name 
		// * 
		// * @see #Name 
		// * 
		// * @deprecated Use the Name property instead. 
		// 

		public string GetName()
		{
			return Name;
		}

		//* 
		// * The synthetic production pattern property. If this property 
		// * is set, the production identified by this pattern has been 
		// * artificially inserted into the grammar. No parse tree nodes 
		// * will be created for such nodes, instead the child nodes 
		// * will be added directly to the parent node. By default this 
		// * property is set to false. 
		// * 
		// * @since 1.5 
		// 

		public bool Synthetic
		{
			get { return m_synthetic; }
			set { m_synthetic = value; }
		}

		//* 
		// * Checks if the synthetic production flag is set. If this 
		// * flag is set, the production identified by this pattern has 
		// * been artificially inserted into the grammar. No parse tree 
		// * nodes will be created for such nodes, instead the child 
		// * nodes will be added directly to the parent node. 
		// * 
		// * @return true if this production pattern is synthetic, or 
		// * false otherwise 
		// * 
		// * @see #Synthetic 
		// * 
		// * @deprecated Use the Synthetic property instead. 
		// 

		public bool IsSyntetic()
		{
			return Synthetic;
		}

		//* 
		// * Sets the synthetic production pattern flag. If this flag is set, 
		// * the production identified by this pattern has been artificially 
		// * inserted into the grammar. By default this flag is set to 
		// * false. 
		// * 
		// * @param syntetic the new value of the synthetic flag 
		// * 
		// * @see #Synthetic 
		// * 
		// * @deprecated Use the Synthetic property instead. 
		// 

		public void SetSyntetic(bool synthetic)
		{
			this.Synthetic = synthetic;
		}

		//* 
		// * The look-ahead set property. This property contains the 
		// * look-ahead set associated with this alternative. 
		// 

		internal LookAheadSet LookAhead
		{
			get { return m_lookAhead; }
			set { m_lookAhead = value; }
		}

		//* 
		// * The default pattern alternative property. The default 
		// * alternative is used when no other alternative matches. The 
		// * default alternative must previously have been added to the 
		// * list of alternatives. This property is set to null if no 
		// * default pattern alternative has been set. 
		// 

		internal ProductionPatternAlternative DefaultAlternative
		{
			get
			{
				if (defaultAlt >= 0)
				{
					object obj = this.alternatives[defaultAlt];
					return (ProductionPatternAlternative)obj;
				}
				else
				{
					return null;
				}
			}
			set
			{
				defaultAlt = 0;
				for (int i = 0; i <= alternatives.Count - 1; i++)
				{
					if (object.ReferenceEquals(alternatives[i], value))
					{
						defaultAlt = i;
					}
				}
			}
		}

		//* 
		// * The production pattern alternative count property 
		// * (read-only). 
		// * 
		// * @since 1.5 
		// 

		public int Count
		{
			get { return alternatives.Count; }
		}

		//* 
		// * Returns the number of alternatives in this pattern. 
		// * 
		// * @return the number of alternatives in this pattern 
		// * 
		// * @see #Count 
		// * 
		// * @deprecated Use the Count property instead. 
		// 

		public int GetAlternativeCount()
		{
			return Count;
		}

		//* 
		// * The production pattern alternative index (read-only). 
		// * 
		// * @param index the alternative index, 0 <= pos < Count 
		// * 
		// * @return the alternative found 
		// * 
		// * @since 1.5 
		// 

		public ProductionPatternAlternative this[int index]
		{
			get { return (ProductionPatternAlternative)alternatives[index]; }
		}

		//* 
		// * Returns an alternative in this pattern. 
		// * 
		// * @param pos the alternative position, 0 <= pos < count 
		// * 
		// * @return the alternative found 
		// * 
		// * @deprecated Use the class indexer instead. 
		// 

		public ProductionPatternAlternative GetAlternative(int pos)
		{
			return this[pos];
		}

		//* 
		// * Checks if this pattern is recursive on the left-hand side. 
		// * This method checks if any of the production pattern 
		// * alternatives is left-recursive. 
		// * 
		// * @return true if at least one alternative is left recursive, or 
		// * false otherwise 
		// 

		public bool IsLeftRecursive()
		{
			ProductionPatternAlternative alt = default(ProductionPatternAlternative);
			for (int i = 0; i <= alternatives.Count - 1; i++)
			{

				alt = (ProductionPatternAlternative)alternatives[i];
				if (alt.IsLeftRecursive())
				{
					return true;
				}
			}
			return false;
		}

		//* 
		// * Checks if this pattern is recursive on the right-hand side. 
		// * This method checks if any of the production pattern 
		// * alternatives is right-recursive. 
		// * 
		// * @return true if at least one alternative is right recursive, or 
		// * false otherwise 
		// 

		public bool IsRightRecursive()
		{
			ProductionPatternAlternative alt = default(ProductionPatternAlternative);
			for (int i = 0; i <= alternatives.Count - 1; i++)
			{

				alt = (ProductionPatternAlternative)alternatives[i];
				if (alt.IsRightRecursive())
				{
					return true;
				}
			}
			return false;
		}

		//* 
		// * Checks if this pattern would match an empty stream of 
		// * tokens. This method checks if any one of the production 
		// * pattern alternatives would match the empty token stream. 
		// * 
		// * @return true if at least one alternative match no tokens, or 
		// * false otherwise 
		// 

		public bool IsMatchingEmpty()
		{
			ProductionPatternAlternative alt = default(ProductionPatternAlternative);
			for (int i = 0; i <= alternatives.Count - 1; i++)
			{

				alt = (ProductionPatternAlternative)alternatives[i];
				if (alt.IsMatchingEmpty())
				{
					return true;
				}
			}
			return false;
		}

		//* 
		// * Adds a production pattern alternative. 
		// * 
		// * @param alt the production pattern alternative to add 
		// * 
		// * @throws ParserCreationException if an identical alternative has 
		// * already been added 
		// 

		public void AddAlternative(ProductionPatternAlternative alt)
		{
			if (alternatives.Contains(alt))
			{
				throw new ParserCreationException(ParserCreationException.ErrorType.INVALID_PRODUCTION, m_name, "two identical alternatives exist");
			}
			alt.SetPattern(this);
			alternatives.Add(alt);
		}

		//* 
		// * Returns a string representation of this object. 
		// * 
		// * @return a token string representation 
		// 

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();
			StringBuilder indent = new StringBuilder();
			int i = 0;

			buffer.Append(m_name);
			buffer.Append("(");
			buffer.Append(m_id);
			buffer.Append(") ");
			for (i = 0; i <= buffer.Length - 1; i++)
			{
				indent.Append(" ");
			}
			for (i = 0; i <= alternatives.Count - 1; i++)
			{
				if (i == 0)
				{
					buffer.Append("= ");
				}
				else
				{
					buffer.Append("" + Convert.ToChar(10) + "");
					buffer.Append(indent);
					buffer.Append("| ");
				}
				buffer.Append(alternatives[i]);
			}
			return buffer.ToString();
		}
	}
}