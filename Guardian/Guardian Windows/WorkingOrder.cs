namespace FluidTrade.Guardian.Windows
{
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// A collection of client orders that can be allocated as a single unit.
	/// </summary>
	public class WorkingOrder : GuardianObject, IMovableObject
	{

		private Guid blotterId;
		private DateTime modifiedTime;
		private Guid securityId;
		private Guid workingOrderId;
		private Int64 rowVersion;
		private String symbol;

		/// <summary>
		/// Create a new working order.
		/// </summary>
		/// <param name="workingOrderRow">The WorkingOrderRow of the working order.</param>
		public WorkingOrder(WorkingOrderRow workingOrderRow)
		{

			// Initialize the object.
			this.blotterId = workingOrderRow.BlotterId;
			this.modifiedTime = workingOrderRow.ModifiedTime;
			this.securityId = workingOrderRow.SecurityId;
			this.workingOrderId = workingOrderRow.WorkingOrderId;
			this.rowVersion = workingOrderRow.RowVersion;

			this.symbol = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId.Symbol;

		}


		/// <summary>
		/// Create a new working order.
		/// </summary>
		/// <param name="workingOrderId">The WorkingOrderId of the working order.</param>
		/// <param name="blotterId">The BlotterId of the blotter this working order will be created in.</param>
		public WorkingOrder(Guid workingOrderId, Guid blotterId)
		{

			// Initialize the object.
			this.blotterId = blotterId;
			this.workingOrderId = workingOrderId;

		}

		/// <summary>
		/// The blotter id of the blotter the working order is in.
		/// </summary>
		public Guid BlotterId
		{

			get { return this.blotterId; }
			set
			{

				if (this.blotterId != value)
				{
					this.blotterId = value;
					this.OnPropertyChanged("BlotterId");
				}

			}

		}

		/// <summary>
		/// The last time the working order was modified.
		/// </summary>
		public DateTime ModifiedTime
		{

			get { return this.modifiedTime; }

		}

		/// <summary>
		/// RowVersion
		/// </summary>
		public Int64 RowVersion
		{
			get { return rowVersion; }
		}

		/// <summary>
		/// The security id.
		/// </summary>
		public Guid SecurityId
		{

			get { return this.securityId; }

		}

		/// <summary>
		/// The symbol of the security this working order is for.
		/// </summary>
		public String Symbol
		{

			get { return this.symbol; }

		}

		/// <summary>
		/// The working order id.
		/// </summary>
		public Guid WorkingOrderId
		{

			get { return this.workingOrderId; }

		}

		/// <summary>
		/// Commit any changes to this object to the server.
		/// </summary>
		public override void Commit()
		{

			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				TradingSupportReference.WorkingOrderRecord record = new TradingSupportReference.WorkingOrderRecord();
				Int32 tries = 0;
				MethodResponseErrorCode response;

				this.PopulateRecord(record);

				do
				{

					if (this.Deleted)
					{

						response = tradingSupportClient.DeleteWorkingOrder(new TradingSupportReference.WorkingOrderRecord[] { record });

						if (this.GetFirstErrorCode(response) == ErrorCode.RecordExists)
							throw new IsSettledException(this.ToString() + " is settled");

					}
					else
					{

						response = tradingSupportClient.UpdateWorkingOrder(new TradingSupportReference.WorkingOrderRecord[] { record });

					}

					tries += 1;

					if (!response.IsSuccessful && (response.Errors.Length == 0 || response.Errors[0].ErrorCode != ErrorCode.Deadlock))
						GuardianObject.ThrowErrorInfo(response.Errors[0]);

				} while (!response.IsSuccessful && tries < WorkingOrder.TotalRetries);

				this.Modified = false;

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				if (this.Deleted)
					throw new DeleteException(this, exception);

			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

		}

		/// <summary>
		/// Delete an set of working orders.
		/// </summary>
		/// <param name="workingOrders">The set of working orders.</param>
		/// <returns>The actual bulk size used.</returns>
		protected override Int32 Delete(List<GuardianObject> workingOrders)
		{

			Int32 attemptedBulkSize = workingOrders.Count;
			Int32 actualBulkSize = attemptedBulkSize;
			GuardianObject failedObject = null;
			TradingSupportReference.WorkingOrderRecord[] records;
			Dictionary<TradingSupportReference.WorkingOrderRecord, WorkingOrder> recordsToOrders =
				new Dictionary<TradingSupportReference.WorkingOrderRecord, WorkingOrder>();

			records = new TradingSupportReference.WorkingOrderRecord[workingOrders.Count];

			// Convert the GuardianObjects to records we can push up to the server.
			for (Int32 index = 0; index < records.Length; ++index)
			{

				WorkingOrder workingOrder = workingOrders[0] as WorkingOrder;

				records[index] = new TradingSupportReference.WorkingOrderRecord();
				workingOrder.PopulateRecord(records[index]);
				recordsToOrders[records[index]] = workingOrder;
				workingOrders.RemoveAt(0);

			}

			try
			{

				Int32 sentSize;
				MethodResponseErrorCode response;

				response = NetworkHelper.Attempt<MethodResponseErrorCode>(
					(client, a) =>
						client.DeleteWorkingOrder(a as TradingSupportReference.WorkingOrderRecord[]),
					records,
					true,
					out sentSize);

				if (sentSize < attemptedBulkSize)
					actualBulkSize = sentSize;

				if (!response.IsSuccessful)
				{

					foreach (ErrorInfo errorInfo in response.Errors)
					{

						// The bulk index is an index into the set we sent, which may be smaller than the set passed in.
						failedObject = recordsToOrders[records[errorInfo.BulkIndex]];

						// If the error's "just" a deadlock, add it to the retry list we can attempt it again.
						if (errorInfo.ErrorCode == ErrorCode.RecordExists)
						{

							throw new IsSettledException(this.ToString() + " is settled");

						}
						// We can safely ignore not-found errors (we are deleting after all), but if the error's more severe, forget the whole
						// thing and throw up the error.
						else if (errorInfo.ErrorCode != ErrorCode.RecordNotFound)
						{

							GuardianObject.ThrowErrorInfo(response.Errors[0]);

						}

					}

				}

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				throw new DeleteException(failedObject, exception);

			}

			return actualBulkSize;

		}

		/// <summary>
		/// Determine whether this working order is a settled accounts.
		/// </summary>
		/// <returns>True if the working order is a settled accounts.</returns>
		protected virtual Boolean IsSettled()
		{

			return false;

		}

		/// <summary>
		/// Move this working order from one blotter to another.
		/// </summary>
		/// <param name="newParent">The new location of the working order.</param>
		/// <param name="errors">The list of errors and at what index.</param>
		public void Move(GuardianObject newParent, List<ErrorInfo> errors)
		{

			this.Move(new List<IMovableObject> { this }, newParent, errors);

		}

		/// <summary>
		/// Move several working orders from one blotter to another.
		/// </summary>
		/// <param name="objects">The list of working orders to move.</param>
		/// <param name="newParent">The new location of the working orders.</param>
		/// <param name="errors">The list of errors and at what index.</param>
		public void Move(List<IMovableObject> objects, GuardianObject newParent, List<ErrorInfo> errors)
		{

			try
			{

				Int32 tries = 0;
				bool retry = false;
				BaseRecord[] records = this.PopulateSecurityRecords(objects);
				MethodResponseErrorCode response = null;

				do
				{

					TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

					response = this.MoveSecurity(tradingSupportClient, records, newParent as Blotter);

					if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
						tradingSupportClient.Close();

					tries += 1;

					if (!response.IsSuccessful)
					{

						List<BaseRecord> retryRecords = new List<BaseRecord>();

						foreach (ErrorInfo errorInfo in response.Errors)
							// If the error's "just" a deadlock, add it to the retry list we can attempt it again.
							if (errorInfo.ErrorCode == ErrorCode.Deadlock)
								retryRecords.Add(records[errorInfo.BulkIndex]);
							//No need to retry if the client does not have permission to move.  
							else if (errorInfo.ErrorCode == ErrorCode.AccessDenied)
								GuardianObject.ThrowErrorInfo(response.Errors[0]);
							else
								errors.Add(errorInfo);

						records = retryRecords.ToArray();
						retry = retryRecords.Count > 0;
					}


				} while (retry && tries < WorkingOrder.TotalRetries);

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				throw;

			}

		}

		/// <summary>
		/// Actual execute a move by moving the securities.
		/// </summary>
		/// <param name="client">The trading support client to use.</param>
		/// <param name="records">The security records to move.</param>
		/// <param name="newParent">The new blotter.</param>
		/// <returns>The server response</returns>
		protected virtual MethodResponseErrorCode MoveSecurity(
			TradingSupportClient client,
			BaseRecord[] records,
			Blotter newParent)
		{

			throw new NotImplementedException("Only consumer debt securities are implemented");

		}

		/// <summary>
		/// Create a new working order based on a working order row.
		/// </summary>
		/// <param name="workingOrderRow">The working order data row.</param>
		/// <returns>The new working order object.</returns>
		public static WorkingOrder New(WorkingOrderRow workingOrderRow)
		{

			SecurityRow securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;

			if (securityRow.GetConsumerTrustRows().Length > 0)
				return new ConsumerTrustWorkingOrder(workingOrderRow);
			if (securityRow.GetConsumerDebtRows().Length > 0)
				return new ConsumerDebtWorkingOrder(workingOrderRow);
			else
				return new WorkingOrder(workingOrderRow);

		}

		/// <summary>
		/// Populates a trading support record with the contents of the entity.
		/// </summary>
		/// <param name="baseRecord">The empty record to populate.</param>
		protected override void PopulateRecord(TradingSupportReference.BaseRecord baseRecord)
		{

			TradingSupportReference.WorkingOrderRecord record = baseRecord as TradingSupportReference.WorkingOrderRecord;

			record.BlotterId = this.BlotterId;
			record.RowId = this.WorkingOrderId;
			record.RowVersion = this.rowVersion;
			record.SecurityId = this.SecurityId;
			record.WorkingOrderId = this.WorkingOrderId;

		}

		/// <summary>
		/// Populates trading support records for the securities of each working order in a set.
		/// </summary>
		/// <param name="orders">The working orders to populate records for.</param>
		/// <returns>The populated records.</returns>
		protected virtual BaseRecord[] PopulateSecurityRecords(List<IMovableObject> orders)
		{
			BaseRecord[] records = new BaseRecord[orders.Count];

			for (Int32 index = 0; index < orders.Count; ++index)
			{

				WorkingOrder order = orders[index] as WorkingOrder;

				records[index] = new BaseRecord { RowId = order.WorkingOrderId, RowVersion = order.RowVersion };

			}

			return records;

		}

		/// <summary>
		/// A description of this working order.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{

			return this.Symbol;

		}

		/// <summary>
		/// Update this object based on another.
		/// </summary>
		/// <param name="obj">The object to draw updates from.</param>
		public override void Update(GuardianObject obj)
		{

			WorkingOrder workingOrder = obj as WorkingOrder;

			if (!workingOrder.Modified)
			{

				this.rowVersion = workingOrder.rowVersion;
				this.modifiedTime = workingOrder.ModifiedTime;
				this.securityId = workingOrder.SecurityId;
				this.symbol = workingOrder.Symbol;

			}

		}

	}

}
