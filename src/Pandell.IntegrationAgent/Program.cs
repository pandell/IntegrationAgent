using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;

using NuGet;

namespace Pandell.IntegrationAgent
{
    /// <summary>
    /// </summary>
    internal class Parameters
    {
        public string Package;
        public string Token;
        public string Repository;
        public string RepositoryUsername;
        public string RepositoryPassword;
        public string Config;
        public bool DisableUpdates;
        public bool ShowHelp;
        public bool ShowVersion;
        public TraceLevel TraceLevel = TraceLevel.Warning;
    }

    /// <summary>
    /// </summary>
    internal class Configuration
    {
        public string package;
        public string token;
        public string repository;
        public string repositoryUsername;
        public string repositoryPassword;
    }

    /// <summary>
    /// </summary>
    internal sealed class Program
    {
        /// <summary />
        [Import("IntegrationAgentMain", AllowDefault = true, AllowRecomposition = false, RequiredCreationPolicy = CreationPolicy.Any, Source = ImportSource.Any)]
        public Action<string, Func<TraceLevel, string, bool>> RunAssembly { get; set; }

        /// <summary />
        private const string NuGetRepository = "https://www.nuget.org/api/v2/";

        /// <summary>
        /// </summary>
        private static int Main(string[] args)
        {
            var traceWriter = Program.CreateTraceWriter(TraceLevel.Warning);
            
            try
            {
                var integrationAgentAssembly = Assembly.GetExecutingAssembly();
                var integrationAgentExeFileName = integrationAgentAssembly.GetCodeBasePath();
                var integrationAgentExeDirectory = Path.GetDirectoryName(integrationAgentExeFileName) ?? ".";

                integrationAgentAssembly.EnableResolvingOfEmbeddedAssemblies();
                var program = new Program();
                return program.Run(integrationAgentAssembly, integrationAgentExeFileName, integrationAgentExeDirectory, args);
            }
            catch (Exception ex)
            {
                traceWriter(TraceLevel.Error, ex.Message);
                if (ex.InnerException != null)
                {
                    traceWriter(TraceLevel.Error, ex.InnerException.Message);
                }
                return -1;
            }
        }

