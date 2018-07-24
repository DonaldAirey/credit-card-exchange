namespace FluidTrade.Core
{

	using System;

    /// <summary>
	/// Summary description for WebPage.
	/// </summary>
	public class WebPage
	{

		public readonly System.Uri Uri;

		public WebPage(params object[] parameter)
		{

			try
			{

				// Initialize the object.
				this.Uri = new System.Uri((string)parameter[0]);

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

			}

		}

	}

}
