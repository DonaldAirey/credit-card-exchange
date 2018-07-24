namespace FluidTrade.Core.Windows
{

    using System.Windows.Controls;

	/// <summary>
	/// Used to pass a value and an UndoAction through a command parameter.
	/// </summary>
	public class UndoObject
	{

		// Public Members
		public System.Object OldValue;
		public System.Object NewValue;
		public System.Windows.Controls.UndoAction UndoAction;

		/// <summary>
		/// Creates an object that describes both the value and the undo action to a command.
		/// </summary>
		/// <param name="value">The value of the object.</param>
		/// <param name="undoAction">The Undo action.</param>
		public UndoObject(object oldValue, object newValue, UndoAction undoAction)
		{

			// Initialize the object
			this.OldValue = oldValue;
			this.NewValue = newValue;
			this.UndoAction = undoAction;

		}

	}

}
