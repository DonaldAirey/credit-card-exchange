namespace FluidTrade.Guardian.Windows
{

    using System.Windows.Input;

    /// <summary>
    /// Commands shared by dialog boxes.
    /// </summary>
    public class DialogCommands
    {

        /// <summary>
        /// Command sent when the "Ok" button is clicked.
        /// </summary>
        public static readonly RoutedUICommand Okay = new RoutedUICommand("OK", "Okay", typeof(DialogCommands));
        /// <summary>
        /// Command sent when the "Cancel" button is clicked.
        /// </summary>
        public static readonly RoutedUICommand Cancel = new RoutedUICommand("Cancel", "Cancel", typeof(DialogCommands));
        /// <summary>
        /// Command sent when the "Apply" button is clicked.
        /// </summary>
        public static readonly RoutedUICommand Apply = new RoutedUICommand("Apply", "Apply", typeof(DialogCommands));

    }

}
