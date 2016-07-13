// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
  /// <summary>
  /// Represents an object that is responsible for transforming configuration data into target containers.
  /// </summary>
  public interface IConfigurationAdapter
  {
    /// <summary>
    /// Creates an <see cref="ITargetContainer"/> instance from an <see cref="IConfiguration"/> instance.
    /// </summary>
    /// <param name="configuration">Required - the configuration object that is to be used to build a container.</param>
    /// <returns></returns>
    ITargetContainer CreateTargetContainer(IConfiguration configuration);
  }
}
