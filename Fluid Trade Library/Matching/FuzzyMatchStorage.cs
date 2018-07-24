using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FluidTrade.Core;
using System.Threading;

namespace FluidTrade.Core.Matching
{
	public partial class ColumnFuzzyMatcher
	{

		/// <summary>
		/// abstract storage class.  Nested class so can be a 'friend' of ColumnFuzzyMatcher and to 
		/// keep each column matcher and it storage class together.  Not sure if friend relationship is needed
		/// 
		/// Note: currently storage is DataRow-centric should probably be made to have DataRow a Generic TypeParam instead of 
		///			hardcoded DataRow
		/// 
		/// The core of the fuzzy matching is implemeted in the Storage
		///		1) internal dictionaries are populated based on SupportedLookupType defined in ctor
		///			A) Exact match dictonary is populated with converted exact value provided by derived class( ConvertToValueInternal() )
		///				it is up to the derived storage class to define/implement what the converted value is
		///					for example: in the NineDigitOrLessMatcher.UIntStorage	a string of "123-45-7809" converted
		///						to the integer value of 123457809
		///			B) Probable match dictonaries are poulated based on the List of probably values provided
		///				by derived class ( ConvertToValueListInternal() ) and then masked using derived class 
		///				 (FillMaskedDataArray())
		///				 for example. in the NineDigitOrLessMatcher.UIntStorage	a string of "123-4-7809" converted
		///						to the list of value of {012347809, 123407809, 123478090} this list
		///						is returned because the required number of digits is 9 and only 8 are provided
		///						for each of the values in the converted list the dervied class FillMaskedDataArray will
		///						be called. This provides a list of masked values, which are essentially 
		///						different combinations of parts of the data. 123456789 might return
		///						{120000000, 000000089, 003450000, 120000089, ...} it is up to the derived
		///						class to provide the masked values and to later provide the strength of the Mask
		///							120000000 would return 22% for the strength (2/9)
		///							
		///		2) Lookup request can be made for either an Exact or a probable match
		///			these 2 calls are decoupled from a single call to FindMatch because
		///			you might have a combination of matchers and want to check all matchers for
		///			an exact match before continuing to look for probable matches
		///		
		///			A) for an exact the same process as population. The value to lookup
		///				is passed to derived class ConvertToValueInternal to get value to lookup
		///				value is used as key into dictionary
		///			B) for probable match also same process as population. A list of probable
		///				values are obtained for the value to lookup. These values are turned into
		///				a list of masked data values. Each of these values are used as a key
		///				into the each of the probable dictionaries. If parameter is set to use 
		///				validation for strength each probable result checked for strength
		///				using Validate(). All probable matches are sorted by strength and returned
		/// 
		/// </summary>
		/// <typeparam name="InputT">dataType of Data that is being matched on</typeparam>
		/// <typeparam name="DataMaskT">dataType for Mask. In most cases this is the same as the InputT, but in the case of a string
		///	it can be more efficient to return a hashcode (int) instead of creating a new string object for each of the DataMasks
		///	</typeparam>
		public abstract class FuzzyMatchStorage<InputT, DataMaskT> : IFuzzyMatchStorage
		{
			private MatchStorageDictionary<InputT> exactMap;
			private int extenededRowStorageIndex = -1;
			private int maskedDataArrayLength;
			
			/// <summary>
			/// the List does not need to be syncronized because it is only
			/// loaded in the constructor and after that the list does not change
			/// </summary>
			private List<MatchStorageDictionary<DataMaskT>> probableMatchSetMapList;
			protected MatcherCreateArgs createArgs;
			
			private ReaderWriterLockSlim addedRowsMapRWL;
			private Dictionary<IRow, MatchStorageList<MatchStorageRowList>> addedRowsMap;
			
