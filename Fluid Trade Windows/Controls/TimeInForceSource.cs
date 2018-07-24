namespace FluidTrade.Core.Windows.Controls
{

    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class TimeInForceSource : ObservableCollection<KeyValuePair<int, string>>
	{

		public TimeInForceSource()
		{
			this.Add(new KeyValuePair<int, string>(0, "Day"));
			this.Add(new KeyValuePair<int, string>(1, "GTC"));
			this.Add(new KeyValuePair<int, string>(2, "OPG"));
		}

	}

}
