namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows;
	using FluidTrade.Core;

	/// <summary>
	/// A list box that nicely displays rights holders.
	/// </summary>
	public class RightsHolderListBox : ListBox
	{

		/// <summary>
		/// Indicates the MultiTenant dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey MultiTenantProperty =
			DependencyProperty.RegisterReadOnly("MultiTenant", typeof(Boolean), typeof(RightsHolderListBox), new PropertyMetadata(false));

		private Boolean multiTenant = false;

		static RightsHolderListBox()
		{

			RightsHolderListBox.DefaultStyleKeyProperty.OverrideMetadata(
				typeof(RightsHolderListBox),
				new FrameworkPropertyMetadata(typeof(RightsHolderListBox)));

		}

		/// <summary>
		/// Create a new RightsHolderListBox.
		/// </summary>
		public RightsHolderListBox()
		{

			ThreadPoolHelper.QueueUserWorkItem(data => this.DetermineMultiTenant());

		}

		/// <summary>
		/// True if this user has access to multiple tenants.
		/// </summary>
		public Boolean MultiTenant
		{
			get { return this.multiTenant; }
			private set
			{

				this.multiTenant = value;
				this.SetValue(RightsHolderListBox.MultiTenantProperty, value);

			}
		}

		/// <summary>
		/// Determine whether we can see multiple tenants, and use the fully qualified name for rights holders if we can.
		/// </summary>
		private void DetermineMultiTenant()
		{

			lock (DataModel.SyncRoot)
				if (DataModel.Tenant.Count > 1)
					this.Dispatcher.BeginInvoke(new Action(() => this.MultiTenant = true));

		}

	}

}
