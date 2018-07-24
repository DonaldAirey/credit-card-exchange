namespace FluidTrade.Core.Windows
{

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

	/// <summary>
	/// Provides support for drag and drop operations.
	/// </summary>
	public class DragDropHelper
	{

		// Private Static Fields
		private static DraggedAdorner draggedAdorner;
		private static DataFormat dataFormat;
		private static Point initialPosition;
		private static Boolean isDragging;
		private static Object sourceData;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DragDropHelper.IsDragSource attached property.
		/// </summary>
		public static readonly DependencyProperty IsDragSourceProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DragDropHelper.IsDragTarget attached property.
		/// </summary>
		public static readonly DependencyProperty IsDropTargetProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DragDropHelper.IsDragOver attached property.
		/// </summary>
		public static readonly DependencyProperty IsDragOverProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DragDropHelper.DragDropTemplate attached property.
		/// </summary>
		public static readonly DependencyProperty DragDropTemplateProperty;

		/// <summary>
		/// Creates the static resources required by the DragDropHelper class.
		/// </summary>
		static DragDropHelper()
		{

			// The common format used in the drag and drop operations.
			DragDropHelper.dataFormat = DataFormats.GetDataFormat("DragDropItemsControl");

			// IsDragSource
			DragDropHelper.IsDragSourceProperty = DependencyProperty.RegisterAttached(
				"IsDragSource",
				typeof(Boolean),
				typeof(DragDropHelper),
				new UIPropertyMetadata(IsDragSourceChanged));

			// IsDragTarget
			DragDropHelper.IsDropTargetProperty = DependencyProperty.RegisterAttached(
				"IsDropTarget",
				typeof(Boolean),
				typeof(DragDropHelper),
				new UIPropertyMetadata(IsDropTargetChanged));

			// IsDragOver
			DragDropHelper.IsDragOverProperty = DependencyProperty.RegisterAttached(
				"IsDragOver",
				typeof(Boolean),
				typeof(DragDropHelper));

			// DragDropTemplate
			DragDropHelper.DragDropTemplateProperty = DependencyProperty.RegisterAttached(
				"DragDropTemplate",
				typeof(DataTemplate),
				typeof(DragDropHelper));

		}

		/// <summary>
		/// Gets the data format used in the drag and drop operations.
		/// </summary>
		public static DataFormat DataFormat
		{
			get { return DragDropHelper.dataFormat; }
		}

		/// <summary>
		/// Finds an ancestor of the given type.
		/// </summary>
		/// <param name="type">The desired type of the ancestor.</param>
		/// <param name="visual">The starting visual for the search.</param>
		/// <returns>The closest ancestor of the given Visual having the requested type.</returns>
		public static FrameworkElement FindAncestor(Type type, Visual visual)
		{
			
			// Walk up the visual tree hierarchy until a visual is found of the given type.
			while (visual != null && !type.IsInstanceOfType(visual))
				visual = VisualTreeHelper.GetParent(visual) as Visual;

			// At this point the visual tree has been searched and there is no ancestor of the given type.
			return visual as FrameworkElement;

		}

		/// <summary>
		/// Gets the value of the IsDragSource attached property from a given UIElement. 
		/// </summary>
		/// <param name="uiElement">The element from which to read the property value.</param>
		/// <returns>The value of the IsDragSource attached property.</returns>
		public static Boolean GetIsDragSource(UIElement uiElement)
		{
			return (Boolean)uiElement.GetValue(IsDragSourceProperty);
		}

		/// <summary>
		/// Gets the value of the DragDropTemplate attached property from a given UIElement. 
		/// </summary>
		/// <param name="uiElement">The element from which to read the property value.</param>
		/// <returns>The value of the DragDropTemplate attached property.</returns>
		public static DataTemplate GetDragDropTemplate(UIElement uiElement)
		{
			return uiElement.GetValue(DragDropTemplateProperty) as DataTemplate;
		}

		/// <summary>
		/// Gets the value of the IsDropTarget attached property from a given UIElement. 
		/// </summary>
		/// <param name="uiElement">The element from which to read the property value.</param>
		/// <returns>The value of the DragDropTemplate attached property.</returns>
		public static Boolean GetIsDropTarget(UIElement uiElement)
		{
			return (Boolean)uiElement.GetValue(IsDropTargetProperty);
		}

		/// <summary>
		/// Gets the value of the IsDragOver attached property from a given UIElement. 
		/// </summary>
		/// <param name="uiElement">The element from which to read the property value.</param>
		/// <returns>The value of the DragDropTemplate attached property.</returns>
		public static Boolean GetIsDragOver(UIElement uiElement)
		{
			return (Boolean)uiElement.GetValue(IsDragOverProperty);
		}

		/// <summary>
		/// Gets the container visual based on the visual that got the hit-test.
		/// </summary>
		/// <param name="itemsControl">The control that owns the hit tested visual.</param>
		/// <param name="childVisual">The hit-tested visual.</param>
		/// <returns>The items container that owns the visual that was hit.</returns>
		private static FrameworkElement GetItemContainer(ItemsControl itemsControl, Visual hitVisual)
		{

			// Items controls can have many visual elements and many child ItemsControls.  Finding out which of these visual
			// elements is the container for the visual that was hit by the mouse must be done recursively to find the closest
			// container.
			foreach (Object item in itemsControl.Items)
			{

				// Get the container of the current item in the control.  First it will be examined recusively to see if any of the
				// children own the hit visual, then the item itself will be tested to see if it is an ancestor of the hit visual.
				// The recursion must be done first to insure the closest relative in a hierarchical structure such as a TreeView.
				DependencyObject dependencyObject = itemsControl.ItemContainerGenerator.ContainerFromItem(item);

				// If this control has children, such as in a TreeView, then recursively examine each of the children to see if
				// they are immediate ancestors of the hit visual.  This step must be done first to get the closest ancestor to the
				// visual that was hit.
				if (dependencyObject is ItemsControl)
				{
					FrameworkElement itemContainer = GetItemContainer(dependencyObject as ItemsControl, hitVisual);
					if (itemContainer != null)
						return itemContainer;
				}

				// Test to see if the current window is an ancestor of the hit visual.  If it was, we've found the item container
				// that holds the hit visual.
				if (dependencyObject is Visual)
				{
					Visual parentVisual = dependencyObject as Visual;
					if (parentVisual.IsAncestorOf(hitVisual))
						return parentVisual as FrameworkElement;
				}

			}

			// At this point the ItemsControl does not contain the hit visual.
			return null;

		}

		/// <summary>
		/// Handles a change to the MarkThree.Windows.DragDropHelper.IsDragSource property.
		/// </summary>
		/// <param name="dependencyObject">The object to which the property belongs.</param>
		/// <param name="dependencyPropertyChangedEventArgs">Event arguments describing the change.</param>
		private static void IsDragSourceChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed values from the generic arguments.
			UIElement uiElement = dependencyObject as UIElement;
			Boolean isDragSource = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// The object must be derrived from a UIElement to handle the drag and drop events.
			if (uiElement != null)
			{

				// The drag and drop operations begin with mouse gestures.  This adds or removes handlers to decipher the mouse
				// gestures and set up a generic drag and drop operation under the right conditions.
				if (isDragSource)
				{
					uiElement.PreviewMouseLeftButtonDown += DragDropHelper.OnDragSourcePreviewMouseLeftButtonDown;
					uiElement.PreviewMouseLeftButtonUp += DragDropHelper.OnDragSourcePreviewMouseLeftButtonUp;
					uiElement.PreviewMouseMove += DragDropHelper.OnDragSourcePreviewMouseMove;
				}
				else
				{
					uiElement.PreviewMouseLeftButtonDown -= DragDropHelper.OnDragSourcePreviewMouseLeftButtonDown;
					uiElement.PreviewMouseLeftButtonUp -= DragDropHelper.OnDragSourcePreviewMouseLeftButtonUp;
					uiElement.PreviewMouseMove -= DragDropHelper.OnDragSourcePreviewMouseMove;
				}

			}

		}

		/// <summary>
		/// Handles a change to the MarkThree.Windows.DragDropHelper.IsDragSource property.
		/// </summary>
		/// <param name="dependencyObject">The object to which the property belongs.</param>
		/// <param name="dependencyPropertyChangedEventArgs">Event arguments describing the change.</param>
		private static void IsDropTargetChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed values from the generic arguments.
			UIElement uiElement = dependencyObject as UIElement;
			Boolean isDragSource = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// The object must be derrived from a UIElement to handle the drag and drop events.
			if (uiElement != null)
			{

				// This will install generic handlers for the dragged adorner which relieves the drop targets from having to manage
				// the cursor rendering.
				if (isDragSource)
				{
					uiElement.AllowDrop = true;
					uiElement.PreviewDragEnter += DragDropHelper.OnDropTargetPreviewDragEnter;
					uiElement.PreviewDragOver += DragDropHelper.OnDropTargetPreviewDragOver;
				}
				else
				{
					uiElement.AllowDrop = false;
					uiElement.PreviewDragEnter -= DragDropHelper.OnDropTargetPreviewDragEnter;
					uiElement.PreviewDragOver -= DragDropHelper.OnDropTargetPreviewDragOver;
				}

			}

		}

		/// <summary>
		/// Handles the mouse left button down routed event.
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="mouseButtonEventArgs">The event data.</param>
		private static void OnDragSourcePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{

			// Extract the strongly typed values from the generic arguments.
			UIElement uiElement = sender as UIElement;
			Visual visual = mouseButtonEventArgs.OriginalSource as Visual;

			// The initial position is used to calculate the relative movement of the mouse.
			DragDropHelper.initialPosition = mouseButtonEventArgs.GetPosition(uiElement);

			// When an ItemsControl is the source for the drag operation the dragged object can be determined from the hit visual.
			if (uiElement is ItemsControl)
			{

				// An ItemsControl can have many levels of visuals and descendant ItemControls.  The item returned in the mouse
				// arguments only indicates which visual was hit.  It doesn't say which item container object was hit which is the
				// most important bit of information in a drag-and-drop operation.  This will recusively walk the ItemsControl
				// looking for the container that is the closest ancestor to the hit visual.
				ItemsControl itemsControl = sender as ItemsControl;
				FrameworkElement sourceItemContainer = DragDropHelper.GetItemContainer(itemsControl, visual);
				if (sourceItemContainer != null)
					DragDropHelper.sourceData = sourceItemContainer.DataContext;

			}

		}

		/// <summary>
		/// Handles the mouse move routed event.
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="mouseButtonEventArgs">The event data.</param>
		private static void OnDragSourcePreviewMouseMove(object sender, MouseEventArgs mouseEventArgs)
		{

			// Extract the strongly typed values from the generic arguments.
			UIElement uiElement = sender as UIElement;

			// This attempts to decode the mouse gestures into an invocation of the drag and drop operation.  To begin, the 
			// drag-and-drop operation is only initiated when the left mouse button is pressed and dragged a distance large enough
			// to insure a deliberate action on the part of the user.  Furthermore, hitting an input element, such as the expander
			// buttons on TreeView controls, can cause a recursive call to this method.  Normally, the 'DoDragDrop' method would
			// capture all mouse activity, but that is not the case when the mouse generates an input command.
			if (mouseEventArgs.LeftButton == MouseButtonState.Pressed && !DragDropHelper.isDragging)
			{

				// The drag and drop operation is invoked only when an object has been selected.
				if (DragDropHelper.sourceData != null)
				{

					// Mouse movements are ignored if the mouse hasn't moved far enough from starting point.  This prevents 
					// accidental drag-and-drop operations by insuring that the user has made a conscious effort to move the mouse
					// with the left mouse button down.
					Point currentPosition = mouseEventArgs.GetPosition(uiElement);
					Point startPosition = DragDropHelper.initialPosition;
					if (Math.Abs(currentPosition.X - startPosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
						Math.Abs(currentPosition.Y - startPosition.Y) >= SystemParameters.MinimumVerticalDragDistance)
					{

						// The top level window is hooked into the drag and drop events so it can provide movement commands for the
						// dragged adorner even when the adorner is not over the source or target elements.
						Window topWindow = (Window)DragDropHelper.FindAncestor(typeof(Window), uiElement);
						Boolean previousAllowDrop = topWindow.AllowDrop;

						// This visual layer sits on top of all other visual layers and provides rendinging for the visual element
						// used to display the dragged object like a cursor.  A cursor-like visual element is created in this layer
						// to track the movement of the mouse and to give feedback about what kind of element is to be dropped.
						AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(uiElement);

						try
						{

							// This is the data item that will be dragged about.  Each of the drop targets can examine this object 
							// to see what operations are acceptable.
							DataObject data = new DataObject(DragDropHelper.DataFormat.Name, DragDropHelper.sourceData);

							// This creates a visual element that provides a picture of the object to be dragged.  It uses the
							// adorner layer for rending and follows the cursor around the screen.  Note that the opacity of the
							// adorner is fixed at a value that was reverse engineered from Microsoft Windows File Explorer.
							DragDropHelper.draggedAdorner = new DraggedAdorner(
								uiElement,
								DragDropHelper.sourceData,
								GetDragDropTemplate(uiElement));
							adornerLayer.Add(DragDropHelper.draggedAdorner);

							// Adding these events to the top window allows the dragged adorner to come up even when it is not over the
							// source or target window.
							topWindow.AllowDrop = true;
							topWindow.DragEnter += OnTopWindowDragEnter;
							topWindow.DragOver += OnTopWindowDragOver;

							// This does all the hard work of dragging and dropping.  Note that the flag prevents recursive calls 
							// to the DoDragDrop when the source involves some visual element that can generate input commands
							// through the mouse, such as expanding or collapsing a node in a TreeView.
							DragDropHelper.isDragging = true;
							DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);

						}
						finally
						{

							// This flag is designed to prevent recursive calls to the drag and drop operation.
							DragDropHelper.isDragging = false;

							// At this point the drag and drop operation is complete and the top window can be restored to its 
							// previous state.
							topWindow.AllowDrop = previousAllowDrop;
							topWindow.DragEnter -= OnTopWindowDragEnter;
							topWindow.DragOver -= OnTopWindowDragOver;

							// This disposes of the cursor-like object that displays the dragged object as the mouse moves.
							adornerLayer.Remove(DragDropHelper.draggedAdorner);
							DragDropHelper.draggedAdorner = null;

						}

						// This ends the operation started when the left mouse button was pressed.
						DragDropHelper.sourceData = null;

					}

				}

			}
			else
			{

				// This will cancel any drag-and-drop operation that was started but not completed.  Normally the end of the
				// operation or the left mouse button being released will clear the status, but in rare cases it's possible to move
				// the mouse off the object before the minimum distance is recognized and then leave the window.  This insures that
				// the drag-and-drop operation is in the initial state when that happens.
				if (DragDropHelper.sourceData != null)
					DragDropHelper.sourceData = null;

			}

		}

		/// <summary>
		/// Handles the mouse left button up routed event.
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="mouseButtonEventArgs">The event data.</param>
		private static void OnDragSourcePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{

			// Pressing the left mouse button will start a drag-and-drop operation but if the mouse isn't moved far enough, the
			// operation isn't handled by the MouseMove event handler and must be terminated here.
			if (DragDropHelper.sourceData != null)
				DragDropHelper.sourceData = null;

		}

		/// <summary>
		/// Handles the tunneling of the drag-and-drop enter event.
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="dragEventArgs">The event data.</param>
		private static void OnDropTargetPreviewDragEnter(object sender, DragEventArgs dragEventArgs)
		{

			try
			{

				// Position the dragged adorner under the current mouse location.
				if (DragDropHelper.draggedAdorner != null)
					DragDropHelper.draggedAdorner.Offset = dragEventArgs.GetPosition(DragDropHelper.draggedAdorner.AdornedElement);

			}
			catch (Exception exception)
			{

				EventLog.Information("{0} in OnDropTargetPreviewDragEnter: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Handles the tunneling of the drag-and-drop enter event.
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="dragEventArgs">The event data.</param>
		private static void OnDropTargetPreviewDragOver(object sender, DragEventArgs dragEventArgs)
		{

			try
			{

				// Position the dragged adorner under the current mouse location.
				DragDropHelper.draggedAdorner.Offset = dragEventArgs.GetPosition(DragDropHelper.draggedAdorner.AdornedElement);

			}
			catch (Exception exception)
			{

				EventLog.Information("{0} in OnDropTargetPreviewDragEnter: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Handles the tunneling of the drag-and-drop enter event for the top level window.
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="dragEventArgs">The event data.</param>
		private static void OnTopWindowDragEnter(object sender, DragEventArgs dragEventArgs)
		{

			try
			{

				// Position the dragged adorner under the current mouse location.
				DragDropHelper.draggedAdorner.Offset = dragEventArgs.GetPosition(DragDropHelper.draggedAdorner.AdornedElement);

				// There is no target in the main window which can accept the dragged object.
				dragEventArgs.Effects = DragDropEffects.None;
				dragEventArgs.Handled = true;

			}
			catch (Exception exception)
			{

				EventLog.Information("{0} in OnDropTargetPreviewDragEnter: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Handles the tunneling of the drag-and-drop drag over event for the top level window.
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="dragEventArgs">The event data.</param>
		private static void OnTopWindowDragOver(object sender, DragEventArgs dragEventArgs)
		{

			try
			{

				// Position the dragged adorner under the current mouse location.
				DragDropHelper.draggedAdorner.Offset = dragEventArgs.GetPosition(DragDropHelper.draggedAdorner.AdornedElement);

				// There is no target in the main window which can accept the dragged object.
				dragEventArgs.Effects = DragDropEffects.None;
				dragEventArgs.Handled = true;

			}
			catch (Exception exception)
			{

				EventLog.Information("{0} in OnDropTargetPreviewDragEnter: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Sets the value of the IsDragSource attached property. 
		/// </summary>
		/// <param name="dependencyObject">The identifier of the dependency property to set.</param>
		/// <param name="value">The new local value.</param>
		public static void SetIsDragSource(DependencyObject dependencyObject, Boolean value)
		{
			dependencyObject.SetValue(DragDropHelper.IsDragSourceProperty, value);
		}

		/// <summary>
		/// Sets the value of the IsDropTarget attached property. 
		/// </summary>
		/// <param name="dependencyObject">The identifier of the dependency property to set.</param>
		/// <param name="value">The new local value.</param>
		public static void SetIsDropTarget(DependencyObject dependencyObject, Boolean value)
		{
			dependencyObject.SetValue(DragDropHelper.IsDropTargetProperty, value);
		}

		/// <summary>
		/// Sets the value of the IsDragOver attached property. 
		/// </summary>
		/// <param name="dependencyObject">The identifier of the dependency property to set.</param>
		/// <param name="value">The new local value.</param>
		public static void SetIsDragOver(DependencyObject dependencyObject, Boolean value)
		{
			dependencyObject.SetValue(DragDropHelper.IsDragOverProperty, value);
		}

		/// <summary>
		/// Sets the value of the DragDropTemplate attached property. 
		/// </summary>
		/// <param name="dependencyObject">The identifier of the dependency property to set.</param>
		/// <param name="value">The new local value.</param>
		public static void SetDragDropTemplate(DependencyObject dependencyObject, DataTemplate value)
		{
			dependencyObject.SetValue(DragDropHelper.DragDropTemplateProperty, value);
		}

	}

}
