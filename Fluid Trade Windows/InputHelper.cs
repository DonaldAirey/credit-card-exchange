namespace FluidTrade.Core.Windows
{

	using System;
    using System.Reflection;
	using System.Windows;
    using System.Windows.Data;

    /// <summary>
	/// Manages the Undo/Redo operations for a single control, group of controls or an entire application.
	/// </summary>
	public class InputHelper
	{

		// Private Static Fields
		private static Boolean isPropertyUpdate;
		private static PropertyInfo propertyIsInTransfer;

		/// <summary>
		/// Create the static resources used by this class.
		/// </summary>
		static InputHelper()
		{

			InputHelper.isPropertyUpdate = false;

			// The information regarding the 'IsInTransfer' property is used to get around the 'private' declaration.  This member
			// is critical to the 'Undo' operation as it allows the code to distinguish between user initiated actions and data
			// driven changes.
			InputHelper.propertyIsInTransfer = typeof(BindingExpressionBase).GetProperty("IsInTransfer",
				BindingFlags.NonPublic | BindingFlags.Instance);

		}

		/// <summary>
		/// Gets or sets a value that indicates that a property is currently updating.
		/// </summary>
		public static Boolean IsPropertyUpdate
		{
			get { return InputHelper.isPropertyUpdate; }
			set { InputHelper.isPropertyUpdate = value; }
		}

		/// <summary>
		/// Gets an indicator of whether an action was initiated by the user.
		/// </summary>
		/// <param name="frameworkElement">The name of the element that owns the binding.</param>
		/// <param name="dependencyProperty">The name of the propert to which the element is bound.</param>
		/// <returns>true if the element is currently transferring data through a data binding.</returns>
		public static bool IsUserInitiated(FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
		{

			// A property update should follow the same rules as a bound data update.  That is, it should not be handled the same way as user input.
			if (InputHelper.isPropertyUpdate)
				return false;

			// This helper method is used to determine whether an update to a framework element has been made through a data binding or through a user
			// interaction.  The 'IsInTrasfer' private flag indicates that a Data Binding is responsible for the update.  Since it is private, reflection is
			// needed to extract the value (this can only be done in a trusted environment). Only user interactions should be pushed onto the Undo/Redo stack.
			BindingExpression bindingExpression = frameworkElement.GetBindingExpression(dependencyProperty);
			return bindingExpression == null ? true : !(bool)InputHelper.propertyIsInTransfer.GetValue(bindingExpression, null);

		}

		/// <summary>
		/// Gets an indicator of whether an action was initiated by the user.
		/// </summary>
		/// <param name="frameworkElement">The name of the element that owns the binding.</param>
		/// <param name="dependencyProperty">The name of the propert to which the element is bound.</param>
		/// <returns>true if the element is currently transferring data through a data binding.</returns>
		public static bool IsUserInitiated(FrameworkElement frameworkElement, params DependencyProperty[] dependencyProperties)
		{

			// A property update should follow the same rules as a bound data update.  That is, it should not be handled the same way as user input.
			if (InputHelper.isPropertyUpdate)
				return false;

			// This helper method is used to determine whether an update to a framework element has been made through a data binding or through a user
			// interaction.  The 'IsInTrasfer' private flag indicates that a Data Binding is responsible for the update.  Since it is private, reflection is
			// needed to extract the value (this can only be done in a trusted environment). Only user interactions should be pushed onto the Undo/Redo stack.
			bool isUserInitiated = true;
			foreach (DependencyProperty dependencyProperty in dependencyProperties)
			{
				BindingExpression bindingExpression = frameworkElement.GetBindingExpression(dependencyProperty);
				if (bindingExpression != null && (bool)InputHelper.propertyIsInTransfer.GetValue(bindingExpression, null))
					isUserInitiated = false;
			}
			return isUserInitiated;

		}

	}

}
