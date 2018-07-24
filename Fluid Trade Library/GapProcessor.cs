namespace FluidTrade.Core
{

	using System;
	using System.Collections;
	using System.Collections.Generic;

    /// <summary>
	/// Manages the gaps found in the row versions between the server and client.
	/// </summary>
	/// <remarks>As the server processes transactions, the row version is changed for any of the effected records.  The client uses
	/// these version numbers to reconcile the local data model with the servers.  If the client detects that is it missing some
	/// records, it will ask the server for those records during the next reconciliation cycle.  However, the sequence of row
	/// versions is not monotonically increasing like they would be for a message based system like FIX.  A client may miss some of
	/// the changes because it was shut down or because multiple updates to a record occurred during a single transaction.  To keep
	/// the list of unfillable gaps from increasing without bounds, this gap processor will keep track of the gaps and purge them 
	/// after a programmable time.  If this time is greater than the maximum time allowed for a transaction, then the client is
	/// guaranteed to stay in sync with the server.</remarks>
	public class GapProcessor : IEnumerable<Gap>
	{

		// Private Constants
		private const System.Int32 maximumAge = 10;

		// Private Fields
		private System.Collections.Generic.List<Gap> gapList;
		private System.TimeSpan maximumLatency;

		/// <summary>
		/// Create an object to manage the row version gaps between the client and the server.
		/// </summary>
		public GapProcessor()
		{

			// Initialize the object
			this.maximumLatency = TimeSpan.FromMinutes(GapProcessor.maximumAge);
			this.gapList = new List<Gap>();

			// The list always has a default gap from the lowest row version to the highest.
			this.gapList.Add(new Gap(0L, long.MaxValue));

		}

		/// <summary>
		/// Gets an infinite gap.
		/// </summary>
		public static long[][] InfiniteGap
		{
			get
			{

				// This will create an infinite gap.
				long[][] infiniteGap = new long[1][];
				infiniteGap[0] = new long[] { 0L, long.MaxValue };
				return infiniteGap;

			}
		}

		/// <summary>
		/// Gets or sets the amount of time that a gap is kept around before being expunged.
		/// </summary>
		public TimeSpan MaximumLatency
		{
			get { lock (this) return this.maximumLatency; }
			set { lock (this) this.maximumLatency = value; }
		}

		/// <summary>
		/// Removes gaps that are no longer useful.
		/// </summary>
		/// <param name="state">The starting parameters for the timer thread.</param>
		public void Purge()
		{

			lock (this)
			{

				// This will remove any gaps that have aged to the point that they are no longer useful.  Note that the last gap is
				// always left in the list because it contains the upper boundary.  That is, the gap at the top of the list is 
				// always asking "What's next" from the server, while the other items in the list are always asking "what did I
				// miss during the last reconcillation?"
				DateTime now = DateTime.Now;
				for (int index = 0; index < this.gapList.Count; index++)
					if (now.Subtract(gapList[index].DateTime) > this.maximumLatency && gapList[index].End != long.MaxValue)
						this.gapList.RemoveAt(index--);

			}

		}

		/// <summary>
		/// Searches the list of gaps for one that contains the given row version.
		/// </summary>
		/// <param name="rowVersion">The version of a row.</param>
		/// <returns>The index where a given row version is found of a ones-compliment value indicating where it should
		/// go in the list.</returns>
		public int BinarySearch(long rowVersion)
		{

			lock (this)
			{

				// A binary search is used to find the gap containing the row version.  The top and bottom bounds will come 
				// together until the gap is located.
				int bottomIndex = 0;
				int topIndex = this.gapList.Count - 1;

				// This will test a Gap to see if it contains the given row version.  If the row version is below the start
				// of the current gap, then the search area is moved to the lower half.  If the row version is above the
				// end of the gap, then the search area is moved to the upper half of the current region.  It continues this
				// way until a Gap is found that contains the row version, or the indices meet indicating that it can't be found.
				while (bottomIndex <= topIndex)
				{
					int rowIndex = (bottomIndex + topIndex) / 2;
					Gap gap = this.gapList[rowIndex];
					if (rowVersion <= gap.Start)
						topIndex = rowIndex - 1;
					else
					{
						if (rowVersion >= gap.End)
							bottomIndex = rowIndex + 1;
						else
							return rowIndex;
					}
				}

				// At this point, the row version isn't found in any of the existing Gaps.  Both of the indices are the same at
				// this point and indicate where the row version would fit in the list.  That index is complimented to indicate it
				// indicates where the item should go instead of where it was found.
				return ~bottomIndex;

			}

		}

		/// <summary>
		/// Fills in the list of gaps with a row version.
		/// </summary>
		/// <param name="rowVersion">The version of a row that has been successfully integrated into the data model.</param>
		public void FillGap(long rowVersion)
		{

			lock (this)
			{

				// Find a gap that contains the row version (note: if no gap exists, then this row version has already been
				// integrated into the list).  When a gap is found, fill it in with the new row version.  A gap disappears when
				// there is no space between the start and end of the gap.  Two gaps are created from one when a row version falls
				// between the start and end.
				int gapIndex = BinarySearch(rowVersion);
				if (gapIndex >= 0)
				{
					Gap gap = this.gapList[gapIndex];
					if (gap.Start + 1 == rowVersion)
					{
						gap.Start = rowVersion;
						if (gap.Start + 1 == gap.End)
							this.gapList.RemoveAt(gapIndex);
					}
					else
					{
						if (gap.End - 1 == rowVersion)
						{
							gap.End = rowVersion;
							if (gap.Start + 1 == gap.End)
								this.gapList.RemoveAt(gapIndex);
						}
						else
						{
							Gap newGap = new Gap(rowVersion, gap.End);
							this.gapList.Insert(gapIndex + 1, newGap);
							gap.End = rowVersion;
						}
					}

				}

			}

		}

		/// <summary>
		/// Converts the list of gaps to an array representing all the gaps in the row versions on the client.
		/// </summary>
		/// <returns>An array of start and end values representing all the gaps in the row version numbers on the client.</returns>
		public long[][] ToArray()
		{

			lock (this)
			{

				// Copy all the start and end values into an array.
				long[][] array = new long[this.gapList.Count][];
				for (int gapIndex = 0; gapIndex < this.gapList.Count; gapIndex++)
				{
					Gap gap = this.gapList[gapIndex];
					array[gapIndex] = new long[] { gap.Start, gap.End };
				}

				// This array represents all the gaps in the row version that the client has detected.  It is sent to the server
				// to indicate what values should be returned in the next reconcillation cycle.
				return array;

			}

		}

		/// <summary>
		/// Clears the gap processor.
		/// </summary>
		public void Clear()
		{

			lock (this)
			{

				// Revert the list to the initial state.
				this.gapList.Clear();
				this.gapList.Add(new Gap(0L, long.MaxValue));

			}

		}

		/// <summary>
		/// Gets the number of gaps in the list.
		/// </summary>
		public int Count { get { lock (this) return this.gapList.Count; } }

		/// <summary>
		/// Gets the Gap at the given index.
		/// </summary>
		/// <param name="index">An index into the list of gaps handled by the processor.</param>
		/// <returns>The Gap element at the given index.</returns>
		public Gap this[int index] { get { return this.gapList[index]; } }

		#region IEnumerable<Gap> Members

		/// <summary>
		/// Returns an enumerator that iterates through the list of gaps.
		/// </summary>
		/// <returns>An enumerator that iterates through a list of gaps.</returns>
		public IEnumerator<Gap> GetEnumerator() { return this.gapList.GetEnumerator(); }

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through the list of gaps.
		/// </summary>
		/// <returns>An enumerator that iterates through a list of gaps.</returns>
		IEnumerator IEnumerable.GetEnumerator() { return this.gapList.GetEnumerator(); }

		#endregion

	}

}
