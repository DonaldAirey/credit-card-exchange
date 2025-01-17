﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;

	//* 
	// * A parse exception. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 
	[Serializable()]
	internal class ParseException : Exception
	{

		//* 
		// * The error type enumeration. 
		// 

		public enum ErrorType
		{

			//* 
			// * The internal error type is only used to signal an error 
			// * that is a result of a bug in the parser or tokenizer 
			// * code. 
			// 

			INTERNAL,

			//* 
			// * The I/O error type is used for stream I/O errors. 
			// 

			IO,

			//* 
			// * The unexpected end of file error type is used when end 
			// * of file is encountered instead of a valid token. 
			// 

			UNEXPECTED_EOF,

			//* 
			// * The unexpected character error type is used when a 
			// * character is read that isn't handled by one of the 
			// * token patterns. 
			// 

			UNEXPECTED_CHAR,

			//* 
			// * The unexpected token error type is used when another 
			// * token than the expected one is encountered. 
			// 

			UNEXPECTED_TOKEN,

			//* 
			// * The invalid token error type is used when a token 
			// * pattern with an error message is matched. The 
			// * additional information provided should contain the 
			// * error message. 
			// 

			INVALID_TOKEN,

			//* 
			// * The analysis error type is used when an error is 
			// * encountered in the analysis. The additional information 
			// * provided should contain the error message. 
			// 

			ANALYSIS
		}

		//* 
		// * The error type. 
		// 

		private ErrorType m_type;

		//* 
		// * The additional information string. 
		// 

		private string m_info;

		//* 
		// * The additional details information. This variable is only 
		// * used for unexpected token errors. 
		// 

		private ArrayList m_details;

		//* 
		// * The line number. 
		// 

		private int m_line;

		//* 
		// * The column number. 
		// 

		private int m_column;

		//* 
		// * Creates a new parse exception. 
		// * 
		// * @param type the parse error type 
		// * @param info the additional information 
		// * @param line the line number, or -1 for unknown 
		// * @param column the column number, or -1 for unknown 
		// 

		public ParseException(ErrorType type, string info, int line, int column)
			: this(type, info, null, line, column)
		{
		}

		//* 
		// * Creates a new parse exception. This constructor is only 
		// * used to supply the detailed information array, which is 
		// * only used for expected token errors. The list then contains 
		// * descriptions of the expected tokens. 
		// * 
		// * @param type the parse error type 
		// * @param info the additional information 
		// * @param details the additional detailed information 
		// * @param line the line number, or -1 for unknown 
		// * @param column the column number, or -1 for unknown 
		// 

		public ParseException(ErrorType type, string info, ArrayList details, int line, int column)
		{
			this.m_type = type;
			this.m_info = info;
			this.m_details = details;
			this.m_line = line;
			this.m_column = column;
		}

		private ParseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			this.m_type = (ErrorType)info.GetInt32("Type");
			this.m_info = info.GetString("Info");
			this.m_details = info.GetValue("Details", typeof(ArrayList)) as ArrayList;
			this.m_line = info.GetInt32("Line");
			this.m_column = info.GetInt32("Column");
		}

		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Type", this.m_type);
			info.AddValue("Info", this.m_info);
			info.AddValue("Details", this.m_details);
			info.AddValue("Line", this.m_line);
			info.AddValue("Column", this.m_column);
		}

		//* 
		// * The error type property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public ErrorType Type
		{
			get { return m_type; }
		}

		//* 
		// * Returns the error type. 
		// * 
		// * @return the error type 
		// * 
		// * @see #Type 
		// * 
		// * @deprecated Use the Type property instead. 
		// 

		public ErrorType GetErrorType()
		{
			return Type;
		}

		//* 
		// * The additional error information property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public string Info
		{
			get { return m_info; }
		}

		//* 
		// * Returns the additional error information. 
		// * 
		// * @return the additional error information 
		// * 
		// * @see #Info 
		// * 
		// * @deprecated Use the Info property instead. 
		// 

		public string GetInfo()
		{
			return Info;
		}

		//* 
		// * The additional detailed error information property 
		// * (read-only). 
		// * 
		// * @since 1.5 
		// 

		public ArrayList Details
		{
			get { return new ArrayList(m_details); }
		}

		//* 
		// * Returns the additional detailed error information. 
		// * 
		// * @return the additional detailed error information 
		// * 
		// * @see #Details 
		// * 
		// * @deprecated Use the Details property instead. 
		// 

		public ArrayList GetDetails()
		{
			return Details;
		}

		//* 
		// * The line number property (read-only). This is the line 
		// * number where the error occured, or -1 if unknown. 
		// * 
		// * @since 1.5 
		// 

		public int Line
		{
			get { return m_line; }
		}

		//* 
		// * Returns the line number where the error occured. 
		// * 
		// * @return the line number of the error, or 
		// * -1 if unknown 
		// * 
		// * @see #Line 
		// * 
		// * @deprecated Use the Line property instead. 
		// 

		public int GetLine()
		{
			return Line;
		}

		//* 
		// * The column number property (read-only). This is the column 
		// * number where the error occured, or -1 if unknown. 
		// * 
		// * @since 1.5 
		// 

		public int Column
		{
			get { return m_column; }
		}

		//* 
		// * Returns the column number where the error occured. 
		// * 
		// * @return the column number of the error, or 
		// * -1 if unknown 
		// * 
		// * @see #Column 
		// * 
		// * @deprecated Use the Column property instead. 
		// 

		public int GetColumn()
		{
			return m_column;
		}

		//* 
		// * The message property (read-only). This property contains 
		// * the detailed exception error message, including line and 
		// * column numbers when available. 
		// * 
		// * @see #ErrorMessage 
		// 

		public override string Message
		{
			get
			{
				StringBuilder buffer = new StringBuilder();

				// Add error description 
				buffer.AppendLine(ErrorMessage);

				// Add line and column 
				if (m_line > 0 && m_column > 0)
				{
					string msg = FleeResourceManager.Instance.GetCompileErrorString(CompileErrorResourceKeys.LineColumn);
					msg = string.Format(msg, m_line, m_column);
					buffer.AppendLine(msg);
				}
				//buffer.Append(", on line: ")
				//buffer.Append(m_line)
				//buffer.Append(" column: ")
				//buffer.Append(m_column)

				return buffer.ToString();
			}
		}

		//* 
		// * Returns a default error message. 
		// * 
		// * @return a default error message 
		// * 
		// * @see #Message 
		// * 
		// * @deprecated Use the Message property instead. 
		// 

		public string GetMessage()
		{
			return Message;
		}

		//* 
		// * The error message property (read-only). This property 
		// * contains all the information available, except for the line 
		// * and column number information. 
		// * 
		// * @see #Message 
		// * 
		// * @since 1.5 
		// 

		public string ErrorMessage
		{
			get
			{
				List<string> args = new List<string>();

				switch (m_type)
				{
				case ErrorType.IO:
				case ErrorType.UNEXPECTED_CHAR:
				case ErrorType.INVALID_TOKEN:
				case ErrorType.ANALYSIS:
				case ErrorType.INTERNAL:
					args.Add(m_info);
					break;
				case ErrorType.UNEXPECTED_TOKEN:
					args.Add(m_info);
					args.Add(this.GetMessageDetails());
					break;
				case ErrorType.UNEXPECTED_EOF:
					break;
				}

				string msg = FleeResourceManager.Instance.GetCompileErrorString(m_type.ToString());
				msg = string.Format(msg, args.ToArray());

				return msg;
			}
			// Add type and info 
			//Select Case m_type
			// Case ErrorType.IO
			// buffer.Append("I/O error: ")
			// buffer.Append(m_info)
			// Exit Select
			// Case ErrorType.UNEXPECTED_EOF
			// buffer.Append("unexpected end of file")
			// Exit Select
			// Case ErrorType.UNEXPECTED_CHAR
			// buffer.Append("unexpected character '")
			// buffer.Append(m_info)
			// buffer.Append("'")
			// Exit Select
			// Case ErrorType.UNEXPECTED_TOKEN
			// buffer.Append("unexpected token ")
			// buffer.Append(m_info)
			// If m_details IsNot Nothing Then
			// buffer.Append(", expected ")
			// If m_details.Count > 1 Then
			// buffer.Append("one of ")
			// End If
			// buffer.Append(GetMessageDetails())
			// End If
			// Exit Select
			// Case ErrorType.INVALID_TOKEN
			// buffer.Append(m_info)
			// Exit Select
			// Case ErrorType.ANALYSIS
			// buffer.Append(m_info)
			// Exit Select
			// Case Else
			// buffer.Append("internal error")
			// If m_info IsNot Nothing Then
			// buffer.Append(": ")
			// buffer.Append(m_info)
			// End If
			// Exit Select
			//End Select

			//Return buffer.ToString()
		}

		//* 
		// * Returns the error message. This message will contain all the 
		// * information available, except for the line and column number 
		// * information. 
		// * 
		// * @return the error message 
		// * 
		// * @see #ErrorMessage 
		// * 
		// * @deprecated Use the ErrorMessage property instead. 
		// 

		public string GetErrorMessage()
		{
			return ErrorMessage;
		}

		//* 
		// * Returns a string containing all the detailed information in 
		// * a list. The elements are separated with a comma. 
		// * 
		// * @return the detailed information string 
		// 

		private string GetMessageDetails()
		{
			StringBuilder buffer = new StringBuilder();
			for (int i = 0; i <= m_details.Count - 1; i++)
			{

				if (i > 0)
				{
					buffer.Append(", ");
					if (i + 1 == m_details.Count)
					{
						buffer.Append("or ");
					}
				}
				buffer.Append(m_details[i]);
			}

			return buffer.ToString();
		}
	}
}