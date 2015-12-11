using System.Reflection;
using NUnit.Framework;
using PackageRunner;

namespace Pandell.PackageRunner.Test
{
    static class AssemblyCheckerTests
    {
        private static readonly byte[] Token = { 0xc4, 0xbc, 0xda, 0xb9, 0xe3, 0xe6, 0xe7, 0xfa };
        private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;

        [Test]
        public static void ValidationSucceed()
        {
            var result = AssemblyChecker.IsValid(AssemblyLocation, Token);
            Assert.AreEqual(true, result);
        }

        [Test]
        public static void ValidationFail()
        {
            var result = AssemblyChecker.IsValid(AssemblyLocation, new byte[] { 0x01 });
            Assert.AreEqual(false, result);
        }
    }
}