			/// <summary>
			/// protected ctor for FuzzyMatchStorage. creates match dictionaries based on SupportedLookupType
			/// </summary>
			/// <param name="numberCharatersInData">default number of characters in the data. Should handle if out of bounds,
			/// but may not be as efficient</param>
			/// <param name="minMatchPercent"></param>
			/// <param name="dataChars"></param>
			/// <param name="lookupTypes"></param>
			/// <param name="extendedStorageUsage">what values can be cached in ExtendedStorageRow</param>
			/// <param name="sourceColumn"></param>
			protected FuzzyMatchStorage(MatcherCreateArgs createArgs)
			{
				this.addedRowsMapRWL = new ReaderWriterLockSlim();
				this.addedRowsMap = new Dictionary<IRow, MatchStorageList<MatchStorageRowList>>();
				this.createArgs = createArgs;

				//if dataTable supports extended storage rows then get the Index of the extendedSorage
				//for this matcher. The matcer will store converted values into the extendedStorage
				//so don't have to convert over and over.  The row should clear the 
				//extended storage whenever there is a value that changes in the row
				if (this.createArgs.SourceColumn.Table is IExtendedStorageTable)
					this.extenededRowStorageIndex = ((IExtendedStorageTable)this.createArgs.SourceColumn.Table).GetNextExtendedStorageIndex();


				//if this matcher supports exact lookup 
				//create the exact lookup dictionary
				if ((this.createArgs.LookupTypes & SupportedLookupType.ExactLookup) ==
					SupportedLookupType.ExactLookup)
				{
					this.exactMap = new MatchStorageDictionary<InputT>(new ReaderWriterLockSlim());
				}


				//if this matcher supports dataMasks
				if ((this.createArgs.LookupTypes & SupportedLookupType.UseDataMasksForProbable) ==
					SupportedLookupType.UseDataMasksForProbable)
				{
					//build the DataMaskSets
					this.maskedDataArrayLength = this.InitializeAvailableDataMasks(this.createArgs.MinMatchPercent);
					probableMatchSetMapList = new List<MatchStorageDictionary<DataMaskT>>();
					for (int i = 0; i < this.MaskedDataArrayLength; i++)
					{
						probableMatchSetMapList.Add(new MatchStorageDictionary<DataMaskT>(new ReaderWriterLockSlim()));
					}
				}
			}

			/// <summary>
			/// index of row storage values assigned in ctor from sourceColumn.DataTable (if supports IExtendedRowStorage)
			/// </summary>
			protected int ExtendedRowStorageIndex { get { return this.extenededRowStorageIndex; } }

			/// <summary>
			/// get number of items in the maskedData array returned from InitializeAvailableDataMasks()
			/// </summary>
			protected Int32 MaskedDataArrayLength
			{
				get
				{
					return this.maskedDataArrayLength;
				}
			}

			/// <summary>
			/// get types of supported lookup operations initialize in ctor
			/// </summary>
			public SupportedLookupType LookupTypes
			{
				get
				{
					return this.createArgs.LookupTypes;
				}
			}

			/// <summary>
			/// get reader writer lock for collection
			/// </summary>
			public ReaderWriterLockSlim ReaderWriterSync
			{
				get
				{
					return this.addedRowsMapRWL;
				}
			}

			/// <summary>
			/// Find an exact match for a value
			/// </summary>
			/// <param name="lookupValue">value to find</param>
			/// <param name="extendedStorageRow">extended storage that can store the converted value, or null if no extended storage supported</param>
			/// <returns>exact match or null if no match or matcher does not support exact match</returns>
			public List<MatchInfo> FindExact(object lookupValue, IRowLockingWrapper valueDataRow, IDataModelTransaction dataModelTransaction)
			{
				if (lookupValue == null ||
					((this.createArgs.LookupTypes & SupportedLookupType.ExactLookup) !=
					SupportedLookupType.ExactLookup))
					return null;

				//get the derived storage specific converted lookupValue
				InputT convertedVal;
				bool isInputValueComplete;
				string overflowString;
				if(this.ConvertToValue(lookupValue, valueDataRow, false, out isInputValueComplete, out convertedVal, out overflowString) == false)
					return null;

				List<MatchInfo> retList = new List<MatchInfo>();
				MatchStorageRowList rowList = null;
				if (exactMap.TryGetValue(convertedVal, out rowList))
				{
					rowList.EnterReadLock();
					try
					{
						foreach(IRow iRow in rowList.GetRawEnumerator())
						{
							MatchInfo mi = new MatchInfo();
							mi.row = iRow;
							mi.strength = 1M;
							mi.value = lookupValue;
							retList.Add(mi);
						}
					}
					finally
					{
						rowList.ExitReadLock();
					}
				}

				return retList;
			}

