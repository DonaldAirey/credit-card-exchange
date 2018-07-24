namespace FluidTrade.Core.Windows
{

    using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;

	/// <summary>
	/// Handles the Undo/Redo logic for the TextBox classes of controls.
	/// </summary>
	class SelectorUndo : UndoBase
	{

		// Private Instance Fields
		private System.Boolean isUndoing;

		/// <summary>
		/// Creates an object that manages the Undo/Redo logic that is particular to this control.
		/// </summary>
		/// <param name="undoManager">An object that coordinates all the undo/redo actions in a scope.</param>
		public SelectorUndo(UndoManager undoManager) : base(undoManager)
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
			Selector selector = dependencyObject as Selector;
			selector.AddHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler(HandleSelectionChanged), true);
			selector.AddHandler(FrameworkElement.PreviewKeyDownEvent, new KeyEventHandler(HandleKeyDown));

		}

		/// <summary>
		/// Removes this object from the chain of events that notify listeners of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will no longer be registered for state change events.</param>
		public override void Unregister(DependencyObject dependencyObject)
		{

			// This will remove this object from the chain of events that keep the control integrated with the Undo/Redo operations
			// in a scope.
			Selector selector = dependencyObject as Selector;
			selector.RemoveHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler(HandleSelectionChanged));
			selector.RemoveHandler(FrameworkElement.PreviewKeyDownEvent, new KeyEventHandler(HandleKeyDown));

		}

		/// <summary>
		/// Handles the key down event.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void HandleKeyDown(object sender, KeyEventArgs keyEventArgs)
		{

			// This family of controls has no native support for the Undo and Redo operations.  This will generate the commands 
			// when the standard keystrokes are recognized.
			if (keyEventArgs.Source is FrameworkElement)
			{

				FrameworkElement frameworkElement = keyEventArgs.Source as FrameworkElement;

				// Ctrl-Y will generate an Undo command.
				if (((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl))) && (Keyboard.IsKeyDown(Key.Y)))
					ApplicationCommands.Redo.Execute(null, frameworkElement);

				// Ctrl-Z will generate a Redo command.
				if (((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl))) && (Keyboard.IsKeyDown(Key.Z)))
					ApplicationCommands.Undo.Execute(null, frameworkElement);

			}

		}

		/// <summary>
		/// Handles changes to the content of the Selector family of controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void HandleSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
		{

			// The code here will push an action onto the UndoManager stacks that allow it to participate in the Unod/Redo actions
			// for an entire scope of controls.
			if (sender is Selector)
			{

				// Extract the target of the action from the generic arguments.
				Selector selector = sender as Selector;

				// Only user initiated operations will modify the Undo/Redo stacks.
				if (InputHelper.IsUserInitiated(selector, Selector.SelectedValueProperty))
				{
					if (this.isUndoing)
						this.undoManager.RedoStack.Push(new CommandArgumentPair(this.Redo, selector, selectionChangedEventArgs));
					else
						this.undoManager.UndoStack.Push(new CommandArgumentPair(this.Undo, selector, selectionChangedEventArgs));
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
			Selector selector = (Selector)genericEventArgs.Arguments[0];
			SelectionChangedEventArgs selectionChangedEventArgs = (SelectionChangedEventArgs)genericEventArgs.Arguments[1];

			try
			{

				// This flag will force the next operation to be pushed onto the 'redo' stack instead of the normal 'undo' stack.  
				// It is critical that the flag is cleared when this task is finished, otherwise the undo/redo logic will be left
				// permanently brain damaged.
				this.isUndoing = true;

				// This will select the item that was unselected, thus undoing the operation.
				selector.SelectedItem = selectionChangedEventArgs.RemovedItems.Count == 0 ? null :
					selectionChangedEventArgs.RemovedItems[0];

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

			// Extract the specific arguments from the generic arguments
			Selector selector = (Selector)genericEventArgs.Arguments[0];
			SelectionChangedEventArgs selectionChangedEventArgs = (SelectionChangedEventArgs)genericEventArgs.Arguments[1];

			// Reverses the action of the most recent undo command.
			selector.SelectedItem = selectionChangedEventArgs.RemovedItems.Count == 0 ? null :
				selectionChangedEventArgs.RemovedItems[0];

		}

	}

}
