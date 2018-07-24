namespace FluidTrade.Core.Windows
{

    using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;

	/// <summary>
	/// Handles the Undo/Redo logic for the TextBox classes of controls.
	/// </summary>
	public abstract class TextBoxBaseUndo : UndoBase
	{

		/// <summary>
		/// Creates an object that manages the Undo/Redo logic that is particular to this control.
		/// </summary>
		/// <param name="undoManager">An object that coordinates all the undo/redo actions in a scope.</param>
		public TextBoxBaseUndo(UndoManager undoManager) : base(undoManager) { }

		/// <summary>
		/// Registers this object to receive notifications of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will register for state change events.</param>
		public override void Register(DependencyObject dependencyObject)
		{

			// The Undo/Redo strategy involves handling global operations that are common to all controls and specific operations
			// that are particular to a family of controls.  This registers handlers for this family of controls that will add the
			// proper actions to the Undo/Redo stacks.
			TextBoxBase textBox = dependencyObject as TextBoxBase;
			textBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextChangedHandler), true);

		}

		/// <summary>
		/// Removes this object from the chain of events that notify listeners of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will no longer be registered for state change events.</param>
		public override void Unregister(DependencyObject dependencyObject)
		{

			// This will remove this object from the chain of events that keep the control integrated with the Undo/Redo operations
			// in a scope.
			TextBoxBase textBox = dependencyObject as TextBoxBase;
			textBox.RemoveHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextChangedHandler));

		}

		/// <summary>
		/// Handles changes to the content of the TextBoxBase family of controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		public abstract void TextChangedHandler(object sender, TextChangedEventArgs e);

		/// <summary>
		/// Undoes the most recent undo command on the stack.
		/// </summary>
		/// <param name="dependencyObject">The target of the undo operation.</param>
		public void Undo(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments
			TextBoxBase textBoxBase = (TextBoxBase)genericEventArgs.Arguments[0];

			// Undoes the most recent undo command.  In other words, undoes the most recent undo unit on the stack.
			textBoxBase.Undo();

		}

		/// <summary>
		/// Reverses the action of the most recent undo command.
		/// </summary>
		/// <param name="dependencyObject">The target of the redo operation.</param>
		public void Redo(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments
			TextBoxBase textBoxBase = (TextBoxBase)genericEventArgs.Arguments[0];

			// Reverses the action of the most recent undo command.
			textBoxBase.Redo();

		}

	}

}
