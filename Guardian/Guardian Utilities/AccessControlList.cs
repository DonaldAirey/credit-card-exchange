namespace FluidTrade.Guardian
{

	using FluidTrade.Guardian.Windows;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Threading;
	using System.Xml.Linq;
	using System.Threading;
	
	class AccessControlList
	{

		// Private Constants
		private const String DefaultDirectory = @"%USERPROFILE%\Documents\Visual Studio 2008\Projects\Fluid Trade\Guardian\Database\Data";
		private const string DefaultFileName = "Access Control.xml";

		/// <summary>
		/// Create working orders.
		/// </summary>
		/// <param name="sender">The generic thread initialization parameter.</param>
		public static void CreateAccessControlList(object state)
		{

			XDocument xDocument = new XDocument();

			lock (DataModel.SyncRoot)
			{

				//<script name="Automatically Generated Orders">
				XElement elementRoot = new XElement("script", new XAttribute("name", "Access Control List"));
				xDocument.Add(elementRoot);

				//  <client name="DataModelClient" type="DataModelClient, Client Data Model" endpoint="TcpDataModelEndpoint" />
				elementRoot.Add(
					new XElement("client",
						new XAttribute("name", "DataModelClient"),
						new XAttribute("type", "DataModelClient, Client Data Model"),
						new XAttribute("endpoint", "TcpDataModelEndpoint")));

				foreach (UserRow userRow in DataModel.User)
				{

					//  <transaction>
					XElement elementTransaction = new XElement("transaction");
					elementRoot.Add(elementTransaction);

					foreach (EntityTreeRow childRelationRow in userRow.EntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
						CreateAccessControlItem(elementTransaction, userRow, childRelationRow.EntityRowByFK_Entity_EntityTree_ChildId);
					
				}

				// Fill out the file name with a default directory and an extension if they are required before saving the
				// generated orders.
				xDocument.Save(Environment.ExpandEnvironmentVariables(Path.Combine(AccessControlList.DefaultDirectory, AccessControlList.DefaultFileName)));

			}

		}

		private static void CreateAccessControlItem(XElement elementTransaction, UserRow userRow, EntityRow childRow)
		{

			//    <method name="CreateWorkingOrderEx" client="DataModelClient">
			XElement elementAccessControlItem = new XElement(
				"method",
				new XAttribute("name", "CreateAccessControlEx"),
				new XAttribute("client", "DataModelClient"));
			elementTransaction.Add(elementAccessControlItem);

			//    <parameter name="accessRights" value="FullControl" />
			elementAccessControlItem.Add(
				new XElement("parameter",
					new XAttribute("name", "accessRights"),
					new XAttribute("value", "FullControl")));

			//      <parameter name="configurationId" value="Default" />
			elementAccessControlItem.Add(
				new XElement("parameter",
					new XAttribute("name", "configurationId"),
					new XAttribute("value", "Default")));

			//    <parameter name="entityKey" value="ATLAS BANK" />
			elementAccessControlItem.Add(
				new XElement("parameter",
					new XAttribute("name", "entityKey"),
					new XAttribute("value", childRow.ExternalId0)));

			//    <parameter name="userKey" value="ADMINISTRATOR" />
			elementAccessControlItem.Add(
				new XElement("parameter",
					new XAttribute("name", "userKey"),
					new XAttribute("value", userRow.EntityRow.ExternalId0)));

			foreach (EntityTreeRow childRelationRow in childRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
				CreateAccessControlItem(elementTransaction, userRow, childRelationRow.EntityRowByFK_Entity_EntityTree_ChildId);

		}

	}

}
