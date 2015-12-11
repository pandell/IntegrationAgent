using System;
using System.IO;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace PackageRunner
{

    /// <summary>
    /// </summary>
    /// <remarks>
    /// Unit tested in <c>Pandell.PackageRunner.Test.AssemblyExtensionsTests.</c>
    /// </remarks>
    public static class AssemblyExtensions
    {

        //--------------------------------------------------
        /// <summary>
        /// Gets full path of the of the given assembly
        /// (as specified originally, i.e. disregarding shadow copying).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method translates the quasi-URI value returned
        /// by the <see cref="Assembly.CodeBase"/> property
        /// into an absolute path (local or UNC).
        /// Partially inspired by http://stackoverflow.com/a/28319367 .
        /// </para>
        /// <para>
        /// This method is a copy of https://github.com/pandell/Pli/blob/v4.6.0.213/Source/Pandell.Common/AssemblyExtensions.cs#L35-L78
        /// </para>
        /// </remarks>
        [ContractAnnotation("doNotThrow:false => notnull; doNotThrow:true => canbenull")]
        public static string GetCodeBasePath([CanBeNull] this Assembly assembly, bool doNotThrow = false)
        {
            if (assembly == null)
            {
                if (doNotThrow) { return null; }
                throw new ArgumentNullException("assembly");
            }

            const string localPathPrefix = @"file:///";
            if (assembly.CodeBase.StartsWith(localPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return assembly.CodeBase.Substring(localPathPrefix.Length).Replace('/', Path.DirectorySeparatorChar);
            }

            const string uncPathPrefix = @"file://";
            if (assembly.CodeBase.StartsWith(uncPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return assembly.CodeBase.Substring(uncPathPrefix.Length - 2).Replace('/', Path.DirectorySeparatorChar);
            }

            if (doNotThrow) { return null; }
            throw new ArgumentException("Specified assembly has unrecognized code-base location.", "assembly");
        }


        //--------------------------------------------------
        /// <summary>
        /// Invokes <c>ICLRStrongName.StrongNameSignatureVerificationEx</c>
        /// that checks whether the assembly manifest at
        /// the specified path contains a strong name signature
        /// (see https://msdn.microsoft.com/en-us/library/ff844054 for more info).
        /// </summary>
        /// <remarks>
        /// If we ever need failure details, <c>Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameErrorInfo</c>
        /// method can be used immediately after the verify call to get the HRESULT;
        /// more information about .NET CLR HRESULT codes can be found here:
        /// http://blogs.msdn.com/b/yizhang/archive/2010/12/17/interpreting-hresults-returned-from-net-clr-0x8013xxxx.aspx
        /// </remarks>
        public static bool HasValidStrongName([NotNull] this Assembly assembly)
        {
            if (assembly == null) { throw new ArgumentNullException("assembly"); }

            bool wasVerified;
            return AssemblyExtensions.VerifySignatureMethod(assembly.Location, true, out wasVerified) && wasVerified;
        }


        //--------------------------------------------------
        /// <summary>
        /// Compares public key token of the specified assembly
        /// with the specified token, returning true when matching,
        /// false otherwise.
        /// </summary>
        public static bool PublicKeyTokenEqualsTo([NotNull] this Assembly assembly, [NotNull] byte[] expectedToken)
        {
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (expectedToken == null) throw new ArgumentNullException("expectedToken");

            var assemblyToken = assembly.GetName().GetPublicKeyToken();
            return assemblyToken.SequenceEqual(expectedToken);
        }



        //**************************************************
        //* Private
        //**************************************************

        //--------------------------------------------------
        private delegate bool VerifySignatureDelegate(string assemblyPath, bool forceCheck, out bool wasVerified);


        //--------------------------------------------------
        private static readonly VerifySignatureDelegate VerifySignatureMethod = LoadVerifySignatureMethod();


        //--------------------------------------------------
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
