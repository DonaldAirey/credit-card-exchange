namespace FluidTrade.Guardian.Windows
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// GUI Element representing a Debt Negotiator
	/// </summary>
	public class DebtNegotiator : DebtClass
	{

		private long rowVersion;

		/// <summary>
		/// Create a debt negotatior object based on an entityRow.
		/// </summary>
		/// <param name="entityRow">The row to base the object on.</param>
        public DebtNegotiator(EntityRow entityRow) : base(entityRow)
		{


		}

		/// <summary>
		/// Create a duplicate debt class.
		/// </summary>
		/// <param name="source">The original debt class.</param>
		public DebtNegotiator(DebtNegotiator source)
			: base(source)
		{

			this.rowVersion = source.RowVersion;

		}
		/// <summary>
		/// Primary Identifier of this object.
		/// </summary>
		public Guid DebtNegotiatorId { get { return this.EntityId; } }

		/// <summary>
		/// The record type used for importing.
		/// </summary>
		protected override Type ImportRecordType
		{

			get { return typeof(TradingSupportReference.DebtNegotiatorRecord); }

		}

		/// <summary>
		/// The record type used for importing.
		/// </summary>
		protected override Stream ImportSchema
		{

			get { return Assembly.GetExecutingAssembly().GetManifestResourceStream(@"FluidTrade.Guardian.Resources.DebtNegotiatorImportSchema.xsd"); }

		}

		/// <summary>
		/// The row version of the debt negotiator row this object is based on.
		/// </summary>
		public new long RowVersion
		{

			get { return this.rowVersion; }

		}

		/// <summary>
		/// Commit any changes to the debt class to the server.
		/// </summary>
		protected override void CommitDebtClass()
		{

			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			TradingSupportReference.DebtNegotiator record = new TradingSupportReference.DebtNegotiator();

			this.PopulateRecord(record);

			if (this.EntityId == Guid.Empty)
			{

				MethodResponseArrayOfguid response = tradingSupportClient.CreateDebtNegotiator(new TradingSupportReference.DebtNegotiator[] { record });

				if (!response.IsSuccessful)
					Entity.ThrowErrorInfo(response.Errors[0]);

			}
			else
			{

				MethodResponseErrorCode response = tradingSupportClient.UpdateDebtNegotiator(new TradingSupportReference.DebtNegotiator[] { record });

				if (!response.IsSuccessful)
					Entity.ThrowErrorInfo(response.Errors[0]);

			}

			tradingSupportClient.Close();

		}

		/// <summary>
		/// Copies the data from one entity to another.
		/// </summary>
		/// <param name="entity">The source entity.</param>
		public override void Copy(GuardianObject entity)
		{

			base.Copy(entity);
			this.rowVersion = (entity as DebtNegotiator).RowVersion;

		}

		/// <summary>
		/// Create a new DebtNegotiator in the data model.
		/// </summary>
		/// <param name="dataModel">The data model client to create the negotiator with.</param>
		/// <param name="typeId">The type-id of the DebtNegotiator type.</param>
		/// <param name="parentId">The entityId of the parent entity.</param>
		/// <param name="tenantId"></param>
		/// <returns>The entity-id of the new debt negotiator.</returns>
		public static new Guid Create(DataModelClient dataModel, Guid typeId, Guid parentId, Guid tenantId)
		{

			Guid entityId = Guid.Empty;
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				TradingSupportReference.DebtNegotiator record = new TradingSupportReference.DebtNegotiator();

				lock (DataModel.SyncRoot)
				{

					DebtClassRow parent = DataModel.DebtClass.DebtClassKey.Find(parentId);

					record.ConfigurationId = "Default";
					record.Name = "New Debt Negotiator";
					record.ParentId = parentId;
					record.TenantId = tenantId;

					if (parent != null)
					{

						record.Address1 = parent.IsAddress1Null() ? null : parent.Address1;
						record.Address2 = parent.IsAddress2Null() ? null : parent.Address2;
						record.BankAccountNumber = parent.IsBankAccountNumberNull() ? null : parent.BankAccountNumber;
						record.BankRoutingNumber = parent.IsBankRoutingNumberNull() ? null : parent.BankRoutingNumber;
						record.City = parent.IsCityNull() ? null : parent.City;
						record.CompanyName = parent.IsCompanyNameNull() ? null : parent.CompanyName;
						record.ContactName = parent.IsContactNameNull() ? null : parent.ContactName;
						record.Department = parent.IsDepartmentNull() ? null : parent.Department;
						record.Email = parent.IsEmailNull() ? null : parent.Email;
						record.Fax = parent.IsFaxNull() ? null : parent.Fax;
						record.ForBenefitOf = parent.IsForBenefitOfNull() ? null : parent.ForBenefitOf;
						record.Phone = parent.IsPhoneNull() ? null : parent.Phone;
						record.PostalCode = parent.IsPostalCodeNull() ? null : parent.PostalCode;
						record.Province = parent.IsProvinceIdNull() ? null : (Guid?)parent.ProvinceId;

					}

				}

				MethodResponseArrayOfguid response = tradingSupportClient.CreateDebtNegotiator(new TradingSupportReference.DebtNegotiator[] { record });
				if (response.IsSuccessful)
					entityId = response.Result[0];
				else
					throw new Exception(String.Format("Server error: {0}, {1}", response.Errors[0].ErrorCode, response.Errors[0].Message));

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
				throw;

			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

			return entityId;

		}

		/// <summary>
		/// Delete an set of debt negotiators.
		/// </summary>
		/// <param name="debtNegotiators">The set of working orders.</param>
		/// <returns>The actual bulk size used.</returns>
		protected override Int32 Delete(List<GuardianObject> debtNegotiators)
		{

			GuardianObject failedObject = null;
			TradingSupportReference.DebtNegotiator[] records = new TradingSupportReference.DebtNegotiator[debtNegotiators.Count];
			Dictionary<TradingSupportReference.DebtNegotiator, DebtNegotiator> recordsToNegotiators =
				new Dictionary<TradingSupportReference.DebtNegotiator, DebtNegotiator>();
			Int32 sentSize;

			for (Int32 index = 0; index < records.Length; ++index)
			{

				DebtNegotiator debtNegotiator = debtNegotiators[0] as DebtNegotiator;

				records[index] = new TradingSupportReference.DebtNegotiator();
				debtNegotiator.PopulateRecord(records[index]);
				recordsToNegotiators[records[index]] = debtNegotiator;
				debtNegotiators.RemoveAt(0);

			}

			try
			{

				MethodResponseErrorCode response;

				response = NetworkHelper.Attempt<MethodResponseErrorCode>(
					(client, a) =>
						client.DeleteDebtNegotiator(a as TradingSupportReference.DebtNegotiator[]),
					records,
					true,
					out sentSize);

				if (!response.IsSuccessful)
				{

					List<TradingSupportReference.DebtNegotiator> retryRecords = new List<TradingSupportReference.DebtNegotiator>();

					foreach (ErrorInfo errorInfo in response.Errors)
					{

						// The bulk index is an index into the set we sent, which may be smaller than the set passed in.
						failedObject = recordsToNegotiators[records[errorInfo.BulkIndex]];

						// If the error's "just" a deadlock, we should retry it.
						if (errorInfo.ErrorCode == ErrorCode.Deadlock)
						{

							retryRecords.Add(records[errorInfo.BulkIndex]);

						}
						else if (errorInfo.ErrorCode == ErrorCode.RecordExists)
						{

							throw new HasSettlementsException(this.ToString() + " has settled accounts");

						}
						// We can safely ignore not-found errors (we are deleting after all), but if the error's more severe, forget the whole
						// thing and throw up the error.
						else if (errorInfo.ErrorCode != ErrorCode.RecordNotFound)
						{

							GuardianObject.ThrowErrorInfo(response.Errors[0]);

						}

					}

					records = retryRecords.ToArray();

				}

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				throw new DeleteException(failedObject, exception);

			}

			return sentSize;

		}

		/// <summary>
		/// Delete the debt holder proper.
		/// </summary>
		protected override void DeleteDebtClass()
		{

			TradingSupportClient client = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			base.DeleteDebtClass();

			MethodResponseErrorCode response = client.DeleteDebtNegotiator(new TradingSupportReference.DebtNegotiator[] { new TradingSupportReference.DebtNegotiator
			{
				RowId = this.EntityId,
				RowVersion = this.RowVersion
			} });

			if (!response.IsSuccessful)
				if (response.Errors.Length > 0)
					Entity.ThrowErrorInfo(response.Errors[0]);
				else
					Entity.ThrowErrorInfo(response.Result);

			client.Close();

		}

		/// <summary>
		/// Load debt holder specific information.
		/// </summary>
		protected override void FinishLoad()
		{

			DebtNegotiatorRow debtNegotiatorRow = DataModel.DebtNegotiator.DebtNegotiatorKey.Find(this.EntityId);

			base.FinishLoad();

			this.rowVersion = debtNegotiatorRow.RowVersion;

		}

		/// <summary>
		/// Get the import translation information for this debt class.
		/// </summary>
		/// <returns>The translation table.</returns>
		public override Dictionary<String, String> GetEffectiveImportTranslation()
		{

			Dictionary<String, String> translation = new Dictionary<String, String>();

			// The default translation is the "identity" translation.
			{

				translation["AccountBalance"] = "AccountBalance";
				translation["AccountCode"] = "AccountCode";
				translation["Address1"] = "Address1";
				translation["Address2"] = "Address2";
				translation["BankAccountNumber"] = "BankAccountNumber";
				translation["BankRoutingNumber"] = "BankRoutingNumber";
				translation["City"] = "City";
				translation["CustomerCode"] = "CustomerCode";
				translation["DateOfBirth"] = "DateOfBirth";
				translation["DebtHolder"] = "DebtHolder";
				translation["FirstName"] = "FirstName";
				translation["IsEmployed"] = "IsEmployed";
				translation["LastName"] = "LastName";
				translation["MiddleName"] = "MiddleName";
				translation["OriginalAccountNumber"] = "OriginalAccountNumber";
				translation["PhoneNumber"] = "PhoneNumber";
				translation["PostalCode"] = "PostalCode";
				translation["ProvinceCode"] = "ProvinceCode";
				translation["SavingsBalance"] = "SavingsBalance";
				translation["SavingsEntityCode"] = "SavingsEntityCode";
				translation["SocialSecurityNumber"] = "SocialSecurityNumber";
				translation["Suffix"] = "Suffix";
				translation["Salutation"] = "Salutation";
				translation["VendorCode"] = "VendorCode";

			}

			return translation;

		}

		/// <summary>
		/// Determine whether this debt negotiator contains settled accounts.
		/// </summary>
		/// <returns>True if the debt negotiator contains settled accounts.</returns>
		protected override Boolean HasSettledAccounts()
		{

			Boolean hasSettledAccounts = false;

			foreach (MatchRow matchRow in DataModel.Blotter.BlotterKey.Find(this.EntityId).GetMatchRows())
				foreach (ConsumerTrustNegotiationRow consumerTrustNegotiationRow in matchRow.GetConsumerTrustNegotiationRows())
					if (consumerTrustNegotiationRow.GetConsumerTrustSettlementRows().Length > 0)
					{

						hasSettledAccounts = true;
						break;

					}

			return hasSettledAccounts;

		}

		/// <summary>
		/// Handle the actual import call and error recording.
		/// </summary>
		/// <param name="records">The records to import</param>
		/// <param name="sentSize">The actual bulk size used.</param>
		/// <returns>The errors encountered.</returns>
		protected override Dictionary<object, string> ImportRecord(Array records, out Int32 sentSize)
		{

			Dictionary<object, string> errors = new Dictionary<object, string>();
			MethodResponseArrayOfguid response;

			response = NetworkHelper.Attempt<MethodResponseArrayOfguid>(
				(client, a) =>
					client.ImportDebtNegotiatorRecords(records as DebtNegotiatorRecord[]),
				records,
				false,
				out sentSize);

			foreach (ErrorInfo error in response.Errors)
				errors[records.GetValue(error.BulkIndex)] = error.Message;

			return errors;

		}

		/// <summary>
		/// If there's no debt negotiator row yet, we need to finish loading up the object later.
		/// </summary>
		/// <returns>True if we can't find a debt negotiator row for the entityId.</returns>
		protected override bool NeedLateLoad()
		{

			return DataModel.DebtNegotiator.DebtNegotiatorKey.Find(this.EntityId) == null;

		}

	}

}
