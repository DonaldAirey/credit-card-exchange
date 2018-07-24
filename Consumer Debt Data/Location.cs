namespace FluidTest
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Xml.Linq;

	public struct Location
	{

		// Public instance Fields
		public String City;
		public String ProvinceCode;
		public String PostalCode;

		/// <summary>
		/// Create a objec that holds information about the location of a consumer.
		/// </summary>
		/// <param name="city">The city where they live.</param>
		/// <param name="state">The state/province where they live.</param>
		/// <param name="postalCode">The location code where they live.</param>
		public Location(String city, String provinceCode, String postalCode)
		{

			// Initialize the object
			this.City = city;
            this.ProvinceCode = provinceCode;
			this.PostalCode = postalCode;

		}

	}

}
