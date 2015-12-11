using System;
using System.Linq;
using System.Reflection;

namespace PackageRunner
{
    /// <summary>
    /// </summary>
    public class AssemblyChecker
    {
        /// <summary>
        /// Invokes <c>ICLRStrongName.StrongNameSignatureVerificationEx</c>
        /// (see https://msdn.microsoft.com/en-us/library/ff844054 for more info).
        /// </summary>
        /// <remarks>
        /// If we ever need failure details, <c>Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameErrorInfo</c>
        /// method can be used immediately after the verify call to get the HRESULT;
        /// more information about .NET CLR HRESULT codes can be found here:
        /// http://blogs.msdn.com/b/yizhang/archive/2010/12/17/interpreting-hresults-returned-from-net-clr-0x8013xxxx.aspx
        /// </remarks>
        public static bool IsStrongNameValid(string fileName)
        {
            bool wasVerified;
            return VerifySignatureMethod(fileName, true, out wasVerified) && wasVerified;
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

        private delegate bool VerifySignatureDelegate(string assemblyPath, bool forceCheck, out bool wasVerified);

        private static readonly VerifySignatureDelegate VerifySignatureMethod = LoadVerifySignatureMethod();

        private static VerifySignatureDelegate LoadVerifySignatureMethod()
        {
            var strongNameHelpers = typeof(AppDomain).Assembly.GetType("Microsoft.Runtime.Hosting.StrongNameHelpers");
            if (strongNameHelpers == null) { throw new InvalidOperationException("StrongNameHelpers cannot be found in mscorlib."); }
            var verifySignatureMethod = strongNameHelpers.GetMethod("StrongNameSignatureVerificationEx");
            if (verifySignatureMethod == null) { throw new InvalidOperationException("StrongNameHelpers.StrongNameSignatureVerificationEx cannot be found in mscorlib."); }
            return (VerifySignatureDelegate)verifySignatureMethod.CreateDelegate(typeof(VerifySignatureDelegate));
        }
    }
}