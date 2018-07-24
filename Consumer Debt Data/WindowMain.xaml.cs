
namespace FluidTest
{
	using System;
	using System.Data;
    using System.Configuration;
    using System.Threading;
    using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Xml.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Diagnostics;
    using System.Security.AccessControl;
    using System.Runtime.Serialization.Formatters.Binary;
    

	/// <summary>
	/// Interaction logic for WindowMain.xaml
	/// </summary>
	public partial class WindowMain : Window
	{

		// Private Instance Fields
		private List<String> streetNameList;
        private List<String> address2List;
        private List<String> salutationList;
		private List<Location> locationList;
	    private List<String> femaleNameList;
	    private List<String> femaleSalutationList;
        private List<String> maleNameList;
	    private List<String> maleSalutationList;
	    private List<String> lastNameList;
        private List<String> maleSuffixList;
        private List<String> phoneNumberList;
	    private Dictionary<String, String> nicknameList;

        // Cached Hash tables to help speed up searching through records when the tables get really large
        private Dictionary<Int32, Int32> trustSideMapOfRealConsumerIds;   // map TrustSideConsumerId to RealConsumerId
        private Dictionary<Int32, String> debtSideMapOfRealConsumerIds;    // map DebtSideConsumerId to RealConsumerId
	    private Dictionary<Int32, Int32> debtSideMapOfRealCreditCardIds;  // map DebtCreditCardId to RealCreditCardId
        private Dictionary<Int32, Int32> trustSideMapOfRealCreditCardIds; // map TrustCreditCardId to RealCreditCardId

	    private List<Int32> consumerTrustIds; // List of record IDs added to the ConsumerTrust table
        private List<Int32> consumerDebtIds;  // List of record IDs added to the ConsumerDebt table

        // creates a copy of the app.config file into the data destination directory which contains all the test config data 
        private String destConfigFile = Properties.Settings.Default.DataOutputLocation +
                                        Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        
	    private String destTimestampFile = Environment.CurrentDirectory + "\\" + 
                                           Properties.Settings.Default.DataOutputLocation +
	                                       Properties.Settings.Default.TimestampFileOutput;

	    private String destCsvMatchDumpFile = Environment.CurrentDirectory + "\\" + 
                                              Properties.Settings.Default.DataOutputLocation +
	                                          Properties.Settings.Default.CsvMatchDumpFile;

        
        // used to keep track of the durations of large data generation operations
        private DurationTracker durationTracker;
        private Stopwatch stopwatch = new Stopwatch();

        // Datasets used for building the generated test data
        private DataSetDebtDataGenerator dataSetDebtDataGen;
        private DebtNegotiator[] dataSetDebtNegotiators = new DebtNegotiator[Properties.Settings.Default.NumberOfImportFilesPerOrganization];
	    private DebtHolder[] dataSetDebtHolders = new DebtHolder[Properties.Settings.Default.NumberOfImportFilesPerOrganization];


	    /// <summary>
		/// Creates the data files for Debt Holder and Debt Negotiator organizations
		/// </summary>
		public WindowMain()
		{
            try
            {
                // make sure we start with a clean output directory
                ResetOutputDirectory();

                // Initialize the Output file we use to keep track of stats and config data.
                InitializeDataGenStatsOutputFile();

                // Validate the config data
                ValidateConfigurationData();
                
                // used to keep track of the durations of large data generation operations
                durationTracker = new DurationTracker(this.destConfigFile, this.destTimestampFile);

                // Initialize the Data Sets we will use to build the data
                this.dataSetDebtDataGen = new DataSetDebtDataGenerator();

                // init the data 
                for (Int32 i = 0; i < Properties.Settings.Default.NumberOfImportFilesPerOrganization; i++)
                {
                    this.dataSetDebtNegotiators[i] = new DebtNegotiator();
                    this.dataSetDebtHolders[i] = new DebtHolder();
                }

                // Initialize the internal lists/caches of raw data we will use to build the groomed data
                this.streetNameList = new List<String>();
                this.address2List   = new List<String>();
                this.locationList   = new List<Location>();
                this.salutationList = new List<String>();
                this.maleSuffixList = new List<String>();
                this.maleNameList   = new List<String>();
                this.femaleNameList = new List<String>();
                this.lastNameList   = new List<String>();
                this.nicknameList   = new Dictionary<String, String>();
                this.phoneNumberList      = new List<String>();
                this.femaleSalutationList = new List<String>();
                this.maleSalutationList   = new List<String>();

                // Initialize the internal cache/maps that will help us index the Real Consumers 
                // and CreditCards to their Trust and Debt versions. These maps are MUCH more 
                // efficient than iterating through the tables, especially when they get huge 
                // (100K+ records)
                this.debtSideMapOfRealConsumerIds    = new Dictionary<Int32, String>();
                this.debtSideMapOfRealCreditCardIds  = new Dictionary<Int32, Int32>();
                this.trustSideMapOfRealConsumerIds   = new Dictionary<Int32, Int32>();
                this.trustSideMapOfRealCreditCardIds = new Dictionary<Int32, Int32>();

                this.consumerTrustIds = new List<Int32>();
                this.consumerDebtIds  = new List<Int32>();

                // Designer maintained components are managed here. 
                InitializeComponent();

                // Initialize a random number generator.  If we do not use a time based seed , then use
                // the 'constant' specified config value
                RandomNumberGenerator random =
                    new RandomNumberGenerator(Properties.Settings.Default.UseTimeBasedRandomSeed, 
                                              Properties.Settings.Default.RandomNumberGeneratorSeed);

                // 
                // Build cached lists based on the flat files 
                //
                ReadStreetNames();  
                ReadAddress2();     
                ReadLocations();    
                ReadSalutations();  
                ReadNicknames();    
                ReadLastNames();    
                ReadMaleNames();
                ReadMaleSalutations();
                ReadFemaleNames();
                ReadFemaleSalutations();
                ReadMaleSuffixes();
                ReadPhoneNumbers();

                //Build the Credit Card Issuer table
                BuildCreditCardIssuerTable();

                // put a time stamp in the checkpoint file
                this.durationTracker.MarkTimestamp(DateTime.Now, "Start");

                // Build the Consumer Table
                this.stopwatch.Restart();    
                BuildConsumerTable(random);
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.BuildConsumerTable, stopwatch.Elapsed.TotalMinutes);

                //Build the organization table
                BuildOrganizationTable();

                // Build the user table
                BuildUserTable();

                // Build the Consumer Trust table
                this.stopwatch.Restart(); 
                BuildConsumerTrustTable(random);
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.BuildConsumerTrustTable, stopwatch.Elapsed.TotalMinutes);

                // Throw if we need more matches than there are credit cards available to match on.  We can't do this 
                // check until after we build the Trust-Side credit card table since it's dynamically built with a 
                // random amount of credit cards, the number of which is at least the number of trust-side consumers
                if (Properties.Settings.Default.NumberOfMatches > this.dataSetDebtDataGen.TrustCreditCard.Count)
                    throw new SystemException("Number of Matches cannot be greater than TrustCreditCard.Count");

                // Build the Consumer Debt table
                this.stopwatch.Restart();             
                BuildConsumerDebtTable(random);
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.BuildConsumerDebtTable, stopwatch.Elapsed.TotalMinutes);

