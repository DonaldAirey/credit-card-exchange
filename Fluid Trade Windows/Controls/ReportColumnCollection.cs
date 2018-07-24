namespace FluidTrade.Core.Windows.Controls
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    /// <summary>
	/// Describes the set of columns and the order that they appear in a MarkThree.Windows.Controls.Report.
	/// </summary>
	public class ReportColumnCollection : Animatable, IList
	{

		// Public Static Fields
		public static readonly DependencyProperty WidthProperty;

		// Private Instance Fields
		private Dictionary<String, ReportColumn> dictionary;
		private List<ReportColumn> list;
		private ReportGrid reportGrid;
		private StorylineQueue storylineQueue;

		/// <summary>
		/// Initialize the static resources required by the MarkThree.Windows.Controls.ReportColumnCollection.
		/// </summary>
		static ReportColumnCollection()
		{

			// Width Property
			ReportColumnCollection.WidthProperty = DependencyProperty.Register(
				"Width",
				typeof(Double),
				typeof(ReportColumnCollection),
				new PropertyMetadata(new PropertyChangedCallback(OnWidthChanged)));

		}

		/// <summary>
		/// Creates a collection that is used to describe the order and the subset of columns displayed in a report.
		/// </summary>
		/// <param name="fieldDefinitionCollection">A collections of column definitions.</param>
		internal ReportColumnCollection(ReportGrid reportGrid)
		{

			// Initialize the object.
			this.reportGrid = reportGrid;
			this.list = new List<ReportColumn>();
			this.dictionary = new Dictionary<String, ReportColumn>();
			this.storylineQueue = new StorylineQueue();

		}

		/// <summary>
		/// Gets the list of visible columns in the report.
		/// </summary>
		public List<ReportColumn> Columns
		{
			get {return this.list;}
		}

		/// <summary>
		/// Gets a MarkThree.Windows.Controls.ReportColumn using the ColumnId as a key.
		/// </summary>
		/// <param name="key">The unique identifier for a column.</param>
		/// <returns>The MarkThee.Controls.ReportColumn having the given key.</returns>
		public ReportColumn this[String key]
		{
			get { return this.dictionary[key]; }
		}

		/// <summary>
		/// Gets the width of the collection of report columns.
		/// </summary>
		public Double Width
		{
			get { return (Double)this.GetValue(ReportColumnCollection.WidthProperty); }
		}

		/// <summary>
		/// Create a freezable copy of the object.
		/// </summary>
		/// <returns>A new, freezable copy of the object.</returns>
		protected override Freezable CreateInstanceCore()
		{
			return new ReportColumnCollection(this.reportGrid);
		}

		/// <summary>
		/// Finds a column at the given horizontal coordinate.
		/// </summary>
		/// <param name="x">A horizontal position in the collection.</param>
		/// <returns>The column that resides at the given location or null if there is no column there.</returns>
		public ReportColumn FindColumnAt(Double x)
		{

			// This will do a horizontal hit test on the given coordinate and return a column if it is found to occupy the same space.
			foreach (ReportColumn reportColumn in this.list)
				if (reportColumn.Left <= x && x < reportColumn.Left + reportColumn.Width)
					return reportColumn;

			// At this point, there is no column that occupies the coordinate.
			return null;

		}

		/// <summary>
		/// First segment of the animation sequence to insert a column into the collection.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="genericEventArgs">The event arguments.</param>
		private Storyline FirstSegmentInsert(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation segment from the current command on the queue.
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[0];
			ReportColumn reportColumn = (ReportColumn)genericEventArgs.Arguments[1];
			Int32 index = (Int32)genericEventArgs.Arguments[2];
			Duration duration = (Duration)genericEventArgs.Arguments[3];

			// New columns are added to the end of the report.  Otherwise the column takes its position from the current occupant of that column's position.
			reportColumn.Left = index == this.list.Count ? this.Width : this.list[index].Left;

			// This is where the actual work is done to add the ReportColumn to the collection.  The list maintains the order of the columns for user
			// interfaces.  The dictionary is used for fast access to the column based on the ColumnId.
			this.list.Insert(index, reportColumn);
			this.dictionary.Add(reportColumn.ColumnId, reportColumn);

			// This will ask the Report that owns this collection to update the user interface with a new column.  The new column needs to be installed and
			// bound to this column collection for the animatin to have any effect. This will broadcast an event that indicates that the collection was
			// modified.
			this.reportGrid.AddReportColumn();

			// This will notify any listeners that the collection has changed.
			this.reportGrid.RaiseEvent(
				new CollectionChangedEventArgs(ReportGrid.CollectionChangedEvent, undoAction, CollectionChangedAction.Add, reportColumn, index));

			// Inserting a column involves several animations tied to a single clock.
			Storyline storyline = new Storyline();

			// This animation will change the width of the entire collection.
			DoubleAnimation collectionWidthAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(collectionWidthAnimation, this);
			Storyline.SetTargetProperty(collectionWidthAnimation, ReportColumnCollection.WidthProperty);
			collectionWidthAnimation.From = this.Width;
			collectionWidthAnimation.To = this.Width + reportColumn.Width;
			collectionWidthAnimation.Duration = duration;
			storyline.Children.Add(collectionWidthAnimation);

			// This animation will instantaneously change the left edge of the new column.
			DoubleAnimation columnLeftAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(columnLeftAnimation, reportColumn);
			Storyline.SetTargetProperty(columnLeftAnimation, ReportColumn.ActualLeftProperty);
			columnLeftAnimation.From = reportColumn.Left;
			columnLeftAnimation.To = reportColumn.Left;
			columnLeftAnimation.Duration = new Duration(TimeSpan.Zero);
			storyline.Children.Add(columnLeftAnimation);

			// This animation will change the width of the selected column.
			DoubleAnimation columnWidthAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(columnWidthAnimation, reportColumn);
			Storyline.SetTargetProperty(columnWidthAnimation, ReportColumn.ActualWidthProperty);
			columnWidthAnimation.From = 0;
			columnWidthAnimation.To = reportColumn.Width;
			columnWidthAnimation.Duration = duration;
			storyline.Children.Add(columnWidthAnimation);

			// This will create a series of animation sequences that will shift all the columns that appear after the inserted column.
			Double left = 0.0;
			foreach (ReportColumn movingColumn in this.list)
			{

				// Only the columns that have actually moved will be animated.
				if (movingColumn.Left != left)
				{

					// This is the target position for the left edge of the column.
					movingColumn.Left = left;

					// This will animate the movement of the column from its current position to the target position.
					DoubleAnimation leftAnimation = new DoubleAnimation();
					Storyline.SetTargetObject(leftAnimation, movingColumn);
					Storyline.SetTargetProperty(leftAnimation, ReportColumn.ActualLeftProperty);
					leftAnimation.From = movingColumn.ActualLeft;
					leftAnimation.To = movingColumn.Left;
					leftAnimation.Duration = duration;
					storyline.Children.Add(leftAnimation);

				}

				// This keeps track of the left edge of the column in the ordered list.
				left += movingColumn.Width;

			}

			// Start the animation.
			return storyline;

		}

		/// <summary>
		/// First part of the animation sequence to move a column from one index to another.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="genericEventArgs">The event arguments.</param>
		private Storyline FirstSegmentMove(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation from the current command on the queue.
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[0];
			Int32 oldIndex = (Int32)genericEventArgs.Arguments[1];
			Int32 newIndex = (Int32)genericEventArgs.Arguments[2];
			Duration duration = (Duration)genericEventArgs.Arguments[3];

			// This is where the actual work is done to move a ReportColumn from one position to another.
			ReportColumn reportColumn = this.list[oldIndex];
			this.list.RemoveAt(oldIndex);
			this.list.Insert(newIndex, reportColumn);

			// This will notify any listeners that the collection has changed and is generally used to modify the source XAML to
			// reflect a change to the configuration of the columns.
			this.reportGrid.RaiseEvent(
				new CollectionChangedEventArgs(
					ReportGrid.CollectionChangedEvent,
					undoAction,
					CollectionChangedAction.Move,
					this.list[newIndex],
					newIndex,
					oldIndex));

			// This brings the selected column to the front of the Z index so it will appear to float over the columns that only move a little.
			this.reportGrid.BringToFront(this.list);

			// Setting the width of a column involves several animations tied to a single clock.
			Storyline storyline = new Storyline();

			// The columns to the left will be shifted to the right to fill in the space where the selected column used to reside.
			Double left = 0.0;
			foreach (ReportColumn movingColumn in this.list)
			{

				// Only the columns that have actuall moved will be animated.
				if (movingColumn.Left != left)
				{

					// This sets the target location for the left edge of the column.
					movingColumn.Left = left;

					// This will animate the movement of the column from the curernt position to the desired location.
					DoubleAnimation leftAnimation = new DoubleAnimation();
					Storyline.SetTargetObject(leftAnimation, movingColumn);
					Storyline.SetTargetProperty(leftAnimation, ReportColumn.ActualLeftProperty);
					leftAnimation.From = movingColumn.ActualLeft;
					leftAnimation.To = movingColumn.Left;
					leftAnimation.Duration = duration;
					storyline.Children.Add(leftAnimation);

				}

				// This keeps track of the left edge of the column in the ordered list.
				left += movingColumn.Width;

			}

			// Start the animation.
			return storyline;

		}

		/// <summary>
		/// First segment of the animation sequence to remove a column from the view.
		/// </summary>
		/// <param name="iAnimatable">The target object for the animation.</param>
		private Storyline FirstSegmentRemove(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation from the current command on the queue.
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[0];
			ReportColumn reportColumn = (ReportColumn)genericEventArgs.Arguments[1];
			Duration duration = (Duration)genericEventArgs.Arguments[2];

			// Inserting a column involves several animations tied to a single clock.
			Storyline storyline = new Storyline();

			// This animation will change the width of the entire collection.
			DoubleAnimation collectionWidthAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(collectionWidthAnimation, this);
			Storyline.SetTargetProperty(collectionWidthAnimation, ReportColumnCollection.WidthProperty);
			collectionWidthAnimation.From = this.Width;
			collectionWidthAnimation.To = this.Width - reportColumn.Width;
			collectionWidthAnimation.Duration = duration;
			storyline.Children.Add(collectionWidthAnimation);

			// This animation will change the width of the selected column.
			DoubleAnimation columnWidthAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(columnWidthAnimation, reportColumn);
			Storyline.SetTargetProperty(columnWidthAnimation, ReportColumn.ActualWidthProperty);
			columnWidthAnimation.From = reportColumn.ActualWidth;
			columnWidthAnimation.To = 0.0;
			columnWidthAnimation.Duration = duration;
			storyline.Children.Add(columnWidthAnimation);

			// This will create a series of animation sequences that will shift the remaining columns to fill in the space created when the selected column was
			// removed.
			Double left = 0.0;
			foreach (ReportColumn movingColumn in this.list)
			{

				// The column that is being removed is not included in the calculation of the left edges.
				if (movingColumn == reportColumn)
					continue;

				// Only the columns that have actually moved will be animated.
				if (movingColumn.Left != left)
				{

					// This updates the target for the left edge of the column.
					movingColumn.Left = left;

					// This animation will move the column from its current position to the target.
					DoubleAnimation leftAnimation = new DoubleAnimation();
					Storyline.SetTargetObject(leftAnimation, movingColumn);
					Storyline.SetTargetProperty(leftAnimation, ReportColumn.ActualLeftProperty);
					leftAnimation.From = movingColumn.ActualLeft;
					leftAnimation.To = movingColumn.Left;
					leftAnimation.Duration = duration;
					storyline.Children.Add(leftAnimation);

				}

				// This keeps track of the left edge of the column in the ordered list.
				left += movingColumn.Width;

			}

			// Start the animation.
			return storyline;

		}

		/// <summary>
		/// First segment of the animation sequence to reset the list.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="genericEventArgs">The event arguments.</param>
		private Storyline FirstSegmentReplace(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation from the current command on the queue.
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[0];
			List<ReportColumn> newList = (List<ReportColumn>)genericEventArgs.Arguments[1];
			Duration duration = (Duration)genericEventArgs.Arguments[3];

			// For the Undo logic to work, it will need the set of columns present before they were replaced.  The copy of the current column list is passed 
			// to the second segment (and thus on to the event handler), through the GenericEventArgs.
			List<ReportColumn> originalList = new List<ReportColumn>();
			originalList.AddRange(this.list);
			genericEventArgs.Arguments[2] = originalList;

			// This list collects the new columns and is broadcast as an event.
			List<ReportColumn> addedColumns = new List<ReportColumn>();

			// Setting the width of a column involves several animations tied to a single clock.
			Storyline storyline = new Storyline();

			// This will calculate the total width of the report after the columns have been reset.
			Double width = 0.0;
			foreach (ReportColumn reportColumn in newList)
				width += reportColumn.Width;

			// This animation will change the width of the entire collection.
			DoubleAnimation collectionWidthAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(collectionWidthAnimation, this);
			Storyline.SetTargetProperty(collectionWidthAnimation, ReportColumnCollection.WidthProperty);
			collectionWidthAnimation.From = this.Width;
			collectionWidthAnimation.To = width;
			collectionWidthAnimation.Duration = duration;
			storyline.Children.Add(collectionWidthAnimation);

			// This keeps track of the place where the next column will be added to the report as each of the columns in the new list is given an animation
			// sequence to move it into position.
			Double left = 0.0;

			// As items are added to the collection they need to move from some spot to another spot.  Just having them materialize at their destination
			// location is disturbing to the eye.  The best place visually is the current right edge of the previous column in the new order.  This will keep
			// track of the previous column as the animation sequences are created.
			ReportColumn previousColumn = null;
			
			// This loop will create an animation sequence that will move each of the columns in the new column set to their new position.
			foreach (ReportColumn reportColumn in newList)
			{

				// Columns that are already part of the report will be moved if their position has changed.  Columns that are are new to the report will be
				// added.  Columns that are no longer part of the report will be removed when this loop is finished.
				if (this.list.Contains(reportColumn))
				{

					// An animation sequence is created to move the column if the position has changed.
					if (reportColumn.Left != left)
					{

						// This updates the target location for the left edge of the column.
						reportColumn.Left = left;

						// This will animate the movement of the column from the current position to the target location.
						DoubleAnimation leftAnimation = new DoubleAnimation();
						Storyline.SetTargetObject(leftAnimation, reportColumn);
						Storyline.SetTargetProperty(leftAnimation, ReportColumn.ActualLeftProperty);
						leftAnimation.From = reportColumn.ActualLeft;
						leftAnimation.To = reportColumn.Left;
						leftAnimation.Duration = duration;
						storyline.Children.Add(leftAnimation);

					}

				}
				else
				{

					// New columns will require a horizontal position in the list.
					reportColumn.Left = left;

					// The data for the column needs to be added first.  The list and the dictionary are used for ordered access and random access,
					// respectively.
					this.list.Insert(newList.IndexOf(reportColumn), reportColumn);
					this.dictionary.Add(reportColumn.ColumnId, reportColumn);

					// This will call out to the report to have the data added for this column.  Every row must include a cell for the newly added column.
					addedColumns.Add(reportColumn);

					// This animation will make the new column appear to grow from nothing until it reaches its full size.
					DoubleAnimation columnWidthAnimation = new DoubleAnimation();
					Storyline.SetTargetObject(columnWidthAnimation, reportColumn);
					Storyline.SetTargetProperty(columnWidthAnimation, ReportColumn.ActualWidthProperty);
					columnWidthAnimation.From = 0.0;
					columnWidthAnimation.To = reportColumn.Width;
					columnWidthAnimation.Duration = duration;
					storyline.Children.Add(columnWidthAnimation);

					// This animation will set the left edge of the new column using the current location of its precessor.
					if (previousColumn != null)
					{
						DoubleAnimation columnLeftAnimation = new DoubleAnimation();
						Storyline.SetTargetObject(columnLeftAnimation, reportColumn);
						Storyline.SetTargetProperty(columnLeftAnimation, ReportColumn.ActualLeftProperty);
						columnLeftAnimation.From = previousColumn.ActualLeft + previousColumn.ActualWidth;
						columnLeftAnimation.To = reportColumn.Left;
						columnLeftAnimation.Duration = duration;
						storyline.Children.Add(columnLeftAnimation);
					}

				}

				// The previous column is used to compute the starting point for the animation of the left edge of new columns.
				previousColumn = reportColumn;

				// The running total of the columns width is used to set the left position of the next column.
				left += reportColumn.Width;

			}

			// The next loop will remove columns from the report.  For reasons similar to adding a new column, removing a column needs a destination for the
			// left edge otherwise it appears to stand still while all the other columns are moving.  Picking a destination for columns that have been removed
			// is somewhat artificial because they really don't belong to the report.  However, choosing the right edge of the previous column appears to be
			// the most natural operation on the eye.
			previousColumn = null;
	
			// This creates an animation to remove the columns that are obsolete.  Note that the actual data is not removed until the animation sequence is
			// complete.  The philosophy is: if you can still see it, it must be part of the report.
			foreach (ReportColumn reportColumn in this.list)
			{

				// The previous column that is part of the visible report is used as a visual anchor for a column when it is removed from the report.  Even
				// though the deleted column really has no position in the new version of the report, it needs to disappear gracefully.  Keeping the column in 
				// the same place while others move around it is visually disturbing.  So while the visual anchor for a deleted column is somewhat artificial, 
				// keeping track of the previous column is useful in making the animation appear smoother.
				if (newList.Contains(reportColumn))
				{
					previousColumn = reportColumn;
				}
				else
				{

					// If a previous column is available, it is used as a visual anchor for the column that is to be deleted.  The deleted column will move
					// sideways while shrinking down to nothing and thus be removed from the visual report.
					if (previousColumn != null)
					{
						DoubleAnimation columnLeftAnimation = new DoubleAnimation();
						Storyline.SetTargetObject(columnLeftAnimation, reportColumn);
						Storyline.SetTargetProperty(columnLeftAnimation, ReportColumn.ActualLeftProperty);
						columnLeftAnimation.From = reportColumn.Left;
						columnLeftAnimation.To = previousColumn.Left + previousColumn.Width;
						columnLeftAnimation.Duration = duration;
						storyline.Children.Add(columnLeftAnimation);
					}

					// This will shrink the deleted column down to nothng.
					DoubleAnimation columnWidthAnimation = new DoubleAnimation();
					Storyline.SetTargetObject(columnWidthAnimation, reportColumn);
					Storyline.SetTargetProperty(columnWidthAnimation, ReportColumn.ActualWidthProperty);
					columnWidthAnimation.From = reportColumn.ActualWidth;
					columnWidthAnimation.To = 0.0;
					columnWidthAnimation.Duration = duration;
					storyline.Children.Add(columnWidthAnimation);

				}

			}

			// After the first segment of the animation sequence is initialized, the new columns are part of the document and should be added to all the rows
			// to which they apply.
			this.reportGrid.AddReportColumn();

			// Start the animation.
			return storyline;

		}

		/// <summary>
		/// First segment of the animation sequence to set the width of a column.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="genericEventArgs">The event arguments.</param>
		private Storyline FirstSegmentSetWidth(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation from the current command on the queue.
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[0];
			ReportColumn reportColumn = (ReportColumn)genericEventArgs.Arguments[1];
			Double newWidth = (Double)genericEventArgs.Arguments[3];
			Duration duration = (Duration)genericEventArgs.Arguments[4];

			// The previous and new width of the column will be broadcast in an event at the end of this operation.  Saving the column width at this point will
			// allow the undo operation to restore the column.
			Double oldWidth = reportColumn.Width;

			// This is how far all the other columns need to be moved to make way for the new column width.
			Double movement = newWidth - reportColumn.Width;

			// This sets the width of the column.  The rest of the logic in this segment and the next deals with animating the change and notifying listeners
			// of the events when they are effected.
			reportColumn.Width = newWidth;

			// Setting the width of a column involves several animations tied to a single clock.
			Storyline storyline = new Storyline();

			// This animation will change the width of the entire collection.
			DoubleAnimation collectionWidthAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(collectionWidthAnimation, this);
			Storyline.SetTargetProperty(collectionWidthAnimation, ReportColumnCollection.WidthProperty);
			collectionWidthAnimation.From = this.Width;
			collectionWidthAnimation.To = this.Width + movement;
			collectionWidthAnimation.Duration = duration;
			storyline.Children.Add(collectionWidthAnimation);

			// This animation will change the width of the selected column.
			DoubleAnimation columnWidthAnimation = new DoubleAnimation();
			Storyline.SetTargetObject(columnWidthAnimation, reportColumn);
			Storyline.SetTargetProperty(columnWidthAnimation, ReportColumn.ActualWidthProperty);
			columnWidthAnimation.From = reportColumn.ActualWidth;
			columnWidthAnimation.To = reportColumn.Width;
			columnWidthAnimation.Duration = duration;
			storyline.Children.Add(columnWidthAnimation);

			// This will create a series of animations that will shift all the columns to the right of the modified column.
			for (Int32 index = this.list.IndexOf(reportColumn) + 1; index < this.list.Count; index++)
			{

				// This sets the target for the columns motion.
				ReportColumn movingColumn = this.list[index];
				movingColumn.Left = movingColumn.Left + movement;

				// This will move the column from its current location to the new location.
				DoubleAnimation leftAnimation = new DoubleAnimation();
				Storyline.SetTargetObject(leftAnimation, movingColumn);
				Storyline.SetTargetProperty(leftAnimation, ReportColumn.ActualLeftProperty);
				leftAnimation.From = movingColumn.ActualLeft;
				leftAnimation.To = movingColumn.Left;
				leftAnimation.Duration = duration;
				storyline.Children.Add(leftAnimation);

			}

			// This will notify any listeners that the column property has changed.
			this.reportGrid.RaiseEvent(
				new ColumnChangedEventArgs(ReportGrid.ColumnChangedEvent, undoAction, reportColumn, ReportColumn.WidthProperty, oldWidth, newWidth));

			// Start the animation.
			return storyline;

		}

		/// <summary>
		/// Inserts an element into the MarkThree.Windows.Controls.ColumnReferenceCollection at the specified index.
		/// </summary>
		/// <param name="index">The index of the new element.</param>
		/// <param name="value">The new element to be placed in the collection.</param>
		internal void Insert(Int32 index, ReportColumn reportColumn)
		{

			// This form of the method is a shortcut for the longer form which includes the Undo action.  The Undo action tells the
			// handlers for the Undo and Redo actions how to store the command so it can be undone or redone property at some later
			// time.
			Insert(index, reportColumn, UndoAction.Create);

		}

		/// <summary>
		/// Inserts an element into the MarkThree.Windows.Controls.ColumnReferenceCollection at the specified index.
		/// </summary>
		/// <param name="index">The index of the new element.</param>
		/// <param name="value">The new element to be placed in the collection.</param>
		/// <param name="undoAction">The state of the Undo/Redo handling logic.</param>
		internal void Insert(Int32 index, ReportColumn reportColumn, UndoAction undoAction)
		{

			// The width defaults to the field width if this column has never been part of this report.  Note that is is possible
			// for a column which has already been initalized to be added back to the report through an Undo/Redo sequence.
			ReportField fieldDefinition = this.reportGrid.reportFieldCollection[reportColumn.ColumnId];
			if (Double.IsNaN(reportColumn.Width))
				reportColumn.Width = fieldDefinition.Width;

			// This will initiate an animation sequence that will add the column.
			this.storylineQueue.Enqueue(FirstSegmentInsert, undoAction, reportColumn, index, this.reportGrid.duration);

		}

		/// <summary>
		/// Move a column from one position to another.
		/// </summary>
		/// <param name="oldIndex">The current index of the column in the list of columns.</param>
		/// <param name="index">The desired index of the column in the list of columns.</param>
		internal void Move(Int32 oldIndex, Int32 index)
		{

			// This method is just a shortcut for moving columns with an implicit Undo action.  The 'Create' undo action indicates
			// that this is the start of something that should be put on the Undo stack so it can be undone at a later time.  This
			// is the default action for all new operations.
			Move(oldIndex, index, UndoAction.Create);

		}

		/// <summary>
		/// Move a column from one position to another.
		/// </summary>
		/// <param name="oldIndex">The current index of the column in the list of columns.</param>
		/// <param name="index">The desired index of the column in the list of columns.</param>
		internal void Move(Int32 oldIndex, Int32 index, UndoAction undoAction)
		{

			// This will queue up a command to move a column from the given index to the new index.
			GenericEventArgs genericEventArgs = new GenericEventArgs(undoAction, oldIndex, index, this.reportGrid.duration);
			this.storylineQueue.Enqueue(FirstSegmentMove, genericEventArgs);

		}

		/// <summary>
		/// Handles a change to the Width property.
		/// </summary>
		/// <param name="dependencyObject">The object posessing the property that has changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">Information about the property change.</param>
		private static void OnWidthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// This will broadcast a notification that the width of the entire collection has changed.
			if (dependencyObject is ReportColumnCollection)
			{
				ReportColumnCollection reportColumnCollection = dependencyObject as ReportColumnCollection;
				reportColumnCollection.reportGrid.UpdateExtent();
			}

		}

		/// <summary>
		/// Removes the first occurence of the specified element from the MarkThree.Windows.Controls.ColumnReferenceCollection.
		/// </summary>
		/// <param name="reportColumn">The column that is to be removed from the report.</param>
		/// <param name="undoAction">The state of the Undo/Redo operation.</param>
		internal void Remove(ReportColumn reportColumn)
		{

			// The public form of this method is a convinient way to call the version with the Undo/Redo state already initialized.
			Remove(reportColumn, UndoAction.Create);

		}

		/// <summary>
		/// Removes the first occurence of the specified element from the MarkThree.Windows.Controls.ColumnReferenceCollection.
		/// </summary>
		/// <param name="reportColumn">The column that is to be removed from the report.</param>
		/// <param name="undoAction">The state of the Undo/Redo operation.</param>
		internal void Remove(ReportColumn reportColumn, UndoAction undoAction)
		{

			// This will invoke an animation sequence that will remove the column.
			GenericEventArgs genericEventArgs = new GenericEventArgs(undoAction, reportColumn, this.reportGrid.duration);
			this.storylineQueue.Enqueue(FirstSegmentRemove, genericEventArgs);
			this.storylineQueue.Enqueue(SecondSegmentRemove, genericEventArgs);

		}

		/// <summary>
		/// Replaces the current column set with a new one.
		/// </summary>
		/// <param name="reportColumns">A list of the new columns for the report.</param>
		internal void Replace(List<ReportColumn> reportColumns)
		{

			// This is the public version of the internal method that passes the Undo/Redo state around.
			Replace(reportColumns, UndoAction.Create);

		}

		/// <summary>
		/// Replaces the current column set with a new one.
		/// </summary>
		/// <param name="reportColumns">A list of the new columns for the report.</param>
		/// <param name="undoAction">The state of the Undo/Redo stack when executing this command.</param>
		internal void Replace(List<ReportColumn> reportColumns, UndoAction undoAction)
		{

			// This will invoke an animation sequence that will replace one set of columns with another.  Note that the 'GenericEventArgs' is shared between 
			// the first and second segment.  The shared structure allows the first segment to pass information on to the second segment.  In this case, the
			// values passed are a copy of the old and new lists of columns which are needed for the 'Undo' and 'Redo' actions.
			GenericEventArgs genericEventArgs = new GenericEventArgs(undoAction, reportColumns, null, this.reportGrid.duration);
			this.storylineQueue.Enqueue(FirstSegmentReplace, genericEventArgs);
			this.storylineQueue.Enqueue(SecondSegmentReplace, genericEventArgs);

		}

		/// <summary>
		/// Second segment of the animation sequence to remove a column from the view.
		/// </summary>
		/// <param name="iAnimatable">The target object for the animation.</param>
		private Storyline SecondSegmentRemove(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation from the current command on the queue.
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[0];
			ReportColumn reportColumn = (ReportColumn)genericEventArgs.Arguments[1];

			// This is where the actual work is done to remove a ColumnReference from the collection.
			Int32 index = this.list.IndexOf(reportColumn);
			this.list.Remove(reportColumn);
			this.dictionary.Remove(reportColumn.ColumnId);

			// This event will request that the owning object purge the user interface of the obsolete columns.
			List<ReportColumn> reportColumns = new List<ReportColumn>();
			reportColumns.Add(reportColumn);
			this.reportGrid.RemoveColumns(reportColumns);

			// This will notify any listeners that the collection has changed and is generally used to modify the source XAML to
			// reflect a change to the configuration of the columns.
			this.reportGrid.RaiseEvent(
				new CollectionChangedEventArgs(ReportGrid.CollectionChangedEvent, undoAction, CollectionChangedAction.Remove, reportColumn, index));

			// Some operations in an animated environment must be occur in sequence.  This segment must run only at the completion
			// of the first animation segment.  However, the next command after this one can run Immediately.  The NullTimeline
			// provides an infinitely small unit of time between this command and the next.
			return new NullTimeline();

		}

		/// <summary>
		/// Second segment of the animation sequence to reset the list.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="genericEventArgs">The event arguments.</param>
		private Storyline SecondSegmentReplace(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the arguments for this animation from the current command on the queue.
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[0];
			List<ReportColumn> newItems = (List<ReportColumn>)genericEventArgs.Arguments[1];
			List<ReportColumn> oldItems = (List<ReportColumn>)genericEventArgs.Arguments[2];

			// The columns deleted during this operation are used to update the user interface through an event trigger.
			List<ReportColumn> deletedColumns = new List<ReportColumn>();

			// The columns that are no longer needed are expunged from the report.  Both the list and the dictionary are cleared of the colum and then the user
			// interface elements are cleared using the event callback.
			for (Int32 index = 0; index < this.list.Count; )
			{
				ReportColumn reportColumn = this.list[index];
				if (!newItems.Contains(reportColumn))
				{
					this.list.RemoveAt(index);
					this.dictionary.Remove(reportColumn.ColumnId);
					deletedColumns.Add(reportColumn);
				}
				else
					index++;
			}

			// At this point, all the obsolete columns have been removed.  The items left in the list are intended to be there but are out of order.  This will
			// run through the list and move all the items to the propert order as defined by the new version of the column list.
			for (Int32 index = 0; index < newItems.Count; index++)
			{

				// If the column is moved if it has changed positions in the ordered list.
				ReportColumn reportColumn = newItems[index];
				Int32 oldIndex = this.list.IndexOf(reportColumn);
				if (index != oldIndex)
				{
					this.list.RemoveAt(oldIndex);
					this.list.Insert(index, reportColumn);
				}

			}

			// This instructs the main grid to remove the set of deleted columns.  This, in turn, calls the row collection to recursively remove the column in 
			// any row in which this set of columns is found.
			this.reportGrid.RemoveColumns(deletedColumns);

			// Once the operation is completed an event is broadcast that describes the changes to the column set.
			this.reportGrid.RaiseEvent(
				new CollectionChangedEventArgs(ReportGrid.CollectionChangedEvent, undoAction, CollectionChangedAction.Replace, newItems, oldItems));

			// This provides an instantaneous animation sequence that allows Storylines to be run as a sequence.
			return new NullTimeline();

		}

		/// <summary>
		/// Sets the width of a column in the collection.
		/// </summary>
		/// <param name="reportColumn">The column to be modified.</param>
		/// <param name="width">The new width of the column.</param>
		public void SetColumnWidth(ReportColumn reportColumn, Double width)
		{

			// This public method is a simple form of the version of the method that keeps track of the Undo/Redo state.
			SetColumnWidth(reportColumn, width, UndoAction.Create);

		}

		/// <summary>
		/// Sets the width of a column in the collection.
		/// </summary>
		/// <param name="reportColumn">The column to be modified.</param>
		/// <param name="width">The new width of the column.</param>
		/// <param name="undoAction">The state of the Undo/Redo stack when executing this command.</param>
		internal void SetColumnWidth(ReportColumn reportColumn, Double width, UndoAction undoAction)
		{

			// This will animate the movement of all the columns to adjust the to the new width.
			GenericEventArgs genericEventArgs = new GenericEventArgs(undoAction, reportColumn, null, width, this.reportGrid.duration);
			this.storylineQueue.Enqueue(FirstSegmentSetWidth, genericEventArgs);

		}

		/// <summary>
		/// Gets the value associated with the given key.
		/// </summary>
		/// <param name="key">The key value.</param>
		/// <param name="value">The ReportColumn having the given key.</param>
		/// <returns>True if the given key is in the dictionary.</returns>
		public Boolean TryGetValue(String key, out ReportColumn value)
		{

			// The dictionary contains the ReportColumn values indexed by the ColumnId.
			return this.dictionary.TryGetValue(key, out value);

		}

		#region IList Members

		/// <summary>
		/// Adds a MarkThree.Windows.Controls.ColumnReference to the end of the collection.
		/// </summary>
		/// <param name="value">The value to be added to the end of the collection.</param>
		/// <returns>0</returns>
		public Int32 Add(object value)
		{

			// The XAML parser only recognizes the generic IList interfaces, so incoming values need to be qualified to be added to this collection.
			if (value is ReportColumn)
			{

				// Columns added through the XAML will not be animated.
				ReportColumn reportColumn = value as ReportColumn;

				// The column's width defaults to the field's width if this column has never been part of this report.
				ReportField fieldDefinition = this.reportGrid.reportFieldCollection[reportColumn.ColumnId];
				if (Double.IsNaN(reportColumn.Width))
					reportColumn.Width = fieldDefinition.Width;

				// This forces the left edge of the new column to be at the right edge of the collection.
				reportColumn.Left = this.Width;

				// These values are normally animated and have no direct method for setting.  However, when add a column directly to the report it is simply poked
				// into its position, bypassing the animation.
				reportColumn.SetValue(ReportColumn.ActualWidthProperty, reportColumn.Width);
				reportColumn.SetValue(ReportColumn.ActualLeftProperty, reportColumn.Left);
				this.SetValue(ReportColumnCollection.WidthProperty, reportColumn.Left + reportColumn.Width);

				// This is where the actual work is done to add the ColumnDefinition to the collection.  The list maintains the order of the columns for user
				// interfaces.  The dictionary is used for fast access to the column based on the ColumnId.
				this.list.Add(reportColumn);
				this.dictionary.Add(reportColumn.ColumnId, reportColumn);

				// This event is used to update the user interface with a new set of columns.
				this.reportGrid.AddReportColumn();

			}

			// This is here to satisfy the interface specification for an IList.
			return 0;

		}

		/// <summary>
		/// Clears the collection of MarkThree.Windows.Controls.ColumnReferences.
		/// </summary>
		public void Clear()
		{

			// The list of deleted columns is broadcast to instruct the user interface elements to remove obsolete columns.
			List<ReportColumn> deletedColumns = new List<ReportColumn>();
			foreach (ReportColumn reportColumn in this.list)
				deletedColumns.Add(reportColumn);

			// The owner grid must remove all these columns from the current report.
			this.reportGrid.RemoveColumns(deletedColumns);

			// This will restore the collection to the original state.
			this.list.Clear();
			this.dictionary.Clear();

		}

		/// <summary>
		/// Determins whether the MarkThree.Windows.Controls.ColumnReferenceCollection contains the specified key.
		/// </summary>
		/// <param name="value">A key to a ColumnReference item.</param>
		/// <returns>True if the collection contains an item with the specified key, false otherwise.</returns>
		public Boolean Contains(object value)
		{

			// The dictionary is used to indicate if the given key exists in the collection.
			if (value is ReportColumn)
			{
				ReportColumn reportColumn = value as ReportColumn;
				return this.dictionary.ContainsKey(reportColumn.ColumnId);
			}

			// No other object type can be found in this collection.
			return false;

		}

		/// <summary>
		/// Searches for the specified object and returns the zero based index to the first occurrence in the collection.
		/// </summary>
		/// <param name="value">The value to be indexed.</param>
		/// <returns>The zero based index to the first occurence of the object in the collection.</returns>
		public Int32 IndexOf(object value)
		{

			// This insures that the index of the specific ColumnReference is returned.
			if (value is ReportColumn)
				return this.list.IndexOf(value as ReportColumn);

			// No other object type can be found in this collection.
			return -1;

		}

		/// <summary>
		/// Inserts an element into the MarkThree.Windows.Controls.ColumnReferenceCollection at the specified index.
		/// </summary>
		/// <param name="index">The index of the new element.</param>
		/// <param name="value">The new element to be placed in the collection.</param>
		public void Insert(Int32 index, object value)
		{

			// The generic IList version of this method is passed on to the strongly typed version.
			if (value is ReportColumn)
				Insert(index, value as ReportColumn, UndoAction.Create);

		}

		/// <summary>
		/// Gets an indication of whether the collection can change size or not.
		/// </summary>
		public Boolean IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an indication of whether the collection can be modified.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Removes the first occurence of the specified element from the MarkThree.Windows.Controls.ColumnReferenceCollection.
		/// </summary>
		/// <param name="value">The element to be removed.</param>
		public void Remove(object value)
		{

			// The XAML parser only recognizes the generic IList interfaces, so incoming values need to be qualified to be added to
			// this collection.
			if (value is ReportColumn)
				Remove(value as ReportColumn, UndoAction.Create);

		}

		/// <summary>
		/// Removes the element at the specified index of the MarkThree.Windows.Controls.ColumnReferenceCollection.
		/// </summary>
		/// <param name="index">The location where the element will be removed.</param>
		public void RemoveAt(Int32 index)
		{

			// This will delete the column at the given index.
			Remove(this.list[index], UndoAction.Create);

		}

		/// <summary>
		/// Gets or sets the ColumnReference at the specified index.
		/// </summary>
		/// <param name="index">The index of the element.</param>
		/// <returns>The object at the given index.</returns>
		public object this[Int32 index]
		{
			get
			{
				return this.list[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// Copies the entire collection to a compatible one dimensional array, starting at the specified index.
		/// </summary>
		/// <param name="array">The target array.</param>
		/// <param name="index">The starting index.</param>
		public void CopyTo(Array array, Int32 index)
		{
			this.list.CopyTo(array as ReportColumn[], index);
		}

		/// <summary>
		/// Gets the number of elements actually contained in the collection.
		/// </summary>
		public Int32 Count
		{
			get { return this.list.Count; }
		}

		/// <summary>
		/// This property is not supported by this class.
		/// </summary>
		public Boolean IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// This property is not supported by this class.
		/// </summary>
		public object SyncRoot
		{
			get { return null; }
		}
		
		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}
		
		#endregion

	}

}
