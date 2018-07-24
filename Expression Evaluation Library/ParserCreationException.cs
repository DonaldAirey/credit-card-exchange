﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.Text;

	//* 
	// * A parser creation exception. This exception is used for signalling 
	// * an error in the token or production patterns, making it impossible 
	// * to create a working parser or tokenizer. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class ParserCreationException : Exception
	{

		//* 
		// * The error type enumeration. 
		// 

		public enum ErrorType
		{

			//* 
			// * The internal error type is only used to signal an 
			// * error that is a result of a bug in the parser or 
			// * tokenizer code. 
			// 

			INTERNAL,

			//* 
			// * The invalid parser error type is used when the parser 
			// * as such is invalid. This error is typically caused by 
			// * using a parser without any patterns. 
			// 

			INVALID_PARSER,

			//* 
			// * The invalid token error type is used when a token 
			// * pattern is erroneous. This error is typically caused 
			// * by an invalid pattern type or an erroneous regular 
			// * expression. 
			// 

			INVALID_TOKEN,

			//* 
			// * The invalid production error type is used when a 
			// * production pattern is erroneous. This error is 
			// * typically caused by referencing undeclared productions, 
			// * or violating some other production pattern constraint. 
			// 

			INVALID_PRODUCTION,

			//* 
			// * The infinite loop error type is used when an infinite 
			// * loop has been detected in the grammar. One of the 
			// * productions in the loop will be reported. 
			// 

			INFINITE_LOOP,

			//* 
			// * The inherent ambiguity error type is used when the set 
			// * of production patterns (i.e. the grammar) contains 
			// * ambiguities that cannot be resolved. 
			// 

			INHERENT_AMBIGUITY
		}

		//* 
		// * The error type. 
		// 

		private ErrorType m_type;

		//* 
		// * The token or production pattern name. This variable is only 
		// * set for some error types. 
		// 

		private string m_name;

		//* 
		// * The additional error information string. This variable is only 
		// * set for some error types. 
		// 

		private string m_info;

		//* 
		// * The error details list. This variable is only set for some 
		// * error types. 
		// 

		private ArrayList m_details;

		//* 
		// * Creates a new parser creation exception. 
		// * 
		// * @param type the parse error type 
		// * @param info the additional error information 
		// 

		public ParserCreationException(ErrorType type, string info)
			: this(type, null, info)
		{
		}

		//* 
		// * Creates a new parser creation exception. 
		// * 
		// * @param type the parse error type 
		// * @param name the token or production pattern name 
		// * @param info the additional error information 
		// 

		public ParserCreationException(ErrorType type, string name, string info)
			: this(type, name, info, null)
		{
		}

		//* 
		// * Creates a new parser creation exception. 
		// * 
		// * @param type the parse error type 
		// * @param name the token or production pattern name 
		// * @param info the additional error information 
		// * @param details the error details list 
		// 

		public ParserCreationException(ErrorType type, string name, string info, ArrayList details)
		{

			this.m_type = type;
			this.m_name = name;
			this.m_info = info;
			this.m_details = details;
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
		// * The token or production name property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public string Name
		{
			get { return m_name; }
		}

		//* 
		// * Returns the token or production name. 
		// * 
		// * @return the token or production name 
		// * 
		// * @see #Name 
		// * 
		// * @deprecated Use the Name property instead. 
		// 

		public string GetName()
		{
			return Name;
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
		// * The detailed error information property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public string Details
		{
			get
			{
				StringBuilder buffer = new StringBuilder();

				if (m_details == null)
				{
					return null;
				}
				for (int i = 0; i <= m_details.Count - 1; i++)
				{
					if (i > 0)
					{
						buffer.Append(", ");
						if (i + 1 == m_details.Count)
						{
							buffer.Append("and ");
						}
					}
					buffer.Append(m_details[i]);
				}

				return buffer.ToString();
			}
		}

		//* 
		// * Returns the detailed error information as a string 
		// * 
		// * @return the detailed error information 
		// * 
		// * @see #Details 
		// * 
		// * @deprecated Use the Details property instead. 
		// 

		public string GetDetails()
		{
			return Details;
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

				switch (m_type)
				{
				case ErrorType.INVALID_PARSER:
					buffer.Append("parser is invalid, as ");
					buffer.Append(m_info);
					break;
				case ErrorType.INVALID_TOKEN:
					buffer.Append("token '");
					buffer.Append(m_name);
					buffer.Append("' is invalid, as ");
					buffer.Append(m_info);
					break;
				case ErrorType.INVALID_PRODUCTION:
					buffer.Append("production '");
					buffer.Append(m_name);
					buffer.Append("' is invalid, as ");
					buffer.Append(m_info);
					break;
				case ErrorType.INFINITE_LOOP:
					buffer.Append("infinite loop found in production pattern '");
					buffer.Append(m_name);
					buffer.Append("'");
					break;
				case ErrorType.INHERENT_AMBIGUITY:
					buffer.Append("inherent ambiguity in production '");
					buffer.Append(m_name);
					buffer.Append("'");
					if (m_info != null)
					{
						buffer.Append(" ");
						buffer.Append(m_info);
					}

					if (m_details != null)
					{
						buffer.Append(" starting with ");
						if (m_details.Count > 1)
						{
							buffer.Append("tokens ");
						}
						else
						{
							buffer.Append("token ");
						}
						buffer.Append(Details);
					}

					break;
				default:
					buffer.Append("internal error");
					break;
				}
				return buffer.ToString();
			}
		}

		//* 
		// * Returns the error message. This message will contain all the 
		// * information available. 
		// * 
		// * @return the error message 
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