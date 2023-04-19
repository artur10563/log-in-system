using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace PasswordSystemLaba2
{
    internal static class Security
    {
        public class HashedString
        {
            internal int key { get; set; }
            internal string hashedString { get; set; }
            internal HashedString(int key, string hashedString)
            {
                this.key = key;
                this.hashedString = hashedString;
            }

        }

        //Constant for hash function
        private static readonly double a = 0.000_000_5;
        private static readonly double tolerance = 0.000001;

        //Generate random int
        internal static int GenerateRandomInt()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);

                return BitConverter.ToInt32(bytes, 0);
            }
        }

        /// <summary>
        /// Encryption function: lg(a/x)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// 
        internal static HashedString Encrypt(string str)
        {
            int key = GenerateRandomInt();

            double resultHashCode = Math.Log10(a / key) * str.GetHashCode();
            string result = Convert.ToBase64String(BitConverter.GetBytes(resultHashCode));

            //AAAAAAAA+P8= (key value is too big or too small) resulting in same hash
            if (result.CompareTo("AAAAAAAA+P8=") == 0)
                return Encrypt(str);
            else
                return new HashedString(key, result);

        }

        internal static bool Verify(string initialStr, string encryptedStr, int key)
        {
            double d = BitConverter.ToDouble(Convert.FromBase64String(encryptedStr), 0);

            double expectedResult = Math.Log10(a / key) * initialStr.GetHashCode();

            double realResult = BitConverter.ToDouble(Convert.FromBase64String(encryptedStr), 0);

            
            if (Math.Abs(expectedResult - realResult) < tolerance)
                return true;
            else
                return false;
        }

    }
}
