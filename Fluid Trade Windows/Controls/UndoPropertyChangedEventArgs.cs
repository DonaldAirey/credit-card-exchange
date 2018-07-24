namespace FluidTrade.Core.Windows.Controls
{

    using System.Windows;
	using System.Windows.Controls;

	public delegate void UndoPropertyChangedEventHandler(object sender, UndoPropertyChangedEventArgs undoPropertyChangedEventArgs);

	public class UndoPropertyChangedEventArgs : RoutedEventArgs
	{

		// Public Instance Fields
		public readonly DependencyProperty DependencyProperty;
		public readonly System.Windows.Controls.UndoAction UndoAction;
		public readonly System.Object OldValue;
		public readonly System.Object NewValue;

		public UndoPropertyChangedEventArgs(RoutedEvent routedEvent, UndoAction undoAction,
			DependencyProperty dependencyProperty, object oldValue, object newValue)
			: base(routedEvent)
		{

			// Initialize the object
			this.UndoAction = undoAction;
			this.DependencyProperty = dependencyProperty;
			this.OldValue = oldValue;
			this.NewValue = newValue;

		}

	}

}
