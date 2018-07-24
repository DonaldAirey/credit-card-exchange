namespace FluidTrade.Guardian
{

	using System;
	using FluidTrade.Core;
	using System.ServiceModel;
using System.Threading;
	using System.IO;

	/// <summary>
	/// These are the parsing states used to read the arguments on the command line.
	/// </summary>
	enum ArgumentState { None, FileName, Password, UserName };

	/// <summary>
	/// This object will load the property table from a formatted file.
	/// </summary>
	class Program
	{

		// Private Static Fields
		private static ArgumentState argumentState;
		private static string fileName = null;
		private static string password = "scrappy must die";
		private static string username = "superuser@fluidtrade.com";
		private static ManualResetEvent waitForEndResetEvent;
	
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			try
			{
				bool streamFileContentsBackToClient = false;
				bool logLocks = false;
				bool cleanMatches = false;
				bool matchAll = false;
				bool resetConfig = false;
				waitForEndResetEvent = new ManualResetEvent(false);
				// The command line parser is driven by different states that are triggered by the flags read.  Unless a flag has 
				// been read, the command line parser assumes that it's reading the file name from the command line.
				argumentState = ArgumentState.FileName;

				// Parse the command line for arguments.
				foreach (string argument in args)
				{
					// Decode the current argument into a state change (or some other action).
					if(argument == "-f") { continue; }
					if(argument == "-s") { streamFileContentsBackToClient = true; continue; }
					if(argument == "-l") { logLocks = true; continue; }
					if(argument == "-m") { cleanMatches = true; continue; }
					if (argument == "-ma") { matchAll = true; continue; }
					if (argument == "-rc") { resetConfig = true; continue; }
					if (argument == "-i") { argumentState = ArgumentState.FileName; continue; }
					if (argument == "-p") { argumentState = ArgumentState.Password; continue; }
					if (argument == "-u") { argumentState = ArgumentState.UserName; continue; }

					// The parsing state will determine which variable is read next.
					switch (argumentState)
					{

						case ArgumentState.FileName:
							fileName = Environment.ExpandEnvironmentVariables(argument);
							break;
						case ArgumentState.Password:
							password = Environment.ExpandEnvironmentVariables(argument);
							break;
						case ArgumentState.UserName:
							username = Environment.ExpandEnvironmentVariables(argument);
							break;

					}

					// The default state is to look for the input file name on the command line.
					argumentState = ArgumentState.FileName;

				}

				// Throw a usage message back at the user if no file name was given.
				if (logLocks == false && cleanMatches == false && matchAll == false && resetConfig == false)
				{
					if (fileName == null)
					throw new Exception("Usage: \"Script Loader\" [-u username] [-p password] [-f] -i <FileName>");

					if(Path.IsPathRooted(fileName) == false)
					{
						fileName = Path.Combine(Environment.CurrentDirectory, fileName);
					}
				}
				// Now that the command line arguments have been parsed into the loader, send the data to the server.

				Console.Write("Connecting to: ");

				ChannelStatus.LoginEvent.Set();

				bool hasException = true;
				ServerAdminMessageListner listener = new ServerAdminMessageListner(username, password);
				try
				{
					listener.Open();

					ServerAdminMessageSender sender = new ServerAdminMessageSender(listener.ClientId, username, password);
					try
					{
						if(logLocks == true)
						{
							sender.OutputLocksToLog();
						}
						else if(cleanMatches == true)
						{
							sender.CleanUnpairedMatches();
						}
						else if (matchAll == true)
						{
							sender.RematchAllWorkingOrders();
						}
						else if (resetConfig == true)
						{
							sender.ResetServerConfigs();
						}
						else
						{
							if(sender.SendLoadScriptFile(fileName, streamFileContentsBackToClient) == false)
							{
								hasException = false;
								//error SendLoadScriptFile will write error to console
								return -1;
							}

							waitForEndResetEvent.WaitOne();

							if(listener.StreamDataId == null)
							{
								AsyncMethodResponse retVal = listener.GetAsyncResponseData(listener.PayloadTicket);
								Console.WriteLine(retVal.AsyncResponseData);
							}
							else
							{
								FluidTrade.Guardian.ServerAdminStreamRef.ServerAdminStreamManagerClient streamClient =
												new FluidTrade.Guardian.ServerAdminStreamRef.ServerAdminStreamManagerClient("TcpServerAdminStreamMgrEndpoint");
								streamClient.ClientCredentials.UserName.UserName = username;
								streamClient.ClientCredentials.UserName.Password = password;

								Stream serverStream = streamClient.GetAsyncResponseStreamData(listener.StreamDataId);

								StreamReader streamReader = new StreamReader(serverStream);
								string tmp = streamReader.ReadToEnd();
								Console.WriteLine(tmp);
							}
							//Console.WriteLine(message);
							//if(response.IsSuccessful == false)
							//{
							//    Console.WriteLine("FAILED");
							//}
							//Console.WriteLine(retVal.AsyncResponseData);
						}
						hasException = false;
					}
					finally
					{
						try
						{
							if(hasException == false)
								sender.Dispose();
						}
						catch
						{
						}
					}
				}
				finally
				{
					try
					{
						if(hasException == false)
							listener.Dispose();
					}
					catch
					{
					}
				}
			}
			catch (Exception ex)
			{
				Exception exOut = ex;
				while(exOut != null)
				{
					//// Display the error.
					Console.WriteLine("{0}: {1}", exOut.Message, ex.ToString());
					Console.WriteLine("");

					exOut = exOut.InnerException;
				}
				return -1;
			}

			return 0;

		}

		[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
		protected abstract class ServerAdminBase :IDisposable
		{
			protected string userName;
			protected string password;
			
			protected ServerAdminBase(string userName, string password)
			{
				this.userName = userName;
				this.password = password;

				this.CreateService();
			}

			public abstract void Dispose();
			protected abstract void CreateService();


			public Guid ClientId { get; protected set; }
		}

		[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
		protected class ServerAdminMessageSender : ServerAdminBase
		{
			protected FluidTrade.Guardian.ServerAdminRef.ServerAdminClient serverAdminClient;

			public ServerAdminMessageSender(Guid clientId, string userName, string password)
				:base(userName, password)
			{
				this.ClientId = clientId;
			}

			protected override void CreateService()
			{
				serverAdminClient = new FluidTrade.Guardian.ServerAdminRef.ServerAdminClient("TcpServerAdminEndpoint");
				serverAdminClient.ClientCredentials.UserName.UserName = this.userName;
				serverAdminClient.ClientCredentials.UserName.Password = this.password;
			}

			public override void Dispose()
			{
				serverAdminClient.Close();
			}

			public bool SendLoadScriptFile(string filePath, bool streamBackFile)
			{
				AsyncMethodResponse code;
				code = serverAdminClient.LoadScriptFile(this.ClientId, filePath, streamBackFile);

				if(code != null && code.IsSuccessful == false)
				{
					Console.WriteLine(code.ErrorText);

					return false;
				}

				return true;
			}

			public void OutputLocksToLog()
			{
				serverAdminClient.OutputLocksToLog();
			}

			public void CleanUnpairedMatches()
			{
				serverAdminClient.CleanUnpairedMatches();
			}


			public void RematchAllWorkingOrders()
			{
				serverAdminClient.RematchAllWorkingOrders();
			}

			public void ResetServerConfigs()
			{
				serverAdminClient.ResetServerConfigs();
			}
		}

		[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
		protected class ServerAdminMessageListner : ServerAdminBase, FluidTrade.Guardian.ServerAdminCallbackRef.IServerAdminCallbackManagerCallback
		{
			protected FluidTrade.Guardian.ServerAdminCallbackRef.ServerAdminCallbackManagerClient serverAdminClient;
			
			public ServerAdminMessageListner(string userName, string password)
				:base(userName, password)
			{
				this.userName = userName;
				this.password = password;
			}

			public void Open()
			{
				AsyncMethodResponse response = serverAdminClient.Subscribe(Guid.Empty);
				this.ClientId = response.ClientId;
			}

			public override void Dispose()
			{
				serverAdminClient.Unsubscribe(this.ClientId);
				serverAdminClient.Close();
			}

			private bool lastConsoleWriteIdDot = false;
			public void OnServerMessage(string message, DateTime timestamp, AsyncMethodResponse response)
			{
				if(message == null)
				{
					Console.Write(".");
					lastConsoleWriteIdDot = true;
				}
				else
				{
					if(lastConsoleWriteIdDot == true)
					{
						lastConsoleWriteIdDot = false;
						Console.WriteLine();
					}
					Console.WriteLine(message);
				}
			}

			public void OnPendingResponseObject(string message, string payloadTicket, DateTime timestamp, AsyncMethodResponse response)
			{
				this.Message = message;
				if(payloadTicket != null)
					this.PayloadTicket = new Guid(payloadTicket);

				this.StreamDataId = response.StreamDataId;
				waitForEndResetEvent.Set();
			}

			public string Message { get; private set; }
			
			public Guid PayloadTicket { get; private set; }
			
			public string StreamDataId{ get; private set; }

			protected override void CreateService()
			{
				//base.CreateServerAdminClient();
				InstanceContext context = new InstanceContext(this);
				serverAdminClient = new FluidTrade.Guardian.ServerAdminCallbackRef.ServerAdminCallbackManagerClient(context, "TcpServerAdminMgrEndpoint");

				serverAdminClient.ClientCredentials.UserName.UserName = this.userName;
				serverAdminClient.ClientCredentials.UserName.Password = this.password;

				Console.WriteLine(this.serverAdminClient.Endpoint.Address.Uri.Host);
				
			}

			public AsyncMethodResponse GetAsyncResponseData(Guid payloadTicket)
			{
				AsyncMethodResponse response = serverAdminClient.GetAsyncResponseData(this.ClientId, payloadTicket);

				if (response != null && !response.IsSuccessful)
				{

					Console.WriteLine(response.ErrorText);

				}
				
				return response;
			}

		}
	}

}