                // Fuzz both Trust and Debt side data for the same consumer record 
                if (Properties.Settings.Default.CommonFuzzCount > 0)
                {
                    // [skt] TBD
                    this.stopwatch.Restart(); 
                    FuzzConsumerTrustAndConsumerDebt(random);
                    this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.FuzzConsumerTrustAndConsumerDebt, stopwatch.Elapsed.TotalMinutes);
                }

                // Fuzz the Trust-side data 
                if (Properties.Settings.Default.TrustSideFuzzCount > 0)
                {
                    this.stopwatch.Restart(); 
                    FuzzConsumerTrust(random);
                    this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.FuzzConsumerTrust, stopwatch.Elapsed.TotalMinutes);
                }

                // Fuzz the Debt-side data
                if (Properties.Settings.Default.DebtSideFuzzCount > 0)
                {
                    // [skt] TBD
                    this.stopwatch.Restart(); 
                    FuzzConsumerDebt(random);
                    this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.FuzzConsumerDebt, stopwatch.Elapsed.TotalMinutes);
                }

                // Sanity check the match count 
                ValidateMatchCounts();

                // Write the generated and cleaned up values to XML files.
                this.stopwatch.Restart();                
                WriteConsumer();

                // Update the organization table with working order counts based on the number and types 
                // of organizations that are in the table
                UpdateOrganizationTableWorkingOrderCounts();

                // Shuffle the lists of ConsumerTrust and ConsumerDebt IDs. This will allow us to have a 'random' 
                // distribution of the generated Trust and Debt records per Organization
                this.stopwatch.Reset();
                DataGenHelpers.RandomlyShuffleList(this.consumerTrustIds, random);
                DataGenHelpers.RandomlyShuffleList(this.consumerDebtIds, random);
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.ShuffleIdLists, stopwatch.Elapsed.TotalMinutes);

                // As we build the ConsumerDebt and ConsumerTrust output files, we need to build the CreditCard output file in 
                // parallel. This is because (1) we need to keep the links between the credit cards and their respective Debt 
                // and Trust records and (2) the tenant (aka Organization) needs to be associated with both the security record
                // (ConsumerDebt or ConsumerTrust) and the credit cards.
                XDocument creditCardOutputFile = InitializeCreditCardOutputFile();               
                WriteConsumerDebtOutputData(creditCardOutputFile);
                WriteConsumerTrustOutputData(creditCardOutputFile);
                WriteCreditCardDataFile(creditCardOutputFile);                
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.WriteScriptLoaderTableXmlFiles, stopwatch.Elapsed.TotalMinutes);
                
                // Write out the Flattened XML files used to test the Import facility
                this.stopwatch.Reset();
                WriteFlatImportFiles(random);
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.WriteFlatInputFiles, stopwatch.Elapsed.TotalMinutes);

                // Dump out the Match data so we can use it to compare Expected vs Actual results
                this.stopwatch.Restart();
                DumpMatchData();
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.DumpMatchData, stopwatch.Elapsed.TotalMinutes);

                // Dump out the Test Validation Data (Real Consumer, Trust, Debt side tables) DataSetDebtDataGenerator
                DumpTestDataStats(random);

                // put a time stamp in the checkpoint file
                this.durationTracker.MarkTimestamp(DateTime.Now, "End");
            }
            catch (Exception ex)
		    {
		        MessageBox.Show(ex.ToString());
		    }
            finally
		    {
		        WrapUpDataGenStatsOutputFile();

                // copy the generated files over to the project directory tree so we can check 
                // them in as part of the VS Solution
                PullGeneratedDataFilesToProjectDir();

                MessageBox.Show("DONE!");

		        // That's all.  The application can quit now.
		        Application.Current.Shutdown();
		    }
		}


        /// <summary>
        /// Make sure the output file is ready to be written to. 
        /// </summary>
        private void InitializeDataGenStatsOutputFile()
        {
            if (File.Exists(this.destConfigFile))
            {
                File.SetAttributes(this.destConfigFile, FileAttributes.Normal);
                File.Delete(this.destConfigFile);
            }

            // Make a copy of the config file used for this assembly so we can append the info to it. It's a
            // good enough mechanism for keeping track of the data gen info because most of the 
            // parameters for configuring how the data is generated is in this config file.
            File.Copy(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, this.destConfigFile, true);

            using (StreamWriter sw = File.AppendText(this.destConfigFile))
            {
                // Open XML Comment
                sw.WriteLine("");
                sw.WriteLine("<!--");
                sw.WriteLine("");

                // spit out a timestamp so we know roughly when this file was generated
                sw.WriteLine("");
                sw.WriteLine("Timestamp[START]: " + DateTime.Now.ToString());
            }
        }


        /// <summary>
        /// Make sure the output file is 
        /// </summary>
        private void WrapUpDataGenStatsOutputFile()
        {
            using (StreamWriter sw = File.AppendText(this.destConfigFile))
            {
                // spit out a timestamp so we know roughly when this file was generated
                sw.WriteLine("");
                sw.WriteLine("Timestamp[END]: " + DateTime.Now.ToString());

                // dump the timestamps
                this.durationTracker.DumpDurations(sw);

                // close XML Comment
                sw.WriteLine("");
                sw.WriteLine("-->");           
            }
        }


        /// <summary>
        /// Pull copies of the generated data files over from the runtime destination to the 
        /// current VS project directory so they can be checked in as part of this solution. 
        /// If this process is running in a runtime environment, then no copies are done
        /// </summary>
        private void PullGeneratedDataFilesToProjectDir()
        {
            string projectDir = Environment.CurrentDirectory + "\\..\\..\\" + Properties.Settings.Default.DataOutputLocation;
            
            // if this directory exists, pretty good chance we are running on a dev 
            // system rather than a production one
            if (Directory.Exists(projectDir))
            {
                DirectoryInfo di = new DirectoryInfo(Properties.Settings.Default.DataOutputLocation);
                FileInfo[] files = di.GetFiles();

                // clean out the directory
                foreach (FileInfo fi in files)
                {
                    if (File.Exists(projectDir + fi.Name))
                        File.SetAttributes(projectDir + fi.Name, FileAttributes.Normal);

                    File.Copy(fi.FullName, projectDir + fi.Name, true);
                }
            }
        }


        /// <summary>
        /// Makes sure we start with a empty output directory
        /// </summary>
        private void ResetOutputDirectory()
        {
           string outputDir = Environment.CurrentDirectory + "\\" + Properties.Settings.Default.DataOutputLocation;

            // Make sure output directory exists
            if (Directory.Exists(outputDir))
            {
                DirectoryInfo di = new DirectoryInfo(outputDir);
                FileInfo[] files = di.GetFiles();

                // clean out the directory
                foreach (FileInfo fi in files)
                {
                    File.SetAttributes(outputDir + fi.Name,FileAttributes.Normal);
                    File.Delete(outputDir + fi.Name);
                }
            }
            else
            {
                throw new SystemException(string.Format("Directory does not exist: {0}", outputDir ));
            }
        }


        /// <summary>
        /// Dump out the test and config data for this "Data Generation" run
        /// </summary>
        private void DumpTestDataStats(RandomNumberGenerator random)
        {
            if (Properties.Settings.Default.DumpDataSetToXml)
            {
                // Dump out the entire Dataset (and schema) in XML so we have a textual record of what we created 
                this.stopwatch.Restart();
                DumpDataset(SerializationFormat.Xml);
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.DumpDataSetInXml,
                                                    stopwatch.Elapsed.TotalMinutes);
            }

            if (Properties.Settings.Default.DumpDataSetToBinary)
            {
                // Dump out the entire Dataset (and schema) in BINARY
                this.stopwatch.Restart();
                DumpDataset(SerializationFormat.Binary);
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.DumpDataSetInBinary,
                                                    stopwatch.Elapsed.TotalMinutes);
            }


            //
            // Dump out the other information.....seed, config variables, etc...
            //
            if (Properties.Settings.Default.DumpMiscDataGenStats)
            {
                this.stopwatch.Restart();

                // [skt] For now just copy any other settings you want to save into the config file at the bottom.
                // in the future we will probably need to have a more robust solution (DB, Application Settings, etc..)
                using (StreamWriter sw = File.AppendText(this.destConfigFile))
                {
                    sw.WriteLine("");
                    sw.WriteLine("Random Seed: " + random.Seed.ToString());
                    sw.WriteLine("");
                    sw.WriteLine("DebtCreditCard.Count: {0} DebtSideConsumer.Count: {1}",
                                 this.dataSetDebtDataGen.DebtCreditCard.Count,
                                 this.dataSetDebtDataGen.DebtSideConsumer.Count);

                    Int32 consumersWithMultipleCreditCards = 0;

                    // For each RealConsumer in the debtSideMapOfRealConsumerIds, if the count of DebtSideConsumerIds is > 1, then
                    // there is more than one credit card on the debt side associated with this RealConsumer 
                    foreach (KeyValuePair<Int32,String> kvp in this.debtSideMapOfRealConsumerIds)
                    {
                        List<Int32> debtSideIdsPerRealConsumer = DataGenHelpers.GetIdsFromCsvList(kvp.Value);

                        if (debtSideIdsPerRealConsumer.Count == 0) throw new SystemException("Debt side consumer w/out a credit card...WTF!?");

                        if (debtSideIdsPerRealConsumer.Count > 1)
                        {
                            // increment the count of consumers that have > 1 credit card on the debt side 
                            // (we can do this with debt side consumers because there is a 1-1 relationship between them)
                            consumersWithMultipleCreditCards++;

                            sw.WriteLine("   Real SSN: {0} with {1} credit cards on Debt Side | {2} matched",
                                         this.dataSetDebtDataGen.RealConsumer[kvp.Key].SocialSecurityNumber,
                                         debtSideIdsPerRealConsumer.Count,
                                         this.dataSetDebtDataGen.RealConsumer[kvp.Key].MatchCount
                                         );
                        }

                    }

                    sw.WriteLine("Consumers with > 1 Credit Card on Debt Side: {0}", consumersWithMultipleCreditCards);

                    Int32 matchedCreditCardCount = 0;

                    // for each Real CC that is matched, dump out the Real Consumers CCN and SSN
                    sw.WriteLine("");
                    sw.WriteLine("Matched CreditCards (RealCreditCard.IsMatched=True)");

                    foreach (DataSetDebtDataGenerator.RealCreditCardRow realCreditCardRow in this.dataSetDebtDataGen.RealCreditCard)
                    {
                        if (realCreditCardRow.IsMatched)
                        {
                            // keep the noise level low in the config file.  If we have a lot of matches, we can see them in the csv dump file anyway
                            if (++matchedCreditCardCount <= 20)
                            {
                                sw.WriteLine("CCN: {0}, SSN: {1}",
                                             realCreditCardRow.OriginalAccountNumber,
                                             realCreditCardRow.RealConsumerRow.SocialSecurityNumber);
                            }
                        }
                    }
                    sw.WriteLine("* See {0} for a full list of matches",Properties.Settings.Default.CsvMatchDumpFile);

                    // dump the counts
                    sw.WriteLine("");
                    sw.WriteLine("COUNTS");

                    sw.WriteLine("{0} Consumer records created ({1} Trust Side, {2} Debt Side)",
                                 this.dataSetDebtDataGen.TrustSideConsumer.Count +
                                 this.dataSetDebtDataGen.DebtSideConsumer.Count,
                                 this.dataSetDebtDataGen.TrustSideConsumer.Count,
                                 this.dataSetDebtDataGen.DebtSideConsumer.Count);

                    Int32 creditCardsPerHolder;
                    try
                    {
                        creditCardsPerHolder = (Properties.Settings.Default.NumberOfCreditCardsPerConsumer > 0
                                                    ? Properties.Settings.Default.NumberOfCreditCardsPerConsumer
                                                    : Properties.Settings.Default.RandomCreditCardsPerConsumerMax/2);
                    }
                    catch /*avoid divbyzero */
                    {
                        creditCardsPerHolder = 0;
                    }

                    string token = (Properties.Settings.Default.NumberOfCreditCardsPerConsumer > 0
                                        ? "a constant"
                                        : "an average");

                    sw.WriteLine("{0} Credit Cards with {1} number of {2} per consumer",
                                 this.dataSetDebtDataGen.DebtCreditCard.Count +
                                 this.dataSetDebtDataGen.TrustCreditCard.Count,
                                 token,
                                 creditCardsPerHolder);

                    sw.WriteLine("{0} Trust Rows created (from a pool of {1} in the RealConsumer table)",
                                 this.dataSetDebtDataGen.ConsumerTrust.Count,
                                 this.dataSetDebtDataGen.RealConsumer.Count);

                    sw.WriteLine("{0} Trust Credit Cards created",
                                 this.dataSetDebtDataGen.TrustCreditCard.Count);

                    sw.WriteLine("{0} Debt Records created (from a pool of {1} Trust Side Credit cards)",
                                 this.dataSetDebtDataGen.ConsumerDebt.Count,
                                 this.dataSetDebtDataGen.TrustCreditCard.Count);

                    sw.WriteLine("{0} Matches, {1} of which are fuzzed", matchedCreditCardCount,
                                 Properties.Settings.Default.TrustSideFuzzCount +
                                 Properties.Settings.Default.DebtSideFuzzCount +
                                 Properties.Settings.Default.CommonFuzzCount);
                }
                
                this.durationTracker.MarkCheckpoint(TestDataConfig.TimeStamps.DumpMiscCountData, stopwatch.Elapsed.TotalMinutes);
            }
        }

        /// <summary>
        /// Dump the contents of the in-memory dataset
        /// </summary>
        /// <param name="serializationFormat"></param>
        private void DumpDataset(SerializationFormat serializationFormat)
        {
            String outputFile = "GeneratedTestData_FullDump";

            // set the serialization format appropriately
            this.dataSetDebtDataGen.RemotingFormat = serializationFormat;

            switch (serializationFormat)
            {
                case SerializationFormat.Binary:
                    {
                        FileStream fs =
                            new FileStream(Properties.Settings.Default.DataOutputLocation + outputFile + ".dat", FileMode.Create);

                        // Construct a BinaryFormatter and use it to serialize the data to the stream.
                        BinaryFormatter formatter = new BinaryFormatter();
                        try
                        {
                            formatter.Serialize(fs, this.dataSetDebtDataGen);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to serialize. Reason: " + ex.Message);
                            throw;
                        }
                        finally
                        {
                            fs.Close();
                        }
                    
                        break;
                    }

                case SerializationFormat.Xml:
                    {
                        this.dataSetDebtDataGen.WriteXml(new StreamWriter(Properties.Settings.Default.DataOutputLocation + outputFile + ".xml"),
                            XmlWriteMode.WriteSchema);
                    }
                
                    break;
            }
        }

        /// <summary>
        /// Dump the data (actual and contra) out to a Table and/or CSV file so you can inspect it
        /// in SQL and/orExcel to help debug issues on the SUT
        /// </summary>
	    private void DumpMatchData()
	    {
            StreamWriter sw = null;

            try
            {
                // If we want to create a CSV file 
                if (Properties.Settings.Default.DumpMatchDataToCsv)
                {
                    // Make sure we create a clean CSV file
                    if (File.Exists(this.destCsvMatchDumpFile))
                    {
                        File.SetAttributes(this.destCsvMatchDumpFile, FileAttributes.Normal);
                        File.Delete(this.destCsvMatchDumpFile);
                    }

                    // open stream to the output csv file
                    sw = File.CreateText(this.destCsvMatchDumpFile);

                    // Dump out the header row 
                    WriteCsvHeaderRow(sw);
                }

                // for each matched CC, spit out the relevant data to help us compare expected to actual
                foreach (
                    DataSetDebtDataGenerator.RealCreditCardRow realCreditCardRow in
                        this.dataSetDebtDataGen.RealCreditCard)
                {
                    DataSetDebtDataGenerator.DebtCreditCardRow debtCreditCardRow = null;
                    DataSetDebtDataGenerator.TrustCreditCardRow trustCreditCardRow = null;

                    // For every Real Credit Card that is matched
                    if (realCreditCardRow.IsMatched)
                    {
                        // Get the Trust side CC:  If the real credit card Id exists in the map, get its corresponding 
                        // debt side card out of the DebtCreditCard table
                        if (this.trustSideMapOfRealCreditCardIds.ContainsKey(realCreditCardRow.RealCreditCardId))
                        {
                            trustCreditCardRow =
                                this.dataSetDebtDataGen.TrustCreditCard[
                                    this.trustSideMapOfRealCreditCardIds[realCreditCardRow.RealCreditCardId]];
                        }
                        else
                        {
                            // this would be a bad situation. It means we think we had a match and recorded it as such...but we 
                            // never put a credit card in the TrustCreditCar table for it.
                            throw new SystemException(
                                String.Format(
                                    "Matched Real Credit Card ID: {0} does not have a card in the Trust Side table.",
                                    realCreditCardRow.RealCreditCardId));
                        }

                        // Get the Debt side CC:  If the real credit card Id exists in the map, get its corresponding 
                        // debt side card out of the DebtCreditCard table
                        if (this.debtSideMapOfRealCreditCardIds.ContainsKey(realCreditCardRow.RealCreditCardId))
                        {
                            debtCreditCardRow =
                                this.dataSetDebtDataGen.DebtCreditCard[
                                    this.debtSideMapOfRealCreditCardIds[realCreditCardRow.RealCreditCardId]];
                        }
                        else
                        {
                            // this would be a bad situation. It means we think we had a match and recorded it as such...but we 
                            // never put a credit card in the DebtCreditCard table for it.
                            throw new SystemException(
                                String.Format(
                                    "Matched Real Credit Card ID: {0} does not have a card in the Debt Side table.",
                                    realCreditCardRow.RealCreditCardId));
                        }

                        // Write a row of Data to CSV file
                        if (Properties.Settings.Default.DumpMatchDataToCsv)
                        {
                            WriteMatchDataRowToCsv(sw, realCreditCardRow, trustCreditCardRow, debtCreditCardRow);
                        }

                        // Write a row of Data to the Matches table
                        if (Properties.Settings.Default.DumpMatchDataToTable)
                        {
                            WriteMatchDataRowToTable(realCreditCardRow, trustCreditCardRow, debtCreditCardRow);
                        }
                    }
                }
            }
            finally
            {
                // make sure the file stream is closed for the csv file if we created one
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                }
            }
	    }

        /// <summary>
        /// Write a row of matched data 
        /// </summary>
        /// <param name="realCreditCardRow"></param>
        /// <param name="trustCreditCardRow"></param>
        /// <param name="debtCreditCardRow"></param>
        private void WriteMatchDataRowToTable(
            DataSetDebtDataGenerator.RealCreditCardRow realCreditCardRow, 
            DataSetDebtDataGenerator.TrustCreditCardRow trustCreditCardRow, 
            DataSetDebtDataGenerator.DebtCreditCardRow debtCreditCardRow)
        {
            DataSetDebtDataGenerator.MatchesRow matchesRow = 
                this.dataSetDebtDataGen.Matches.NewMatchesRow();

            // Real Data
            matchesRow.RealCreditCardOriginalAccountNumber = realCreditCardRow.OriginalAccountNumber;
            matchesRow.RealConsumerSsn = realCreditCardRow.RealConsumerRow.SocialSecurityNumber;
            matchesRow.RealConsumerFirstName = realCreditCardRow.RealConsumerRow.FirstName;
            matchesRow.RealConsumerLastName = realCreditCardRow.RealConsumerRow.LastName;

            // Trust Data
            matchesRow.TrustCreditCardOriginalAccountNumber = trustCreditCardRow.OriginalAccountNumber;
            matchesRow.TrustConsumerSsn = trustCreditCardRow.TrustSideConsumerRow.SocialSecurityNumber;
            matchesRow.TrustConsumerFirstName = trustCreditCardRow.TrustSideConsumerRow.FirstName;
            matchesRow.TrustConsumerLastName = trustCreditCardRow.TrustSideConsumerRow.LastName;

            matchesRow.TrustCreditCardFuzzedFields = trustCreditCardRow.FuzzedFields;
            matchesRow.TrustCreditCardFuzzMethod = trustCreditCardRow.FuzzMethod;
            matchesRow.TrustConsumerFuzzedFields = trustCreditCardRow.TrustSideConsumerRow.FuzzedFields;
            matchesRow.TrustConsumerFuzzMethod = trustCreditCardRow.TrustSideConsumerRow.FuzzMethod;
            
            // Debt Data
            matchesRow.DebtCreditCardOriginalAccountNumber = debtCreditCardRow.OriginalAccountNumber;
            matchesRow.DebtConsumerSsn = debtCreditCardRow.DebtSideConsumerRow.SocialSecurityNumber;
            matchesRow.DebtConsumerFirstName = debtCreditCardRow.DebtSideConsumerRow.FirstName;
            matchesRow.DebtConsumerLastName = debtCreditCardRow.DebtSideConsumerRow.LastName;

            matchesRow.DebtCreditCardFuzzedFields = debtCreditCardRow.FuzzedFields;
            matchesRow.DebtCreditCardFuzzMethod = debtCreditCardRow.FuzzMethod;
            matchesRow.DebtConsumerFuzzedFields = debtCreditCardRow.DebtSideConsumerRow.FuzzedFields;
            matchesRow.DebtConsumerFuzzMethod = debtCreditCardRow.DebtSideConsumerRow.FuzzMethod;

            // add the row to the table
            this.dataSetDebtDataGen.Matches.AddMatchesRow(matchesRow);
        }


        /// <summary>
        /// Write out a row of CSV data
        /// </summary>
        /// <param name="sw"></param>
        private void WriteMatchDataRowToCsv(
            StreamWriter sw, 
            DataSetDebtDataGenerator.RealCreditCardRow realCreditCardRow, 
            DataSetDebtDataGenerator.TrustCreditCardRow trustCreditCardRow, 
            DataSetDebtDataGenerator.DebtCreditCardRow debtCreditCardRow)
        {
            // Dump out a row of CSV data
            sw.WriteLine("'{0}',{1},{2},{3},'{4}',{5},{6},{7},{8},{9},{10},{11},'{12}',{13},{14},{15},{16},{17},{18},{19}",

                // Real Data
                realCreditCardRow.OriginalAccountNumber,
                realCreditCardRow.RealConsumerRow.SocialSecurityNumber,
                realCreditCardRow.RealConsumerRow.FirstName,
                realCreditCardRow.RealConsumerRow.LastName,

                // Trust Data
                trustCreditCardRow.OriginalAccountNumber,
                trustCreditCardRow.TrustSideConsumerRow.SocialSecurityNumber,
                trustCreditCardRow.TrustSideConsumerRow.FirstName,
                trustCreditCardRow.TrustSideConsumerRow.LastName,
                
                trustCreditCardRow.FuzzedFields,
                trustCreditCardRow.FuzzMethod,
                trustCreditCardRow.TrustSideConsumerRow.FuzzedFields,
                trustCreditCardRow.TrustSideConsumerRow.FuzzMethod,

                // Debt Data
                debtCreditCardRow.OriginalAccountNumber,
                debtCreditCardRow.DebtSideConsumerRow.SocialSecurityNumber,
                debtCreditCardRow.DebtSideConsumerRow.FirstName,
                debtCreditCardRow.DebtSideConsumerRow.LastName,

                debtCreditCardRow.FuzzedFields,
                debtCreditCardRow.FuzzMethod,
                debtCreditCardRow.DebtSideConsumerRow.FuzzedFields,
                debtCreditCardRow.DebtSideConsumerRow.FuzzMethod
                );
        }


        /// <summary>
        /// write a header row to the csv file
        /// </summary>
        /// <param name="sw"></param>
        private void WriteCsvHeaderRow(StreamWriter sw)
        {
            // csv header row 
            sw.WriteLine(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}",

                "Real CCN",
                "Real SSN",
                "Real FN",
                "Real LN",

                "Trust CCN",
                "Trust SSN",
                "Trust FN",
                "Trust LN",

                "TrustCreditCardFuzzFields",
                "TrustCreditCardFuzzMethod",
                "TrustConsumerFuzzFields",
                "TrustConsumerFuzzMethod",

                "Debt CCN",
                "Debt SSN",
                "Debt FN",
                "Debt LN",

                "DebtCreditCardFuzzFields",
                "DebtCreditCardFuzzMethod",
                "DebtConsumerFuzzFields",
                "DebtConsumerFuzzMethod"

                );
        }


	    /// <summary>
        /// Read in the Address 2 data from a flat file
        /// </summary>
        private void ReadAddress2()
        { 
            // Read all address 2 values into a list
            using (StreamReader address2Reader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                  Properties.Settings.Default.Address2Input))
            {
                while (!address2Reader.EndOfStream)
                    this.address2List.Add(address2Reader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the Male names from a flat file
        /// </summary>
        private void ReadMaleNames()
        {
            // Read all Male Name values into a list
            using (StreamReader maleNameReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                  Properties.Settings.Default.MaleNamesInput))
            {
                while (!maleNameReader.EndOfStream)
                    this.maleNameList.Add(maleNameReader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the Male salutations from a flat file
        /// </summary>
        private void ReadMaleSalutations()
        {
            // Read all Male Name values into a list
            using (StreamReader salutationReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                    Properties.Settings.Default.MaleSalutationInput))
            {
                while (!salutationReader.EndOfStream)
                    this.maleSalutationList.Add(salutationReader.ReadLine());
            }
        }

        /// <summary>
        /// Read in the Female salutations from a flat file
        /// </summary>
        private void ReadFemaleSalutations()
        {
            // Read all Male Name values into a list
            using (StreamReader salutationReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                    Properties.Settings.Default.FemaleSalutationInput))
            {
                while (!salutationReader.EndOfStream)
                    this.femaleSalutationList.Add(salutationReader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the Female names from a flat file
        /// </summary>
        private void ReadFemaleNames()
        {
            // Read all Female Name values into a list
            using (StreamReader femaleNameReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                    Properties.Settings.Default.FemaleNamesInput))
            {
                while (!femaleNameReader.EndOfStream)
                    this.femaleNameList.Add(femaleNameReader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the Suffixes from a flat file
        /// </summary>
        private void ReadMaleSuffixes()
        {
            // Read all Last Name values into a list
            using (StreamReader suffixReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                Properties.Settings.Default.MaleSuffixInput))
            {
                while (!suffixReader.EndOfStream)
                    this.maleSuffixList.Add(suffixReader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the Phone Numbers from a flat file
        /// </summary>
        private void ReadPhoneNumbers()
        {
            // Read all Last Name values into a list
            using (StreamReader phoneNumberReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                     Properties.Settings.Default.PhoneNumberInput))
            {
                while (!phoneNumberReader.EndOfStream)
                    this.phoneNumberList.Add(phoneNumberReader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the Last Name data from a flat file
        /// </summary>
        private void ReadLastNames()
        {
            // Read all Last Name values into a list
            using (StreamReader lastnameReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                  Properties.Settings.Default.LastNameInput))
            {
                while (!lastnameReader.EndOfStream)
                    this.lastNameList.Add(lastnameReader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the Salutation data from a flat file
        /// </summary>
        private void ReadSalutations()
        {
            // Read all address 2 values into a list
            using (StreamReader salutationReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                    Properties.Settings.Default.MaleSalutationInput))
            {
                while (!salutationReader.EndOfStream)
                    this.salutationList.Add(salutationReader.ReadLine());
            }
        }


        /// <summary>
        /// Read in the nicknames/shortened firstnames from a flat file
        /// </summary>
	    private void ReadNicknames()
	    {
            // Read all nicknames into a dictionary cache
            using (StreamReader nicknameReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                  Properties.Settings.Default.NickNameInput))
            {
                while (!nicknameReader.EndOfStream)
                {
                    string[] nameMap = nicknameReader.ReadLine().Split(' ');
                    this.nicknameList.Add(nameMap[0].ToLower(), nameMap[1].ToLower());
                }
            }
	    }

		/// <summary>
		/// Reads the Provinces from a flat file.
		/// </summary>
		private void ReadLocations()
		{
			// This will read all the street names into a table.
            using (StreamReader locationReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                                  Properties.Settings.Default.LocationInput))
            {
                while (!locationReader.EndOfStream)
                {
                    // This line contains the raw data extracted from an on-line phone book database.
                    String locationText = locationReader.ReadLine();
                    String[] locationFields = locationText.Split('\t');

                    // This record can be used to provide a random location for a given consumer.
                    this.locationList.Add(new Location(locationFields[0], locationFields[1], locationFields[2]));
                }
            }

		}

        /// <summary>
        /// Reads the Street Names from a flat file.
        /// </summary>
        private void ReadStreetNames()
        {
            // This will read all the street names into a table.
            using (StreamReader streetNameReader = new StreamReader(Properties.Settings.Default.DataInputLocation +
                                                             Properties.Settings.Default.StreetInput))
            {
                while (!streetNameReader.EndOfStream)
                    this.streetNameList.Add(streetNameReader.ReadLine());
            }
        }

	
        /// <summary>
        /// Build the table of unique credit card issuers
        /// </summary>
        private void BuildCreditCardIssuerTable()
        {

            // This will read all the issuers into a table.
            StreamReader creditCardIssuerReader = new StreamReader(
                Properties.Settings.Default.DataInputLocation + 
                Properties.Settings.Default.CreditCardIssuerInput);

            while (!creditCardIssuerReader.EndOfStream)
            {
                // This line contains the raw data extracted from an on-line phone book database.
                String rawCreditCardIssuer = creditCardIssuerReader.ReadLine();
                
                // Create the Credit Card Issuer record from the raw data.
                DataSetDebtDataGenerator.RealCreditCardIssuerRow creditCardIssuerRow =
                    this.dataSetDebtDataGen.RealCreditCardIssuer.NewRealCreditCardIssuerRow();

                creditCardIssuerRow.Name = rawCreditCardIssuer;

                creditCardIssuerRow.ExternalId = rawCreditCardIssuer.ToUpper();

                this.dataSetDebtDataGen.RealCreditCardIssuer.AddRealCreditCardIssuerRow(creditCardIssuerRow);
            }
        }


		/// <summary>
		/// Create a random credit card.
		/// </summary>
		/// <param name="random">The random number generator.</param>
		/// <returns>A randomly generated credit card number.</returns>
		private String GenerateCreditCardNumber(Random random)
		{

			Int32 creditCardType = random.Next(0, 3);
			Char[] creditCardArray;
			switch (creditCardType)
			{

			// Visa
			case 0:

				// Generate a random Visa Credit Card Number
				creditCardArray = new Char[16];
				creditCardArray[0] = Convert.ToChar('4');
				creditCardArray[1] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[2] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[3] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[4] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[5] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[6] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[7] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[8] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[9] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[10] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[11] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[12] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[13] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[14] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[15] = Convert.ToChar('0' + random.Next(0, 10));
				break;

			// Master Card
			case 1:

				// Generate a random Master Card Credit Card Number
				creditCardArray = new Char[16];
				creditCardArray[0] = Convert.ToChar('5');
				creditCardArray[1] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[2] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[3] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[4] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[5] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[6] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[7] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[8] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[9] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[10] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[11] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[12] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[13] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[14] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[15] = Convert.ToChar('0' + random.Next(0, 10));
				break;

			// Discover
			default:

				// Generate a random American Express Credit Card Number.
				creditCardArray = new Char[16];
				creditCardArray[0] = Convert.ToChar('7');
				creditCardArray[1] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[2] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[3] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[4] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[5] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[6] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[7] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[8] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[9] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[10] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[11] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[12] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[13] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[14] = Convert.ToChar('0' + random.Next(0, 10));
				creditCardArray[15] = Convert.ToChar('0' + random.Next(0, 10));
				break;

			}

			return new String(creditCardArray);

		}

        /// <summary>
        /// Generates a complete set of consumer records from cached set of individual 'parts'. The number of records 
        /// created is determined by the config setting: NumberOfConsumerRecords
        /// </summary>
        private void BuildConsumerTable(RandomNumberGenerator random)
        {
            // Hashtable of names based on First, Last, and Middle names. We will use this to detect
            // duplicate full names. We use a Dictionay instead of List to keep searching through it
            // close to an O(1) operation instead of O(n)...this list could get huge.
            Dictionary<String, String> uniqueConsumerNameList = new Dictionary<string, string>();
            String nameKey;

            // Hashtable of SSN values. Used to detect the off-chance we randomly generate duplicates. 
            // We have to avoid duplicates when generating the clean/real data. 
            Dictionary<String, String> uniqueSsnList = new Dictionary<string, string>();

            // Informational: used to keep track of how many SSN values we generate.  If we generated more than the
            // number of consumer records we genertate, that means the difference will tell us how many dups we had
            Int32 numSsnGenerations = 0; 

            // While the number of consumer records is less than the amount required by the config settings:
            while (this.dataSetDebtDataGen.RealConsumer.Count < Properties.Settings.Default.NumberOfConsumerRecords)
            {
                // Create a new consumer record 
                DataSetDebtDataGenerator.RealConsumerRow consumerRow =
                    this.dataSetDebtDataGen.RealConsumer.NewRealConsumerRow();

                // Gender: This has a bearing on different parts of the name format
                TestDataConfig.Gender gender = FlipCoinToDetermineGender(random);

                // Salutation:  Depends on config data determining what percentage of consumer records need to have a salutation 
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithSalutations - 1))
                {
                    // one out of 85 consumers will be a Doctor (gender-neutral salutation)
                    if (random.Next(85) == 0)
                    {
                        consumerRow.Salutation = "Dr";
                    }
                    else
                    {
                        consumerRow.Salutation = GenerateSalutation(gender, random);
                    }
                }

                // First Name:  Based on gender
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithFirstNames - 1))
                {
                    consumerRow.FirstName = GenerateFirstName(gender, random);

                    // Nickname/Shortened FirstName: Check if one exists for this first name (i.e. James-->Jim)
                    String nicknameKey = consumerRow.FirstName.ToLower();
                    if (this.nicknameList.ContainsKey(nicknameKey))
                    {
                        // uppercase for the first letter
                        consumerRow.Nickname = this.nicknameList[nicknameKey].Substring(0, 1).ToUpper() +
                                               this.nicknameList[nicknameKey].Substring(1, this.nicknameList[nicknameKey].Length - 1);
                    }
                }

                // Middle Name:  Depends on config data to determine what percentage of consumer records need to have a middle name
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithMiddleNames - 1))
                {
                    consumerRow.MiddleName = GenerateMiddleName(random);
                }

                // Last Name:
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithLastNames - 1))
                {
                    consumerRow.LastName = GenerateLastName(random);
                }

                // Suffix: Depends on config data to determine what percentage of consumer records need to have a suffix
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithSuffixes - 1))
                {
                    consumerRow.Suffix = GenerateSuffix(gender, random);
                }
                  

                // Generate a key based on the consumer's full name
                nameKey = String.Format("LN:{0} FN:{1} MN:{2}",
                    consumerRow.LastName,
                    consumerRow.FirstName,
                    consumerRow.MiddleName);

                // Address 1 (Street Address) Depends on config data to determine what percentage of consumer records need to have it filled out
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithAddress1 - 1))
                {
                    consumerRow.Address1 = String.Format("{0} {1}", random.Next(199) + 1, this.streetNameList[random.Next(streetNameList.Count)]);
                }

                // Address2:  Depends on config data to determine what percentage of consumer records need to have an Address2 filled out
                if (random.Next(100).IsWithinRange(0,Properties.Settings.Default.PercentageOfConsumerRecordsWithAddress2 - 1))
                {
                    consumerRow.Address2 = BuildAddress2(random);
                }


                // DateOfBirth:  Depends on config data to determine what percentage of consumer records need to have this filled out
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithDateOfBirth - 1))
                {
                    // Randomly generate
                    String dateText = String.Format("{0}/{1}/{2}", random.Next(1, 13), random.Next(1, 28), random.Next(1940, 1992));
                    consumerRow.DateOfBirth = Convert.ToDateTime(dateText);
                }

                // Employment Status:  Whether or not we know the employment status depends on the weight set in config data
                if (random.Next(100).IsWithinRange(0,Properties.Settings.Default.PercentageOfConsumersWithKnownEmployeeStatus - 1))
                {    
                    // if we know the status, base it on an unemployment rate:
                    consumerRow.IsEmployed = random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfUnemployment - 1);
                }

                // Build the phone number
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithPhoneNumber - 1))
                {
                    String rawPhoneNumber = this.phoneNumberList[random.Next(0, this.phoneNumberList.Count - 1)];
                    consumerRow.PhoneNumber = String.Format("({0}) {1} {2}",
                                                            rawPhoneNumber.Substring(0, 3),  /* area code */
                                                            rawPhoneNumber.Substring(3, 3),  /* exchange */
                                                            rawPhoneNumber.Substring(6, 4)); /* target */
                }

                // Generate a random location for the consumer.
                Location location = this.locationList[random.Next(0, this.locationList.Count)];

                // City:  Depends on config data to determine what percentage of consumer records need to have this filled out
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithCity - 1))
                {
                    consumerRow.City = location.City;
                }

                // City:  Depends on config data to determine what percentage of consumer records need to have this filled out
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithProvinceCode - 1))
                {
                    consumerRow.ProvinceCode = location.ProvinceCode;
                }

                // City:  Depends on config data to determine what percentage of consumer records need to have this filled out
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithPostalCode - 1))
                {
                    consumerRow.PostalCode = location.PostalCode;
                }


                // Bank data for the Consumer:  Depends on config data to determine what percentage of consumer records need to have this filled out
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfConsumerRecordsWithBankAccountData - 1))
                {
                    // Bank Account Number: usually anywhere from 6 to 19 digits. 
                    // [skt] vary the length instead of the constant 19 digit length
                    consumerRow.BankAccountNumber =
                        string.Format("{0:00000}", random.Next(100000)) +
                        string.Format("{0:00000}", random.Next(100000)) +
                        string.Format("{0:00000}", random.Next(100000)) +
                        string.Format("{0:0000}", random.Next(10000));

                    // Bank Routing Number: 9 digit number is the standard format length
                    consumerRow.BankRoutingNumber =
                        string.Format("{0:00000}", random.Next(100000)) +
                        string.Format("{0:0000}", random.Next(10000));
                }

                // Social Security Number:
                String ssn = null;
                do
                {
                    // get a randomly generated SSN
                    ssn = GenerateSocialSecurityNumber(random);

                    // increment the count of how many SSNs we generate 
                    ++numSsnGenerations;

                } while (uniqueSsnList.ContainsKey(ssn)); // makes sure that the SSN is unique
                uniqueSsnList.Add(ssn, nameKey); 

                consumerRow.SocialSecurityNumber = ssn;

                // Add the consumer data to the table.
                this.dataSetDebtDataGen.RealConsumer.AddRealConsumerRow(consumerRow);

                // If the consumer's name already exists in this list, then we have a person with the same full 
                // name as at least one other already generated.  We want to keep track of these situations  
                // in order to gauge the accuracy of the match algorithm...so add a reference to this record in the
                // DuplicateConsumerNames table. 
                if (uniqueConsumerNameList.ContainsKey(nameKey))
                {
                    // create a record to track the duplicate
                    DataSetDebtDataGenerator.DuplicateConsumerNamesRow duplicateConsumerRow =
                        this.dataSetDebtDataGen.DuplicateConsumerNames.NewDuplicateConsumerNamesRow();

                    // save the duplicated name
                    duplicateConsumerRow.RealConsumerFullName = nameKey;

                    // point to the consumer record that is a duplicate.
                    duplicateConsumerRow.RealConsumerId = consumerRow.RealConsumerId;
                }
                else
                {
                    // Add the name of the consumer as the key so we can quickly search for it in the list
                    uniqueConsumerNameList.Add(nameKey, consumerRow.SocialSecurityNumber);
                }

                // Generate credit cards for the consumer
                GenerateCreditCardsForConsumer(consumerRow, random);
            }

            // Informational:  Write out how many duplicate SSNs we generated (if any)
            using (StreamWriter sw = File.AppendText(this.destConfigFile))
            {
                sw.WriteLine("");
                sw.WriteLine("SSN Duplicates: {0}", numSsnGenerations - this.dataSetDebtDataGen.RealConsumer.Count);
                sw.WriteLine("");
                sw.WriteLine("Full Name Duplicates: {0}", this.dataSetDebtDataGen.DuplicateConsumerNames.Count);
            }
        }


        /// <summary>
        /// Generate a random Social Security Number
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private string GenerateSocialSecurityNumber(RandomNumberGenerator random)
        {
            String ssn = null;

            // Generate a random social security number.
            if (Properties.Settings.Default.NumberOfConsumerRecords == 1)
            {
                // Generate the generic single SSN
                ssn = String.Format("{0:000-00-0000}", 12345678);
            }
            else
            {
                ssn = String.Format("{0:000-00-0000}", random.Next(1000000000));
            }

            return ssn;
        }



        /// <summary>
        /// Generate a suffix
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private string GenerateSuffix(TestDataConfig.Gender gender, RandomNumberGenerator random)
        {
            String  suffix = null;

            if (gender == TestDataConfig.Gender.Female)
            {
                // no-op right now
            }
            else /* male */
            {
                // get a random male suffix from the cached list
                suffix = this.maleSuffixList[random.Next(0, this.maleSuffixList.Count - 1)];
            }

            return suffix;
        }


        /// <summary>
        /// Generate a Middlename 
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private String GenerateMiddleName(RandomNumberGenerator random)
        {
            String middleName = null;

            if (Properties.Settings.Default.NumberOfConsumerRecords == 1)
            {
                middleName = "Z";
            }
            else
            {
                // [skt] todo: right now we are only supplying an initial, if we need full names, we can pull them out of
                // the gender-specific first name files
                middleName = Convert.ToChar('A' + random.Next(0, 26)).ToString();
            }

            return middleName;
        }


        /// <summary>
        /// Generate a last name
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private String GenerateLastName(RandomNumberGenerator random)
        {
            String lastName = null;

            if (Properties.Settings.Default.NumberOfConsumerRecords == 1)
            {
                lastName = "Doe";
            }
            else
            {
                // get random name out of the list
                lastName = this.lastNameList[random.Next(0, this.lastNameList.Count - 1)];

                // uppercase first letter, lowercase the rest
                lastName = lastName[0].ToString().ToUpper() + lastName.Substring(1, lastName.Length - 1).ToLower();
            }

            return lastName;
        }


        /// <summary>
        /// Return a first name based on the gender requirement parameter
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private String GenerateFirstName(TestDataConfig.Gender gender, RandomNumberGenerator random)
        {
            String firstName = null;

            if (gender == TestDataConfig.Gender.Female)
            {
                if (Properties.Settings.Default.NumberOfConsumerRecords == 1)
                {
                    firstName = "Jane";
                }
                else 
                {
                    // get a random female name from the cached list
                    firstName = this.femaleNameList[random.Next(0, this.femaleNameList.Count - 1)];
                }
            }
            else /* male */
            {
                if (Properties.Settings.Default.NumberOfConsumerRecords == 1)
                {
                    firstName = "John";
                }
                else
                {
                    // get a random male name from the cached list
                    firstName = this.maleNameList[random.Next(0, this.maleNameList.Count - 1)];
                }
            }

            // uppercase first letter, lowercase the rest
            firstName = firstName[0].ToString().ToUpper() + firstName.Substring(1, firstName.Length - 1).ToLower();

            return firstName;
        }




        /// <summary>
        /// Returns a random salutation based on the passed-in gender requirement.
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private String GenerateSalutation(TestDataConfig.Gender gender, RandomNumberGenerator random)
        {
            String salutation = null;

            if (gender == TestDataConfig.Gender.Female)
            {
                salutation = this.femaleSalutationList[random.Next(femaleSalutationList.Count)];
            }
            else
            {
                salutation = this.maleSalutationList[random.Next(maleSalutationList.Count)];
            }

            return salutation;
        }


        /// <summary>
        /// Return Male or Female based on a 'coin toss'
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private TestDataConfig.Gender FlipCoinToDetermineGender(RandomNumberGenerator random)
        {
            // flip a coin for male or female
            if (random.Next(100).IsWithinRange(0, 49))
            {
                return TestDataConfig.Gender.Female;
            }
            else
            {
                return TestDataConfig.Gender.Male;
            }
        }


        /// <summary>
        /// Generate credit cards for the passed-in consumer 
        /// </summary>
        /// <param name="consumerRow"></param>
        /// <param name="random"></param>
        private void GenerateCreditCardsForConsumer(DataSetDebtDataGenerator.RealConsumerRow consumerRow, RandomNumberGenerator random)
        {
            Int32 numCreditCards;

            // Get the number of credit cards we need to generate
            if (Properties.Settings.Default.NumberOfCreditCardsPerConsumer == TestDataConfig.GenerateRandomNumberOfCreditCards)
            {
                // Generate between 1 and RandomCreditCardsPerConsumerMax credit cards per consumer.
                //    (RandomCreditCardsPerConsumerMax / 2) gives us a rough average per consumer
                numCreditCards = random.Next(1, Properties.Settings.Default.RandomCreditCardsPerConsumerMax + 1);
            }
            else
            {
                // constant value of credit cards to create per consumer
                numCreditCards = Properties.Settings.Default.NumberOfCreditCardsPerConsumer;
            }

            // go forth and generate the specified number of cards
            for (Int32 index = 0; index < numCreditCards; index++)
            {
                DataSetDebtDataGenerator.RealCreditCardRow creditCardRow =
                    this.dataSetDebtDataGen.RealCreditCard.NewRealCreditCardRow();

                // Create the credit line record
                if (Properties.Settings.Default.NumberOfConsumerRecords == 1)
                {
                    // Ensure that the CCNs are unique and predictable when we are just dealing with one user
                    
                    // This is typically some number associated with the Credit Card and the institution that issued it
                    creditCardRow.AccountCode = string.Format("{0}{1:000}", "111222", index + 1);
                    
                    // this is the actual Credit Card Number
                    creditCardRow.OriginalAccountNumber = string.Format("{0}{1:000}", "6666555544443", index + 1);
                }
                else
                {
                    // AccountNumber is typically some number associated with the Credit Card and the institution that either 
                    // issued it or now owns it
                    creditCardRow.AccountCode = string.Format("{0:000000000}", random.Next(1000000000));

                    // this is the actual Credit Card Number 
                    creditCardRow.OriginalAccountNumber = GenerateCreditCardNumber(random);
                }

                creditCardRow.RealConsumerId = consumerRow.RealConsumerId;

                // Issuer:  whether or not we supply this optional data depends on the config setting's weight distrubtion
                if (random.Next(100).IsWithinRange(0, Properties.Settings.Default.PercentageOfCreditCardRecordsWithIssuerData - 1))
                {
                    // Pick a random Issuer/DebtHolder/Originator (pick the term) for this credit card
                    creditCardRow.RealDebtHolder =
                        this.dataSetDebtDataGen.RealCreditCardIssuer[
                            random.Next(0, this.dataSetDebtDataGen.RealCreditCardIssuer.Count)].Name;
                }

                creditCardRow.ExternalId = Guid.NewGuid().ToString();
                creditCardRow.AccountBalance = Math.Round(Convert.ToDecimal(random.NextDouble() * 15000.0), 2);

                this.dataSetDebtDataGen.RealCreditCard.AddRealCreditCardRow(creditCardRow);

                // Keep track of the credit cards created for this consumer.  It will make it much more efficient
                // to find per-consumer CCs later on rather than iterating through the credit card table, one 
                // record at a time.
                consumerRow.RealCreditCardList = 
                   StringHelpers.AppendTokenToCommaDelimitedString(consumerRow.RealCreditCardList,
                                                                   creditCardRow.RealCreditCardId.ToString());
            }
        }



        /// <summary>
        /// Builds a string for an Address 2 
        /// Examples: Apartment 100, 1st Floor, Suite 44-A, etc..
        /// </summary>
        /// <returns></returns>
        private string BuildAddress2(RandomNumberGenerator random)
        {
            if (random==null)
                return null;

            StringBuilder address2 = new StringBuilder();  

            // get a random address from the available address2List
            string token = this.address2List[random.Next(address2List.Count)];

            // Build a floor address string and return
            if (token.ToLower() == "floor")
            {
                return BuildFloorSubAddress(token, random);
            }

            // do similar stuff for all non-'floor' types of address strings
            address2.Append(token);
            
            // around one out of every 50 addresses we will NOT put a space between the token and the number value
            if (random.Next(50) != 0)
            {
                address2.Append(" ");
            }

            // vary the height of the building
            Int32 topFloor = 
                (random.Next(5) == 0 ? TestDataConfig.BuildingFloors.SkyscraperFloors : TestDataConfig.BuildingFloors.LowriseFloors);

            address2.Append(Convert.ToString(random.Next(1, topFloor)));

            // around one out of 30 times we will put a dash followed by a letter (A thru G)
            if (random.Next(30) == 0)
            {
                address2.Append("-");
                address2.Append(Convert.ToChar('A' + random.Next(0, 7)));
            }                     

            return address2.ToString();
        }

        /// <summary>
        /// Build a string that cooresponds to which floor the address is part of
        /// i.e. 101 st Floor 2nd Floor, etc...
        /// </summary>
        /// <returns></returns>
        private string BuildFloorSubAddress(string token, RandomNumberGenerator random)
        {
            if (random == null)
                return null;
            
            StringBuilder address2 = new StringBuilder();
            string qualifier;

            // vary the height of the building
            Int32 topFloor =
                (random.Next(5) == 0 ? TestDataConfig.BuildingFloors.SkyscraperFloors : TestDataConfig.BuildingFloors.LowriseFloors);

            string floorNumber = Convert.ToString(random.Next(1, topFloor));

            address2.Append(floorNumber);

            // around one out of every 75 addresses will put a space between the floor number and the qualifier
            // 1 st floor versus 1st floor
            if (random.Next(75) == 0)
            {
                address2.Append(" ");
            }

            // build a floor qualifier
            if (floorNumber.EndsWith("1"))
            {
                qualifier = "st";
            }
            else if (floorNumber.EndsWith("2"))
            {
                qualifier = "nd";
            }
            else if (floorNumber.EndsWith("3"))
            {
                qualifier = "rd";
            }
            else
            {
                qualifier = "th";
            }

            // append the qualifier 
            address2.Append(qualifier);

            // around one out of every 50 addresses we will NOT put a space between the qualifier and the next token
            if (random.Next(25) != 0)
            {
                address2.Append(" ");
            }

            // finally append the "floor" string (use token insted of literal to account for different character cases)
            address2.Append(token);

            return address2.ToString();
        }

		/// <summary>
		/// Indicates that a given code is a recognized state code.
		/// </summary>
		/// <param name="stateCode">The state code found in the Consumer Debt file.</param>
		/// <returns>true indicates its a proper state, false indicates the code isn't recognized.</returns>
		private Boolean IsState(String provinceCode)
		{
			// This indicates that the given state is recognized from our list of locations.
			foreach (Location location in this.locationList)
                if (location.ProvinceCode == provinceCode)
					return true;

			// This indicates the code isn't recognized.
			return false;
		}


        /// <summary>
        /// Read organization information from a flat file and populate the Organization table
        /// </summary>
        private void BuildOrganizationTable()
        {
            string organizationInputPath = 
                Properties.Settings.Default.DataInputLocation +
                Properties.Settings.Default.OrganizationInput;

            using (StreamReader organizationDataReader = new StreamReader(organizationInputPath))
            {
                while (!organizationDataReader.EndOfStream)
                {
                    string[] rawOrganization = organizationDataReader.ReadLine().Split(TestDataConfig.CsvDelimiter);
                    
                    // skip any blank or commented out lines
                    if ((rawOrganization[0] == String.Empty) || (rawOrganization[0].Trim()[0] == TestDataConfig.CommentMarker))
                        continue;

                    // name and type columns are required
                    if (rawOrganization.Length < 2)
                    {
                        throw new SystemException(
                            string.Format("{0} does not have at least two columns for Name and Type",organizationInputPath));
                    }

                    DataSetDebtDataGenerator.OrganizationRow organizationRow =
                        this.dataSetDebtDataGen.Organization.NewOrganizationRow();

                    // Org Name and type
                    organizationRow.Name = rawOrganization[TestDataConfig.RawOrganizationColumn.Name].Trim();
                    organizationRow.Type = rawOrganization[TestDataConfig.RawOrganizationColumn.Type].Trim();

                    // any remaining columns (column 3 through ?) are debt class catagories
                    for (Int32 i = 2; i < rawOrganization.Length; i++ )
                    {
                        organizationRow.DebtClassList =
                            StringHelpers.AppendTokenToCommaDelimitedString(organizationRow.DebtClassList,
                                                                            rawOrganization[i].Trim());
                    }

                    // Assign a unique EntityId for this organization
                    organizationRow.EntityId = Guid.NewGuid();

                    // Assign the TenantId
                    organizationRow.TenantId = organizationRow.Name + TestDataConfig.TenantKeySuffix;

                    // Add the organization to the table
                    this.dataSetDebtDataGen.Organization.AddOrganizationRow(organizationRow);
                }
            }
        }


        /// <summary>
        /// Read user information from a flat file and poplulate the User Table 
        /// </summary>
        private void BuildUserTable()
        {
            using (StreamReader userDataReader = 
                new StreamReader(Properties.Settings.Default.DataInputLocation +
                Properties.Settings.Default.UserInput))
            {
                while (!userDataReader.EndOfStream)
                {
                    string[] rawUser = userDataReader.ReadLine().Split(TestDataConfig.CsvDelimiter);

                    // skip any blank or commented out lines
                    if ((rawUser[0] == String.Empty) || (rawUser[0].Trim()[0] == TestDataConfig.CommentMarker))
                        continue;

                    // There has to be an organization to associate this user with...otherwise we will ignore it.
                    if (this.dataSetDebtDataGen.Organization.Rows.Contains(rawUser[TestDataConfig.RawUserColumn.Organization].Trim()))
                    {
                        DataSetDebtDataGenerator.UserRow userRow = this.dataSetDebtDataGen.User.NewUserRow();

                        userRow.UserId = Guid.NewGuid();

                        userRow.Password = rawUser[TestDataConfig.RawUserColumn.Password].Trim();

                        userRow.Name = rawUser[TestDataConfig.RawUserColumn.Name].Trim();

                        userRow.ExternalId = userRow.Name.ToUpper();

                        userRow.Organization = rawUser[TestDataConfig.RawUserColumn.Organization].Trim();

                        userRow.Email = rawUser[TestDataConfig.RawUserColumn.Email].Trim();

                        this.dataSetDebtDataGen.User.AddUserRow(userRow);

                        // I hearby nominate this user to represent the organization when we generate working orders 
                        // via (CreatedUserId)...Representative user is the ExternalId0 of the Entity Table
                        foreach (DataSetDebtDataGenerator.OrganizationRow orgRow in this.dataSetDebtDataGen.Organization)
                        {
                            if (orgRow.Name == userRow.Organization)
                            {
                                orgRow.RepresentativeUser = userRow.Name;
                                break;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Reads raw consumer debt data into memory.  File source is defined by
        /// config setting: ConsumerDebtInput
        /// </summary>
        /// <param name="rawConsumerDebtCache"></param>
        private void ReadRawConsumerDebtData(List<String[]> rawConsumerDebtCache)
        { 
            // Open a stream to a raw data file that has some information we will use to fill some ConsumerDebt fields 
            using (StreamReader sw = 
                new StreamReader(Properties.Settings.Default.DataInputLocation + 
                                 Properties.Settings.Default.ConsumerDebtInput))
            {
                // Read in each line of raw consumer debt data into memory. Each line of text is delimited 
                // so break the lines down into individual string tokens per line.
                while (!sw.EndOfStream)
                {
                    rawConsumerDebtCache.Add(sw.ReadLine().Split(TestDataConfig.RawConsumerDebtDelimiter));
                }
            }

        }

       
        /// <summary>
        /// Randomly chooses a row of consumer debt data from the raw cache
        /// </summary>
        /// <param name="random"></param>
        /// <param name="rawConsumerDebtCache"></param>
        /// <returns></returns>
	    private String[] GetRandomRawConsumerDebtData(Random random, List<String[]> rawConsumerDebtCache)
	    {
            // return the array of strings for this one raw debt record
            return rawConsumerDebtCache[random.Next(0, rawConsumerDebtCache.Count)];
	    }


	    /// <summary>
	    /// Generates the Consumer Trust data. 
	    /// </summary>
	    /// <param name="random"></param>
	    private void  BuildConsumerTrustTable(RandomNumberGenerator random)
	    {
	        // used to track the unique Id of a trust consumer row 
	        Int32 trustConsumerId;

	        Int32 numTrustRecordsToCreate;
	        if (Properties.Settings.Default.NumberOfTrustRecords < 0)
	        {
	            // A negative value for this config setting means to choose a random amount of consumers
	            // to pick from out of the RealConsumer table
	            numTrustRecordsToCreate = random.Next(1, this.dataSetDebtDataGen.RealConsumer.Count);
	        }
	        else
	        {
	            // use the literal amount specified in the config setting.
	            numTrustRecordsToCreate = Properties.Settings.Default.NumberOfTrustRecords;
	        }
            
            // Vendor Code 
            // [skt] for now just a GUID but it should be the org name or some other field out of the organization table
	        String vendorCode = System.Guid.NewGuid().ToString();

            // Populate the Trust Table
	        while (this.trustSideMapOfRealConsumerIds.Count < numTrustRecordsToCreate)
	        {             
	            // randomly fetch a consumer out of the RealConsumer table
	            DataSetDebtDataGenerator.RealConsumerRow realConsumerRow = 
	                this.dataSetDebtDataGen.RealConsumer[random.Next(this.dataSetDebtDataGen.RealConsumer.Count)];

	            // if the real consumer has already been added to the trust side consumer table, try again
                if (this.trustSideMapOfRealConsumerIds.ContainsKey(realConsumerRow.RealConsumerId))
	                continue;

	            // Add a Corresponding Consumer Record  
	            //   trustConsumerId=the ID of the new row in the TrustSideConsumer table
	            AddRealConsumerToTrustSideConsumerTable(realConsumerRow, out trustConsumerId);

	            // keep track of the Real Consumer added to the trust side consumer table
	            this.trustSideMapOfRealConsumerIds.Add(realConsumerRow.RealConsumerId, trustConsumerId);

	            // Add the credit card(s) to the trust side consumer 
	            AssignCreditCardsToTrustSideConsumer(realConsumerRow, trustConsumerId);

	            // 
	            // Create and Add a Trust record
	            //
	            DataSetDebtDataGenerator.ConsumerTrustRow consumerTrustRow = this.dataSetDebtDataGen.ConsumerTrust.NewConsumerTrustRow();

                // Customer Code:  This is the Negotiator's unique identifier per consumer.  We can map it here to the Id of the consumer 
                // added to the trust table since that Id is unique.
	            consumerTrustRow.CustomerCode = trustConsumerId;

                // This will generate random balances for the checking and savings account.
	            consumerTrustRow.SavingsBalance = Math.Round(Convert.ToDecimal(random.NextDouble() * random.Next(500, 50000)), 2);

	            // This creates a random value for how much of the credit card can be deducted automatically from a checking account.
	            switch (random.Next(0, 4))
	            {
                    case 0:
                        consumerTrustRow.AutoSettlement = 0.40M;
                        break;

	                case 1:
	                    consumerTrustRow.AutoSettlement = 0.45M;
	                    break;

	                case 2:
	                    consumerTrustRow.AutoSettlement = 0.50M;
	                    break;
                        
	                case 3:
	                    consumerTrustRow.AutoSettlement = 0.60M;
	                    break;
	            }

	            // 
	            // Set a name for the Savings Entity (the custodial owner of the savings acct)
	            //    For now the options are GCS or Self-Held and will be distributed 80/20 respectively
	            //
	            if (random.Next(100).IsWithinRange(0,79))
	            {
	                consumerTrustRow.SavingsEntityCode = "GCS";
	            }
	            else
	            {
                    consumerTrustRow.SavingsEntityCode = "Self-Held";
	            }

                consumerTrustRow.VendorCode = vendorCode;

	            // Add the consumer trust row to the ConsumerTrust table
	            this.dataSetDebtDataGen.ConsumerTrust.AddConsumerTrustRow(consumerTrustRow);

                // Keep track of the Trust Record IDs in a separate cache so we have easy/fast access to the list
                this.consumerTrustIds.Add(consumerTrustRow.ConsumerTrustId);
	        }
	    }

	    /// <summary>
        /// Builds the Consumer Debt Record based on raw text data and data from the Consumer and CreditCard tables 
        /// </summary>
        /// <param name="random"></param>
	    private void BuildConsumerDebtTable(RandomNumberGenerator random)
	    {
            // Build a cache of the Consumer Debt data based on the supplied raw data file
            List<String[]> rawDebtRecords = new List<string[]>();
            ReadRawConsumerDebtData(rawDebtRecords);

	        String[] rawConsumerDebtRecord;

            // Unique debtHolderAccountCode is needed for each Debt record. We got a number/pattern from GCS at one point
            // which is based on the config setting below. Use that as a starting point and just increment from there
            Int64 debtHolderAccountCode = Convert.ToInt64(Properties.Settings.Default.DebtHolderAccountNumberStartValue);

	        Int32 debtSideConsumerId;
            Int32 numDebtRecordsToCreate;
            DataSetDebtDataGenerator.RealCreditCardRow  realCreditCardRow = null;
           
            // Determine how many debt records we need to create
	        if (Properties.Settings.Default.NumberOfDebtRecords < 0)
	        {
	            // A negative value for this config setting means to choose a random amount of credit cards
	            // to pick from out of the CreditCard table
	            numDebtRecordsToCreate = random.Next(1, this.dataSetDebtDataGen.RealCreditCard.Count);
	        }
	        else
	        {
	            // use the literal amount specified in the config setting.
	            numDebtRecordsToCreate = Properties.Settings.Default.NumberOfDebtRecords;
	        }

            // Vendor Code 
            // [skt] for now just a GUID but it should be the org name or some other field out of the organization table
            String vendorCode = System.Guid.NewGuid().ToString();

            // Build the Debt Records and add them to the Debt-Side Tables
            while (this.debtSideMapOfRealCreditCardIds.Count < numDebtRecordsToCreate)
	        {
	            Boolean currentRecordIsMatch = false;
                
                // the number of debt organizations affects how the matches get divied up...the matches are front loaded 
                // in the debt table.  We shuffle the list of them later on so they are evenly distributed 

                // Determine where to get the CC data from: Keep building matched records until the number of matched records added to 
                // the Debt side equals the required NumberOfMatches. In order to have guaranteed matches (fuzzing aside), 
                // we need to get records out of the Trust CC table which was built already
                if (this.debtSideMapOfRealCreditCardIds.Count < Properties.Settings.Default.NumberOfMatches)
                {
                    // ** Ensure a match (at least prior to fuzzing..which in and of itself could inadvertently create a match)

                    // get a random CC out of the Trust CC Table so we can have a guaranteed match between trust and debt sides
                    DataSetDebtDataGenerator.TrustCreditCardRow trustCreditCardRow = 
                        this.dataSetDebtDataGen.TrustCreditCard[random.Next(this.dataSetDebtDataGen.TrustCreditCard.Count)];

                    // In order to find out if we can match with this one, we need to get the corresponding
                    // Real Credit Card row based on the trust CC record we just pulled out.
                    realCreditCardRow = this.dataSetDebtDataGen.RealCreditCard[trustCreditCardRow.RealCreditCardId];      
            
                    // If the CC is already matched, then move on to the next random CC in the TrustCreditCard table
                    if (realCreditCardRow.IsMatched)
                        continue;
                    // [skt] we may want to ensure a higher amount of control over how multiple consumers' credit cards get 
                    // added to the debt side.  Could do this by trying another CC from the SAME CONSUMER here instead of 
                    // just grabbing another random CC out of the TrustCreditCard table, leaving duplicate consumers up to 'chance'  

                    // Denote that this current set of data is intended to be a matching one
                    currentRecordIsMatch = true;
                }
                else 
                {
                    // ** Ensure we do NOT get a match (at least prior to fuzzing)

                    // Get the credit card data directly from the RealCreditCard table
                    realCreditCardRow = this.dataSetDebtDataGen.RealCreditCard[random.Next(this.dataSetDebtDataGen.RealCreditCard.Count)];                    

                    // We do NOT want to create ANY more matches at this point:+
                    //   If this particular credit card's consumer is in the TrustSideConsumer table, the consumer 
                    //   has already been matched via one or more of its credit cards...so try again
                    if (this.trustSideMapOfRealConsumerIds.ContainsKey(realCreditCardRow.RealConsumerId))   
                        continue;                   
                }

                // if the credit card has already been added to the DebtCreditCard table, try again.  This check is 
                // needed because we are 'randomly' grabbing credit cards out of the CC tables so dups are possible.
                if (this.debtSideMapOfRealCreditCardIds.ContainsKey(realCreditCardRow.RealCreditCardId))
                    continue;
    
                // Check to see if we want to make sure we only one credit card from a consumer is allowed to 
                // be held by this debt organization. This is a corner case as it is entirely possible that a
                // holder organization purchase two separate baskets of CCs, each of which could contain a CC
                // from the same consumer (consumer owns a Macy's and BOA card)
                if ((Properties.Settings.Default.OneConsumerCreditCardPerDebtHolderOrganization) &&
                   (this.debtSideMapOfRealConsumerIds.ContainsKey(realCreditCardRow.RealConsumerId)))
                    continue;

                // Randomly pick a line from the raw Debt Record data to help fill in the rest of the consumer debt data
                rawConsumerDebtRecord = GetRandomRawConsumerDebtData(random, rawDebtRecords);
                    
	            // create a new ConsumerDebt row and start populating it
	            DataSetDebtDataGenerator.ConsumerDebtRow consumerDebtRow = this.dataSetDebtDataGen.ConsumerDebt.NewConsumerDebtRow();

                // Create a unique consumer per debt record and get its Id back so we can use it to map to the Debt Side CC table
                AddRealConsumerToDebtSideConsumerTable(realCreditCardRow.RealConsumerRow, out debtSideConsumerId);

                // used to store the ID of the credit card created for the debt side CC table
	            Int32 debtSideCreditCardId; 

	            // Add the Real CreditCard to the DebtCreditCard table
	            AssignCreditCardToDebtSideConsumer(realCreditCardRow, debtSideConsumerId, random, out debtSideCreditCardId);

                // Update the cache map of RealCreditCards to their Debt-Side representations
                this.debtSideMapOfRealCreditCardIds.Add(realCreditCardRow.RealCreditCardId, debtSideCreditCardId);

	            // Assign a unique DataArchiveId and increment it for the next Debt record
	            consumerDebtRow.AccountCode = debtHolderAccountCode++;

	            // Date of Delinquency:
	            DateTime dateOfDelinquency = DateTime.MinValue;
                if (TryGetDateOfDelinquencyFromRawDebtData(rawConsumerDebtRecord, consumerDebtRow, random, ref dateOfDelinquency))
                {
                    consumerDebtRow.DateOfDelinquency = dateOfDelinquency;
                }

                // Vendor Code: Unqiue per Debt Holder (consumer debt) organization
                consumerDebtRow.VendorCode = vendorCode;

	            // Finally, the consumer debt record is added to the data model.              
	            this.dataSetDebtDataGen.ConsumerDebt.AddConsumerDebtRow(consumerDebtRow);
                
                // Keep track of the Debt Record IDs in a separate cache so we have easy/fast access to the list
	            this.consumerDebtIds.Add(consumerDebtRow.ConsumerDebtId);

                // If we are working with a matched CC, increment the RealConsumer's match count by one and 
                // mark the RealCreditCard as being matched
                if (currentRecordIsMatch)
                {
                    this.dataSetDebtDataGen.RealConsumer[realCreditCardRow.RealConsumerId].MatchCount++;
                    this.dataSetDebtDataGen.RealCreditCard[realCreditCardRow.RealCreditCardId].IsMatched = true;
                }
	        }

            // The rawDebtRecords cache is pretty large so it's worth it to force the garbage collector to clean it up here
            rawDebtRecords.Clear();
	        rawDebtRecords = null;
            Utils.ForceGarbageCollection();
	    }


        /// <summary>
        /// Extract the date of delinquency from the raw debt data
        /// </summary>
        /// <param name="rawConsumerDebtList"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private Boolean TryGetDateOfDelinquencyFromRawDebtData
            (
            String[] rawConsumerDebtList, 
            DataSetDebtDataGenerator.ConsumerDebtRow consumerDebtRow,
            RandomNumberGenerator random,
            ref DateTime dateOfDelinquency
            )
        {
            Boolean retVal = false;

            if (rawConsumerDebtList[TestDataConfig.RawConsumerDebtColumn.DateOfDelinquency] != "NULL")
            {
                // we have a value in the raw data
                dateOfDelinquency = Convert.ToDateTime(rawConsumerDebtList[TestDataConfig.RawConsumerDebtColumn.DateOfDelinquency]);
                retVal = true;
            }
            else
            {
                // [skt] else: eventually make something up
            }

            return retVal;
        }


        /// <summary>
        /// Extract the interest rate out of the raw debt data
        /// </summary>
        /// <param name="rawConsumerDebtRecord"></param>
        /// <returns></returns>
        private decimal GetInterestRateFromRawDebtData(String[] rawConsumerDebtRecord, Random random)
        {

            if (rawConsumerDebtRecord[TestDataConfig.RawConsumerDebtColumn.InterestRate] != "NULL")
            {              
                return 
                    Convert.ToDecimal(rawConsumerDebtRecord[TestDataConfig.RawConsumerDebtColumn.InterestRate]);
            }
            else
            {
                // make something up that makes sense for interest rate:  Random %age between 0 and 35
                return 
                    Math.Round(Convert.ToDecimal(random.Next(0, 35)) * Convert.ToDecimal(random.NextDouble()), 3);
            }
        }

      
	    /// <summary>
        /// Takes the passed in credit card row and adds it to the DebtCreditCard table
        /// </summary>
        /// <param name="creditCardRow"></param>
        /// <param name="debtConsumerId"></param>
        private void AssignCreditCardToDebtSideConsumer
            (
            DataSetDebtDataGenerator.RealCreditCardRow realCreditCardRow, 
            Int32 debtConsumerId, 
            RandomNumberGenerator random,
            out Int32 debtCreditCardId
            )
        {
            DataSetDebtDataGenerator.DebtCreditCardRow debtCreditcardRow =
                this.dataSetDebtDataGen.DebtCreditCard.NewDebtCreditCardRow();

            // mark it as not fuzzed yet
            debtCreditcardRow.FuzzedFields = Convert.ToInt32(CreditCardFuzzedFields.None);

            // map to the DebtConsumer row's ID
            debtCreditcardRow.DebtSideConsumerId = debtConsumerId;

            // AccountNumber is typically some number associated with the Credit Card and the institution that either issued it or now owns it
            debtCreditcardRow.AccountCode = string.Format("{0:000000000}", random.Next(1000000000));
            debtCreditcardRow.AccountBalance = realCreditCardRow.AccountBalance;
            debtCreditcardRow.RealDebtHolder = realCreditCardRow.RealDebtHolder;
            debtCreditcardRow.RealCreditCardId = realCreditCardRow.RealCreditCardId;
            
            // each credit card needs to be uniquely identified so as not to be overwritten in 
            // the DB (even if the same CC exists on both the Trust and Debt sides
            debtCreditcardRow.ExternalId = Guid.NewGuid().ToString(); 

            debtCreditcardRow.OriginalAccountNumber = realCreditCardRow.OriginalAccountNumber;

            // Add the new row to the DebtCreditCard table
            this.dataSetDebtDataGen.DebtCreditCard.AddDebtCreditCardRow(debtCreditcardRow);

            // make sure we return the Id of this new credit card added 
	        debtCreditCardId = debtCreditcardRow.DebtCreditCardId;
        }

        /// <summary>
        /// Takes the passed in realConsumerRow and adds that consumer's real credit card info to the TrustSideConsumer 
        /// </summary>
        /// <param name="realConsumerRow"></param>
        /// <param name="trustConsumerId"></param>
        private void AssignCreditCardsToTrustSideConsumer(DataSetDebtDataGenerator.RealConsumerRow realConsumerRow, Int32 trustConsumerId)
        {
            // Get the list of Real credit cards associcated with this consumer
            List<DataSetDebtDataGenerator.RealCreditCardRow> realCreditCardList 
                = GetRealCreditCardList(realConsumerRow);

            // Get the Consumer Trust's Consumer record so we can add the CC Id's to its list
            DataSetDebtDataGenerator.TrustSideConsumerRow trustConsumer = 
                this.dataSetDebtDataGen.TrustSideConsumer[trustConsumerId];

            // Add the consumers' Real Credit Cards info to the TrustCreditCard table
            for (Int32 index = 0; index < realCreditCardList.Count; index++)
            {
                DataSetDebtDataGenerator.TrustCreditCardRow trustCreditCardRow =
                    this.dataSetDebtDataGen.TrustCreditCard.NewTrustCreditCardRow();

                trustCreditCardRow.FuzzedFields = Convert.ToInt32(CreditCardFuzzedFields.None);
                
                // map to the TrustConsumer row's ID
                trustCreditCardRow.TrustSideConsumerId = trustConsumerId;

                // use the real credit card's AccountNumber here. We will use a different number on the Debt side
                // to make sure they are different 
                trustCreditCardRow.AccountCode = realCreditCardList[index].AccountCode;
                trustCreditCardRow.AccountBalance = realCreditCardList[index].AccountBalance;
                trustCreditCardRow.RealDebtHolder = realCreditCardList[index].RealDebtHolder;
                trustCreditCardRow.RealCreditCardId = realCreditCardList[index].RealCreditCardId;

                // each credit card needs to be uniquely identified so as not to be overwritten in 
                // the DB (even if the same CC exists on both the Trust and Debt sides
                trustCreditCardRow.ExternalId = Guid.NewGuid().ToString();                
                
                trustCreditCardRow.OriginalAccountNumber = realCreditCardList[index].OriginalAccountNumber;

                // Add the new row to the TrustCreditCard table
                this.dataSetDebtDataGen.TrustCreditCard.AddTrustCreditCardRow(trustCreditCardRow);

                // Update the cache map of RealCreditCards to their Trust-Side representations
                this.trustSideMapOfRealCreditCardIds.Add(realCreditCardList[index].RealCreditCardId, trustCreditCardRow.TrustCreditCardId);

                // Keep track of the credit cards created for this Trust-Side consumer.  It will make it much more efficient
                // to find per-TrustConsumer CCs later on rather than iterating through the entire credit card table, one 
                // record at a time.
                trustConsumer.TrustSideCreditCardList =
                    StringHelpers.AppendTokenToCommaDelimitedString(trustConsumer.TrustSideCreditCardList,
                                                                    trustCreditCardRow.TrustCreditCardId.ToString());
            }
        }
       
        
        /// <summary>
        /// Return a list of all credit card rows in the RealCreditCard table based on the 
        /// passed-in Consumer row
        /// </summary>
        /// <param name="realConsumer"></param>
        /// <returns></returns>
        private List<DataSetDebtDataGenerator.RealCreditCardRow> 
            GetRealCreditCardList(DataSetDebtDataGenerator.RealConsumerRow realConsumer)
        {
            // Initialize the list of RealCreditCardRows we will be returning
            List<DataSetDebtDataGenerator.RealCreditCardRow> realCreditCardList 
                = new List<DataSetDebtDataGenerator.RealCreditCardRow>();
            
            // Get the list of Credit Card Id's this consumer 'owns'
            List <Int32> creditCardIdList =
                DataGenHelpers.GetIdsFromCsvList(realConsumer.RealCreditCardList);

            // Now populate the return value with the acutal list of credit card rows
            foreach (Int32 creditCardId in creditCardIdList)
            {
                realCreditCardList.Add(this.dataSetDebtDataGen.RealCreditCard[creditCardId]);
            }

            return realCreditCardList;
        }

        /// <summary>
        /// Return a list of all credit card rows in the TrustCreditCard table based on the 
        /// passed-in trust-side consumer row
        /// </summary>
        /// <param name="trustConsumer"></param>
        /// <returns></returns>
        private List<DataSetDebtDataGenerator.TrustCreditCardRow>
            GetTrustCreditCardList(DataSetDebtDataGenerator.TrustSideConsumerRow trustConsumer)
        {
            // Initialize the list of TrustCreditCardRows we will be returning
            List<DataSetDebtDataGenerator.TrustCreditCardRow> trustCreditCardList 
                = new List<DataSetDebtDataGenerator.TrustCreditCardRow>();

            // Get the list of Credit Card Id's that this trust consumer 'owns'
            List<Int32> creditCardIdList =
                DataGenHelpers.GetIdsFromCsvList(trustConsumer.TrustSideCreditCardList);

            // Now populate the return value with the acutal list of credit card rows
            foreach (Int32 creditCardId in creditCardIdList)
            {
                trustCreditCardList.Add(this.dataSetDebtDataGen.TrustCreditCard[creditCardId]);
            }

            return trustCreditCardList;
        }


        /// <summary>
        /// Takes the passed in consumer row and adds it to the DebtSideConsumer table.
        /// </summary>
        /// <param name="consumerRow"></param>
        /// <param name="debtSideConsumerId"></param>
        private void AddRealConsumerToDebtSideConsumerTable(DataSetDebtDataGenerator.RealConsumerRow consumerRow, 
                                                            out Int32 debtSideConsumerId)
        {
            var debtSideConsumerRow = this.dataSetDebtDataGen.DebtSideConsumer.NewDebtSideConsumerRow();

            // map to the real consumer ID
            debtSideConsumerRow.RealConsumerId = consumerRow.RealConsumerId;

            debtSideConsumerRow.Address1 = consumerRow.Address1;
            debtSideConsumerRow.Address2 = consumerRow.Address2;
            debtSideConsumerRow.City = consumerRow.City;
            if (!consumerRow.IsDateOfBirthNull())
            {
                debtSideConsumerRow.DateOfBirth = consumerRow.DateOfBirth;
            }

            // [skt] would be nice to also include organization name here just like we use for the xml import files.
            // as it is for now the DebtSideConsumerId is enough of a unique value to use for this external Id
            debtSideConsumerRow.ExternalId = debtSideConsumerRow.DebtSideConsumerId.ToString();
            
            debtSideConsumerRow.FirstName = consumerRow.FirstName;
            debtSideConsumerRow.LastName = consumerRow.LastName;
            debtSideConsumerRow.MiddleName = consumerRow.MiddleName;

            if (!consumerRow.IsIsEmployedNull())
            {
                debtSideConsumerRow.IsEmployed = consumerRow.IsEmployed;
            }

            debtSideConsumerRow.PostalCode = consumerRow.PostalCode;
            debtSideConsumerRow.PhoneNumber = consumerRow.PhoneNumber;
            debtSideConsumerRow.ProvinceCode = consumerRow.ProvinceCode;
            debtSideConsumerRow.SocialSecurityNumber = consumerRow.SocialSecurityNumber;
            debtSideConsumerRow.Salutation = consumerRow.Salutation;
            debtSideConsumerRow.Suffix = consumerRow.Suffix;

            // keep track of what is fuzzed
            debtSideConsumerRow.FuzzedFields = Convert.ToInt32(ConsumerFuzzedFields.None);

            this.dataSetDebtDataGen.DebtSideConsumer.AddDebtSideConsumerRow(debtSideConsumerRow);

            // 'return' the unique Id of the row added
            debtSideConsumerId = debtSideConsumerRow.DebtSideConsumerId;

            // we also need to keep track of the RealConsumerId in case we want to prevent adding 
            // another Credit Card owned by the same consumer. 
            if (!this.debtSideMapOfRealConsumerIds.ContainsKey(consumerRow.RealConsumerId))
            {
                // Add consumer to the map with the first debt side version of the consumer
                this.debtSideMapOfRealConsumerIds.Add(consumerRow.RealConsumerId, debtSideConsumerId.ToString());
            }
            else
            {
                // Real Consumer has already been added to the map, so add the next debt side version of that consumer to it
                this.debtSideMapOfRealConsumerIds[consumerRow.RealConsumerId] =
                    StringHelpers.AppendTokenToCommaDelimitedString
                    (
                        this.debtSideMapOfRealConsumerIds[consumerRow.RealConsumerId],
                        debtSideConsumerId.ToString()
                    );
            }
        }


        /// <summary>
        /// Takes the passed in consumer row and adds it to the TrustSideConsumer table.
        /// </summary>
        /// <param name="consumerRow"></param>
        /// <param name="trustSideConsumerId"></param>
        private void AddRealConsumerToTrustSideConsumerTable(DataSetDebtDataGenerator.RealConsumerRow consumerRow,
                                                             out Int32 trustSideConsumerId)
        {
            var trustSideConsumerRow = this.dataSetDebtDataGen.TrustSideConsumer.NewTrustSideConsumerRow();
            
            // map to the real consumer ID
            trustSideConsumerRow.RealConsumerId = consumerRow.RealConsumerId;
            
            trustSideConsumerRow.Address1 = consumerRow.Address1;
            trustSideConsumerRow.Address2 = consumerRow.Address2; 
            trustSideConsumerRow.City = consumerRow.City;

            if (!consumerRow.IsDateOfBirthNull())
            {
                trustSideConsumerRow.DateOfBirth = consumerRow.DateOfBirth;
            }

            // [skt] would be nice to have organization name here just like we use for the xml import files
            trustSideConsumerRow.ExternalId = trustSideConsumerRow.TrustSideConsumerId.ToString(); 

            trustSideConsumerRow.FirstName = consumerRow.FirstName;
            trustSideConsumerRow.LastName = consumerRow.LastName;
            trustSideConsumerRow.MiddleName = consumerRow.MiddleName;

            if (!consumerRow.IsIsEmployedNull())
            {
                trustSideConsumerRow.IsEmployed = consumerRow.IsEmployed;
            }
            trustSideConsumerRow.PostalCode = consumerRow.PostalCode;
            trustSideConsumerRow.PhoneNumber = consumerRow.PhoneNumber;
            trustSideConsumerRow.ProvinceCode = consumerRow.ProvinceCode;
            trustSideConsumerRow.SocialSecurityNumber = consumerRow.SocialSecurityNumber;
            trustSideConsumerRow.Salutation = consumerRow.Salutation;
            trustSideConsumerRow.Suffix = consumerRow.Suffix;

            trustSideConsumerRow.BankAccountNumber = consumerRow.BankAccountNumber;
            trustSideConsumerRow.BankRoutingNumber = consumerRow.BankRoutingNumber;

            // keep track of what is fuzzed
            trustSideConsumerRow.FuzzedFields = Convert.ToInt32(ConsumerFuzzedFields.None);
            
            this.dataSetDebtDataGen.TrustSideConsumer.AddTrustSideConsumerRow(trustSideConsumerRow);

            // 'return' the unique Id of the row added
            trustSideConsumerId = trustSideConsumerRow.TrustSideConsumerId;
        }


        /// <summary>
        /// Writes the Consumer data to an XML file.  Data comes from two separate tables (Trust side and Debt side) and is
        /// combined into one xml file to be imported by the Guardian Script Loader utility.
        /// </summary>
        private void WriteConsumer()
        {
            XDocument xDocument = new XDocument();
            xDocument.Add(new XElement("script", new XAttribute("name", "Consumer")));
            xDocument.Root.Add(
                new XElement(
                    "client",
                    new XAttribute("name", "DataModelClient"),
                    new XAttribute("type", "DataModelClient, FluidTrade.ClientDataModel"),
                    new XAttribute("endpoint", "TcpDataModelEndpoint")));

            // Trust side
            foreach (DataSetDebtDataGenerator.TrustSideConsumerRow trustConsumer in this.dataSetDebtDataGen.TrustSideConsumer.Rows)
            {
                // Due to the huge number of consumers, each Consumer Record is a seperate transaction.
                XElement transactionElement = new XElement("transaction");
               
                // Create the Consumer record.
                XElement methodElement = new XElement("method", new XAttribute("name", "CreateConsumerEx"), new XAttribute("client", "DataModelClient"));

                // SSN is not optional
                methodElement.Add(new XElement("parameter", new XAttribute("name", "socialSecurityNumber"), new XAttribute("value", trustConsumer.SocialSecurityNumber)));

                // Optional fields:

                if (!trustConsumer.IsFirstNameNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "firstName"), new XAttribute("value", trustConsumer.FirstName)));
                
                if (!trustConsumer.IsLastNameNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "lastName"), new XAttribute("value", trustConsumer.LastName)));
                
                if (!trustConsumer.IsSalutationNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "salutation"), new XAttribute("value", trustConsumer.Salutation)));
                
                if (!trustConsumer.IsMiddleNameNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "middleName"), new XAttribute("value", trustConsumer.MiddleName)));
                
                if (!trustConsumer.IsSuffixNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "suffix"), new XAttribute("value", trustConsumer.Suffix)));

                if (!trustConsumer.IsAddress1Null())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "address1"), new XAttribute("value", trustConsumer.Address1)));
                
                if (!trustConsumer.IsAddress2Null())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "address2"), new XAttribute("value", trustConsumer.Address2)));
                
                if (!trustConsumer.IsCityNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "city"), new XAttribute("value", trustConsumer.City)));
                
                if (!trustConsumer.IsDateOfBirthNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "dateOfBirth"), new XAttribute("value", trustConsumer.DateOfBirth)));
                
                if (!trustConsumer.IsIsEmployedNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "isEmployed"), new XAttribute("value", trustConsumer.IsEmployed)));
                
                if (!trustConsumer.IsPostalCodeNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "postalCode"), new XAttribute("value", trustConsumer.PostalCode)));
                
                if (!trustConsumer.IsPhoneNumberNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "phoneNumber"), new XAttribute("value", trustConsumer.PhoneNumber)));
                
                if (!trustConsumer.IsProvinceCodeNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "provinceKey"), new XAttribute("value", trustConsumer.ProvinceCode)));

                if (!trustConsumer.IsBankAccountNumberNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "bankAccountNumber"), new XAttribute("value", trustConsumer.BankAccountNumber)));
                
                if (!trustConsumer.IsBankRoutingNumberNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "bankRoutingNumber"), new XAttribute("value", trustConsumer.BankRoutingNumber)));

                // Now add the Trust-side specific parameters

                // The Consumer Trust records can have identical SSN to the Consumer Debt records.  In order to keep one set of consumers from overwriting
                // the other set when the data is loaded, the configuration parameter is used to drive the load process to use different external identifier
                // pools for importing.
                methodElement.Add(new XElement("parameter", new XAttribute("name", "configurationId"), new XAttribute("value", "Default")));

                // The Consumers added through the Consumer Trust database will use a different external identifier than the ones entered through the Consumer
                // Debt side of the transaction.  
                methodElement.Add(new XElement("parameter", new XAttribute("name", "externalId0"), new XAttribute("value", trustConsumer.ExternalId)));
    
                
                // This completes the transaction.
                transactionElement.Add(methodElement);
                xDocument.Root.Add(transactionElement);
            }

            // Debt side
            foreach (DataSetDebtDataGenerator.DebtSideConsumerRow debtConsumer in this.dataSetDebtDataGen.DebtSideConsumer.Rows)
            {
                // Due to the huge number of consumers, each Consumer Record is a seperate transaction.
                XElement transactionElement = new XElement("transaction");

                // Create the Consumer record.
                XElement methodElement = new XElement("method", new XAttribute("name", "CreateConsumerEx"), new XAttribute("client", "DataModelClient"));

                // SSN is not optional:
                methodElement.Add(new XElement("parameter", new XAttribute("name", "socialSecurityNumber"), new XAttribute("value", debtConsumer.SocialSecurityNumber)));

                // Add common method elements (parameters)
                if (!debtConsumer.IsFirstNameNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "firstName"), new XAttribute("value", debtConsumer.FirstName)));

                if (!debtConsumer.IsLastNameNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "lastName"), new XAttribute("value", debtConsumer.LastName)));

                if (!debtConsumer.IsSalutationNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "salutation"), new XAttribute("value", debtConsumer.Salutation)));

                if (!debtConsumer.IsMiddleNameNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "middleName"), new XAttribute("value", debtConsumer.MiddleName)));

                if (!debtConsumer.IsSuffixNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "suffix"), new XAttribute("value", debtConsumer.Suffix)));

                if (!debtConsumer.IsAddress1Null())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "address1"), new XAttribute("value", debtConsumer.Address1)));
                
                if (!debtConsumer.IsAddress2Null())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "address2"), new XAttribute("value", debtConsumer.Address2)));
                
                if (!debtConsumer.IsCityNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "city"), new XAttribute("value", debtConsumer.City)));
                
                if (!debtConsumer.IsDateOfBirthNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "dateOfBirth"), new XAttribute("value", debtConsumer.DateOfBirth)));
                
                if (!debtConsumer.IsIsEmployedNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "isEmployed"), new XAttribute("value", debtConsumer.IsEmployed)));
                
                if (!debtConsumer.IsPostalCodeNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "postalCode"), new XAttribute("value", debtConsumer.PostalCode)));
                
                if (!debtConsumer.IsPhoneNumberNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "phoneNumber"), new XAttribute("value", debtConsumer.PhoneNumber)));
                
                if (!debtConsumer.IsProvinceCodeNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "provinceKey"), new XAttribute("value", debtConsumer.ProvinceCode)));
                //
                // Now add the Debt-side specific parameters
                //

                // The Consumer Trust records can have identical SSN to the Consumer Debt records.  In order to keep one set of consumers from overwriting
                // the other set when the data is loaded, the configuration parameter is used to drive the load process to use different external identifier
                // pools for importing.
                methodElement.Add(new XElement("parameter", new XAttribute("name", "configurationId"), new XAttribute("value", "DEBT HOLDER")));

                // The Consumers added through the Consumer Trust database will use a different external identifier than the ones entered through the Consumer
                // Debt side of the transaction. 
                methodElement.Add(new XElement("parameter", new XAttribute("name", "externalId1"), new XAttribute("value", debtConsumer.ExternalId)));

                // This completes the transaction.
                transactionElement.Add(methodElement);
                xDocument.Root.Add(transactionElement);
            }

            // Write the contents of the XDocument to disk.
            xDocument.Save(Properties.Settings.Default.DataOutputLocation +
                           Properties.Settings.Default.ConsumerOutput);
        }


        /// <summary>        
        /// Add the Consumer Debt (Debt Holder) credit card info to the CreditCard data file
        /// </summary>
        /// <param name="debtCreditCardRow"></param>
        /// <param name="creditCardOutputFile"></param>
        private void AddConsumerDebtCreditCardDataToOutputDoc(
            DataSetDebtDataGenerator.DebtCreditCardRow debtCreditCardRow,
            DataSetDebtDataGenerator.OrganizationRow organizationRow,
            XDocument creditCardOutputFile)
        {           
            // Each Credit Card is a transaction
            XElement transactionElement = new XElement("transaction");

            // Create the Credit Card record.
            XElement methodElement = new XElement("method", new XAttribute("name", "CreateCreditCardEx"), new XAttribute("client", "DataModelClient"));

            // The Consumer Trust records can have identical SSN to the Consumer Debt records.  In order to keep one set of consumers from overwriting
            // the other set when the data is loaded, the configuration parameter is used to drive the load process to use different external identifier
            // pools for importing.
            methodElement.Add(new XElement("parameter", new XAttribute("name", "configurationId"), new XAttribute("value", "DEBT HOLDER"))); //Debt-side="DEBT HOLDER" todo: get rid of this hardcoded string

            // A credit card's Isssuer just some "tag" field and is the same no matter which side it is on (trust or debt or real) 
            if (!debtCreditCardRow.IsRealDebtHolderNull())
            {
                methodElement.Add(new XElement("parameter", new XAttribute("name", "debtHolder"),
                                               new XAttribute("value", debtCreditCardRow.RealDebtHolder)));
            }

            methodElement.Add(new XElement("parameter", new XAttribute("name", "consumerKey"), new XAttribute("value", debtCreditCardRow.DebtSideConsumerRow.ExternalId)));
            methodElement.Add(new XElement("parameter", new XAttribute("name", "accountNumber"), new XAttribute("value", debtCreditCardRow.AccountCode)));
            methodElement.Add(new XElement("parameter", new XAttribute("name", "originalAccountNumber"), new XAttribute("value", debtCreditCardRow.OriginalAccountNumber)));

            methodElement.Add(new XElement("parameter", new XAttribute("name", "externalId0"), new XAttribute("value", debtCreditCardRow.ExternalId)));
            methodElement.Add(new XElement("parameter", new XAttribute("name", "externalId1"), new XAttribute("value", debtCreditCardRow.ExternalId)));
            methodElement.Add(new XElement("parameter", new XAttribute("name", "tenantKey"), new XAttribute("value", organizationRow.TenantId)));

            if (!debtCreditCardRow.IsAccountBalanceNull())
                methodElement.Add(new XElement("parameter", new XAttribute("name", "accountBalance"), new XAttribute("value", debtCreditCardRow.AccountBalance)));

            // This completes the transaction.
            transactionElement.Add(methodElement);
            creditCardOutputFile.Root.Add(transactionElement);          
        }


        /// <summary>
        /// Add the Consumer Trust (Debt Negotiator) credit card info to the CreditCard data file
        /// </summary>
        /// <param name="consumerTrustRow"></param>
        /// <param name="organizationRow"></param>
        /// <param name="creditCardOutputFile"></param>
        private void AddConsumerTrustCreditCardDataToOutputDoc(
            DataSetDebtDataGenerator.ConsumerTrustRow consumerTrustRow,
            DataSetDebtDataGenerator.OrganizationRow organizationRow,
            XDocument creditCardOutputFile)
        {
            // Get the credit cards associated with this trust record
            var trustCreditCards = GetTrustCreditCardList(consumerTrustRow.TrustSideConsumerRow);
          
            // for evey credit card that this consumer has in the trust record:
            foreach (var trustCreditCardRow in trustCreditCards)
            {
                // Each Credit Card is a transaction
                XElement transactionElement = new XElement("transaction");

                // Create the Credit Card record.
                XElement methodElement = new XElement("method", new XAttribute("name", "CreateCreditCardEx"), new XAttribute("client", "DataModelClient"));

                // The Consumer Trust records can have identical SSN to the Consumer Debt records.  In order to keep one set of consumers from overwriting
                // the other set when the data is loaded, the configuration parameter is used to drive the load process to use different external identifier
                // pools for importing.
                methodElement.Add(new XElement("parameter", new XAttribute("name", "configurationId"), new XAttribute("value", "Default"))); //Trust-side="default"

                // A credit card's Isssuer just some "tag" field and is the same no matter which side it is on (trust or debt or real)
                if (!trustCreditCardRow.IsRealDebtHolderNull())
                {
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "debtHolder"),
                                                   new XAttribute("value", trustCreditCardRow.RealDebtHolder)));
                }

                methodElement.Add(new XElement("parameter", new XAttribute("name", "consumerKey"),
                                                            new XAttribute("value", trustCreditCardRow.TrustSideConsumerRow.ExternalId)));

                methodElement.Add(new XElement("parameter", new XAttribute("name", "accountNumber"), new XAttribute("value", trustCreditCardRow.AccountCode)));
                methodElement.Add(new XElement("parameter", new XAttribute("name", "originalAccountNumber"), new XAttribute("value", trustCreditCardRow.OriginalAccountNumber)));
                methodElement.Add(new XElement("parameter", new XAttribute("name", "externalId1"), new XAttribute("value", trustCreditCardRow.ExternalId)));
                methodElement.Add(new XElement("parameter", new XAttribute("name", "tenantKey"), new XAttribute("value", organizationRow.TenantId)));

                if (!trustCreditCardRow.IsAccountBalanceNull())
                    methodElement.Add(new XElement("parameter", new XAttribute("name", "accountBalance"), new XAttribute("value", trustCreditCardRow.AccountBalance)));

                // This completes the transaction.
                transactionElement.Add(methodElement);
                creditCardOutputFile.Root.Add(transactionElement);
            }
        }


        /// <summary>
        /// Set up the Credit Card document that gets consumed by the script loader 
        /// </summary>
        /// <returns></returns>
        private XDocument InitializeCreditCardOutputFile()
        {
            // The Credit Card data is written out to a file format that is compatible with the Guardian script loader utility.
            XDocument creditCardDocument = new XDocument();
            creditCardDocument.Add(new XElement("script", new XAttribute("name", "Credit Card")));
            creditCardDocument.Root.Add(
                new XElement(
                    "client",
                    new XAttribute("name", "DataModelClient"),
                    new XAttribute("type", "DataModelClient, FluidTrade.ClientDataModel"),
                    new XAttribute("endpoint", "TcpDataModelEndpoint")));

            return creditCardDocument;
        }

        /// <summary>
        /// Saves the contents of the Credit Card output file we have been building out to disk
        /// </summary>
        /// <param name="creditCardOutputFile"></param>
        private void WriteCreditCardDataFile(XDocument creditCardOutputFile)
        {
            // Write the contents of the XDocument to disk.
            creditCardOutputFile.Save(Properties.Settings.Default.DataOutputLocation + 
                Properties.Settings.Default.CreditCardOutput);
        }

        /// <summary>
        /// Write the Consumer Debt (Debt Holder) and associated CreditCard information to an XML file to be 
        /// consumed by Script Loader
        /// </summary>
        /// <param name="creditCardDocument"></param>
        private void WriteConsumerDebtOutputData(XDocument creditCardDocument)
        {
            // Create the Consumer Debt Output file
            XDocument consumerDebtOutputFile = CreateNewScriptLoaderInputFile(TestDataConfig.OrganizationType.DebtHolder);

            // Distribute the ConsumerDebt orders evenly across Debt Holder organizations (tenants)
            foreach (DataSetDebtDataGenerator.OrganizationRow organizationRow in this.dataSetDebtDataGen.Organization)
            {
                // Only care about DebtHolder organizations here
                if (organizationRow.Type == TestDataConfig.OrganizationType.DebtHolder)
                {
                    // Create the output file that will contain the working orders for this Debt Holder organization
                    XDocument workingOrderOutputFile = CreateNewScriptLoaderInputFile(organizationRow.Name + " Orders");

                    // Get the list of Debt Classes associated with the organization (so we can evenly divy up orders between them)
                    List<String> debtClasses = StringHelpers.GetTokensFromCommaDelimitedString(organizationRow.DebtClassList);
                    
                    Int32 debtClassIndex = 0;

                    // Index used to grab a Debt record out of a randomly shuffled list
                    Int32 rndDebtHolderIndex;

                    for (Int32 index = organizationRow.StartingOrderId; index <= organizationRow.EndingOrderId; index++)
                    {
                        // get the next debt row from the randomly shuffled list
                        rndDebtHolderIndex = this.consumerDebtIds[index];
                        DataSetDebtDataGenerator.ConsumerDebtRow consumerDebtRow =
                            this.dataSetDebtDataGen.ConsumerDebt[rndDebtHolderIndex];

                        // Wrap the related records in a transaction.
                        XElement transactionElement = new XElement("transaction");

                        // These methods are to be committed as a unit and represent a "Consumer Debt" security.
                        transactionElement.Add(ConsumerDebt.CreateEntityRecord(consumerDebtRow, organizationRow));
                        transactionElement.Add(ConsumerDebt.CreateSecurityRecord(consumerDebtRow, organizationRow));
                        transactionElement.Add(ConsumerDebt.CreateConsumerDebtRecord(consumerDebtRow, organizationRow));

                        // This completes the transaction for the ConsumerDebt record
                        consumerDebtOutputFile.Root.Add(transactionElement);

                        // Now add the associated Credit Card record to the Credit Card document 
                        // (there is always only 1 CC per Debt Holder record)
                        AddConsumerDebtCreditCardDataToOutputDoc(consumerDebtRow.DebtCreditCardRow, organizationRow, creditCardDocument);
                        
                        // Write out the appropriate working order for the current organization (tenant) we are working with here.  
                        // Working orders themselves are not tenanted but this DataGen application divides up the working orders 
                        // accross output files named after the organizations.  This gives us some flexibility when using script 
                        // loader control which organizations get working orders loaded for them or not.
                        GenerateDebtHolderWorkingOrder(debtClasses, debtClassIndex, workingOrderOutputFile, organizationRow, consumerDebtRow);

                        // circular index through the available debt classes
                        debtClassIndex = (++debtClassIndex >= debtClasses.Count ? 0 : debtClassIndex); 
                    }

                    // Write out the corresponding working order file for the current organization
                    WriteWorkingOrdersToDisk(organizationRow, workingOrderOutputFile);
                }
            }

            // Write the Consumer Debt (DebtHolder) output to disk. This set of data is used to load the ConsumerDebt table 
            consumerDebtOutputFile.Save(Properties.Settings.Default.DataOutputLocation + 
                Properties.Settings.Default.ConsumerDebtOutput);
        }


        /// <summary>
        /// Write the Consumer Trust (Debt Negotiator) and associated CreditCard information to an XML file to be 
        /// consumed by Script Loader
        /// </summary>
        /// <param name="creditCardDocument"></param>
        private void WriteConsumerTrustOutputData(XDocument creditCardDocument)
        {
            // Create the Consumer Trust Output file
            XDocument consumerTrustOutputFile = CreateNewScriptLoaderInputFile(TestDataConfig.OrganizationType.DebtNegotiator);

            // Distribute the ConsumerTrust orders evenly across Debt Negotiator organizations (tenants)
            foreach (DataSetDebtDataGenerator.OrganizationRow organizationRow in this.dataSetDebtDataGen.Organization)
            {
                // Only care about Debt Negotiator organizations here
                if (organizationRow.Type == TestDataConfig.OrganizationType.DebtNegotiator)
                {
                    // Create the output file that will contain the working orders for this Debt Holder organization
                    XDocument workingOrderOutputFile = CreateNewScriptLoaderInputFile(organizationRow.Name + " Orders");

                    // Get the list of Debt Classes associated with the organization (so we can evenly divy up orders between them)
                    List<String> debtClasses = StringHelpers.GetTokensFromCommaDelimitedString(organizationRow.DebtClassList);

                    Int32 debtClassIndex = 0;

                    // Index used to grab a Debt record out of a randomly shuffled list
                    Int32 rndDebtHolderIndex;

                    for (Int32 index = organizationRow.StartingOrderId; index <= organizationRow.EndingOrderId; index++)
                    {
                        // get the next trust row from the randomly shuffled list
                        rndDebtHolderIndex = this.consumerTrustIds[index];
                        DataSetDebtDataGenerator.ConsumerTrustRow consumerTrustRow = 
                            this.dataSetDebtDataGen.ConsumerTrust[rndDebtHolderIndex];

                        // Wrap the related records in a transaction.
                        XElement transactionElement = new XElement("transaction");

                        // These methods are to be committed as a unit and represent a "Consumer Debt" security.
                        transactionElement.Add(ConsumerTrust.CreateEntityRecord(consumerTrustRow, organizationRow));
                        transactionElement.Add(ConsumerTrust.CreateSecurityRecord(consumerTrustRow, organizationRow));
                        transactionElement.Add(ConsumerTrust.CreateConsumerTrustRecord(consumerTrustRow, organizationRow));

                        // This completes the transaction for the ConsumerTrust record
                        consumerTrustOutputFile.Root.Add(transactionElement);

                        // Now add the associated Credit Card record to the Credit Card document 
                        // (there is always only 1 CC per Debt Holder record)
                        AddConsumerTrustCreditCardDataToOutputDoc(consumerTrustRow, organizationRow, creditCardDocument);

                        // Write out the appropriate working order for the curreht organization (tenant) we are working with here.  
                        // Working orders themselves are not tenanted but this DataGen application divides up the working orders 
                        // accross output files named after the organizations.  This gives us some flexibility when using script 
                        // loader control which organizations get working orders loaded for them or not.
                        GenerateDebtNegotiatorWorkingOrder(debtClasses, debtClassIndex, workingOrderOutputFile, organizationRow, consumerTrustRow);

                        // circular index through the available debt classes
                        debtClassIndex = (++debtClassIndex >= debtClasses.Count ? 0 : debtClassIndex); 
                    }

                    // Write out the corresponding working order file for the current organization
                    WriteWorkingOrdersToDisk(organizationRow, workingOrderOutputFile);
                }
            }

            // Write the contents of the XDocument to disk.
            consumerTrustOutputFile.Save(Properties.Settings.Default.DataOutputLocation +
                           Properties.Settings.Default.ConsumerTrustOutput);
        }


	    /// <summary>
	    /// Returns the number of organizations of a particular type.
	    /// </summary>
	    /// <param name="organizationType"></param>
	    /// <returns></returns>
	    private Int32 GetNumberOfOrganizations(String organizationType)
	    {
	        Int32 count = 0;

	        // Go through each row in the Organization table and see if it is the type we are looking for
	        foreach (DataSetDebtDataGenerator.OrganizationRow organizationRow in this.dataSetDebtDataGen.Organization)
	        {
	            if (organizationRow.Type == organizationType)
	            {
	                ++count;
	            }
	        }

	        return count;
	    }


	    /// <summary>
        /// Initialize a file compatible with the Script Loader input format
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        private XDocument CreateNewScriptLoaderInputFile(String scriptName) 
        {
	        XDocument inputFile = new XDocument();

            if (scriptName.ToLower() == TestDataConfig.OrganizationType.DebtNegotiator.ToLower())
            {
                scriptName = "Consumer Trust";
            }
            if (scriptName.ToLower() == TestDataConfig.OrganizationType.DebtHolder.ToLower())
            {
                scriptName = "Consumer Debt";
            }

	        // The data is written out to a file format that is compatible with the Guardian script loader utility.
            inputFile.Add(new XElement("script", new XAttribute("name", scriptName)));
            inputFile.Root.Add(
                new XElement(
                    "client",
                    new XAttribute("name", "DataModelClient"),
                    new XAttribute("type", "DataModelClient, FluidTrade.ClientDataModel"),
                    new XAttribute("endpoint", "TcpDataModelEndpoint")));

            return inputFile;
        }


	    /// <summary>
        /// Takes the passed-in working order doc and divides the working orders in it among 
        /// separate files 
        /// </summary>
        /// <param name="originalWorkingOrderDoc"></param>
        /// <param name="organizationName"></param>
        private void SplitUpWorkingOrderFile(XDocument originalWorkingOrderDoc, String organizationName)
        {
            // one (or fewer) files really isn't divying up the orders into multiple files...just return
            if (Properties.Settings.Default.NumberOfWorkingOrderFilesPerOrganization <= 1)
                return;

            // Generate an array of working order docs, the length of which is based on the config setting'
            // that dictates how many separate working order files we should generate per organization
            XDocument[] workingOrderDocs =
                GetNewWorkingOrderDocuments(
                Properties.Settings.Default.NumberOfWorkingOrderFilesPerOrganization,
                organizationName);

            // Get the transaction elements from the document.  Each transaction element in this xml file equates
            // to a working order.
            IEnumerable<XElement> originalTransactions = 
                from element in originalWorkingOrderDoc.Root.Elements()
                where element.Name == "transaction"
                select element;

	        Int32 docIndex = 0;

            // for all the original transactions: 
            foreach (XElement transaction in originalTransactions)
            {
                // divy them up between the newly created files
                workingOrderDocs[docIndex].Root.Add(transaction);

                // circular index through the available files
                docIndex = (++docIndex >= workingOrderDocs.Length ? 0 : docIndex); 
            }   
       
            // reset docIndex to help name our files
	        docIndex = 1;

            // now save each document to disk
	        foreach (XDocument doc in workingOrderDocs)
	        {
                doc.Save(Properties.Settings.Default.DataOutputLocation + 
                    organizationName + " Orders_" + (docIndex++) + ".xml");
	        }
        }

	    /// <summary>
	    /// Write out organization-specific working order data to disk
	    /// </summary>
	    /// <param name="organizationRow"></param>
	    /// <param name="workingOrderOutputFile"></param>
	    private void WriteWorkingOrdersToDisk(
	        DataSetDebtDataGenerator.OrganizationRow organizationRow,
	        XDocument workingOrderOutputFile)
	    {
	        // Write out the Organization-specific script file to disk
	        workingOrderOutputFile.Save(Properties.Settings.Default.DataOutputLocation + organizationRow.Name + " Orders.xml");

	        // See if we need to split up the organization's working orders into a bunch of small
	        // ones.  this helps us tests importing them in parallel via script loader
	        if (Properties.Settings.Default.SplitUpOrganizationWorkingOrders)
	        {
	            SplitUpWorkingOrderFile(workingOrderOutputFile, organizationRow.Name);
	        }
	    }

	    /// <summary>
        /// Return an array of initialized working order documents
        /// </summary>
        /// <param name="numDocs"></param>
        /// <returns></returns>
        public XDocument[] GetNewWorkingOrderDocuments(Int32 numDocs, String name)
        {
            XDocument[] docs = new XDocument[numDocs];

            for (Int32 index = 0; index < numDocs; index++)
            {
                docs[index] =
                    CreateNewScriptLoaderInputFile(name + " Orders_" + (index + 1).ToString());
            }

            return docs;
        }


	    /// <summary>
	    /// Writes out the flat files used to test the Import functionality that customers will use to 
	    /// load the data into the system
	    /// </summary>
	    /// <param name="random"></param>
	    private void WriteFlatImportFiles(RandomNumberGenerator random)
	    {
	        Int32 numDebtHolderOrganizations =
	            GetNumberOfOrganizations(TestDataConfig.OrganizationType.DebtHolder);

	        Int32 numDebtNegotiatorOrganizations =
	            GetNumberOfOrganizations(TestDataConfig.OrganizationType.DebtNegotiator);

            
	        // For each organization in the Organization table:
	        foreach (DataSetDebtDataGenerator.OrganizationRow organizationRow in this.dataSetDebtDataGen.Organization)
	        {
                // ** Now do the Organization Type-specific work **

	            if (organizationRow.Type == TestDataConfig.OrganizationType.DebtNegotiator)
	            {
	                // XML
	                GenerateDebtNegotiatorXmlImportFile(random, organizationRow);
                    
	                // CSV
	                //WriteDebtNegotiatorCsvInput(random, organizationRow.Name);
	            }
	            else if (organizationRow.Type == TestDataConfig.OrganizationType.DebtHolder)
	            {
	                // XML
	                GenerateDebtHolderXmlImportFile(random, organizationRow);

	                // CSV
	                //WriteDebtHolderCsvInput(random, organizationRow.Name);
	            }
	        }

	        // Write out the Debt Negotiator schema in a seperate file
	        this.dataSetDebtNegotiators[0].DebtNegotiatorRecord.WriteXmlSchema(
	            new StreamWriter(Properties.Settings.Default.DataOutputLocation + "DebtNegotiatorImportSchema" + ".xsd"));

	        // write out the Debt Holder schema in a separate file
	        this.dataSetDebtHolders[0].DebtHolderRecord.WriteXmlSchema(
	            new StreamWriter(Properties.Settings.Default.DataOutputLocation + "DebtHolderImportSchema" + ".xsd"));
	    }

	    /// <summary>
	    /// Write out the Trust-side, XML-based data that will be used for importing into the DB.  This is 
	    /// essentially the data format that Debt Negotiators will be using to populate the system.
	    /// </summary>
	    /// <param name="random"></param>
	    private void GenerateDebtNegotiatorXmlImportFile(
            RandomNumberGenerator random,
            DataSetDebtDataGenerator.OrganizationRow organizationRow)
	    {
	        DebtNegotiator.DebtNegotiatorRecordRow negotiatorRecord;

	        // make sure the table(s) are clear since this method can be called from within a loop.
            for (Int32 i = 0; i < Properties.Settings.Default.NumberOfImportFilesPerOrganization; i++)
            {
                this.dataSetDebtNegotiators[i].DebtNegotiatorRecord.Clear();
            }

	        Int32 rndConsumerTrustIndex = 0;

	        Int32 importFileIndex = 0; 

	        // create an xml-based record for each ConsumerTrust's CreditCard row based on the sequence specified in the organization table
	        for (Int32 index = organizationRow.StartingOrderId; index <= organizationRow.EndingOrderId; index++)
	        {
	            // get the next trust row from the randomly shuffled list
                rndConsumerTrustIndex = this.consumerTrustIds[index];
                DataSetDebtDataGenerator.ConsumerTrustRow trustRow = 
                    this.dataSetDebtDataGen.ConsumerTrust[rndConsumerTrustIndex];

	            // get the credit cards associated with this trust record
	            var trustCreditCards = GetTrustCreditCardList(trustRow.TrustSideConsumerRow);
              
	            // for evey credit card that this consumer has in the trust record:
	            foreach (var trustCreditCardRow in trustCreditCards)
	            {
	                // create a new row
                    negotiatorRecord = this.dataSetDebtNegotiators[importFileIndex].DebtNegotiatorRecord.NewDebtNegotiatorRecordRow();
                    
	                // required fields 
	                negotiatorRecord.SocialSecurityNumber = trustRow.TrustSideConsumerRow.SocialSecurityNumber;
	                negotiatorRecord.OriginalAccountNumber = trustCreditCardRow.OriginalAccountNumber;

	                negotiatorRecord.AccountCode = trustCreditCardRow.AccountCode;
	                negotiatorRecord.AccountBalance = trustCreditCardRow.AccountBalance;

	                // 'optional' fields - Consumer
	                negotiatorRecord.Salutation = trustRow.TrustSideConsumerRow.Salutation;
	                negotiatorRecord.FirstName = trustRow.TrustSideConsumerRow.FirstName;
	                negotiatorRecord.LastName = trustRow.TrustSideConsumerRow.LastName;
	                negotiatorRecord.MiddleName = trustRow.TrustSideConsumerRow.MiddleName;
	                negotiatorRecord.Suffix = trustRow.TrustSideConsumerRow.Suffix;
	                negotiatorRecord.Address1 = trustRow.TrustSideConsumerRow.Address1;
	                negotiatorRecord.Address2 = trustRow.TrustSideConsumerRow.Address2;
	                negotiatorRecord.City = trustRow.TrustSideConsumerRow.City;
	                negotiatorRecord.PostalCode = trustRow.TrustSideConsumerRow.PostalCode;
	                negotiatorRecord.PhoneNumber = trustRow.TrustSideConsumerRow.PhoneNumber;
	                negotiatorRecord.ProvinceCode = trustRow.TrustSideConsumerRow.ProvinceCode;
                   
	                // assign a unique customer code which is unique per trust organization, hence using organization name 
	                // as a part of the ID string.
                    negotiatorRecord.CustomerCode = String.Format("{0}-{1}", organizationRow.Name, trustRow.CustomerCode);
                    
                    if (!trustRow.TrustSideConsumerRow.IsDateOfBirthNull())
                    {
                        // ensure a date string with format yyyy-mm-dd
                        negotiatorRecord.DateOfBirth = trustRow.TrustSideConsumerRow.DateOfBirth.ToString("u").Substring(0, 10);
                    }

	                if (!trustRow.TrustSideConsumerRow.IsIsEmployedNull())
	                    negotiatorRecord.IsEmployed = trustRow.TrustSideConsumerRow.IsEmployed;

	                // 'optional' fields - CreditCard

                    negotiatorRecord.DebtHolder = trustCreditCardRow.RealDebtHolder;
	                    
	                // 'optional' fields - ConsumerTrust

	                negotiatorRecord.SavingsBalance = trustRow.SavingsBalance;
	                negotiatorRecord.SavingsEntityCode = trustRow.SavingsEntityCode;
	                negotiatorRecord.BankAccountNumber = trustRow.TrustSideConsumerRow.BankAccountNumber;
	                negotiatorRecord.BankRoutingNumber = trustRow.TrustSideConsumerRow.BankRoutingNumber;

	                // Vendor Code (unique per debt holder organization)
	                negotiatorRecord.VendorCode = organizationRow.Name;

	                // add the row 
                    this.dataSetDebtNegotiators[importFileIndex].DebtNegotiatorRecord.AddDebtNegotiatorRecordRow(negotiatorRecord);
	            }

                // circular index through the available import file 'slots'
                importFileIndex = (++importFileIndex >= Properties.Settings.Default.NumberOfImportFilesPerOrganization
                    ? 0 : importFileIndex); 
	        }

	        Stopwatch s = new Stopwatch();
	        s.Restart();
            
            // write out the import files to disk
	        WriteFlatImportFilesToDisk(organizationRow.Name, TestDataConfig.OrganizationType.DebtNegotiator);

            s.Stop();

	        this.durationTracker.MarkCheckpoint(organizationRow.Name + " xml dump", s.Elapsed.TotalMinutes);
	    }


        /// <summary>
        /// Write the flat import file(s) out to disk
        /// </summary>
        /// <param name="organizationName"></param>
        /// <param name="organizationType"></param>
        private void WriteFlatImportFilesToDisk(String organizationName, String organizationType)
        {
            if (Properties.Settings.Default.NumberOfImportFilesPerOrganization > 1)
            {
                // we have to write out multiple files, give them a unique number in the file name
                for (Int32 importFileIndex = 0;
                     importFileIndex < Properties.Settings.Default.NumberOfImportFilesPerOrganization;
                     importFileIndex++)
                {
                    // Write the XML representation of the tables out to disk
                    using (StreamWriter sw =
                        new StreamWriter(Properties.Settings.Default.DataOutputLocation + organizationName +
                                         " Import_" + (importFileIndex + 1) + ".xml"))
                    {
                        // Write out the xml import file
                        if (organizationType == TestDataConfig.OrganizationType.DebtNegotiator)
                        {
                            this.dataSetDebtNegotiators[importFileIndex].DebtNegotiatorRecord.WriteXml(sw);
                        }
                        if (organizationType == TestDataConfig.OrganizationType.DebtHolder)
                        {
                            this.dataSetDebtHolders[importFileIndex].DebtHolderRecord.WriteXml(sw);
                        }
                    }
                }
            }
            else 
            {
                // we have just one file, don't put any numbers in the file name
                using (StreamWriter sw =
                    new StreamWriter(Properties.Settings.Default.DataOutputLocation + organizationName +
                                     " Import.xml"))
                {
                    // Write out the xml import file
                    if (organizationType == TestDataConfig.OrganizationType.DebtNegotiator)
                    {
                        // Write out the xml import file
                        this.dataSetDebtNegotiators[0].DebtNegotiatorRecord.WriteXml(sw);
                    }
                    if (organizationType == TestDataConfig.OrganizationType.DebtHolder)
                    {
                        this.dataSetDebtHolders[0].DebtHolderRecord.WriteXml(sw);
                    }
                }
            }
        }


	    /// <summary>
        /// Write out the Debt-side, XML-based data that will be used for importing into the DB.  This is 
        /// essentially the data format that Debt Holders will be using to populate the system.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="organizationName"></param>
        private void GenerateDebtHolderXmlImportFile(
            RandomNumberGenerator random, 
            DataSetDebtDataGenerator.OrganizationRow organizationRow)
        {
            // make sure the table(s) are clear since this method can be called from within a loop.
            for (Int32 i = 0; i < Properties.Settings.Default.NumberOfImportFilesPerOrganization; i++)
            {
                this.dataSetDebtHolders[i].DebtHolderRecord.Clear();
            }

	        Int32 rndDebtHolderIndex = 0;

            Int32 importFileIndex = 0; 


            // create an xml-based record for each ConsumerDebt row in the sequence specified in the organization table
            for (Int32 index = organizationRow.StartingOrderId; index <= organizationRow.EndingOrderId; index++)
            {
                // get the next debt row from the randomly shuffled list
                rndDebtHolderIndex = this.consumerDebtIds[index];
                DataSetDebtDataGenerator.ConsumerDebtRow debtRow = 
                    this.dataSetDebtDataGen.ConsumerDebt[rndDebtHolderIndex];

                // create a new holder record row
                DebtHolder.DebtHolderRecordRow holderRecord = this.dataSetDebtHolders[importFileIndex].DebtHolderRecord.NewDebtHolderRecordRow();

                // required fields 
                holderRecord.SocialSecurityNumber = debtRow.DebtCreditCardRow.DebtSideConsumerRow.SocialSecurityNumber;
                holderRecord.OriginalAccountNumber = debtRow.DebtCreditCardRow.OriginalAccountNumber;

                // 'optional' fields - Consumer
                holderRecord.FirstName = debtRow.DebtCreditCardRow.DebtSideConsumerRow.FirstName;
                holderRecord.LastName = debtRow.DebtCreditCardRow.DebtSideConsumerRow.LastName;
                holderRecord.MiddleName = debtRow.DebtCreditCardRow.DebtSideConsumerRow.MiddleName;
                holderRecord.Suffix = debtRow.DebtCreditCardRow.DebtSideConsumerRow.Suffix;
                holderRecord.Address1 = debtRow.DebtCreditCardRow.DebtSideConsumerRow.Address1;
                holderRecord.Address2 = debtRow.DebtCreditCardRow.DebtSideConsumerRow.Address2;
                holderRecord.City = debtRow.DebtCreditCardRow.DebtSideConsumerRow.City;
                holderRecord.PostalCode = debtRow.DebtCreditCardRow.DebtSideConsumerRow.PostalCode;
                holderRecord.PhoneNumber = debtRow.DebtCreditCardRow.DebtSideConsumerRow.PhoneNumber;
                holderRecord.ProvinceCode = debtRow.DebtCreditCardRow.DebtSideConsumerRow.ProvinceCode;

                if (!debtRow.DebtCreditCardRow.DebtSideConsumerRow.IsDateOfBirthNull())
                {
                    // ensure a date string with format yyyy-mm-dd
                    holderRecord.DateOfBirth = debtRow.DebtCreditCardRow.DebtSideConsumerRow.DateOfBirth.ToString("u").Substring(0, 10);
                }

                // ensure a date string with format yyyy-mm-dd
                if (!debtRow.IsDateOfDelinquencyNull())
                {
                    holderRecord.DateOfDelinquency = debtRow.DateOfDelinquency.ToString("u").Substring(0, 10);
                }

                //  - CreditCard

                holderRecord.AccountCode = debtRow.DebtCreditCardRow.AccountCode;
                holderRecord.AccountBalance = debtRow.DebtCreditCardRow.AccountBalance;
                holderRecord.OriginalAccountNumber = debtRow.DebtCreditCardRow.OriginalAccountNumber;

                holderRecord.DebtHolder = debtRow.DebtCreditCardRow.RealDebtHolder;

                // Vendor Code (unique per debt holder organization)
                holderRecord.VendorCode = organizationRow.Name;

                this.dataSetDebtHolders[importFileIndex].DebtHolderRecord.AddDebtHolderRecordRow(holderRecord);

                // circular index through the available import file 'slots'
                importFileIndex = (++importFileIndex >= Properties.Settings.Default.NumberOfImportFilesPerOrganization
                    ? 0 : importFileIndex);
            }

            Stopwatch s = new Stopwatch();
	        s.Restart();

            // write out the import files to disk
            WriteFlatImportFilesToDisk(organizationRow.Name, TestDataConfig.OrganizationType.DebtHolder);

            s.Stop();
            this.durationTracker.MarkCheckpoint(organizationRow.Name + " xml dump", s.Elapsed.TotalMinutes);
        }


	    /// <summary>
        /// Updates the Organization table with information on how to divy up the working orders 
        /// among the existing organizations based on type (Holder or Negotiator) and how many 
        /// organizations of said type exist.
        /// </summary>
        private void UpdateOrganizationTableWorkingOrderCounts()
        {
            Int32 numDebtHolderOrganizations = 
                GetNumberOfOrganizations(TestDataConfig.OrganizationType.DebtHolder);

            Int32 numDebtNegotiatorOrganizations = 
                GetNumberOfOrganizations(TestDataConfig.OrganizationType.DebtNegotiator);

            // make sure we have at least one organization on each side of the chinese wall
            if ((numDebtNegotiatorOrganizations < 1) || (numDebtHolderOrganizations < 1))
            {
                throw new SystemException(String.Format("Need at least one organization on each side of the wall"));
            }

            // break down the Debt Holder orders evenly among organizations
            Int32 debtHolderOrdersPerOrganization =
                this.dataSetDebtDataGen.ConsumerDebt.Count() / numDebtHolderOrganizations;

            Int32 debtHolderOrdersPerOrganizationRemainder =
                this.dataSetDebtDataGen.ConsumerDebt.Count() % numDebtHolderOrganizations;

            Int32 debtNegotiatorsOrdersPerOrganization = 
                this.dataSetDebtDataGen.ConsumerTrust.Count() / numDebtNegotiatorOrganizations;

            Int32 debtNegotiatorsOrdersPerOrganizationRemainder =
                this.dataSetDebtDataGen.ConsumerTrust.Count() % numDebtNegotiatorOrganizations;


	        Int32 negotiatorOrgCount = 0;  // keeps track of the number of Negotiation organizations
            Int32 holderOrgCount = 0;      // keeps track of the number of Holder organizations

            Int32 negotiatorIdMarker = 0;  // marks the Id of the debt negotiation record currently in 'context'
            Int32 holderIdMarker = 0;      // marks the Id of the debt holder record currently in 'context'

            // Go through each org in the table and divy up the orders among the organization types
            foreach (DataSetDebtDataGenerator.OrganizationRow organizationRow in this.dataSetDebtDataGen.Organization)
            {
                // Assign the number of working orders per Debt Negotiator organization 
                if (organizationRow.Type == TestDataConfig.OrganizationType.DebtNegotiator)
                {
                    organizationRow.WorkingOrderCount = debtNegotiatorsOrdersPerOrganization;

                    // if 1) there are > 1 Negotiation organizations and
                    //    2) we reached the last negotiator organization 
                    // then give any 'extra' (remainder) working order(s) to the last negotiator organization                                        
                    if ((numDebtNegotiatorOrganizations > 1) &&
                        (++negotiatorOrgCount == numDebtNegotiatorOrganizations))                        
                    {
                        organizationRow.WorkingOrderCount += debtNegotiatorsOrdersPerOrganizationRemainder;
                    }

                    // use the start and end order Ids to divide orders up between multiple organizations
                    organizationRow.StartingOrderId = negotiatorIdMarker;
                    organizationRow.EndingOrderId = negotiatorIdMarker + organizationRow.WorkingOrderCount - 1;
                    negotiatorIdMarker = organizationRow.EndingOrderId + 1;
                }

                // Assign the number of working orders per Debt Holder organization 
                else if (organizationRow.Type == TestDataConfig.OrganizationType.DebtHolder)
                {
                    organizationRow.WorkingOrderCount = debtHolderOrdersPerOrganization;

                    // if 1) there are > 1 holder organizations and
                    //    2) we reached the last holder organization 
                    // then give any 'extra' (remainder) working order(s) to the last holder organization    
                    if ((numDebtHolderOrganizations > 1) &&
                        (++holderOrgCount == numDebtHolderOrganizations))                    
                    {
                        organizationRow.WorkingOrderCount += debtHolderOrdersPerOrganizationRemainder;
                    }

                    // use the start and end order Ids to divide orders up between multiple organizations
                    organizationRow.StartingOrderId = holderIdMarker;
                    organizationRow.EndingOrderId = holderIdMarker + organizationRow.WorkingOrderCount - 1;
                    holderIdMarker = organizationRow.EndingOrderId + 1;
                }
            }
        }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="random"></param>
	    private void WriteDebtHolderCsvImportFile(RandomNumberGenerator random, string outputFile)
	    {
	        using (StreamWriter sw = new StreamWriter(Properties.Settings.Default.DataOutputLocation + outputFile + ".csv"))
	        {

	            if (Properties.Settings.Default.IncludeHeadersInCsvOutput)
	            {
	                // Include header info
	                sw.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}",
	                                           "CollectionDate",        /* Debt-side specifc field */
	                                           "DataArchiveId",         /* Debt-side specifc field */
	                                           "DateOfDeliquency",      /* Debt-side specifc field */
	                                           "InterestRate",          /* Debt-side specifc field */
	                                           "LastPaidDate",          /* Debt-side specifc field */
	                                           "OpenedDate",            /* Debt-side specifc field */
	                                           "Representative",        /* Debt-side specifc field */
	                                           "SellersAccountNumber",  /* Debt-side specifc field */
	                                           "Address1",
	                                           "Address2",
	                                           "BankAccountNumber",
	                                           "BankAccountRoutingNumber",
	                                           "City",
	                                           "DateOfBirth",
	                                           "FirstName",
	                                           "IsEmployed",
	                                           "LastName",
	                                           "Location",
	                                           "MiddleName",
	                                           "PhoneNumber",
	                                           "ProvinceId",
	                                           "SocialSecurityNumber", /* Required by Data Model */
	                                           "Suffix")
	                    );
	            }

	        }

	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="random"></param>
	    private void WriteDebtNegotiatorCsvImportFile(RandomNumberGenerator random, string outputFile)
	    {
	        using (StreamWriter sw = new StreamWriter(Properties.Settings.Default.DataOutputLocation + outputFile + ".csv"))
	        {
                
	            if (Properties.Settings.Default.IncludeHeadersInCsvOutput)
	            {
	                // Include header info
	                sw.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
	                                           "SavingsBalance",    /* Trust-side specifc field */
	                                           "Address1",
	                                           "Address2",
	                                           "BankAccountNumber",
	                                           "BankAccountRoutingNumber",
	                                           "City",
	                                           "DateOfBirth",
	                                           "FirstName",
	                                           "IsEmployed",
	                                           "LastName",
	                                           "Location",
	                                           "MiddleName",
	                                           "PhoneNumber",
	                                           "ProvinceId",
	                                           "SocialSecurityNumber", /* Required by Data Model */
	                                           "Suffix")
	                    );
	            }
	        }
	    }


        /// <summary>
        /// Generate a Debt Negotiator working order
        /// </summary>
        /// <param name="debtClasses"></param>
        /// <param name="workingOrderDoc"></param>
        /// <param name="organizationRow"></param>
        private void GenerateDebtNegotiatorWorkingOrder(
            List<String> debtClasses,
            Int32 debtClassIndex,
            XDocument workingOrderDoc,
            DataSetDebtDataGenerator.OrganizationRow organizationRow,
            DataSetDebtDataGenerator.ConsumerTrustRow consumerTrustRow)
        {
            XElement transactionElement = new XElement("transaction");

            // Generate a unique ID for this working order
            Guid workingOrderId = Guid.NewGuid();

            // Generate a unique blotter key for this order based on the debt class it is associated with
            String blotterKey = organizationRow.Name + " " + debtClasses[debtClassIndex].ToUpper() + " BLOTTER";

            transactionElement.Add(
                    ConsumerTrust.CreateWorkingOrder(consumerTrustRow, workingOrderId, blotterKey, organizationRow));
            
            workingOrderDoc.Root.Add(transactionElement);
        }


        /// <summary>
        /// Generate a Debt Holder working order
        /// </summary>
        /// <param name="debtClasses"></param>
        /// <param name="debtClassIndex"></param>
        /// <param name="workingOrderDoc"></param>
        /// <param name="organizationRow"></param>
        /// <param name="consumerDebtRow"></param>
        private void GenerateDebtHolderWorkingOrder(
            List<String> debtClasses, 
            Int32 debtClassIndex,
            XDocument workingOrderDoc, 
            DataSetDebtDataGenerator.OrganizationRow organizationRow,
            DataSetDebtDataGenerator.ConsumerDebtRow consumerDebtRow)
        {
            XElement transactionElement = new XElement("transaction");

            // Generate a unique ID for this working order
            Guid workingOrderId = Guid.NewGuid();
            
            // Generate a unique blotter key for this order based on the debt class it is associated with
            String blotterKey = organizationRow.Name + " " + debtClasses[debtClassIndex].ToUpper() + " BLOTTER";
            
            transactionElement.Add(
                    ConsumerDebt.CreateWorkingOrder(consumerDebtRow, workingOrderId, blotterKey, organizationRow));

            workingOrderDoc.Root.Add(transactionElement);
        }


	    /// <summary>
	    /// Validate any config data as needed
	    /// </summary>
	    private void ValidateConfigurationData()
	    {
            
            // make sure we have access to the raw data source directory
            if (!Directory.Exists(Properties.Settings.Default.DataInputLocation))
            {
                throw new SystemException(
                    string.Format("Import directory does not exist: {0}", Properties.Settings.Default.DataInputLocation));
            }

            /// Debt and Trust record counts
	        if (Properties.Settings.Default.NumberOfTrustRecords >= Properties.Settings.Default.NumberOfConsumerRecords ||
	            Properties.Settings.Default.NumberOfDebtRecords >= Properties.Settings.Default.NumberOfConsumerRecords)
	        {
	            throw new SystemException(
	                "NumberOfTrustRecords and NumberOfDebtRecords must be less than or equal to NumberOfConsumerRecords");
	        }

            // Match count must be less than or equal to the number of cosumers in the TrustConsumer table...assuming there
            // is only one CC per consumer at a bare minimum
            if (Properties.Settings.Default.NumberOfMatches > Properties.Settings.Default.NumberOfTrustRecords ||
                Properties.Settings.Default.NumberOfMatches > Properties.Settings.Default.NumberOfDebtRecords) 
            {
                throw new SystemException(
                    "NumberOfMatches must be less than or equal to NumberOfTrustRecords and less than or equal to NumberOfDebtRecords");
            }

            // The total number of consumers must be greater or equal to the amount of Trust Records + amount of Debt Records 
            // minus the numer of matches between the two.  This assures we have enough non-matching consumers to divy up on either side
            if (Properties.Settings.Default.NumberOfConsumerRecords < (Properties.Settings.Default.NumberOfTrustRecords +
                                                                       Properties.Settings.Default.NumberOfDebtRecords  -
                                                                       Properties.Settings.Default.NumberOfMatches))
            {
                throw new SystemException(
                    "NumberOfConsumerRecords must be greater than or equal to the amount of Trust Records" +
                    " + amount of Debt Records minus the numer of matches between the two.");
            }

            // Fuzz counts need to be within a valid range of 0 to total number of matches
            if (!Properties.Settings.Default.TrustSideFuzzCount.IsWithinRange(0,Properties.Settings.Default.NumberOfMatches) ||
                !Properties.Settings.Default.DebtSideFuzzCount.IsWithinRange(0,Properties.Settings.Default.NumberOfMatches)  ||
                !Properties.Settings.Default.CommonFuzzCount.IsWithinRange(0,Properties.Settings.Default.NumberOfMatches))
            {
                throw new SystemException(
                    "Fuzz Counts must be less than or equal to the specified number of matches and greater than or equal to zero");
            }

            // make sure TrustSideNicknamePercentage is valid
            if (!Properties.Settings.Default.TrustSideFuzzNicknamePercentage.IsWithinRange(0,100))
            {
                throw new SystemException(
                    "TrustSideNicknamePercentage must be a valid percentage value (0-100)");
            }

            // make sure DebtSideNicknamePercentage is valid
            if (!Properties.Settings.Default.DebtSideFuzzNicknamePercentage.IsWithinRange(0, 100))
            {
                throw new SystemException(
                    "DebtSideNicknamePercentage must be a valid percentage value (0-100)");
            }

            // make sure at least one import file will be created per organization
            if (Properties.Settings.Default.NumberOfImportFilesPerOrganization < 1)
            {
                throw new SystemException(
                    "Need at least one Import File per organization");
            }

	    }


	    /// <summary>
	    /// Sort of a unit test to make sure the matching count integrity holds
	    /// </summary>
	    private void ValidateMatchCounts()
	    {
	        // Sanity Check / Debugging Unit Test to ensure match numbers are correct
	        Int32 realConsumerMatches = 0;
	        foreach (DataSetDebtDataGenerator.RealConsumerRow realConsumer in this.dataSetDebtDataGen.RealConsumer)
	        {
	            realConsumerMatches += realConsumer.MatchCount;
	        }

	        if (realConsumerMatches != Properties.Settings.Default.NumberOfMatches)
	            throw new SystemException("RealConsumer Match Count <> Properties.Settings.Default.NumberOfMatches");

            // skt: need to count matches on both sides of the chinese wall


	    }


	}

}

