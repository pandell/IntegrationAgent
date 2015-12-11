using System;
using System.IO;
using System.Reflection;

using NUnit.Framework;

using PackageRunner;

namespace Pandell.PackageRunner.Test
{

    /// <summary>
    /// Unit tests for <see cref="AssemblyExtensions"/>.
    /// </summary>
    internal static class AssemblyExtensionsTests
    {

        //--------------------------------------------------
        [Test]
        public static void GetCodeBasePath_PackageRunnerAssembly_GetsCurrentDirectoryAndExpectedExeName()
        {
            var packageRunnerAssembly = typeof(AssemblyExtensions).Assembly;
            var codeBasePath = packageRunnerAssembly.GetCodeBasePath();

            StringAssert.AreEqualIgnoringCase(Environment.CurrentDirectory, Path.GetDirectoryName(codeBasePath));
            StringAssert.AreEqualIgnoringCase("PackageRunner.exe", Path.GetFileName(codeBasePath));
        }


        ////--------------------------------------------------
        [Test]
        public static void HasValidStrongName_PackageRunnerAssembly_Succeeds()
        {
            var packageRunnerAssembly = typeof(AssemblyExtensions).Assembly;
            var hasValidStrongName = packageRunnerAssembly.HasValidStrongName();

            Assert.IsTrue(hasValidStrongName, "Package runner assembly should be signed");
        }


        ////--------------------------------------------------
        [Test]
        public static void HasValidStrongName_UnsignedAssembly_Fails()
        {
            var unsignedAssembly = AssemblyExtensionsTests.LoadUnsignedAssembly();
            var hasValidStrongName = unsignedAssembly.HasValidStrongName();

            Assert.IsFalse(hasValidStrongName, "Unsigned assembly should not be signed");
        }


        //--------------------------------------------------
        /// <summary>
        /// We cannot use PackageRunner assembly, because it
        /// will be signed with different key (the real Pandell
        /// one) on TeamCity (where this test will also run).
        /// The test assembly, however, will always be signed
        /// with the source-controlled key.
        /// </summary>
        [Test]
        public static void PublicKeyTokenEqualsTo_TestsAssembly_ValidatesToken()
        {
            var testsAssembly = typeof(AssemblyExtensionsTests).Assembly;
            var equalsToValidToken = testsAssembly.PublicKeyTokenEqualsTo(AssemblyExtensionsTests.KeyTokenValid);
            var equalsToInvalidToken = testsAssembly.PublicKeyTokenEqualsTo(AssemblyExtensionsTests.KeyTokenInvalid);

            Assert.IsTrue(equalsToValidToken, "Test assembly public key token was expected to match valid token");
            Assert.IsFalse(equalsToInvalidToken, "Test assembly public key token was expected to not match invalid token");
        }


        //--------------------------------------------------
        /// <summary>
        /// We cannot use PackageRunner assembly, because it
        /// will be signed with different key (the real Pandell
        /// one) on TeamCity (where this test will also run).
        /// The test assembly, however, will always be signed
        /// with the source-controlled key.
        /// </summary>
        [Test]
        public static void PublicKeyTokenEqualsTo_UnsignedAssembly_NeverMatches()
        {
            var unsignedAssembly = AssemblyExtensionsTests.LoadUnsignedAssembly();
            var equalsToValidToken = unsignedAssembly.PublicKeyTokenEqualsTo(AssemblyExtensionsTests.KeyTokenValid);
            var equalsToInvalidToken = unsignedAssembly.PublicKeyTokenEqualsTo(AssemblyExtensionsTests.KeyTokenInvalid);

            Assert.False(equalsToValidToken, "Unsigned assembly public key token was expected to not match valid token");
            Assert.IsFalse(equalsToInvalidToken, "Unsigned assembly public key token was expected to not match invalid token");
        }



        //**************************************************
        //* Private
        //**************************************************

        //--------------------------------------------------
        private static readonly byte[] KeyTokenValid = { 0xc4, 0xbc, 0xda, 0xb9, 0xe3, 0xe6, 0xe7, 0xfa };


        //--------------------------------------------------
        private static readonly byte[] KeyTokenInvalid = { 0x42 };


        //--------------------------------------------------
        private static Assembly LoadUnsignedAssembly()
        {
            const string unsignedAssemblyFileName = "../../src/Pandell.PackageRunner.Test/AssemblyExtensionsTests_UnsignedAssembly.dll";
            Assert.IsTrue(File.Exists(unsignedAssemblyFileName), "Unsigned assembly is missing");

            return Assembly.LoadFrom(unsignedAssemblyFileName);
        }

    }

}
