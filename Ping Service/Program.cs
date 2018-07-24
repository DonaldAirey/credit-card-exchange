using System;
using System.ServiceModel;

namespace StatusMonitor
{
	class Program
	{
		const string USAGE = "PingService.exe -S servername [-U username] [-P password] [-Verbose]";
		static int Main(string[] args)
		{
			bool getServerName = false;
			string serverName = null;
			bool getUserName = false;
			string userName = null;
			string password = string.Empty;
			bool getPassword = false;
			bool verbose = false;
			try
			{

				foreach (string arg in args)
				{
					if (getServerName)
					{
						serverName = arg;
						getServerName = false;
					}
					else if (getUserName)
					{
						userName = arg;
						getUserName = false;
					}
					else if (getPassword)
					{
						password = arg;
						getPassword = false;
					}
					else if (string.Compare(arg, "-S") == 0)
					{
						getServerName = true;
						continue;
					}
					else if (string.Compare(arg, "-U") == 0)
					{
						getUserName = true;
						continue;
					}
					else if (string.Compare(arg, "-P") == 0)
					{
						getPassword = true;
						continue;
					}
					else if (arg.StartsWith("-V"))
					{
						verbose = true;
						continue;
					}
				}

				if (String.IsNullOrEmpty(serverName))
				{
					Console.WriteLine(USAGE);
					return 1;
				}


				GetUserId(serverName, userName, password, verbose);

			}
			catch (EndpointNotFoundException ex)
			{
				if (verbose)
				{
					Console.Error.WriteLine("Server Down");
					Console.Error.WriteLine(ex.Message);
					Console.Error.WriteLine(ex.StackTrace.ToString());
				}
				Console.WriteLine("FAIL");
				return 1;

			}
			catch (System.ServiceModel.Security.MessageSecurityException securityException)
			{
				if (verbose)
				{
					Console.WriteLine("Validation falied. Invalid credentials");
				}

			}
			catch (Exception ex)
			{
				if (verbose)
				{
					Console.Error.WriteLine(ex.ToString());
					Console.Error.WriteLine(ex.Message);
					Console.Error.WriteLine(ex.StackTrace.ToString());
				}
				Console.WriteLine("FAIL");
				return 1;
			}


			Console.WriteLine("Success");
			return 0;
		}

		private static void GetUserId(string serverName, string userName, string password, bool verbose)
		{
			Uri tcpUri = new Uri(serverName);

			NetTcpBinding binding = new NetTcpBinding(SecurityMode.TransportWithMessageCredential);
			binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

			if (verbose)
				Console.WriteLine("Connecting to: " + serverName);

			EndpointAddress address = new EndpointAddress(tcpUri);

			TradingSupportClient client = new TradingSupportClient(binding, address);
			client.ClientCredentials.UserName.UserName = userName;
			client.ClientCredentials.UserName.Password = password;

			Guid result = client.GetUserId();

			if(verbose)
				Console.WriteLine("UserId: " + result.ToString());
		}
		
	}
}
