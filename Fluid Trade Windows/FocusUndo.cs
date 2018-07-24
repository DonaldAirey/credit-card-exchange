namespace FluidTrade.Core.Windows
{

    using System.Windows;

    /// <summary>
	/// Handles the Undo/Redo logic for the TextBox classes of controls.
	/// </summary>
	class FocusUndo : UndoBase
	{

		// Private Instance Fields
		private System.Boolean isUndoing;

		/// <summary>
		/// Creates an object that manages the Undo/Redo logic that is particular to this control.
		/// </summary>
		/// <param name="undoManager">An object that coordinates all the undo/redo actions in a scope.</param>
		public FocusUndo(UndoManager undoManager)
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
			if (dependencyObject is FrameworkElement)
			{
				FrameworkElement frameworkElement = dependencyObject as FrameworkElement;
				frameworkElement.AddHandler(FrameworkElement.LostFocusEvent, new RoutedEventHandler(FocusHandler), true);
			}

			// The Undo/Redo strategy involves handling global operations that are common to all controls and specific operations
			// that are particular to a family of controls.  This registers handlers for this family of controls that will add the 
			// proper actions to the Undo/Redo stacks.
			if (dependencyObject is FrameworkContentElement)
			{
				FrameworkContentElement frameworkContentElement = dependencyObject as FrameworkContentElement;
				frameworkContentElement.AddHandler(FrameworkContentElement.LostFocusEvent, new RoutedEventHandler(FocusHandler), true);
			}

		}

		/// <summary>
		/// Removes this object from the chain of events that notify listeners of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will no longer be registered for state change events.</param>
		public override void Unregister(DependencyObject dependencyObject)
		{

			// This will remove the handler for the mouse focus events.
			if (dependencyObject is FrameworkElement)
			{
				FrameworkElement frameworkElement = dependencyObject as FrameworkElement;
				frameworkElement.RemoveHandler(FrameworkElement.LostFocusEvent, new RoutedEventHandler(FocusHandler));
			}

			// This will remove the handler for the mouse focus events.
			if (dependencyObject is FrameworkContentElement)
			{
				FrameworkContentElement frameworkContentElement = dependencyObject as FrameworkContentElement;
				frameworkContentElement.RemoveHandler(FrameworkContentElement.LostFocusEvent, new RoutedEventHandler(FocusHandler));
			}

		}

		/// <summary>
		/// Handles a change to the focus.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="executedRoutedEventArgs">The event arguments.</param>
		internal void FocusHandler(object sender, RoutedEventArgs routedEventArgs)
		{

			// When the focus is changed, a command is pushed onto the 'undo' stack that will remember the movement and be able to
			// undo it when requested.  Every time an undo operation is executed, an equal and opposite 'redo' operation is pushed 
			// onto the redo stack.  This allows use to move forward and backward through the users actions.
			if (routedEventArgs.Source is FrameworkElement)
			{
				FrameworkElement frameworkElement = routedEventArgs.Source as FrameworkElement;
				if (this.isUndoing)
					this.undoManager.RedoStack.Push(new CommandArgumentPair(this.Redo, frameworkElement));
				else
					this.undoManager.UndoStack.Push(new CommandArgumentPair(this.Undo, frameworkElement));
			}

		}

		/// <summary>
		/// Undoes the movement of the focus from one control to another.
		/// </summary>
		/// <param name="dependencyObject">The target control of the operation to move the focus.</param>
		public void Undo(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments
			FrameworkElement frameworkElement = (FrameworkElement)genericEventArgs.Arguments[0];

			// The 'isUndoing' flag will force an operation on the stack that will 'undo' the 'undo' operation.  That is, it will 
			// move the cursor back to where it was before this 'undo' operation was performed.
			this.isUndoing = true;
			frameworkElement.Focus();
			this.isUndoing = false;

		}

		/// <summary>
		/// Redoes the movement of the focus from one control to another.
		/// </summary>
		/// <param name="dependencyObject">The target control of the operation to move the focus.</param>
		public void Redo(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments
			FrameworkElement frameworkElement = (FrameworkElement)genericEventArgs.Arguments[0];

			// This simply sets the focus to where it was before the 'undo' command was invoked.
			frameworkElement.Focus();

		}

	}

}
