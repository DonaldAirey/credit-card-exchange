namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Records;
	using FluidTrade.Core;

	/// <summary>
	/// Persistence methods for commission schedules.
	/// </summary>
	internal class CommissionSchedulePersistence : DataModelPersistence<CommissionSchedule>
	{

		/// <summary>
		/// Create a commission schedule.
		/// </summary>
		/// <param name="record">The commission schedule.</param>
		/// <returns>The CommissionScheduleId of the new schedule.</returns>
		public override Guid Create(CommissionSchedule record)
		{

			DataModel dataModel = new DataModel();
			Guid commissionScheduleId = Guid.NewGuid();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			dataModel.CreateCommissionSchedule(commissionScheduleId, null, record.Name);

			foreach (CommissionTranche tranche in record.CommissionTranches)
			{

				dataModel.CreateCommissionTranche(
					commissionScheduleId,
					Guid.NewGuid(),
					tranche.CommissionType,
					tranche.CommissionUnit,
					tranche.EndRange,
					null,
					tranche.StartRange,
					tranche.Value);

			}

			return commissionScheduleId;

		}

		/// <summary>
		/// Retrieve a commission schedule.
		/// </summary>
		/// <param name="id">The CommissionScheduleId.</param>
		/// <returns>The commission schedule object.</returns>
		public override CommissionSchedule Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Update a commission schedule.
		/// </summary>
		/// <param name="record">The commission schedule object.</param>
		public override void Update(CommissionSchedule record)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Delete a commission schedule.
		/// </summary>
		/// <param name="record">The commission schedule object.</param>
		/// <returns>Any resulting error.</returns>
		public override ErrorCode Delete(CommissionSchedule record)
		{
			throw new NotImplementedException();
		}

	}

}
