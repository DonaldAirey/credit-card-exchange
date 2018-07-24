namespace FluidTrade.Core.Windows.Utilities
{
    using System.Windows;

    /// <summary>
    /// UI helper extension methods
    /// </summary>
    public static class UIHelper
    {

        /// <summary>
        /// Calculates center poinf of a rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Point Center(this Rect rect)
        {
            Point centerPoint = new Point();
            centerPoint.X = rect.Left + (rect.Width / 2);
            centerPoint.Y = rect.Top + (rect.Height / 2);
            return centerPoint;
        }
    }
}
