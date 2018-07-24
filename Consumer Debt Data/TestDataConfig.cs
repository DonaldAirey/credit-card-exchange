namespace FluidTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// For now a generic class that contains configuration values that are less prone to be tweaked versus
    /// something in the app.config 
    /// </summary>
    public static class TestDataConfig
    {
        // Misc/Standalone constants

        public const Char RawConsumerDebtDelimiter = '\t';  // used to delimit a string containing raw consumer debt data
        public const Char CsvDelimiter = ',';               // used to delimit strings read in from most of the raw data files.
        public const Char CommentMarker = '#';              // used to denote a commented-out line in the test data

        public const Int32 GenerateRandomNumberOfCreditCards = -1; //flag maps to config setting to generate a random amount of CCs

        // TenantId format (for now anyway) is Organization name appended by "ORGANIZATION"
        public const String TenantKeySuffix = " ORGANIZATION";

        /// <summary>
        /// categories of buildling heights
        /// </summary>
        public static class BuildingFloors
        {
            public const Int32 SkyscraperFloors = 150; // very tall building 
            public const Int32 LowriseFloors = 8;
            public const Int32 ApartmentsPerFloor = 20;
        }


        /// <summary>
        /// Strings used to help denote points in time during the data gen process.
        /// </summary>
        public static class TimeStamps
        {
            public const String Start = "Start";
            public const String BuildConsumerTable = "Build Consumer Table";
            public const String BuildConsumerTrustTable = "Build Consumer Trust Table";
            public const String BuildConsumerDebtTable = "Build Consumer Debt Table";

            public const String WriteScriptLoaderTableXmlFiles = "Write Script Loader Table XML Files";
            public const String WriteWorkingOrderXmlFiles = "Write Working Order XML Files";
            public const String DumpDataSetInXml = "Dump the DataSet in XML format";
            public const String DumpDataSetInBinary = "Dump the DataSet in Binary format";
            public const String DumpMatchData = "Dump the Match Data";
            public const String DumpMiscCountData = "Dump the miscelaneous Count data";
            public const String ShuffleIdLists = "Shuffle the Debt and Trust ID lists";

            public const String WriteFlatInputFiles = "Write Flat Input Files";
            public const String WriteFlatTrustXmlFile = "Write Flat Trust XML File";
            public const String WriteFlatDebtXmlFile = "Write Flat Debt XML File";

            public const String FuzzConsumerTrustAndConsumerDebt = "Fuzz Trust and Debt";
            public const String FuzzConsumerTrust = "Fuzz Trust";
            public const String FuzzConsumerDebt = "Fuzz Debt";
            public const String End = "END";
        }


        /// <summary>
        /// Class that defines the colunn order of data fields we are reading in from the Raw Consumer Debt 
        /// data file.  That file is based on the config setting: ConsumerDebtInput
        /// </summary>
        public static class RawConsumerDebtColumn
        {
            public const Int32 OpenedDate = 4;
            public const Int32 LastPaidDate = 5;
            public const Int32 CollectionDate = 6;
            public const Int32 DateOfDelinquency = 7;
            public const Int32 InterestRate = 11;
        }

        /// <summary>
        /// Description tag for the type of organization
        /// </summary>
        public static class OrganizationType
        {
            public const String DebtNegotiator = "DEBT NEGOTIATOR";
            public const String DebtHolder = "DEBT HOLDER";
        }

        /// <summary>
        /// Class that defines the column order of data fields we are reading in from the
        /// raw organization data file.  That file is based on the config setting:
        /// OrganizationInput
        /// </summary>
        public static class RawOrganizationColumn
        {
            public const Int32 Name = 0;
            public const Int32 Type = 1;

            // the rest of the columns are placeholders for organization-specific debt classes
            public const Int32 DebtClass1 = 2;
            public const Int32 DebtClass2 = 3;
            public const Int32 DebtClass3 = 4;
            public const Int32 DebtClass4 = 5;
            public const Int32 DebtClass5 = 6;
            public const Int32 DebtClass7 = 8;
            public const Int32 DebtClass8 = 9;
            public const Int32 DebtClass9 = 10;
        }

        /// <summary>
        /// 
        /// </summary>
        public static class RawUserColumn
        {
            public const Int32 Name = 0;
            public const Int32 Password = 1;
            public const Int32 Organization = 2;
            public const Int32 Email = 3;
        }

        /// <summary>
        /// 
        /// </summary>
        public static class RawDebtClassColumn
        {
            public const Int32 Name = 0;
            public const Int32 OrganizationType = 1;
        }


        /// <summary>
        /// Mars or Venus?
        /// </summary>
        public enum Gender
        {
            Female = 0, /* ladies first */
            Male = 1
        }

    }
}
