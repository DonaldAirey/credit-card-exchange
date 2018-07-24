using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.CrystalReports.Engine;

namespace FluidTrade.Reporting
{
	/// <summary>
	/// event args for filling in stream infomation for ReportGenerationItems
	/// NotImplemnted yet
	/// </summary>
    public class ReportStreamEventArgs : EventArgs
    {
        //Stream
		public ReportStreamEventArgs()
		{
			throw new NotImplementedException();
		}
    }

	public delegate void ReportStreamEventHandler(object sender, ReportStreamEventArgs reportConfigureEventArgs);
}
