// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public interface IRezolveTargetMetadataExtension : IRezolveTargetMetadata
	{
		string ExtensionType { get; }
	}
}
