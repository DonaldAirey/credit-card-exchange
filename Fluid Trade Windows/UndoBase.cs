namespace FluidTrade.Core.Windows
{

    using System.Windows;

    /// <summary>
	/// Base class for an Undo/Redo handler for a family of user interface controls.
	/// </summary>
	public abstract class UndoBase : IUndo
	{

		// Private Instance Fields
		public readonly UndoManager undoManager;

		public UndoBase(UndoManager undoManager)
		{

			// Initialize the object.
			this.undoManager = undoManager;

		}

		public abstract void Register(DependencyObject dependencyObject);

		public abstract void Unregister(DependencyObject dependencyObject);

	}

}
