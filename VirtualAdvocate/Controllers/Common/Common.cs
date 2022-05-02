#region NameSpaces
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Common
namespace VirtualAdvocate.Common
{
    #region Helper
    public static class Helper
    {
        #region Global Variables
        static byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool"); 
        #endregion

        #region RetrievePassword
        public static string RetrievePassword()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RandomString(1, false));
            builder.Append(RandomNumber(10, 99));
            builder.Append(RandomSpecialCharacters());
            builder.Append(RandomString(2, true));
            return builder.ToString();

        }
        #endregion

        #region RandomString
        public static string RandomString(int size, bool lowercase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                if (lowercase)
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)), CultureInfo.InvariantCulture);
                else
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(15 * random.NextDouble() + 65)), CultureInfo.InvariantCulture);

                builder.Append(ch);
            }
            if (lowercase)
                return builder.ToString().ToLower(CultureInfo.CurrentCulture);
            return builder.ToString();
        }
        #endregion

        #region RandomNumber
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
        #endregion

        #region RandomSpecialCharacters
        public static string RandomSpecialCharacters()
        {
            int val = ((DateTime.Now.Second) / 4);

            string[] strSplChr = new string[] { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "{", "}", "-", "_", "?", "+" };

            return strSplChr[val];
        }
        #endregion

        #region ComputeHash
        /// <summary>
        /// Accepts the password, generates a eight byte salt value and password hash using SHA512. 
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static byte[] ComputeHash(string plaintext)
        {
            // If salt is not specified, generate it on the fly.

            byte[] saltBytes;

            // Define min and max salt sizes.
            int minSaltSize = 8;
            int maxSaltSize = 8;

            // Generate a random number for the size of the salt.
            Random random = new Random();
            int saltSize = random.Next(minSaltSize, maxSaltSize);

            // Allocate a byte array, which will hold the salt.
            saltBytes = new byte[saltSize];

            // Initialize a random number generator.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            // Fill the salt with cryptographically strong byte values.
            rng.GetNonZeroBytes(saltBytes);

            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plaintext);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            // Because we support multiple hashing algorithms, we must define
            // hash object as a common (abstract) base class. We will specify the
            // actual hashing algorithm class later during object creation.
            HashAlgorithm hash;
            hash = new SHA512Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length +
                                                saltBytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            // Return the result.
            return hashWithSaltBytes;
        }
        #endregion

        #region Encrypt
        public static string Encrypt(string originalString)
        {
            byte[] data_byte = Encoding.UTF8.GetBytes(originalString);
            return HttpUtility.UrlEncode(Convert.ToBase64String(data_byte));
        }
        #endregion

        #region Decrypt
        public static string Decrypt(string cryptedString)
        {
            byte[] data_byte = Convert.FromBase64String(HttpUtility.UrlDecode(cryptedString));
            return Encoding.UTF8.GetString(data_byte);
        }
        #endregion

        #region GetBaseUrl
        public static string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;
            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }
        #endregion

        #region GetUtcDate
        /// <summary>
        /// To return the dbDate
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTime GetUtcDate(VirtualAdvocateEntities context)
        {
            var dbQuery = ((IObjectContextAdapter)context).ObjectContext.CreateQuery<DateTime>("CurrentUtcDateTime()");
            DateTime dbDate = dbQuery.AsEnumerable().First();

            return dbDate;
        }
        #endregion

        #region GetDateFromUtc
        /// <summary>
        /// To convert utc datetime to local datetime
        /// </summary>
        /// <param name="utcDate"></param>
        /// <returns></returns>
        public static DateTime GetDateFromUtc(DateTime utcDate)
        {
            HttpCookie timeZoneCookie = HttpContext.Current.Request.Cookies["timeZoneCookie"];
            if (timeZoneCookie != null)
            {
                var offset = int.Parse(timeZoneCookie.Value);
                DateTime localVersion = utcDate.AddMinutes(-1 * offset);

                return localVersion;
            }
            else
                return utcDate.ToLocalTime();
        }
        #endregion

        #region ValidateAtLeastOneCheckedAttribute
        public class ValidateAtLeastOneCheckedAttribute : ValidationAttribute
        {
            #region IsValid
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Type type = value.GetType();
                IEnumerable<PropertyInfo> checkBoxeProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType == typeof(bool));

                foreach (PropertyInfo checkBoxProperty in checkBoxeProperties)
                {
                    var isChecked = (bool)checkBoxProperty.GetValue(value);
                    if (isChecked)
                    {
                        return ValidationResult.Success;
                    }
                }

                return new ValidationResult(base.ErrorMessageString);
            } 
            #endregion
        } 
        #endregion

    } 
    #endregion
} 
#endregion