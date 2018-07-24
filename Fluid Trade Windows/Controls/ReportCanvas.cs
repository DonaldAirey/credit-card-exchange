namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Media;

    /// <summary>
	/// A canvas upon which the different quadrants of a report are drawn.
	/// </summary>
	public class ReportCanvas : Canvas
	{

		/// <summary>
		/// Identifies the Offset dependency property.
		/// </summary>
		internal static readonly DependencyProperty OffsetProperty;

		// Private Instance Fields
		private Dictionary<Type, Stack<FrameworkElement>> elementCache;
		private Dictionary<ReportCell, FrameworkElement> elementTable;
		private RectangleGeometry rectangleGeometry;
		private TranslateTransform translateTransform;

		/// <summary>
		/// Create the static resources required by this element.
		/// </summary>
		static ReportCanvas()
		{

			// Offset Dependency Property
			ReportCanvas.OffsetProperty = DependencyProperty.Register(
				"Offset",
				typeof(Point),
				typeof(ReportCanvas),
				new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnOffsetChanged)));

		}

		/// <summary>
		/// Creates a canvas upon which a part of the report is drawn.
		/// </summary>
		internal ReportCanvas()
		{

			// This transformation is used to scroll the area in the viewer to different parts of the virtual document.  By
			// modifying the offsets for rendering, it can appear that the entire content of this quadrant has moved.
			this.translateTransform = new TranslateTransform();
			this.RenderTransform = this.translateTransform;

			// This geometry is used to provide a viewport for each of the panels and to prevent any one panel from drawing on the
			// others.  The canvases makes use of a translation transform which causes all drawing operations to be offset from the
			// origin.  One of the operations effected by the translation is the 'ClipToBounds' property of this control making it
			// unsuitable for clipping.  This geometry uses coordinates that aren't effected by the translation of the origin 
			// point.
			this.rectangleGeometry = new RectangleGeometry();
			this.rectangleGeometry.Rect = new Rect();
			this.Clip = this.rectangleGeometry;

			// This canvas is a virtual panel.  The ReportCell represents the abstraction of the visible control and the
			// FrameworkElement is the actualization of that ReportCell.  A ReportCell can be shared between canvases, so there is
			// not one-to-one relationship between a ReportCell and a FrameworkElement.  This table is used to map a ReportCell in
			// this canvas to a FrameworkElement in this canvas.  This list can be thought of as an inventory of the ReportCells 
			// that have been instantiated on this canvas.
			this.elementTable = new Dictionary<ReportCell, FrameworkElement>();

			// This cache is used as a factory for visible elements.  Since creating, adding and removing elements from the
			// ReportCanvas is a very time consuming task, the visual elements are reused whenever possible.  Only when there are
			// no elements to be borrowed from the cache will a FrameworkElement be created from the templates.  This cache
			// maintains a mapping between the CLR types and a list of FrameworkElements suitable for displaying that type.
			this.elementCache = new Dictionary<Type, Stack<FrameworkElement>>();

		}

		/// <summary>
		/// Gets or sets the offset in the virtual document space for this section of the report.
		/// </summary>
		internal Point Offset
		{
			get { return (Point)GetValue(ReportCanvas.OffsetProperty); }
			set { SetValue(ReportCanvas.OffsetProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parent report for this canvas.
		/// </summary>
		internal ReportGrid ReportGrid
		{
			get { return this.Parent as ReportGrid; }
		}

		/// <summary>
		/// Gets the absolute viewport coordinates of this canvas.
		/// </summary>
		public Rect Viewport
		{
			get { return this.rectangleGeometry.Rect; }
		}

		/// <summary>
		/// Brings all the cells in a given column to the top of the Z axis.
		/// </summary>
		/// <param name="frontColumn">The column to be moved in front of the other columns.</param>
		internal void BringToFront(List<ReportColumn> reportColumns)
		{

			// Each visual element that matches the specified report row will be recycled and the data connection to the underlying
			// cell severed.
			foreach (UIElement uiElement in this.Children)
				if (uiElement is FrameworkElement)
				{
					FrameworkElement frameworkElement = uiElement as FrameworkElement;
					ReportCell reportCell = DynamicReport.GetCell(frameworkElement);
					if (reportCell != null)
						Canvas.SetZIndex(frameworkElement, reportColumns.Contains(reportCell.ReportColumn) ? 1 : 0);
				}

		}

		/// <summary>
		/// Brings all the cells in a given column to the top of the Z axis.
		/// </summary>
		/// <param name="frontColumn">The column to be moved in front of the other columns.</param>
		internal void SetZIndex(List<ReportRow> reportRows)
		{

			// Each visual element that matches the specified report row will be recycled and the data connection to the underlying
			// cell severed.
			foreach (UIElement uiElement in this.Children)
			{
				ReportCell reportCell = DynamicReport.GetCell(uiElement);
				if (reportCell != null)
					Canvas.SetZIndex(uiElement, reportCell.ReportRow.ZIndex);
			}

		}

		/// <summary>
		/// Removes all elements from a canvas.
		/// </summary>
		internal void Clear()
		{

			// This will reset the canvas and reclaim all the visual elements associated with virtual cells.
			foreach (UIElement uiElement in this.Children)
				if (uiElement is FrameworkElement)
				{
					FrameworkElement frameworkElement = uiElement as FrameworkElement;
					ReportCell reportCell = DynamicReport.GetCell(frameworkElement);
					if (reportCell != null)
						this.Remove(reportCell);
				}

		}

		/// <summary>
		/// Get the generated framework element for a cell.
		/// </summary>
		/// <param name="cell">The cell.</param>
		/// <returns>The framework element.</returns>
		public FrameworkElement GetFrameworkElement(ReportCell cell)
		{

			return this.elementTable[cell];

		}

		/// <summary>
		/// Overrides the default arrangement of a Canvas.
		/// </summary>
		/// <param name="arrangeSize">The largest possible size for the canvas.</param>
		/// <returns>The actual size used by this canvas.</returns>
		protected override Size MeasureOverride(Size constraint)
		{

			// This calculates the visible area of the virtual canvas.  Note that the 'MeasureOverride' is called before the 
			// 'OnRenderSizeChanged' so the rectangle used for clipping can't be used here because it hasn't been calclated
			// properly yet.
			Rect viewport = new Rect(this.rectangleGeometry.Rect.Location, constraint);

			// IMPORTANT CONCEPT: The FrameworkElements are recycled for performance.  Creating the user interface elements and
			// adding and removing them from the canvas are very time consuming tasks.  The XAML for the reports contains templates
			// used to create FrameworkElements for a given CLR type.  It turns out that any CLR object of a given type can use any
			// FrameworkElement created for that given type.  So a factory of sorts is created for building and reusing the
			// framework elements based on a CLR type.  A screen is drawn by first reclaiming all the elements that are no longer
			// visible.  These elements are not removed from the screen in the first pass.  The secod pass draws the visible area
			// of the report using reclaimed elements when it can, creating new FrameworkElements when it can't.  The important
			// performance kick here is that the elements are not actually removed from the screen during the reclaimation pass and
			// the drawing pass.  All elements are put into this list when during the reclaimation cycle.  If they are still in
			// this list after the drawing pass, then they are not visible and can be removed from the canvas. This is the
			// reclamation cycle.  All the user interface elements that have become invisible are pushed back into the element
			// cache where they can be used again when the viewport is updated.
			foreach (UIElement uiElement in this.Children)
				if (uiElement is FrameworkElement)
				{

					// This element will be examined to see if it needs to be recycled.
					FrameworkElement frameworkElement = uiElement as FrameworkElement;

					// The 'ReportCell' is the virtual information behind the user interface element and is attached to all the 
					// visual elements in the canvas.  It is used here to indicate whether the associated FrameworkElement is
					// invisible.
					ReportCell reportCell = DynamicReport.GetCell(frameworkElement);
					try
					{

						if (reportCell != null)
						{

							// This will test the virtual cell to see if it is invisible.  Note that any cell that touches the right or
							// bottom edge of the viewport is considered to be invisible.  There is no part of these cells that can
							// actually be seen, whereas the left and top edges are visible.
							Rect rect = reportCell.ActualRect;
							if (((viewport.Top > rect.Top || rect.Top >= viewport.Bottom) &&
								(viewport.Top >= rect.Bottom || rect.Bottom > viewport.Bottom)) ||
								((viewport.Left > rect.Left || rect.Left >= viewport.Right) &&
								(viewport.Left >= rect.Right || rect.Right > viewport.Right)))
							{

								// When the framework element has become invisible, it is placed back in the cache based on the CLR
								// type of the object it can display.  It can be recycled for use with similar types when the new
								// viewport is constructed in the second pass.
								Type contentType = reportCell.Content.GetType();
								Stack<FrameworkElement> elementStack;
								if (!this.elementCache.TryGetValue(contentType, out elementStack))
								{
									elementStack = new Stack<FrameworkElement>();
									this.elementCache.Add(contentType, elementStack);
								}
								elementStack.Push(frameworkElement);

								// This disconnects the visual element from the virtual ReportCell and the virtual ReportCell from the 
								// visual element.
								DynamicReport.SetCell(frameworkElement, ReportCell.Empty);
								this.elementTable.Remove(reportCell);

								// The focus scope for this canvas must be cleared when the element that had the focus is recycled.
								// This method 'virtualizes' the handling of the focus.  The virtual focus is saved in the 'IsFocused'
								// property of the ReportCell while the cell is invisible.  When this virtual cell becomes visible
								// again, the keyboard focus will be given back to to user interface element used to instantiate the 
								// virtual cell.
								if (reportCell.IsFocused)
								{
									FocusManager.SetFocusedElement(this.ReportGrid, null);
									if (frameworkElement.IsKeyboardFocusWithin)
										Keyboard.Focus(this.ReportGrid);
								}

								// All these bindings need to be released when a cell is no longer visible.  There seems to be a very 
								// practical limitation to how many binding updates can be done by the operating system.  Failure to
								// release a binding when a user element is no longer visible will quickly result all the resources
								// being used.
								BindingOperations.ClearBinding(frameworkElement, Canvas.LeftProperty);
								BindingOperations.ClearBinding(frameworkElement, Canvas.TopProperty);
								BindingOperations.ClearBinding(frameworkElement, FrameworkElement.WidthProperty);
								BindingOperations.ClearBinding(frameworkElement, FrameworkElement.HeightProperty);
								BindingOperations.ClearBinding(frameworkElement, DynamicReport.IsEvenProperty);
								BindingOperations.ClearBinding(frameworkElement, DynamicReport.IsSelectedProperty);

							}

						}

					}
					catch (Exception exception)
					{

						String message = reportCell.ReportColumn == null ? "ReportColumn is null" : reportCell.ReportRow == null ? "ReportRow is null" : "not sure why";
						EventLog.Error("{0}: {1} ({2})\n{3}", exception.GetType(), exception.Message, message, exception.StackTrace);

					}

				}

			// This pass will construct the visible part of the canvas.  The main idea is to find the virtual cells that appear in
			// the viewport and associate the cell with a visual element.  If a visual element can be found in the cache, it is
			// used, otherwise one is created from a template.
			foreach (ReportRow reportRow in this.ReportGrid.Rows)
			{
				if(reportRow.IsEmpty)
					continue;

				// Only rows that appear in the visible part of the canvas are considered.  Evaluating the rows this way quickly
				// removes the number of cells that need to be checked.
				if (((viewport.Top <= reportRow.Top && reportRow.Top < viewport.Bottom) ||
					(viewport.Top < reportRow.Bottom && reportRow.Bottom <= viewport.Bottom)))
				{

					// At this point we have a row that is part of the visible canvas.  This will see which of the columns are
					// visible.
					foreach (ReportColumn reportColumn in this.ReportGrid.Columns)
					{

						// Note that columns that fall on the right edge are excluded.  Even though they appear to intersect with
						// the viewport, there is no part of these columns that's actally visible.
						if ((viewport.Left <= reportColumn.Left && reportColumn.Left < viewport.Right) ||
							(viewport.Left < reportColumn.Right && reportColumn.Right <= viewport.Right))
						{

							// The cell exists at a virtual location in the document coordinate system.  They are instantiated and
							// added to the canvas when the become visible and are remove from the canvas when not visible.  This
							// is how a very large document can be viewed with a relatively small number of visual elements and
							// bindings.
							ReportCell reportCell = reportRow[reportColumn];

							// If this cell doesn't have a framework element associated with it, then one is either recycled
							// from the cache or created from a template.
							FrameworkElement frameworkElement;
							if (!this.elementTable.TryGetValue(reportCell, out frameworkElement))
							{

								// Each Framework element will work with one and only one CLR type.  The content of the
								// ReportCell determines what kind of FrameworkElement instance is used to display the data in
								// that cell.
								Type contentType = reportCell.Content.GetType();

								// The CLR type of the content is used to find a stack of FrameworkElements that can be used to
								// display that content.
								Stack<FrameworkElement> elementStack;
								if (!this.elementCache.TryGetValue(contentType, out elementStack))
								{
									elementStack = new Stack<FrameworkElement>();
									this.elementCache.Add(contentType, elementStack);
								}

								// If there are no instances of a visual element that can be used to display the content of
								// this ReportCell, then one is generated from the template.  Otherwise, a recycled element is
								// used.  Note that all default focus styles are removed from visual elements on the canvas as
								// there these elements have a custom style applied which functions like the Microsoft Excel
								// focus and selection.
								if (elementStack.Count == 0)
								{
									DataTemplateKey dataTemplateKey = new DataTemplateKey(reportCell.Content.GetType());
									DataTemplate dataTemplate = this.ReportGrid.Resources[dataTemplateKey] as DataTemplate;
									if (dataTemplate != null)
									{
										frameworkElement = dataTemplate.LoadContent() as FrameworkElement;
										frameworkElement.FocusVisualStyle = null;
									}
								}
								else
								{
									frameworkElement = elementStack.Pop();
								}

								// This associates the visual element with the ReportCell that contains the information about 
								// the location of that element in the virtual document coordinate space.  The 'elementTable'
								// contains the reciprocal relation.
								DynamicReport.SetCell(frameworkElement, reportCell);
								this.elementTable.Add(reportCell, frameworkElement);

								// The data context for the XAML elements is the content of the cell, not the ReportCell.  
								// This is done intensionally to hide the implementation details of how elements are positioned
								// on the screen.  It is also done to simplify the programming model for the XAML code.
								frameworkElement.DataContext = reportCell.Content;

								// Bind the element's 'Left' property of the element to the column's position.
								Binding leftBinding = new Binding("ActualLeft");
								leftBinding.Source = reportCell.ReportColumn;
								BindingOperations.SetBinding(frameworkElement, Canvas.LeftProperty, leftBinding);

								// Bind the element's 'Top' property of the element to the row's position.
								Binding topBinding = new Binding("ActualTop");
								topBinding.Source = reportCell.ReportRow;
								BindingOperations.SetBinding(frameworkElement, Canvas.TopProperty, topBinding);

								// Bind the element's 'Width' property of the element to the column's width.
								Binding widthBinding = new Binding("ActualWidth");
								widthBinding.Source = reportCell.ReportColumn;
								BindingOperations.SetBinding(frameworkElement, FrameworkElement.WidthProperty, widthBinding);

								// Bind the element's 'Height' property of the element to the row's height.
								Binding heightBinding = new Binding("ActualHeight");
								heightBinding.Source = reportCell.ReportRow;
								BindingOperations.SetBinding(frameworkElement, FrameworkElement.HeightProperty, heightBinding);

								// Bind the element's 'IsSelected' property to the cell's property.  This property is used to 
								// highlight the selected cells.
								Binding isSelectedBinding = new Binding("IsSelected");
								isSelectedBinding.Source = reportCell;
								BindingOperations.SetBinding(frameworkElement, DynamicReport.IsSelectedProperty, isSelectedBinding);

								// Bind the 'Height' property of the element to the row's 'IsEven' property.  This property is 
								// generally used to shade every other line to make it easier to follow the information on a
								// row.
								Binding evenBinding = new Binding("IsEven");
								evenBinding.Source = reportCell.ReportRow;
								BindingOperations.SetBinding(frameworkElement, DynamicReport.IsEvenProperty, evenBinding);

							}

							// New and recycled elements need to be added to the canvas to become part of the visible report.
							if (frameworkElement.Parent == null)
								this.Children.Add(frameworkElement);

							// The logical (and keyboard) focus is restored to the element that has the virtual focus when the
							// virtual cell is made visible again.  Note that the logical focus is only moved when the canvas
							// has the keyboard focus.  This prevents a scenario where the canvas grabs the input focus away 
							// from another window when the input focus is made visible again.
							if (reportCell.IsFocused)
								if (this.IsKeyboardFocusWithin)
									if (FocusManager.GetFocusedElement(this.ReportGrid) != frameworkElement)
										FocusManager.SetFocusedElement(this.ReportGrid, frameworkElement);

						}

					}

				}

			}

			// At this point, all the elements that can be recycled have been recycled.  The remaining elements on the canvas that
			// are not associated with a virtual report element need to be removed.
			for (int index = 0; index < this.Children.Count; )
			{
				UIElement uiElement = this.Children[index];
				if (DynamicReport.GetCell(uiElement) == ReportCell.Empty)
					this.Children.RemoveAt(index);
				else
					index++;
			}

			// The base class takes care of the hard work of measuring the visible user interface elements on the canvas.
			return base.MeasureOverride(constraint);

		}

		/// <summary>
		/// Handles a change to the Offset property.
		/// </summary>
		/// <param name="dependencyObject">The target object of the property change.</param>
		/// <param name="dependencyPropertyChangedEventArgs">Arguments that describe how the property has changed.</param>
		private static void OnOffsetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the offset for the canvas drawing operations from the event arguments.
			Point offset = (Point)dependencyPropertyChangedEventArgs.NewValue;

			// This will change the transformation matrix so that all drawing operations will be offset from the original location
			// by the given coordinates.  This is how the different quadrants of the report can appear to scroll and not bleed into
			// the other quadrants.
			ReportCanvas reportCanvas = dependencyObject as ReportCanvas;
			reportCanvas.translateTransform.X = offset.X;
			reportCanvas.translateTransform.Y = offset.Y;
			reportCanvas.rectangleGeometry.Rect = new Rect(new Point(-offset.X, -offset.Y), reportCanvas.rectangleGeometry.Rect.Size);

		}

		/// <summary>
		/// Handles a change to the size of the canvas.
		/// </summary>
		/// <param name="sizeInfo">Information about the change to the dimensions of the window.</param>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{

			// This will adjust the clipping rectangle to allow drawing only in the specified window.
			Rect rect = new Rect(this.rectangleGeometry.Rect.Location, sizeInfo.NewSize);
			if (this.rectangleGeometry.Rect != rect)
				this.rectangleGeometry.Rect = rect;

			// Allow the base class to finish changing the screen size.
			base.OnRenderSizeChanged(sizeInfo);

		}

		/// <summary>
		/// Removes a ReportCell from the canvas.
		/// </summary>
		/// <param name="frameworkElement"></param>
		/// <param name="reportCell"></param>
		private void Remove(ReportCell reportCell)
		{

			// The visual element associated with this cell can be found using the mapping table.  Once the element is found
			// it is reclaimed for use later on and the data connection is severed.
			FrameworkElement frameworkElement;
			if (this.elementTable.TryGetValue(reportCell, out frameworkElement))
			{

				// This recycles the element by putting it back in the cache associated with the cell's datatype.  This visual element
				// can be used again when another cell with the same data type needs to be instantiated.
				Type contentType = reportCell.Content.GetType();
				Stack<FrameworkElement> elementStack;
				if (!this.elementCache.TryGetValue(contentType, out elementStack))
				{
					elementStack = new Stack<FrameworkElement>();
					this.elementCache.Add(contentType, elementStack);
				}
				elementStack.Push(frameworkElement);

				// The association between the visual element and the abstract cell is removed when the element is removed from the
				// canvas.
				DynamicReport.SetCell(frameworkElement, ReportCell.Empty);
				this.elementTable.Remove(reportCell);

				// When a virtual cell is removed from the report -- as opposed to simply being made invisible -- the virtual focus
				// is reset.  Resetting the virtual focus is a problem because the FocusScope of the parent focus scope (as well as
				// the current focus scope) maintains a link to the last visual element that had the focus.  When that visual
				// element goes away, there's no good way to tell the parent focus scope to change.  If you sent a FocusScope's
				// FocusedElement to anything other than 'null', the handler tries to set the keyboard focus as well.  This has the
				// undesireable effect of stealing the keyboard focus away from some other window (there's no guarantee that the
				// keyboard focus is on this canvas when removing rows).  Resetting the current and parent focus scopes is the best
				// of some bad options.
				if (reportCell.IsFocused)
				{
					this.ReportGrid.ClearFocusedCell();
					FocusManager.SetFocusedElement(this.ReportGrid, null);
					FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this.ReportGrid.Parent), null);
					if (frameworkElement.IsKeyboardFocusWithin)
						Keyboard.Focus(this.ReportGrid);
				}

				// The visual element can now be safely removed from the window.
				this.Children.Remove(frameworkElement);

			}

		}

		/// <summary>
		/// Removes a ReportColumn from the canvas.
		/// </summary>
		/// <param name="columnDefinition">The description of the column to be removed.</param>
		internal void Remove(ReportColumn reportColumn)
		{

			// Each visual element that matches the specified report column will be recycled and the data connection to the
			// underlying cell severed.
			for (int childIndex = 0; childIndex < this.Children.Count; )
			{
				UIElement uiElement = this.Children[childIndex];
				if (uiElement is FrameworkElement)
				{
					FrameworkElement frameworkElement = uiElement as FrameworkElement;
					ReportCell reportCell = DynamicReport.GetCell(frameworkElement);
					if (reportCell != null && reportCell.ReportColumn == reportColumn)
						this.Remove(reportCell);
					else
						childIndex++;
				}
			}

		}

		/// <summary>
		/// Removes a ReportRow from the canvas.
		/// </summary>
		/// <param name="columnDefinition">The description of the column to be removed.</param>
		internal void Remove(ReportRow reportRow)
		{

			// Each visual element that matches the specified report row will be recycled and the data connection to the underlying
			// cell severed.
			for (int childIndex = 0; childIndex < this.Children.Count; )
			{
				UIElement uiElement = this.Children[childIndex];
				if (uiElement is FrameworkElement)
				{
					FrameworkElement frameworkElement = uiElement as FrameworkElement;
					ReportCell reportCell = DynamicReport.GetCell(frameworkElement);
					if (reportCell != null && reportCell.ReportRow == reportRow)
						this.Remove(reportCell);
					else
						childIndex++;
				}
			}

		}

	}

}
