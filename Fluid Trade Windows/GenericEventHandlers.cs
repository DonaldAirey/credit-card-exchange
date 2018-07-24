namespace FluidTrade.Core.Windows
{


    /// <summary>
	/// Used to invoke events that require a column definition.
	/// </summary>
	/// <param name="reportColumn"></param>
	public delegate void GenericEventHandler(object sender, GenericEventArgs genericEventArgs);

	public delegate Storyline StorylineHandler(object sender, GenericEventArgs genericEventArgs);

}
