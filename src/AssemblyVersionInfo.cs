using System.Reflection;
using System.Resources;

[assembly: AssemblyCompany("Pandell Technology Corporation")]
[assembly: AssemblyCopyright("Copyright (C) 2002-2015 Pandell Technology Corporation. All rights reserved.")]
[assembly: AssemblyTrademark("Pandell Liquid Intelligence (PLI) is a registered trademark of Pandell Technology Corporation in Canada and/or other countries.")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: AssemblyVersion("1.5.0.0")] // for more discussion on versioning see http://stackoverflow.com/questions/62353/what-are-the-best-practices-for-using-assembly-attributes (and its followup)
[assembly: AssemblyFileVersion("1.5.0.0")]
[assembly: AssemblyProduct("Pandell IntegrationAgent "
#if DEBUG
    + "(debug)"
#else
    + "(release)"
#endif
)]

[assembly: AssemblyConfiguration(
#if DEBUG
    "DEBUG"
#else
    "RELEASE"
#endif
)]
