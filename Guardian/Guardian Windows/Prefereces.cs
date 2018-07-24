namespace FluidTrade.Guardian.Windows
{

    using System;

	/// <summary>
	/// Summary description for UserPrefereces.
	/// </summary>
	public class Preferences : System.ComponentModel.Component
	{

		private static object timeInForceCode = null;
		private static object blotterId = null;
		private static object brokerId = null;
		private static Pricing pricing = Pricing.Last;

		/// <summary>
		/// The identifier of this user.
		/// </summary>
		public static Guid UserId
		{
			get
			{

				return Guid.Empty;

			}

		}
		/// <summary>
		/// Time in Force code.
		/// </summary>
		public static int TimeInForceCode
		{
			get {return (int)Preferences.TimeInForceCode;}
			set {Preferences.TimeInForceCode = value;}
		}
		/// <summary>
		/// Blotter Id.
		/// </summary>
		public static Guid BlotterId
		{
			get {return (Guid)Preferences.BlotterId;}
			set {Preferences.blotterId = value;}
		}
		/// <summary>
		/// Broker
		/// </summary>
		public static Guid BrokerId
		{
			get {return (Guid)Preferences.brokerId;}
			set {Preferences.BrokerId = value;}
		}
		/// <summary>
		/// Pricing
		/// </summary>
		public static Pricing Pricing
		{
			get {return Preferences.pricing;}
			set {Preferences.pricing = value;}
		}

		/// <summary>
		/// Checks if TimeInForce is assigned.
		/// </summary>
		/// <returns></returns>
		public static Boolean IsTimeInForceCodeNull() { return Preferences.timeInForceCode == null; }

		/// <summary>
		/// Check if we have a valid blotter.
		/// </summary>
		/// <returns></returns>
		public static Boolean IsBlotterIdNull() { return Preferences.blotterId == null; }

	}
}
