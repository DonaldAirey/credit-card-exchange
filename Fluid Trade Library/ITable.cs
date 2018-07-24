using System;
namespace FluidTrade.Core
{

	/// <summary>
	/// Abstract interface to common features of a table.
	/// </summary>
	public interface ITable
	{

		/// <summary>
		/// Gets the collection of columns in this table.
		/// </summary>
		global::System.Data.DataColumnCollection Columns
		{
			get;
		}

		/// <summary>
		/// A delegate used to filter the results passed back to a client from the server.
		/// </summary>
		FilterRowDelegate FilterRowHandler
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the delegate used to determine the container of a given object.
		/// </summary>
		GetContainerDelegate GetContainerHandler
		{
			get;
			set;
		}

		/// <summary>
		/// The absolute index of the table within the collection of tables in a DataSet.
		/// </summary>
		int Ordinal
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the primary key of the table.
		/// </summary>
		System.Data.DataColumn[] PrimaryKey
		{
			get;
		}
	}


	public abstract partial class DataTableBase<T> : System.Data.TypedTableBase<T>, ITable where T : System.Data.DataRow
	{
		protected FilterRowDelegate filterRowHandler;
		protected GetContainerDelegate getContainerHandler;
		private global::FluidTrade.Core.DataIndexCollection dataIndices;
		private int ordinal;


		protected DataTableBase()
		{
			this.dataIndices = new global::FluidTrade.Core.DataIndexCollection();
		}

		/// <summary>
		/// Gets or sets a delegate used to filter the results passed back to a client from the server.
		/// </summary>
		public FilterRowDelegate FilterRowHandler
		{
			get
			{
				return this.filterRowHandler;
			}
			set
			{
				this.filterRowHandler = value;
			}
		}

		/// <summary>
		/// Gets or sets a delegate used to obtain the owner of an object.
		/// </summary>
		public GetContainerDelegate GetContainerHandler
		{
			get
			{
				return this.getContainerHandler;
			}
			set
			{
				this.getContainerHandler = value;
			}
		}

		/// <summary>
		/// Gets the absolute index of the AccessControl table in the DataSet.
		/// </summary>
		[global::System.ComponentModel.BrowsableAttribute(false)]
		public int Ordinal
		{
			get
			{
				return this.ordinal;
			}
			set
			{
				this.ordinal = value;
			}
		}


		/// <summary>
		/// Gets a collection of FluidTrade.Core.DataIndices on a table.
		/// </summary>
		public global::FluidTrade.Core.DataIndexCollection Indices
		{
			get
			{
				return this.dataIndices;
			}
		}

		/// <summary>
		/// Raises the AccessControlRowChanged event.
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected override void OnRowChanged(global::System.Data.DataRowChangeEventArgs e)
		{
			base.OnRowChanged(e);
		}

		/// <summary>
		/// method that is called at the end of the OnRowChanged().
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected virtual void AfterOnRowChanged(global::System.Data.DataRowChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the AccessControlRowChanging event.
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected override void OnRowChanging(global::System.Data.DataRowChangeEventArgs e)
		{
			base.OnRowChanging(e);
			this.AfterOnRowChanging(e);
		}

		/// <summary>
		/// method that is called at the end of the OnRowChanging().
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected virtual void AfterOnRowChanging(global::System.Data.DataRowChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the AccessControlRowDeleted event.
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected override void OnRowDeleted(global::System.Data.DataRowChangeEventArgs e)
		{
			base.OnRowDeleted(e);
			this.AfterOnRowDeleted(e);
		}

		/// <summary>
		/// method that is called at the end of the OnRowDeleted().
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected virtual void AfterOnRowDeleted(global::System.Data.DataRowChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the AccessControlRowDeleting event.
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected override void OnRowDeleting(global::System.Data.DataRowChangeEventArgs e)
		{
			base.OnRowDeleting(e);
			this.AfterOnRowDeleting(e);
		}

		/// <summary>
		/// method that is called at the end of the OnRowDeleting().
		/// </summary>
		/// <param name="e">Provides data for the AccessControlRow changing and deleting events.</param>
		protected virtual void AfterOnRowDeleting(global::System.Data.DataRowChangeEventArgs e)
		{
		}
	}

	/// <summary>
	/// Handles filters that remove data from the stream returned to the client.
	/// </summary>
	/// <param name="dataModelTransaction">The current transaction.</param>
	/// <param name="filterContext">A context for the current filter operation.</param>
	/// <param name="containerContext">A container item that is to be filtered.</param>
	/// <returns>true indicates the row can be passed to the client, false indicates it should not.</returns>
	public delegate bool FilterRowDelegate(IDataModelTransaction dataModelTransaction, object filterContext, object containerContext);


	/// <summary>
	/// Handles filters that determine the container of a given object.
	/// </summary>
	/// <param name="iRow">The record for which the container record is desired.</param>
	/// <returns>The record that contains the given record, null if a container is not applicable.</returns>
	public delegate object GetContainerDelegate(global::FluidTrade.Core.IRow iRow);
	

}
