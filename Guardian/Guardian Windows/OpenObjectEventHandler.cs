namespace FluidTrade.Guardian.Windows
{

	using System;

	/// <summary>
	/// Aguments for opening an general object in the Guardian Framework.
	/// </summary>
	public class OpenObjectEventArgs : EventArgs
	{

		// Private Instance Fields
		private Entity entity;
		private Object[] arguments;

		/// <summary>
		/// Constructs the arguments for an event that opens a general object.
		/// </summary>
		/// <param name="entity">The entity that is to be opened.</param>
		/// <param name="arguments">The arguments for opening a viewer for the given entity.</param>
		public OpenObjectEventArgs(Entity entity, params Object[] arguments)
		{

			// Initialize the object
			this.entity = entity;
			this.arguments = arguments;

		}

		/// <summary>
		/// Gets the Entity that is to be opened.
		/// </summary>
		public Entity Entity
		{
			get { return this.entity; }
		}

		/// <summary>
		/// Gets the arguments used to open the entity.
		/// </summary>
		public Object[] Arguments
		{
			get { return this.arguments; }
		}

	}

	/// <summary>
	/// Used by controls to notify the container that an object needs to be opened in a viewer.
	/// </summary>
	public delegate void OpenObjectEventHandler(Object sender, OpenObjectEventArgs openObjectEventArgs);

}
