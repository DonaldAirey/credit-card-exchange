namespace FluidTrade.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FluidTrade.Reporting.Interfaces;
    using FluidTrade.Guardian;

    /// <summary>
    /// helper class for some defualt implemention for the ReportGenerationItem handlers
    /// ReportGetData returns DataModel as the dataSource
    ///     and the DataModel syncRoot as the lock object
    /// </summary>
    public class ReportGenerationItemHelper 
    {
        /// <summary>
        /// default ctor
        /// </summary>
        public ReportGenerationItemHelper()
        {
        }

        /// <summary>
        /// fill in the DataSource information for the ReportGetDataSourceEventArgs
        /// set the reportGetDataSourceEventArgs.DataSource to the DataModel.DataSet
        /// and set the reportGetDataSourceEventArgs.DataSourceSyncObject = DataModel.SyncRoot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reportGetDataSourceEventArgs"></param>
        public void ReportGetData(object sender, ReportGetDataSourceEventArgs reportGetDataSourceEventArgs)
        {
            reportGetDataSourceEventArgs.DataSource = DataModel.Tables[0].DataSet;
            reportGetDataSourceEventArgs.DataSourceSyncObject = DataModel.SyncRoot;
        }

        /// <summary>
        /// fill in the Report Parameters. Will fill in the path information. 
        /// At this point the path information is mostly blotter based. Assuming that 
        /// the tree are blotters. This can be changed to be entity based, but some
        /// keys/relations are missing to make the entity lookup efficient
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="defaultId">defualt Id to be used if cannot find match</param>
        /// <param name="dataRow">dataRow that is the current item row</param>
        /// <param name="reportFillParamsArgs">args to be filled in</param>
        public void ReportFillParams(object sender, Guid defaultId, System.Data.DataRow dataRow, ReportFillParameterEventArgs reportFillParamsArgs)
        {
            //trying to keep this generic, but for now folders are == blotters
            
            switch (reportFillParamsArgs.PathType)
            {

                case PathType.FolderRoot://Position cursor to the root folder
                {
                    //walk up parents until the parent is null
                     BlotterRow blotterRow = null;
                    if (reportFillParamsArgs.ParentValues != null &&
                        reportFillParamsArgs.ParentValues.Count > 0)
                    {
                        blotterRow = reportFillParamsArgs.ParentValues[0].UserObject as BlotterRow;
                    }

                    if(blotterRow == null)
                        blotterRow = DataModel.Blotter.Rows.Find(defaultId) as BlotterRow; ;

                    BlotterRow nextBlotterRow = blotterRow;
                    while (nextBlotterRow != null)
                    {
                        nextBlotterRow = GetBlotterFolderParent(reportFillParamsArgs.ParentValues, defaultId);

                        if (nextBlotterRow == null || nextBlotterRow == blotterRow)
                            break;

                        blotterRow = nextBlotterRow;
                    }

                    //found the root
                    if(blotterRow != null)
                        reportFillParamsArgs.AddValue(new ReportParameterValue(blotterRow.BlotterId, blotterRow));

                    break;
                }
                case PathType.FolderParent:
                {
					//position cursor to the parent folder of the current location
                    BlotterRow blotterRow = GetBlotterFolderParent(reportFillParamsArgs.ParentValues, defaultId);

                    if (blotterRow != null)
                        reportFillParamsArgs.AddValue(new ReportParameterValue(blotterRow.BlotterId, blotterRow));

                    break;
                }
                case PathType.SelectedItem:
                {
					//position cursor to selectedItem. will use the
					//PK of that dataRow to define the value of the selected item
                    if (dataRow != null &&
                        dataRow.Table.PrimaryKey != null &&
                        dataRow.Table.PrimaryKey.Length == 1)
                    {
                        reportFillParamsArgs.AddValue(new ReportParameterValue(dataRow[dataRow.Table.PrimaryKey[0]], null));
                    }
                    else
                    {
                        System.Data.DataRow blotterRow = DataModel.Blotter.Rows.Find(defaultId);
                        reportFillParamsArgs.AddValue(new ReportParameterValue(defaultId, blotterRow));

                    }
                    break;
                }
                case PathType.SelectedFolder:
                {
					//position cursor to the Folder of the selected item
                    System.Data.DataRow blotterRow = DataModel.Blotter.Rows.Find(defaultId);
                    reportFillParamsArgs.AddValue(new ReportParameterValue(defaultId, blotterRow));

                    break;
                }
                case PathType.CursorFolder:
                {
					//return the Folder that the cursor is at
                    if (reportFillParamsArgs.ParentValues != null &&
                        reportFillParamsArgs.ParentValues.Count != 0)
                        reportFillParamsArgs.AddValue(reportFillParamsArgs.ParentValues[0]);
                    
                    break;
                }
                case PathType.CursorChildFolders:
                {
					//return the child folders under the cursor position
                    GetBlotterFolderChildren(defaultId, reportFillParamsArgs);
                    break;
                }
            }
        }

		/// <summary>
		/// fills in reportFillParamsArgs with the folders under the current folder
		/// </summary>
		/// <param name="defaultId">folder id to use if cannot find a match</param>
		/// <param name="reportFillParamsArgs">args to fill in</param>
        private void GetBlotterFolderChildren(Guid defaultId, ReportFillParameterEventArgs reportFillParamsArgs)
        {
            BlotterRow blotterRow = null;

			//get the current row out of the parentValue
            if (reportFillParamsArgs.ParentValues != null &&
                        reportFillParamsArgs.ParentValues.Count != 0)
            {
                blotterRow = reportFillParamsArgs.ParentValues[0].UserObject as BlotterRow;
            }

			//if there is not row in parent use the default
            if (blotterRow == null)
                blotterRow = DataModel.Blotter.Rows.Find(defaultId) as BlotterRow;

			//no parent row return
            if (blotterRow == null)
                return;

			//get all the child blotter rows
            foreach (EntityTreeRow entityTreeRow in blotterRow.EntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
                foreach (BlotterRow childRow in entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId.GetBlotterRows())
                {
                    reportFillParamsArgs.AddValue(new ReportParameterValue(childRow.BlotterId, childRow));
                }
        }

		/// <summary>
		/// returns blotter row that is the parent folder of current location
		/// </summary>
		/// <param name="previousValues">previous values that define the current folder</param>
		/// <param name="defaultId">folder id to use if cannot find a match</param>
		/// <returns>blotterRow that is the parent</returns>
        private BlotterRow GetBlotterFolderParent(List<ReportParameterValue> previousValues, Guid defaultId)
        {
			//find the current blotter row
            BlotterRow blotterRow = null;
            if (previousValues != null &&
                previousValues.Count > 0)
            {
                blotterRow = previousValues[0].UserObject as BlotterRow;
            }

            if (blotterRow == null)
                blotterRow = DataModel.Blotter.Rows.Find(defaultId) as BlotterRow;

			//not blotter row return null
            if (blotterRow == null)
                return null;

			//find the parent
            foreach (EntityTreeRow entityTreeRow in blotterRow.EntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId())
            {
                foreach (BlotterRow parentRow in entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId.GetBlotterRows())
                {
                    return parentRow;
                }
            }

            //cant find a parent return
            return null;
        }

		//RM start of some code to get folders by entity tree instead of blotter. 
        //private void GetEntityFolderParent()
        //{
        //    if (reportFillParamsArgs.PreviousValues == null)
        //    {
        //        System.Data.DataRow blotterRow = DataModel.Blotter.Rows.Find(defaultId);
        //        reportFillParamsArgs.AddValue(new ReportParameterValue(defaultId, blotterRow));
        //    }
        //    else
        //    {
        //        //!!!RM ask someone if this dataView exists
        //        // if not store it in the root reportFillParamsArgs so dont have
        //        //to recreate over and over
        //        System.Data.DataView entityDataView =
        //            new System.Data.DataView(DataModel.EntityTree, "",
        //                DataModel.EntityTree.ChildIdColumn.ColumnName, System.Data.DataViewRowState.CurrentRows);

        //        Dictionary<Guid, System.Data.DataRow> parentIdToBlotterRow = new Dictionary<Guid, System.Data.DataRow>();
        //        System.Data.DataColumn parentCol = DataModel.EntityTree.ParentIdColumn;
        //        foreach (ReportParameterValue childValue in reportFillParamsArgs.PreviousValues)
        //        {
        //            //!!!RM the FindRows is returning more than I expect but filtered out of the report
        //            //it would be nice to keep this generic and use the EntityTree table
        //            foreach (System.Data.DataRowView parentRow in entityDataView.FindRows(childValue.Value))
        //            {
        //                //verify this is a blotter
        //                Guid parentGuid = (Guid)parentRow.Row[parentCol];
        //                if (parentIdToBlotterRow.ContainsKey(parentGuid))
        //                    continue;

        //                System.Data.DataRow blotterRow = DataModel.Blotter.Rows.Find(parentGuid);
        //                if (blotterRow != null)
        //                    parentIdToBlotterRow.Add(parentGuid, blotterRow);
        //            }
        //        }

        //        foreach (KeyValuePair<Guid, System.Data.DataRow> pair in parentIdToBlotterRow)
        //        {
        //            reportFillParamsArgs.AddValue(new ReportParameterValue(pair.Key, pair.Value));
        //        }
        //    }
        //}
    }
}
