using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace FluidTrade.Guardian
{
	public class EmailConfigSettings : ConfigurationSection
	{
		private static EmailConfigSettings settings;

		public static EmailConfigSettings GetSettings()
        {
            if (settings == null)
            {
				EmailConfigSettings toReturn = ConfigurationManager.GetSection(EmailConfigSettings.SectionName)
					as EmailConfigSettings;

                if (toReturn == null)
                {
                    throw new ConfigurationErrorsException(String.Format("Failed to load {0} configuration section",
							EmailConfigSettings.SectionName));
                }

                settings = toReturn;
            }

            return settings;
        }

		public static string SectionName = "FluidTrade.Guardian.EmailSettings";

		[ConfigurationProperty("mailFrom", DefaultValue = "")]
		public string EmailFromAddress
        {
			get { return (string)base["mailFrom"]; }
			set { base["mailFrom"] = value; }
        }

        [ConfigurationProperty("mailServer", DefaultValue = "")]
        public string EmailServer
        {
            get { return (string)base["mailServer"]; }
            set { base["mailServer"] = value; }
        }

        [ConfigurationProperty("mailServerPort", DefaultValue = 0)]
        public int EmailServerPort
        {
            get { return (int)base["mailServerPort"]; }
            set { base["mailServerPort"] = value; }
        }

        [ConfigurationProperty("mailServerUser", DefaultValue = "")]
        public string EmailServerUser
        {
            get { return (string)base["mailServerUser"]; }
            set { base["mailServerUser"] = value; }
        }

        [ConfigurationProperty("mailServerPassword", DefaultValue = "")]
        public string EmailServerPassword
        {
            get { return (string)base["mailServerPassword"]; }
            set { base["mailServerPassword"] = value; }
        }


        [ConfigurationProperty("mailServerRequiresSSL", DefaultValue = false)]
        public bool EmailServerUseSSL
        {
            get { return (bool)base["mailServerRequiresSSL"]; }
            set { base["mailServerRequiresSSL"] = value; }
        }

       
	}
}
