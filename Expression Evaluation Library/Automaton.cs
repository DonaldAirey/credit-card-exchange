namespace FluidTrade.ExpressionEvaluation
{

	using System;

	//* 
	// * A deterministic finite state automaton. This is a simple 
	// * automaton for character sequences, currently used for string 
	// * token patterns. It only handles single character transitions 
	// * between states, but supports running in an all case-insensitive 
	// * mode. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// * @since 1.5 
	// 

	internal class Automaton
	{

		//* 
		// * The state value. 
		// 

		private object value;

		//* 
		// * The automaton state transition tree. Each transition from 
		// * this state to another state is added to this tree with the 
		// * corresponding character. 
		// 

		private AutomatonTree tree = new AutomatonTree();

		//* 
		// * Creates a new empty automaton. 
		// 

		public Automaton()
		{
		}

		//* 
		// * Adds a string match to this automaton. New states and 
		// * transitions will be added to extend this automaton to 
		// * support the specified string. 
		// * 
		// * @param str the string to match 
		// * @param caseInsensitive the case-insensitive flag 
		// * @param value the match value 
		// 

		public void AddMatch(string str, bool caseInsensitive, object value)
		{
			Automaton state = default(Automaton);

			if (str.Length == 0)
			{
				this.value = value;
			}
			else
			{
				state = tree.Find(str[0], caseInsensitive);
				if (state == null)
				{
					state = new Automaton();
					state.AddMatch(str.Substring(1), caseInsensitive, value);
					tree.Add(str[0], caseInsensitive, state);
				}
				else
				{
					state.AddMatch(str.Substring(1), caseInsensitive, value);
				}
			}
		}

		//* 
		// * Checks if the automaton matches an input stream. The 
		// * matching will be performed from a specified position. This 
		// * method will not read any characters from the stream, just 
		// * peek ahead. The comparison can be done either in 
		// * case-sensitive or case-insensitive mode. 
		// * 
		// * @param input the input stream to check 
		// * @param pos the starting position 
		// * @param caseInsensitive the case-insensitive flag 
		// * 
		// * @return the match value, or 
		// * null if no match was found 
		// * 
		// * @throws IOException if an I/O error occurred 
		// 

		public object MatchFrom(LookAheadReader input, int pos, bool caseInsensitive)
		{

			object result = null;
			Automaton state = default(Automaton);
			int c = 0;

			c = input.Peek(pos);
			if (tree != null && c >= 0)
			{
				state = tree.Find(Convert.ToChar(c), caseInsensitive);
				if (state != null)
				{
					result = state.MatchFrom(input, pos + 1, caseInsensitive);
				}
			}
			return ((result == null) ? value : result);
		}
	}

	//* 
	// * An automaton state transition tree. This class contains a 
	// * binary search tree for the automaton transitions from one state 
	// * to another. All transitions are linked to a single character. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// * @since 1.5 
	// 

	internal class AutomatonTree
	{

		//* 
		// * The transition character. If this value is set to the zero 
		// * character ('\0'), this tree is empty. 
		// 

		private char value;

		//* 
		// * The transition state. 
		// 

		private Automaton state;

		//* 
		// * The left subtree. 
		// 

		private AutomatonTree left;

		//* 
		// * The right subtree. 
		// 

		private AutomatonTree right;

		//* 
		// * Creates a new empty automaton transition tree. 
		// 

		public AutomatonTree()
		{
		}

		//* 
		// * Finds an automaton state from the specified transition 
		// * character. This method searches this transition tree for a 
		// * matching transition. The comparison can optionally be done 
		// * with a lower-case conversion of the character. 
		// * 
		// * @param c the character to search for 
		// * @param lowerCase the lower-case conversion flag 
		// * 
		// * @return the automaton state found, or 
		// * null if no transition exists 
		// 

		public Automaton Find(char c, bool lowerCase)
		{
			if (lowerCase)
			{
				c = Char.ToLower(c);
			}
			if (value == Convert.ToChar(0) || value == c)
			{
				return state;
			}
			else if (value > c)
			{
				return left.Find(c, false);
			}
			else
			{
				return right.Find(c, false);
			}
		}

		//* 
		// * Adds a transition to this tree. If the lower-case flag is 
		// * set, the character will be converted to lower-case before 
		// * being added. 
		// * 
		// * @param c the character to transition for 
		// * @param lowerCase the lower-case conversion flag 
		// * @param state the state to transition to 
		// 

		public void Add(char c, bool lowerCase, Automaton state)
		{
			if (lowerCase)
			{
				c = Char.ToLower(c);
			}
			if (value == Convert.ToChar(0))
			{
				this.value = c;
				this.state = state;
				this.left = new AutomatonTree();
				this.right = new AutomatonTree();
			}
			else if (value > c)
			{
				left.Add(c, false, state);
			}
			else
			{
				right.Add(c, false, state);
			}
		}
	}
}