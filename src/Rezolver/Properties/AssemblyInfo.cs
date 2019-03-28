// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Runtime.CompilerServices;
using System.Security;

// comment this out when including fastexpressioncompiler (1.4.0)
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]

[assembly: InternalsVisibleTo("Rezolver.Tests")]
[assembly: InternalsVisibleTo("Rezolver.Tests.Compilation.Specification")]
[assembly: InternalsVisibleTo("Rezolver.PerfAnalysis.NetCore")]
