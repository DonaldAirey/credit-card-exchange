﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.Text;

	//* 
	// * A parser log exception. This class contains a list of all the 
	// * parse errors encountered while parsing. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// * @since 1.1 
	// 
	[Serializable()]
	internal class ParserLogException : Exception
	{

		//* 
		// * The list of errors found. 
		// 

		private ArrayList errors = new ArrayList();

		//* 
		// * Creates a new empty parser log exception. 
		// 

		public ParserLogException()
		{
		}

		private ParserLogException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			this.errors = info.GetValue("Errors", typeof(ArrayList)) as ArrayList;
		}

		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Errors", errors, typeof(ArrayList));
		}

		//* 
		// * The message property (read-only). This property contains 
		// * the detailed exception error message. 
		// 

		public override string Message
		{
			get
			{
				StringBuilder buffer = new StringBuilder();
				for (int i = 0; i <= Count - 1; i++)
				{

					if (i > 0)
					{
						buffer.Append("" + Convert.ToChar(10) + "");
					}
					buffer.Append(this[i].Message);
				}
				return buffer.ToString();
			}
		}

		//* 
		// * The error count property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public int Count
		{
			get { return errors.Count; }
		}

		//* 
		// * Returns the number of errors in this log. 
		// * 
		// * @return the number of errors in this log 
		// * 
		// * @see #Count 
		// * 
		// * @deprecated Use the Count property instead. 
		// 

		public int GetErrorCount()
		{
			return Count;
		}

		//* 
		// * The error index (read-only). This index contains all the 
		// * errors in this error log. 
		// * 
		// * @param index the error index, 0 <= index < Count 
		// * 
		// * @return the parse error requested 
		// * 
		// * @since 1.5 
		// 

		public ParseException this[int index]
		{
			get { return (ParseException)errors[index]; }
		}

		//* 
		// * Returns a specific error from the log. 
		// * 
		// * @param index the error index, 0 <= index < count 
		// * 
		// * @return the parse error requested 
		// * 
		// * @deprecated Use the class indexer instead. 
		// 

		public ParseException GetError(int index)
		{
			return this[index];
		}

		//* 
		// * Adds a parse error to the log. 
		// * 
		// * @param e the parse error to add 
		// 

		public void AddError(ParseException e)
		{
			errors.Add(e);
		}

		//* 
		// * Returns the detailed error message. This message will contain 
		// * the error messages from all errors in this log, separated by 
		// * a newline. 
		// * 
		// * @return the detailed error message 
		// * 
		// * @see #Message 
		// * 
		// * @deprecated Use the Message property instead. 
		// 

		public string GetMessage()
		{
			return Message;
		}
	}
}