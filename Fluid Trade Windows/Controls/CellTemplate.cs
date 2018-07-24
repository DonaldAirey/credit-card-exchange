namespace FluidTrade.Core.Windows.Controls
{

    using System.Windows;

	public class CellTemplate : DataTemplate
	{

		// Private Instance Fields
		private System.String columnId;

		public string ColumnId
		{

			get { return this.columnId; }
			set { this.columnId = value; }
		}

	}

}
