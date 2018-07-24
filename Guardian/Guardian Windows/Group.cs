namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;
using System.Collections.ObjectModel;

	/// <summary>
	/// A group, containing users.
	/// </summary>
	public class Group : RightsHolder
	{

		GroupType groupType;

		/// <summary>
		/// Create a new group based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the data model.</param>
		public Group(EntityRow entityRow)
			: base(entityRow)
		{


		}

		/// <summary>
		/// Create a new group based on an existing group entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public Group(Entity entity)
			: base(entity)
		{

		}

		/// <summary>
		/// Gets the unique identifier of the group.
		/// </summary>
		public Guid GroupId
		{

			get { return this.EntityId; }

		}

		/// <summary>
		/// Gets the type of the group.
		/// </summary>
		public GroupType GroupType
		{

			get { return this.groupType; }

		}

		/// <summary>
		/// Load group specific information.
		/// </summary>
		protected override void FinishLoad()
		{

			GroupRow groupRow = DataModel.Group.GroupKey.Find(this.EntityId);

			base.FinishLoad();

			if (groupRow != null)
				groupType = groupRow.GroupTypeRow.GroupTypeCode;

		}

		/// <summary>
		/// If there's no group row yet, we need to finish loading up the object later.
		/// </summary>
		/// <returns>True if we can't find a group row for the entityId.</returns>
		protected override bool NeedLateLoad()
		{

			return DataModel.Group.GroupKey.Find(this.EntityId) == null;

		}

		/// <summary>
		/// Create the properties window for this object.
		/// </summary>
		/// <returns>The properties window.</returns>
		protected override WindowEntityProperties CreatePropertiesWindow()
		{

			return new WindowGroupProperties();

		}

	}

}
