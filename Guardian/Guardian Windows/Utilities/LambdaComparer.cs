namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Collections;

	/// <summary>
	/// A comparer to comparer any two objects of the same type.
	/// </summary>
	/// <typeparam name="T">The type of the two objects.</typeparam>
	public class LambdaComparer<T> : IComparer<T>, IComparer
	{

		/// <summary>
		/// A delegate used for doing comparisons.
		/// </summary>
		/// <param name="left">The "left" object.</param>
		/// <param name="right">The "right" object.</param>
		/// <returns>-1, 0, or 1 depending on how the two objects compare.</returns>
		public delegate int LambdaDelegate(T left, T right);

		/// <summary>
		/// Create an empty lambda comparer.
		/// </summary>
		public LambdaComparer()
		{


		}

		/// <summary>
		/// Create a lambda comparer with a with a comparison delegate.
		/// </summary>
		/// <param name="lambda">The comparison delegate.</param>
		public LambdaComparer(LambdaDelegate lambda)
		{

			this.Lambda = lambda;

		}

		/// <summary>
		/// Gets or sets the lambda delegate used to do the comparisons.
		/// </summary>
		public LambdaDelegate Lambda { get; set; }

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.</returns>
		public int Compare(T x, T y)
		{

			return this.Lambda(x, y);

		}

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.</returns>
		public int Compare(object x, object y)
		{

			return this.Lambda((T)x, (T)y);

		}

	}

}
