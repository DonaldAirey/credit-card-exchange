namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Windows.Media.Imaging;

	/// <summary>
	/// Summary description for Account.
	/// </summary>
    public class Account : Entity
	{

		static BitmapImage bitmapImage;

		static Account()
		{

			// Indicates that the column can be selected.
			Account.bitmapImage = new BitmapImage(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/SelectColumn.cur", UriKind.Relative));

		}

		/// <summary>
		/// Create a new Account based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the DataModel.</param>
		public Account(EntityRow entityRow) : base(entityRow)
		{

		}

	}

}
