namespace FluidTrade.Core.Windows
{

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

	/// <summary>
	/// A movable image of a drag and drop object.
	/// </summary>
	public class DraggedAdorner : Adorner
	{

		// Private Constants
		private const Double opacity = 0.84;

		// Private Instance Fields
		private ContentPresenter contentPresenter;
		private Point offset;

		/// <summary>
		/// Creates a graphical element that follows the cursor during drag and drop operations.
		/// </summary>
		/// <param name="dragDropData">The object to be dragged and dropped.</param>
		/// <param name="dataTemplate">A template describing the object being dragged.</param>
		/// <param name="uiElement">The user interface element which is adorned.</param>
		public DraggedAdorner(UIElement uiElement, Object dragDropData, DataTemplate dataTemplate)
			: base(uiElement)
		{

			// Initialize the object.
			this.contentPresenter = new ContentPresenter();
			this.contentPresenter.Opacity = DraggedAdorner.opacity;
			this.contentPresenter.Content = dragDropData;
			this.contentPresenter.ContentTemplate = dataTemplate;

		}

		/// <summary>
		/// Gets or sets the position of the dragged adorner.
		/// </summary>
		public Point Offset
		{
			get { return this.offset; }
			set
			{
				this.offset = value;
				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement);
				adornerLayer.Update(this.AdornedElement);
			}
		}

		/// <summary>
		/// Gets the number of visual child elements within this element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get { return 1; }
		}

		/// <summary>
		/// Implements any custom measuring behavior for the adorner.
		/// </summary>
		/// <param name="constraint">A size to constrain the adorner to.</param>
		/// <returns>A Size object representing the amount of layout space needed by the adorner.</returns>
		protected override Size MeasureOverride(Size constraint)
		{

			// An adorner has no default measuring logic to override so the ContentPresenter provides the size of this adorner.
			this.contentPresenter.Measure(constraint);
			return this.contentPresenter.DesiredSize;

		}

		/// <summary>
		/// When overridden in a derived class, positions child elements and determines a size for a FrameworkElement derived class. 
		/// </summary>
		/// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
		/// <returns>The actual size used.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{

			// An adorner has no default arrangment logic to override so the ContentPresenter arranges provides this function.
			this.contentPresenter.Arrange(new Rect(finalSize));
			return finalSize;

		}

		/// <summary>
		/// Overrides Visual.GetVisualChild, and returns a child at the specified index from a collection of child elements.
		/// </summary>
		/// <param name="index">The zero-based index of the requested child element in the collection.</param>
		/// <returns>The requested child element. This should not return a null reference; if the provided index is out of range,
		/// an exception is thrown.</returns>
		protected override Visual GetVisualChild(int index)
		{

			// The adorner has no native logic for managing visual children.  However, this adorner has a single child which is
			// easily provided.
			if (index > 0)
				throw new ArgumentException("Index out of range");
			return this.contentPresenter;

		}

		/// <summary>
		/// Returns a Transform for the adorner, based on the transform that is currently applied to the adorned element.
		/// </summary>
		/// <param name="transform">The transform that is currently applied to the adorned element.</param>
		/// <returns>A transform to apply to the adorner.</returns>
		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{

			// Adding the current position to the existing transforms makes the adorner appear to follow the mouse.
			GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
			generalTransformGroup.Children.Add(base.GetDesiredTransform(transform));
			generalTransformGroup.Children.Add(new TranslateTransform(this.offset.X - this.ActualWidth * 0.5, this.offset.Y - this.ActualHeight * 0.875));
			return generalTransformGroup;

		}

	}

}
