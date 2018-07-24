using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FluidTrade.Reporting.Windows
{
    /// <summary>
    /// helper class that brings up an open file window with Reporting Defaults
    /// </summary>
	public static class WindowPickReportFromDisk
    {
        /// <summary>
        /// show OpenFile dialog return fileName on OK; null on cancel
        /// </summary>
        /// <returns></returns>
        public static string PickReport()
        {
            OpenFileDialog open = new OpenFileDialog();

            open.AddExtension = false;
			open.AutoUpgradeEnabled = true;
			open.CheckFileExists = true;
			open.CheckPathExists = true;
			open.DefaultExt = ".rpt";
			open.DereferenceLinks = true;
			open.Filter = "Crystal Reports|*.rpt";
			open.Multiselect = false;
			open.SupportMultiDottedExtensions = true;
            //TODO:!!!RM LOCALIZE
            open.Title = "Pick Report Template";

			if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return open.FileName;

            return null;
        }

		/// <summary>
		/// show OpenFile dialog return fileName on OK; null on cancel
		/// </summary>
		/// <returns></returns>
		public static string PickTranslation()
		{
			OpenFileDialog open = new OpenFileDialog();

			open.AddExtension = false;
			open.AutoUpgradeEnabled = true;
			open.CheckFileExists = true;
			open.CheckPathExists = true;
			open.DefaultExt = ".dll";
			open.DereferenceLinks = true;
			open.Multiselect = false;
			open.SupportMultiDottedExtensions = true;
			//TODO:!!!RM LOCALIZE
			open.Title = "Pick Report Translation";

			if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				return open.FileName;

			return null;
		}
    }
}
