using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluidTrade.Core.Windows
{
    /// <summary>
    /// Formats a string using a mask.
    /// </summary>
    public class MaskTextFormatter
    {

        #region Public Methods and Properties    
        
        /// <summary>
        /// Delegate to validate input.
        /// </summary>
        public Func<char, bool> Validate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mask"></param>
        public MaskTextFormatter(string mask)
        {
            this.mask = mask;
            CreateSubstitutionDictionary();
            m_formattedString = new char[this.mask.Length];
        }

        /// <summary>
        ///Length of the formatted string so far. 
        /// </summary>          
        public int Length
        {
            get 
            {
                int index = 0;
                while (index < m_formattedString.Length && m_formattedString[index] != '\x0')
                    index++;

                return index; 
            }
        }

        /// <summary>
        /// Total capacity that this field can hold.
        /// </summary>
        public int Capacity
        {
            get { return m_formattedString.Length; }
        }

        /// <summary>
        /// Insert a string at a position.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public int InsertAt(string text, int position)
        {
            try
            {
                bool insertSucceeded = false;
                foreach (var character in text)
                {
                    if (Length < Capacity)
                        insertSucceeded = Insert(character, ref position);
                    else
                        insertSucceeded = Replace(character, ref position);

                    if(insertSucceeded)
                        position++;
                }
            }
            catch (EndOfStreamException)
            {               
            }

            return position;
        }

        /// <summary>
        /// Replace text at a position.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public int Replace(string text, int position)
        {
            try
            {
                foreach (var character in text)
                {
                    Replace(character, ref position);
                    position++;
                }
            }
            catch (EndOfStreamException)
            {
            }

            return position;
        }

        /// <summary>
        /// Removes a selection.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Remove(int position, int count)
        {
            for (int index = 0; index < count; index++)
            {
                if (Remove(position) == false)
                    return false;
            }
            
            return true;

        }

        /// <summary>
        /// Removes a character at a position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Remove(int position)
        {
            try                       
            {
                if (position > Capacity)
                    return false;

                if (m_charSubsitution.ContainsKey(position))
                {
                    position--;
                }

                Assign('\x0', position);
                if (position < this.mask.Length)
                    ShiftCharsLeft(position);               
            }
            catch (EndOfStreamException)
            {
                //TODO: Log exception 
                return false;
            }

            return true;

        }

        /// <summary>
        /// Replaces the whole string.
        /// </summary>
        /// <param name="text"></param>
        public void Set(string text)
        {
            try
            {
                int position = 0;
                foreach (var character in text)
                {
                    if (Insert(character, ref position) == true)
                        position++;
                }
            }
            catch (EndOfStreamException)
            {
                return;              
            }            
        }

        /// <summary>
        /// Finds first editable character.
        /// </summary>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public int FindEditPositionFrom(int startPosition)
        {
            int nextEditable = startPosition;
            for (; nextEditable < m_formattedString.Length; nextEditable++)
            {
                if (m_formattedString[nextEditable] == '\x0')
                    break;             
            }

            return nextEditable;
            
        }

        /// <summary>
        /// Formatted string. 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder formatedString = new StringBuilder();
            foreach (char character in m_formattedString)
            {
                if(character == '\x0')
                    break;

                formatedString.Append(character);
            }

            return formatedString.ToString();
        }

        #endregion
        
        /// <summary>
        /// Create an internal dictionary for lookup.
        /// </summary>
        private void CreateSubstitutionDictionary()
        {
            m_charSubsitution.Clear();
            for (int index = 0; index < this.mask.Length; index++)
            {
                if (!Char.IsDigit(this.mask[index]))
                {
                    m_charSubsitution.Add(index, this.mask[index]);
                }
            }
        }

        /// <summary>
        /// Inserts a character at a position
        /// </summary>
        /// <param name="character">To insert.</param>
        /// <param name="position"></param>
        private bool Insert(char character, ref int position)
        {
            //If a validation callback exists then validate before continuing.
            if (Validate != null &&
                Validate.Invoke(character) == false)
            {
                return false;
            }

            //Make room for th character to insert.
            ShiftCharsRight(position, Length);
            //Lookup the position to see if mask character exists.
            if (m_charSubsitution.ContainsKey(position))
            {
                Assign(m_charSubsitution[position], position);
                if (m_charSubsitution[position] == character)
                    return true;

                position++;
            }
            
            Assign(character, position);
            return true;
        }


        /// <summary>
        /// Shifts the character one space starting from position
        /// </summary>
        /// <param name="position"></param>
        private void ShiftCharsLeft(int position)
        {
            int nextPosition = position + 1;
            if (m_charSubsitution.ContainsKey(nextPosition))
            {
                nextPosition++;
            }

            if (nextPosition < m_formattedString.Length)
            {
                Assign(m_formattedString[nextPosition], position);
                if (m_formattedString[nextPosition] != '\x0')
                    ShiftCharsLeft(nextPosition);
            }
            else
            {
                Assign('\x0', position);
            }
        }

        /// <summary>
        ///Shifts the character one space starting from startposition till lastIndex.
        /// </summary>
        /// <param name="startposition"></param>
        /// <param name="lastIndex"></param>
        private void ShiftCharsRight(int startposition, int lastIndex)
        {
            if (lastIndex <= startposition)
                return;

            int position = lastIndex - 1;
            if (m_formattedString[position] == '\x0')
                return;

            if (m_charSubsitution.ContainsKey(position))
            {
                Assign(m_charSubsitution[position], position);
                position--;
            }
            
            Assign(m_formattedString[position], lastIndex);
            

            ShiftCharsRight(startposition, position);
        }

        /// <summary>
        /// Replaces the character at a position.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="position"></param>
        private bool Replace(char character, ref int position)
        {
            if (m_charSubsitution.ContainsKey(position))
            {
                Assign(m_charSubsitution[position], position);
                position++;
            }

            Assign(character, position);
            return true;
        }

        /// <summary>
        /// Assigns a character at a postion.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="position"></param>
        private void Assign(char character, int position)
        {
            if (position >= this.mask.Length)
            {
                return;
                //throw new EndOfStreamException("MaskTextFormatter: Trying to add past the Mask property");
            }
            
            m_formattedString[position] = character;

        }

        private char[] m_formattedString;
        private readonly string mask;
        private IDictionary<int, char> m_charSubsitution = new Dictionary<int, char>();
        
    }    

}
