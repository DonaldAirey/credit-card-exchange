namespace FluidTrade.Guardian.Windows
{

	using System.Collections.Generic;
	using System.Data;

	/// <summary>
	/// A complete (and self-updating) list of the payment method types available in the data model.
	/// </summary>
	public class PaymentMethodTypeList : DataBoundList<PaymentMethodType>
	{

		/// <summary>
		/// The global SideList
		/// </summary>
		public static readonly PaymentMethodTypeList Default = new PaymentMethodTypeList();

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static PaymentMethodTypeList.CompareItems comparer = new PaymentMethodTypeList.CompareItems();

		/// <summary>
		/// Create a new payment-method-type list.
		/// </summary>
		private PaymentMethodTypeList()
		{

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<PaymentMethodType> Comparer
		{
			get { return PaymentMethodTypeList.comparer; }
		}

		/// <summary>
		/// The PaymentMethodType table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.PaymentMethodType; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<PaymentMethodType>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(PaymentMethodType left, PaymentMethodType right)
			{

				return left.Name.CompareTo(right.Name);

			}

		}

		/// <summary>
		/// Create a new item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the payment-method-type table.</param>
		/// <returns>A new PaymentMethodType object.</returns>
		protected override PaymentMethodType New(DataRow dataRow)
		{

			PaymentMethodType item;
			PaymentMethodTypeRow baseRow = dataRow as PaymentMethodTypeRow;

			item = new PaymentMethodType(baseRow);

			return item;

		}

		/// <summary>
		/// Update a PaymentMethodType object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(PaymentMethodType old, PaymentMethodType update)
		{

			old.Update(update);

		}

	}

}
