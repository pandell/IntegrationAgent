using System;
using System.IO;
using System.Reflection;

using JetBrains.Annotations;

using NUnit.Framework;

using KeyTokenValid = Pandell.IntegrationAgent.Token;

namespace Pandell.IntegrationAgent.Test
{

    /// <summary>
    /// Unit tests for <see cref="AssemblyExtensions"/>.
    /// </summary>
    internal static class AssemblyExtensionsTests
    {

        //--------------------------------------------------
        [Test]
        public static void EnableResolvingOfEmbeddedAssemblies_IntegrationAgentAssembly_ProvidesAssembliesNotOnDisk()
        {
            var integrationAgentAssembly = typeof(AssemblyExtensions).Assembly;
            integrationAgentAssembly.EnableResolvingOfEmbeddedAssemblies();

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "IntegrationAgent.exe")), "IntegrationAgent.exe should exist in the current directory");
            Assert.IsFalse(File.Exists(Path.Combine(Environment.CurrentDirectory, "Microsoft.Web.XmlTransform.dll")), "Microsoft.Web.XmlTransform.dll should NOT exist in the current directory");
            Assert.IsNotNull(Assembly.Load("Microsoft.Web.XmlTransform"), "Microsoft.Web.XmlTransform.dll should be available even though it is not on disk");
        }


        //--------------------------------------------------
        [Test]
        public static void GetCodeBasePath_IntegrationAgentAssembly_GetsCurrentDirectoryAndExpectedExeName()
        {
            var integrationAgentAssembly = typeof(AssemblyExtensions).Assembly;
            var codeBasePath = integrationAgentAssembly.GetCodeBasePath();

            StringAssert.AreEqualIgnoringCase(Environment.CurrentDirectory, Path.GetDirectoryName(codeBasePath));
            StringAssert.AreEqualIgnoringCase("IntegrationAgent.exe", Path.GetFileName(codeBasePath));
        }


        ////--------------------------------------------------
        [Test]
        public static void HasValidStrongName_IntegrationAgentAssembly_Succeeds()
        {
            var integrationAgentAssembly = typeof(AssemblyExtensions).Assembly;
            var hasValidStrongName = integrationAgentAssembly.HasValidStrongName();

            Assert.IsTrue(hasValidStrongName, "Package runner assembly should be signed");
        }


        ////--------------------------------------------------
        [Test]
        public static void HasValidStrongName_UnsignedAssembly_Fails()
        {
            var unsignedAssembly = LoadUnsignedAssembly();
            var hasValidStrongName = unsignedAssembly.HasValidStrongName();

            Assert.IsFalse(hasValidStrongName, "Unsigned assembly should not be signed");
        }


        //--------------------------------------------------
        /// <summary>
        /// We cannot use IntegrationAgent assembly, because it
        /// will be signed with different key (the real Pandell
        /// one) on TeamCity (where this test will also run).
        /// The test assembly, however, will always be signed
        /// with the source-controlled key.
        /// </summary>
        [Test]
        public static void PublicKeyTokenEqualsTo_TestsAssembly_ValidatesToken()
        {
            var testsAssembly = typeof(AssemblyExtensionsTests).Assembly;
            var equalsToValidToken = testsAssembly.PublicKeyTokenEqualsTo(Token.Bytes);
            var equalsToInvalidToken = testsAssembly.PublicKeyTokenEqualsTo(KeyTokenInvalid);

            Assert.IsTrue(equalsToValidToken, "Test assembly public key token was expected to match valid token");
            Assert.IsFalse(equalsToInvalidToken, "Test assembly public key token was expected to not match invalid token");
        }


        //--------------------------------------------------
        [Test]
        public static void PublicKeyTokenEqualsTo_UnsignedAssembly_NeverMatches()
        {
            var unsignedAssembly = LoadUnsignedAssembly();
            var equalsToValidToken = unsignedAssembly.PublicKeyTokenEqualsTo(Token.Bytes);
            var equalsToInvalidToken = unsignedAssembly.PublicKeyTokenEqualsTo(KeyTokenInvalid);

            Assert.IsFalse(equalsToValidToken, "Unsigned assembly public key token was expected to not match valid token");
            Assert.IsFalse(equalsToInvalidToken, "Unsigned assembly public key token was expected to not match invalid token");
        }



        //**************************************************
        //* Private
        //**************************************************

        //--------------------------------------------------
        /// <summary>
        /// Random invalid key token.
        /// </summary>
        [NotNull] private static readonly byte[] KeyTokenInvalid = { 0x42, 0x24, 0x42, 0x24, 0x42, 0x24, 0x42, 0x24 };


        //--------------------------------------------------
        /// <summary>
        /// Verifies that unsigned assembly exists on disk,
        /// then loads it.
        /// </summary>
        [NotNull] private static Assembly LoadUnsignedAssembly()
        {
            const string unsignedAssemblyFileName = "../../src/Pandell.IntegrationAgent.Test/AssemblyExtensionsTests_UnsignedAssembly.dll";
            Assert.IsTrue(File.Exists(unsignedAssemblyFileName), "Unsigned assembly is missing");

            return Assembly.LoadFrom(unsignedAssemblyFileName);
        }

    }

}
