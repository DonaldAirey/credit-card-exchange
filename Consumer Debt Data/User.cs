
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

    class User
    {

        public static XElement CreateEntityRecord(DataSetDebtDataGenerator.UserRow userRow)
        {

            //<method name="CreateEntityEx" client="DataModelClient">
            XElement methodElement = new XElement("method", new XAttribute("name", "CreateEntityEx"), new XAttribute("client", "DataModelClient"));

            //  <parameter name="configurationId" value="Default" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "configurationId"), new XAttribute("value", "Default")));

			//  <parameter name="createdTime" value="10/12/009 12:42:02" />
			methodElement.Add(new XElement("parameter", new XAttribute("name", "createdTime"), new XAttribute("value", DateTime.UtcNow.ToString())));

            //  <parameter name="externalId0" value="ANNIE LENNOX" />
            methodElement.Add(new XElement("parameter",
                        new XAttribute("name", "externalId0"),
                        new XAttribute("value", userRow.ExternalId)));

            //  <parameter name="groupPermission" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "groupPermission"), new XAttribute("value", "0")));

            //  <parameter name="hidden" value="false" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "hidden"), new XAttribute("value", "False")));

            //  <parameter name="imageKey" value="OBJECT" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "imageKey"), new XAttribute("value", "OBJECT")));

			//  <parameter name="modifiedTime" value="10/12/009 12:42:02" />
			methodElement.Add(new XElement("parameter", new XAttribute("name", "modifiedTime"), new XAttribute("value", DateTime.UtcNow.ToString())));

            //  <parameter name="name" value="Annie Lennox" />
            methodElement.Add(new XElement("parameter",
                        new XAttribute("name", "name"),
                        new XAttribute("value", userRow.Name)));

            //  <parameter name="owner" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "owner"), new XAttribute("value", "0")));

            //  <parameter name="ownerPermission" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "ownerPermission"), new XAttribute("value", "0")));

            //  <parameter name="readOnly" value="false" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "readOnly"), new XAttribute("value", "False")));

            //  <parameter name="typeKey" value="USER" />
            methodElement.Add(new XElement("parameter",
                        new XAttribute("name", "typeKey"),
                        new XAttribute("value", "USER")));

            //  <parameter name="worldPermission" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "worldPermission"), new XAttribute("value", "0")));

            //</method>
            return methodElement;

        }

        public static XElement CreateUserRecord(DataSetDebtDataGenerator.UserRow userRow)
        {

            //<method name="CreateUserEx" client="DataModelClient">
            XElement methodElement = new XElement("method",
                    new XAttribute("name", "CreateUserEx"),
                    new XAttribute("client", "DataModelClient"));

            //  <parameter name="configurationId" value="Default" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "configurationId"), new XAttribute("value", "Default")));

            //  <parameter name="entityKey" value="ANNIE LENNOX" />
            methodElement.Add(new XElement("parameter",
                        new XAttribute("name", "entityKey"),
                        new XAttribute("value", userRow.ExternalId)));

            //</method>
            return methodElement;

        }

    }
}
