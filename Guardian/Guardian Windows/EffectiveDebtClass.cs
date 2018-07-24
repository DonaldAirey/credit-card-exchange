namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Windows;
	using System.Data;

	/// <summary>
	/// A debt class whose fields are populated with the "effective" values of the underlying debt class. Meaning, if the debt class has the value
	/// set, that set value is used; if not then the debt class's ancestors in the EntityTree are searched for an appropriate value.
	/// </summary>
	public class EffectiveDebtClass : DebtClass, IDisposable
	{

		List<Guid> ancestors = new List<Guid>();
		private Boolean live = true;
		private Boolean isAddress1Inherited = false;
		private Boolean isAddress2Inherited = false;
		private Boolean isBankAccountNumberInherited = false;
		private Boolean isBankRoutingNumberInherited = false;
		private Boolean isCityInherited = false;
		private Boolean isCompanyNameInherited = false;
		private Boolean isContactNameInherited = false;
		private Boolean isDepartmentInherited = false;
		private Boolean isEmailInherited = false;
		private Boolean isFaxInherited = false;
		private Boolean isForBenefitOfInherited = false;
		private Boolean isPhoneInherited = false;
		private Boolean isStateInherited = false;
		private Boolean isPostalCodeInherited = false;

		/// <summary>
		/// Create a new EffectiveDebtClass from an EntityRow.
		/// </summary>
		/// <param name="entityRow">The entity row to base the debt class on.</param>
		public EffectiveDebtClass(EntityRow entityRow)
			: base(entityRow)
		{

			// This would have retrieved values from the hierarchy.
			//this.Construct();

			// We watch DebtClass in case data in one of our ancestors changes.
			DataModel.DebtClass.DebtClassRowChanged += this.OnDebtClassChanged;
			// And we watch EntityTree in case we or one of our ancestors moves.
			//DataModel.EntityTree.EntityTreeRowChanged += this.OnEntityTreeChanged;

		}

		/// <summary>
		/// Create a new EffectiveDebtClass based on another DebtClass.
		/// </summary>
		/// <param name="source">The debt class to base this debt class on.</param>
		public EffectiveDebtClass(DebtClass source) : base(source)
		{

			// This would have retrieved values from the hierarchy.
			//this.Construct();

			DebtClassRow debtClass = DataModel.DebtClass.DebtClassKey.Find(this.DebtClassId);

			this.TryFillFields(debtClass);
			base.RowVersion = debtClass.RowVersion;

			// We watch DebtClass in case data in one of our ancestors changes.
			DataModel.DebtClass.DebtClassRowChanged += this.OnDebtClassChanged;
			// And we watch EntityTree in case we or one of our ancestors moves.
			//DataModel.EntityTree.EntityTreeRowChanged += this.OnEntityTreeChanged;

		}

		/// <summary>
		/// Gets or sets whether debt class is live. If true, updates to the database will update the debt class.
		/// </summary>
		public Boolean Live
		{

			get { return this.live; }
			set { this.live = value; }

		}

		/// <summary>
		/// If true, the value of Address1 is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsAddress1Inherited
		{

			get { return this.isAddress1Inherited; }

		}

		/// <summary>
		/// If true, the value of Address2 is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsAddress2Inherited
		{

			get { return this.isAddress2Inherited; }

		}

		/// <summary>
		/// If true, the value of BankAccountNumber is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsBankAccountNumberInherited
		{

			get { return this.isBankAccountNumberInherited; }

		}

		/// <summary>
		/// If true, the value of BankRoutingNumber is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsBankRoutingNumberInherited
		{

			get { return this.isBankRoutingNumberInherited; }

		}

		/// <summary>
		/// If true, the value of City is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsCityInherited
		{

			get { return this.isCityInherited; }

		}

		/// <summary>
		/// If true, the value of CompanyName is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsCompanyNameInherited
		{

			get { return this.isCompanyNameInherited; }

		}

		/// <summary>
		/// If true, the value of ContactName is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsContactNameInherited
		{

			get { return this.isContactNameInherited; }

		}

		/// <summary>
		/// If true, the value of Department is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsDepartmentInherited
		{

			get { return this.isDepartmentInherited; }

		}

		/// <summary>
		/// If true, the value of Email is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsEmailInherited
		{

			get { return this.isEmailInherited; }

		}

		/// <summary>
		/// If true, the value of Fax is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsFaxInherited
		{

			get { return this.isFaxInherited; }

		}

		/// <summary>
		/// If true, the value of ForBenefitOf is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsForBenefitOfInherited
		{

			get { return this.isForBenefitOfInherited; }

		}

		/// <summary>
		/// If true, the value of Phone is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsPhoneInherited
		{

			get { return this.isPhoneInherited; }

		}

		/// <summary>
		/// If true, the value of State is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsStateInherited
		{

			get { return this.isStateInherited; }

		}

		/// <summary>
		/// If true, the value of PostalCode is inherited from an ancestory debt class.
		/// </summary>
		public Boolean IsPostalCodeInherited
		{

			get { return this.isPostalCodeInherited; }

		}

		/// <summary>
		/// Clear the values of all the fields of the underlying debt class and mark all fields as not inherited.
		/// </summary>
		private void ClearFields()
		{

			this.Address1 = null;
			this.Address2 = null;
			this.BankAccountNumber = null;
			this.BankRoutingNumber = null;
			this.City = null;
			this.CompanyName = null;
			this.ContactName = null;
			this.Department = null;
			this.Email = null;
			this.Fax = null;
			this.ForBenefitOf = null;
			this.Phone = null;
			this.Province = null;
			this.PostalCode = null;

			this.isAddress1Inherited = false;
			this.isAddress2Inherited = false;
			this.isBankAccountNumberInherited = false;
			this.isBankRoutingNumberInherited = false;
			this.isCityInherited = false;
			this.isCompanyNameInherited = false;
			this.isContactNameInherited = false;
			this.isDepartmentInherited = false;
			this.isEmailInherited = false;
			this.isFaxInherited = false;
			this.isForBenefitOfInherited = false;
			this.isPhoneInherited = false;
			this.isStateInherited = false;
			this.isPostalCodeInherited = false;

		}

		/// <summary>
		/// Build the list of ancestors and fill in the fields of the debt class.
		/// </summary>
		private void Construct()
		{

			DebtClassRow debtClass = DataModel.DebtClass.DebtClassKey.Find(this.DebtClassId);
			Guid typeId = DataModel.Entity.EntityKey.Find(this.DebtClassId).TypeId;

			this.RowVersion = debtClass.RowVersion;

			this.ClearFields();
			this.ancestors.Clear();

			// Construct the ancestor list and fill in effective values for our various fields along the way. The ancestor list is ordered near-to-far
			// (starting with ourselves).
			while (debtClass != null)
			{

				EntityTreeRow parentRelationship;

				this.ancestors.Add(debtClass.DebtClassId);

				this.TryFillFields(debtClass);

				parentRelationship = DataModel.EntityTree.FirstOrDefault(row =>
					row.ChildId == debtClass.DebtClassId && row.EntityRowByFK_Entity_EntityTree_ParentId.TypeId == typeId);
				debtClass = null;

				if (parentRelationship != null)
					debtClass = DataModel.DebtClass.DebtClassKey.Find(parentRelationship.ParentId);

            }

		}

		/// <summary>
		/// Dispose of our resources - ie. unregister our event listeners.
		/// </summary>
		public void Dispose()
		{

			DataModel.DebtClass.DebtClassRowChanged -= this.OnDebtClassChanged;
			//DataModel.EntityTree.EntityTreeRowChanged -= this.OnEntityTreeChanged;

		}

		/// <summary>
		/// Handle changes to the DebtRule table. Track relevant changes to ourself and our ancestors.
		/// </summary>
		/// <param name="sender">The originator of the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnDebtClassChanged(object sender, DebtClassRowChangeEventArgs eventArgs)
		{

			// When one of our ancestors changes, we need to update our fields.
			if (eventArgs.Action == DataRowAction.Commit &&
				eventArgs.Row.RowState != DataRowState.Detached)
			{

#if false
				if (this.live &&
					this.ancestors.Contains(eventArgs.Row.DebtClassId))
				{

					this.ClearFields();
					foreach (Guid debtClassId in this.ancestors)
						this.TryFillFields(DataModel.DebtClass.DebtClassKey.Find(debtClassId));

				}
#endif

				if (this.DebtClassId == eventArgs.Row.DebtClassId)
				{

					//this.rowVersion = eventArgs.Row.RowVersion;
					this.Update(eventArgs.Row);

				}

			}

		}

		/// <summary>
		/// Handle changes to the EntityTree table. Track relevant changes to the entity hiearchy.
		/// </summary>
		/// <param name="sender">The originator of the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnEntityTreeChanged(object sender, EntityTreeRowChangeEventArgs eventArgs)
		{

			// If one of our ancestors moved in the entity tree, we'll need to rebuild our entity list (because we've effectively been moved) and
			// update our fields.
			if (this.live &&
				eventArgs.Action == DataRowAction.Commit &&
				eventArgs.Row.RowState != DataRowState.Detached &&
				this.ancestors.Contains(eventArgs.Row.ChildId))
			{

				this.Construct();

			}

		}

		/// <summary>
		/// Fill in any fields that haven't been filled in yet that the DebtClassRow has values for. If the DebtClassRow is not the row that corresponds
		/// the underlying DebtClass object, then mark the field as inherited.
		/// </summary>
		/// <param name="debtClass">The debt class to use to fill the fields.</param>
		private void TryFillFields(DebtClassRow debtClass)
		{

			if (String.IsNullOrEmpty(this.Address1) && !debtClass.IsAddress1Null() && debtClass.Address1 != "")
			{

				this.isAddress1Inherited = debtClass.DebtClassId != this.DebtClassId;
				this.Address1 = debtClass.Address1;

			}
			if (String.IsNullOrEmpty(this.Address2) && !debtClass.IsAddress2Null() && debtClass.Address2 != "")
			{

				this.isAddress2Inherited = debtClass.DebtClassId != this.DebtClassId;
				this.Address2 = debtClass.Address2;

			}
			if (String.IsNullOrEmpty(this.BankAccountNumber) && !debtClass.IsBankAccountNumberNull() && debtClass.BankAccountNumber != "")
			{

				this.isBankAccountNumberInherited = debtClass.DebtClassId != this.DebtClassId;
				this.BankAccountNumber = debtClass.BankAccountNumber;

			}
			if (String.IsNullOrEmpty(this.BankRoutingNumber) && !debtClass.IsBankRoutingNumberNull() && debtClass.BankRoutingNumber != "")
			{

				this.isBankRoutingNumberInherited = debtClass.DebtClassId != this.DebtClassId;
				this.BankRoutingNumber = debtClass.BankRoutingNumber;

			}
			if (String.IsNullOrEmpty(this.City) && !debtClass.IsCityNull() && debtClass.City != "")
			{

				this.isCityInherited = debtClass.DebtClassId != this.DebtClassId;
				this.City = debtClass.City;

			}
			if (String.IsNullOrEmpty(this.CompanyName) && !debtClass.IsCompanyNameNull() && debtClass.CompanyName != "")
			{

				this.isCompanyNameInherited = debtClass.DebtClassId != this.DebtClassId;
				this.CompanyName = debtClass.CompanyName;

			}
			if (String.IsNullOrEmpty(this.ContactName) && !debtClass.IsContactNameNull() && debtClass.ContactName != "")
			{

				this.isContactNameInherited = debtClass.DebtClassId != this.DebtClassId;
				this.ContactName = debtClass.ContactName;

			}
			if (String.IsNullOrEmpty(this.Department) && !debtClass.IsDepartmentNull() && debtClass.Department != "")
			{

				this.isDepartmentInherited = debtClass.DebtClassId != this.DebtClassId;
				this.Department = debtClass.Department;

			}
			if (String.IsNullOrEmpty(this.Email) && !debtClass.IsEmailNull() && debtClass.Email != "")
			{

				this.isEmailInherited = debtClass.DebtClassId != this.DebtClassId;
				this.Email = debtClass.Email;

			}
			if (String.IsNullOrEmpty(this.Fax) && !debtClass.IsFaxNull() && debtClass.Fax != "")
			{

				this.isFaxInherited = debtClass.DebtClassId != this.DebtClassId;
				this.Fax = debtClass.Fax;

			}
			if (String.IsNullOrEmpty(this.ForBenefitOf) && !debtClass.IsForBenefitOfNull() && debtClass.ForBenefitOf != "")
			{

				this.isForBenefitOfInherited = debtClass.DebtClassId != this.DebtClassId;
				this.ForBenefitOf = debtClass.ForBenefitOf;

			}
			if (String.IsNullOrEmpty(this.Phone) && !debtClass.IsPhoneNull() && debtClass.Phone != "")
			{

				this.isPhoneInherited = debtClass.DebtClassId != this.DebtClassId;
				this.Phone = debtClass.Phone;

			}
			if (this.Province == null && !debtClass.IsProvinceIdNull() && debtClass.ProvinceId != null)
			{

				this.isStateInherited = debtClass.DebtClassId != this.DebtClassId;
				this.Province = debtClass.ProvinceId;

			}
			if (String.IsNullOrEmpty(this.PostalCode) && !debtClass.IsPostalCodeNull() && debtClass.PostalCode != "")
			{

				this.isPostalCodeInherited = debtClass.DebtClassId != this.DebtClassId;
				this.PostalCode = debtClass.PostalCode;

			}

		}

	}

}
 