			/// <summary>
			/// find a list of probable matches 
			/// </summary>
			/// <param name="value">value to lookup probable matches for</param>
			/// <param name="valueExtendedStorageRow">IExtendedStorageRow (can be null) that storage can store temp info into for quicker lookup/generation</param>
			/// <param name="useValidateForStrength">should use Validate() to verify strength of match</param>
			/// <returns>returns a dictionary of dataRow to MatchInfo. Returns dictionary so can lookup dataRows for duplicates</returns>
			public Dictionary<IRow, MatchInfo> FindProbable(object value,
				IRowLockingWrapper valueDataRow, 
				bool useValidateForStrength,
				IDataModelTransaction dataModelTransaction)
			{
				if ((this.createArgs.LookupTypes & SupportedLookupType.UseDataMasksForProbable) !=
					SupportedLookupType.UseDataMasksForProbable)
					return null;

				Dictionary<IRow, MatchInfo> dataRowToMatches = new Dictionary<IRow, MatchInfo>();
				decimal listFactor;
				List<InputT> list = this.ConvertToValueList(value, valueDataRow, out listFactor);

				//eventhough list factor from ConvertToValueList() could be small will coninue
				//because Validation step could pick up a higher strength
				if (list == null)
					return null;

				//lookup any of the exact matches from the valueList
				foreach (InputT curVal in list)
				{
					List<MatchInfo> matchList = this.FindExact(curVal, null, dataModelTransaction);
					if (matchList != null)
						foreach (MatchInfo mi in matchList)
						{
							//the strength of the exact matches is
							//the strength of the list * the strength of exact match (list * 1)
							mi.strength = listFactor;
							dataRowToMatches[mi.row] = mi;
						}
				}

				bool isInputComplete;
				InputT convertedValue;
				string overflowString;
				this.ConvertToValue(value, valueDataRow, true, out isInputComplete, out convertedValue, out overflowString);
				
				foreach (InputT curVal in list)
				{
					//get the dataMask values for each of the values in the converted value list
					bool[] validAr;
					DataMaskT[] maskedDataAr = this.GetMaskedDataAr(curVal, null, out validAr);

					for (int i = 0; i < this.MaskedDataArrayLength; i++)
					{
						if (validAr != null && validAr[i] == false)
							continue;
						MatchStorageRowList rowList;
						if (probableMatchSetMapList[i].TryGetValue(maskedDataAr[i], out rowList) == false)
							continue;

						Decimal curFactor;
						if (useValidateForStrength == false)
							curFactor = this.GetMaskedDataMatchStrength(curVal, i) * listFactor;
						else
							curFactor = 0M;

						foreach (IRow iRow in rowList)
						{
							MatchInfo oldMatch;
							InputT convertedRowVal;
							bool isInputValueComplete;
							string convertedRowValOverflow = null;

							RowLockingWrapper<IRow> rowWrapper = new RowLockingWrapper<IRow>(iRow, dataModelTransaction);

							rowWrapper.AcquireReaderLock();
							if(rowWrapper.Row.RowState == DataRowState.Deleted ||
								rowWrapper.Row.RowState == DataRowState.Detached)
							{
								rowWrapper.ReleaseReaderLock();
								continue;
							}

							try
							{
								if(useValidateForStrength == true)
								{
									if(this.ConvertToValue(rowWrapper.Row[this.createArgs.SourceColumn], rowWrapper, true, out isInputValueComplete, out convertedRowVal, out convertedRowValOverflow) == false &&
										convertedRowValOverflow == null)
									{
										continue;
									}

									this.ValidateInternal(convertedValue, null, overflowString, convertedRowVal, rowWrapper, convertedRowValOverflow, out curFactor);
								}
								else
								{
									if(this.ConvertToValue(rowWrapper.Row[this.createArgs.SourceColumn], rowWrapper, false, out isInputValueComplete, out convertedRowVal, out convertedRowValOverflow) == false)
									{
										//if cannot convert
										continue;
									}
								}
							}
							finally
							{
								rowWrapper.ReleaseReaderLock();
							}

							if(dataRowToMatches.TryGetValue(rowWrapper.Row, out oldMatch))
							{
								if (oldMatch.strength < curFactor)
								{
									oldMatch.strength = curFactor;
									oldMatch.value = maskedDataAr[i];
								}
							}
							else
							{
								oldMatch = new MatchInfo();
								oldMatch.row = rowWrapper.Row;
								oldMatch.strength = curFactor;
								oldMatch.value = maskedDataAr[i];
								dataRowToMatches[rowWrapper.Row] = oldMatch;
							}
						}
					}
				}

				return dataRowToMatches;
			}

			public void AddTableRowsToMap(IDataModelTransaction dataModelTransaction)
			{
				if (this.createArgs.LookupTypes == SupportedLookupType.ValidateOnly)
					return;

				foreach (DataRow r in this.createArgs.SourceColumn.Table.Rows)
				{
					IRow iRow = r as IRow;
					if(iRow == null)
						break;

					RowLockingWrapper<IRow> rowWrapper = new RowLockingWrapper<IRow>(iRow, dataModelTransaction);
					rowWrapper.AcquireReaderLock();
					try
					{
						DataRowState state = r.RowState;
						if(state == DataRowState.Deleted ||
							state == DataRowState.Detached)
							continue;

						this.AddRowToMapInternal(rowWrapper);
					}
					finally
					{
						rowWrapper.ReleaseWriterLock();
					}
				}
			}


			public void AddRowToMap(IRowLockingWrapper rowWrapper)
			{
				rowWrapper.AcquireReaderLock();
				try
				{
					if(this.createArgs.RowValidForMatcherMap != null &&
						this.createArgs.RowValidForMatcherMap(rowWrapper) == false)
						return;

					//this.rowFitsMatcher(row);
					if(this.createArgs.LookupTypes == SupportedLookupType.ValidateOnly)
						return;

					this.AddRowToMapInternal(rowWrapper);
				}
				finally
				{
					rowWrapper.ReleaseReaderLock();
				}
			}

