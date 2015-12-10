using System;
using System.Net;
using NuGet;

namespace PackageRunner
{
    /// <summary>
    /// </summary>
    internal class NugetCredentialProvider : ICredentialProvider
    {
        private readonly ICredentials _credentials;

        /// <summary>
        /// </summary>
        public NugetCredentialProvider(string user, string password)
        {
            _credentials = new NetworkCredential(user, password);
        }

        /// <summary>
        /// </summary>
        public ICredentials GetCredentials(Uri uri, IWebProxy proxy, CredentialType credentialType, bool retrying)
        {
            return _credentials;
        }
    }
}