namespace FluidTrade.Guardian
{

	using System;
    using System.Data;
	using System.IO;

    /// <summary>
	/// Summary description for Hierarchy.
	/// </summary>
	public class TransactionLog
	{

		public static void Dump(Object[] transactionLog)
		{

			StreamWriter streamWriter = new StreamWriter(String.Format("L{0}.log", DateTime.Now.ToFileTime()), false);

			foreach (Object[] transactionLogItem in transactionLog)
			{
				String action = ((Int32)transactionLogItem[0]) == 0 ? "Add" : ((Int32)transactionLogItem[0]) == 1 ? "Deleted" : "Modified";
				DataTable dataTable = DataModel.Tables[(Int32)transactionLogItem[1]];
				String key = String.Empty;
				for (int keyIndex = 0; keyIndex < dataTable.PrimaryKey.Length; keyIndex++)
					key += String.Format("{0} ", transactionLogItem[2 + keyIndex]);
				streamWriter.WriteLine("{0} action on table {1}, Record {2}", action, dataTable.TableName, key);
			}

			streamWriter.Close();

		}

	}

}
