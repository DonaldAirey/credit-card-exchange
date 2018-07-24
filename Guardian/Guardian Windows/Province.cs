using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Guardian
{

	/// <summary>
	/// An object representing a province as in the DataModel.
	/// </summary>
	public class Province : IComparable
	{

		private String abbreviation;
		private Guid countryId;
		private String name;
		private Guid provinceId;

		/// <summary>
		/// Create a Province based on a ProvinceRow in the data model.
		/// </summary>
		/// <param name="provinceRow"></param>
		public Province(ProvinceRow provinceRow)
		{

			this.abbreviation = provinceRow.Abbreviation;
			this.countryId = provinceRow.CountryId;
			this.name = provinceRow.Name;
			this.provinceId = provinceRow.ProvinceId;

		}

		/// <summary>
		/// The standard abbreviation of the province.
		/// </summary>
		public String Abbreviation
		{

			get { return this.abbreviation; }

		}

		/// <summary>
		/// The countryId of the country the province is in.
		/// </summary>
		public Guid CountryId
		{

			get { return this.countryId; }

		}

		/// <summary>
		/// The proper name of the province.
		/// </summary>
		public String Name
		{

			get { return this.name; }

		}

		/// <summary>
		/// The provinceId of the province.
		/// </summary>
		public Guid ProvinceId
		{

			get { return this.provinceId; }

		}

		/// <summary>
		/// Compare to provinces by their name.
		/// </summary>
		/// <param name="obj">The other province.</param>
		/// <returns>A indication of how they should be sorted.</returns>
		public int CompareTo(object obj)
		{

			return this.Name.CompareTo((obj as Province).Name);

		}

		/// <summary>
		/// Update the province based on another.
		/// </summary>
		/// <param name="province">The province to copy.</param>
		public void Update(Province province)
		{

			if (this.provinceId == province.ProvinceId)
			{

				this.abbreviation = province.Abbreviation;
				this.countryId = province.CountryId;
				this.name = province.Name;

			}

		}

	}

}
