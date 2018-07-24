namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Reflection;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;

    /// <summary>
    /// GUI Element representing a Debt Holder
    /// </summary>
    public class DebtHolder : DebtClass
    {

		private long rowVersion;

		/// <summary>
		/// Create a new debt holder based on an EntityRow.
		/// </summary>
		/// <param name="entityRow"></param>
        public DebtHolder(EntityRow entityRow) : base(entityRow)
        {


		}

		/// <summary>
		/// Create a duplicate debt class.
		/// </summary>
		/// <param name="source">The original debt class.</param>
		public DebtHolder(DebtHolder source)
			: base(source)
		{

			this.rowVersion = source.RowVersion;

		}

		
        /// <summary>
		/// Primary Identifier of this object.
        /// </summary>
        public Guid DebtHolderId {get {return this.EntityId;}}

		/// <summary>
		/// The record type used for importing.
		/// </summary>
		protected override Type ImportRecordType
		{

			get { return typeof(TradingSupportReference.DebtHolderRecord); }

		}

		/// <summary>
		/// The record type used for importing.
		/// </summary>
		protected override Stream ImportSchema
		{

			get { return Assembly.GetExecutingAssembly().GetManifestResourceStream(@"FluidTrade.Guardian.Resources.DebtHolderImportSchema.xsd"); }

		}

		/// <summary>
		/// The row version of the debt holder row this object is based on.
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
			TradingSupportReference.DebtHolder record = new TradingSupportReference.DebtHolder();

			this.PopulateRecord(record);

			if (this.EntityId == Guid.Empty)
			{

				MethodResponseArrayOfguid response = tradingSupportClient.CreateDebtHolder(new TradingSupportReference.DebtHolder[] { record });

				if (!response.IsSuccessful)
					Entity.ThrowErrorInfo(response.Errors[0]);

			}
			else
			{

				MethodResponseErrorCode response = tradingSupportClient.UpdateDebtHolder(new TradingSupportReference.DebtHolder[] { record });

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
			this.rowVersion = (entity as DebtHolder).RowVersion;

		}

        /// <summary>
        /// Create a new DebtHolder in the data model.
        /// </summary>
        /// <param name="dataModel">The data model client to create the debt holder with.</param>
        /// <param name="typeId">The type-id of the DebtHolder type.</param>
		/// <param name="parentId">The entityId of the parent entity.</param>
		/// <param name="tenantId"></param>
		/// <returns>The entity-id of the new debt holder.</returns>
        public static new Guid Create(DataModelClient dataModel, Guid typeId, Guid parentId, Guid tenantId)
        {

			Guid entityId = Guid.Empty;
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				TradingSupportReference.DebtHolder record = new TradingSupportReference.DebtHolder();

				lock (DataModel.SyncRoot)
				{

					DebtClassRow parent = DataModel.DebtClass.DebtClassKey.Find(parentId);

					record.ConfigurationId = "Default";
					record.Name = "New Debt Holder";
					record.Address1 = parent.IsAddress1Null()? null : parent.Address1;
					record.Address2 = parent.IsAddress2Null()? null : parent.Address2;
					record.BankAccountNumber = parent.IsBankAccountNumberNull()? null : parent.BankAccountNumber;
					record.BankRoutingNumber = parent.IsBankRoutingNumberNull()? null : parent.BankRoutingNumber;
					record.City = parent.IsCityNull()? null : parent.City;
					record.CompanyName = parent.IsCompanyNameNull()? null : parent.CompanyName;
					record.ContactName = parent.IsContactNameNull()? null : parent.ContactName;
					record.Department = parent.IsDepartmentNull()? null : parent.Department;
					record.Email = parent.IsEmailNull()? null : parent.Email;
					record.Fax = parent.IsFaxNull()? null : parent.Fax;
					record.ForBenefitOf = parent.IsForBenefitOfNull()? null : parent.ForBenefitOf;
					record.ParentId = parent.DebtClassId;
					record.Phone = parent.IsPhoneNull()? null : parent.Phone;
					record.PostalCode = parent.IsPostalCodeNull()? null : parent.PostalCode;
					record.Province = parent.IsProvinceIdNull() ? null : (Guid?)parent.ProvinceId;
				}
				record.TenantId = tenantId;				
				MethodResponseArrayOfguid response = tradingSupportClient.CreateDebtHolder(new TradingSupportReference.DebtHolder[] { record });
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
		/// Create the properties window appropriate for this type.
		/// </summary>
		/// <returns></returns>
		protected override WindowEntityProperties CreatePropertiesWindow()
		{

			return new WindowDebtHolderProperties();

		}

		/// <summary>
		/// Delete an set of debt holders.
		/// </summary>
		/// <param name="debtHolders">The set of working orders.</param>
		/// <returns>The actual bulk size used.</returns>
		protected override Int32 Delete(List<GuardianObject> debtHolders)
		{

			Int32 attemptedBulkSize = debtHolders.Count;
			Int32 actualBulkSize = attemptedBulkSize;
			GuardianObject failedObject = null;
			TradingSupportReference.DebtHolder[] records = new TradingSupportReference.DebtHolder[debtHolders.Count];
			Dictionary<TradingSupportReference.DebtHolder, DebtHolder> recordsToHolders =
				new Dictionary<TradingSupportReference.DebtHolder, DebtHolder>();

			// Convert the GuardianObjects to records we can push up to the server.
			for (Int32 index = 0; index < records.Length; ++index)
			{

				DebtHolder debtHolder = debtHolders[0] as DebtHolder;

				records[index] = new TradingSupportReference.DebtHolder();
				debtHolder.PopulateRecord(records[index]);
				recordsToHolders[records[index]] = debtHolder;
				debtHolders.RemoveAt(0);

			}

			try
			{

				Int32 sentSize;
				MethodResponseErrorCode response;

				response = NetworkHelper.Attempt<MethodResponseErrorCode>(
					(client, a) =>
						client.DeleteDebtHolder(a as TradingSupportReference.DebtHolder[]),
					records,
					true,
					out sentSize);

				if (sentSize < attemptedBulkSize)
					actualBulkSize = sentSize;

				if (!response.IsSuccessful)
				{

					List<TradingSupportReference.DebtHolder> retryRecords = new List<TradingSupportReference.DebtHolder>();

					foreach (ErrorInfo errorInfo in response.Errors)
					{

						// The bulk index is an index into the set we sent, which may be smaller than the set passed in.
						failedObject = recordsToHolders[records[errorInfo.BulkIndex]];

						// If the error's "just" a deadlock, we should retry it.
						if (errorInfo.ErrorCode == ErrorCode.Deadlock)
						{

							retryRecords.Add(records[errorInfo.BulkIndex]);

						}
						else if (errorInfo.ErrorCode == ErrorCode.RecordExists)
						{

							throw new HasSettlementsException(this.ToString() + " has settled accounts");

						}
						// We can safely ignore not-found errors (we are deleting after all), but if the error's more severe, forget the how
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

			return actualBulkSize;

		}

		/// <summary>
		/// Delete the debt holder proper.
		/// </summary>
		protected override void DeleteDebtClass()
		{

			TradingSupportClient client = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			base.DeleteDebtClass();

			MethodResponseErrorCode response = client.DeleteDebtHolder(new TradingSupportReference.DebtHolder[] { new TradingSupportReference.DebtHolder
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
		/// Find the effective (inherited) value of a column in a debt class.
		/// </summary>
		/// <typeparam name="T">The type of the column.</typeparam>
		/// <param name="debtClassId">The DebtClassId of the debt class in question.</param>
		/// <param name="field">The column to retrieve.</param>
		/// <returns>Effective value of the indicated column, eg. the value in the debt class with the indicated ID, or, if that debt class has no
		/// value for the column, the value of the column in nearest ancestor debt class that has a value for that column. If no value can be found,
		/// returns null.</returns>
		private static object FindEffectiveField<T>(Guid debtClassId, DataColumn field)
		{

			object value = null;

			lock (DataModel.SyncRoot)
			{

				DebtHolderRow parent = DebtHolder.FindParentWithField(debtClassId, field);

				if (parent != null)
					value = (object)parent.Field<T>(field);

			}

			return value;

		}

		/// <summary>
		/// Find the nearest ancestor DebtClass that has a value for a particular field.
		/// </summary>
		/// <param name="debtClassId">The DebtClassId of the debt class to start with.</param>
		/// <param name="field">The column to look at.</param>
		/// <returns>The row of the first debt class found to have a value for the indicated field (including the initial debt class).</returns>
		private static DebtHolderRow FindParentWithField(Guid debtClassId, DataColumn field)
		{

			DebtHolderRow parent = null;
			DebtHolderRow debtHolder = DataModel.DebtHolder.DebtHolderKey.Find(debtClassId);
			Guid typeId = DataModel.Entity.EntityKey.Find(debtClassId).TypeId;

			while (debtHolder != null && parent == null)
			{

				EntityRow child = DataModel.Entity.EntityKey.Find(debtHolder.DebtHolderId);
				EntityTreeRow[] parents = child.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId();

				if (!debtHolder.IsNull(field))
					parent = debtHolder;

				if (parents.Length != 0)
					debtHolder = DataModel.DebtHolder.DebtHolderKey.Find(parents[0].ParentId);
				else
					debtHolder = null;

			}

			return parent;

		}

		/// <summary>
		/// Load debt holder specific information.
		/// </summary>
		protected override void FinishLoad()
		{
	
			DebtHolderRow debtHolderRow = DataModel.DebtHolder.DebtHolderKey.Find(this.EntityId);

			base.FinishLoad();

			this.rowVersion = debtHolderRow.RowVersion;

		}

		/// <summary>
		/// Get the import translation information for this debt class.
		/// </summary>
		/// <returns></returns>
		public override Dictionary<String, String> GetEffectiveImportTranslation()
		{

			Dictionary<String, String> translation = new Dictionary<String, String>();
			object effectiveTranslationId = DebtHolder.FindEffectiveField<Guid>(this.DebtHolderId, DataModel.DebtHolder.DebtHolderImportTranslationIdColumn);

			if (effectiveTranslationId != null)
			{

				DebtHolderImportTranslationRow translationRow = DataModel.DebtHolderImportTranslation.DebtHolderImportTranslationKey.Find((Guid)effectiveTranslationId);
				
				translation[translationRow.AccountBalance] = "AccountBalance";
				translation[translationRow.AccountCode] = "AccountCode";
				translation[translationRow.OriginalAccountNumber] = "OriginalAccountNumber";
				translation[translationRow.SocialSecurityNumber] = "SocialSecurityNumber";
				if (!translationRow.IsAddress1Null())
					translation[translationRow.Address1] = "Address1";
				if (!translationRow.IsAddress2Null())
					translation[translationRow.Address2] = "Address2";
				if (!translationRow.IsCityNull())
					translation[translationRow.City] = "City";
				if (!translationRow.IsDateOfBirthNull())
					translation[translationRow.DateOfBirth] = "DateOfBirth";
				if (!translationRow.IsDateOfDelinquencyNull())
					translation[translationRow.DateOfDelinquency] = "DateOfDelinquency";
				if (!translationRow.IsDebtHolderNull())
					translation[translationRow.DebtHolder] = "DebtHolder";
				if (!translationRow.IsFirstNameNull())
					translation[translationRow.FirstName] = "FirstName";
				if (!translationRow.IsLastNameNull())
					translation[translationRow.LastName] = "LastName";
				if (!translationRow.IsMiddleNameNull())
					translation[translationRow.MiddleName] = "MiddleName";
				if (!translationRow.IsPhoneNumberNull())
					translation[translationRow.PhoneNumber] = "PhoneNumber";
				if (!translationRow.IsPostalCodeNull())
					translation[translationRow.PostalCode] = "PostalCode";
				if (!translationRow.IsProvinceCodeNull())
					translation[translationRow.ProvinceCode] = "ProvinceCode";
				if (!translationRow.IsSuffixNull())
					translation[translationRow.Suffix] = "Suffix";
				if (!translationRow.IsVendorCodeNull())
					translation[translationRow.VendorCode] = "VendorCode";

			}
			// The default translation is the "identity" translation.
			else
			{

				translation["AccountBalance"] = "AccountBalance";
				translation["AccountCode"] = "AccountCode";
				translation["Address1"] = "Address1";
				translation["Address2"] = "Address2";
				translation["City"] = "City";
				translation["DateOfBirth"] = "DateOfBirth";
				translation["DateOfDelinquency"] = "DateOfDelinquency";
				translation["DebtHolder"] = "DebtHolder";
				translation["FirstName"] = "FirstName";
				translation["LastName"] = "LastName";
				translation["MiddleName"] = "MiddleName";
				translation["OriginalAccountNumber"] = "OriginalAccountNumber";
				translation["PhoneNumber"] = "PhoneNumber";
				translation["PostalCode"] = "PostalCode";
				translation["ProvinceCode"] = "ProvinceCode";
				translation["Suffix"] = "Suffix";
				translation["SocialSecurityNumber"] = "SocialSecurityNumber";
				translation["VendorCode"] = "VendorCode";

			}

			return translation;

		}

		/// <summary>
		/// Determine whether this debt holder contains settled accounts.
		/// </summary>
		/// <returns>True if the debt holder contains settled accounts.</returns>
		protected override Boolean HasSettledAccounts()
		{

			Boolean hasSettledAccounts = false;

			foreach (MatchRow matchRow in DataModel.Blotter.BlotterKey.Find(this.EntityId).GetMatchRows())
				foreach (ConsumerDebtNegotiationRow consumerDebtNegotiationRow in matchRow.GetConsumerDebtNegotiationRows())
					if (consumerDebtNegotiationRow.GetConsumerDebtSettlementRows().Length > 0)
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
					client.ImportDebtHolderRecords(records as TradingSupportReference.DebtHolderRecord[]),
				records,
				false,
				out sentSize);

			foreach (TradingSupportReference.ErrorInfo error in response.Errors)
				errors[records.GetValue(error.BulkIndex)] = error.Message;

			return errors;

		}

		/// <summary>
		/// If there's no debt holder row yet, we need to finish loading up the object later.
		/// </summary>
		/// <returns>True if we can't find a debt holder row for the entityId.</returns>
		protected override bool NeedLateLoad()
		{

			return DataModel.DebtHolder.DebtHolderKey.Find(this.EntityId) == null;

		}

    }

}
