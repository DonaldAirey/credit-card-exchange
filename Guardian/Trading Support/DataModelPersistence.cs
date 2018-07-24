namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using FluidTrade.Guardian.Records;
	using FluidTrade.Core;

	/// <summary>
	/// Base class for Persitance.
	/// </summary>
	public abstract class DataModelPersistence<T> where T : BaseRecord
	{

		/// <summary>
		/// Protected constructor, since this is an abstract class.
		/// </summary>
		protected DataModelPersistence()
		{
		}
		
		/// <summary>
		/// Creates a new object in the data store. Returns a new id of the object created.
		/// </summary>		
		/// <param name="record"></param>
		public abstract Guid Create(T record);
		/// <summary>
		/// Retrieve object
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public abstract T Get(Guid id);
		/// <summary>
		/// Updates or creates an object.
		/// </summary>
		/// <param name="record"></param>
		public abstract void Update(T record);
		/// <summary>
		/// Updates or creates array of object.
		/// </summary>
		/// <returns>True for success</returns>	
		public void Update(IEnumerable<T> records)
		{
			foreach (T record in records)
			{
				Update(record);
			}
		}
		/// <summary>
		/// Deletes an object.
		/// </summary>		
		/// <returns></returns>	
		public abstract ErrorCode Delete(T record);
	}
}
