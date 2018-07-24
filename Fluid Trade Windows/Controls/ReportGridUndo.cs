namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;

	/// <summary>
	/// Handles the Undo/Redo logic for the MarkThree.Windows.Controls.ReportGrid.
	/// </summary>
	class ReportUndo : UndoBase
	{

		// Private Instance Fields
		private Dictionary<DependencyProperty, RoutedCommand> propertyMap;
		//private List<Key> pressedKeys = new List<Key>();

		/// <summary>
		/// Creates a manager for the Undo/Redo logic for a MarkThree.Windows.Controls.Report.
		/// </summary>
		/// <param name="undoManager">The undo manager scope for this object.</param>
		public ReportUndo(UndoManager undoManager) : base(undoManager)
		{

			// The PropertyMap is a means of mapping a given property to a command that undoes or redoes a change to that property.
			this.propertyMap = new Dictionary<DependencyProperty, RoutedCommand>();
			this.propertyMap.Add(ReportGrid.ScaleProperty, ReportGrid.SetScale);
			this.propertyMap.Add(DynamicReport.IsLayoutFrozenProperty, DynamicReport.SetIsLayoutFrozen);

		}

		/// <summary>
		/// Handles changes to the content of a ColumnDefinition in a ReportColumnCollection controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="collectionChangedEventArgs">The event arguments.</param>
		private void HandleColumnChanged(object sender, ColumnChangedEventArgs columnChangedEventArgs)
		{

			// The object that generated this event is a ReportColumnCollection.  The main idea here is to store the action on the proper stack so that it
			// can be undone or redone if the need arises.  Note that it is critical to tag the command with the state of Undoing or Redoing.
			ReportGrid reportGrid = sender as ReportGrid;

			// Two stacks maintain the state of undoing and redoing all actions within its scope.  This will determine which commands will need to execute to
			// undo a given action.  The actual act of undoing an operation will happen when the commands that are placed on the stacks here are executed.
			switch (columnChangedEventArgs.UndoAction)
			{

			case UndoAction.Create:

				// The act of creating an 'Undo' operation (as opposed to simply playing it back) will clear the Redo stack.  That is, once a user udpates the
				// user interface, it is no longer possible to replay the stack forward.
				this.undoManager.RedoStack.Clear();
				this.undoManager.UndoStack.Push(new CommandArgumentPair(UndoColumn, reportGrid, columnChangedEventArgs, UndoAction.Undo));

				break;

			case UndoAction.Redo:

				// If the actions are being played forward, a command needs to be put back on the Undo stack in order to play it backwards.
				this.undoManager.UndoStack.Push(new CommandArgumentPair(UndoColumn, reportGrid, columnChangedEventArgs, UndoAction.Undo));

				break;

			case UndoAction.Undo:

				// When playing a set of actions backwards, those actions are popped off the Undo stack.  This, in turn, will push those same actions onto the
				// Redo stack so the operations can be played forward.
				this.undoManager.RedoStack.Push(new CommandArgumentPair(UndoColumn, reportGrid, columnChangedEventArgs, UndoAction.Redo));

				break;

			}

		}

		/// <summary>
		/// Handles changes to the content of the ReportColumnCollection family of controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void HandleCollectionChanged(object sender, CollectionChangedEventArgs collectionChangedEventArgs)
		{

			// The object that generated this event is a ReportColumnCollection.  The main idea here is to store the action on the proper stack so that it
			// can be undone or redone if the need arises.  Note that it is critical to tag the command with the state of Undoing or Redoing.
			ReportGrid reportGrid = sender as ReportGrid;

			// Two stacks maintain the state of undoing and redoing all actions within its scope.  This will determine which commands will need to execute to
			// undo a given action.  The actual act of undoing an operation will happen when the commands that are placed on the stacks here are executed.
			switch (collectionChangedEventArgs.UndoAction)
			{

			case UndoAction.Create:

				// The act of creating an 'Undo' operation (as opposed to simply playing it back) will clear the Redo stack.  That is, once a user udpates the
				// user interface, it is no longer possible to replay the stack forward.
				this.undoManager.RedoStack.Clear();
				this.undoManager.UndoStack.Push(new CommandArgumentPair(UndoCollection, reportGrid, collectionChangedEventArgs, UndoAction.Undo));
				break;

			case UndoAction.Redo:

				// If the actions are being played forward, a command needs to be put back on the Undo stack in order to play it backwards.
				this.undoManager.UndoStack.Push(new CommandArgumentPair(UndoCollection, reportGrid, collectionChangedEventArgs, UndoAction.Undo));
				break;

			case UndoAction.Undo:

				// When playing a set of actions backwards, those actions are popped off the Undo stack.  This, in turn, will push those same actions onto the
				// Redo stack so the operations can be played forward.
				this.undoManager.RedoStack.Push(new CommandArgumentPair(UndoCollection, reportGrid, collectionChangedEventArgs, UndoAction.Redo));
				break;

			}

		}

		/// <summary>
		/// Handles changes to the content of a ColumnDefinition in a ReportColumnCollection controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="collectionChangedEventArgs">The event arguments.</param>
		private void HandleUndoPropertyChanged(object sender, UndoPropertyChangedEventArgs undoPropertyChangedEventArgs)
		{

			// The object that generated this event is a Report.  The main idea here is to store the action on the proper stack so that it can be undone or
			// redone if the need arises.  Note that it is critical to tag the command with the state of Undoing or Redoing.
			ReportGrid reportGrid = sender as ReportGrid;

			// Two stacks maintain the state of undoing and redoing all actions within its scope.  This will determine which commands will need to execute to
			// undo a given action.  The actual act of undoing an operation will happen when the commands that are placed on the stacks here are executed.
			switch (undoPropertyChangedEventArgs.UndoAction)
			{

			case UndoAction.Create:

				// The act of creating an 'Undo' operation (as opposed to simply playing it back) will clear the Redo stack.  That is, once a user udpates the
				// user interface, it is no longer possible to replay the stack forward.
				this.undoManager.RedoStack.Clear();
				this.undoManager.UndoStack.Push(
					new CommandArgumentPair(
						UndoProperty,
						reportGrid,
						undoPropertyChangedEventArgs.DependencyProperty,
						undoPropertyChangedEventArgs.OldValue,
						undoPropertyChangedEventArgs.NewValue,
						UndoAction.Undo));

				break;

			case UndoAction.Redo:

				// If the actions are being played forward, a command needs to be put back on the Undo stack in order to play it backwards.
				this.undoManager.UndoStack.Push(
					new CommandArgumentPair(
						UndoProperty,
						reportGrid,
						undoPropertyChangedEventArgs.DependencyProperty,
						undoPropertyChangedEventArgs.OldValue,
						undoPropertyChangedEventArgs.NewValue,
						UndoAction.Undo));

				break;

			case UndoAction.Undo:

				// When playing a set of actions backwards, those actions are popped off the Undo stack.  This, in turn, will push those same actions onto the
				// Redo stack so the operations can be played forward.
				this.undoManager.RedoStack.Push(
					new CommandArgumentPair(
						UndoProperty,
						reportGrid,
						undoPropertyChangedEventArgs.DependencyProperty,
						undoPropertyChangedEventArgs.OldValue,
						undoPropertyChangedEventArgs.NewValue,
						UndoAction.Redo));

				break;

			}

		}


		/// <summary>
		/// Removes this object from the chain of events that notify listeners of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will no longer be registered for state change events.</param>
		public override void Unregister(DependencyObject dependencyObject)
		{
			// This will remove this object from the event notifications.
			ReportGrid reportGrid = dependencyObject as ReportGrid;
			reportGrid.RemoveHandler(DynamicReport.UndoPropertyChangedEvent, new UndoPropertyChangedEventHandler(HandleUndoPropertyChanged));
			reportGrid.RemoveHandler(ReportGrid.ColumnChangedEvent, new ColumnChangedEventHandler(HandleColumnChanged));
			reportGrid.RemoveHandler(ReportGrid.CollectionChangedEvent, new CollectionChangedEventHandler(HandleCollectionChanged));
		}

		/// <summary>
		/// Undoes the most recent undo command on the stack.
		/// </summary>
		/// <param name="dependencyObject">The target of the undo operation.</param>
		private void UndoCollection(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments
			ReportGrid reportGrid = (ReportGrid)genericEventArgs.Arguments[0];
			CollectionChangedEventArgs collectionChangedEventArgs = (CollectionChangedEventArgs)genericEventArgs.Arguments[1];
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[2];

			// Each action done to a collection has a specific method to be undone.  Note that the 'Undo' state is passed onto the method to indicate the state
			// of the action.  This tells the event handler on which stack to place the command needed to undo the action.  For example, when undoing a a
			// column addition, you will remove the column.  That removal operation needs to be encoded so that it is placed on the 'Redo' stack when the event
			// is handled.
			switch (collectionChangedEventArgs.Action)
			{

			case CollectionChangedAction.Add:

				// This will undo the action of adding a column to a collection by removing it.
				foreach (ReportColumn reportColumn in collectionChangedEventArgs.NewItems)
					reportGrid.reportColumnCollection.Remove(reportColumn, undoAction);

				break;

			case CollectionChangedAction.Move:

				// This will undo the action of moving a column by returning it to the original position.
				int oldIndex = collectionChangedEventArgs.OldStartingIndex;
				int newIndex = collectionChangedEventArgs.NewStartingIndex;
				foreach (ReportColumn reportColumn in collectionChangedEventArgs.NewItems)
				{
					reportGrid.reportColumnCollection.Move(newIndex, oldIndex, undoAction);
					newIndex++;
					oldIndex++;
				}

				break;

			case CollectionChangedAction.Replace:

				// This will undo the action of replacing the entire list by restoring the original list.
				reportGrid.reportColumnCollection.Replace(collectionChangedEventArgs.OldItems as List<ReportColumn>, undoAction);

				break;

			case CollectionChangedAction.Remove:

				// This will undo the action of removing a column by inserting back in at the original position.
				int index = collectionChangedEventArgs.OldStartingIndex;
				foreach (ReportColumn reportColumn in collectionChangedEventArgs.OldItems)
					reportGrid.reportColumnCollection.Insert(index++, reportColumn, undoAction);

				break;

			}

		}

		/// <summary>
		/// Undoes the most recent undo command on the stack.
		/// </summary>
		/// <param name="dependencyObject">The target of the undo operation.</param>
		private void UndoColumn(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments
			ReportGrid reportGrid = (ReportGrid)genericEventArgs.Arguments[0];
			ColumnChangedEventArgs columnChangedEventArgs = (ColumnChangedEventArgs)genericEventArgs.Arguments[1];
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[2];

			// This will undo a change to the width of a column.
			if (columnChangedEventArgs.DependencyProperty == ReportColumn.WidthProperty)
				reportGrid.reportColumnCollection.SetColumnWidth(columnChangedEventArgs.ColumnDefinition, (Double)columnChangedEventArgs.OldValue, undoAction);

		}

		/// <summary>
		/// Undoes or Redoes a change to a property in a MarkThree.Windows.Controls.Report.
		/// </summary>
		/// <param name="dependencyObject">The target of the undo operation.</param>
		private void UndoProperty(object sender, GenericEventArgs genericEventArgs)
		{

			// Extract the specific arguments from the generic arguments stored on the Undo/Redo stacks.
			ReportGrid reportGrid = (ReportGrid)genericEventArgs.Arguments[0];
			DependencyProperty dependencyProperty = (DependencyProperty)genericEventArgs.Arguments[1];
			object oldValue = genericEventArgs.Arguments[2];
			object newValue = genericEventArgs.Arguments[3];
			UndoAction undoAction = (UndoAction)genericEventArgs.Arguments[4];

			// This will use the property map to Undo or Redo the change to the property.  Notice that the new value and old value are transposed here from
			// their original positions.
			this.propertyMap[dependencyProperty].Execute(new UndoObject(newValue, oldValue, undoAction), reportGrid);

		}

		/// <summary>
		/// Registers this object to receive notifications of changes to the content.
		/// </summary>
		/// <param name="dependencyObject">The target element that will register for state change events.</param>
		public override void Register(DependencyObject dependencyObject)
		{

			// The Undo/Redo strategy involves handling event notifications to changes in the report.  This method registers handlers for this family of
			// controls that will add the proper actions to the Undo/Redo stacks.
			ReportGrid reportGrid = dependencyObject as ReportGrid;
			reportGrid.AddHandler(DynamicReport.UndoPropertyChangedEvent, new UndoPropertyChangedEventHandler(HandleUndoPropertyChanged), true);
			reportGrid.AddHandler(ReportGrid.ColumnChangedEvent, new ColumnChangedEventHandler(HandleColumnChanged), true);
			reportGrid.AddHandler(ReportGrid.CollectionChangedEvent, new CollectionChangedEventHandler(HandleCollectionChanged), true);

		}

	}

}
