using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FluidTrade.Guardian;
using FluidTrade.Core;

namespace FluidTrade.Core.Matching
{
	/// <summary>
	/// CardSocialLastNameFuzzyMatcher contains a columnMatcher for 
	///		CreditCard, SocialSecurity, LastName and FirstName
	/// 
	///		CreditCard and SocialSecurity matchers have Part lookup
	///			enabled, First and Last name do not
	///			
	///		The First and Last name matchers currently are only used for validation
	///			of rows that are first looked up using CreditCard or SocialSecurity
	///			
	///		
	///		All the Matchers above are created and populated in the ctor()
	///		
	///		The one method that is exposed it FindMatch()
	///			FindMatch :	 (generalization)
	///					lookup by an exact credit card match
	///						if find exact credit card match(es)
	///							use SocialSecurity First and Last matchers to validate the row
	///								if validate passes returns the row
	///					if validation fails
	///					lookup by an exact socialSecurity
	///						if find exact socialSecurity match(es)
	///							use CreditCard, First and Last matchers to validate the row
	///								if validate passes returns the row
	///					
	///					if could not find exact match on credit card or social security
	///						lookup all probable matches of the social security
	///							validate all the probable social security matches to get match strength
	///						lookup all probably matches of the credit card
	///							validate all the probable credit card matches to get match strength
	///							
	///					return the highest match strength
	/// </summary>
	public class CardSocialLastNameFuzzyMatcher
	{
		private ColumnFuzzyMatcher cardMatcher;
		//ColumnFuzzyMatcher dobMatcher;
		private System.Data.DataColumn creditCardColumn;
		private System.Data.DataColumn firstNameColumn;
		private ColumnFuzzyMatcher firstNameMatcher;
		private System.Data.DataColumn lastNameColumn;
		private ColumnFuzzyMatcher lastNameMatcher;
		private ColumnFuzzyMatcher socialMatcher;
		private System.Data.DataColumn ssnColumn;
		//private System.Data.DataColumn dobNameColumn;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creditCardTable"></param>
		/// <param name="consumerTable"></param>
		/// <param name="creditCardRowList"></param>
		/// <param name="consumerRowList"></param>
		/// <param name="dataModelTransaction"></param>
		public CardSocialLastNameFuzzyMatcher(CreditCardDataTable creditCardTable,
												ConsumerDataTable consumerTable,
												List<RowLockingWrapper<CreditCardRow>> creditCardRowList, 
												List<RowLockingWrapper<ConsumerRow>> consumerRowList,
												IDataModelTransaction dataModelTransaction)
		{
			//get the credit card number column
			this.creditCardColumn = creditCardTable.OriginalAccountNumberColumn;
			
			//create the matcher for the credit card number.
			//will support Exact and Parts lookup types  (validation is implicit)
			this.cardMatcher = ColumnFuzzyMatcher.CreateFuzzyMatcher(new MatcherCreateArgs(16, DataCharType.DigitsOnly,
										SupportedLookupType.ExactLookup | SupportedLookupType.UseDataMasksForProbable,
										ExtendedStorageTypes.ConvertToValue | ExtendedStorageTypes.ConvertToValueList,
										this.creditCardColumn));
			
			//load up the matcher
			foreach(RowLockingWrapper<CreditCardRow> curCreditCardRow in creditCardRowList)
			{
				curCreditCardRow.AcquireReaderLock();
				try
				{
					this.cardMatcher.AddTableRowToMap(curCreditCardRow);
				}
				finally
				{
					curCreditCardRow.ReleaseReaderLock();
				}
			}

			this.ssnColumn = consumerTable.SocialSecurityNumberColumn;
			this.socialMatcher = ColumnFuzzyMatcher.CreateFuzzyMatcher(new MatcherCreateArgs(9, DataCharType.DigitsOnly,
				SupportedLookupType.ExactLookup | SupportedLookupType.UseDataMasksForProbable,
										ExtendedStorageTypes.ConvertToValue | ExtendedStorageTypes.ConvertToValueList,
										this.ssnColumn));


			
			//no parts for these
			this.lastNameColumn = consumerTable.LastNameColumn;
			this.lastNameMatcher = ColumnFuzzyMatcher.CreateFuzzyMatcher(new MatcherCreateArgs(12, DataCharType.String,
				SupportedLookupType.ValidateOnly,
										ExtendedStorageTypes.ConvertToValue | ExtendedStorageTypes.ConvertToValueList,
										this.lastNameColumn));


			this.firstNameColumn = consumerTable.FirstNameColumn;
			this.firstNameMatcher = ColumnFuzzyMatcher.CreateFuzzyMatcher(new MatcherCreateArgs(12, DataCharType.String,
				SupportedLookupType.ValidateOnly,
										ExtendedStorageTypes.ConvertToValue | ExtendedStorageTypes.ConvertToValueList,
										this.firstNameColumn));


			foreach(RowLockingWrapper<ConsumerRow> consumerRow in consumerRowList)
			{
				consumerRow.AcquireReaderLock();
				try
				{
					this.firstNameMatcher.AddTableRowToMap(consumerRow);
					this.lastNameMatcher.AddTableRowToMap(consumerRow);
					this.socialMatcher.AddTableRowToMap(consumerRow);
				}
				finally
				{
					consumerRow.ReleaseReaderLock();
				}
			}

			//this.dobMatcher = ColumnFuzzyMatcher.CreateFuzzyMatcher(ColumnFuzzyMatcher.DataCharType.DateTime,
			//    ColumnFuzzyMatcher.SupportedLookupType.ValidateOnly,
			//                            ds.Consumer.LastNameColumn);
			//this.dobMatcher.AddTableRowsToMap();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creditCardRowToFind"></param>
		/// <param name="consumerRowToFind"></param>
		/// <param name="dataModelTransaction"></param>
		/// <param name="matchThreshold"></param>
		/// <returns></returns>
		public List<MatchResult> FindMatch(RowLockingWrapper<CreditCardRow> creditCardRowToFind, RowLockingWrapper<ConsumerRow> consumerRowToFind,
											IDataModelTransaction dataModelTransaction, Decimal matchThreshold)
		{
			//check exact
			object ccNum;
			creditCardRowToFind.AcquireReaderLock();
			try
			{
				ccNum = creditCardRowToFind.Row[this.creditCardColumn];
			}
			finally
			{
				creditCardRowToFind.ReleaseReaderLock();
			}

			object ssnNum;
			object lastName;
			consumerRowToFind.AcquireReaderLock();
			try
			{
				ssnNum = consumerRowToFind.Row[this.ssnColumn];
				lastName = consumerRowToFind.Row[this.lastNameColumn];
			}
			finally
			{
				consumerRowToFind.ReleaseReaderLock();
			}

			List<MatchResult> results = new List<MatchResult>();

			//search by exact CreditCard
			this.SearchByExactCreditCard(creditCardRowToFind, consumerRowToFind, ccNum, ssnNum, lastName, results, dataModelTransaction, matchThreshold);
			
			//System.Diagnostics.Trace.WriteLine(
			//    string.Format("{0}  CreditCard search: {1}  {2}  {3}", System.Threading.Thread.CurrentThread.Name,
			//                        ccNum, ssnNum, lastName));
			//System.Diagnostics.Trace.Indent();
			//foreach(MatchResult mr in results)
			//    System.Diagnostics.Trace.WriteLine(mr.ToString());
			//System.Diagnostics.Trace.Unindent();

			//if have a ~100% result continue?
			int resultIndex = 0;
			for (; resultIndex < results.Count; resultIndex++)
			{
				MatchResult mr = results[resultIndex];
				if (mr.Strength >= 1M)
				{
					results.Sort();
					return results;
				}
			}
			//search by exact SSN
			this.SearchByExactSocialSecurity(creditCardRowToFind, consumerRowToFind, ccNum, ssnNum, lastName, results, dataModelTransaction, matchThreshold);

			//System.Diagnostics.Trace.WriteLine(
			//        string.Format("{0}  SSN search: {1}  {2}  {3}", System.Threading.Thread.CurrentThread.Name,
			//                            ccNum, ssnNum, lastName));
			//System.Diagnostics.Trace.Indent();
			//foreach(MatchResult mr in results)
			//    System.Diagnostics.Trace.WriteLine(mr.ToString());
			//System.Diagnostics.Trace.Unindent();

			//if have a ~100% result continue? could not have an exact match here
			//and not have had it in the cc, but could have a high % match
			for (; resultIndex < results.Count; resultIndex++)
			{
				MatchResult mr = results[resultIndex];
				if (mr.Strength >= 1M)
				{
					results.Sort();
					return results;
				}
			}

			this.SearchByProbableSocialSecurity(creditCardRowToFind, consumerRowToFind, ccNum, ssnNum, lastName, results, dataModelTransaction, matchThreshold);

			//System.Diagnostics.Trace.WriteLine(
			//        string.Format("{0}  Probable CreditCard search: {1}  {2}  {3}", System.Threading.Thread.CurrentThread.Name,
			//                            ccNum, ssnNum, lastName));
			//System.Diagnostics.Trace.Indent();
			//foreach(MatchResult mr in results)
			//    System.Diagnostics.Trace.WriteLine(mr.ToString());
			//System.Diagnostics.Trace.Unindent();

			this.SearchByProbableCreditCard(creditCardRowToFind, consumerRowToFind, ccNum, ssnNum, lastName, results, dataModelTransaction, matchThreshold);

			results.Sort();
			
			//System.Diagnostics.Trace.WriteLine(
			//                string.Format("{0}  Probable CreditCard search: {1}  {2}  {3}", System.Threading.Thread.CurrentThread.Name,
			//                                    ccNum, ssnNum, lastName));

			//System.Diagnostics.Trace.Indent();
			//foreach(MatchResult mr in results)
			//    System.Diagnostics.Trace.WriteLine(mr.ToString());
			//System.Diagnostics.Trace.Unindent();

			if (results.Count == 0)
				return null;

			return results;
		}

		private void SearchByExactCreditCard(RowLockingWrapper<CreditCardRow> creditCardRowToFind, RowLockingWrapper<ConsumerRow> consumerRowToFind, 
											object ccNum, object ssnNum, object lastName, List<MatchResult> results,
											IDataModelTransaction dataModelTransaction, Decimal matchThreshold)
		{
			List<MatchInfo> ccMiList = cardMatcher.FindExact(ccNum, creditCardRowToFind, dataModelTransaction);
			if (ccMiList != null)
			{
				//remove the input row
				for (int i = ccMiList.Count - 1; i >= 0; i--)
				{
					if (ccMiList[i].row == creditCardRowToFind.Row)
						ccMiList.RemoveAt(i);
				}
			}

			GetMatchResultsForCreditCard(creditCardRowToFind, consumerRowToFind, ssnNum, lastName, results, ccMiList, dataModelTransaction, matchThreshold);
		}

		private void SearchByProbableCreditCard(RowLockingWrapper<CreditCardRow> creditCardRowToFind, RowLockingWrapper<ConsumerRow> consumerRowToFind,
											object ccNum, object ssnNum, object lastName, List<MatchResult> results,
											IDataModelTransaction dataModelTransaction, Decimal matchThreshold)
		{
			Dictionary<IRow, MatchInfo> probableMap = this.cardMatcher.FindProbable(ccNum, creditCardRowToFind, true, dataModelTransaction);
			if(probableMap == null)
				return;

			probableMap.Remove(creditCardRowToFind.Row);

			GetMatchResultsForCreditCard(creditCardRowToFind, consumerRowToFind, ccNum, lastName, results, probableMap.Values, dataModelTransaction, matchThreshold);
		}

		private void GetMatchResultsForCreditCard(RowLockingWrapper<CreditCardRow> creditCardRowToFind, RowLockingWrapper<ConsumerRow> consumerRowToFind, object ssnNum,
			object lastName, List<MatchResult> results, IEnumerable<MatchInfo> ccMiList,
											IDataModelTransaction dataModelTransaction, Decimal matchThreshold)
		{
			if (ccMiList == null)
				return;

			//!!!RM what is faster using the .Consumer row
			//or using the ConsumerRow Fuz Matcher
			foreach (MatchInfo ccMi in ccMiList)
			{
				ConsumerRow miConsumerRow;
				if(dataModelTransaction != null)
					((CreditCardRow)ccMi.row).AcquireReaderLock(dataModelTransaction.TransactionId, ((CreditCardRow)ccMi.row).LockTimeout);

				if(ccMi.row.RowState == DataRowState.Deleted ||
					ccMi.row.RowState == DataRowState.Detached)
				{
					if(dataModelTransaction != null)
						((CreditCardRow)ccMi.row).ReleaseReaderLock(dataModelTransaction.TransactionId);

					continue;
				}
				try
				{
					miConsumerRow = ((CreditCardRow)ccMi.row).ConsumerRow;
				}
				finally
				{
					if(dataModelTransaction != null)
						((CreditCardRow)ccMi.row).ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				if(miConsumerRow == null)
					continue;

				RowLockingWrapper<ConsumerRow> consumerRowWrapper = new RowLockingWrapper<ConsumerRow>(miConsumerRow, dataModelTransaction);

				consumerRowWrapper.AcquireReaderLock();
				if(consumerRowWrapper.Row.RowState == DataRowState.Deleted ||
					consumerRowWrapper.Row.RowState == DataRowState.Detached)
				{
					consumerRowWrapper.ReleaseReaderLock();
					continue;
				}

				try
				{
					MatchInfo ccSsnMi = socialMatcher.Validate(ssnNum, consumerRowWrapper, dataModelTransaction);
					MatchInfo ccLnMi = this.lastNameMatcher.Validate(lastName, consumerRowWrapper, dataModelTransaction);

					MatchResult mr = new MatchResult(consumerRowToFind.TypedRow, creditCardRowToFind.TypedRow, ccMi, ccSsnMi, ccLnMi);
					//if(mr.Strength < 1M)
					//{
						mr.AddFirstNameMatchInfo(this.firstNameMatcher.Validate(consumerRowToFind.Row[this.firstNameColumn], consumerRowWrapper, dataModelTransaction));
					//}

					if(mr.Strength >= matchThreshold)
						results.Add(mr);
				}
				finally
				{
					consumerRowWrapper.ReleaseReaderLock();
				}
			}
		}

		private void SearchByProbableSocialSecurity(RowLockingWrapper<CreditCardRow> creditCardRowToFind, RowLockingWrapper<ConsumerRow> consumerRowToFind,
											object ccNum, object ssnNum, object lastName, List<MatchResult> results,
											IDataModelTransaction dataModelTransaction, Decimal matchThreshold)
		{
			Dictionary<IRow, MatchInfo> probableMap = this.socialMatcher.FindProbable(ssnNum, consumerRowToFind, true, dataModelTransaction);
			if(probableMap == null)
				return;

			probableMap.Remove(consumerRowToFind.Row);

			GetMatchResultsForSocial(creditCardRowToFind, consumerRowToFind, ccNum, lastName, results, probableMap.Values, dataModelTransaction, matchThreshold);
		}

		private void SearchByExactSocialSecurity(RowLockingWrapper<CreditCardRow> creditCardRowToFind, RowLockingWrapper<ConsumerRow> consumerRowToFind,
											object ccNum, object ssnNum, object lastName, List<MatchResult> results,
											IDataModelTransaction dataModelTransaction, Decimal matchThreshold)
		{
			List<MatchInfo> snMiList = this.socialMatcher.FindExact(ssnNum, consumerRowToFind, dataModelTransaction);
			if (snMiList != null)
			{
				//remove the input row
				for (int i = snMiList.Count - 1; i >= 0; i--)
				{
					if(snMiList[i].row == consumerRowToFind.Row)
						snMiList.RemoveAt(i);
				}
			}

			GetMatchResultsForSocial(creditCardRowToFind, consumerRowToFind, ccNum, lastName, results, snMiList, dataModelTransaction, matchThreshold);
		}

		private void GetMatchResultsForSocial(RowLockingWrapper<CreditCardRow> creditCardRowToFind, 
									RowLockingWrapper<ConsumerRow> consumerRowToFind, 
									object ccNum, object lastName,
									List<MatchResult> results, IEnumerable<MatchInfo> snMiList,
									IDataModelTransaction dataModelTransaction, Decimal matchThreshold)
		{
			if (snMiList == null)
				return;

			//!!!RM what is faster using the .Consumer row
			//or using the ConsumerRow Fuz Matcher
			foreach (MatchInfo snMi in snMiList)
			{
				ConsumerRow miConsumerRow = (ConsumerRow)snMi.row;

				RowLockingWrapper<ConsumerRow> miConsumerRowWrapper = new RowLockingWrapper<ConsumerRow>(miConsumerRow, dataModelTransaction);
				MatchInfo ssnLnMi = this.lastNameMatcher.Validate(lastName, miConsumerRowWrapper, dataModelTransaction);

				MatchInfo ssnFnMi = null;

				miConsumerRowWrapper.AcquireReaderLock();
				if(miConsumerRow.RowState == DataRowState.Deleted ||
					miConsumerRow.RowState == DataRowState.Detached)
					continue;
				try
				{
					CreditCardRow[] ccRows = miConsumerRow.GetCreditCardRows_NoLockCheck();

					if(ccRows.Length > 0)
					{
						foreach(CreditCardRow snCcRow in ccRows)
						{
							RowLockingWrapper<CreditCardRow> snCcRowWrappper = new RowLockingWrapper<CreditCardRow>(snCcRow, dataModelTransaction);

							MatchInfo ssnCcMi = this.cardMatcher.Validate(ccNum, snCcRowWrappper, dataModelTransaction);
							MatchResult mr = new MatchResult(consumerRowToFind.TypedRow, creditCardRowToFind.TypedRow, ssnCcMi, snMi, ssnLnMi);
							//if(mr.Strength < 1M)
							//{
								if(ssnFnMi == null)
								{
									consumerRowToFind.AcquireReaderLock();
									try
									{
										ssnFnMi = this.firstNameMatcher.Validate(consumerRowToFind.Row[this.firstNameColumn], miConsumerRowWrapper, dataModelTransaction);
									}
									finally
									{
										consumerRowToFind.ReleaseReaderLock();
									}
								}
								mr.AddFirstNameMatchInfo(ssnFnMi);
							//}
					
							if(mr.Strength >= matchThreshold)
								results.Add(mr);
						}
					}
					else
					{
						MatchResult mr = new MatchResult(consumerRowToFind.TypedRow, creditCardRowToFind.TypedRow, null, snMi, ssnLnMi);

						consumerRowToFind.AcquireReaderLock();
						try
						{
							mr.AddFirstNameMatchInfo(this.firstNameMatcher.Validate(consumerRowToFind.Row[this.firstNameColumn], miConsumerRowWrapper, dataModelTransaction));
						}
						finally
						{
							consumerRowToFind.ReleaseReaderLock();
						}

						if(mr.Strength >= matchThreshold)
							results.Add(mr);
					}
				}
				finally
				{
					miConsumerRowWrapper.ReleaseReaderLock();
				}
			}
		}

		#region Nested type: MatchResult

		/// <summary>
		/// 
		/// </summary>
		public class MatchResult : IComparable<MatchResult>
		{
			private const decimal consumerWeightMultiplier = 1.0M;
			private const decimal ccWeight  = 0.40M;

			//private const decimal dobWeight = 0.15M;
			private const decimal firstNameWeight = 0.05M;
			private const decimal lastNameWeight = 0.15M;
			private const decimal ssnWeight = 0.40M;
			
			MatchInfo creditCardMatchInfo;
			//MatchInfo dobMatchInfo;
			MatchInfo firstNameMatchInfo;
			MatchInfo lastNameMatchInfo;
			//private decimal rawStrength;
			MatchInfo socialMatchInfo;
			private decimal strength;
			private string strengthDetails;

			private ConsumerRow consumerRowToFind;
			private CreditCardRow creditCardRowToFind;
			
			/// <summary>
			/// 
			/// </summary>
			/// <param name="consumerRowToFind"></param>
			/// <param name="creditCardRowToFind"></param>
			/// <param name="creditCardMatchInfo"></param>
			/// <param name="socialMatchInfo"></param>
			/// <param name="lastNameMatchInfo"></param>
			public MatchResult(ConsumerRow consumerRowToFind,
								CreditCardRow creditCardRowToFind,
								MatchInfo creditCardMatchInfo,
								MatchInfo socialMatchInfo,
								MatchInfo lastNameMatchInfo)
			{
				this.consumerRowToFind = consumerRowToFind;
				this.creditCardRowToFind = creditCardRowToFind;
				this.creditCardMatchInfo = creditCardMatchInfo;
				this.socialMatchInfo = socialMatchInfo;
				this.lastNameMatchInfo = lastNameMatchInfo;

				this.ComputeStrength();
			}

			/// <summary>
			/// 
			/// </summary>
			public CreditCardRow MatchedCreditCardRow
			{
				get
				{
					if (this.creditCardMatchInfo != null)
						return this.creditCardMatchInfo.row as CreditCardRow;

					return null;
				}
			}

			/// <summary>
			/// 
			/// </summary>
			public ConsumerRow MatchedConsumerRow
			{
				get
				{
					if (this.socialMatchInfo != null)
						return this.socialMatchInfo.row as ConsumerRow;

					if (this.lastNameMatchInfo != null)
						return this.lastNameMatchInfo.row as ConsumerRow;

					if (this.firstNameMatchInfo != null)
						return this.firstNameMatchInfo.row as ConsumerRow;

					//if (this.dobMatchInfo != null)
					//    return this.dobMatchInfo.row as ConsumerRow;

					return null;
				}
			}

			/// <summary>
			/// 
			/// </summary>
			public decimal Strength
			{
				get
				{
					return this.strength;
				}
			}

			/// <summary>
			/// 
			/// </summary>
			public string StrengthDetails
			{
				get
				{
					return this.strengthDetails;
				}
			}


			/// <summary>
			/// 
			/// </summary>
			public ConsumerRow ConsumerRowToFind
			{
				get
				{
					return this.consumerRowToFind;
				}
			}

			/// <summary>
			/// 
			/// </summary>
			public CreditCardRow CreditCardRowToFind
			{
				get
				{
					return this.creditCardRowToFind;
				}
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public int CompareTo(MatchResult other)
			{
				if (other == null)
					return -1;

				return other.strength.CompareTo(this.strength);
			}

			
			private void ComputeStrength()
			{
				decimal accountNumStrength = (this.creditCardMatchInfo == null) ? 0M : this.creditCardMatchInfo.strength;
				decimal creditCardStrength = accountNumStrength * ccWeight;


				decimal socialStrength = (this.socialMatchInfo == null) ? 0M : this.socialMatchInfo.strength;
				decimal lastNameStrength = (this.lastNameMatchInfo == null) ? 0M : this.lastNameMatchInfo.strength;
				decimal firstNameStrength = (this.firstNameMatchInfo == null) ? 0M : this.firstNameMatchInfo.strength;

				decimal consumerStrength = socialStrength * ssnWeight +
											lastNameStrength * lastNameWeight +
											firstNameStrength * firstNameWeight;

				this.strengthDetails = string.Format("OA:{0:0.0}:{1:0.0},SS:{2:0.0}:{3:0.0},LN:{4:0.0}:{5:0.0},FN:{6:0.0}:{7:0.0}",
														ccWeight * accountNumStrength * 100M, accountNumStrength * 100M,
														ssnWeight * socialStrength * 100M, socialStrength * 100M,
														lastNameWeight * lastNameStrength * 100M, lastNameStrength * 100M,
														firstNameWeight * firstNameStrength * 100M, firstNameStrength * 100M);
				//if consumer strength is less than 1
				////try to factor in other extra credit ie:firstname
				//if(consumerStrength < 1M)
				//{				
				//    decimal fnVal = ((this.firstNameMatchInfo == null) ? 0M : this.firstNameMatchInfo.strength) * firstNameWeight;

				//    decimal adjustedConsumerStrength = (fnVal + consumerStrength) / (ssnWeight + lastNameWeight + firstNameWeight);

				//    if(adjustedConsumerStrength > consumerStrength)
				//    {
				//        consumerStrength = adjustedConsumerStrength;
				//    }				
				//}
				consumerStrength *= consumerWeightMultiplier;

				this.strength = consumerStrength + creditCardStrength;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="firstNameMatchInfo"></param>
			public void AddFirstNameMatchInfo(MatchInfo firstNameMatchInfo)
			{
				this.firstNameMatchInfo = firstNameMatchInfo;
				this.ComputeStrength();
			}

			///// <summary>
			///// 
			///// </summary>
			///// <param name="dobMatchInfo"></param>
			//public void AddDobMatchInfo(MatchInfo dobMatchInfo)
			//{
			//    this.dobMatchInfo = dobMatchInfo;
			//    this.ComputeStrength();
			//}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{

				return string.Format("{0:0.00}%\r\n" +
									"     CC:{2}  {3}  {4}\r\n" +
									"     SS:{5}  {6}  {7}\r\n" +
									"     LN:{8}  {9}  {10}\r\n" +
									"     FN:{11}  {12}  {13}\r\n",
					this.Strength, "unused",
					(this.creditCardMatchInfo == null) ? "" : this.creditCardMatchInfo.strength.ToString(),
						(this.CreditCardRowToFind == null) ? "" : this.CreditCardRowToFind["OriginalAccountNumber"],					
						(this.MatchedCreditCardRow == null) ? "" : this.MatchedCreditCardRow["OriginalAccountNumber"],

					(this.socialMatchInfo == null) ? "" : this.socialMatchInfo.strength.ToString(),
						(this.ConsumerRowToFind == null) ? "" : this.ConsumerRowToFind["SocialSecurityNumber"],
						(this.MatchedConsumerRow == null) ? "" : this.MatchedConsumerRow["SocialSecurityNumber"],

					(this.lastNameMatchInfo == null) ? "" : this.lastNameMatchInfo.strength.ToString(),
						(this.ConsumerRowToFind == null) ? "" : this.ConsumerRowToFind["LastName"],
						(this.MatchedConsumerRow == null) ? "" : this.MatchedConsumerRow["LastName"],

					(this.firstNameMatchInfo == null) ? "" : this.firstNameMatchInfo.strength.ToString(),
						(this.ConsumerRowToFind == null) ? "" : this.ConsumerRowToFind["FirstName"],
						(this.MatchedConsumerRow == null) ? "" : this.MatchedConsumerRow["FirstName"]
						);
			}
		}

		#endregion

		internal bool UpdateConsumer(RowLockingWrapper<ConsumerRow> consumerRowWrapper, 
									FluidTrade.Core.IDataModelTransaction dataModelTransaction, bool isValid)
		{
			this.socialMatcher.RemoveRowFromMap(consumerRowWrapper.Row);
			this.lastNameMatcher.RemoveRowFromMap(consumerRowWrapper.Row);
			this.firstNameMatcher.RemoveRowFromMap(consumerRowWrapper.Row);

			if(isValid == false)
				return false;

			consumerRowWrapper.AcquireReaderLock();
			try
			{

				//if not valid or rowState is delete/detach all can do is remove the row
				if(	consumerRowWrapper.Row.RowState == DataRowState.Deleted ||
					consumerRowWrapper.Row.RowState == DataRowState.Detached)
					return false;


				this.socialMatcher.AddTableRowToMap(consumerRowWrapper);
				this.lastNameMatcher.AddTableRowToMap(consumerRowWrapper);
				this.firstNameMatcher.AddTableRowToMap(consumerRowWrapper);
			}
			finally
			{
				consumerRowWrapper.ReleaseReaderLock();
			}

			return true;
		}
		internal bool UpdateCreditCard(RowLockingWrapper<CreditCardRow> creditCardRowWrapper,
							FluidTrade.Core.IDataModelTransaction dataModelTransaction, bool isValid)
		{
			this.cardMatcher.RemoveRowFromMap(creditCardRowWrapper.Row);

			if(isValid == false)
				return false;

			//System.Diagnostics.Trace.WriteLine(
			//    string.Format("{0}  Updating CreditCard: {1}", System.Threading.Thread.CurrentThread.Name,
			//                        creditCardRowWrapper.TypedRow["OriginalAccountNumber"]));

			creditCardRowWrapper.AcquireReaderLock();
			try
			{
				//if not valid or rowState is delete/detach all can do is remove the row
				if(creditCardRowWrapper.Row.RowState == DataRowState.Deleted ||
					creditCardRowWrapper.Row.RowState == DataRowState.Detached)
					return false;


				this.cardMatcher.AddTableRowToMap(creditCardRowWrapper);
			}
			finally
			{
				creditCardRowWrapper.ReleaseReaderLock();
			}

			return true;
		}

	}
}
