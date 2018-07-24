namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Markup;
	using System.Windows.Media.Animation;

	/// <summary>
	/// Describes the attributes of a column.
	/// </summary>
	[ContentProperty("Types")]
	public class ReportField : Animatable, IList<Type>, INotifyPropertyChanged
	{

		// Constants
		private const int defaultDuration = 0;

		// Public Static Fields
		public static readonly System.Windows.DependencyProperty ColumnIdProperty;
		public static readonly System.Windows.DependencyProperty WidthProperty;

		// Private Instance Variables
		private System.Boolean isSelectable;
		private System.Collections.Generic.Dictionary<IAnimatable, Queue<CommandArgumentPair>> commandTable;
		private System.Collections.Generic.Dictionary<AnimationClock, IAnimatable> animationMap;
		private System.Collections.Generic.List<Type> typeList;
		private System.String description;
		private System.Windows.Duration duration;

		// Public Events
		public event PropertyChangedEventHandler PropertyChanged;

		static ReportField()
		{

			// ColumnId Property
			ReportField.ColumnIdProperty = DependencyProperty.Register("ColumnId", typeof(String), typeof(ReportField));

			// Width Property
			ReportField.WidthProperty = DependencyProperty.Register("Width", typeof(System.Double), typeof(ReportField),
				new PropertyMetadata(new PropertyChangedCallback(OnWidthChanged)));

		}

		public ReportField()
		{

			// This object supports the animation of values as columns are modified.  Since the animation takes a finite amount of
			// time to transition from one value to another, the commands to modify this object are queued up and handled in the
			// order in which they were recieved.
			this.commandTable = new Dictionary<IAnimatable, Queue<CommandArgumentPair>>();
			this.animationMap = new Dictionary<AnimationClock, IAnimatable>();
			this.duration = new Duration(TimeSpan.FromMilliseconds(ReportField.defaultDuration));
			this.typeList = new List<Type>();

		}

		protected override Freezable CreateInstanceCore()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Returns a System.String that represents the current object.
		/// </summary>
		/// <returns>A System.String that represents the current object.</returns>
		public override string ToString()
		{
			return string.Format("{{{0}}}", this.ColumnId);
		}

		/// <summary>
		/// Gets or sets the width of the column.
		/// </summary>
		public double Width
		{
			get { return (double)this.GetValue(ReportField.WidthProperty); }
			set { this.SetValue(ReportField.WidthProperty, value); }
		}

		/// <summary>
		/// Gets or sets the column identifier.
		/// </summary>
		public string ColumnId
		{
			get { return (string)this.GetValue(ReportField.ColumnIdProperty); }
			set { this.SetValue(ReportField.ColumnIdProperty, value); }
		}

		/// <summary>
		/// Gets or set the description of the column.
		/// </summary>
		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

		/// <summary>
		/// Gets or sets a value that allows the column to be selected instead of sorted.
		/// </summary>
		public Boolean IsSelectable
		{
			get { return this.isSelectable; }
			set { this.isSelectable = value; }
		}

		public List<Type> Types
		{
			get { return this.typeList; }
		}
		
		/// <summary>
		/// Handles a change to the Width property.
		/// </summary>
		/// <param name="dependencyObject">The object posessing the property that has changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">Information about the property change.</param>
		private static void OnWidthChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			if (dependencyObject is ReportField)
			{
				ReportField fieldDefinition = dependencyObject as ReportField;
				if (fieldDefinition.PropertyChanged != null)
					fieldDefinition.PropertyChanged(fieldDefinition, new PropertyChangedEventArgs("Width"));
			}

		}

		#region IList<Type> Members

		public int IndexOf(Type item)
		{
			return this.typeList.IndexOf(item);
		}

		public void Insert(int index, Type item)
		{
			this.typeList.IndexOf(item, index);
		}

		public void RemoveAt(int index)
		{
			this.typeList.RemoveAt(index);
		}

		public Type this[int index]
		{
			get
			{
				return this.typeList[index];
			}
			set
			{
				this.typeList[index] = value;
			}
		}

		#endregion

		#region ICollection<Type> Members

		public void Add(Type item)
		{
			this.typeList.Add(item);
		}

		public void Clear()
		{
			this.typeList.Clear();
		}

		public bool Contains(Type item)
		{
			return this.typeList.Contains(item);
		}

		public void CopyTo(Type[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return this.typeList.Count; }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool Remove(Type item)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable<Type> Members

		public IEnumerator<Type> GetEnumerator()
		{
			return this.typeList.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.typeList.GetEnumerator();
		}

		#endregion
	}

}
