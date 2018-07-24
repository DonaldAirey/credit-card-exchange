namespace FluidTrade.Core.Windows
{

    using System.Windows;

    /// <summary>
	/// Base class for an Undo/Redo handler for a family of user interface controls.
	/// </summary>
	public interface IUndo
	{

		void Register(DependencyObject dependencyObject);
		void Unregister(DependencyObject dependencyObject);

	}

}
