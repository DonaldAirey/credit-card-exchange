namespace FluidTrade.ExpressionEvaluation
{

	using System.Collections;

	//* 
	// * A production node. This class represents a grammar production 
	// * (i.e. a list of child nodes) in a parse tree. The productions 
	// * are created by a parser, that adds children a according to a 
	// * set of production patterns (i.e. grammar rules). 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class Production : Node
	{

		//* 
		// * The production pattern used for this production. 
		// 

		private ProductionPattern m_pattern;

		//* 
		// * The child nodes. 
		// 

		private ArrayList children;

		//* 
		// * Creates a new production node. 
		// * 
		// * @param pattern the production pattern 
		// 

		public Production(ProductionPattern pattern)
		{
			this.m_pattern = pattern;
			this.children = new ArrayList();
		}

		//* 
		// * The node type id property (read-only). This value is set as 
		// * a unique identifier for each type of node, in order to 
		// * simplify later identification. 
		// * 
		// * @since 1.5 
		// 

		public override int Id
		{
			get { return m_pattern.Id; }
		}

		//* 
		// * The node name property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public override string Name
		{
			get { return m_pattern.Name; }
		}

		//* 
		// * The child node count property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public override int Count
		{
			get { return children.Count; }
		}

		//* 
		// * The child node index (read-only). 
		// * 
		// * @param index the child index, 0 <= index < Count 
		// * 
		// * @return the child node found, or 
		// * null if index out of bounds 
		// * 
		// * @since 1.5 
		// 

		public override Node this[int index]
		{
			get
			{
				if (index < 0 || index >= children.Count)
				{
					return null;
				}
				else
				{
					return (Node)children[index];
				}
			}
		}

		//* 
		// * Adds a child node. The node will be added last in the list of 
		// * children. 
		// * 
		// * @param child the child node to add 
		// 

		public void AddChild(Node child)
		{
			if (child != null)
			{
				child.SetParent(this);
				children.Add(child);
			}
		}

		//* 
		// * The production pattern property (read-only). This property 
		// * contains the production pattern linked to this production. 
		// * 
		// * @since 1.5 
		// 

		public ProductionPattern Pattern
		{
			get { return m_pattern; }
		}

		//* 
		// * Returns the production pattern for this production. 
		// * 
		// * @return the production pattern 
		// * 
		// * @see #Pattern 
		// * 
		// * @deprecated Use the Pattern property instead. 
		// 

		public ProductionPattern GetPattern()
		{
			return Pattern;
		}

		//* 
		// * Checks if this node is hidden, i.e. if it should not be visible 
		// * outside the parser. 
		// * 
		// * @return true if the node should be hidden, or 
		// * false otherwise 
		// 

		internal override bool IsHidden()
		{
			return m_pattern.Synthetic;
		}

		//* 
		// * Returns a string representation of this production. 
		// * 
		// * @return a string representation of this production 
		// 

		public override string ToString()
		{
			return m_pattern.Name + '(' + m_pattern.Id + ')';
		}
	}
}