namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// Objects implementing this interface can be "moved" from from one place to another.
	/// </summary>
	public interface IMovableObject
	{

		/// <summary>
		/// Move this object from one place to another.
		/// </summary>
		/// <param name="newParent">The new location of the object.</param>
		/// <param name="errors">The list of errors and at what index.</param>
		void Move(GuardianObject newParent, List<ErrorInfo> errors);

		/// <summary>
		/// Move several objects (of the same type) from one place to another.
		/// </summary>
		/// <param name="objects">The list of objects to move.</param>
		/// <param name="newParent">The new location of the object.</param>
		/// <param name="errors">The list of errors and at what index.</param>
		void Move(List<IMovableObject> objects, GuardianObject newParent, List<ErrorInfo> errors);

	}

}
