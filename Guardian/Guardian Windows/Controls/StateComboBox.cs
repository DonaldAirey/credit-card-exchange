namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Windows;

	/// <summary>
	/// A combo box containing States (and territories) of the United States.
	/// </summary>
	public class StateComboBox : PersistentComboBox
	{

		private static ProvinceList stateList;

		static StateComboBox()
		{

			StateComboBox.SelectedValuePathProperty.OverrideMetadata(typeof(StateComboBox), new FrameworkPropertyMetadata("ProvinceId"));
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data =>
				StateComboBox.InitializeProvince());

		}

		/// <summary>
		/// Create a new state combo box.
		/// </summary>
		public StateComboBox()
		{

			this.DisplayMemberPath = "Abbreviation";
			this.ItemsSource = StateComboBox.stateList;

		}

		/// <summary>
		/// Initialize the province list.
		/// </summary>
		private static void InitializeProvince()
		{

			lock (DataModel.SyncRoot)
			{

				CountryRow countryRow = DataModel.Country.CountryKeyExternalId0.Find("US");

				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(delegate(object data)
					{
						StateComboBox.stateList = new ProvinceList((Guid)data);
					}), countryRow.CountryId);

			}

		}

		/// <summary>
		/// Dispose of the province list.
		/// </summary>
		private void Dispose()
		{

			if (this.ItemsSource != null)
				(this.ItemsSource as ProvinceList).Dispose();

		}

	}

}
