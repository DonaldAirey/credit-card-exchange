using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using FluidTrade.Actipro;

namespace FluidTrade.Guardian.Windows.Controls
{
	/// <summary>
	/// Interaction logic for MatchPartsUserControl.xaml
	/// </summary>
	public partial class MatchPartsUserControl : UserControl
	{
		private FluidTrade.Actipro.HeatIndexControl acountNumberHeatControl;
		private FluidTrade.Actipro.HeatIndexControl socialSecurityHeatControl;
		private FluidTrade.Actipro.HeatIndexControl lastNameHeatControl;
		private FluidTrade.Actipro.HeatIndexControl firstNameHeatControl;
		private Regex heatIndexDetailsRegex;

		/// <summary>
		/// ctor
		/// </summary>
		public MatchPartsUserControl()
		{
			InitializeComponent();
			//create the accountNumber heat control
			//need to dig into the inner control to set the minimum setting because
			//default is set to 50%
			this.acountNumberHeatControl = new FluidTrade.Actipro.HeatIndexControl();
			//((ActiproSoftware.Windows.Controls.Gauge.LinearGauge)((Grid)this.acountNumberHeatControl.Content).Children[0]).Scales[0].TickSets[0].Minimum = 0F;
			this.accountNumberContentControl.Child = this.acountNumberHeatControl;

			//create the socialSecurity heat control
			//need to dig into the inner control to set the minimum setting because
			//default is set to 50%
			this.socialSecurityHeatControl = new FluidTrade.Actipro.HeatIndexControl();
			//((ActiproSoftware.Windows.Controls.Gauge.LinearGauge)((Grid)this.socialSecurityHeatControl.Content).Children[0]).Scales[0].TickSets[0].Minimum = 0F;
			this.socialSecurityContentControl.Child = this.socialSecurityHeatControl;

			//create the lastName heat control
			//need to dig into the inner control to set the minimum setting because
			//default is set to 50%
			this.lastNameHeatControl = new FluidTrade.Actipro.HeatIndexControl();
			//((ActiproSoftware.Windows.Controls.Gauge.LinearGauge)((Grid)this.lastNameHeatControl.Content).Children[0]).Scales[0].TickSets[0].Minimum = 0F;
			this.lastNameContentControl.Child = this.lastNameHeatControl;

			//create the first heat control
			//need to dig into the inner control to set the minimum setting because
			//default is set to 50%
			this.firstNameHeatControl = new FluidTrade.Actipro.HeatIndexControl();
			//((ActiproSoftware.Windows.Controls.Gauge.LinearGauge)((Grid)this.firstNameHeatControl.Content).Children[0]).Scales[0].TickSets[0].Minimum = 0F;
			this.firstNameContentControl.Child = this.firstNameHeatControl;

			//create parser for the heat details this needs to match \Trading Support\CardSocialLastNameFuzzyMatcher.cs
			//heat index details. at some point move both into a common location
			this.heatIndexDetailsRegex = new Regex(@"(?<header>.*)\,.*\:(?<accountNumWeight>.*)\:(?<accountNumStrength>.*)" +
																	@"\,.*\:(?<ssnWeight>.*)\:(?<socialStrength>.*)" +
																	@"\,.*\:(?<lastNameWeight>.*)\:(?<lastNameStrength>.*)" +
																	@"\,.*\:(?<firstNameWeight>.*)\:(?<firstNameStrength>.*)", RegexOptions.Compiled);

			//string strengthDetails = string.Format("CT1,OA:{0:0.0}:{1:0.0},SS:{2:0.0}:{3:0.0},LN:{4:0.0}:{5:0.0},FN:{6:0.0}:{7:0.0}",
			//                    0.40M * 100M, 0.90M * 100M,
			//                    0.40M * 100M, 0.80M * 100M,
			//                    0.15M * 100M, 0.70M * 100M,
			//                    0.05M * 100M, 0.60M * 100M);

			//SetDetails(strengthDetails);

		}

		/// <summary>
		/// Set the inner controls/labels values base on the HeatIndexDetails string
		/// </summary>
		/// <param name="heatIndexDetails"></param>
		public void SetDetails(string heatIndexDetails)
		{
			MatchCollection mc = this.heatIndexDetailsRegex.Matches(heatIndexDetails);
			if(mc.Count != 1)
				return;

			if(mc[0].Groups.Count != 10) //first index has whole string
				return;


			//extract the account number details
			decimal tmpDecimal;
			string tmp = mc[0].Groups[2].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				this.accountNumberPercentLabel.Content = tmp;
			}

			tmp = mc[0].Groups[3].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				if(decimal.TryParse(tmp, out tmpDecimal))
				{
					this.acountNumberHeatControl.Percent = tmpDecimal / 100M;
				}
			}

			//extract the social security number details
			tmp = mc[0].Groups[4].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				this.socialSecurityPercentLabel.Content = tmp;
			}

			tmp = mc[0].Groups[5].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				if(decimal.TryParse(tmp, out tmpDecimal))
				{
					this.socialSecurityHeatControl.Percent = tmpDecimal / 100M;
				}
			}

			//extract the last name details
			tmp = mc[0].Groups[6].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				this.lastNamePercentLabel.Content = tmp;
			}

			tmp = mc[0].Groups[7].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				if(decimal.TryParse(tmp, out tmpDecimal))
				{
					this.lastNameHeatControl.Percent = tmpDecimal / 100M;
				}
			}

			//extract the first name details
			tmp = mc[0].Groups[8].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				this.firstNamePercentLabel.Content = tmp;
			}

			tmp = mc[0].Groups[9].Value;
			if(string.IsNullOrEmpty(tmp) == false)
			{
				if(decimal.TryParse(tmp, out tmpDecimal))
				{
					this.firstNameHeatControl.Percent = tmpDecimal / 100M;
				}
			}

		}

		/*
		public int AccountNumberWeight
		{
			get
			{
				if(this.accountNumberPercentLabel.Content == null)
					return 0;

				int val;
				if(int.TryParse(this.accountNumberPercentLabel.Content.ToString(), out val) == false)
					return 0;
				
				return val;
			}
			set
			{
				this.accountNumberPercentLabel.Content = value.ToString();
			}
		}

		public int SocialSecurityWeight
		{
			get
			{
				if(this.socialSecurityPercentLabel.Content == null)
					return 0;

				int val;
				if(int.TryParse(this.socialSecurityPercentLabel.Content.ToString(), out val) == false)
					return 0;

				return val;
			}
			set
			{
				this.socialSecurityPercentLabel.Content = value.ToString();
			}
		}

		public int LastNameWeight
		{
			get
			{
				if(this.lastNamePercentLabel.Content == null)
					return 0;

				int val;
				if(int.TryParse(this.lastNamePercentLabel.Content.ToString(), out val) == false)
					return 0;

				return val;
			}
			set
			{
				this.lastNamePercentLabel.Content = value.ToString();
			}
		}

		public int FirstNameWeight
		{
			get
			{
				if(this.firstNamePercentLabel.Content == null)
					return 0;

				int val;
				if(int.TryParse(this.firstNamePercentLabel.Content.ToString(), out val) == false)
					return 0;

				return val;
			}
			set
			{
				this.firstNamePercentLabel.Content = value.ToString();
			}
		}
		*/
	}
}
