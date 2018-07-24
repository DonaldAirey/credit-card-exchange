using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataModelSerializer
{
	public class DataSetCompressedStream : Stream
	{
		private DataSet dataSet;
		private List<DataTable> tableList;
		private int tableIndex = -1;
		private long byteCount = 0;
		private MemoryStream innerMemoryStream;
		private DataTableBinarySerializer dataTableSerializer;
		private MemoryStream tableMemoryStream;
		private long innerMemoryStreamLength;

		private const int TableBytesToReadCount = 200000;
		private const int VERSION = 1;

		internal DataSetCompressedStream(DataSet ds)
		{
			this.dataSet = ds;
			FileStream fs = new FileStream(@"c:\DataFile.dat", FileMode.Create);
			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
			    DataSet tmpDs = new DataSet();
				foreach(DataTable dt in ds.Tables)
				{
					DataTable newTable = tmpDs.Tables.Add(dt.TableName);
					foreach(DataColumn dc in dt.Columns)
						newTable.Columns.Add(dc.ColumnName, dc.DataType);

					foreach(DataRow r in dt.Rows)
						newTable.ImportRow(r);

				}
			    tmpDs.RemotingFormat = SerializationFormat.Binary;
			    formatter.Serialize(fs, tmpDs);
			}
			catch(SerializationException ex)
			{
			    Console.WriteLine("Failed to serialize. Reason: " + ex.Message);
			    throw;
			}
			finally
			{
			    fs.Close();
			}

			
			this.tableList = new List<DataTable>();
			foreach(DataTable table in ds.Tables)
			{
				tableList.Add(table);
			}

			this.innerMemoryStream = new MemoryStream();
			DBConverter.ToMemoryStreamNoHeader(this.innerMemoryStream, VERSION);
			DBConverter.ToMemoryStream(this.innerMemoryStream, this.dataSet.DataSetName);
			DBConverter.ToMemoryStreamNoHeader(this.innerMemoryStream, this.tableList.Count);
			this.innerMemoryStreamLength = this.innerMemoryStream.Position;
			this.innerMemoryStream.Position = 0;

			this.tableMemoryStream = new MemoryStream();
		}

		public override bool CanRead
		{
			get { return this.tableIndex < tableList.Count; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override void Flush()
		{
			throw new Exception("This stream does not support writing.");
		}

		public override long Length
		{
			get { throw new Exception("This stream does not support the Length property."); }
		}

		public override long Position
		{
			get
			{
				return this.byteCount;
			}
			set
			{
				throw new Exception("This stream does not support setting the Position property.");
			}
		}


		public override int Read(byte[] buffer, int offset, int count)
		{
			//if at the end of the inner stream need to fill it up with more data
			if(this.innerMemoryStream.Position >= this.innerMemoryStreamLength)
			{
				if(this.tableIndex >= this.tableList.Count-1)
					return 0;

				long tableWriteCount = 0;
				do
				{
					if(this.dataTableSerializer == null ||
						this.dataTableSerializer.IsComplete)
					{
						this.tableIndex++;
						if(this.tableIndex >= this.tableList.Count)
							break;

						this.tableMemoryStream.Position = 0;
						this.dataTableSerializer = new DataTableBinarySerializer(this.tableList[tableIndex], this.tableMemoryStream);
					}

					tableWriteCount += this.dataTableSerializer.WriteTableToStream(TableBytesToReadCount);

				} while(tableWriteCount < TableBytesToReadCount);

				this.innerMemoryStream.Position = 0;
				DeflateStream compressedzipStream = new DeflateStream(this.innerMemoryStream, CompressionMode.Compress, true);
				compressedzipStream.Write(this.tableMemoryStream.ToArray(), 0, (int)this.tableMemoryStream.Position);
				// Close the stream.
				compressedzipStream.Close();

				this.innerMemoryStreamLength = this.innerMemoryStream.Position;
				this.innerMemoryStream.Position = 0;
			}

			if(count > this.innerMemoryStreamLength - this.innerMemoryStream.Position)
				count = (int)(this.innerMemoryStreamLength - this.innerMemoryStream.Position);

			
			return this.innerMemoryStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new Exception("This stream does not support seeking.");
		}

		public override void SetLength(long value)
		{
			throw new Exception("This stream does not support setting the Length.");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new Exception("This stream does not support writing.");
		}

		public override void Close()
		{
			try
			{
				this.innerMemoryStream.Close();
			}
			finally
			{
				base.Close();
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				this.innerMemoryStream.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}

	public class DBConverter
	{
		private static Type GuidType;
		private static Type ByteArType;
		
		static DBConverter()
		{
			GuidType = typeof(Guid);
			ByteArType = typeof(byte[]);
		}

		public static byte[] Serialize(DataTable table)
		{
			TypeCodeEx[] colDbTypeAr = new TypeCodeEx[table.Columns.Count];

			for(int i = 0; i < table.Columns.Count; i++)
			{
				DataColumn dc = table.Columns[i];
				if(dc.DataType == GuidType)
					colDbTypeAr[i] = TypeCodeEx.Guid;
				else if(dc.DataType == ByteArType)
					colDbTypeAr[i] = TypeCodeEx.ByteAr;
				else
					colDbTypeAr[i] = (TypeCodeEx)Type.GetTypeCode(dc.DataType);
			}

			using(MemoryStream memStream = new MemoryStream())
			{
				foreach(DataRow row in table.Rows)
				{
					if(row.RowState == DataRowState.Deleted)
						continue;

					BytesFromRow(row, table.Columns, colDbTypeAr, memStream);
				}

				memStream.Flush();
				return memStream.ToArray();
			}
		}

		public static void Deserialize(Byte[] bytes, DataTable table)
		{
			TypeCodeEx[] colDbTypeAr = GetDataColumnTypeCodes(table);

			DataRow newRow = table.NewRow();
			using(MemoryStream memStream = new MemoryStream(bytes))
			{
				RowFromBytes(newRow, table.Columns, colDbTypeAr, memStream);
			}
		}

		public static TypeCodeEx[] GetDataColumnTypeCodes(DataTable table)
		{
			TypeCodeEx[] colDbTypeAr = new TypeCodeEx[table.Columns.Count];

			for(int i = 0; i < table.Columns.Count; i++)
			{
				DataColumn dc = table.Columns[i];
				if(dc.DataType == GuidType)
					colDbTypeAr[i] = TypeCodeEx.Guid;
				else if(dc.DataType == ByteArType)
					colDbTypeAr[i] = TypeCodeEx.ByteAr;
				else
					colDbTypeAr[i] = (TypeCodeEx)Type.GetTypeCode(dc.DataType);
			}
			return colDbTypeAr;
		}

		public static void BytesFromRow(DataRow row, DataColumnCollection columns, TypeCodeEx[] colDbTypeAr, MemoryStream memStream)
		{
			for(int i = 0; i < columns.Count; i++)
			{
				DataColumn dc = columns[i];
				object cellVal = row[dc];

				TypeCodeEx typeCodeEx = colDbTypeAr[i];

				if(cellVal == dc.DefaultValue)
					typeCodeEx = TypeCodeEx.Empty;
				else if(cellVal == null ||
					DBNull.Value.Equals(cellVal))
					typeCodeEx = TypeCodeEx.DBNull;

				Byte[] cellBytes = null;

				switch(typeCodeEx)
				{
					case TypeCodeEx.Empty:
					case TypeCodeEx.Object:
						memStream.WriteByte((byte)DataHeader.DefaultValue);
						break;
					case TypeCodeEx.DBNull:
						memStream.WriteByte((byte)DataHeader.DbNull);
						break;
					case TypeCodeEx.Boolean:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes((bool)cellVal);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.Char:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes((char)cellVal);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.SByte:
					case TypeCodeEx.Byte:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						memStream.WriteByte((byte)cellVal);
						break;
					case TypeCodeEx.Int16:
					case TypeCodeEx.UInt16:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes((Int16)cellVal);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.Int32:
					case TypeCodeEx.UInt32:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes((Int32)cellVal);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.Int64:
					case TypeCodeEx.UInt64:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes((Int64)cellVal);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.Single:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes((float)cellVal);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.Double:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes((double)cellVal);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.Decimal:
						{
							memStream.WriteByte((byte)DataHeader.FixedLenValue);
							int[] intAr = Decimal.GetBits((Decimal)cellVal);
							foreach(int decPart in intAr)
							{
								cellBytes = BitConverter.GetBytes(decPart);
								memStream.Write(cellBytes, 0, cellBytes.Length);
							}
						}
						break;

					case TypeCodeEx.DateTime:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = BitConverter.GetBytes(((DateTime)cellVal).Ticks);
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.String:
						{
							DBConverter.ToMemoryStream(memStream, (string)cellVal);
						}
						break;
					case TypeCodeEx.Guid:
						memStream.WriteByte((byte)DataHeader.FixedLenValue);
						cellBytes = ((Guid)cellVal).ToByteArray();
						memStream.Write(cellBytes, 0, cellBytes.Length);
						break;
					case TypeCodeEx.ByteAr:
						{
							memStream.WriteByte((byte)DataHeader.VariableLenValue);
							cellBytes = (byte[])cellVal;
							Byte[] lengthBytes = BitConverter.GetBytes(cellBytes.Length);
							memStream.Write(lengthBytes, 0, lengthBytes.Length);
							memStream.Write(cellBytes, 0, cellBytes.Length);
						}
						break;
				}
			}//foreach column
		}

		public static void ToMemoryStream(MemoryStream memStream, string cellVal)
		{
			memStream.WriteByte((byte)DataHeader.VariableLenValue);
			Byte[] cellBytes = Encoding.Unicode.GetBytes(cellVal);
			Byte[] lengthBytes = BitConverter.GetBytes(cellBytes.Length);
			memStream.Write(lengthBytes, 0, lengthBytes.Length);
			memStream.Write(cellBytes, 0, cellBytes.Length);
		}

		public static void ToMemoryStream(MemoryStream memStream, int cellVal)
		{
			memStream.WriteByte((byte)DataHeader.FixedLenValue);
			Byte[] cellBytes = BitConverter.GetBytes(cellVal);
			memStream.Write(cellBytes, 0, cellBytes.Length);
		}

		public static void ToMemoryStreamNoHeader(MemoryStream memStream, int cellVal)
		{
			Byte[] cellBytes = BitConverter.GetBytes(cellVal);
			memStream.Write(cellBytes, 0, cellBytes.Length);
		}

		public static void RowFromBytes(DataRow newRow, DataColumnCollection columns, TypeCodeEx[] colDbTypeAr, MemoryStream memStream)
		{
			byte[] fixedLenBuffer = new byte[16];
			byte[] stringBuffer = null;

			for(int i = 0; i < columns.Count; i++)
			{
				DataColumn dc = columns[i];
				TypeCodeEx typeCodeEx = colDbTypeAr[i];

				DataHeader header = (DataHeader)memStream.ReadByte();
				switch(header)
				{
					case DataHeader.DbNull:
						if(dc.DefaultValue == null)
							continue;
						newRow[dc] = DBNull.Value;
						continue;

					case DataHeader.DefaultValue:
						continue;
				}

				switch(typeCodeEx)
				{
					case TypeCodeEx.Boolean:
						memStream.Read(fixedLenBuffer, 0, 1);
						newRow[dc] = BitConverter.ToBoolean(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.Char:
						memStream.Read(fixedLenBuffer, 0, 2);
						newRow[dc] = BitConverter.ToChar(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.SByte:
						memStream.Read(fixedLenBuffer, 0, 1);
						newRow[dc] = (sbyte)fixedLenBuffer[0];
						break;
					case TypeCodeEx.Byte:
						memStream.Read(fixedLenBuffer, 0, 1);
						newRow[dc] = (Byte)fixedLenBuffer[0];
						break;
					case TypeCodeEx.Int16:
						memStream.Read(fixedLenBuffer, 0, 2);
						newRow[dc] = BitConverter.ToInt16(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.UInt16:
						memStream.Read(fixedLenBuffer, 0, 2);
						newRow[dc] = BitConverter.ToUInt16(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.Int32:
						memStream.Read(fixedLenBuffer, 0, 4);
						newRow[dc] = BitConverter.ToInt32(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.UInt32:
						memStream.Read(fixedLenBuffer, 0, 4);
						newRow[dc] = BitConverter.ToUInt32(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.Int64:
						memStream.Read(fixedLenBuffer, 0, 8);
						newRow[dc] = BitConverter.ToInt64(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.UInt64:
						memStream.Read(fixedLenBuffer, 0, 8);
						newRow[dc] = BitConverter.ToUInt64(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.Single:
						memStream.Read(fixedLenBuffer, 0, 4);
						newRow[dc] = BitConverter.ToSingle(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.Double:
						memStream.Read(fixedLenBuffer, 0, 8);
						newRow[dc] = BitConverter.ToDouble(fixedLenBuffer, 0);
						break;
					case TypeCodeEx.Decimal:
						{
							int[] intAr = new int[4];
							memStream.Read(fixedLenBuffer, 0, 8);
							intAr[0] = BitConverter.ToInt32(fixedLenBuffer, 0);
							intAr[1] = BitConverter.ToInt32(fixedLenBuffer, 4);
							memStream.Read(fixedLenBuffer, 0, 8);
							intAr[2] = BitConverter.ToInt32(fixedLenBuffer, 0);
							intAr[3] = BitConverter.ToInt32(fixedLenBuffer, 4);

							newRow[dc] = new decimal(intAr);
						}
						break;

					case TypeCodeEx.DateTime:
						memStream.Read(fixedLenBuffer, 0, 8);
						newRow[dc] = new DateTime(BitConverter.ToInt64(fixedLenBuffer, 0));
						break;
					case TypeCodeEx.String:
						{
							memStream.Read(fixedLenBuffer, 0, 4);
							int length = BitConverter.ToInt32(fixedLenBuffer, 0);
							if(stringBuffer == null ||
								stringBuffer.Length < length)
								stringBuffer = new byte[length];

							int offset = 0;
							do
							{
								offset += memStream.Read(stringBuffer, offset, length);
							} while(offset < length);
							newRow[dc] = Encoding.Unicode.GetString(stringBuffer, 0, length);
						}
						break;
					case TypeCodeEx.Guid:
						memStream.Read(fixedLenBuffer, 0, 16);
						newRow[dc] = new Guid(fixedLenBuffer);
						break;
					case TypeCodeEx.ByteAr:
						{
							memStream.Read(fixedLenBuffer, 0, 4);
							int length = BitConverter.ToInt32(fixedLenBuffer, 0);
							if(stringBuffer == null ||
								stringBuffer.Length < length)
								stringBuffer = new byte[length];

							int offset = 0;
							do
							{
								offset += memStream.Read(stringBuffer, offset, length);
							} while(offset < length);

							Byte[] byteArVal = new byte[length];
							Buffer.BlockCopy(stringBuffer, 0, byteArVal, 0, length);
							newRow[dc] = byteArVal;
						}
						break;
				}
			}//foreach column
		}
	}
	public enum DataHeader : byte
	{
		DbNull = 0x0,					//000
		DefaultValue = 5,					//101
		FixedLenValue = 6,				//110
		VariableLenValue = 7,			//111
	}

	public enum TypeCodeEx
	{
		// Summary:
		//     A null reference.
		Empty = 0,
		//
		// Summary:
		//     A general type representing any reference or value type not explicitly represented
		//     by another TypeCode.
		Object = 1,
		//
		// Summary:
		//     A database null (column) value.
		DBNull = 2,
		//
		// Summary:
		//     A simple type representing Boolean values of true or false.
		Boolean = 3,
		//
		// Summary:
		//     An integral type representing unsigned 16-bit integers with values between
		//     0 and 65535. The set of possible values for the System.TypeCode.Char type
		//     corresponds to the Unicode character set.
		Char = 4,
		//
		// Summary:
		//     An integral type representing signed 8-bit integers with values between -128
		//     and 127.
		SByte = 5,
		//
		// Summary:
		//     An integral type representing unsigned 8-bit integers with values between
		//     0 and 255.
		Byte = 6,
		//
		// Summary:
		//     An integral type representing signed 16-bit integers with values between
		//     -32768 and 32767.
		Int16 = 7,
		//
		// Summary:
		//     An integral type representing unsigned 16-bit integers with values between
		//     0 and 65535.
		UInt16 = 8,
		//
		// Summary:
		//     An integral type representing signed 32-bit integers with values between
		//     -2147483648 and 2147483647.
		Int32 = 9,
		//
		// Summary:
		//     An integral type representing unsigned 32-bit integers with values between
		//     0 and 4294967295.
		UInt32 = 10,
		//
		// Summary:
		//     An integral type representing signed 64-bit integers with values between
		//     -9223372036854775808 and 9223372036854775807.
		Int64 = 11,
		//
		// Summary:
		//     An integral type representing unsigned 64-bit integers with values between
		//     0 and 18446744073709551615.
		UInt64 = 12,
		//
		// Summary:
		//     A floating point type representing values ranging from approximately 1.5
		//     x 10 -45 to 3.4 x 10 38 with a precision of 7 digits.
		Single = 13,
		//
		// Summary:
		//     A floating point type representing values ranging from approximately 5.0
		//     x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.
		Double = 14,
		//
		// Summary:
		//     A simple type representing values ranging from 1.0 x 10 -28 to approximately
		//     7.9 x 10 28 with 28-29 significant digits.
		Decimal = 15,
		//
		// Summary:
		//     A type representing a date and time value.
		DateTime = 16,
		//
		// Summary:
		//     A sealed class type representing Unicode character strings.
		String = 18,

		Guid = 20,

		ByteAr = 21,
	}
}
