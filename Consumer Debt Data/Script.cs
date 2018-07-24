namespace FluidTest
{

    using System;
    using System.Xml.Linq;

    /// <summary>
    /// XML primitives for building Script Loader script primitives.
    /// </summary>
    class Script
    {

        public static XElement CreateEntityRecord(
                string externalId0, string imageKey, string name, string type)
        {

            //<method name="CreateEntityEx" client="DataModelClient">
            XElement methodElement = new XElement("method", new XAttribute("name", "CreateEntityEx"), new XAttribute("client", "DataModelClient"));

            //  <parameter name="configurationId" value="Default" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "configurationId"), new XAttribute("value", "Default")));

			//  <parameter name="createdTime" value="10/12/009 12:42:02" />
			methodElement.Add(new XElement("parameter", new XAttribute("name", "createdTime"), new XAttribute("value", DateTime.UtcNow.ToString())));

            //  <parameter name="externalId0" value="WINDFALL HOLDINGS" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "externalId0"), new XAttribute("value", externalId0)));

            //  <parameter name="groupPermission" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "groupPermission"), new XAttribute("value", "0")));

            //  <parameter name="hidden" value="false" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "hidden"), new XAttribute("value", "False")));

            //  <parameter name="imageKey" value="OBJECT" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "imageKey"), new XAttribute("value", imageKey)));

			//  <parameter name="modifiedTime" value="10/12/009 12:42:02" />
			methodElement.Add(new XElement("parameter", new XAttribute("name", "modifiedTime"), new XAttribute("value", DateTime.UtcNow.ToString())));

            //  <parameter name="name" value="Annie Lennox" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "name"), new XAttribute("value", name)));

            //  <parameter name="owner" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "owner"), new XAttribute("value", "0")));

            //  <parameter name="ownerPermission" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "ownerPermission"), new XAttribute("value", "0")));

            //  <parameter name="readOnly" value="false" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "readOnly"), new XAttribute("value", "False")));

            //  <parameter name="typeKey" value="USER" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "typeKey"), new XAttribute("value", type)));

            //  <parameter name="worldPermission" value="0" />
            methodElement.Add(new XElement("parameter", new XAttribute("name", "worldPermission"), new XAttribute("value", "0")));

            //</method>
            return methodElement;

        }

        public static XElement CreateParameter(string name, string value)
        {

            return new XElement("parameter",
                        new XAttribute("name", name),
                        new XAttribute("value", value));

        }

        public static XElement CreateMethod(string name, params XElement[] parameters)
        {

            XElement method = new XElement("method",
                    new XAttribute("name", name),
                    new XAttribute("client", "DataModelClient"));

            foreach (XElement param in parameters)
                method.Add(param);

            return method;

        }

        public static XElement CreateTreeRelation(string parent, string child)
        {

            return Script.CreateMethod("CreateEntityTreeEx",
                    Script.CreateParameter("configurationId", "Default"),
                    Script.CreateParameter("entityKeyByChildId", child),
                    Script.CreateParameter("entityKeyByParentId", parent),
                    Script.CreateParameter("externalId0", Guid.NewGuid().ToString()));

        }

    }

}
