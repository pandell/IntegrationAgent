using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PackageRunner
{
    /// <summary>
    /// </summary>
    internal class AssemblyChecker
    {
        [DllImport("mscoree.dll", CharSet = CharSet.Unicode)]
        private static extern bool StrongNameSignatureVerificationEx(string wszFilePath, byte fForceVerification, ref byte pfWasVerified);

        /// <summary>
        /// </summary>
        public static bool IsStrongNameValid(string fileName)
        {
            var forceVerification = Convert.ToByte(true);
            var wasVerified = Convert.ToByte(false);

            return StrongNameSignatureVerificationEx(fileName, forceVerification, ref wasVerified);
        }

        /// <summary>
        /// </summary>
        public static bool IsPublicKeyTokenValid(string fileName, byte[] expectedToken)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (expectedToken == null) throw new ArgumentNullException("expectedToken");

            try
            {
                // Get the public key token of the given assembly 
                var asmToken = AssemblyName.GetAssemblyName(fileName).GetPublicKeyToken();

                // Compare it to the given token
                if (asmToken.Length != expectedToken.Length)
                {
                    return false;
                }

                return !asmToken.Where((t, i) => t != expectedToken[i]).Any();
            }
            catch (System.IO.FileNotFoundException)
            {
                // couldn’t find the assembly
                return false;
            }
            catch (BadImageFormatException)
            {
                // the given file couldn’t get through the loader
                return false;
            }
        }

        /// <summary>
        /// </summary>
        public static bool IsValid(string fileName, byte[] expectedToken)
        {
            var isStrongNameValid = IsStrongNameValid(fileName);
            var isPublicKeyTokenValid = IsPublicKeyTokenValid(fileName, expectedToken);

            return isStrongNameValid && isPublicKeyTokenValid;
        }
    }
}