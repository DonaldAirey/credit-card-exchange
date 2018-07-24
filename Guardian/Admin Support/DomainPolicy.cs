using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace FluidTrade.Guardian
{
	internal class DomainPolicy
	{
		ResultPropertyCollection attribs;

		public DomainPolicy(DirectoryEntry domainRoot)
		{
			string[] policyAttributes = new string[] {
			  "maxPwdAge", "minPwdAge", "minPwdLength", 
			  "lockoutDuration", "lockOutObservationWindow", 
			  "lockoutThreshold", "pwdProperties", 
			  "pwdHistoryLength", "objectClass", 
			  "distinguishedName"
			  };

			DirectorySearcher ds = new DirectorySearcher(
			  domainRoot,
			  "(objectClass=domainDNS)",
			  policyAttributes,
			  SearchScope.Base
			  );

			SearchResult result = ds.FindOne();

			if (result == null)
			{
			  throw new ArgumentException("DomainRoot is not a domainDNS object.");
			}

			this.attribs = result.Properties;
		}

			  
		private long GetAbsValue(object longInt)
		{
			return Math.Abs((long)longInt);
		}

		/// <summary>
		/// Password Age
		/// </summary>
		public TimeSpan MaxPasswordAge
		{
			get
			{
				string val = "maxPwdAge";
				if (this.attribs.Contains(val))
				{
					long ticks = GetAbsValue(
					  this.attribs[val][0]
					  );

					if (ticks > 0)
						return TimeSpan.FromTicks(ticks);
				}

				return TimeSpan.MaxValue;
			}
		}		
  }
   	
}
