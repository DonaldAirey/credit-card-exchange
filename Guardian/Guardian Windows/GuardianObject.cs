namespace FluidTrade.Guardian.Windows
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.ComponentModel;
	using FluidTrade.Core;
	using System.ServiceModel.Security;
	using System.ServiceModel;

	/// <summary>
	/// A base object for all object based on the data model.
	/// </summary>
	public abstract class GuardianObject : INotifyPropertyChanged
	{

		private static Int64 nextId = 0;
		/// <summary>
		/// Unique id for debugging.
		/// </summary>
		protected Int64 id;

		/// <summary>
		/// The total number of retries for deadlocks and other transient errors.
		/// </summary>
		protected static readonly Int32 TotalRetries = 3;

		private Boolean modified;
		private Boolean deleted;

		/// <summary>
		/// The event raised when a property of the Entity object has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Create a GuardianObject.
		/// </summary>
		public GuardianObject()
		{

			this.modified = false;
			this.deleted = false;
			this.id = GuardianObject.nextId++;

		}

		/// <summary>
		/// Gets or sets the Delete flag. If the Delete flag is true, the entity will be deleted on the next Commit call.
		/// </summary>
		public Boolean Deleted
		{

			get { return this.deleted; }
			set
			{

				if (this.deleted != value)
				{
					this.deleted = value;

					// Calling OnPropertyChanged will set the Modified flag, which, for the specific instance of Delete, we don't want.
					if (this.PropertyChanged != null)
						this.PropertyChanged(this, new PropertyChangedEventArgs("Deleted"));
				}

			}

		}

		/// <summary>
		/// Gets whether the entity was directly updated or not.
		/// </summary>
		public Boolean Modified
		{
			get { return this.modified; }
			protected set { this.modified = value; }
		}

		/// <summary>
		/// The name of the type of this object.
		/// </summary>
		public virtual String TypeName
		{
			get { return "file"; }
		}

		/// <summary>
		/// Copies the data from one entity to another.
		/// </summary>
		/// <param name="obj">The source entity.</param>
		public virtual void Copy(GuardianObject obj)
		{

			this.modified = obj.Modified;

		}

		/// <summary>
		/// Create an independant clone of this entity.
		/// </summary>
		/// <returns>An exact copy of this entity.</returns>
		public virtual GuardianObject Clone()
		{

			// Because it has only value-type members (save string, which is immutable), Entity, and most of its derivitives, a shallow copy is
			// identical to a deep copy.
			return this.MemberwiseClone() as GuardianObject;

		}
		
		/// <summary>
		/// Commit any changes to this object to the server.
		/// </summary>
		public abstract void Commit();

		/// <summary>
		/// Commit a list of objects at once. The objects should all be of the same type.
		/// </summary>
		/// <param name="objects">The list to commit.</param>
		/// <returns>The actual bulk size used.</returns>
		public Int32 Commit(IEnumerable<GuardianObject> objects)
		{

			List<GuardianObject> deleteList = new List<GuardianObject>();
			List<GuardianObject> updateList = new List<GuardianObject>();

			foreach (GuardianObject obj in objects)
			{

				if (obj.Deleted)
					deleteList.Add(obj);
				else if (obj.Modified)
					updateList.Add(obj);

			}

			if (deleteList.Count > 0)
				return this.Delete(deleteList);
			else if (updateList.Count > 0)
				return this.Update(updateList);
			else
				throw new ArgumentException("No objects marked as Deleted nor Modified");

		}

		/// <summary>
		/// Delete objects in bulk. The objects should all be of the same type.
		/// </summary>
		/// <param name="objects">The list of objects to delete.</param>
		/// <returns>The actual bulk size used.</returns>
		protected virtual Int32 Delete(List<GuardianObject> objects)
		{

			throw new NotImplementedException();

		}

		/// <summary>
		/// Retrieve a list of GuardianObjects that depend on this one. That is, objects that must be deleted if this one is.
		/// </summary>
		/// <returns>A list of dependent objects.</returns>
		public virtual List<GuardianObject> GetDependents()
		{

			List<GuardianObject> dependencies = new List<GuardianObject>();

			dependencies.Add(this);

			return dependencies;

		}

		/// <summary>
		/// Get the most likely error code for single-record response.
		/// </summary>
		/// <param name="response">The response from an admin web service.</param>
		/// <returns>The error code.</returns>
		protected ErrorCode GetFirstErrorCode(FluidTrade.Guardian.AdminSupportReference.MethodResponseErrorCode response)
		{

			ErrorCode error = response.Result;

			if (response.Errors.Length > 0)
				error = response.Errors[0].ErrorCode;

			return error;

		}

		/// <summary>
		/// Get the most likely error code for single-record response.
		/// </summary>
		/// <param name="response">The response from an admin web service.</param>
		/// <returns>The error code.</returns>
		protected ErrorCode GetFirstErrorCode(FluidTrade.Guardian.TradingSupportReference.MethodResponseErrorCode response)
		{

			ErrorCode error = response.Result;

			if (response.Errors.Length > 0)
				error = response.Errors[0].ErrorCode;

			return error;

		}

		/// <summary>
		/// Raise the property changed event with specified arguments.
		/// </summary>
		/// <param name="propertyChangedEventArgs">The arguments to pass the event subscribers.</param>
		protected void OnPropertyChanged(PropertyChangedEventArgs propertyChangedEventArgs)
		{

			this.modified = true;
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, propertyChangedEventArgs);

		}
		
		/// <summary>
		/// Raise the property changed event with specified arguments.
		/// </summary>
		/// <param name="property">The arguments to pass the event subscribers.</param>
		protected void OnPropertyChanged(String property)
		{

			this.OnPropertyChanged(new PropertyChangedEventArgs(property));

		}

		/// <summary>
		/// Populates a trading support record with the contents of the entity.
		/// </summary>
		/// <param name="record">The empty record to populate.</param>
		protected virtual void PopulateRecord(TradingSupportReference.BaseRecord record)
		{


		}

		/// <summary>
		/// Treat the object as though it were unmodified, while retaining any material modifications.
		/// </summary>
		public void Reset()
		{

			this.Modified = false;
			this.Deleted = false;

		}

		/// <summary>
		/// Convert an ErrorInfo object into an exception and throw it up.
		/// </summary>
		/// <param name="errorInfo">The error info to throw.</param>
		public static void ThrowErrorInfo(TradingSupportReference.ErrorInfo errorInfo)
		{

			switch (errorInfo.ErrorCode)
			{

				case ErrorCode.Success:
					return;
				case ErrorCode.AccessDenied:
					throw new SecurityAccessDeniedException(String.Format("Access Denied: {0}", errorInfo.Message));
				case ErrorCode.ArgumentError:
					throw new FaultException<ArgumentFault>(new ArgumentFault(errorInfo.Message), errorInfo.Message);
				case ErrorCode.Deadlock:
					throw new FaultException<DeadlockFault>(new DeadlockFault(errorInfo.Message), errorInfo.Message);
				case ErrorCode.FieldRequired:
					throw new FaultException<FieldRequiredFault>(new FieldRequiredFault(errorInfo.Message), errorInfo.Message);
				case ErrorCode.RecordExists:
					throw new FaultException<RecordExistsFault>(new RecordExistsFault(), errorInfo.Message);
				case ErrorCode.RecordNotFound:
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Unknown", new Object[0]), errorInfo.Message);
				default:
					throw new Exception(String.Format("Server error: {0}, {1}", errorInfo.ErrorCode, errorInfo.Message));

			}

		}

		/// <summary>
		/// Convert an ErrorInfo object into an exception and throw it up.
		/// </summary>
		/// <param name="errorInfo">The error info to throw.</param>
		public static void ThrowErrorInfo(AdminSupportReference.ErrorInfo errorInfo)
		{

			switch (errorInfo.ErrorCode)
			{

				case ErrorCode.Success:
					return;
				case ErrorCode.AccessDenied:
					throw new SecurityAccessDeniedException(String.Format("Access Denied: {0}", errorInfo.Message));
				case ErrorCode.ArgumentError:
					throw new FaultException<ArgumentFault>(new ArgumentFault(errorInfo.Message), errorInfo.Message);
				case ErrorCode.FieldRequired:
					throw new FaultException<FieldRequiredFault>(new FieldRequiredFault(errorInfo.Message), errorInfo.Message);
				case ErrorCode.Deadlock:
					throw new FaultException<DeadlockFault>(new DeadlockFault(errorInfo.Message), errorInfo.Message);
				case ErrorCode.RecordExists:
					throw new FaultException<RecordExistsFault>(new RecordExistsFault(), errorInfo.Message);
				case ErrorCode.RecordNotFound:
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Unknown", new Object[0]), errorInfo.Message);
				default:
					throw new Exception(String.Format("Server error: {0}, {1}", errorInfo.ErrorCode, errorInfo.Message));

			}

		}

		/// <summary>
		/// Convert an ErrorCode into an exception and throw it up.
		/// </summary>
		/// <param name="errorCode">The error info to throw.</param>
		public static void ThrowErrorInfo(ErrorCode errorCode)
		{

			switch (errorCode)
			{

				case ErrorCode.Success:
					return;
				case ErrorCode.AccessDenied:
					throw new SecurityAccessDeniedException(String.Format("Access Denied"));
				case ErrorCode.ArgumentError:
					throw new FaultException<ArgumentFault>(new ArgumentFault("Argument error"));
				case ErrorCode.Deadlock:
					throw new FaultException<DeadlockFault>(new DeadlockFault("Server deadlock occured"));
				case ErrorCode.FieldRequired:
					throw new FaultException<FieldRequiredFault>(new FieldRequiredFault("Required field is missing"));
				case ErrorCode.RecordExists:
					throw new FaultException<RecordExistsFault>(new RecordExistsFault(), "Record exists");
				case ErrorCode.RecordNotFound:
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Unknown", new Object[0]), "Record not found");
				default:
					throw new Exception(String.Format("Server error: {0}", errorCode));

			}

		}

		/// <summary>
		/// Update this object based on another.
		/// </summary>
		/// <param name="obj">The object to draw updates from.</param>
		public abstract void Update(GuardianObject obj);

		/// <summary>
		/// Update objects in bulk. The objects should all be of the same type.
		/// </summary>
		/// <param name="objects">The list of objects to delete.</param>
		/// <returns>The actual bulk size used.</returns>
		protected virtual Int32 Update(List<GuardianObject> objects)
		{

			throw new NotImplementedException();

		}

	}

}
