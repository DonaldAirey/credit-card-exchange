namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;
	using Microsoft.Win32;
	using System.ServiceModel;

    /// <summary>
    /// GUI Element representing a Debt Class
    /// </summary>
	public class DebtClass : Blotter
    {

		private string address1;
		private string address2;
		private string bankAccountNumber;
		private string bankRoutingNumber;
		private string city;
		private string companyName;
		private string contactName;
		private string department;
		private Guid? debtRuleId;
		private string email;
		private string fax;
		private string forBenefitOf;
		private string phone;
		private Guid? province;
		private string postalCode;
		private long rowVersion;
		private String settlementTemplate;
        
		/// <summary>
		/// Create a new debt class based on an entity row.
		/// </summary>
		/// <param name="entityRow">The entity row in the DataModel.</param>
        public DebtClass(EntityRow entityRow) : base(entityRow)
        {


		}

		/// <summary>
		/// Create a duplicate debt class.
		/// </summary>
		/// <param name="source">The original debt class.</param>
		public DebtClass(DebtClass source) : base(source)
		{

			this.Update(source);

		}

		/// <summary>
		/// Gets or sets the first line of the mailing address for the debt class.
		/// </summary>
		public string Address1
		{

			get { return this.address1; }
			set
			{

				if (this.address1 != value)
				{

					this.address1 = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Address1"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the second line of the mailing address for the debt class.
		/// </summary>
		public string Address2
		{

			get { return this.address2; }
			set
			{

				if (this.address2 != value)
				{

					this.address2 = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Address2"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the bank routing number used by this debt class.
		/// </summary>
		public string BankRoutingNumber
		{

			get { return this.bankRoutingNumber; }
			set
			{

				if (this.bankRoutingNumber != value)
				{

					this.bankRoutingNumber = value;
					OnPropertyChanged(new PropertyChangedEventArgs("BankRoutingNumber"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the bank account number used by this debt class.
		/// </summary>
		public string BankAccountNumber
		{

			get { return this.bankAccountNumber; }
			set
			{

				if (this.bankAccountNumber != value)
				{

					this.bankAccountNumber = value;
					OnPropertyChanged(new PropertyChangedEventArgs("BankAccountNumber"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the city this debt class is in.
		/// </summary>
		public string City
		{

			get { return this.city; }
			set
			{

				if (this.city != value)
				{

					this.city = value;
					OnPropertyChanged(new PropertyChangedEventArgs("City"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the name of the company this debt class belongs to.
		/// </summary>
		public string CompanyName
		{

			get { return this.companyName; }
			set
			{

				if (this.companyName != value)
				{

					this.companyName = value;
					OnPropertyChanged(new PropertyChangedEventArgs("CompanyName"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the contact name for this debt class.
		/// </summary>
		public string ContactName
		{

			get { return this.contactName; }
			set
			{

				if (this.contactName != value)
				{

					this.contactName = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ContactName"));

				}

			}

		}

        /// <summary>
		/// Primary Identifier of this object.
        /// </summary>
        public Guid DebtClassId
		{
	
			get {return this.EntityId;}
	
		}

		/// <summary>
		/// Gets or sets the RuleId of the rule used to settle in this debt class.
		/// </summary>
		public Guid? DebtRuleId
		{

			get { return this.debtRuleId; }
			set
			{

				if (this.debtRuleId != value)
				{

					this.debtRuleId = value;
					OnPropertyChanged(new PropertyChangedEventArgs("DebtRuleId"));

				}
			
			}

		}

		/// <summary>
		/// Gets or sets the department this debt class belongs to.
		/// </summary>
		public string Department
		{

			get { return this.department; }
			set
			{

				if (this.department != value)
				{

					this.department = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Department"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the contact email address for this debt class.
		/// </summary>
		public string Email
		{

			get { return this.email; }
			set
			{

				if (this.email != value)
				{

					this.email = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Email"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the contact fax number for this debt class.
		/// </summary>
		public string Fax
		{

			get { return this.fax; }
			set
			{

				if (this.fax != value)
				{

					this.fax = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Fax"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the "for benefit of" field of the debt class.
		/// </summary>
		public string ForBenefitOf
		{

			get { return this.forBenefitOf; }
			set
			{

				if (this.forBenefitOf != value)
				{

					this.forBenefitOf = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ForBenefitOf"));

				}

			}

		}

		/// <summary>
		/// The record type used for importing.
		/// </summary>
		protected virtual Type ImportRecordType
		{

			get { return null; }

		}

		/// <summary>
		/// The schema used to validate import files.
		/// </summary>
		protected virtual Stream ImportSchema
		{

			get { return null; }

		}

		/// <summary>
		/// Gets or sets the contact phone number for this debt class.
		/// </summary>
		public string Phone
		{

			get { return this.phone; }
			set
			{

				if (this.phone != value)
				{

					this.phone = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Phone"));

				}

			}

		}

		/// <summary>
		/// Get the row version this debt class corresponds to.
		/// </summary>
		public new long RowVersion
		{

			get { return this.rowVersion; }
			protected set { this.rowVersion = value; }

		}

		/// <summary>
		/// Gets or sets the state this debt class is in.
		/// </summary>
		public Guid? Province
		{

			get { return this.province; }
			set
			{

				if (this.province != value)
				{

					this.province = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Province"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the zip code this debt class is in.
		/// </summary>
		public string PostalCode
		{

			get { return this.postalCode; }
			set
			{

				if (this.postalCode != value)
				{

					this.postalCode = value;
					OnPropertyChanged(new PropertyChangedEventArgs("PostalCode"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the settlement letter template for this debt class.
		/// </summary>
		public String SettlementTemplate
		{

			get { return this.settlementTemplate; }
			set
			{

				if (this.settlementTemplate != value)
				{

					this.settlementTemplate = value;
					OnPropertyChanged(new PropertyChangedEventArgs("SettlementTemplate"));

				}

			}

		}

		/// <summary>
		/// Copies the data from one entity to another.
		/// </summary>
		/// <param name="entity">The source entity.</param>
		public override void Copy(GuardianObject entity)
		{

			DebtClass source = entity as DebtClass;

			base.Copy(entity);

			this.DebtRuleId = source.DebtRuleId;

			this.Address1 = source.Address1;
			this.Address2 = source.Address2;
			this.BankRoutingNumber = source.BankRoutingNumber;
			this.BankAccountNumber = source.BankAccountNumber;
			this.City = source.City;
			this.CompanyName = source.CompanyName;
			this.ContactName = source.ContactName;
			this.Department = source.Department;
			this.Email = source.Email;
			this.Fax = source.Fax;
			this.ForBenefitOf = source.ForBenefitOf;
			this.Phone = source.Phone;
			this.Province = source.Province;
			this.PostalCode = source.PostalCode;
			this.SettlementTemplate = source.SettlementTemplate;
			this.Modified = source.Modified;
			this.rowVersion = source.rowVersion;

		}

		/// <summary>
		/// Commit any changes to the debt class to the server.
		/// </summary>
		public override void Commit()
		{

			Boolean retry = false;
			Int32 tries = 0;

			do
			{

				try
				{

					if (this.Deleted)
					{

						this.DeleteDebtClass();

					}
					else
					{

						this.CommitDebtClass();

					}

					tries += 1;

					this.Modified = false;

				}
				catch (FaultException<DeadlockFault>)
				{

					retry = true;

				}
				catch (Exception exception)
				{

					// Any issues trying to communicate to the server are logged.
					EventLog.Error("DebtClass Commit: {0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

					if (this.Deleted)
						throw new DeleteException(this, exception);

					throw;

				}

			} while (retry && tries < 3);

		}

		/// <summary>
		/// Commit changes to a debt class (but not delete it!).
		/// </summary>
		protected virtual void CommitDebtClass()
		{

		}

		/// <summary>
		/// Create the properties window appropriate for this type.
		/// </summary>
		/// <returns></returns>
		protected override WindowEntityProperties CreatePropertiesWindow()
		{

			return new WindowDebtClassProperties();

		}

		/// <summary>
		/// The function to be called by DebtClass.Delete to finally delete the debt class proper.
		/// </summary>
		protected virtual void DeleteDebtClass()
		{

			lock (DataModel.SyncRoot)
				if (this.HasSettledAccounts())
					throw new HasSettlementsException(this.Name + " contains settled accounts and cannot be deleted");

		}

		/// <summary>
		/// Find the nearest ancestor DebtClass that has a value for a particular field.
		/// </summary>
		/// <param name="debtClassId">The DebtClassId of the debt class to start with.</param>
		/// <param name="field">The column to look at.</param>
		/// <returns>The row of the first debt class found to have a value for the indicated field (including the initial debt class).</returns>
		private static DebtClassRow FindParentWithField(Guid debtClassId, DataColumn field)
		{

			DebtClassRow parent = null;
			DebtClassRow debtClass = DataModel.DebtClass.DebtClassKey.Find(debtClassId);
			Guid typeId = DataModel.Entity.EntityKey.Find(debtClassId).TypeId;

			while (debtClass != null && parent == null)
			{

				EntityRow child = DataModel.Entity.EntityKey.Find(debtClass.DebtClassId);
				EntityTreeRow[] parents = child.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId();

				if (!debtClass.IsNull(field))
					parent = debtClass;

				if (parents.Length != 0)
					debtClass = DataModel.DebtClass.DebtClassKey.Find(parents[0].ParentId);
				else
					debtClass = null;

			}

			return parent;

		}

		/// <summary>
		/// Load debt class specific information.
		/// </summary>
		protected override void FinishLoad()
		{

			base.FinishLoad();

			this.Update(DataModel.DebtClass.DebtClassKey.Find(this.EntityId));

		}

		/// <summary>
		/// Retrieve the custom menu items for this debt class.
		/// </summary>
		/// <returns>The list of custom menu items.</returns>
		public override List<Control> GetCustomMenuItems()
		{

			List<Control> menuItems = base.GetCustomMenuItems();
			MenuItem importMenuItem = new MenuItem() { Header = "Import..." };

			importMenuItem.Click += (s, e) => this.ImportAccounts();
			menuItems.Add(importMenuItem);

			return menuItems;

		}

        /// <summary>
        /// Get the debt rule used for this debt class. This function locks the DataModel.
        /// </summary>
        /// <returns>The debt rule. If none can be found either in the debt class or its ancestors, null is returned.</returns>
        public DebtRule GetDebtRule()
        {

            return DebtClass.GetDebtRule(this.EntityId, this.TypeId);

        }

        /// <summary>
        /// Find the debt rule assocatiated with a debt class of a particular type. This function locks the DataModel.
        /// </summary>
        /// <param name="debtClassId">The DebtClassId of the debt class.</param>
        /// <param name="typeId">The TypeId of the debt class.</param>
        /// <returns>The debt rule. If none can be found either in the debt class or its ancestors, null is returned.</returns>
        public static DebtRule GetDebtRule(Guid debtClassId, Guid typeId)
        {

            DebtRule rule = null;

            lock(DataModel.SyncRoot)
            {

				DebtClassRow parent = DebtClass.FindParentWithField(debtClassId, DataModel.DebtClass.DebtRuleIdColumn);

				if (parent != null)
					rule = new DebtRule(parent.DebtRuleRow);

            }

            return rule;

        }

		/// <summary>
		/// Get the effective (inherited) value of Address1.
		/// </summary>
		/// <returns>If Address1 is non-null, returns Address1, otherwise returns the Address1 of the closest ancestor whose Address1 is
		/// non-null.</returns>
		public string GetEffectiveAddress1()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.Address1Column) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of Address2.
		/// </summary>
		/// <returns>If Address2 is non-null, returns Address2, otherwise returns the Address2 of the closest ancestor whose Address2 is
		/// non-null.</returns>
		public string GetEffectiveAddress2()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.Address2Column) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of BankAccountNumber.
		/// </summary>
		/// <returns>If BankAccountNumber is non-null, returns BankAccountNumber, otherwise returns the BankAccountNumber of the closest ancestor
		/// whose BankAccountNumber is non-null.</returns>
		public string GetEffectiveBankAccountNumber()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.BankAccountNumberColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of BankRoutingNumber.
		/// </summary>
		/// <returns>If BankRoutingNumber is non-null, returns BankRoutingNumber, otherwise returns the BankRoutingNumber of the closest ancestor
		/// whose BankRoutingNumber is non-null.</returns>
		public string GetEffectiveBankRoutingNumber()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.BankRoutingNumberColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of City.
		/// </summary>
		/// <returns>If City is non-null, returns City, otherwise returns the City of the closest ancestor whose City is non-null.</returns>
		public string GetEffectiveCity()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.CityColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of CompanyName.
		/// </summary>
		/// <returns>If CompanyName is non-null, returns CompanyName, otherwise returns the CompanyName of the closest ancestor whose CompanyName is
		/// non-null.</returns>
		public string GetEffectiveCompanyName()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.CompanyNameColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of ContactName.
		/// </summary>
		/// <returns>If ContactName is non-null, returns ContactName, otherwise returns the ContactName of the closest ancestor whose ContactName is
		/// non-null.</returns>
		public string GetEffectiveContactName()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.ContactNameColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of Department.
		/// </summary>
		/// <returns>If Department is non-null, returns Department, otherwise returns the Department of the closest ancestor whose Department is
		/// non-null.</returns>
		public string GetEffectiveDepartment()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.DepartmentColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of Email.
		/// </summary>
		/// <returns>If Email is non-null, returns Email, otherwise returns the Email of the closest ancestor whose Email is non-null.</returns>
		public string GetEffectiveEmail()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.EmailColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of Fax.
		/// </summary>
		/// <returns>If Fax is non-null, returns Fax, otherwise returns the Fax of the closest ancestor whose Fax is non-null.</returns>
		public string GetEffectiveFax()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.FaxColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of ForBenefitOf.
		/// </summary>
		/// <returns>If ForBenefitOf is non-null, returns ForBenefitOf, otherwise returns the ForBenefitOf of the closest ancestor whose ForBenefitOf
		/// is non-null.</returns>
		public string GetEffectiveForBenefitOf()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.ForBenefitOfColumn) as string;

		}

		/// <summary>
		/// Get the import translation information for this debt class.
		/// </summary>
		/// <returns>The translation table.</returns>
		public virtual Dictionary<String, String> GetEffectiveImportTranslation()
		{

			return null;

		}

		/// <summary>
		/// Get the effective (inherited) value of Phone.
		/// </summary>
		/// <returns>If Phone is non-null, returns Phone, otherwise returns the Phone of the closest ancestor whose Phone is non-null.</returns>
		public string GetEffectivePhone()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.PhoneColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of State.
		/// </summary>
		/// <returns>If State is non-null, returns State, otherwise returns the State of the closest ancestor whose State is non-null.</returns>
		public string GetEffectiveState()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.ProvinceIdColumn) as string;

		}

		/// <summary>
		/// Get the effective (inherited) value of PostalCode.
		/// </summary>
		/// <returns>If PostalCode is non-null, returns PostalCode, otherwise returns the PostalCode of the closest ancestor whose PostalCode is non-null.</returns>
		public string GetEffectivePostalCode()
		{

			return DebtClass.FindEffectiveField<string>(this.DebtClassId, DataModel.DebtClass.PostalCodeColumn) as string;

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

				DebtClassRow parent = DebtClass.FindParentWithField(debtClassId, field);

				if (parent != null)
					value = (object)parent.Field<T>(field);

			}

			return value;

		}

		/// <summary>
		/// Determine whether this debt class has settled accounts.
		/// </summary>
		/// <returns>True if there are settled accounts in this debt class.</returns>
		protected virtual Boolean HasSettledAccounts()
		{

			return true;

		}

		/// <summary>
		/// Prompt the user to import accounts into this debt class.
		/// </summary>
		public void ImportAccounts()
		{

			bool? result = null;
			OpenFileDialog openFile = new OpenFileDialog();

			// Configure the 'Open File' dialog box to look for the available XML files.
			openFile.InitialDirectory = Environment.CurrentDirectory;
			openFile.DefaultExt = ".xml";
			openFile.Filter = "All Supported File Types (.xml, .xlsx)|*.xml;*.xlsx|XML Documents (.xml)|*.xml|Excel Documents (.xlsx)|*.xlsx";

			// Show open file dialog box
			result = openFile.ShowDialog();

			if (result == true)
			{

				WindowImport import = new WindowImport();

				//import.Owner = Application.Current.MainWindow;
				import.Parameters["Blotter"] = this.EntityId;
				import.Parameters["ConfigurationId"] = "Default";
				import.Parameters["CountryCode"] = "US";
				import.Parameters["Currency"] = "USD";
				import.Parameters["VendorCode"] = "novendor";
				import.Parameters["Tag"] = Path.GetFileNameWithoutExtension(openFile.FileName);
				import.RecordType = this.ImportRecordType;
				import.Importer = this.ImportRecord;
				import.Schema = this.ImportSchema;
				import.SchemaVersion = 2;
				import.ImportFile = openFile.FileName;
				import.BulkCount = 500;
				import.Translation = this.GetEffectiveImportTranslation();

				import.Show();

			}

		}

		/// <summary>
		/// Handle the actual import call and error recording.
		/// </summary>
		/// <param name="records">The records to import.</param>
		/// <param name="sentSize">The actual bulk size used.</param>
		/// <returns>The errors encountered.</returns>
		protected virtual Dictionary<object, string> ImportRecord(Array records, out Int32 sentSize)
		{

			sentSize = records.Length;
			return new Dictionary<object, string>();

		}

		/// <summary>
		/// Populate a trading support record with information from this debt class.
		/// </summary>
		/// <param name="entityRecord">The record to populate.</param>
		protected override void PopulateRecord(TradingSupportReference.BaseRecord entityRecord)
		{

			TradingSupportReference.DebtClass record = entityRecord as TradingSupportReference.DebtClass;

			base.PopulateRecord(entityRecord);

			record.Address1 = this.Address1;
			record.Address2 = this.Address2;
			record.BankAccountNumber = this.BankAccountNumber;
			record.BankRoutingNumber = this.BankRoutingNumber;
			record.City = this.City;
			record.CompanyName = this.CompanyName;
			record.ContactName = this.ContactName;
			record.DebtRuleId = this.DebtRuleId;
			record.Department = this.Department;
			record.Description = this.Description;
			record.Email = this.Email;
			record.Fax = this.Fax;
			record.ForBenefitOf = this.ForBenefitOf;
			record.Phone = this.Phone;
			record.Province = this.Province;
			record.PostalCode = this.PostalCode;
			record.SettlementTemplate = this.SettlementTemplate;

		}

		/// <summary>
		/// Update the debt class with new information from the data model.
		/// </summary>
		/// <param name="entityRow"></param>
		public override void Update(EntityRow entityRow)
		{

			base.Update(entityRow);

			this.Update(DataModel.DebtClass.DebtClassKey.Find(entityRow.EntityId));

		}

		/// <summary>
		/// Update the debt class information from a debt class row.
		/// </summary>
		/// <param name="debtClassRow">The row to update from.</param>
		protected void Update(DebtClassRow debtClassRow)
		{

			if (!this.Modified && this.DebtClassId == debtClassRow.DebtClassId)
			{

				this.DebtRuleId = debtClassRow.IsDebtRuleIdNull() ? null : (Guid?)debtClassRow.DebtRuleId;

				this.Address1 = debtClassRow.IsAddress1Null() ? null : debtClassRow.Address1;
				this.Address2 = debtClassRow.IsAddress2Null() ? null : debtClassRow.Address2;
				this.BankAccountNumber = debtClassRow.IsBankAccountNumberNull() ? null : debtClassRow.BankAccountNumber;
				this.BankRoutingNumber = debtClassRow.IsBankRoutingNumberNull() ? null : debtClassRow.BankRoutingNumber;
				this.City = debtClassRow.IsCityNull() ? null : debtClassRow.City;
				this.CompanyName = debtClassRow.IsCompanyNameNull() ? null : debtClassRow.CompanyName;
				this.ContactName = debtClassRow.IsContactNameNull() ? null : debtClassRow.ContactName;
				this.Department = debtClassRow.IsDepartmentNull() ? null : debtClassRow.Department;
				this.Email = debtClassRow.IsEmailNull() ? null : debtClassRow.Email;
				this.Fax = debtClassRow.IsFaxNull() ? null : debtClassRow.Fax;
				this.ForBenefitOf = debtClassRow.IsForBenefitOfNull() ? null : debtClassRow.ForBenefitOf;
				this.Phone = debtClassRow.IsPhoneNull() ? null : debtClassRow.Phone;
				this.Province = debtClassRow.IsProvinceIdNull() ? null : (Guid?)debtClassRow.ProvinceId;
				this.PostalCode = debtClassRow.IsPostalCodeNull() ? null : debtClassRow.PostalCode;
				this.SettlementTemplate = debtClassRow.IsSettlementTemplateNull() ? null : debtClassRow.SettlementTemplate;
				this.Modified = false;

			}

			this.rowVersion = debtClassRow.RowVersion;

		}

		/// <summary>
		/// Update the debt class with new information from another debt class.
		/// </summary>
		/// <param name="entity"></param>
		public override void Update(GuardianObject entity)
		{

			DebtClass source = entity as DebtClass;

			if (!this.Modified && this.DebtClassId == source.DebtClassId)
			{

				this.DebtRuleId = source.DebtRuleId;

				this.Address1 = source.Address1;
				this.Address2 = source.Address2;
				this.BankRoutingNumber = source.BankRoutingNumber;
				this.BankAccountNumber = source.BankAccountNumber;
				this.City = source.City;
				this.CompanyName = source.CompanyName;
				this.ContactName = source.ContactName;
				this.Department = source.Department;
				this.Email = source.Email;
				this.Fax = source.Fax;
				this.ForBenefitOf = source.ForBenefitOf;
				this.Phone = source.Phone;
				this.Province = source.Province;
				this.PostalCode = source.PostalCode;
				this.SettlementTemplate = source.SettlementTemplate;
				this.Modified = false;

			}

			this.rowVersion = source.RowVersion;

		}

    }

}