        /// <summary>
        /// </summary>
        private int Run(Assembly integrationAgentAssembly, string integrationAgentExeFileName, string integrationAgentExeDirectory, string[] args)
        {
            var parameters = Program.ParseArguments(args);
            var fileVersion = FileVersionInfo.GetVersionInfo(integrationAgentAssembly.Location).FileVersion;

            if (parameters.ShowVersion || parameters.ShowHelp)
            {
                Console.WriteLine("IntegrationAgent  v" + fileVersion);
            }

            if (parameters.ShowHelp)
            {
                Program.ShowHelp();
            }

            if (parameters.ShowVersion || parameters.ShowHelp)
            {
                return 0;
            }

            // Verify that assembly is signed and uses the correct key
            var traceWriter = Program.CreateTraceWriter(parameters.TraceLevel);
            traceWriter(TraceLevel.Verbose, "Checking assembly strong name.");
            if (!integrationAgentAssembly.HasValidStrongName())
            {
                traceWriter(TraceLevel.Error, "Unsigned assembly!");
                return 1;
            }
            traceWriter(TraceLevel.Verbose, "Verifying assembly signature.");
            if (!integrationAgentAssembly.PublicKeyTokenEqualsTo(Token.Bytes))
            {
                traceWriter(TraceLevel.Error, "Invalid assembly!");
                return 2;
            }

            // If no JSON config file name provided as paramter uses the application name
            traceWriter(TraceLevel.Verbose, "Looking for JSON config file.");
            var configFile = Path.Combine(integrationAgentExeDirectory, Path.GetFileNameWithoutExtension(integrationAgentExeFileName) + ".json");
            if (!string.IsNullOrEmpty(parameters.Config))
            {
                if (!parameters.Config.EndsWith(".json"))
                {
                    parameters.Config = parameters.Config + ".json";
                }
                configFile = Path.Combine(integrationAgentExeDirectory, parameters.Config);
            }

            // Check and reads the configuration file
            var configuration = new Configuration();
            if (File.Exists(configFile))
            {
                traceWriter(TraceLevel.Verbose, "Reading the JSON config file.");
                var configJson = File.ReadAllText(configFile);
                var jsonSerializer = new JavaScriptSerializer();
                configuration = jsonSerializer.Deserialize<Configuration>(configJson) ?? configuration;
                traceWriter(TraceLevel.Verbose, "JSON config file loaded.");
            }

            // Merges config file and command line parameters. Command line paramters have precedence.
            configuration.package = parameters.Package ?? configuration.package;
            configuration.token = parameters.Token ?? configuration.token;
            configuration.repository = parameters.Repository ?? configuration.repository;
            configuration.repositoryUsername = parameters.RepositoryUsername ?? configuration.repositoryUsername;
            configuration.repositoryPassword = parameters.RepositoryPassword ?? configuration.repositoryPassword;

            traceWriter(TraceLevel.Verbose, "Checking input parameters.");
            if (string.IsNullOrWhiteSpace(configuration.package) && string.IsNullOrEmpty(configuration.token))
            {
                traceWriter(TraceLevel.Error, "Invalid configuration!");
                return 3;
            }

            // Initializes NuGet repositories
            traceWriter(TraceLevel.Verbose, "Initializing NuGet repositories.");
            var nugetRepository = new DataServicePackageRepository(new Uri(NuGetRepository));
            var aggregateRepository = new AggregateRepository(new[] { nugetRepository });
            if (Uri.IsWellFormedUriString(configuration.repository, UriKind.Absolute))
            {
                if (!string.IsNullOrWhiteSpace(configuration.repositoryUsername) &&
                    !string.IsNullOrWhiteSpace(configuration.repositoryPassword))
                {
                    HttpClient.DefaultCredentialProvider = new NugetCredentialProvider(
                        configuration.repositoryUsername, configuration.repositoryPassword);
                }
                var client = new HttpClient(new Uri(configuration.repository));
                var customRepository = new DataServicePackageRepository(client);
                aggregateRepository = new AggregateRepository(new[] { customRepository, nugetRepository });
            }

            // Perform auto-update if not disabled
            if (!parameters.DisableUpdates)
            {
                traceWriter(TraceLevel.Verbose, "Checking for self update.");
                var integrationAgentAssemblyName = integrationAgentAssembly.GetName();
                var version = new SemanticVersion(integrationAgentAssemblyName.Version);
                var package = aggregateRepository
                    .GetUpdates(new[] { new PackageName(integrationAgentAssemblyName.Name, version) }, includePrerelease: false, includeAllVersions: false)
                    .OrderBy(p => p.Version)
                    .LastOrDefault();

                if (package != null && package.Version > version)
                {
                    traceWriter(TraceLevel.Verbose, "Newer version found. Updating files.");
                    var filename = Path.GetFileName(integrationAgentExeFileName);
                    var file = package.GetFiles().FirstOrDefault(f => !string.IsNullOrEmpty(f.Path) && Path.GetFileName(f.Path).Equals(filename, StringComparison.OrdinalIgnoreCase));
                    if (file != null)
                    {
                        File.Delete(integrationAgentExeFileName + ".bak");
                        File.Move(integrationAgentExeFileName, integrationAgentExeFileName + ".bak");
                        using (Stream fromStream = file.GetStream(), toStream = File.Create(integrationAgentExeFileName))
                        {
                            fromStream.CopyTo(toStream);
                        }
                        Process.Start(integrationAgentExeFileName, string.Join(" ", args) + " -disableupdates");
                        Environment.Exit(0);
                    }
                }
                else
                {
                    traceWriter(TraceLevel.Verbose, "Version is up to date.");
                }
            }

            // Install the package to run including its dependencies
            traceWriter(TraceLevel.Verbose, "Checking for execution package.");
            var packagesPath = Path.Combine(integrationAgentExeDirectory, "packages");
            var remotePackage = aggregateRepository.FindPackagesById(configuration.package).OrderBy(p => p.Version).LastOrDefault();
            var localRepository = new SharedPackageRepository(packagesPath);
            if (!localRepository.Exists(remotePackage))
            {
                traceWriter(TraceLevel.Verbose, "Execution package not found localy. Installing remote.");
                var packageManager = new PackageManager(aggregateRepository, packagesPath);
                packageManager.InstallPackage(remotePackage, ignoreDependencies: false, allowPrereleaseVersions: false);
            }

            var localPackage = localRepository.FindPackagesById(configuration.package).OrderBy(p => p.Version).LastOrDefault();
            if (localPackage == null)
            {
                traceWriter(TraceLevel.Error, "Package not found!");
                return 4;
            }

            // Build a dictionary list of assemblies based on assembly fully qualified name for dynamically resolving from the loaded package
            traceWriter(TraceLevel.Verbose, "Resolving execution package dependencies.");
            var allAssemblies = localRepository
                .GetPackages()
                .ToArray()
                .SelectMany(p => p.AssemblyReferences.Select(a =>
                {
                    var path = Path.Combine(packagesPath, p.Id + "." + p.Version, a.Path);
                    var aname = AssemblyName.GetAssemblyName(path);
                    return new { key = aname.FullName, value = path };
                }))
                .DistinctBy(i => i.key)
                .ToDictionary(i => i.key, i => i.value);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var aname = new AssemblyName(eventArgs.Name);
                if (allAssemblies.ContainsKey(aname.FullName))
                {
                    return Assembly.LoadFile(allAssemblies[aname.FullName]);
                }
                return null;
            };

