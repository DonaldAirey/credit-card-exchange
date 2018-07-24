using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Runtime.ConstrainedExecution;
using System.Drawing;
using FluidTrade.Core;

namespace FluidTrade.Core.Matching
{
	/// <summary>
	/// abstract class for Fuzzy matcher that used a dataColumn for its data source. 
	/// The defualt impl of the ColumnFuzzyMatcher is basically a pass through to the
	/// contained IFuzzyMatchStorage storage.  The one exception is the abstract CreateStorage()
	/// which is where the derived class creates the typed storage. CreateStorage() is called 
	/// from the ColumnFuzzyMatcher constructor
	/// </summary>
	public abstract partial class ColumnFuzzyMatcher
	{
		/// <summary>
		/// used for GetStringWithAdded. When have a string that is known not to be complete
		/// will insert a random characters at different location in the string to make it 
		/// the right number of characters. this is not thread safe
		/// but dont care because looking for a pseduo random number
		/// </summary>
		private static int nextDigitForAdded = 0;

		//storage for the matcher.  Storage object contains maps and logic. 
		//this is defined as an interface while the individual storage classes are 
		//derived from the Generic FuzzyMatchStorge in order to eliminate boxing/unboxing costs
		private IFuzzyMatchStorage storage;


		/// <summary>
		/// ctor. will create the inner storage object
		/// </summary>
		/// <param name="dataChars"></param>
		/// <param name="lookupTypes"></param>
		/// <param name="extendedStorageUsage">what values can be cached in ExtendedStorageRow</param>
		/// <param name="sourceColumn"></param>
		protected ColumnFuzzyMatcher(MatcherCreateArgs createArgs)
		{
			this.storage = this.CreateStorage(createArgs);
		}

		/// <summary>
		/// get inner IFuzzyMatchStorage object.  This probably should be protected, but public for testing
		/// </summary>
		public IFuzzyMatchStorage Storage { get { return this.storage; } }

		/// <summary>
		/// Create a matcher based on the DataCharType (dataChars). 
		/// </summary>
		/// <param name="dataChars">type of charaters in the data for the column. if data is not defined type will attempt to convert</param>
		/// <param name="lookupTypes">supported matcher operations</param>
		/// <param name="extendedStorageUsage">what values can be cached in ExtendedStorageRow</param>
		/// <param name="sourceColumn">dataColumn for the data</param>
		/// <returns></returns>
		public static ColumnFuzzyMatcher CreateFuzzyMatcher(MatcherCreateArgs createArgs)
		{
			switch (createArgs.DataChars)
			{
				case DataCharType.DigitsOnly:
					if (createArgs.NumberCharatersInData < 10)
						return new NineDigitOrLessMatcher(createArgs);
					else if (createArgs.NumberCharatersInData < 17)
						return new SixteenOrLessDigitOrLessMatcher(createArgs);
					else if (createArgs.NumberCharatersInData < 18)
						return new SixteenOrLessDigitOrLessMatcher(createArgs);
					return new StringTo10Matcher(createArgs);//should not be to 10 but ok for now
				case DataCharType.String:
					return new StringTo10Matcher(createArgs);
			}

			return null;
		}


		/// <summary>
		/// abstract method to create the inner IFuzzyMatchStorage 
		/// </summary>
		/// <param name="dataChars">default type of characters that are in the column data.  Will try to convert.</param>
		/// <param name="lookupTypes">type of operations supported by matcher</param>
		/// <param name="extendedStorageUsage">what values can be cached in ExtendedStorageRow</param>
		/// <param name="sourceColumn">DataColumn to use as data source</param>
		/// <returns></returns>
		protected abstract IFuzzyMatchStorage CreateStorage(MatcherCreateArgs createArgs);

		/// <summary>
		/// add all the rows in the table (defined by sourceColumn.Table.Rows) to the 
		/// Matcher
		/// </summary>
		public void AddTableRowsToMap(IDataModelTransaction dataModelTransaction)
		{
			this.storage.AddTableRowsToMap(dataModelTransaction);
		}

