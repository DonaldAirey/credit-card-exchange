﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Text;

	//* 
	// * A token pattern. This class contains the definition of a token 
	// * (i.e. it's pattern), and allows testing a string against this 
	// * pattern. A token pattern is uniquely identified by an integer id, 
	// * that must be provided upon creation. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class TokenPattern
	{

		//* 
		// * The pattern type enumeration. 
		// 

		public enum PatternType
		{

			//* 
			// * The string pattern type is used for tokens that only 
			// * match an exact string. 
			// 

			STRING,

			//* 
			// * The regular expression pattern type is used for tokens 
			// * that match a regular expression. 
			// 

			REGEXP
		}

		//* 
		// * The token pattern identity. 
		// 

		private int m_id;

		//* 
		// * The token pattern name. 
		// 

		private string m_name;

		//* 
		// * The token pattern type. 
		// 

		private PatternType m_type;

		//* 
		// * The token pattern. 
		// 

		private string m_pattern;

		//* 
		// * The token error flag. If this flag is set, it means that an 
		// * error should be reported if the token is found. The error 
		// * message is present in the errorMessage variable. 
		// * 
		// * @see #errorMessage 
		// 

		private bool m_error;

		//* 
		// * The token error message. This message will only be set if the 
		// * token error flag is set. 
		// * 
		// * @see #error 
		// 

		private string m_errorMessage;

		//* 
		// * The token ignore flag. If this flag is set, it means that the 
		// * token should be ignored if found. If an ignore message is 
		// * present in the ignoreMessage variable, it will also be reported 
		// * as a warning. 
		// * 
		// * @see #ignoreMessage 
		// 

		private bool m_ignore;

		//* 
		// * The token ignore message. If this message is set when the token 
		// * ignore flag is also set, a warning message will be printed if 
		// * the token is found. 
		// * 
		// * @see #ignore 
		// 

		private string m_ignoreMessage;

		//* 
		// * Creates a new token pattern. 
		// * 
		// * @param id the token pattern id 
		// * @param name the token pattern name 
		// * @param type the token pattern type 
		// * @param pattern the token pattern 
		// 

		protected TokenPattern()
		{

		}

		public TokenPattern(int id, string name, PatternType type, string pattern)
		{
			this.SetData(id, name, type, pattern);
		}

		protected void SetData(int id, string name, PatternType type, string pattern)
		{
			this.m_id = id;
			this.m_name = name;
			this.m_type = type;
			this.m_pattern = pattern;
		}

		//* 
		// * The token pattern identity property (read-only). This 
		// * property contains the unique token pattern identity value. 
		// * 
		// * @since 1.5 
		// 

		public int Id
		{
			get { return m_id; }
		}

		//* 
		// * Returns the unique token pattern identity value. 
		// * 
		// * @return the token pattern id 
		// * 
		// * @see #Id 
		// * 
		// * @deprecated Use the Id property instead. 
		// 

		public int GetId()
		{
			return m_id;
		}

		//* 
		// * The token pattern name property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public string Name
		{
			get { return m_name; }
		}

		//* 
		// * Returns the token pattern name. 
		// * 
		// * @return the token pattern name 
		// * 
		// * @see #Name 
		// * 
		// * @deprecated Use the Name property instead. 
		// 

		public string GetName()
		{
			return m_name;
		}

		//* 
		// * The token pattern type property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public PatternType Type
		{
			get { return m_type; }
		}

		//* 
		// * Returns the token pattern type. 
		// * 
		// * @return the token pattern type 
		// * 
		// * @see #Type 
		// * 
		// * @deprecated Use the Type property instead. 
		// 

		public PatternType GetPatternType()
		{
			return m_type;
		}

		//* 
		// * The token pattern property (read-only). This property 
		// * contains the actual pattern (string or regexp) which have 
		// * to be matched. 
		// * 
		// * @since 1.5 
		// 

		public string Pattern
		{
			get { return m_pattern; }
			set { this.m_pattern = value; }
		}

		//* 
		// * Returns te token pattern. 
		// * 
		// * @return the token pattern 
		// * 
		// * @see #Pattern 
		// * 
		// * @deprecated Use the Pattern property instead. 
		// 

		public string GetPattern()
		{
			return m_pattern;
		}

		//* 
		// * The error flag property. If this property is true, the 
		// * token pattern corresponds to an error token and an error 
		// * should be reported if a match is found. When setting this 
		// * property to true, a default error message is created if 
		// * none was previously set. 
		// * 
		// * @since 1.5 
		// 

		public bool Error
		{
			get { return m_error; }
			set
			{
				m_error = value;
				if (m_error && m_errorMessage == null)
				{
					m_errorMessage = "unrecognized token found";
				}
			}
		}

		//* 
		// * The token error message property. The error message is 
		// * printed whenever the token is matched. Setting the error 
		// * message property also sets the error flag to true. 
		// * 
		// * @see #Error 
		// * 
		// * @since 1.5 
		// 

		public string ErrorMessage
		{
			get { return m_errorMessage; }
			set
			{
				m_error = true;
				m_errorMessage = value;
			}
		}

		//* 
		// * Checks if the pattern corresponds to an error token. If this 
		// * is true, it means that an error should be reported if a 
		// * matching token is found. 
		// * 
		// * @return true if the pattern maps to an error token, or 
		// * false otherwise 
		// * 
		// * @see #Error 
		// * 
		// * @deprecated Use the Error property instead. 
		// 

		public bool IsError()
		{
			return Error;
		}

		//* 
		// * Returns the token error message if the pattern corresponds to 
		// * an error token. 
		// * 
		// * @return the token error message 
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
		// * Sets the token error flag and assigns a default error message. 
		// * 
		// * @see #Error 
		// * 
		// * @deprecated Use the Error property instead. 
		// 

		public void SetError()
		{
			Error = true;
		}

		//* 
		// * Sets the token error flag and assigns the specified error 
		// * message. 
		// * 
		// * @param message the error message to display 
		// * 
		// * @see #ErrorMessage 
		// * 
		// * @deprecated Use the ErrorMessage property instead. 
		// 

		public void SetError(string message)
		{
			ErrorMessage = message;
		}

		//* 
		// * The ignore flag property. If this property is true, the 
		// * token pattern corresponds to an ignore token and should be 
		// * skipped if a match is found. 
		// * 
		// * @since 1.5 
		// 

		public bool Ignore
		{
			get { return m_ignore; }
			set { m_ignore = value; }
		}

		//* 
		// * The token ignore message property. The ignore message is 
		// * printed whenever the token is matched. Setting the ignore 
		// * message property also sets the ignore flag to true. 
		// * 
		// * @see #Ignore 
		// * 
		// * @since 1.5 
		// 

		public string IgnoreMessage
		{
			get { return m_ignoreMessage; }
			set
			{
				m_ignore = true;
				m_ignoreMessage = value;
			}
		}

		//* 
		// * Checks if the pattern corresponds to an ignored token. If this 
		// * is true, it means that the token should be ignored if found. 
		// * 
		// * @return true if the pattern maps to an ignored token, or 
		// * false otherwise 
		// * 
		// * @see #Ignore 
		// * 
		// * @deprecated Use the Ignore property instead. 
		// 

		public bool IsIgnore()
		{
			return Ignore;
		}

		//* 
		// * Returns the token ignore message if the pattern corresponds to 
		// * an ignored token. 
		// * 
		// * @return the token ignore message 
		// * 
		// * @see #IgnoreMessage 
		// * 
		// * @deprecated Use the IgnoreMessage property instead. 
		// 

		public string GetIgnoreMessage()
		{
			return IgnoreMessage;
		}

		//* 
		// * Sets the token ignore flag and clears the ignore message. 
		// * 
		// * @see #Ignore 
		// * 
		// * @deprecated Use the Ignore property instead. 
		// 

		public void SetIgnore()
		{
			Ignore = true;
		}

		//* 
		// * Sets the token ignore flag and assigns the specified ignore 
		// * message. 
		// * 
		// * @param message the ignore message to display 
		// * 
		// * @see #IgnoreMessage 
		// * 
		// * @deprecated Use the IgnoreMessage property instead. 
		// 

		public void SetIgnore(string message)
		{
			IgnoreMessage = message;
		}

		//* 
		// * Returns a string representation of this object. 
		// * 
		// * @return a token pattern string representation 
		// 

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();

			buffer.Append(m_name);
			buffer.Append(" (");
			buffer.Append(m_id);
			buffer.Append("): ");
			switch (m_type)
			{
			case PatternType.STRING:
				buffer.Append("\"");
				buffer.Append(m_pattern);
				buffer.Append("\"");
				break;
			case PatternType.REGEXP:
				buffer.Append("<<");
				buffer.Append(m_pattern);
				buffer.Append(">>");
				break;
			}
			if (m_error)
			{
				buffer.Append(" ERROR: \"");
				buffer.Append(m_errorMessage);
				buffer.Append("\"");
			}
			if (m_ignore)
			{
				buffer.Append(" IGNORE");
				if (m_ignoreMessage != null)
				{
					buffer.Append(": \"");
					buffer.Append(m_ignoreMessage);
					buffer.Append("\"");
				}
			}

			return buffer.ToString();
		}

		//* 
		// * Returns a short string representation of this object. 
		// * 
		// * @return a short string representation of this object 
		// 

		public string ToShortString()
		{
			StringBuilder buffer = new StringBuilder();
			int newline = m_pattern.IndexOf(Convert.ToChar(10));

			if (m_type == PatternType.STRING)
			{
				buffer.Append("\"");
				if (newline >= 0)
				{
					if (newline > 0 && m_pattern[newline - 1] == Convert.ToChar(13))
					{
						newline -= 1;
					}
					buffer.Append(m_pattern.Substring(0, newline));
					buffer.Append("(...)");
				}
				else
				{
					buffer.Append(m_pattern);
				}
				buffer.Append("\"");
			}
			else
			{
				buffer.Append("<");
				buffer.Append(m_name);
				buffer.Append(">");
			}

			return buffer.ToString();
		}
	}
}