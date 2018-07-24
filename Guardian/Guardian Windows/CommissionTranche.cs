namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;
	using System.ComponentModel;
	
	/// <summary>
	/// The UI representation of a CommissionTrancheRow.
	/// </summary>
	public class CommissionTranche : INotifyPropertyChanged
	{

		private Guid commissionScheduleId;
		private Guid commissionTrancheId;
		private CommissionType commissionType;
		private CommissionUnit commissionUnit;
		private Decimal? endRange;
		private Int64 rowVersion;
		private Decimal startRange;
		private Decimal value;

		/// <summary>
		/// The event raised when a property of the CommissionTranche object has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Create a new (empty) tranche.
		/// </summary>
		/// <param name="commissionScheduleId"></param>
		public CommissionTranche(Guid commissionScheduleId)
		{

			this.commissionScheduleId = commissionScheduleId;
			this.commissionTrancheId = Guid.Empty;
			this.commissionType = new CommissionType();
			this.commissionUnit = new CommissionUnit();
			this.endRange = 0m;
			this.startRange = 0m;
			this.value = 0m;

		}

		/// <summary>
		/// Create a new CommissionTranche based on a commission tranche row from the DataModel.
		/// </summary>
		/// <param name="commissionTrancheRow">The row to base this commission tranche on.</param>
		public CommissionTranche(CommissionTrancheRow commissionTrancheRow)
		{

			this.commissionScheduleId = commissionTrancheRow.CommissionScheduleId;
			this.commissionTrancheId = commissionTrancheRow.CommissionTrancheId;
			this.commissionType = commissionTrancheRow.CommissionTypeRow.CommissionTypeCode;
			this.commissionUnit = commissionTrancheRow.CommissionUnitRow.CommissionUnitCode;
			this.endRange = commissionTrancheRow.IsEndRangeNull()? null : (Decimal?)commissionTrancheRow.EndRange;
			this.rowVersion = commissionTrancheRow.RowVersion;
			this.startRange = commissionTrancheRow.StartRange;
			this.value = commissionTrancheRow.Value;

		}

		/// <summary>
		/// Create a new CommissionTranche based on a commission tranche row from the DataModel.
		/// </summary>
		/// <param name="commissionTranche">The commission tranche to base this new tranche on.</param>
		public CommissionTranche(CommissionTranche commissionTranche)
		{

			this.commissionScheduleId = commissionTranche.CommissionScheduleId;
			this.commissionTrancheId = commissionTranche.CommissionTrancheId;
			this.commissionType = commissionTranche.CommissionType;
			this.commissionUnit = commissionTranche.CommissionUnit;
			this.rowVersion = commissionTranche.RowVersion;
			this.startRange = commissionTranche.StartRange;
			this.endRange = commissionTranche.EndRange;
			this.value = commissionTranche.Value;

		}

		/// <summary>
		/// Gets or sets the CommissionScheduleId of the object.
		/// </summary>
		public Guid CommissionScheduleId
		{
			get { return this.commissionScheduleId; }
			protected set
			{
				if (this.commissionScheduleId != value)
				{
					this.commissionScheduleId = value;
					OnPropertyChanged("CommissionScheduleId");
				}
			}
		}

		/// <summary>
		/// Gets or sets the CommissionTrancheId of the object.
		/// </summary>
		public Guid CommissionTrancheId
		{
			get { return this.commissionScheduleId; }
			protected set
			{
				if (this.commissionTrancheId != value)
				{
					this.commissionTrancheId = value;
					OnPropertyChanged("CommissionTrancheId");
				}
			}
		}

		/// <summary>
		/// Gets or sets the comission type of the object.
		/// </summary>
		public CommissionType CommissionType
		{
			get { return this.commissionType; }
			set
			{
				if (this.commissionType != value)
				{
					this.commissionType = value;
					OnPropertyChanged("CommissionType");
				}
			}
		}

		/// <summary>
		/// Gets or sets the commission unit of the object.
		/// </summary>
		public CommissionUnit CommissionUnit
		{
			get { return this.commissionUnit; }
			set
			{
				if (this.commissionUnit != value)
				{
					this.commissionUnit = value;
					OnPropertyChanged("CommissionUnit");
				}
			}
		}

		/// <summary>
		/// Gets or sets the end of the range of the object.
		/// </summary>
		public Decimal? EndRange
		{
			get { return this.endRange; }
			set
			{
				if (this.endRange != value)
				{
					this.endRange = value;
					OnPropertyChanged("EndRange");
				}
			}
		}

		/// <summary>
		/// Gets the row version this object corresponds to.
		/// </summary>
		public Int64 RowVersion
		{
			get { return this.rowVersion; }
		}

		/// <summary>
		/// Gets or sets the start of the range of the object.
		/// </summary>
		public Decimal StartRange
		{
			get { return this.startRange; }
			set
			{
				if (this.startRange != value)
				{
					this.startRange = value;
					OnPropertyChanged("StartRange");
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of the object.
		/// </summary>
		public Decimal Value
		{
			get { return this.value; }
			set
			{
				if (this.value != value)
				{
					this.value = value;
					OnPropertyChanged("Value");
				}
			}
		}

		/// <summary>
		/// Raise the property changed event with specified arguments.
		/// </summary>
		/// <param name="property">The arguments to pass the event subscribers.</param>
		protected virtual void OnPropertyChanged(string property)
		{

			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));

		}

	}

}