			/// <summary>
			/// //don't need to lock the rowWrapper.Row
			/// because not using the inner values just the row itself as the key
			/// </summary>
			/// <param name="row"></param>
			public void RemoveRowFromMap(IRow row)
			{
				MatchStorageList<MatchStorageRowList> listOfRowList;
				if(this.addedRowsMapRWL.TryEnterWriteLock(60000))
				{
					try
					{
						if(addedRowsMap.TryGetValue(row, out listOfRowList))
						{
							foreach(MatchStorageRowList list in listOfRowList)
								list.Remove(row, true);

							this.addedRowsMap.Remove(row);
						}
					}
					finally
					{
						this.addedRowsMapRWL.ExitWriteLock();
					}
				}
				else
				{
					throw new TimeoutException();
				}
			}

			#region Not Implemented SearchForProbableUsingValidate and SearchForProbableUsingValidate
			/// <summary>
			/// searches through rows in table for probable row matches. This uses the same algo as
			/// the FindProbable() but does not require storage to populate maps with DataMasks
			/// </summary>
			/// <param name="value">value to lookup, will be converted to storage type</param>
			/// <param name="extendedStorageRow">IExtendedStorageRow (can be null) that storage can store temp info into for quicker lookup/generation</param>
			/// <param name="useValidateForStrength">run validation on looked up probable rows to get a better match strength</param>
			/// <returns></returns>
			public Dictionary<DataRow, MatchInfo> SearchForProbableUsingDataMasks(object value, IExtendedStorageRow extendedStorageRow, bool useValidateForStrength)
			{
				throw new NotImplementedException();

				//Decimal listFactor;
				//List<InputT> possibleValues = this.ConvertToValueList(value, out listFactor);
				//if (possibleValues == null)
				//    return null;

				//if (possibleValues.Count != 1)
				//{
				//    InputT exactConvertedVal;
				//    this.ConvertToValue(value, out exactConvertedVal);
				//    possibleValues.Insert(0, exactConvertedVal);
				//}

				//Dictionary<DataRow, MatchInfo> dataRowToMatches = new Dictionary<DataRow, MatchInfo>();

				//Decimal posValFactor = 1M; 

				//for(int posValIndex = 0; posValIndex < possibleValues.Count; posValIndex++ )
				//{
				//    InputT curPossibleValue = possibleValues[posValIndex];
				//    bool[] validAr;
				//    DataMaskT[] possibleValMaskedDatas = this.GetMaskedDatas(curPossibleValue, out validAr);
				//    if (posValIndex == 1)
				//        posValFactor = listFactor;

				//    //First match should be the highest strength
				//    foreach (DataRow row in this.sourceColumn.Table.Rows)
				//    {
				//        DataRowState state = row.RowState;
				//        if (state == DataRowState.Deleted ||
				//            state == DataRowState.Detached)
				//            continue;

				//        object rowVal = row[sourceColumn];

				//        Decimal rowFactor;
				//        InputT exactConvertedRowVal;
				//        List<InputT> possibleRowValues;
				//        MatchInfo oldMatch;

				//        if (this.ConvertToValue(rowVal, out exactConvertedRowVal) == true &&
				//            object.Equals(curPossibleValue, exactConvertedRowVal))
				//        {
				//            if (dataRowToMatches.TryGetValue(row, out oldMatch))
				//            {
				//                if (oldMatch.strength < 1M)
				//                {
				//                    oldMatch.strength = 1M;
				//                    oldMatch.value = exactConvertedRowVal;
				//                }
				//            }
				//            else
				//            {
				//                oldMatch = new MatchInfo();
				//                oldMatch.row = row;
				//                oldMatch.strength = 1M;
				//                oldMatch.value = exactConvertedRowVal;
				//                dataRowToMatches[row] = oldMatch;
				//            }

				//            continue;
				//        }
				//        else
				//        {
				//            possibleRowValues = this.ConvertToValueList(rowVal, out rowFactor);
				//        }

				//        if (possibleRowValues == null)
				//            continue;

				//        foreach (InputT curRowVal in possibleRowValues)
				//        {
				//            bool[] validAr2;
				//            DataMaskT[] rowMaskedDatas = this.GetMaskedDatas(curRowVal, out validAr2);
				//            for (int i = 0; i < this.MatchSetsCount; i++)
				//            {
				//                if (validAr != null && validAr[i] == false ||
				//                    (validAr2 != null && validAr2[i] == false))
				//                    continue;

				//                DataMaskT curMaskedDataVal = rowMaskedDatas[i];
				//                DataMaskT curPossibleMaskedDataVal = possibleValMaskedDatas[i];
				//                if(object.Equals(curMaskedDataVal,curPossibleMaskedDataVal )) //!!!RM get rid of boxing
				//                {
				//                    Decimal curFactor = this.GetMaskedDataMatchStrength(curRowVal, i) * posValFactor;
				//                    if (dataRowToMatches.TryGetValue(row, out oldMatch))
				//                    {
				//                        if (oldMatch.strength < 1M)
				//                        {
				//                            oldMatch.strength = 1M;
				//                            oldMatch.value = exactConvertedRowVal;
				//                        }
				//                    }
				//                    else
				//                    {
				//                        oldMatch = new MatchInfo();
				//                        oldMatch.row = row;
				//                        oldMatch.strength = 1M;
				//                        oldMatch.value = exactConvertedRowVal;
				//                        dataRowToMatches[row] = oldMatch;
				//                    }
				//                }
				//            }
				//        }
				//    }
				//}

				//if (dataRowToMatches.Count == 0)
				//    return null;

				//return dataRowToMatches;
			}

