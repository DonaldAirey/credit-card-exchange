namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.ComponentModel;
	using System.Threading;
	using System.Windows.Threading;

	/// <summary>
	/// Interaction logic for PaymentScheduleControl.xaml
	/// </summary>
	public partial class PaymentScheduleControl : UserControl
	{

		/// <summary>
		/// Command indicating a change of inherits status.
		/// </summary>
		public static RoutedUICommand InheritCommand = new RoutedUICommand("Inherits commission schedule from parent", "InheritCommand", typeof(PaymentScheduleControl)); 

		/// <summary>
		/// Indicates the CommissionSchedule dependency property.
		/// </summary>
		public static readonly DependencyProperty EntityProperty = DependencyProperty.Register(
			"Entity",
			typeof(Blotter),
			typeof(PaymentScheduleControl),
			new PropertyMetadata(PaymentScheduleControl.OnEntityChanged));
		/// <summary>
		/// Indicates the Inherits dependency property.
		/// </summary>
		public static readonly DependencyProperty InheritsProperty = DependencyProperty.Register(
			"Inherits",
			typeof(Boolean),
			typeof(PaymentScheduleControl),
			new PropertyMetadata(PaymentScheduleControl.OnInheritsChanged));

		private CommissionSchedule inheritedSchedule;
	
		/// <summary>
		/// Build a new PaymentSchedule control.
		/// </summary>
		public PaymentScheduleControl()
		{

			InitializeComponent();
	
		}

		/// <summary>
		/// The payment schedule that the control is editing.
		/// </summary>
		public Blotter Entity
		{

			get { return this.GetValue(PaymentScheduleControl.EntityProperty) as Blotter; }
			set { this.SetValue(PaymentScheduleControl.EntityProperty, value); }

		}

		/// <summary>
		/// The payment schedule that the control is editing.
		/// </summary>
		public Boolean Inherits
		{

			get { return (Boolean)this.GetValue(PaymentScheduleControl.InheritsProperty); }
			set { this.SetValue(PaymentScheduleControl.InheritsProperty, value); }

		}

		/// <summary>
		/// Determine whether it is possible for Entity to inherit its commission schedule and enable/disable the inherits checkbox accordingly.
		/// </summary>
		/// <param name="entity"></param>
		private void CanInherit(Entity entity)
		{

			this.Dispatcher.BeginInvoke(new WaitCallback(delegate(object data)
				{
					Boolean canInherit = (Boolean)data;

					if (canInherit)
					{

						this.inherit.Visibility = Visibility.Visible;

					}
					else
					{

						this.inherit.Visibility = Visibility.Collapsed;
						this.Inherits = false;

					}
				}),
				this.HasParent(entity.EntityId, entity.TypeId));
		}

		/// <summary>
		/// Determine whether we can add tranches to the schedule we're viewing.
		/// </summary>
		/// <param name="sender">The originator of the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanNew(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = !this.Inherits;
			eventArgs.Handled = true;

		}

		/// <summary>
		/// Check for a parent parent of same entity type as this entity.
		/// </summary>
		/// <param name="entityId">The id of the entity we're interested in.</param>
		/// <param name="typeId">The id of the type we're interested in.</param>
		/// <returns>True if a parent was found - false otherwise.</returns>
		private Boolean HasParent(Guid entityId, Guid typeId)
		{

			lock (DataModel.SyncRoot)
				foreach (EntityTreeRow entityTreeRow in DataModel.Entity.EntityKey.Find(entityId).GetEntityTreeRowsByFK_Entity_EntityTree_ChildId())
					if (entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId.TypeId == typeId)
						return true;

			return false;

		}

		/// <summary>
		/// Handle the CommissionSchedule changing.
		/// </summary>
		/// <param name="sender">The PaymentScheduleControl that owns the property.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnEntityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			PaymentScheduleControl schedule = sender as PaymentScheduleControl;

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => schedule.CanInherit(data as Entity), schedule.Entity.Clone());

			if (schedule.Entity.CommissionSchedule != null)
			{

				schedule.Inherits = false;
				schedule.content.DataContext = schedule.Entity.CommissionSchedule;

			}
			else
			{

				schedule.Inherits = true;

			}

		}
		
		/// <summary>
		/// Handle the Inherits changing.
		/// </summary>
		/// <param name="sender">The PaymentScheduleControl that owns the property.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnInheritsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			PaymentScheduleControl schedule = sender as PaymentScheduleControl;

			if (schedule.Inherits)
			{

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
					{
						lock (DataModel.SyncRoot)
							schedule.Dispatcher.BeginInvoke(
								new WaitCallback(cs => schedule.SetInheritedSchedule(cs as CommissionSchedule)),
								DispatcherPriority.Normal,
								new object[] { Blotter.GetEffectiveCommissionSchedule((Guid)data) });
					},
					schedule.Entity.EntityId);

			}
			else
			{

				if (schedule.Entity.CommissionSchedule == null)
					schedule.Entity.CommissionSchedule = new CommissionSchedule();
				schedule.content.DataContext = schedule.Entity.CommissionSchedule;

			}

		}

		/// <summary>
		/// Handle an tranche's list view item getting focus. Select the row.
		/// </summary>
		/// <param name="sender">The ListViewItem in question.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnTrancheGotKeyboardFocus(object sender, RoutedEventArgs eventArgs)
		{

				(sender as ListViewItem).IsSelected = true;

		}

		/// <summary>
		/// Handle the new command. Add a new tranche to the commission schedule.
		/// </summary>
		/// <param name="sender">The New Tranche button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnNew(object sender, RoutedEventArgs eventArgs)
		{

			if (this.Entity.CommissionSchedule != null)
				this.Entity.CommissionSchedule.CommissionTranches.Add(new CommissionTranche(this.Entity.CommissionSchedule.CommissionScheduleId));

		}

		/// <summary>
		/// Handle the delete command. Remove a tranche from the commission schedule.
		/// </summary>
		/// <param name="sender">The Delete Tranche button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnDelete(object sender, RoutedEventArgs eventArgs)
		{

			if (this.Entity.CommissionSchedule != null)
				this.Entity.CommissionSchedule.CommissionTranches.Remove(this.tranches.SelectedItem as CommissionTranche);

		}

		/// <summary>
		/// Set the schedule in-use to one inherited from up the tree.
		/// </summary>
		/// <param name="commissionSchedule"></param>
		private void SetInheritedSchedule(CommissionSchedule commissionSchedule)
		{

			this.inheritedSchedule = commissionSchedule;
			this.content.DataContext = this.inheritedSchedule;

		}

	}

}
