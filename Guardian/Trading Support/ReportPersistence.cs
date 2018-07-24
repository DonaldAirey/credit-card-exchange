namespace FluidTrade.Guardian
{
	using System;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// 
	/// </summary>
	internal class ReportPersistence : DataModelPersistence<Report>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public ReportPersistence()
		{
		}

		/// <summary>
		/// Create a new DebtHolder
		/// </summary>
		/// <returns></returns>
		public override Guid Create(Report record)
		{
			DataModel dataModel = new DataModel();
			Guid reportId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			////Create a entry in Province			
			dataModel.CreateReport(
				record.ExternalId0,
				record.Name,
				reportId,
				record.ReportTypeId,
				record.Xaml);

			return reportId;
		}

		/// <summary>
		/// Update Province
		/// </summary>
		/// <returns></returns>
		public override void Update(Report record)
		{
			DataModel dataModel = new DataModel();


			if (record.RowId == null || DataModel.Report.ReportKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Report", new object[] { record.RowId }));
			}

			dataModel.UpdateReport(
				record.ExternalId0,
				record.Name,
				null,
				new object[] { record.RowId },
				record.ReportTypeId,
				record.RowVersion,
				record.Xaml);
		}

		/// <summary>
		/// Delete a Province
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(Report record)
		{
			DataModel dataModel = new DataModel();

			if (record.RowId == null || DataModel.Report.ReportKey.Find(record.RowId) == null)
			{
				return ErrorCode.RecordNotFound;
			}

			dataModel.DestroyReport(
					new object[] { record.RowId },
					record.RowVersion);

			return ErrorCode.Success;
		}


		public override Report Get(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}
