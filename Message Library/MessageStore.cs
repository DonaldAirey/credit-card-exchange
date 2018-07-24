namespace FluidTrade.Core
{
	using System;
	using System.Diagnostics;
	using System.IO;


    /// <summary>
	/// Persistent data store for Messages.
	/// </summary>
	public class MessageStore
	{

		private bool isOpen;
		private int count;
		private string name;
		private FileStream fileStreamData;
		private FileStream fileStreamIndex;
		private MessageReader messageReaderIndex;
		private MessageReader messageReaderData;
		private MessageWriter messageWriterIndex;
		private MessageWriter messageWriterData;

		/// <summary>
		/// Gets a value indicating whether the MessageStore is open.
		/// </summary>
		public bool IsOpen {get {return this.isOpen;}}

		public int Count {get {return this.count;}}

		/// <summary>
		/// Initializes a MessageStore.
		/// </summary>
		public MessageStore()
		{

			// Initialize the object.
			this.isOpen = false;

		}

		public void Open(string name)
		{

			this.name = name;

			this.count = 0;

			// put the files in the application directory
			string filePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			string dataFileName = string.Format("{0}.db", this.name);
			string indexFileName = string.Format("{0}.idx", this.name);

			if(!Directory.Exists(filePath))
				throw new Exception("Unable to store messages in the application folder.");

			try
			{
			
				this.fileStreamData = new FileStream(Path.Combine(filePath, dataFileName), FileMode.OpenOrCreate,
					FileAccess.ReadWrite, FileShare.ReadWrite);

				this.fileStreamIndex = new FileStream(Path.Combine(filePath, indexFileName), FileMode.OpenOrCreate,
					FileAccess.ReadWrite, FileShare.ReadWrite);

				this.messageReaderIndex = new MessageReader(fileStreamIndex);
				this.messageReaderData = new MessageReader(fileStreamData);
				this.messageWriterIndex = new MessageWriter(fileStreamIndex);
				this.messageWriterData = new MessageWriter(fileStreamData);

				// The number of elements in this message store is determined by the size of the index array which is stored in the
				// index file.
				this.count = (int)(this.fileStreamIndex.Length / System.Runtime.InteropServices.Marshal.SizeOf(typeof(long)));

				// This connection is now open for business.
				this.isOpen = true;

			}
			catch (Exception exception)
			{

				Debug.WriteLine(exception.Message);

			}

		}

		public void Close()
		{

			this.fileStreamData.Close();

			this.fileStreamIndex.Close();

			this.isOpen = false;

		}

		public void Flush()
		{

			this.fileStreamData.Flush();

			this.fileStreamIndex.Flush();

		}

		public void Truncate()
		{

			this.fileStreamData.SetLength(0L);
			this.fileStreamIndex.SetLength(0L);

		}

		public Message this[int recordIndex]
		{

			get
			{

				if (recordIndex > this.count)
					return null;

				Seek(recordIndex);

				// If the file is already positioned at the end of the message store, then the record doesn't exist for reading.
				if (this.fileStreamIndex.Position == this.fileStreamIndex.Length)
					return null;
				
				int persistentRecordIndex = this.messageReaderData.ReadInt32();

				return persistentRecordIndex == recordIndex ? this.messageReaderData.ReadMessage() : null;

			}

			set
			{

				try
				{

					Seek(recordIndex);
					
					// All new records are written to the end of the data store.
					this.fileStreamData.Position = this.fileStreamData.Length;

					// Write the indexLocation of this record to the index file.
					this.messageWriterIndex.Write(this.fileStreamData.Position);

					// Write the sequence number (sanity check), the length and the message to the data file.
					this.messageWriterData.Write(recordIndex);
					this.messageWriterData.Write(value);

					// The sequence number is incremented for every record written to the data store.  The sequence number will always
					// reflect the current position in the file.
					this.count = this.count > recordIndex + 1 ? this.count : recordIndex + 1;

					// Flush the contents out to disk.
					Flush();

				}
				catch (Exception exception)
				{

					Debug.WriteLine(exception.Message);

				}

			}

		}

		private void Seek(int recordIndex)
		{

			long indexPosition = System.Runtime.InteropServices.Marshal.SizeOf(typeof(long)) * recordIndex;
			if (indexPosition == this.fileStreamIndex.Position)
			{

				if (this.fileStreamIndex.Position >= this.fileStreamIndex.Length)
				{
					if (this.fileStreamData.Position != this.fileStreamData.Length)
						this.fileStreamData.Position = this.fileStreamData.Length;
				}
				else
				{
					long calculatedDataPosition = this.messageReaderIndex.ReadInt64();
					if (calculatedDataPosition != this.fileStreamData.Position)
						this.fileStreamData.Position = calculatedDataPosition;
				}

			}
			else
			{

				this.fileStreamIndex.Position = indexPosition;

				if (indexPosition >= this.fileStreamIndex.Length)
					this.fileStreamData.Position = this.fileStreamData.Length;
				else
					this.fileStreamData.Position = this.messageReaderIndex.ReadInt64();

			}

		}

	}

}
