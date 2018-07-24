namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.ComponentModel;
	using System.Collections.ObjectModel;

	/// <summary>
	/// The UI representation of a CommissionScheduleRow.
	/// </summary>
	public class CommissionSchedule : INotifyPropertyChanged
	{

		private Guid commissionScheduleId;
		private String name;
		private ObservableCollection<CommissionTranche> commissionTranches;
		private Int64 rowVersion;

		/// <summary>
		/// The event raised when a property of the CommissionTranche object has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Create a new (empty) commission schedule.
		/// </summary>
		public CommissionSchedule()
		{

			this.commissionScheduleId = Guid.Empty;
			this.name = null;
			this.commissionTranches = new ObservableCollection<CommissionTranche>();
			this.commissionTranches.CollectionChanged += OnCommissionTranchesChanged;

		}

		/// <summary>
		/// Create a new commission schedule object from a commission schedule row.
		/// </summary>
		/// <param name="commissionScheduleRow"></param>
		public CommissionSchedule(CommissionScheduleRow commissionScheduleRow)
		{

			this.commissionScheduleId = commissionScheduleRow.CommissionScheduleId;
			this.commissionTranches = new ObservableCollection<CommissionTranche>();
			this.commissionTranches.CollectionChanged += this.OnCommissionTranchesChanged;
			this.Update(commissionScheduleRow);

		}

		/// <summary>
		/// Create a new commission schedule object based on another.
		/// </summary>
		/// <param name="commissionSchedule"></param>
		public CommissionSchedule(CommissionSchedule commissionSchedule)
		{

			this.commissionScheduleId = commissionSchedule.CommissionScheduleId;
			this.commissionTranches = new ObservableCollection<CommissionTranche>();
			this.commissionTranches.CollectionChanged += this.OnCommissionTranchesChanged;
			this.Update(commissionSchedule);

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
		/// Gets or sets the Name of the object.
		/// </summary>
		public String Name
		{
			get { return this.name; }
			set
			{
				if (this.name != value)
				{
					this.name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		/// <summary>
		/// Gets the tranches of the object.
		/// </summary>
		public ObservableCollection<CommissionTranche> CommissionTranches
		{
			get { return this.commissionTranches; }
		}

		/// <summary>
		/// Gets the row version this object corresponds to.
		/// </summary>
		public Int64 RowVersion
		{
			get { return this.rowVersion; }
		}

		/// <summary>
		/// Handle a change in an individual tranche.
		/// </summary>
		/// <param name="sender">The tranche that changed.</param>
		/// <param name="e">The event arguments.</param>
		private void OnCommissionTrancheChanged(object sender, PropertyChangedEventArgs e)
		{

			this.OnPropertyChanged("CommissionTranches");

		}

		/// <summary>
		/// Handle changes to the tranche collection.
		/// </summary>
		/// <param name="sender">The tranche collection.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCommissionTranchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArgs)
		{

			if (eventArgs.NewItems != null)
				foreach (CommissionTranche tranche in eventArgs.NewItems)
					tranche.PropertyChanged += OnCommissionTrancheChanged;

			if (eventArgs.OldItems != null)
				foreach (CommissionTranche tranche in eventArgs.OldItems)
					tranche.PropertyChanged -= OnCommissionTrancheChanged;

			this.OnPropertyChanged("CommissionTranches");

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

		/// <summary>
		/// Update the schedule from another one.
		/// </summary>
		/// <param name="commissionSchedule">The source of updated information.</param>
		public void Update(CommissionSchedule commissionSchedule)
		{

			this.Name = commissionSchedule.Name;
			this.rowVersion = commissionSchedule.RowVersion;

//			foreach (CommissionTranche commissionTranche in this.CommissionTranches)
//				commissionTranche.PropertyChanged -= this.OnCommissionTrancheChanged;
			this.CommissionTranches.Clear();

			foreach (CommissionTranche commissionTranche in commissionSchedule.CommissionTranches)
			{

//				commissionTranche.PropertyChanged += this.OnCommissionTrancheChanged;
				this.CommissionTranches.Add(commissionTranche);

			}

		}

		/// <summary>
		/// Update the schedule from another one.
		/// </summary>
		/// <param name="commissionSchedule">The source of updated information.</param>
		public void Update(CommissionScheduleRow commissionSchedule)
		{

			this.Name = commissionSchedule.IsNameNull()? null : commissionSchedule.Name;
			this.rowVersion = commissionSchedule.RowVersion;

//			foreach (CommissionTranche commissionTranche in this.CommissionTranches)
//				commissionTranche.PropertyChanged -= this.OnCommissionTrancheChanged;
			this.CommissionTranches.Clear();

			foreach (CommissionTrancheRow commissionTrancheRow in commissionSchedule.GetCommissionTrancheRows())
			{

				CommissionTranche commissionTranche = new CommissionTranche(commissionTrancheRow);
//				commissionTranche.PropertyChanged += this.OnCommissionTrancheChanged;
				this.CommissionTranches.Add(commissionTranche);

			}

		}

	}

}
