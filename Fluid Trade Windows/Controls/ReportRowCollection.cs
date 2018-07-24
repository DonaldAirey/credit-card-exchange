namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections;
	using System.Collections.Generic;
    using System.Reflection;
	using System.Windows;
	using System.Windows.Media.Animation;

    /// <summary>
	/// A collection of rows that appear in a report.
	/// </summary>
	public class ReportRowCollection : Animatable
	{

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.ReportGrid.ExtentHeight dependency property.
		/// </summary>
		public static readonly DependencyProperty HeightProperty;

		// Private Instance Fields
		private List<ReportRow> animatedRows;
		private Dictionary<Object, ReportRow> dictionary;
		private Double height;
		private Boolean isMeasurementRequired;
		private Int32 ordinal;
		private ReportGrid reportGrid;
		private Storyline storyline;
		private StorylineQueue storylineQueue;
		private Rect viewport;

		/// <summary>
		/// Initialize the static resources required by the ReportRowCollection.
		/// </summary>
		static ReportRowCollection()
		{

			// Height Property
			ReportRowCollection.HeightProperty = DependencyProperty.Register(
				"Height",
				typeof(Double),
				typeof(ReportRowCollection),
				new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHeightChanged)));

		}

		/// <summary>
		/// Create a collection of rows that appear in a report.
		/// </summary>
		internal ReportRowCollection(ReportGrid reportGrid)
		{

			// Many of the internal properties of the parent ReportGrid are needed to manage the collection of rows.
			this.reportGrid = reportGrid;

			// The rows are held in a dictionary indexed by a unique key assigned to each row.  Typically this key is the DataRow of the primary record used by
			// the report.  The key is used to know whether an incoming record is new or has updated data for an existing row.
			this.dictionary = new Dictionary<Object, ReportRow>();

			// The animation of many rows is only effective when done as part of a ParallelTimeline and ParallelTimelines do not look good when running at the
			// same time.  For this reason, every new update to the content of the report is animated and no other animations, at least on the lines, is allowed to
			// run until the previous one has completed.  This queue makes insures that each animation that updates the state of the rows in the report are
			// accomplished serially.
			this.storylineQueue = new StorylineQueue();

		}

		/// <summary>
		/// Gets the height of the collection of report rows.
		/// </summary>
		public Double Height
		{
			get { return (Double)this.GetValue(ReportRowCollection.HeightProperty); }
		}

		/// <summary>
		/// Removes all the rows definitions from the collection.
		/// </summary>
		public void Clear()
		{

			// Clear the object.
			this.dictionary.Clear();
			this.storylineQueue.Clear();

		}

		/// <summary>
		/// Create a freezable copy of the object.
		/// </summary>
		/// <returns>A new, freezable copy of the object.</returns>
		protected override Freezable CreateInstanceCore()
		{
			return new ReportRowCollection(this.reportGrid);
		}

		/// <summary>
		/// Finds the row at the given coordinate in the virtual space of the report.
		/// </summary>
		/// <param name="y">A position in the vritual space of the report.</param>
		/// <returns>The row definition at the specified coordinates or null if no row is defined.</returns>
		public ReportRow FindRowAt(Double y)
		{

			// Iterate through the collection until a row is found at the specified coordinates.
			foreach (ReportRow reportRow in this.dictionary.Values)
				if (reportRow.Top <= y && y < reportRow.Top + reportRow.Height)
					return reportRow;

			// This indicates that there was no row definition at the specified location.
			return null;

		}

		/// <summary>
		/// Sets the content that is displayed in the report.
		/// </summary>
		/// <param name="iContent">The data that is displayed in this report.</param>
		private Storyline FirstSegmentSetContent(Object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation from the current command on the queue.
			IContent iContent = (IContent)genericEventArgs.Arguments[0];
			Duration duration = (Duration)genericEventArgs.Arguments[1];

			// These fields keeps track of all the things that can change while recursing into the report hierarchy.
			this.animatedRows = new List<ReportRow>();
			this.height = 0.0;
			this.isMeasurementRequired = false;
			this.ordinal = 0;
			this.viewport = this.reportGrid.reportCanvases[3].Viewport;

			// Updating a report can generate many animation effects: moving rows from one place to another, resizing them, etc.  All these effects must be
			// driven of the same clock or the effect gets jumpy and hard to follow.  The Storyline is nearly identical to a ParallelTimeline except that it
			// has some Attached properties to specify the target object and property and some methods to synchronize them all.
			this.storyline = new Storyline();

			// This will find the root template for the report and start the recursive process that will expand the report hierarchy according to the structure
			// found in the XAML source.
			if (this.reportGrid.RowTemplates.Count > 0)
				RecursivelyUpdateRow(iContent, this.reportGrid.RowTemplates[0]);

			// When animating a large group of objects it is important to bring the objects to the front of the ZIndex that move the farthest.  The ones that
			// move the least can appear behind the others.  This allows the eye to follow the objects with the greatest amount of movement.
			foreach (ReportCanvas reportCanvas in this.reportGrid.reportCanvases)
				reportCanvas.SetZIndex(this.animatedRows);

			// When the recursion is finished and the report has been completely expanded, the running height variable that was used for the location of each
			// row can now be used as the height of the entire collection.  Note that an internal method is used to set the value.  Anyone can get the value of
			// the virtual height, but only this object can set it.
			this.SetValue(ReportRowCollection.HeightProperty, this.height);

			// A unique key is used to identify each row so it can persist from one update to the next.  Purging rows that are no longer part of the content is
			// done by marking all of the persistent rows with an 'IsObsolete' flag. If a row persists from one update to the next, then this flag will be
			// reset as the data is transferred from the new content. If this flag is still set after the recursive update, it means that there is no
			// corresponding row in the new content and it should be removed from the report.  Note that after the test is made for obsolescence, the flag
			// is reset for the next pass.  This saves doing an extra iteration through all the rows on the next full content set.
			List<ReportRow> deletedRows = new List<ReportRow>();
			foreach (ReportRow reportRow in this)
				if (reportRow.IsObsolete)
				{
					if ((this.viewport.Top <= reportRow.Top && reportRow.Top < this.viewport.Bottom) ||
						(this.viewport.Top <= reportRow.Top + reportRow.Height && reportRow.Top + reportRow.Height < this.viewport.Bottom))
						this.isMeasurementRequired = true;
					deletedRows.Add(reportRow);
				}
				else
					reportRow.IsObsolete = true;

			// The dictionary that holds the mappings between the content keys and the report rows can't be purged in the loop abovce because that would modify
			// the collection, so they need to be put into a temporary buffer and then they can be removed.
			foreach (ReportRow reportRow in deletedRows)
			{
				this.dictionary.Remove(reportRow.IContent.Key);
				foreach (ReportCanvas reportCanvas in this.reportGrid.reportCanvases)
					reportCanvas.Remove(reportRow);
			}

			// If any rows are added or removed from the viewport then the "MeasureOverride" will need to remove the obsolete graphic elements and add the new
			// ones using the virtual space layout computed here.
			if (this.isMeasurementRequired)
				this.reportGrid.InvalidateMeasure();

			// The next TimelineSegment in the queue will be executed when this Timeline is finished.  In this way, actions that
			// take a finite amount of time can be executed in an orderly sequence.
			return this.storyline;

		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection of MarkThree.Windows.Controls.RowDefinitions.
		/// </summary>
		/// <returns>An enumerator that iterates through the collection of row definitions.</returns>
		public Dictionary<Object, ReportRow>.ValueCollection.Enumerator GetEnumerator()
		{

			// This is used to enumerate through the rows in the report in no particular order.
			return this.dictionary.Values.GetEnumerator();

		}

		/// <summary>
		/// Sets the content of the collection.
		/// </summary>
		/// <param name="iContent">The content of the report.</param>
		public void Load(IContent iContent)
		{

			// This will queue up the new content for the report.  The content of the report will be updated as fast as this queue can be emptied.  If the
			// animation takes a long time, then this queue can fill up.  It will be the responsibility of an external worker thread to reduce the animation
			// time in order to keep the queue from overflowing.
			if (iContent != null)
				this.storylineQueue.Enqueue(FirstSegmentSetContent, iContent, this.reportGrid.Duration);

		}

		/// <summary>
		/// Handles a change to the Height property.
		/// </summary>
		/// <param name="dependencyObject">The object posessing the property that has changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">Information about the property change.</param>
		private static void OnHeightChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// This will broadcast a notification that the height of the entire collection has changed.
			if (dependencyObject is ReportRowCollection)
			{
				ReportRowCollection reportRowCollection = dependencyObject as ReportRowCollection;
				reportRowCollection.reportGrid.UpdateExtent();
			}

		}

		/// <summary>
		/// Recursively add a column to the collection of rows.
		/// </summary>
		/// <param name="iContent">The content of the current row in the recursion.</param>
		/// <param name="rowTemplate">The template describing the layout of data in the current row.</param>
		internal void RecursivelyAddColumns(IContent iContent, RowTemplate rowTemplate)
		{

			// This section of code will create a virtual cell where data can be found and coordinates in the document space assigned.  The existing row is
			// pulled apart and the properties are examined.  If a template has been associated with the property's type, then an association can be made
			// between the row and the column where this property should be displayed.
			ReportRow reportRow = null;
			if (this.dictionary.TryGetValue(iContent.Key, out reportRow))
				foreach (PropertyInfo propertyInfo in iContent.GetType().GetProperties())
				{
					IContent childContent = propertyInfo.GetValue(iContent, null) as IContent;
					if (childContent != null)
					{
						string columnId;
						if (this.reportGrid.reportFieldCollection.ColumnIdMap.TryGetValue(childContent.GetType(), out columnId))
						{
							ReportColumn reportColumn;
							if (this.reportGrid.reportColumnCollection.TryGetValue(columnId, out reportColumn))
								if (!reportRow.ContainsKey(reportColumn))
									reportRow.Add(reportColumn, new ReportCell(childContent, reportColumn, reportRow));
						}
					}
				}

			// This will recurse into the report's hierarchy.  Each row template can specify a child row and the property that is 
			// used to create that child row.  These, in turn, can be recursed into ad infinitum.
			foreach (RowTemplate childRowDefinition in rowTemplate.Children)
			{
				PropertyInfo propertyInfo = iContent.GetType().GetProperty(childRowDefinition.Path);
				Object property = propertyInfo.GetGetMethod().Invoke(iContent, null);
				if (property is IEnumerable)
				{
					IEnumerable iEnumerable = property as IEnumerable;
					foreach (IContent childContent in iEnumerable)
						RecursivelyAddColumns(childContent, childRowDefinition);
				}
			}

		}

		/// <summary>
		/// Constructs a collection of report rows based on the data content and the templates that describes the row.
		/// </summary>
		/// <param name="iContent">The content to be displayed in the row.</param>
		/// <param name="rowTemplate">The template that describes how the data is presented.</param>
		private void RecursivelyUpdateRow(IContent iContent, RowTemplate rowTemplate)
		{

			// Each IContent object has a unique key used to associate it with a report row.  If a given report row has already has been mapped to the
			// IContent's key, then the contents of that row will be updated.  Otherwise, a new row is created and associated with the IContent.
			ReportRow reportRow = null;
			if (iContent.Key != null && this.dictionary.TryGetValue(iContent.Key, out reportRow))
			{

				// Any change to the location or the height of the row will be animated so long as the current or target location is visible.  There is no
				// sense in animating the movement of invisible elements of the report.
				if (reportRow.Top != this.height || reportRow.Height != rowTemplate.Height)
				{

					// At this point it has been determined that the row has moved.  An animation sequence will be created for those rows that appear in or
					// disappear from the viewport.
					Boolean isRowVisible = (this.viewport.Top <= reportRow.Top && reportRow.Top < this.viewport.Bottom) ||
						(this.viewport.Top <= reportRow.Top + reportRow.Height && reportRow.Top + reportRow.Height < this.viewport.Bottom);
					Boolean willRowBeVisible = (this.viewport.Top <= this.height && this.height < this.viewport.Bottom) ||
						(this.viewport.Top <= this.height + rowTemplate.Height && this.height + rowTemplate.Height < this.viewport.Bottom);

					// This creates a list of rows who's ZIndex must be set for the animation sequence.
					if (isRowVisible || willRowBeVisible)
					{

						// At this point the report row either is currently visible or will be visible and has been moved or resized.  This means that the
						// visible part of the virtual document should be measured.  Items that have become visible will need to be instantiated and items that
						// are no longer visible need to be recycled.
						this.isMeasurementRequired = true;

						// The ZIndex of this row is set such that the rows that move the greatest distance will appear to float above the ones that only move a little.  This scheme
						// makes the movement appear more natural.
						reportRow.ZIndex = Math.Abs(reportRow.Ordinal - this.ordinal);

						// When all the report rows have been examined and the Z order of the visible rows set, this list is used to set the ZIndex of all the 
						// graphical elements associated with this row.
						this.animatedRows.Add(reportRow);

					}

					// The order of the row in the report is used for setting graphical cues such as shading and Z order.
					reportRow.Ordinal = this.ordinal;

					// If the location occupied by this row has changed then an animation sequence is constructed to move a row from one place to another.  If
					// the row isn't visible or is not going to be visible, then just the location is modified.
					if (reportRow.Top != this.height)
					{

						// This will move the row to its proper position in the virtual document.
						reportRow.Top = this.height;

						// The starting location is clipped by the visible part of the report.  This prevents a row that is moved to a far distant place from
						// moving too fast to be seen on the screen.
						// HACK - This should be repaired when time permits.
//						Double startLocation = (reportRow.ActualTop < viewport.Top) ? viewport.Top - reportRow.Height :
//							(reportRow.ActualTop > viewport.Bottom) ? viewport.Bottom : reportRow.ActualTop;
						Double startLocation = reportRow.ActualTop;

						// The ending location is also clipped by the visible part of the report for the same reason.  Without this code, rows appear to 
						// disappear instantly from the screen instead of moving smoothly to the nearest edge.
						// HACK - This should be repaired when time permits.
//						Double endLocation = (reportRow.Top + reportRow.Height < viewport.Top) ? viewport.Top - reportRow.Height :
//							(reportRow.Top > viewport.Bottom) ? viewport.Bottom : reportRow.Top;
						Double endLocation = reportRow.Top;

						// This animates the movement of the row from its current position to the new position.  If the row is no longer visible then any
						// animation that was associated with that row will be terminated.
						if (isRowVisible || willRowBeVisible)
						{
							DoubleAnimation topAnimation = new DoubleAnimation();
							Storyline.SetTargetObject(topAnimation, reportRow);
							Storyline.SetTargetProperty(topAnimation, ReportRow.ActualTopProperty);
							topAnimation.From = startLocation;
							topAnimation.To = endLocation;
							topAnimation.Duration = this.reportGrid.Duration;
							this.storyline.Children.Add(topAnimation);
						}
						else
						{
							if (reportRow.HasAnimatedProperties)
								reportRow.ApplyAnimationClock(ReportRow.ActualTopProperty, null);
							reportRow.ActualTop = reportRow.Top;
						}

					}

					// If the height occupied by this row has changed then an animation sequence is constructed to resize the row.  If the row isn't visible or
					// is not going to be visible, then the property is modified without an animation sequence. This saves the processor from doing work when
					// an element isn't visible.
					if (reportRow.Height != rowTemplate.Height)
					{

						// This sets the height of the row.
						reportRow.Height = rowTemplate.Height;

						// This will animate the change in the row's height from the current height to the new target height.
						if (isRowVisible || willRowBeVisible)
						{
							DoubleAnimation heightAnimation = new DoubleAnimation();
							Storyline.SetTargetObject(heightAnimation, reportRow);
							Storyline.SetTargetProperty(heightAnimation, ReportRow.ActualHeightProperty);
							heightAnimation.From = reportRow.ActualHeight;
							heightAnimation.To = reportRow.Height;
							heightAnimation.Duration = this.reportGrid.Duration;
							this.storyline.Children.Add(heightAnimation);
						}
						else
						{
							if (reportRow.HasAnimatedProperties)
								reportRow.ApplyAnimationClock(ReportRow.ActualHeightProperty, null);
							reportRow.ActualHeight = reportRow.Height;
						}

					}

				}

				// A virtual method is used to copy the current content over the existing content.  The event handlers will take 
				// care of propagating the content and properties of the data to the screen elements.
				reportRow.IContent.Copy(iContent);

				// This indicates that the row is still in use and shouldn't be purged.
				reportRow.IsObsolete = false;

			}
			else
			{

				// This is where new rows are created to hold the given content and added to the report.
				reportRow = new ReportRow();
				reportRow.IContent = iContent;
				this.dictionary.Add(iContent.Key, reportRow);

				// The order of the row in the report drives visual cues such as alternate line shading and Z order properties.
				reportRow.Ordinal = this.ordinal;

				// The target location and height are initialized here without animation.
				reportRow.Top = this.height;
				reportRow.Height = rowTemplate.Height;

				// The actual top and height used when rendering the rows.
				reportRow.ActualTop = reportRow.Top;
				reportRow.ActualHeight = reportRow.Height;

				// Rows that are added to the visible portion of the display will trigger the 'MeasureOverride' which will instantiate the new report elements.
				if ((this.viewport.Top <= reportRow.Top && reportRow.Top < this.viewport.Bottom) ||
					(this.viewport.Top <= reportRow.Top + reportRow.Height && reportRow.Top + reportRow.Height < this.viewport.Bottom))
					this.isMeasurementRequired = true;

				// The report row is really just a container for report cells.  The System.Reflection library is used to rip apart the content and create
				// columns that are tightly bound to the type of data found in the incoming record.
				foreach (PropertyInfo propertyInfo in iContent.GetType().GetProperties())
				{
					IContent childContent = propertyInfo.GetValue(iContent, null) as IContent;
					if (childContent != null)
					{
						string columnId;
						if (this.reportGrid.reportFieldCollection.ColumnIdMap.TryGetValue(childContent.GetType(), out columnId))
						{
							ReportColumn reportColumn;
							if (this.reportGrid.reportColumnCollection.TryGetValue(columnId, out reportColumn))
								reportRow.Add(reportColumn, new ReportCell(childContent, reportColumn, reportRow));
						}
					}
				}

			}

			// This is used to keep track of the order of the row in the report.
			this.ordinal++;

			// This moves the virtual cursor up to the next available row in the virtual space.
			this.height += reportRow.Height;

			// The report can have any number of levels in the hierarchical arrangment or the rows.  This will recurse into the template that defines the
			// outline of the report and generate any rows that can be matched to the templates that are children of the current template.  The 'Path' property
			// on the template rows indicate which properties of the content row to follow when recursing.
			foreach (RowTemplate childRowDefinition in rowTemplate.Children)
			{
				PropertyInfo propertyInfo = iContent.GetType().GetProperty(childRowDefinition.Path);
				if (propertyInfo != null)
				{
					Object property = propertyInfo.GetValue(iContent, null);
					if (property is IEnumerable)
					{
						IEnumerable iEnumerable = property as IEnumerable;
						foreach (IContent childContent in iEnumerable)
							RecursivelyUpdateRow(childContent, childRowDefinition);
					}
				}
			}

		}

		/// <summary>
		/// Removes a row from the collection.
		/// </summary>
		/// <param name="reportRow">The row to be removed.</param>
		public void Remove(ReportRow reportRow)
		{

			// Remove the row from this collection.
			this.dictionary.Remove(reportRow);

			// This will remove the visual elements from the screen that are associated with this row.
			foreach (ReportCanvas reportCanvas in this.reportGrid.reportCanvases)
				reportCanvas.Remove(reportRow);

			// The report needs to purge itself of obviated user interface elements from the obsolete columns.
			this.viewport = this.reportGrid.reportCanvases[3].Viewport;
			if ((viewport.Top <= reportRow.Top && reportRow.Top < viewport.Bottom) ||
				(viewport.Top <= reportRow.Top + reportRow.Height && reportRow.Top + reportRow.Height < viewport.Bottom))
				this.reportGrid.InvalidateMeasure();

		}

	}

}
