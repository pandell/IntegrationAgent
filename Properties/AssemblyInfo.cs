using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("PackageRunner")]
[assembly: AssemblyProduct("PackageRunner")]
[assembly: AssemblyDescription("PackageRunner - A shell for executing auto-updatable NuGet packages components")]
[assembly: AssemblyCompany("Pandell Technology Corporation")]
[assembly: AssemblyCopyright("Copyright (C) 2002-2015 Pandell Technology Corporation. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: AssemblyConfiguration(
#if DEBUG
    "DEBUG"
#else
    "RELEASE"
#endif
)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("dbe88ffc-a1ad-44fa-9205-3d5c9285e843")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.5.0.0")]