		public void AddTableRowToMap(IRowLockingWrapper rowWrapper)
		{
			this.storage.AddRowToMap(rowWrapper);
		}
		/// <summary>
		/// lookup exact match for value. returns list of matches or null
		/// </summary>
		/// <param name="value">value to lookup, will be converted to storage type</param>
		/// <param name="extendedStorageRow">IExtendedStorageRow (can be null) that storage can store temp info into for quicker lookup/generation</param>
		/// <returns></returns>
		public List<MatchInfo> FindExact(object value, IRowLockingWrapper valueDataRow, IDataModelTransaction dataModelTransaction)
		{
			return this.storage.FindExact(value, valueDataRow, dataModelTransaction);
		}

		/// <summary>
		/// find rows that might match, or match some of the value
		/// </summary>
		/// <param name="value">value to lookup, will be converted to storage type</param>
		/// <param name="extendedStorageRow">IExtendedStorageRow (can be null) that storage can store temp info into for quicker lookup/generation</param>
		/// <param name="useValidateForStrength">run validation on looked up probable rows to get a better match strength</param>
		/// <returns></returns>
		public Dictionary<IRow, MatchInfo> FindProbable(object value, IRowLockingWrapper valueDataRow, 
															bool useValidateForStrength, IDataModelTransaction dataModelTransaction)
		{
			return this.storage.FindProbable(value, valueDataRow, useValidateForStrength, dataModelTransaction);
		}


		/// <summary>
		/// 
		/// </summary>
		public void RemoveRowFromMap(IRow row)
		{
			this.storage.RemoveRowFromMap(row);
		}

		///// <summary>
		///// searches through rows in table for probable row matches. This uses the same algo as
		///// the FindProbable() but does not require storage to populate maps with Data Masking
		///// </summary>
		///// <param name="value">value to lookup, will be converted to storage type</param>
		///// <param name="extendedStorageRow">IExtendedStorageRow (can be null) that storage can store temp info into for quicker lookup/generation</param>
		///// <param name="useValidateForStrength">run validation on looked up probable rows to get a better match strength</param>
		///// <returns></returns>
		//public Dictionary<DataRow, MatchInfo> SearchForProbableUsingDataMasks(object value, IExtendedStorageRow extendedStorageRow, bool useValidateForStrength)
		//{
		//    throw new NotImplementedException();
		//    //return this.storage.SearchForProbableUsingDataMasks(value, extendedStorageRow, useValidateForStrength);
		//}

		///// <summary>
		///// searches through rows in table for row matches. This uses the validate() to calculate match strength as
		///// but does not require storage to populate maps with DataMasks/// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public List<MatchInfo> SearchForProbableUsingValidate(object value)
		//{
		//    throw new NotImplementedException();
		//    //return this.storage.SearchForProbableUsingValidate(value);
		//}

		/// <summary>
		/// validate a value against a row. This is needed because when Finding probable the strength is not as accurate as 
		/// the validation method. 
		/// </summary>
		/// <param name="value">value to validate</param>
		/// <param name="row">row to check value against. (using sourceColumn)</param>
		/// <returns></returns>
		public MatchInfo Validate(object value, IRowLockingWrapper targetValidationRow, IDataModelTransaction dataModelTransaction)
		{
			return this.storage.Validate(value, targetValidationRow, dataModelTransaction);
		}

		/// <summary>
		/// returns a list of all possible strings with charaters removed from original string
		/// charaters are removed one at a time from a string at different positions. 
		/// 
		/// "123" remove 1 would return {"12", "13", "23"}
		/// 
		/// this is a recursive call
		/// </summary>
		/// <param name="sb">original string to remove characters from</param>
		/// <param name="strings">list of return strings</param>
		/// <param name="index">index of sb to start removing</param>
		/// <param name="level">level of recursion</param>
		/// <param name="numberToRemove">number of charaters to remove</param>
		public static void GetStringWithRemovedSingle(StringBuilder sb, List<string> strings, int index, int level, int numberToRemove)
		{
			if (level == numberToRemove)
			{
				strings.Add(sb.ToString());
				return;
			}

			for (int i = index; i < sb.Length; i++)
			{
				StringBuilder mySb = new StringBuilder(sb.ToString());
				mySb.Remove(i, 1);
				GetStringWithRemovedSingle(mySb, strings, i, level + 1, numberToRemove);
			}
		}

