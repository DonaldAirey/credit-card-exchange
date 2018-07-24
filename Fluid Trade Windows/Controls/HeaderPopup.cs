namespace FluidTrade.Core.Windows.Controls
{

    using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Media;

	/// <summary>
	/// An indicator of the column header that is the object of a drag-and-drop operation.
	/// </summary>
	/// <copyright>Copyright © 2006 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class HeaderPopup : Popup
	{

		// Private Instance Members
		private System.Collections.Generic.List<ReportCell> cellList;
		private System.Windows.Media.ScaleTransform scaleTransform;
		private System.Windows.ResourceDictionary resourceDictionary;
		private System.Windows.Controls.Orientation orientation;
		
		/// <summary>
		/// Create a cursor for moving columns around with drag-and-drop operations.
		/// </summary>
		/// <param name="bitmap">The bitmap to use for the cursor.</param>
		public HeaderPopup()
		{

			// Initialize the object.
			this.Placement = PlacementMode.Relative;

		}

		/// <summary>
		/// Gets or sets the orientation of cells in the popup window.
		/// </summary>
		public Orientation Orientation
		{
			get { return this.orientation; }
			set { this.orientation = value; }
		}

		/// <summary>
		/// Gets or sets the resources referenced by this Popup when it creates the content window.
		/// </summary>
		public new ResourceDictionary Resources
		{
			get { return this.resourceDictionary; }
			set { this.resourceDictionary = value; }
		}

		/// <summary>
		/// Gets or sets the data contents of the Popup control.
		/// </summary>
		public List<ReportCell> Content
		{

			get { return this.cellList; }
			set
			{

				// This will save a copy of the content in case it is needed later.
				this.cellList = value;

				// This canvas will be painted with the selected content.
				Canvas canvas = new Canvas();

				foreach (ReportCell reportCell in this.cellList)
				{

					// The main idea of setting the content is to create a framework element based on the type of the content.  If
					// there is no template defined for the data, then this Popup will be empty.
					FrameworkElement frameworkElement = null;

					// The content of this control is data driven.  The data type of the content identifies a template that is used
					// to create a framework element and then that data is used as the data context for the created element.  But 
					// none of this will work if there is no resource to hold a template.
					if (this.resourceDictionary != null)
					{

						// This will create a framework element from a data template that is found using the type of the content.  
						// Note that a name scope is created for the object in the event that there are internal names to be
						// resolved in the template.
						object resourceObject = resourceDictionary[new DataTemplateKey(reportCell.Content.GetType())];
						if (resourceObject is DataTemplate)
						{
							DataTemplate dataTemplate = resourceObject as DataTemplate;
							frameworkElement = dataTemplate.LoadContent() as FrameworkElement;
							NameScope nameScope = new NameScope();
							NameScope.SetNameScope(frameworkElement, nameScope);
							ReportGrid.ApplyNames(frameworkElement, nameScope);
							frameworkElement.DataContext = reportCell.Content;
							frameworkElement.Width = reportCell.Rect.Width;
							frameworkElement.Height = reportCell.Rect.Height;

							if (this.orientation == Orientation.Vertical)
							{
								Canvas.SetTop(frameworkElement, reportCell.Rect.Top);
								Canvas.SetLeft(frameworkElement, 0.0);
							}
							else
							{
								Canvas.SetTop(frameworkElement, 0.0);
								Canvas.SetLeft(frameworkElement, reportCell.Rect.Left);
							}

						}

						// This is used to indicate that the element created is a Popup.  The template can apply special formatting
						// to a column header that is disembodied and being used for a drag-and-drop operation.
						DynamicReport.SetIsPopup(frameworkElement, true);

					}

					canvas.Children.Add(frameworkElement);

				}

				// The content of this Popup is a version of the disembodied column header.
				this.Child = canvas;

			}

		}

		/// <summary>
		/// Gets or sets the magnification factor used for this Popup.
		/// </summary>
		public double Scale
		{

			get { return this.scaleTransform.ScaleX; }
			set
			{
				this.scaleTransform = new ScaleTransform(value, value);
				this.RenderTransform = this.scaleTransform;
			}

		}

		/// <summary>
		/// Gets or sets the location of the Column Header Popup.
		/// </summary>
		public Point Location
		{

			get { return new Point(this.HorizontalOffset + this.Width / 2.0, this.VerticalOffset + (this.Height - 4)); }
			set
			{
				this.HorizontalOffset = value.X - this.Width / 2.0;
				this.VerticalOffset = value.Y - (this.Height - 4);
			}

		}

	}

}
