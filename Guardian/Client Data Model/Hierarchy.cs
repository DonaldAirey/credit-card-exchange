namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;

    /// <summary>
	/// Summary description for Hierarchy.
	/// </summary>
	public class Hierarchy
	{

		/// <summary>
		/// Determines if a given blotter is visible on the current document.
		/// </summary>
		/// <param name="parentRow">The current object in the recursive search.</param>
		/// <param name="blotterId">The blotter id to be found.</param>
		/// <returns>true if the blotter is part of the tree structure.</returns>
		public static bool IsDescendant(EntityRow parentRow, EntityRow childRow)
		{

			// Don't attempt to search if there is no starting point.
			if (parentRow != null)
			{

				// If the parent is the same as the child, then the original records are related.
				if (parentRow == childRow)
					return true;

				// Recursively search each of the children of the current node.
				foreach (EntityTreeRow entityTreeRow in parentRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
					if (IsDescendant(entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId, childRow))
						return true;

			}

			// At this point, there were no children found on this tree that matched the blotter id.
			return false;

		}

		/// <summary>
		/// Determines if a given blotter is visible on the current document.
		/// </summary>
		/// <param name="parentRow">The current object in the recursive search.</param>
		/// <param name="blotterId">The blotter id to be found.</param>
		/// <returns>true if the blotter is part of the tree structure.</returns>
		public static List<Guid> GetDescendants(EntityRow parentRow)
		{
			lock (DataModel.SyncRoot)
			{
				List<Guid> descendants = new List<Guid>();
				GetDescendants(descendants, parentRow);
				return descendants;
			}

		}

		/// <summary>
		/// Determines if a given blotter is visible on the current document.
		/// </summary>
		/// <param name="parentRow">The current object in the recursive search.</param>
		/// <param name="blotterId">The blotter id to be found.</param>
		/// <returns>true if the blotter is part of the tree structure.</returns>
		private static void GetDescendants(List<Guid> descendants, EntityRow parentRow)
		{

			// Don't attempt to search if there is no starting point.
			if (parentRow != null)
			{

				descendants.Add(parentRow.EntityId);

				// Recursively search each of the children of the current node.
				foreach (EntityTreeRow entityTreeRow in parentRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
					GetDescendants(descendants, entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId);

			}

		}

	}

}
