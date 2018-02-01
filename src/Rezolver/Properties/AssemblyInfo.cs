// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System.Runtime.CompilerServices;
using System.Security;

[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
#if !MAXCOMPAT
[assembly: SecurityRules(SecurityRuleSet.Level2,SkipVerificationInFullTrust=true)]
#endif

