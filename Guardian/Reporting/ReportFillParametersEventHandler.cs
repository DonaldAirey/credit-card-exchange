using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.CrystalReports.Engine;
using FluidTrade.Reporting.Interfaces;
using System.Collections;

namespace FluidTrade.Reporting
{
    /// <summary>
    /// and parameters (if parameters not set still contains a list of param names)
    /// </summary>
    public class ReportFillParameterEventArgs : EventArgs
    {
        /// <summary>
        /// list of child nodes
        /// </summary>
        private List<ReportFillParameterEventArgs> childList;

        /// <summary>
        /// name of the parameter to fill
        /// </summary>
        private string name;

        /// <summary>
        /// parent or container of the parmater to fill. In the case of a path
        /// the tree looks like 
        ///     cursor position
        ///         (child)cursor position
        ///             (child)cursor position
        ///                 DataToGet1
        ///                 DataToGet2
        ///                 DataToGetN
        /// </summary>
        private ReportFillParameterEventArgs parent;

        /// <summary>
        /// path type of parameter
        /// </summary>
        private PathType pathType;

        /// <summary>
        /// the IStaticReport instance for that is to be configured. 
        /// </summary>
        private IStaticReport report;

        /// <summary>
        /// List of values for the parameter
        /// </summary>
        private List<ReportParameterValue> values;


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="report">IStaticReport that this parameter is for</param>
        /// <param name="name">name of the parameter</param>
        /// <param name="values">List of values that is to be populated, or possibly changed if the
        /// IStaticReport has provided default values</param>
        /// <param name="pathType">is this parameter a path and if so what type of path node</param>
        /// <param name="parent">parent parameter</param>
        public ReportFillParameterEventArgs(IStaticReport report,
            string name, List<ReportParameterValue> values,
            PathType pathType, ReportFillParameterEventArgs parent)
        {
            this.name = name;
            this.report = report;
            this.pathType = pathType;
            this.values = values;

            //if this has a parent set up the relationship
            if (parent != null)
            {
                this.parent = parent;
                if (this.parent.childList == null)
                    this.parent.childList = new List<ReportFillParameterEventArgs>();

                this.parent.childList.Add(this);
            }
        }

        /// <summary>
        /// List of values for the parameter. Should add values to list
        /// using the AddValue()
        /// </summary>
        public List<ReportParameterValue> Values
        {
            get
            {
                return this.values;
            }
        }

        /// <summary>
        /// Get a list of values from the parent ReportFillParameter
        /// null if no parent
        /// </summary>
        public List<ReportParameterValue> ParentValues
        {
            get
            {
                if (this.parent == null)
                    return null;

                return this.parent.values;
            }
        }

        /// <summary>
        /// get the PathType of the ReportFillParameter
        /// </summary>
        public PathType PathType
        {
            get
            {
                return this.pathType;
            }
        }

        /// <summary>
        /// get the name of the ReportFillParameter
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Get the list of children of this ReportFillParameter
        /// </summary>
        public List<ReportFillParameterEventArgs> Children
        {
            get
            {
                return this.childList;
            }
        }

        /// <summary>
        /// Get the first child, null if no children
        /// </summary>
        public ReportFillParameterEventArgs FirstChild
        {
            get
            {
                if (this.childList != null && this.childList.Count != 0)
                    return this.childList[0];
               
                return null;
            }
        }

        /// <summary>
        /// add a value
        /// </summary>
        /// <param name="val"></param>
        public void AddValue(ReportParameterValue val)
        {
            if(this.values == null)
                this.values = new List<ReportParameterValue>();
            
            this.values.Add(val);
        }

        /// <summary>
        /// add a range of values
        /// </summary>
        /// <param name="list"></param>
        public void AddValues(IEnumerable<ReportParameterValue> list)
        {
            foreach (ReportParameterValue val in list)
                this.AddValue(val);
        }
    }

    /// <summary>
    /// class that holds the values for a parameter. 
    /// If the value is a range Value is startValue and EndValue is ...
    /// </summary>
    public class ReportParameterValue
    {
        /// <summary>
        /// end value of a range
        /// </summary>
        private Object endVal;
        
        /// <summary>
        /// is this value a range
        /// </summary>
        private Boolean isRange;

        /// <summary>
        /// an object that can be set by consumer for custom purposes
        /// </summary>
        private Object userObject;

        /// <summary>
        /// value. in the case of a range startValue
        /// </summary>
        private Object val;


        /// <summary>
        /// default ctor
        /// </summary>
        public ReportParameterValue()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="val">value of parameter</param>
        /// <param name="userObject">custom user object</param>
        public ReportParameterValue(Object val, Object userObject)
        {
            this.val = val;
            this.userObject = userObject;
        }

        /// <summary>
        /// get or set if this value is a range
        /// </summary>
        public Boolean IsRange { get { return this.isRange; } set { this.isRange = value; } }

        /// <summary>
        /// get or set value, in case of range this is StartValue
        /// </summary>
        public Object Value { get { return this.val; } set { this.val = value; } }

        /// <summary>
        /// get or set end value of range
        /// </summary>
        public Object EndValue { get { return this.endVal; } set { this.endVal = value; } }

        /// <summary>
        /// get or set custom user object
        /// </summary>
        public Object UserObject { get { return this.userObject; } set { this.userObject = value; } }
    }

    public delegate void ReportFillParameterEventHandler(object sender, ReportFillParameterEventArgs fillParameterEventArgs);

    /// <summary>
    /// enumeration to define if ReportFillParameterEventArgs is a path and if so 
    /// what type of path node type
    /// </summary>
    public enum PathType
    {
        None,
        /// <summary>
        /// move cursor on SelectedItem
        /// </summary>
        SelectedItem,

        /// <summary>
        /// move cursor on selected folder
        /// </summary>
        SelectedFolder,

        /// <summary>
        /// move cursor up one parent
        /// </summary>
        FolderParent,

        /// <summary>
        /// move cursor to root of tree
        /// </summary>
        FolderRoot,

        /// <summary>
        /// get the folder that the cursor is pointing at
        /// </summary>
        CursorFolder,
        
        /// <summary>
        /// get the immediate child folders of the folder that the cursor is pointing at
        /// </summary>
        CursorChildFolders,

        /// <summary>
        /// get the items in the folder that the cursor is pointing at
        /// </summary>
        CursorItems,

        /// <summary>
        /// get all the descendant folders of the folder that the cursor is pointing at
        /// </summary>
        CursorDescendantFolders,

        /// <summary>
        /// get all the itmes in all the descendant folders of the folder that the cursor is pointing at
        /// </summary>
        CursorDescendantItems,           
       
        /// <summary>
        /// denotes path, used for root level path tree
        /// </summary>
        Path
    }
}
