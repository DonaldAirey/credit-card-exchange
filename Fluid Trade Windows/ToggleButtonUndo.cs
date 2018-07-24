namespace FluidTrade.Core.Windows
{

    using System.Windows;
    using System.Windows.Controls.Primitives;
	using System.Windows.Input;

	/// <summary>
	/// Handles the Undo/Redo logic for the TextBox classes of controls.
	/// </summary>
	class ToggleButtonUndo : UndoBase
	{

		// Private Instance Fields
		private System.Boolean isUndoing;

		/// <summary>
		/// Creates an object that manages the Undo/Redo logic that is particular to this control.
		/// </summary>
		/// <param name="undoManager">An object that coordinates all the undo/redo actions in a scope.</param>
		public ToggleButtonUndo(UndoManager undoManager)
			: base(undoManager)
		{

			// Initialzie the object
			this.isUndoing = false;

		}

		/// <summary>
		/// Registers this object to receive notifications of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will register for state change events.</param>
		public override void Register(DependencyObject dependencyObject)
		{

			// The Undo/Redo strategy involves handling global operations that are common to all controls and specific operations
			// that are particular to a family of controls.  This registers handlers for this family of controls that will add the
			// proper actions to the Undo/Redo stacks.
			ToggleButton toggleButton = dependencyObject as ToggleButton;
			toggleButton.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(CheckedHandler), true);
			toggleButton.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(CheckedHandler), true);
			toggleButton.AddHandler(FrameworkElement.PreviewKeyDownEvent, new KeyEventHandler(KeyDownHandler));

		}

		/// <summary>
		/// Removes this object from the chain of events that notify listeners of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will no longer be registered for state change events.</param>
		public override void Unregister(DependencyObject dependencyObject)
		{

			// This will remove this object from the chain of events that keep the control integrated with the Undo/Redo operations
			// in a scope.
			ToggleButton toggleButton = dependencyObject as ToggleButton;
			toggleButton.RemoveHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(CheckedHandler));
			toggleButton.RemoveHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(CheckedHandler));
			toggleButton.RemoveHandler(FrameworkElement.PreviewKeyDownEvent, new KeyEventHandler(KeyDownHandler));

		}

		/// <summary>
		/// Handles the key down event.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void KeyDownHandler(object sender, KeyEventArgs keyEventArgs)
		{

			// This family of controls has no native support for the Undo and Redo operations.  This will generate the commands 
			// when the standard keystrokes are recognized.
			if (keyEventArgs.Source is FrameworkElement)
			{

				FrameworkElement frameworkElement = keyEventArgs.Source as FrameworkElement;

				// Ctrl-Y will generate an Undo command.
				if (((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl))) && (Keyboard.IsKeyDown(Key.Y)))
				{
					keyEventArgs.Handled = true;
					ApplicationCommands.Redo.Execute(null, frameworkElement);
				}

				// Ctrl-Z will generate a Redo command.
				if (((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl))) && (Keyboard.IsKeyDown(Key.Z)))
				{
					keyEventArgs.Handled = true;
					ApplicationCommands.Undo.Execute(null, frameworkElement);
				}

			}

		}

		/// <summary>
		/// Handles changes to the content of the Selector family of controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		void CheckedHandler(object sender, RoutedEventArgs routedEventArgs)
		{

			// The code here will push an action onto the UndoManager stacks that allow it to participate in the Unod/Redo actions
			// for an entire scope of controls.
			if (sender is ToggleButton)
			{

				// Extract the target of the action from the generic arguments.
				ToggleButton toggleButton = sender as ToggleButton;

				// Only user initiated operations will modify the Undo/Redo stacks.
				if (InputHelper.IsUserInitiated(toggleButton, ToggleButton.IsCheckedProperty))
				{
					if (this.isUndoing)
						this.undoManager.RedoStack.Push(new CommandArgumentPair(this.Redo, toggleButton, toggleButton.IsChecked));
					else
						this.undoManager.UndoStack.Push(new CommandArgumentPair(this.Undo, toggleButton, toggleButton.IsChecked));
				}

			}

		}

		/// <summary>
		/// Undoes the most recent undo command on the stack.
		/// </summary>
		/// <param name="dependencyObject">The target of the undo operation.</param>
		public void Undo(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments
			ToggleButton toggleButton = (ToggleButton)genericEventArgs.Arguments[0];
			bool isChecked = (bool)genericEventArgs.Arguments[1];

			try
			{

				// This flag will force the next operation to be pushed onto the 'redo' stack instead of the normal 'undo' stack.  
				// It is critical that the flag is cleared when this task is finished, otherwise the undo/redo logic will be left
				// permanently brain damaged.
				this.isUndoing = true;

				// The state of the button is reversed, thus undoing the operation.
				toggleButton.IsChecked = !isChecked;

			}
			finally
			{

				// This directs the next operation that changes the state of the control to be pushed onto the normal 'undo' stack.
				this.isUndoing = false;

			}


		}

		/// <summary>
		/// Reverses the action of the most recent undo command.
		/// </summary>
		/// <param name="dependencyObject">The target of the redo operation.</param>
		public void Redo(object sender, GenericEventArgs genericEventArgs)
		{

			// This resets the state of the button to what it was before the operation.
			// Extract the specific arguments from the generic arguments
			ToggleButton toggleButton = (ToggleButton)genericEventArgs.Arguments[0];
			bool isChecked = (bool)genericEventArgs.Arguments[1];

			toggleButton.IsChecked = !isChecked;

		}

	}

}
