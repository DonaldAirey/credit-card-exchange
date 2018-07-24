namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.TradingSupportReference;

	class ConsumerDebtWorkingOrder : WorkingOrder
	{

		Int64 consumerDebtRowVersion;
		String firstName;
		String lastName;
		String socialSecurityNumber;

		/// <summary>
		/// Create a new working order.
		/// </summary>
		/// <param name="workingOrderRow">The WorkingOrderRow of the working order.</param>
		public ConsumerDebtWorkingOrder(WorkingOrderRow workingOrderRow)
			: base(workingOrderRow)
		{

			SecurityRow securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
			ConsumerDebtRow consumerDebtRow = securityRow.GetConsumerDebtRows()[0];
			ConsumerRow consumerRow = consumerDebtRow.ConsumerRow;

			this.consumerDebtRowVersion = consumerDebtRow.RowVersion;
			this.firstName = consumerRow.IsFirstNameNull() ? null : consumerRow.FirstName;
			this.lastName = consumerRow.IsLastNameNull() ? null : consumerRow.LastName;
			this.socialSecurityNumber = consumerRow.SocialSecurityNumber;

		}

		/// <summary>
		/// The first name of the consumer this order is for.
		/// </summary>
		public String FirstName
		{
			get { return this.firstName; }
			set
			{

				if (value != this.firstName)
				{

					this.firstName = value;
					this.OnPropertyChanged("FirstName");

				}

			}
		}

		/// <summary>
		/// The last name of the consumer this order is for.
		/// </summary>
		public String LastName
		{
			get { return this.lastName; }
			set
			{

				if (value != this.lastName)
				{

					this.lastName = value;
					this.OnPropertyChanged("LastName");

				}

			}
		}

		/// <summary>
		/// The social security number of the consumer this order is for.
		/// </summary>
		public String SocialSecurityNumber
		{
			get { return this.socialSecurityNumber; }
			set
			{

				if (value != this.socialSecurityNumber)
				{

					this.socialSecurityNumber = value;
					this.OnPropertyChanged("SocialSecurityNumber");

				}

			}
		}

		/// <summary>
		/// The "type" of the working order.
		/// </summary>
		public override string TypeName
		{
			get
			{
				return "Account";
			}
		}

		/// <summary>
		/// Determine whether this working order is a settled accounts.
		/// </summary>
		/// <returns>True if the working order is a settled accounts.</returns>
		protected override Boolean IsSettled()
		{

			Boolean isSettled = false;

			lock (DataModel.SyncRoot)
				foreach (MatchRow matchRow in DataModel.WorkingOrder.WorkingOrderKey.Find(this.WorkingOrderId).GetMatchRows())
				{

					foreach (ConsumerDebtNegotiationRow consumerDebtNegotiationRow in matchRow.GetConsumerDebtNegotiationRows())
						if (consumerDebtNegotiationRow.GetConsumerDebtSettlementRows().Length > 0)
						{

							isSettled = true;
							break;

						}

				}

			return isSettled;

		}

		/// <summary>
		/// Actual execute a move by moving the securities.
		/// </summary>
		/// <param name="client">The trading support client to use.</param>
		/// <param name="records">The security records to move.</param>
		/// <param name="newParent">The new blotter.</param>
		/// <returns>The server response</returns>
		protected override MethodResponseErrorCode MoveSecurity(
			TradingSupportClient client,
			BaseRecord[] records,
			Blotter newParent)
		{

			MethodResponseErrorCode response = null;

			response = client.MoveConsumerDebtToBlotter(newParent.BlotterId, records);

			return response;

		}

		/// <summary>
		/// A string representation of the working order.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{

			if (this.FirstName != null && this.LastName != null)
				return String.Format("{1}, {0} -  SSN: {2}", this.FirstName, this.LastName, this.SocialSecurityNumber);
			else if (this.LastName != null)
				return String.Format("{0} -  SSN: {1}", this.LastName, this.SocialSecurityNumber);
			else if (this.FirstName != null)
				return String.Format("{0} - SSN: {1}", this.FirstName, this.SocialSecurityNumber);
			else
				return String.Format("SSN: {0}", this.SocialSecurityNumber);

		}

	}
}