            // Run the package export delegate if found
            var assemblies = localPackage.AssemblyReferences.Select(a => new AssemblyCatalog(Path.Combine(packagesPath, localPackage.Id + "." + localPackage.Version, a.Path)));
            using (var catalog = new AggregateCatalog(assemblies))
            using (var container = new CompositionContainer(catalog))
            {
                traceWriter(TraceLevel.Verbose, "Resolving execution package entry point.");
                container.SatisfyImportsOnce(this);
                if (this.RunAssembly == null)
                {
                    traceWriter(TraceLevel.Error, "Execution package extry point not found!");
                    return 5;
                }
                traceWriter(TraceLevel.Verbose, "Invoking execution package extry point.");
                this.RunAssembly(configuration.token, traceWriter);
                traceWriter(TraceLevel.Verbose, "Execution package finished successfully.");
                return 0;
            }
        }

        /// <summary>
        /// </summary>
        private static Func<TraceLevel, string, bool> CreateTraceWriter(TraceLevel maxLevel)
        {
            return (messageLevel, message) =>
            {
                if (messageLevel > maxLevel)
                {
                    return false;
                }
                if (message != null)
                {
                    ConsoleColor originalColor;
                    switch (messageLevel)
                    {
                        case TraceLevel.Off:
                            break;
                        case TraceLevel.Error:
                            originalColor = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine(message);
                            Console.ForegroundColor = originalColor;
                            break;
                        case TraceLevel.Verbose:
                            Console.Error.WriteLine(message);
                            break;
                        case TraceLevel.Warning:
                            originalColor = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(message);
                            Console.ForegroundColor = originalColor;
                            break;
                        case TraceLevel.Info:
                            Console.WriteLine(message);
                            break;
                    }
                }
                return true;
            };
        }

        /// <summary>
        /// </summary>
        private static Parameters ParseArguments(IEnumerable<string> args)
        {
            var arguments = args
                .Select(a => a.Split(new[] { ':' }, 2))
                .ToLookup(k => k[0].ToLower().TrimStart('/', '-'), v => v.Length > 1 ? v[1] : "");

            var parameters = new Parameters();

            var parameterDefinitions = new[]
            {
                new { names = new [] {"package", "p"}, parse = new Action<string>(val => { parameters.Package = val; } )},
                new { names = new [] {"token", "t"}, parse = new Action<string>(val => { parameters.Token = val; } )},
                new { names = new [] {"repository", "r"}, parse = new Action<string>(val => { parameters.Repository = val; } )},
                new { names = new [] {"username", "u"}, parse = new Action<string>(val => { parameters.RepositoryUsername = val; } )},
                new { names = new [] {"password", "w"}, parse = new Action<string>(val => { parameters.RepositoryPassword = val; } )},
                new { names = new [] {"config", "c"}, parse = new Action<string>(val => { parameters.Config = val; } )},
                new { names = new [] {"disableupdates", "d"}, parse = new Action<string>(val => { parameters.DisableUpdates = true; } )},
                new { names = new [] {"help", "h"}, parse = new Action<string>(val => { parameters.ShowHelp = true; } )},
                new { names = new [] {"version", "v"}, parse = new Action<string>(val => { parameters.ShowVersion = true; } )},
                new { names = new [] {"log", "l"}, parse = new Action<string>(val =>
                {
                    if (!Enum.TryParse(val, true, out parameters.TraceLevel) || !Enum.IsDefined(typeof(TraceLevel), parameters.TraceLevel))
                    {
                        parameters.TraceLevel = TraceLevel.Warning;
                    }
                })}
            };

            foreach (var parameter in parameterDefinitions)
            {
                var argument = arguments.FirstOrDefault(a => parameter.names.Contains(a.Key));
                if (argument == null)
                {
                    continue;
                }
                var argumentValue = argument.FirstOrDefault() ?? "";
                parameter.parse(argumentValue);
            }

            return parameters;
        }

        /// <summary>
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine(@"
Runs a code from a NuGet package.

Usage: 
{0} (-c:Filename | -p:Package -t:Token [-r:Repository] [-u:Username] [-p:Password]) [-d] [-h] [-v]

Where:
-c:Filename (also --config:Filename), Specifies a JSON configuration file for input parameters. Not used if other parameters are specified.
-p:Package (also --package:Package), Specifies the NuGet package name to execute. Required if --config is not used.
-t:Token (also --token:Token), Specifies a JWT token to pass to the execution method as parameter. Required if --config is not used.
-r:Repository (also --repository:Repository), An optional repository to look for the NuGet package and it's dependencies. If not provided only the public NuGet repository is used.
-u:Username (also --username:Username), An optional username for the repository
-p:Password (also --password:Password), An optional password for the repository
-d (also --disableupdates), A switch to disable autoupdate of the {0}
-v (also --version), Displays the {0} version and exits
-h (also --help), Displays this help and exits
-l:Level (also --log:Level), Sets log output level: 0|Off, 1|Error, 2|Warning, 3|Info, 4|Verbose. The default is Warning.

Examples:
{0} -c:ConfigFile.json
{0} -p:Package.Name -t:eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
", "IntegrationAgent");
        }

    }

}
