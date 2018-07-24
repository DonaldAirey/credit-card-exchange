namespace FluidTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Security.Cryptography;

    /// <summary>
    /// Helper class to assist in some Data Generation-specific tasks
    /// </summary>
    public static class DataGenHelpers
    {

        /// <summary>
        /// Returns the SSN without the dashes in it
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public static String GetRawSsn(String ssn)
        {
            // remove the dashes in the ssn
            string[] ssnParts = ssn.Split('-');
            StringBuilder sb = new StringBuilder();
            foreach (var s in ssnParts)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }
    

        /// <summary>
        /// Inserts dashes into a character array from left to right to create a formated SSN.
        /// Any extra characters in the passed in array are dropped.
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public static String BuildFormattedSsn(String ssn)
        {
            return BuildFormattedSsn(ssn.ToCharArray());           
        }


        /// <summary>
        /// Inserts dashes into a character array from left to right to create a formated SSN.
        /// Any extra characters in the passed in array are dropped.
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public static String BuildFormattedSsn(Char[] ssn)
        {
            StringBuilder sb = new StringBuilder();
                      
            sb.Append(new String(ssn));

            sb.Insert(3, '-'); //insert dash after the first three digits
            sb.Insert(6, '-'); //insert dash after the first six characters (5 digits and the previously-inserted dash)

            // only return a string with 11 characters (fully formated SSN)
            return (sb.Length > 11 ? sb.ToString().Remove(12): sb.ToString());
        }



        /// <summary>
        /// Extension method which returns a random character from a character array
        /// </summary>
        /// <param name="?"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
        public static Char RandomCharacter(this Char[] source, Random random)
        {

            Char current = ' ';

            Int32 count = 0;

            foreach (Char character in source)
            {
                count++;
                if (random.Next(count) == 0)
                {
                    current = character;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }

            return current;

        }


        /// <summary>
        /// Extension method which returns a random element from an enumerable type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
        public static T RandomElement<T>(this IEnumerable<T> source, Random rng)
        {
            T current = default(T); //default: returns 0 for value types and null for ref types
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }



        /// <summary>
        /// Shuffles characters in a string
        /// </summary>
        /// <param name="list"></param>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public static void RandomlyShuffleString(string str, Random random)
        {
            // instance of the random class used for mixing up the list.
            if (random == null)
                random = new Random(DateTime.Now.Millisecond);

            // temp variable need to do the swaping
            int temp = 0;

            char c;

            char[] chars = str.ToCharArray();

            // for every char in the array swap it with another
            for (int i = 0; i < str.Length; i++)
            {
                temp = random.Next(0, str.Length);
                c = chars[temp];
                chars[temp] = chars[i];
                chars[i] = c;
            }
        }


        /// <summary>
        /// Shuffles elements in a List no matter what type its elements are
        /// </summary>
        /// <param name="list"></param>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public static void RandomlyShuffleList<T>(this List<T> list, Random random)
        {
            // temp variable need to do the swaping
            int temp = 0;

            // generic type of item in the list
            T listItem;

            // for every item in the list swap it with another
            for (int i = 0; i < list.Count; i++)
            {
                temp = random.Next(0, list.Count);
                listItem = list[temp];
                list[temp] = list[i];
                list[i] = listItem;
            }
        }

        /// <summary>
        /// Replace the current character with a random replacement
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Char RandomCharacterReplacement(Char character, Random random)
        {
            Char newChar;

            do
            {
                newChar = DataGenHelpers.GetRandomAlphaNumericCharacter(random);

            } while (newChar == character); // try again if we end up with the same character

            return newChar;
        }

        /// <summary>
        /// Returns a random Alpha Numeric character
        /// </summary>
        /// <param name="random"></param>
        public static Char GetRandomAlphaNumericCharacter(Random random)
        {
            Int32 randValue = random.Next(0, 62);
            Char newChar;

            if (randValue < 10)
                newChar = Convert.ToChar(randValue + Convert.ToInt32('0'));

            else if (randValue < 36)
                newChar = Convert.ToChar((randValue - 10) + Convert.ToInt32('A'));

            else
                newChar = Convert.ToChar((randValue - 36) + Convert.ToInt32('a'));

            return newChar;

        }



        /// <summary>
        /// Returns a list of integers extracted from the passed-in, delimited csvList parameter
        /// </summary>
        /// <param name="csvList"></param>
        /// <returns></returns>
        public static List<Int32> GetIdsFromCsvList(String csvList)
        {
            // count how many delimiters are in the list
            Int32 numDelimiters = csvList.CountOccurrences(TestDataConfig.CsvDelimiter.ToString());

            // The number of Ids is the delimiter count plus one
            String[] idArray = new String[numDelimiters + 1];

            // Strip out the individual Ids from the string version of the list
            idArray = csvList.Split(TestDataConfig.CsvDelimiter);

            // initialize the list we will be returning
            List<Int32> idList = new List<int>();

            // add each int-based Id to the list we will be returning
            for (Int32 index = 0; index < idArray.Length; index++)
            {
                idList.Add(Convert.ToInt32(idArray[index]));
            }

            return idList;
        }

    }

}
