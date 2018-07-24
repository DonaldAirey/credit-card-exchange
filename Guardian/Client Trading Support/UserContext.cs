using System;
using FluidTrade.Core;
using FluidTrade.Guardian.AdminSupportReference;
using System.Diagnostics;
using System.Reflection;

namespace FluidTrade.Guardian
{
    //Evanescent user configuration settings.
    public sealed class UserContext
    {

        //Eager initialization since we will need it
        public static readonly UserContext Instance = new UserContext();
        private readonly UserConfiguration userConfiguration = new UserConfiguration();
		private Guid userId = Guid.Empty;

		 ///<summary>
		 ///C# compiler marks "beforefieldinit" on classes with Static constructor ensuring lazy initiailization.,
		 ///</summary>
        static UserContext()
        {
        }

        //private constructor for Singleton
        private UserContext() 		
		{
			AdminSupportClient adminSupportClient = new AdminSupportClient(Guardian.Properties.Settings.Default.AdminSupportEndpoint);
			ServerPath = adminSupportClient.Endpoint.Address.Uri.Host;

			ClientVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();	
		}


		/// <summary>
		/// Version of the client
		/// </summary>
		public String ClientVersion
		{
			get;
			private set;
		}

		/// <summary>
		///  Get ServerPath
		/// </summary>
		public String ServerPath
		{
			get;
			private set;
		}

		 ///<summary>
		 ///Get Server Name
		 ///</summary>
        public String ServerVersion
        {
            get
            {
                if (this.userConfiguration.UsercontextData != null)
                    return this.userConfiguration.UsercontextData.ServerVersion;
                else
                    return String.Empty;
            }
        }

		 ///<summary>
		 ///Get user Id
		 ///</summary>
        public Guid UserId
        {
            get
            {
				return userId;
            }
			set
			{
				userId = value;
			}
        }

         /// <summary>
         ///  Get user Name
         /// </summary>
        public String UserName
        {
            get
            {
				if (this.userConfiguration.UsercontextData != null)
					return this.userConfiguration.UsercontextData.FullName;
				else
					return String.Empty;
            } 
        }

		///<summary>
		 ///Inner type
		 ///</summary>
        class UserConfiguration
        {
            private UserContextData userContextData = null;

            internal UserContextData UsercontextData
            {
                get
                {
                    if (this.userContextData == null)
                    {
                        //Webservice is expensive so do this only once.
                        lock (this)
                        {
                            //Lock check.  Make sure another thread did not update userContextData while waiting for lock.
                            if (this.userContextData == null)
                            {
                                this.userContextData = new UserContextData();

                                // Get the context.
                                AdminSupportClient adminSupportClient = new AdminSupportClient(Guardian.Properties.Settings.Default.AdminSupportEndpoint);
                                try
                                {
                                    //Replace this with one call to get all the configuration settings.
                                    MethodResponseUserContextData response = adminSupportClient.GetUserContext();
                                    if (response.IsSuccessful)
                                        this.userContextData = response.Result;
                                    else
                                        this.userContextData = null;

                                }
                                catch (Exception exception)
                                {
                                    this.userContextData = null;
                                     //Any issues trying to communicate to the server are logged.
                                    FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
                                }
                                finally
                                {
                                     //At this point the client can be shut down gracefully.
									if(adminSupportClient.State == System.ServiceModel.CommunicationState.Opened)
										adminSupportClient.Close();
                                }
                            }
                        }
                    }

                    return this.userContextData;
                }
            }
        }
    }
}
