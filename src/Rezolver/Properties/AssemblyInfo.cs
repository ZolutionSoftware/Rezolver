// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Resources;
using System.Reflection;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Rezolver")]
[assembly: AssemblyDescription("High performance portable IOC container for .Net")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Zolution Software Ltd")]
[assembly: AssemblyProduct("Rezolver")]
[assembly: AssemblyCopyright("Copyright ©Zolution Software 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
#if !DOTNET
[assembly: SecurityRules(SecurityRuleSet.Level2,SkipVerificationInFullTrust=true)]
#endif

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