			public List<MatchInfo> SearchForProbableUsingValidate(object value)
			{
				//call validate on all rows
				throw new NotImplementedException();
			}
			#endregion

			public MatchInfo Validate(object value, IRowLockingWrapper targetValidationRow, IDataModelTransaction dataModelTransaction)
			{
				//!!! store some info in the DataRow instead of lookups or repeated converts?
				InputT convertedVal;
				bool isInputValueComplete;
				string convertedValOverflow;

				if(this.ConvertToValue(value, null, true, out isInputValueComplete, out convertedVal, out convertedValOverflow) == false &&
					convertedValOverflow == null)
					return null;

				InputT convertedRowVal;
				string convertedRowValOverflow;

				try
				{
					targetValidationRow.AcquireReaderLock();
					if(targetValidationRow.Row.RowState == DataRowState.Deleted ||
						targetValidationRow.Row.RowState == DataRowState.Detached)
						return null;
					if(this.ConvertToValue(targetValidationRow.Row[this.createArgs.SourceColumn], targetValidationRow, true, out isInputValueComplete, out convertedRowVal, out convertedRowValOverflow) == false &&
						convertedRowValOverflow == null)
						return null;

					decimal strength;

					if(this.ValidateInternal(convertedVal, null, convertedValOverflow, convertedRowVal, targetValidationRow, convertedRowValOverflow, out strength) == false)
						return null;

					MatchInfo mi = new MatchInfo();
					mi.row = targetValidationRow.Row;
					mi.strength = strength;
					mi.value = value;

					return mi;
				}
				finally
				{
					targetValidationRow.ReleaseReaderLock();
				}
			}


			/// <summary>
			/// called from ctor. Allows for derived class to setup the data masks based on a minimum strength
			/// the Higher the % the fewer results that are returned from the FindProbable(). 
			/// This would improve performance but you run the risk of missing some potential matches.
			/// </summary>
			/// <param name="minMatchPercent">minimum match strength to include dataMask</param>
			/// <returns>number of available data masks</returns>
			protected abstract int InitializeAvailableDataMasks(int minMatchPercent);

			/// <summary>
			/// converts value to object of the derived storage class defined data type
			/// 
			/// Converted value could be in exact map, or used as a value that will be masked and put into the probable maps
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="extendedStorageRow">extended storage row for storing converted value (or null)</param>
			/// <param name="isInputValueComplete">is the input value complete, only makes sense when number of digits/charaters are defined. true if the value 
			/// being converted can be considered a complete value after the conversion.  If you had defined the type to be 4 digits and the 
			/// lookup was a string of "a1234" the converted value might be 1234 since there are 4 digits this would be considered complete. 
			/// when adding rows to internal maps if the value is complete the converted value can be used for generation of Masked Data.
			/// If the input is not complete then the convereted value need to be adjusted by either adding or removing characters before
			/// generating the Masked Data
			/// </param>
			/// <param name="convertedValue">out converted value</param>
			/// <returns>true if could convert the value</returns>
			private bool ConvertToValue(object value, IRowLockingWrapper valueDataRow,
				bool allowExtraCharacters,
				out bool isInputValueComplete, 
				out InputT convertedValue,
				out String overflowString)
			{

				//dont use extenededStorage cache for extra charaters
				if(allowExtraCharacters == true)
				{
					return ConvertToValueInternal(value, true, out isInputValueComplete, out convertedValue, out overflowString);
				}

				overflowString = null;
				if ((this.createArgs.ExtendedStorageUsage & ExtendedStorageTypes.ConvertToValue) ==
					ExtendedStorageTypes.None)
				{
					return ConvertToValueInternal(value, false, out isInputValueComplete, out convertedValue, out overflowString);
				}

				//check extened storage before doing the convert
				ExtendedStorage extStorage;
				IExtendedStorageRow extendedStorageRow = null;

				if(valueDataRow != null)
					valueDataRow.AcquireReaderLock();
				try
				{
					if(valueDataRow != null)
					{
						extendedStorageRow = valueDataRow.Row as IExtendedStorageRow;
						if(this.extenededRowStorageIndex >= 0 &&
							extendedStorageRow != null)
						{
							extStorage = extendedStorageRow.GetExtendedStorage(this.extenededRowStorageIndex) as ExtendedStorage;
							if(extStorage != null &&
								extStorage.hasConvertToValues)
							{
								//found it return it
								isInputValueComplete = extStorage.convertToValueIsInputValueComplete;
								convertedValue = extStorage.convertToValueConvertedValue;
								return extStorage.convertToValueRetValue;
							}
						}
						else
						{
							extStorage = null;
						}
					}
					else
					{
						extStorage = null;
					}

					//either no extenededStorage or no value in extended storage do the convert
					bool retVal = ConvertToValueInternal(value, false, out isInputValueComplete, out convertedValue, out overflowString);

					//if have exteneded storage put the converted value in for future use
					if(extendedStorageRow != null)
					{
						if(extStorage == null)
							extStorage = new ExtendedStorage();

						extStorage.hasConvertToValues = true;
						extStorage.convertToValueIsInputValueComplete = isInputValueComplete;
						extStorage.convertToValueConvertedValue = convertedValue;
						extStorage.convertToValueRetValue = retVal;

						extendedStorageRow.SetExtendedStorage(this.extenededRowStorageIndex, extStorage);
					}

					return retVal;
				}
				finally
				{
					if(valueDataRow != null)
						valueDataRow.ReleaseReaderLock();
				}
			}

