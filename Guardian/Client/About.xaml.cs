using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows;
using FluidTrade.Core;
using FluidTrade.Guardian.AdminSupportReference;
using Timers = System.Timers;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class About : Window
	{

		/// <summary>
		/// 1 second timer for latency calculation
		/// </summary>
		private Timers.Timer timer = new Timers.Timer(1000);
		
		public bool ConnectToServer { get; set; }

		public About()
		{
			InitializeComponent();
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);
			ConnectToServer = false;
		}

		/// <summary>
		/// OnUnloaded handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			timer.Enabled = false;
			timer.Elapsed -= new Timers.ElapsedEventHandler(OnTimerElapsed);
		}

		/// <summary>
		/// OnLoaded handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnLoaded(object sender, RoutedEventArgs e)
		{
			AdminSupportClient adminSupportClient = new AdminSupportClient(Guardian.Properties.Settings.Default.AdminSupportEndpoint);
			this.Title = "About " + FluidTrade.Guardian.Properties.Settings.Default.ApplicationName;

			this.clientVersion.Text = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();
			this.serverName.Text = adminSupportClient.Endpoint.Address.Uri.Host;

			if ( ConnectToServer == false || !ChannelStatus.LoginEvent.WaitOne(0, false) )
			{
				this.userName.Text = "No active server connection";
				this.serverVersion.Text = "No active server connection";
			}
			else
			{
				this.userName.Text = "Retrieving";
				this.serverVersion.Text = "Retrieving";				
			}

			this.latency.Text = "Retrieving";

			//Spin a thread for the web service call
			if ( ConnectToServer != false)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(InitializeServerValues);

			timer.Enabled = true;
			timer.Elapsed += new Timers.ElapsedEventHandler(OnTimerElapsed);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		private void InitializeServerValues(object state)
		{
			
			try
			{
				string userName = UserContext.Instance.UserName;
				string serverVersion = UserContext.Instance.ServerVersion;

				this.Dispatcher.BeginInvoke(new Action(delegate()
				{
					this.userName.Text = userName;
					this.serverVersion.Text = serverVersion;
				}));
			}
			catch (Exception)
			{
			}
		}
		/// <summary>
		/// Timer handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimerElapsed(object sender, Timers.ElapsedEventArgs e)
		{
			try
			{				
				Ping pingSender = new Ping();
				PingOptions options = new PingOptions();

				string serverPath = UserContext.Instance.ServerPath;

				// Use the default Ttl value which is 128,
				// but change the fragmentation behavior.
				options.DontFragment = true;

				// Create a buffer of 32 bytes of data to be transmitted.
				string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
				byte[] buffer = Encoding.ASCII.GetBytes(data);
				int timeout = 1480;
				PingReply reply = pingSender.Send(serverPath, timeout, buffer, options);
				if (reply.Status == IPStatus.Success)
				{
					this.Dispatcher.BeginInvoke(new Action(() =>
						this.latency.Text = reply.RoundtripTime.ToString() + " ms"));

				}
				else if (reply.Status == IPStatus.TimedOut)
				{
					this.Dispatcher.BeginInvoke(new Action(() =>
						this.latency.Text = "Request timed out." ));
				}

			}
			catch (Exception ex)
			{
				//Stop processing on error
				FluidTrade.Core.EventLog.Error(ex);
				this.timer.Enabled = false;
			}

		}

	}
}

