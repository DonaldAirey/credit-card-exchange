using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using FluidTrade.Core;
using System.ServiceModel;
using System.Data.SqlClient;

namespace FluidTrade.Guardian.Records
{
	
	/// <summary>
	/// Error info.
	/// </summary>
	[DataContract]
	public class ErrorInfo
	{

		/// <summary>
		/// Which record in a bulk operation this error corresponds to.
		/// </summary>
		[DataMember]
		public int BulkIndex { get; set; }

		/// <summary>
		/// See above
		/// </summary>
		[DataMember]
		public ErrorCode ErrorCode { get; set; }

		/// <summary>
		/// Message.
		/// </summary>
		[DataMember]
		public String Message { get; set; }

	}

	/// <summary>
	/// Method response data contract. WCF requires conctrete types for marshalling.
	/// So it will change the Class MethodResonse[Guid] to MethodResponseofGuid[Mangledstring].
	/// The Name directive gives us more control how the generated name looks.
	/// </summary>
	/// <typeparam name="ReturnType"></typeparam>
	[DataContract(Name = "MethodResponse{0}")]
	public class MethodResponse<ReturnType>
	{
		//Errordata collection
		private List<ErrorInfo> errors = new List<ErrorInfo>();

		/// <summary>
		/// Constructor
		/// </summary>
		public MethodResponse () 
		{
			IsSuccessful = true;
		}
		/// <summary>
		/// General status of the whole operation.
		/// </summary>
		[DataMember]
		[DefaultValue(true)]
		public Boolean IsSuccessful { get; private set; }

		/// <summary>
		/// Add error.
		/// </summary>
		/// <param name="toAdd"></param>
		public void AddError(ErrorInfo toAdd)
		{
			IsSuccessful = false;
			errors.Add(toAdd);
		}

		/// <summary>
		/// Add error
		/// </summary>
		/// <param name="toAdd"></param>
		public void AddError(String toAdd)
		{
			IsSuccessful = false;
			errors.Add(new ErrorInfo() { ErrorCode = ErrorCode.NoJoy, Message = toAdd });
		}

		/// <summary>
		/// Add error information.
		/// </summary>
		/// <param name="toAdd"></param>
		/// <param name="code"></param>
		public void AddError(String toAdd, ErrorCode code)
		{
			IsSuccessful = false;
			errors.Add(new ErrorInfo() { ErrorCode = code, Message = toAdd });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(Exception exception, Int32 bulkIndex)
		{

			Type type = exception.GetType();
			IsSuccessful = false;

			if (type == typeof(FaultException<ArgumentFault>))
				this.AddError(exception as FaultException<ArgumentFault>, bulkIndex);
			if (type == typeof(FaultException<FieldRequiredFault>))
				this.AddError(exception as FaultException<FieldRequiredFault>, bulkIndex);
			else if (type == typeof(FaultException<FormatFault>))
				this.AddError(exception as FaultException<FormatFault>, bulkIndex);
			else if (type == typeof(FaultException<IndexNotFoundFault>))
				this.AddError(exception as FaultException<IndexNotFoundFault>, bulkIndex);
			else if (type == typeof(FaultException<OptimisticConcurrencyFault>))
				this.AddError(exception as FaultException<OptimisticConcurrencyFault>, bulkIndex);
			else if (type == typeof(FaultException<PaymentMethodFault>))
				this.AddError(exception as FaultException<PaymentMethodFault>, bulkIndex);
			else if (type == typeof(FaultException<RecordExistsFault>))
				this.AddError(exception as FaultException<RecordExistsFault>, bulkIndex);
			else if (type == typeof(FaultException<RecordNotFoundFault>))
				this.AddError(exception as FaultException<RecordNotFoundFault>, bulkIndex);
			else if (type == typeof(FaultException<SecurityFault>))
				this.AddError(exception as FaultException<SecurityFault>, bulkIndex);
			else if (type == typeof(FaultException<SynchronizationLockFault>))
				this.AddError(exception as FaultException<SynchronizationLockFault>, bulkIndex);
			else if (type == typeof(SqlException))
				this.AddError(exception as SqlException, bulkIndex);
			else
				errors.Add(new ErrorInfo() {
					BulkIndex = bulkIndex,
					ErrorCode = ErrorCode.GeneralProtectionFault,
					Message = String.Format("{0}: {1}", exception.GetType(), exception.Message) });

		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<ArgumentFault> exception, Int32 bulkIndex)
		{
			String message;
			if (String.IsNullOrEmpty(exception.Detail.Message))
				message = exception.Message;
			else
				message = String.Format("{0} ({1})", exception.Message, exception.Detail.Message);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.ArgumentError, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<FieldRequiredFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1})", exception.Message, exception.Detail.Message);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.FieldRequired, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<FormatFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1})", exception.Message, exception.Detail.Message);
			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.GeneralProtectionFault, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<IndexNotFoundFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1}, {2})", exception.Message, exception.Detail.TableName, exception.Detail.IndexName);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.RecordNotFound, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<OptimisticConcurrencyFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1}, {2})", exception.Message, exception.Detail.TableName, exception.Detail.Key);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.GeneralProtectionFault, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<PaymentMethodFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1})", exception.Message, exception.Detail.Message);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.GeneralProtectionFault, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<RecordExistsFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1}, {2})", exception.Message, exception.Detail.TableName, exception.Detail.Key);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.RecordExists, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<RecordNotFoundFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1}, {2})", exception.Message, exception.Detail.TableName, exception.Detail.Key);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.RecordNotFound, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<SecurityFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1})", exception.Message, exception.Detail.Message);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.AccessDenied, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(FaultException<SynchronizationLockFault> exception, Int32 bulkIndex)
		{
			String message = String.Format("{0} ({1})", exception.Message, exception.Detail.TableName);

			IsSuccessful = false;
			errors.Add(new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.Deadlock, Message = message });
		}

		/// <summary>
		/// Add exception information to the object.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <param name="bulkIndex">The index into the record set where the error happened.</param>
		public void AddError(SqlException exception, Int32 bulkIndex)
		{
			ErrorInfo errorInfo = new ErrorInfo() { BulkIndex = bulkIndex, ErrorCode = ErrorCode.SqlError, Message = exception.Message };
			IsSuccessful = false;

			foreach (SqlError error in exception.Errors)
				// These are error codes gleaned from Sql Server's master.sys.messages view that appear to be for deadlocks of different kinds.
				if (error.Number == 17888 || error.Number == 3635 || error.Number == 1205)
				{
					errorInfo.ErrorCode = ErrorCode.Deadlock;
					break;
				}

			errors.Add(errorInfo);
		}

		/// <summary>
		/// Error log
		/// </summary>
		[DataMember]
		public ErrorInfo[] Errors
		{
			get { return errors.ToArray(); }			
		}

		/// <summary>
		/// Return type.
		/// </summary>
		[DataMember]
		public ReturnType Result
		{
			get;
			set;
		}

		/// <summary>
		/// Determine if there are any errors.
		/// </summary>
		/// <returns></returns>
		public bool HasErrors()
		{
			return errors.Count > 0;
		}
	}
}