			/// <summary>
			/// converts value to list of possible/probable values. Used when ConvertToValue does not 
			/// return a complete value or does not return any value. Unlike the ConvertToValue() the 
			/// values returned by ConvertToValueList are only used as values to be masked and put into the
			/// probable maps (the return values from ConvertToValueList are not used for the exact map)
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="extendedStorageRow">extended storage row for storing converted value (or null)</param>
			/// <param name="factor">strength of the return list. (if storage call for 9 digit  and an 8 didgit string is passed the factor would be 0.22 (2/9)</param>
			/// <returns>List of converted values or null</returns>
			protected List<InputT> ConvertToValueList(object value, IRowLockingWrapper valueDataRow, out decimal factor)
			{
				if(value == null || string.Empty.Equals(value))
				{
					factor = 0M;
					return null;
				}

				if ((this.createArgs.ExtendedStorageUsage & ExtendedStorageTypes.ConvertToValueList) ==
						ExtendedStorageTypes.None)
				{
					return ConvertToValueListInternal(value, out factor);
				}

				//check extened storage before doing the convert
				ExtendedStorage extStorage;
				IExtendedStorageRow extendedStorageRow = null;
				if(valueDataRow != null)
					valueDataRow.AcquireReaderLock();
				try
				{
					if(valueDataRow != null)
					{
						if (this.extenededRowStorageIndex >= 0 &&
							extendedStorageRow != null)
						{
							extStorage = extendedStorageRow.GetExtendedStorage(this.extenededRowStorageIndex) as ExtendedStorage;
							if (extStorage != null &&
								extStorage.hasConvertToValueList)
							{
								//found it return it
								factor = extStorage.convertToValueListFactor;
								return extStorage.convertToValueListRetValue;
							}
						}
						else
						{
							extStorage = null;
						}
					}
					else
					{
						extStorage = null;
					}

					//either no extenededStorage or no value in extended storage do the convert
					List<InputT> retVal = ConvertToValueListInternal(value, out factor);


					if (extendedStorageRow != null)
					{
						if (extStorage == null)
							extStorage = new ExtendedStorage();

						extStorage.hasConvertToValueList = true;
						extStorage.convertToValueListFactor = factor;
						extStorage.convertToValueListRetValue = retVal;

						extendedStorageRow.SetExtendedStorage(this.extenededRowStorageIndex, extStorage);
					}
					return retVal;
				}
				finally
				{
					if(valueDataRow != null)
						valueDataRow.ReleaseReaderLock();
				}
			}