		/// <summary>
		/// returns a list of three strings with charaters removed from the original string
		/// one string with the characters removed at the begining, one with the
		/// charaters removed at the middle and one with the charaters removed at the end
		/// </summary>
		/// <param name="sb">original string to remove characters from</param>
		/// <param name="strings">list of return strings</param>
		/// <param name="numberToRemove">number of charaters to remove from the string</param>
		public static void GetStringWithRemovedMultiple(StringBuilder sb, List<string> strings, int numberToRemove)
		{
			string fullStr = sb.ToString();
			strings.Add(fullStr.Remove(0, numberToRemove));
			strings.Add(fullStr.Remove(fullStr.Length/2 - numberToRemove/2, numberToRemove));
			strings.Add(fullStr.Remove(fullStr.Length - numberToRemove, numberToRemove));
		}


		/// <summary>
		/// returns a list of all possible strings with charaters added to the original string
		/// charaters are added one at a time from a string at different positions. 
		/// 
		/// 
		/// "123" add 1 would return {"x123", "1x23", "12x3", "123x"} x is random number
		/// 
		/// this is a recursive call
		/// </summary>
		/// <param name="sb">original string to remove characters from</param>
		/// <param name="strings">list of return strings</param>
		/// <param name="index">index of sb to start adding</param>
		/// <param name="level">level of recursion</param>
		/// <param name="numberToAdd">number of charaters to add</param>
		public static void GetStringWithAddedSingle(StringBuilder sb, List<string> strings, 
			int index, int level, int numberToAdd)
		{
			if (level == numberToAdd)
			{
				strings.Add(sb.ToString());
				return;
			}

			//access of nextDigitForAdded not thread safe
			//but dont care because looking for
			//a pseduo random number
			char insertNumber = (char)((int)('0') + ((nextDigitForAdded+=3) % 10));

			for (int i = index; i <= sb.Length; i++)
			{
				StringBuilder mySb = new StringBuilder(sb.ToString());
				if (i == sb.Length)
					mySb.Append(insertNumber);
				else
				{
					if (mySb[i] != insertNumber)
					{
						mySb.Insert(i, insertNumber);
					}
					else
					{
						mySb.Insert(i, insertNumber);
						i++;
					}
				}
				GetStringWithAddedSingle(mySb, strings, i, level + 1, numberToAdd);
			}
		}

		/// <summary>
		/// returns a list of three strings with charaters added to the original string
		/// one string with the characters added at the begining, one with the
		/// charaters added at the middle and one with the charaters added at the end
		/// </summary>
		/// <param name="sb">original string to add characters from</param>
		/// <param name="strings">list of return strings</param>
		/// <param name="numberToAdd">number of charaters to add from the string</param>
		public static void GetStringWithAddedMultiple(StringBuilder sb, List<string> strings, int numberToAdd)
		{
			StringBuilder addAtBeginingSb = new StringBuilder(sb.ToString());
			StringBuilder addAtMidSb = null;
			StringBuilder addAtEndSb = new StringBuilder(sb.ToString());

			double midStartDouble = ((double)sb.Length / 2F) - ((double)numberToAdd / 2F);
			if(midStartDouble <= 0F)
				midStartDouble = ((double)sb.Length / 2F);

			int midStart = (int)Math.Round(midStartDouble, 0);
			if(midStart != 0 && midStart != sb.Length)
				addAtMidSb = new StringBuilder(sb.ToString());
			
			for (int i = 0; i < numberToAdd; i++)
			{
				addAtBeginingSb.Insert(i, (char)((int)('0') + ((nextDigitForAdded += 3) % 10)));
				if(addAtMidSb != null)
					addAtMidSb.Insert(midStart, (char)((int)('0') + ((nextDigitForAdded += 3) % 10)));
				addAtEndSb.Append((char)((int)('0') + ((nextDigitForAdded += 3) % 10)));
			}

			strings.Add(addAtBeginingSb.ToString());
			if(addAtMidSb != null)
				strings.Add(addAtMidSb.ToString());
			strings.Add(addAtEndSb.ToString());
		}
	}
}
