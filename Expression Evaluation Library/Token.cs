namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Text;

	//* 
	// * A token node. This class represents a token (i.e. a set of adjacent 
	// * characters) in a parse tree. The tokens are created by a tokenizer, 
	// * that groups characters together into tokens according to a set of 
	// * token patterns. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class Token : Node
	{

		//* 
		// * The token pattern used for this token. 
		// 

		private TokenPattern m_pattern;

		//* 
		// * The characters that constitute this token. This is normally 
		// * referred to as the token image. 
		// 

		private string m_image;

		//* 
		// * The line number of the first character in the token image. 
		// 

		private int m_startLine;

		//* 
		// * The column number of the first character in the token image. 
		// 

		private int m_startColumn;

		//* 
		// * The line number of the last character in the token image. 
		// 

		private int m_endLine;

		//* 
		// * The column number of the last character in the token image. 
		// 

		private int m_endColumn;

		//* 
		// * The previous token in the list of tokens. 
		// 

		private Token m_previous;

		//* 
		// * The next token in the list of tokens. 
		// 

		private Token m_next;

		//* 
		// * Creates a new token. 
		// * 
		// * @param pattern the token pattern 
		// * @param image the token image (i.e. characters) 
		// * @param line the line number of the first character 
		// * @param col the column number of the first character 
		// 

		public Token(TokenPattern pattern, string image, int line, int col)
		{
			this.m_pattern = pattern;
			this.m_image = image;
			this.m_startLine = line;
			this.m_startColumn = col;
			this.m_endLine = line;
			this.m_endColumn = col + image.Length - 1;
			int pos = 0;
			while (image.IndexOf(Convert.ToChar(10), pos) >= 0)
			{
				pos = image.IndexOf(Convert.ToChar(10), pos) + 1;
				this.m_endLine += 1;
				m_endColumn = image.Length - pos;
			}
		}

		//* 
		// * The node type id property (read-only). This value is set as 
		// * a unique identifier for each type of node, in order to 
		// * simplify later identification. 
		// * 
		// * @since 1.5 
		// 

		public override int Id
		{
			get { return m_pattern.Id; }
		}

		//* 
		// * The node name property (read-only). 
		// * 
		// * @since 1.5 
		// 

		public override string Name
		{
			get { return m_pattern.Name; }
		}

		//* 
		// * The line number property of the first character in this 
		// * node (read-only). If the node has child elements, this 
		// * value will be fetched from the first child. 
		// * 
		// * @since 1.5 
		// 

		public override int StartLine
		{
			get { return m_startLine; }
		}

		//* 
		// * The column number property of the first character in this 
		// * node (read-only). If the node has child elements, this 
		// * value will be fetched from the first child. 
		// * 
		// * @since 1.5 
		// 

		public override int StartColumn
		{
			get { return m_startColumn; }
		}

		//* 
		// * The line number property of the last character in this node 
		// * (read-only). If the node has child elements, this value 
		// * will be fetched from the last child. 
		// * 
		// * @since 1.5 
		// 

		public override int EndLine
		{
			get { return m_endLine; }
		}

		//* 
		// * The column number property of the last character in this 
		// * node (read-only). If the node has child elements, this 
		// * value will be fetched from the last child. 
		// * 
		// * @since 1.5 
		// 

		public override int EndColumn
		{
			get { return m_endColumn; }
		}

		//* 
		// * The token image property (read-only). The token image 
		// * consists of the input characters matched to form this 
		// * token. 
		// * 
		// * @since 1.5 
		// 

		public string Image
		{
			get { return m_image; }
		}

		//* 
		// * Returns the token image. The token image consists of the 
		// * input characters matched to form this token. 
		// * 
		// * @return the token image 
		// * 
		// * @see #Image 
		// * 
		// * @deprecated Use the Image property instead. 
		// 

		public string GetImage()
		{
			return Image;
		}

		//* 
		// * The token pattern property (read-only). 
		// 

		internal TokenPattern Pattern
		{
			get { return m_pattern; }
		}

		//* 
		// * The previous token property. If the token list feature is 
		// * used in the tokenizer, all tokens found will be chained 
		// * together in a double-linked list. The previous token may be 
		// * a token that was ignored during the parsing, due to it's 
		// * ignore flag being set. If there is no previous token or if 
		// * the token list feature wasn't used in the tokenizer (the 
		// * default), the previous token will always be null. 
		// * 
		// * @see #Next 
		// * @see Tokenizer#UseTokenList 
		// * 
		// * @since 1.5 
		// 

		public Token Previous
		{
			get { return m_previous; }
			set
			{
				if (m_previous != null)
				{
					m_previous.Next = null;
				}
				m_previous = value;
				if (m_previous != null)
				{
					m_previous.Next = this;
				}
			}
		}

		//* 
		// * Returns the previous token. The previous token may be a token 
		// * that has been ignored in the parsing. Note that if the token 
		// * list feature hasn't been used in the tokenizer, this method 
		// * will always return null. By default the token list feature is 
		// * not used. 
		// * 
		// * @return the previous token, or 
		// * null if no such token is available 
		// * 
		// * @see #Previous 
		// * @see #GetNextToken 
		// * @see Tokenizer#UseTokenList 
		// * 
		// * @since 1.4 
		// * 
		// * @deprecated Use the Previous property instead. 
		// 

		public Token GetPreviousToken()
		{
			return Previous;
		}

		//* 
		// * The next token property. If the token list feature is used 
		// * in the tokenizer, all tokens found will be chained together 
		// * in a double-linked list. The next token may be a token that 
		// * was ignored during the parsing, due to it's ignore flag 
		// * being set. If there is no next token or if the token list 
		// * feature wasn't used in the tokenizer (the default), the 
		// * next token will always be null. 
		// * 
		// * @see #Previous 
		// * @see Tokenizer#UseTokenList 
		// * 
		// * @since 1.5 
		// 

		public Token Next
		{
			get { return m_next; }
			set
			{
				if (m_next != null)
				{
					m_next.Previous = null;
				}
				m_next = value;
				if (m_next != null)
				{
					m_next.Previous = this;
				}
			}
		}

		//* 
		// * Returns the next token. The next token may be a token that has 
		// * been ignored in the parsing. Note that if the token list 
		// * feature hasn't been used in the tokenizer, this method will 
		// * always return null. By default the token list feature is not 
		// * used. 
		// * 
		// * @return the next token, or 
		// * null if no such token is available 
		// * 
		// * @see #Next 
		// * @see #GetPreviousToken 
		// * @see Tokenizer#UseTokenList 
		// * 
		// * @since 1.4 
		// * 
		// * @deprecated Use the Next property instead. 
		// 

		public Token GetNextToken()
		{
			return Next;
		}

		//* 
		// * Returns a string representation of this token. 
		// * 
		// * @return a string representation of this token 
		// 

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();
			int newline = m_image.IndexOf(Convert.ToChar(10));

			buffer.Append(m_pattern.Name);
			buffer.Append("(");
			buffer.Append(m_pattern.Id);
			buffer.Append("): \"");
			if (newline >= 0)
			{
				if (newline > 0 && m_image[newline - 1] == Convert.ToChar(13))
				{
					newline -= 1;
				}
				buffer.Append(m_image.Substring(0, newline));
				buffer.Append("(...)");
			}
			else
			{
				buffer.Append(m_image);
			}
			buffer.Append("\", line: ");
			buffer.Append(m_startLine);
			buffer.Append(", col: ");
			buffer.Append(m_startColumn);

			return buffer.ToString();
		}

		//* 
		// * Returns a short string representation of this token. The 
		// * string will only contain the token image and possibly the 
		// * token pattern name. 
		// * 
		// * @return a short string representation of this token 
		// 

		public string ToShortString()
		{
			StringBuilder buffer = new StringBuilder();
			int newline = m_image.IndexOf(Convert.ToChar(10));

			buffer.Append('"');
			if (newline >= 0)
			{
				if (newline > 0 && m_image[newline - 1] == Convert.ToChar(13))
				{
					newline -= 1;
				}
				buffer.Append(m_image.Substring(0, newline));
				buffer.Append("(...)");
			}
			else
			{
				buffer.Append(m_image);
			}
			buffer.Append('"');
			if (m_pattern.Type == TokenPattern.PatternType.REGEXP)
			{
				buffer.Append(" <");
				buffer.Append(m_pattern.Name);
				buffer.Append(">");
			}

			return buffer.ToString();
		}
	}
}