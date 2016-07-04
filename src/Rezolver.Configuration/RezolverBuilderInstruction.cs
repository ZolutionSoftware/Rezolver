// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
  /// <summary>
  /// An instruction to perform an operation on a rezolver builder from an IConfigurationEntry that's
  /// been parsed from a configuration source (and which has been retrieved from an IConfiguration instance).
  /// </summary>
  public abstract class RezolverBuilderInstruction
  {
    /// <summary>
    /// The source configuration entry for this instruction.
    /// </summary>
    public IConfigurationEntry Entry { get; private set; }

    /// <summary>
    /// Inheritance constructor.
    /// </summary>
    /// <param name="entry">The entry that built this instruction.</param>
    protected RezolverBuilderInstruction(IConfigurationEntry entry)
    {
      Entry = entry;
    }

    /// <summary>
    /// Abstract method which performs whatever instruction this instance represents on the passed
    /// builder.
    /// </summary>
    /// <param name="builder"></param>
    public abstract void Apply(ITargetContainer builder);
  }
}
