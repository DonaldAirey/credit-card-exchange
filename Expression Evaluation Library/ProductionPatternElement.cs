﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Text;

	//* 
	// * A production pattern element. This class represents a reference to 
	// * either a token or a production. Each element also contains minimum 
	// * and maximum occurence counters, controlling the number of 
	// * repetitions allowed. A production pattern element is always 
	// * contained within a production pattern rule. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class ProductionPatternElement
	{

		//* 
		// * The token flag. This flag is true for token elements, and 
		// * false for production elements. 
		// 

		private bool token;

		//* 
		// * The node identity. 
		// 

		private int m_id;

		//* 
		// * The minimum occurance count. 
		// 

		private int min;

		//* 
		// * The maximum occurance count. 
		// 

		private int max;

		//* 
		// * The look-ahead set associated with this element. 
		// 

		private LookAheadSet m_lookAhead;

		//* 
		// * Creates a new element. If the maximum value if zero (0) or 
		// * negative, it will be set to Int32.MaxValue. 
		// * 
		// * @param isToken the token flag 
		// * @param id the node identity 
		// * @param min the minimum number of occurancies 
		// * @param max the maximum number of occurancies, or 
		// * negative for infinite 
		// 

		public ProductionPatternElement(bool isToken, int id, int min, int max)
		{

			this.token = isToken;
			this.m_id = id;
			if (min < 0)
			{
				min = 0;
			}
			this.min = min;
			if (max <= 0)
			{
				max = Int32.MaxValue;
			}
			else if (max < min)
			{
				max = min;
			}
			this.max = max;
		}

		//* 
		// * The node identity property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public int Id
		{
			get { return m_id; }
		}

		//* 
		// * Returns the node identity. 
		// * 
		// * @return the node identity 
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
		// * The minimum occurence count property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public int MinCount
		{
			get { return min; }
		}

		//* 
		// * Returns the minimum occurence count. 
		// * 
		// * @return the minimum occurence count 
		// * 
		// * @see #MinCount 
		// * 
		// * @deprecated Use the MinCount property instead. 
		// 

		public int GetMinCount()
		{
			return MinCount;
		}

		//* 
		// * The maximum occurence count property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public int MaxCount
		{
			get { return max; }
		}

		//* 
		// * Returns the maximum occurence count. 
		// * 
		// * @return the maximum occurence count 
		// * 
		// * @see #MaxCount 
		// * 
		// * @deprecated Use the MaxCount property instead. 
		// 

		public int GetMaxCount()
		{
			return MaxCount;
		}

		//* 
		// * The look-ahead set property. This is the look-ahead set 
		// * associated with this alternative. 
		// 

		internal LookAheadSet LookAhead
		{
			get { return m_lookAhead; }
			set { m_lookAhead = value; }
		}

		//* 
		// * Returns true if this element represents a token. 
		// * 
		// * @return true if the element is a token, or 
		// * false otherwise 
		// 

		public bool IsToken()
		{
			return token;
		}

		//* 
		// * Returns true if this element represents a production. 
		// * 
		// * @return true if the element is a production, or 
		// * false otherwise 
		// 

		public bool IsProduction()
		{
			return !token;
		}

		//* 
		// * Checks if a specific token matches this element. This 
		// * method will only return true if this element is a token 
		// * element, and the token has the same id and this element. 
		// * 
		// * @param token the token to check 
		// * 
		// * @return true if the token matches this element, or 
		// * false otherwise 
		// 

		public bool IsMatch(Token token)
		{
			return IsToken() && token != null && token.Id == m_id;
		}

		//* 
		// * Checks if this object is equal to another. This method only 
		// * returns true for another identical production pattern 
		// * element. 
		// * 
		// * @param obj the object to compare with 
		// * 
		// * @return true if the object is identical to this one, or 
		// * false otherwise 
		// 

		public override bool Equals(object obj)
		{
			ProductionPatternElement elem = default(ProductionPatternElement);

			if (obj is ProductionPatternElement)
			{
				elem = (ProductionPatternElement)obj;
				return this.token == elem.token && this.m_id == elem.Id && this.min == elem.min && this.max == elem.max;
			}
			else
			{
				return false;
			}
		}

		/**
		 * Returns a hash code for this object.
		 *
		 * @return a hash code for this object
		 */
		public override int GetHashCode()
		{
			return this.m_id * 37;
		}

		//* 
		// * Returns a string representation of this object. 
		// * 
		// * @return a string representation of this object 
		// 

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();

			buffer.Append(m_id);
			if (token)
			{
				buffer.Append("(Token)");
			}
			else
			{
				buffer.Append("(Production)");
			}
			if (min != 1 || max != 1)
			{
				buffer.Append("{");
				buffer.Append(min);
				buffer.Append(",");
				buffer.Append(max);
				buffer.Append("}");
			}
			return buffer.ToString();
		}
	}
}