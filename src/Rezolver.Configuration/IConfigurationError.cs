// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
  /// <summary>
  /// Interface for an error that originates from a configuration file.
  /// </summary>
  public interface IConfigurationError
  {
    /// <summary>
    /// The message
    /// </summary>
    string ErrorMessage { get; }
    /// <summary>
    /// The message formatted with the start/end line and position for more accurate error reporting.
    /// </summary>
    string ErrorMessageWithLineInfo { get; }
    /// <summary>
    /// Start and (potentially) end position within the configuration file where this error originates.
    /// </summary>
    IConfigurationLineInfo LineInfo { get; }
  }
}