			/// <summary>
			/// converts value to list of possible/probable values. Used when ConvertToValue does not 
			/// return a complete value or does not return any value. Unlike the ConvertToValue() the 
			/// values returned by ConvertToValueList are only used as values to be masked and put into the
			/// probable maps (the return values from ConvertToValueList are not used for the exact map)
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="factor">strength of the return list. (if storage call for 9 digit  and an 8 didgit string is passed the factor would be 0.22 (2/9)</param>
			/// <returns>List of converted values or null</returns>
			protected abstract List<InputT> ConvertToValueListInternal(object value, out decimal factor);

			/// <summary>
			/// converts value to object of the derived storage class defined data type
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="isInputValueComplete">is the input value complete, only makes sense when number of digits/charaters are defined. true if the value 
			/// being converted can be considered a complete value after the conversion.  If you had defined the type to be 4 digits and the 
			/// lookup was a string of "a1234" the converted value might be 1234 since there are 4 digits this would be considered complete. 
			/// when adding rows to internal maps if the value is complete the converted value can be used for generation of Masked Data.
			/// If the input is not complete then the convereted value need to be adjusted by either adding or removing characters before
			/// generating the Masked Data
			/// </param>
			/// <param name="convertedValue">out converted value</param>
			/// <returns>true if could convert the value</returns>
			protected abstract bool ConvertToValueInternal(object value, bool allowExtraCharacters,
								out bool isInputValueComplete, out InputT convertedValue,
								out String overflowString);
				
			/// <summary>
			/// add row to the internal maps (exact and probable)
			/// </summary>
			/// <param name="row">row to be added</param>
			protected void AddRowToMapInternal(IRowLockingWrapper rowWrapper)
			{
				try
				{
					//get value from row using sourceCoulumn
					InputT exactConvertedVal;
					List<InputT> possibleValues = null;
					object rowVal;
					rowWrapper.AcquireReaderLock();
					try
					{
						rowVal = rowWrapper.Row[createArgs.SourceColumn];
					}
					finally
					{
						rowWrapper.ReleaseReaderLock();
					}

					MatchStorageRowList rowList;
					MatchStorageList<MatchStorageRowList> addedListOfList = new MatchStorageList<MatchStorageRowList>(this.addedRowsMapRWL);

					if(this.addedRowsMapRWL.TryEnterWriteLock(60000))
					{
						try
						{
							this.addedRowsMap[rowWrapper.Row] = addedListOfList;
						}
						finally
						{
							this.addedRowsMapRWL.ExitWriteLock();
						}
					}
					else
					{
						throw new TimeoutException();
					}

					//convert value in row to storage specific converted value
					bool isInputValueComplete;
					string overflowString;

					if(this.ConvertToValue(rowVal, rowWrapper, false, out isInputValueComplete, out exactConvertedVal, out overflowString) == true)
					{

						if((this.createArgs.LookupTypes & SupportedLookupType.ExactLookup) ==
									SupportedLookupType.ExactLookup)
						{
							//if could convert add the row using the converted value as the key
							//there could be multiple rows so the key points to a list of dataRows instead of
							//just one dataRow
							if(this.exactMap.TryGetValue(exactConvertedVal, out rowList) == false)
							{
								rowList = new MatchStorageRowList(this.exactMap.InnerDictionaryRWL);
								this.exactMap.Add(exactConvertedVal, rowList);
							}
							addedListOfList.Add(rowList, true);
							rowList.Add(rowWrapper.Row, true);
						}

						//exact values are also used for the input to the probable
						if((this.createArgs.LookupTypes & SupportedLookupType.UseDataMasksForProbable) ==
									SupportedLookupType.UseDataMasksForProbable)
						{
							possibleValues = new List<InputT>();
							possibleValues.Add(exactConvertedVal);

							//if the value is not complete need to add multiple
							//values to the probable list. 
							//multiple values are return by ConvertToValueList
							if(isInputValueComplete == false)
							{
								List<InputT> tmpPossibleValues;
								Decimal factor;
								tmpPossibleValues = this.ConvertToValueList(rowVal, rowWrapper, out factor);
								if(tmpPossibleValues != null && tmpPossibleValues.Count > 0)
									possibleValues.AddRange(tmpPossibleValues);

							}
						}
					}
					else if((this.createArgs.LookupTypes & SupportedLookupType.UseDataMasksForProbable) ==
									SupportedLookupType.UseDataMasksForProbable)
					{
						//if cannot convert to a single value
						//try using the convertToValueList to get a list of probabale values
						Decimal factor;
						possibleValues = this.ConvertToValueList(rowVal, rowWrapper, out factor);
					}

					//if no probable or DataMask not supported return
					if(possibleValues == null ||
						((this.createArgs.LookupTypes & SupportedLookupType.UseDataMasksForProbable) !=
									SupportedLookupType.UseDataMasksForProbable))
						return;

					//create dataMasks for each of the possible values
					foreach(InputT curVal in possibleValues)
					{
						bool[] validAr;
						//get the dataMasks from the derived class
						DataMaskT[] maskedDataAr = this.GetMaskedDataAr(curVal, rowWrapper, out validAr);
						for(int i = 0; i < this.MaskedDataArrayLength; i++)
						{
							//for each of the values if the value
							//is not null add it to the corresponding dictionary
							if(validAr != null && validAr[i] == false)
								continue;

							MatchStorageDictionary<DataMaskT> curProbableList = probableMatchSetMapList[i];
							DataMaskT curMaskedDataVal = maskedDataAr[i];
							//data value is the key for the map pointing to the list of dataRows
							if(curProbableList.TryGetValue(curMaskedDataVal, out rowList) == false)
							{
								rowList = new MatchStorageRowList(curProbableList.InnerDictionaryRWL);
								curProbableList.Add(curMaskedDataVal, rowList);
							}

							addedListOfList.Add(rowList, true);
							rowList.Add(rowWrapper.Row, true);
						}
					}
				}
				catch(Exception exception)
				{
					try
					{
						//incase some of it got into the maps
						//try to clean up the mess
						this.RemoveRowFromMap(rowWrapper.Row);
					}
					catch
					{
					}

					EventLog.Error("{0}, {1}", exception.Message, exception.ToString());
				}
			}


			private DataMaskT[] GetMaskedDataAr(InputT curVal, IRowLockingWrapper rowWrapper, out bool[] validAr)
			{
				DataMaskT[] maskedDataArray;
				
				if ((this.createArgs.ExtendedStorageUsage & ExtendedStorageTypes.GetDataMasks) ==
						ExtendedStorageTypes.None)
				{
					validAr = null;
					maskedDataArray = new DataMaskT[this.MaskedDataArrayLength];
					this.FillMaskedDataArray(curVal, maskedDataArray, out validAr);
					return maskedDataArray;
				}

				//check extened storage before doing the convert
				ExtendedStorage extStorage;
				IExtendedStorageRow extendedStorageRow = null;

				if(rowWrapper != null)
					rowWrapper.AcquireReaderLock();
				try
				{
					if(rowWrapper != null)
					{
						extendedStorageRow = rowWrapper.Row as IExtendedStorageRow;
							
						if(this.extenededRowStorageIndex >= 0 &&
								extendedStorageRow != null)	
						{
							extStorage = extendedStorageRow.GetExtendedStorage(this.extenededRowStorageIndex) as ExtendedStorage;
							if (extStorage != null &&
								extStorage.hasGetMaskedDataAr)
							{
								//found it return it
								validAr = extStorage.GetMaskedValidAr;
								return extStorage.GetMaskedDataAr;
							}
						}
						else
						{
							extStorage = null;
						}
					}
					else
					{
						extStorage = null;
					}

					validAr = null;
					maskedDataArray = new DataMaskT[this.MaskedDataArrayLength];
					this.FillMaskedDataArray(curVal, maskedDataArray, out validAr);

					if(extendedStorageRow != null)
					{
						if (extStorage == null)
							extStorage = new ExtendedStorage();

						extStorage.hasGetMaskedDataAr = true;
						extStorage.GetMaskedValidAr = validAr;
						extStorage.GetMaskedDataAr = maskedDataArray;

						extendedStorageRow.SetExtendedStorage(this.extenededRowStorageIndex, extStorage);
					}

					return maskedDataArray;
				}
				finally
				{
					if(rowWrapper != null)
						rowWrapper.ReleaseReaderLock();
				}
			}

			protected abstract void FillMaskedDataArray(InputT curVal, DataMaskT[] maskedDataArray, out bool[] validAr);

			protected abstract string ValueToString(InputT value);

			protected virtual bool ValidateInternal(InputT valA, IRowLockingWrapper valALockWrapper, string valAStr,
													InputT valB, IRowLockingWrapper valBLockWrapper, string valBStr,
													out decimal strength)
			{

				strength = 0M;
				if(valA == null ||
					valB == null)
				{
					return false;
				}

				if(valAStr == null)
					valAStr = this.ValueToString(valA);
				if(valBStr == null)
					valBStr = this.ValueToString(valB);

				int len = (valBStr.Length < valAStr.Length) ? valBStr.Length : valAStr.Length;

				bool[] charMatchAr = new bool[len];

				for (int i = 0; i < len; i++)
				{
					if(valBStr[i] == valAStr[i])
						charMatchAr[i] = true;
				}

				len++;
				for(int i = 1; i < len; i++)
				{
					if(valBStr[valBStr.Length - i] == valAStr[valAStr.Length - i])
						charMatchAr[charMatchAr.Length - i] = true;
				}


				int numer = 0;
				foreach(bool charMatch in charMatchAr)
				{
					if(charMatch == true)
						numer++;
				}

				int denom = (valBStr.Length > valAStr.Length) ? valBStr.Length : valAStr.Length;
				if(denom == 0 ||
					numer == 0)
				{
					strength = 0M;
				}
				else if(numer == denom)
				{
					strength = 1M;
				}
				else
				{
					strength = (decimal)numer /
							(decimal)denom;
				}


				return true;
			}

			protected abstract Decimal GetMaskedDataMatchStrength(InputT curVal, int maskedDataIndex);

			protected class ExtendedStorage
			{
				public InputT convertToValueConvertedValue;
				public bool convertToValueIsInputValueComplete;

				public decimal convertToValueListFactor;
				public List<InputT> convertToValueListRetValue;
				public bool convertToValueRetValue;
				public bool hasConvertToValueList;
				public bool hasConvertToValues;

				public bool hasGetMaskedDataAr;
				public DataMaskT[] GetMaskedDataAr;
				public bool[] GetMaskedValidAr;
			}

		}
	}
}
