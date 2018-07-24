using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FluidTrade.Core
{
	//Extension methods to facilitate string encryption.  Sensitive data like Credit Card can be encrypted
	//using this.
	public sealed class CryptoHelper
	{
		/// <summary>
		/// Minimum length for passwords
		/// </summary>
		public static int MinPasswordLength = 8;

		private static string saltText;
		private static string SaltText
		{
			get
			{
				// Read salt from file
				if (string.IsNullOrEmpty(saltText))
				{
					try
					{
						///Hack -  Move this configuration
						string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Passwordkey.txt");
						using (StreamReader saltStrem = new StreamReader(fileName))
						{
							saltText = saltStrem.ReadLine();
						}
					}
					catch (FileNotFoundException)
					{
						throw new ApplicationException("Password encryption hint missing.");
					}
				}

				return saltText;
			}
		}

		/// <summary>
		/// Encrypts a string using HashAlgorithm and provided salted key. This is a one way hash.  The idea
		/// being if you ever store passwords then you hash it once and then compare hashed values for 
		/// validity.
		/// </summary>
		/// <param name="encryptThis">String to encrypt</param>
		/// <returns>Encrypted String</returns>
		public static string EncryptUsingHash(string encryptThis)
		{

			if (string.IsNullOrEmpty(encryptThis))
				return string.Empty;

			HashAlgorithm sha1 = new SHA1CryptoServiceProvider();

			string saltedString = encryptThis + SaltText;
			byte[] encryptBytes = Encoding.UTF8.GetBytes(saltedString);
			byte[] encryptHash = sha1.ComputeHash(encryptBytes);

			return Convert.ToBase64String(encryptHash);

		}

		/// <summary>
		/// This uses current user protection level to encrypt data.  This will prevent other users
		/// from decrypting data.
		/// </summary>
		/// <param name="encryptThis">String to encrypt</param>
		/// <returns>Encrypted String</returns>
		public static string EncryptPerUser(string encryptThis)
		{
			if (string.IsNullOrEmpty(encryptThis))
				return string.Empty;

			byte[] encodedText = Encoding.UTF8.GetBytes(encryptThis);
			byte[] encodedEntropy = Encoding.UTF8.GetBytes(SaltText);

			byte[] cipherText = ProtectedData.Protect(encodedText, encodedEntropy, DataProtectionScope.CurrentUser);

			return Convert.ToBase64String(cipherText);

		}

		/// <summary>
		/// This uses current user protection level to decrypt data. 
		/// </summary>
		/// <param name="decryptThis"></param>
		/// <returns></returns>
		public static string DecryptPerUser(string decryptThis)
		{
			if (string.IsNullOrEmpty(decryptThis))
				return string.Empty;

			byte[] cipherText = Convert.FromBase64String(decryptThis);
			byte[] encodedEntropy = Encoding.UTF8.GetBytes(SaltText);

			byte[] encodedText = ProtectedData.Unprotect(cipherText, encodedEntropy, DataProtectionScope.CurrentUser);

			return Encoding.UTF8.GetString(encodedText);

		}

		/// <summary>
		/// Checks to see that the password is at least 8 characters in length.  Has at least
		/// one uppercase , one lowercase and 1 digit in it.
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public static bool IsStrongPassword(string password)
		{
			Regex regex = new Regex(@"^(?=.{8,})(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).*$");
			return regex.IsMatch(password);
		}

		/// <summary>
		/// Creates a strong password.
		/// </summary>
		/// <param name="passwordLength"></param>
		/// <returns></returns>
		public static string GenerateStrongPassword(int passwordLength)
		{
			if (passwordLength < MinPasswordLength)
				throw new ApplicationException("Strong passwords cannot be less than " + MinPasswordLength.ToString() + " characters");

			//Add an uppercase and special character
			StringBuilder uppercase = new StringBuilder();
			StringBuilder numericCharacters = new StringBuilder();
			StringBuilder passwordCharacters = new StringBuilder();

			for (int i = 48; i < 58; i++)
				numericCharacters.Append(Convert.ToChar(i));

			for (int i = 65; i < 91; i++)
				uppercase.Append(Convert.ToChar(i));

			for (int i = 33; i < 126; i++)
				passwordCharacters.Append(Convert.ToChar(i));

			char[] charSet = passwordCharacters.ToString().ToCharArray();
			byte[] random = new byte[passwordLength * 2];
			new RNGCryptoServiceProvider().GetBytes(random);

			MemoryStream mStream = new MemoryStream(random, 0, random.Length, false, false);
			BinaryReader reader = new BinaryReader(mStream);
			StringBuilder builder = new StringBuilder(random.Length, random.Length);
			while (passwordLength-- > 0)
			{
				int i = (reader.ReadUInt16() & 0x8FFF) % charSet.Length;
				builder.Append(charSet[i]);
			}

			//Ensure that we have at least one uppercase and one numeric character
			passwordLength = builder.Length;
			System.Random randNum = new System.Random();
			builder[randNum.Next(passwordLength)] = uppercase[randNum.Next(uppercase.Length)];
			builder[randNum.Next(passwordLength)] = numericCharacters[randNum.Next(numericCharacters.Length)];
			return builder.ToString();
		}

	}

}
