namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Linq;
	using System.Data;
	using FluidTrade.Core;
	using System.ComponentModel;
	using System.Windows.Threading;
	using System.Windows;
	using System.Collections.Generic;

    /// <summary>
	/// Summary description for Blotter.
	/// </summary>
	public class Blotter : Entity
	{

		private CommissionSchedule commissionSchedule;

        /// <summary>
        /// Create a new Blotter entity.
        /// </summary>
        /// <param name="entityRow"></param>
		public Blotter(EntityRow entityRow) : base(entityRow)
		{


		}

		/// <summary>
		/// Create a duplicate blotter.
		/// </summary>
		/// <param name="source">The original blotter.</param>
		public Blotter(Blotter source) : base(source)
		{

			if (source.CommissionSchedule != null)
			{

				this.commissionSchedule = new CommissionSchedule(source.CommissionSchedule);
				this.commissionSchedule.PropertyChanged += this.OnCommissionScheduleChanged;

			}

		}

		/// <summary>
		/// The primary identifier of this object.
		/// </summary>
		public Guid BlotterId { get { return this.EntityId; } }

		/// <summary>
		/// The commission schedule directly assigned to this blotter, or null if this blotter inherits its comission schedule.
		/// </summary>
		public CommissionSchedule CommissionSchedule
		{

			get { return this.commissionSchedule; }
			set
			{

				if (this.commissionSchedule != value)
				{

					if (this.commissionSchedule != null)
						this.commissionSchedule.PropertyChanged -= this.OnCommissionScheduleChanged;
					this.commissionSchedule = value;
					if (this.commissionSchedule != null)
						this.commissionSchedule.PropertyChanged += this.OnCommissionScheduleChanged;
					OnPropertyChanged(new PropertyChangedEventArgs("CommissionSchedule"));

				}

			}

		}

		/// <summary>
		/// Create an independent clone of this blotter.
		/// </summary>
		/// <returns>The new blotter.</returns>
		public override GuardianObject Clone()
		{

			GuardianObject newEntity = base.Clone();

			if (this.CommissionSchedule != null)
			{

				Blotter newBlotter = newEntity as Blotter;

				newBlotter.commissionSchedule = new CommissionSchedule(this.commissionSchedule);
				newBlotter.commissionSchedule.PropertyChanged += newBlotter.OnCommissionScheduleChanged;

			}

			return newEntity;

		}

		/// <summary>
		/// Copy the contents of another entity into this one.
		/// </summary>
		/// <param name="entity">The source entity.</param>
		public override void Copy(GuardianObject entity)
		{

			base.Copy(entity);

			if ((entity as Blotter).CommissionSchedule != null)
				if (this.CommissionSchedule == null)
				{

					this.CommissionSchedule = new CommissionSchedule((entity as Blotter).CommissionSchedule);
					this.CommissionSchedule.PropertyChanged += this.OnCommissionScheduleChanged;

				}
				else
				{

					this.CommissionSchedule.Update((entity as Blotter).CommissionSchedule);

				}
	
		}

		/// <summary>
		/// Retrvieve a list of GuardianObjects that depend on this one. That is, objects that must be deleted if this one is.
		/// </summary>
		/// <returns>A list of dependent objects.</returns>
		public override List<GuardianObject> GetDependents()
		{

			List<GuardianObject> list = new List<GuardianObject>();

			return this.GetDependents(list);

		}

		/// <summary>
		/// Retrieve a list of GuardianObjects that depend on this one. That is, objects that must be deleted if this one is.
		/// </summary>
		/// <param name="list">The list to add dependent objects to.</param>
		/// <returns>A list of dependent objects.</returns>
		private List<GuardianObject> GetDependents(List<GuardianObject> list)
		{

			EntityRow entityRow = DataModel.Entity.EntityKey.Find(this.EntityId);

			list.Add(this);

			// We're basically building a heap out of this branch of the tree so we can run through it LIFO order and maintain the integrity of the
			// entity tree.
			foreach (EntityTreeRow entityTreeRow in entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
			{

				Entity child = Entity.New(entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId);

				(child as Blotter).GetDependents(list);

			}

			return list;

		}

		/// <summary>
		/// Create the properties window appropriate for this type.
		/// </summary>
		/// <returns></returns>
		protected override WindowEntityProperties CreatePropertiesWindow()
		{

			return new WindowBlotterProperties();

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
		/// Load blotter specific information.
		/// </summary>
		protected override void FinishLoad()
		{

			base.FinishLoad();
	
			this.Update(DataModel.Entity.EntityKey.Find(this.EntityId));

		}

		/// <summary>
		/// Retrieve the commission schedule in use for this blotter, whether or not the schedule is set at this level in the tree. This function locks the data model.
		/// </summary>
		/// <returns>The commission schedule, or a new one if none could be found.</returns>
		public CommissionSchedule GetEffectiveCommissionSchedule()
		{

			return Blotter.GetEffectiveCommissionSchedule(this.BlotterId);

		}

		/// <summary>
		/// Retrieve the commission schedule in use for this blotter, whether or not the schedule is set at this level in the tree. This function locks the data model.
		/// </summary>
		/// <param name="blotterId">The blotter id of the blotter.</param>
		/// <returns>The commission schedule, or a new one if none could be found.</returns>
		public static CommissionSchedule GetEffectiveCommissionSchedule(Guid blotterId)
		{

			CommissionSchedule commissionSchedule = null;

			lock (DataModel.SyncRoot)
			{

				// HACK: The commission schedule is keyed to the debt class right now, but should change to Blotter.
				Guid? commissionScheduleId = (Guid?)Blotter.FindEffectiveField<Guid>(blotterId, DataModel.DebtClass.CommissionScheduleIdColumn);

				if (commissionScheduleId != null)
					commissionSchedule = new CommissionSchedule(
						DataModel.CommissionSchedule.CommissionScheduleKey.Find(commissionScheduleId.Value));
				else
					commissionSchedule = new CommissionSchedule();

			}

			return commissionSchedule;

		}

		/// <summary>
		/// Handle a change in the contents of the commission schedule.
		/// </summary>
		/// <param name="sender">The commission schedule object.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCommissionScheduleChanged(object sender, PropertyChangedEventArgs eventArgs)
		{

			this.OnPropertyChanged(new PropertyChangedEventArgs("CommissionSchedule"));

		}
		
		/// <summary>
		/// Populates a trading support record with the contents of the blotter.
		/// </summary>
		/// <param name="record">The empty record to populate.</param>
		protected override void PopulateRecord(TradingSupportReference.BaseRecord record)
		{

			base.PopulateRecord(record);

			(record as TradingSupportReference.Blotter).BlotterId = this.BlotterId;

			if (record is TradingSupportReference.DebtClass)
			{

				TradingSupportReference.DebtClass debtClass = record as TradingSupportReference.DebtClass;

				if (this.CommissionSchedule != null)
				{
					debtClass.CommissionSchedule = new TradingSupportReference.CommissionSchedule();

					debtClass.CommissionSchedule.RowId = this.CommissionSchedule.CommissionScheduleId;
					debtClass.CommissionSchedule.RowVersion = this.CommissionSchedule.RowVersion;
					debtClass.CommissionSchedule.CommissionTranches = new TradingSupportReference.CommissionTranche[this.CommissionSchedule.CommissionTranches.Count];

					for (int tranche = 0; tranche < this.CommissionSchedule.CommissionTranches.Count; ++tranche)
					{

						Guid commissionType = DataModel.CommissionType.First(row => row.CommissionTypeCode ==
								this.CommissionSchedule.CommissionTranches[tranche].CommissionType).CommissionTypeId;
						Guid commissionUnit = DataModel.CommissionUnit.First(row => row.CommissionUnitCode ==
								this.CommissionSchedule.CommissionTranches[tranche].CommissionUnit).CommissionUnitId;

						debtClass.CommissionSchedule.CommissionTranches[tranche] = new TradingSupportReference.CommissionTranche()
						{
							CommissionType = commissionType,
							CommissionUnit = commissionUnit,
							EndRange = this.CommissionSchedule.CommissionTranches[tranche].EndRange,
							RowId = this.CommissionSchedule.CommissionTranches[tranche].CommissionTrancheId,
							RowVersion = this.CommissionSchedule.CommissionTranches[tranche].RowVersion,
							StartRange = this.CommissionSchedule.CommissionTranches[tranche].StartRange,
							Value = this.CommissionSchedule.CommissionTranches[tranche].Value
						};

					}

				}

			}

		}

			/// <summary>
		/// Update the entity from another entity.
		/// </summary>
		/// <param name="obj">The entity to base the update on.</param>
		public override void Update(GuardianObject obj)
		{

			base.Update(obj);

			Entity entity = obj as Entity;

			if (!this.Modified && this.EntityId == entity.EntityId)
			{

				if (this.CommissionSchedule == null)
				{

					this.CommissionSchedule = new CommissionSchedule((entity as Blotter).CommissionSchedule);
					this.CommissionSchedule.PropertyChanged += this.OnCommissionScheduleChanged;

				}
				else
				{

					this.CommissionSchedule.Update((entity as Blotter).CommissionSchedule);

				}

				this.Modified = false;

			}

		}

		/// <summary>
		/// Update the entity with the information current in the DataModel.
		/// </summary>
		/// <param name="entityRow">The entity row to base the update on.</param>
		public override void Update(EntityRow entityRow)
		{

			// HACK: CommissionSchedule should be in all blotters.
			DebtClassRow debtClassRow = DataModel.DebtClass.DebtClassKey.Find(entityRow.EntityId);

			base.Update(entityRow);

			if (!this.Modified && this.EntityId == entityRow.EntityId)
			{

				if (debtClassRow != null && debtClassRow.CommissionScheduleRow != null)
				{

					if (this.commissionSchedule != null)
					{

						this.commissionSchedule.Update(debtClassRow.CommissionScheduleRow);

					}
					else
					{

						this.commissionSchedule = new CommissionSchedule(debtClassRow.CommissionScheduleRow);
						this.commissionSchedule.PropertyChanged += this.OnCommissionScheduleChanged;

					}

				}
				else if (this.commissionSchedule != null)
				{

					this.commissionSchedule.PropertyChanged -= this.OnCommissionScheduleChanged;
					this.commissionSchedule = null;

				}

				this.Modified = false;

			}
	
		}

	}

}
