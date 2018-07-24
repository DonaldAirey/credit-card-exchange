using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace DataModelSerializer
{
	public class DataTableBinarySerializer 
	{
		private const int MemoryStreamStartingSize = 500000;
		private DataTable table;
		private List<DataRow> rowList;
		private int rowIndex = -1; 
		//private long byteCount = 0;
		private MemoryStream outputStream;
		private long innerMemoryStreamLength;

		private const int VERSION = 1;
		private TypeCodeEx[] colDbTypeAr;
		public DataTableBinarySerializer(DataTable table, MemoryStream outputStream)
		{
			this.table = table;
			this.colDbTypeAr = DBConverter.GetDataColumnTypeCodes(table);

			this.rowList = new List<DataRow>();
			foreach(DataRow r in table.Rows)
			{
				if(r.RowState == DataRowState.Deleted)
					continue;

				rowList.Add(r);
			}
			this.outputStream = outputStream;
			DBConverter.ToMemoryStreamNoHeader(this.outputStream, VERSION);
			DBConverter.ToMemoryStream(this.outputStream, table.TableName);
			DBConverter.ToMemoryStreamNoHeader(this.outputStream, this.rowList.Count);
			this.innerMemoryStreamLength = this.outputStream.Position;
		}

		//// The unsafe keyword allows pointers to be used within the following method:
		//static unsafe void ClearBytes(byte[] src, int srcIndex, long count)
		//{
			
		//    int srcLen = src.Length;
			
		//    // The following fixed statement pins the location of the src and dst objects
		//    // in memory so that they will not be moved by garbage collection.
		//    fixed(byte* pSrc = src)
		//    {
		//        byte* ps = pSrc;

		//        long countMod4 = count % 4;
		//        long countDiv4 = count / 4;
		//        // Loop over the count in blocks of 4 bytes, copying an integer (4 bytes) at a time:
		//        for(long i = 0; i < countDiv4; i++)
		//        {
		//            *((int*)ps) = 0;
		//            ps += 4;
		//        }

		//        // Complete the copy by moving any bytes that weren't moved in blocks of 4:
		//        for(long i = 0; i < countMod4; i++)
		//        {
		//            *ps = (byte)0;
		//            ps++;
		//        }
		//    }
		//}

		
		public long WriteTableToStream(long countHint)
		{
			long startPos = this.outputStream.Position;
			while(this.outputStream.Position - startPos < countHint)
			{
				this.rowIndex++;
				if(this.rowIndex >= this.rowList.Count)
				{
					this.isComplete = true;
					return 0;
				}
				DBConverter.BytesFromRow(rowList[rowIndex], this.table.Columns, this.colDbTypeAr, this.outputStream);
			}

			return this.outputStream.Position - startPos;
		}

		private bool isComplete;
		public bool IsComplete
		{
			get { return this.isComplete; }
		}
	}
}
