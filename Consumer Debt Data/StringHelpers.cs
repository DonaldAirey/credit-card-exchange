
namespace FluidTest
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    
    
    public static class StringHelpers
    {


        /// <summary>
        /// Extension method that counts the number of occurances of pattern in string: source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int CountOccurrences(this String source, String pattern)
        {
            Regex regEx = new Regex(pattern, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.           
            MatchCollection matches = regEx.Matches(source);

            return matches.Count;  
        }


        /// <summary>
        /// Extension method that returns a boolean based on whether or not the passed-in string contains all the same characters
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Boolean HasAllTheSameCharacters(this String source)
        {
            return source.ToCharArray().HasAllTheSameCharacters();
        }

        /// <summary>
        /// Extension method that returns a boolean based on whether or not the passed-in character array contains all the same values
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Boolean HasAllTheSameCharacters(this Char[] source)
        {

            // if all the characters are the same, just return the original source
            if (source.All(c => c == source[0]))
                return true;

            return false;
        }

        /// <summary>
        /// Takes the passed-in token and appends it to a comma delimited string
        /// </summary>
        /// <param name="commaSeparatedString"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static String AppendTokenToCommaDelimitedString(String commaSeparatedString, String token)
        {

            // if the current string is empty, just return the token
            if (String.IsNullOrEmpty(commaSeparatedString))
                return token;

            // else: add a comma and the token to the end of the list
            return commaSeparatedString +
                   TestDataConfig.CsvDelimiter +
                   token;
        }

        /// <summary>
        /// Return a list of string tokens from the passed in comma delimted list
        /// </summary>
        /// <param name="commaSeparatedString"></param>
        /// <returns></returns>
        public static List<String> GetTokensFromCommaDelimitedString(String commaSeparatedString) 
        {
            // count how many delimiters are in the list
            Int32 numDelimiters = commaSeparatedString.CountOccurrences(TestDataConfig.CsvDelimiter.ToString());

            // The number of tokens is the delimiter count plus one
            String[] tokens = new String[numDelimiters + 1];

            // Strip out the individual Ids from the string version of the list
            tokens = commaSeparatedString.Split(TestDataConfig.CsvDelimiter);

            // initialize the list we will be returning
            List<String> tokenList = new List<String>();

            // add each int-based Id to the list we will be returning
            for (Int32 index = 0; index < tokens.Length; index++)
            {
               tokenList.Add(tokens[index]);
            }

            return tokenList;
        }


        /// Generates random string
        /// [skt] The basis for this class was borrowed from a random password generator 
        ///    http://www.obviex.com/Samples/Password.aspx
        /// <summary>
        /// This class will generate a random string of length determined by the parameter(s)
        /// of the Generate() method.  The pool of available characters is comprised of all 
        /// printable characters from the MS Windows Western character set:
        ///    http://www.microsoft.com/globaldev/reference/sbcs/1252.mspx
        /// The consumer has the option of generating only alpha-numeric character strings or
        /// strings that can be comprised of any of the printable characters in the full character 
        /// set.
        /// 
        /// *** NOTE *** 
        /// The comma character has been explicitly ommitted from the character pool.  This is because
        /// the strings 
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////
        public sealed class RandomString
        {
            private RandomString() { }

            // Define default min and max password lengths.
            private static int DEFAULT_MIN_PASSWORD_LENGTH = 8;
            private static int DEFAULT_MAX_PASSWORD_LENGTH = 10;

            // Define supported password characters divided into groups.
            // You can add (or remove) characters to (from) these groups.

            // Characters retrieved from (Windows Western Character Set) 
            // Grouped in order of appearance.  Used Charmap.exe to get the codes
            private static string PASSWORD_CHARS_SPECIAL_1 = "!\"#$%&'()*+,-./";
            private static string PASSWORD_CHARS_NUMERIC = "0123456789";
            private static string PASSWORD_CHARS_SPECIAL_2 = ":;<=>?@";
            private static string PASSWORD_CHARS_UCASE = "ABCDEFGHIJKLMNPQRSTWXYZ";
            private static string PASSWORD_CHARS_SPECIAL_3 = "[\\]^_'";
            private static string PASSWORD_CHARS_LCASE = "abcdefgijklmnopqrstwxyz";
            private static string PASSWORD_CHARS_SPECIAL_4 = "{|}~";
            private static string PASSWORD_CHARS_SPECIAL_5 = "€‚ƒ„…E";
            private static string PASSWORD_CHARS_SPECIAL_6 = "‡ˆ‰Š‹ŒŽ‘’“”•–—Eš›œž";
            private static string PASSWORD_CHARS_SPECIAL_7 = "Ÿ ¡¢£¤¥¦§¨©ª«¬­®¯°±";
            private static string PASSWORD_CHARS_SPECIAL_8 = "³´µ¶·¸¸¹º»¼½¾¿ÀÁÂÃÄÅÆ";
            private static string PASSWORD_CHARS_SPECIAL_9 = "ÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚ";
            private static string PASSWORD_CHARS_SPECIAL_10 = "ÛÜÝÞßàáâãäåæçèéEEE";
            private static string PASSWORD_CHARS_SPECIAL_11 = "Eñòóôõö÷øùúûüþýÿ";


            /// <summary>
            /// Generates a random password.
            /// </summary>
            /// <returns>
            /// Randomly generated password.
            /// </returns>
            /// <remarks>
            /// The length of the generated password will be determined at
            /// random. It will be no shorter than the minimum default and
            /// no longer than maximum default.
            /// </remarks>
            public static string Generate()
            {
                return Generate(DEFAULT_MIN_PASSWORD_LENGTH,
                                DEFAULT_MAX_PASSWORD_LENGTH,
                                false);
            }

            /// <summary>
            /// Generates a random password of the exact length.
            /// </summary>
            /// <param name="length">
            /// Exact password length.
            /// </param>
            /// <param name="alphaNumeric">
            /// Force the returned string to contain only alpha numeric characters.
            /// </param>
            /// <returns>
            /// Randomly generated password.
            /// </returns>
            public static string Generate(int length)
            {
                return Generate(length, length, false);
            }

            public static string Generate(int length, Boolean alphaNumeric)
            {
                return Generate(length, length, alphaNumeric);
            }

            /// <summary>
            /// Generates a random password.
            /// </summary>
            /// <param name="minLength">
            /// Minimum password length.
            /// </param>
            /// <param name="maxLength">
            /// Maximum password length.
            /// </param>
            /// <param name="alphaNumeric">
            /// Force the returned string to contain only alpha numeric characters.
            /// </param>        
            /// <returns>
            /// Randomly generated password.
            /// </returns>
            /// <remarks>
            /// The length of the generated password will be determined at
            /// random and it will fall with the range determined by the
            /// function parameters.
            /// </remarks>
            public static string Generate(int minLength,
                                          int maxLength,
                                          Boolean alphaNumeric)
            {
                // Make sure that input parameters are valid.
                if (minLength <= 0 || maxLength <= 0 || minLength > maxLength)
                    return null;

                // Create a local array containing supported password characters
                // grouped by types. You can remove character groups from this
                // array, but doing so will weaken the password strength.
                char[][] charGroups;

                if (alphaNumeric)
                {
                    // Use only alpha-numeric characters from the Microsoft Windows Western character set

                    charGroups = new char[][]
                                     {
                                         PASSWORD_CHARS_LCASE.ToCharArray(),
                                         PASSWORD_CHARS_UCASE.ToCharArray(),
                                         PASSWORD_CHARS_NUMERIC.ToCharArray()
                                     };
                }
                else
                {
                    // Use ALL printable characters from the Microsoft Windows Western character set

                    charGroups = new char[][]             
                                     {
                                         PASSWORD_CHARS_LCASE.ToCharArray(),
                                         PASSWORD_CHARS_UCASE.ToCharArray(),
                                         PASSWORD_CHARS_NUMERIC.ToCharArray(),
                                         PASSWORD_CHARS_SPECIAL_1.ToCharArray(), 
                                         PASSWORD_CHARS_SPECIAL_2.ToCharArray(),
                                         PASSWORD_CHARS_SPECIAL_3.ToCharArray(),
                                         PASSWORD_CHARS_SPECIAL_4.ToCharArray(),
                                         PASSWORD_CHARS_SPECIAL_5.ToCharArray(),
                                         PASSWORD_CHARS_SPECIAL_6.ToCharArray(), 
                                         PASSWORD_CHARS_SPECIAL_7.ToCharArray(), 
                                         PASSWORD_CHARS_SPECIAL_8.ToCharArray(), 
                                         PASSWORD_CHARS_SPECIAL_9.ToCharArray(), 
                                         PASSWORD_CHARS_SPECIAL_10.ToCharArray(), 
                                         PASSWORD_CHARS_SPECIAL_11.ToCharArray() 
                                     };
                }

                // Use this array to track the number of unused characters in each
                // character group.
                int[] charsLeftInGroup = new int[charGroups.Length];

                // Initially, all characters in each group are not used.
                for (int i = 0; i < charsLeftInGroup.Length; i++)
                    charsLeftInGroup[i] = charGroups[i].Length;

                // Use this array to track (iterate through) unused character groups.
                int[] leftGroupsOrder = new int[charGroups.Length];

                // Initially, all character groups are not used.
                for (int i = 0; i < leftGroupsOrder.Length; i++)
                    leftGroupsOrder[i] = i;

                // Because we cannot use the default randomizer, which is based on the
                // current time (it will produce the same "random" number within a
                // second), we will use a random number generator to seed the
                // randomizer.

                // Use a 4-byte array to fill it with random bytes and convert it then
                // to an integer value.
                byte[] randomBytes = new byte[4];

                // Generate 4 random bytes.
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(randomBytes);

                // Convert 4 bytes into a 32-bit integer value.
                int seed = (randomBytes[0] & 0x7f) << 24 |
                           randomBytes[1] << 16 |
                           randomBytes[2] << 8 |
                           randomBytes[3];

                // Now, this is real randomization.
                Random random = new Random(seed);

                // This array will hold password characters.
                char[] password = null;

                // Allocate appropriate memory for the password.
                if (minLength < maxLength)
                    password = new char[random.Next(minLength, maxLength + 1)];
                else
                    password = new char[minLength];

                // Index of the next character to be added to password.
                int nextCharIdx;

                // Index of the next character group to be processed.
                int nextGroupIdx;

                // Index which will be used to track not processed character groups.
                int nextLeftGroupsOrderIdx;

                // Index of the last non-processed character in a group.
                int lastCharIdx;

                // Index of the last non-processed group.
                int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

                // Generate password characters one at a time.
                for (int i = 0; i < password.Length; i++)
                {
                    // If only one character group remained unprocessed, process it;
                    // otherwise, pick a random character group from the unprocessed
                    // group list. To allow a special character to appear in the
                    // first position, increment the second parameter of the Next
                    // function call by one, i.e. lastLeftGroupsOrderIdx + 1.
                    if (lastLeftGroupsOrderIdx == 0)
                        nextLeftGroupsOrderIdx = 0;
                    else
                        nextLeftGroupsOrderIdx = random.Next(0, lastLeftGroupsOrderIdx + 1);

                    // Get the actual index of the character group, from which we will
                    // pick the next character.
                    nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

                    // Get the index of the last unprocessed characters in this group.
                    lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

                    // If only one unprocessed character is left, pick it; otherwise,
                    // get a random character from the unused character list.
                    if (lastCharIdx == 0)
                        nextCharIdx = 0;
                    else
                        nextCharIdx = random.Next(0, lastCharIdx + 1);

                    // Add this character to the password.
                    password[i] = charGroups[nextGroupIdx][nextCharIdx];

                    // If we processed the last character in this group, start over.
                    if (lastCharIdx == 0)
                        charsLeftInGroup[nextGroupIdx] =
                            charGroups[nextGroupIdx].Length;
                    // There are more unprocessed characters left.
                    else
                    {
                        // Swap processed character with the last unprocessed character
                        // so that we don't pick it until we process all characters in
                        // this group.
                        if (lastCharIdx != nextCharIdx)
                        {
                            char temp = charGroups[nextGroupIdx][lastCharIdx];
                            charGroups[nextGroupIdx][lastCharIdx] =
                                charGroups[nextGroupIdx][nextCharIdx];
                            charGroups[nextGroupIdx][nextCharIdx] = temp;
                        }
                        // Decrement the number of unprocessed characters in
                        // this group.
                        charsLeftInGroup[nextGroupIdx]--;
                    }

                    // If we processed the last group, start all over.
                    if (lastLeftGroupsOrderIdx == 0)
                        lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                    // There are more unprocessed groups left.
                    else
                    {
                        // Swap processed group with the last unprocessed group
                        // so that we don't pick it until we process all groups.
                        if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                        {
                            int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                            leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                leftGroupsOrder[nextLeftGroupsOrderIdx];
                            leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                        }
                        // Decrement the number of unprocessed groups.
                        lastLeftGroupsOrderIdx--;
                    }
                }

                // Convert password characters into a string and return the result.
                return new string(password);
            }
        }


    }


}